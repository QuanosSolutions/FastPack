using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using FastPack.Lib.Hashing;
using FastPack.Lib.TypeExtensions;

namespace FastPack.Lib.ManifestManagement.Serialization;

public class BinaryArchiveFileReaderV1 : IArchiveFileReader
{
	internal IHashProviderFactory HashProviderFactory { get; set; } = new HashProviderFactory();

	public async Task<Manifest> ReadManifest(Stream inputStream)
	{
		Manifest manifest = new();
		using (BinaryReader reader = new(inputStream, Encoding.UTF8, true))
		{
			if (!reader.ReadUInt64().Equals(Constants.FastpackSignature))
				throw new InvalidDataException("File is not a valid fup file");

			manifest.Version = reader.ReadUInt16();

			long manifestPosition = reader.ReadInt64();
			inputStream.Seek(manifestPosition, SeekOrigin.Begin);
		}

		await using DeflateStream manifestDeflateStream = new(inputStream, CompressionMode.Decompress, true);
		await using BufferedStream bufferedManifestStream = new (manifestDeflateStream, ushort.MaxValue);
		using BinaryReader manifestReader = new(bufferedManifestStream, Encoding.UTF8, true);
		
		manifest.HashAlgorithm = (HashAlgorithm)manifestReader.ReadUInt16();
		IHashProvider hashProvider = HashProviderFactory.GetHashProvider(manifest.HashAlgorithm);
		int hashSizeInBytes = hashProvider.GetHashSizeInBytes();

		manifest.CompressionAlgorithm = (CompressionAlgorithm)manifestReader.ReadUInt16();
		manifest.CompressionLevel = manifestReader.ReadUInt16();
		manifest.CreationDateUtc = new DateTime(manifestReader.ReadInt64(), DateTimeKind.Utc);
		manifest.MetaDataOptions = (MetaDataOptions)manifestReader.ReadByte();
		manifest.Comment = manifestReader.ReadUtf8String();
		int entryCount = manifestReader.ReadInt32();

		for (int i = 0; i < entryCount; i++)
		{
			ManifestEntry entry = new();
			manifest.Entries.Add(entry);
			entry.Type = (EntryType)manifestReader.ReadByte();
			if (entry.Type == EntryType.File)
			{
				entry.DataIndex = manifestReader.ReadInt64();
				entry.DataSize = manifestReader.ReadInt64();
				entry.OriginalSize = manifestReader.ReadInt64();

				entry.Hash = hashProvider.HashBytesToString(manifestReader.ReadBytes(hashSizeInBytes));
				//entry.Hash = reader.ReadUtf8String();
			}

			int fileSystemEntryCount = manifestReader.ReadInt32();
			for (int j = 0; j < fileSystemEntryCount; j++)
			{
				ManifestFileSystemEntry fileSystemEntry = new();
				entry.FileSystemEntries.Add(fileSystemEntry);
				fileSystemEntry.RelativePath = manifestReader.ReadUtf8String();
				if (manifest.MetaDataOptions.HasFlag(MetaDataOptions.IncludeFileSystemDates))
				{
					fileSystemEntry.CreationDateUtc = new DateTime(manifestReader.ReadInt64(), DateTimeKind.Utc);
					fileSystemEntry.LastAccessDateUtc = new DateTime(manifestReader.ReadInt64(), DateTimeKind.Utc);
					fileSystemEntry.LastWriteDateUtc = new DateTime(manifestReader.ReadInt64(), DateTimeKind.Utc);
				}

				if (manifest.MetaDataOptions.HasFlag(MetaDataOptions.IncludeFileSystemPermissions))
				{
					// Compat with old manifests that contained all bits from stat syscall and not just the ones representing the permissions
					var readUInt32 = manifestReader.ReadUInt32();
					var validValuesMask = 0x0FFF;
					fileSystemEntry.FilePermissions = (UnixFileMode) (readUInt32 & validValuesMask);
				}
			}
		}
		return manifest;
	}
}