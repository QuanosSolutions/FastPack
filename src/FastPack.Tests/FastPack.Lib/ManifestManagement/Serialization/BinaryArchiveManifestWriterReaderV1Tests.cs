using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using FastPack.Lib;
using FastPack.Lib.Hashing;
using FastPack.Lib.ManifestManagement;
using FastPack.Lib.ManifestManagement.Serialization;
using AwesomeAssertions;
using NUnit.Framework;

namespace FastPack.Tests.FastPack.Lib.ManifestManagement.Serialization;

[TestFixture]
internal class BinaryArchiveManifestWriterReaderV1Tests
{
	[Test]
	public async Task Ensure_Write_And_Read_Manifest_Works()
	{
		// arrange
		IHashProviderFactory hashProviderFactory = new HashProviderFactory();
		IArchiveManifestWriter archiveManifestWriter = new BinaryArchiveManifestWriterV1();
		IArchiveFileReader archiveFileReader = new BinaryArchiveFileReaderV1();
		IArchiveFileWriter archiveFileWriter = new BinaryArchiveFileWriterV1(archiveManifestWriter);
		((BinaryArchiveManifestWriterV1)archiveManifestWriter).HashProviderFactory = hashProviderFactory;
		((BinaryArchiveFileReaderV1)archiveFileReader).HashProviderFactory = hashProviderFactory;
		await using MemoryStream memoryStream = new MemoryStream();
		Manifest manifest = new Manifest
		{
			Version = 1,
			HashAlgorithm = HashAlgorithm.XXHash,
			CompressionAlgorithm = CompressionAlgorithm.Deflate,
			CompressionLevel = (ushort)CompressionLevel.Optimal,
			CreationDateUtc = DateTime.UtcNow,
			MetaDataOptions = MetaDataOptions.None,
			Comment = "This is the comment",
			Entries = new List<ManifestEntry>
			{
				new()
				{
					Type = EntryType.Directory,
					FileSystemEntries = new List<ManifestFileSystemEntry>
					{
						new()
						{
							RelativePath = "subDir"
						}
					}
				},
				new()
				{
					Type = EntryType.File,
					OriginalSize = 123,
					Hash = "1234567890",
					FileSystemEntries = new List<ManifestFileSystemEntry>
					{
						new()
						{
							RelativePath = "test.txt"
						},
						new()
						{
							RelativePath = "subDir/test.txt"
						}
					}
				}
			}
		};


		// act
		await archiveFileWriter.WriteHeader(memoryStream, manifest);
		await archiveManifestWriter.WriteManifest(memoryStream, manifest);
		memoryStream.Position = 0;
		Manifest readManifest = await archiveFileReader.ReadManifest(memoryStream);
		memoryStream.Position = 0;

		// assert
		readManifest.Should().BeEquivalentTo(manifest);
		memoryStream.GetBuffer()[Constants.FastpackFileDataIndex].Should().Be(18);
	}
	[Test]
	public async Task Ensure_Write_And_Read_Manifest_Works_With_Optional_Data()
	{
		// arrange
		IHashProviderFactory hashProviderFactory = new HashProviderFactory();
		IArchiveManifestWriter archiveManifestWriter = new BinaryArchiveManifestWriterV1();
		IArchiveFileReader archiveFileReader = new BinaryArchiveFileReaderV1();
		IArchiveFileWriter archiveFileWriter = new BinaryArchiveFileWriterV1(archiveManifestWriter);
		((BinaryArchiveManifestWriterV1)archiveManifestWriter).HashProviderFactory = hashProviderFactory;
		((BinaryArchiveFileReaderV1)archiveFileReader).HashProviderFactory = hashProviderFactory;
		await using MemoryStream memoryStream = new MemoryStream();
		Manifest manifest = new Manifest
		{
			Version = 1,
			HashAlgorithm = HashAlgorithm.XXHash,
			CompressionAlgorithm = CompressionAlgorithm.Deflate,
			CompressionLevel = (ushort)CompressionLevel.Optimal,
			CreationDateUtc = DateTime.UtcNow,
			MetaDataOptions = MetaDataOptions.IncludeFileSystemDates | MetaDataOptions.IncludeFileSystemPermissions,
			Comment = "This is the comment",
			Entries = new List<ManifestEntry>
			{
				new()
				{
					Type = EntryType.Directory,
					FileSystemEntries = new List<ManifestFileSystemEntry>
					{
						new()
						{
							RelativePath = "subDir",
							CreationDateUtc = new DateTime(2022, 1, 1, 1 , 1, 1),
							LastAccessDateUtc = new DateTime(2022, 1, 2, 1 , 1, 1),
							LastWriteDateUtc = new DateTime(2022, 1, 3, 1 , 1, 1),
							FilePermissions = UnixFileMode.OtherExecute
						}
					}
				},
				new()
				{
					Type = EntryType.File,
					OriginalSize = 123,
					Hash = "1234567890",
					FileSystemEntries = new List<ManifestFileSystemEntry>
					{
						new()
						{
							RelativePath = "test.txt",
							CreationDateUtc = new DateTime(2022, 1, 4, 1 , 1, 1),
							LastAccessDateUtc = new DateTime(2022, 1, 5, 1 , 1, 1),
							LastWriteDateUtc = new DateTime(2022, 1, 6, 1 , 1, 1),
							FilePermissions = UnixFileMode.OtherWrite
						},
						new()
						{
							RelativePath = "subDir/test.txt",
							CreationDateUtc = new DateTime(2022, 1, 7, 1 , 1, 1),
							LastAccessDateUtc = new DateTime(2022, 1, 8, 1 , 1, 1),
							LastWriteDateUtc = new DateTime(2022, 1, 9, 1 , 1, 1),
							FilePermissions = UnixFileMode.OtherRead

						}
					}
				}
			}
		};


		// act
		await archiveFileWriter.WriteHeader(memoryStream, manifest);
		await archiveManifestWriter.WriteManifest(memoryStream, manifest);
		memoryStream.Position = 0;
		Manifest readManifest = await archiveFileReader.ReadManifest(memoryStream);
		memoryStream.Position = 0;

		// assert
		readManifest.Should().BeEquivalentTo(manifest);
		memoryStream.GetBuffer()[Constants.FastpackFileDataIndex].Should().Be(18);
	}
}