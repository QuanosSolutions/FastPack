using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using FastPack.Lib.Hashing;
using FastPack.Lib.TypeExtensions;

namespace FastPack.Lib.ManifestManagement.Serialization;

public class BinaryArchiveManifestWriterV1 : IArchiveManifestWriter
{
	internal IHashProviderFactory HashProviderFactory { get; set; } = new HashProviderFactory();

	public virtual async Task WriteManifest(Stream outputStream, Manifest manifest)
	{
		IHashProvider hashProvider = HashProviderFactory.GetHashProvider(manifest.HashAlgorithm);

		await using (BinaryWriter writer = new(outputStream, Encoding.UTF8, true))
		{
			long startPosition = outputStream.Position;
			writer.Seek(Constants.FastpackFileDataIndex, SeekOrigin.Begin);
			writer.Write(startPosition);
			writer.Seek(0, SeekOrigin.End);
		}

		await using (DeflateStream deflateStream = new(outputStream, CompressionLevel.Optimal, true))
		{
			await using (BinaryWriter writer = new(deflateStream, Encoding.UTF8, true))
			{
				writer.Write((ushort)manifest.HashAlgorithm);
				writer.Write((ushort)manifest.CompressionAlgorithm);
				writer.Write(manifest.CompressionLevel);
				writer.Write(manifest.CreationDateUtc.Ticks);
				writer.Write((byte)manifest.MetaDataOptions);
				writer.WriteUtf8String(manifest.Comment);

				writer.Write(manifest.Entries.Count);
				foreach (ManifestEntry manifestEntry in manifest.Entries)
				{
					writer.Write((byte)manifestEntry.Type);
					if (manifestEntry.Type == EntryType.File)
					{
						writer.Write(manifestEntry.DataIndex);
						writer.Write(manifestEntry.DataSize);
						writer.Write(manifestEntry.OriginalSize);
						writer.Write(hashProvider.HashStringToBytes(manifestEntry.Hash));
					}
					writer.Write(manifestEntry.FileSystemEntries.Count);
					foreach (ManifestFileSystemEntry fileSystemEntry in manifestEntry.FileSystemEntries)
					{
						writer.WriteUtf8String(fileSystemEntry.RelativePath);
						if (manifest.MetaDataOptions.HasFlag(MetaDataOptions.IncludeFileSystemDates))
						{
							writer.Write(fileSystemEntry.CreationDateUtc!.Value.Ticks);
							writer.Write(fileSystemEntry.LastAccessDateUtc!.Value.Ticks);
							writer.Write(fileSystemEntry.LastWriteDateUtc!.Value.Ticks);
						}
						if (manifest.MetaDataOptions.HasFlag(MetaDataOptions.IncludeFileSystemPermissions))
							writer.Write((uint)fileSystemEntry.FilePermissions!.Value);
					}
				}
			}
		}
	}
}