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

		await using (DeflateStream deflateStream = new(inputStream, CompressionMode.Decompress, true))
		{
			using (BinaryReader reader = new(deflateStream, Encoding.UTF8, true))
			{
				manifest.HashAlgorithm = (HashAlgorithm) reader.ReadUInt16();
				IHashProvider hashProvider = HashProviderFactory.GetHashProvider(manifest.HashAlgorithm);
				int hashSizeInBytes = hashProvider.GetHashSizeInBytes();

				manifest.CompressionAlgorithm = (CompressionAlgorithm) reader.ReadUInt16();
				manifest.CompressionLevel = reader.ReadUInt16();
				manifest.CreationDateUtc = new DateTime(reader.ReadInt64(), DateTimeKind.Utc);
				manifest.MetaDataOptions = (MetaDataOptions)reader.ReadByte();
				manifest.Comment = reader.ReadUtf8String();
				int entryCount = reader.ReadInt32();

				for (int i = 0; i < entryCount; i++)
				{
					ManifestEntry entry = new();
					manifest.Entries.Add(entry);
					entry.Type = (EntryType) reader.ReadByte();
					if (entry.Type == EntryType.File)
					{
						entry.DataIndex = reader.ReadInt64();
						entry.DataSize = reader.ReadInt64();
						entry.OriginalSize = reader.ReadInt64();

						entry.Hash = hashProvider.HashBytesToString(reader.ReadBytes(hashSizeInBytes));
						//entry.Hash = reader.ReadUtf8String();
					}

					int fileSystemEntryCount = reader.ReadInt32();
					for (int j = 0; j < fileSystemEntryCount; j++)
					{
						ManifestFileSystemEntry fileSystemEntry = new();
						entry.FileSystemEntries.Add(fileSystemEntry);
						fileSystemEntry.RelativePath = reader.ReadUtf8String();
						if (manifest.MetaDataOptions.HasFlag(MetaDataOptions.IncludeFileSystemDates))
						{
							fileSystemEntry.CreationDateUtc = new DateTime(reader.ReadInt64(), DateTimeKind.Utc);
							fileSystemEntry.LastAccessDateUtc = new DateTime(reader.ReadInt64(), DateTimeKind.Utc);
							fileSystemEntry.LastWriteDateUtc = new DateTime(reader.ReadInt64(), DateTimeKind.Utc);
						}
						if (manifest.MetaDataOptions.HasFlag(MetaDataOptions.IncludeFileSystemPermissions))
							fileSystemEntry.FilePermissions = reader.ReadUInt32();
					}
				}
			}
		}
		return manifest;
	}
}