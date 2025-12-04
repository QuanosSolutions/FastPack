using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using FastPack.Lib.Compression;
using FastPack.Lib.Hashing;
using FastPack.Lib.ManifestManagement;
using FastPack.Lib.ManifestReporting;
using AwesomeAssertions;
using Moq;
using NUnit.Framework;

namespace FastPack.Tests.FastPack.Lib.ManifestReporting;

[TestFixture]
internal class ManifestExtensionTests
{
	[Test]
	public void Ensure_CreateManifestReport_Works()
	{
		// arrange
		Mock<IFileCompressorFactory> fileCompressorFactoryMock = new Mock<IFileCompressorFactory>();
		Mock<IFileCompressor> fileCompressorMock = new Mock<IFileCompressor>();
		fileCompressorFactoryMock.Setup(x => x.GetCompressor(CompressionAlgorithm.Deflate)).Returns(fileCompressorMock.Object);
		fileCompressorMock.Setup(x => x.GetCompressionLevelValuesWithNames()).Returns(new Dictionary<ushort, string> { { 0, "Optimal" } });
		Manifest manifest = CreateManifest();

		// act
		ManifestReport manifestReport = manifest.CreateManifestReport(fileCompressorFactoryMock.Object, false, false);

		// assert
		AssertCommonProperties(manifestReport, manifest);
		AssertCompressedSizeDisabled(manifestReport);
		AssertDetailedDisabled(manifestReport);
	}

	[Test]
	public void Ensure_CreateManifestReport_Works_With_Compressed_Size()
	{
		// arrange
		Mock<IFileCompressorFactory> fileCompressorFactoryMock = new Mock<IFileCompressorFactory>();
		Mock<IFileCompressor> fileCompressorMock = new Mock<IFileCompressor>();
		fileCompressorFactoryMock.Setup(x => x.GetCompressor(CompressionAlgorithm.Deflate)).Returns(fileCompressorMock.Object);
		fileCompressorMock.Setup(x => x.GetCompressionLevelValuesWithNames()).Returns(new Dictionary<ushort, string> { { 0, "Optimal" } });
		Manifest manifest = CreateManifest();

		// act
		ManifestReport manifestReport = manifest.CreateManifestReport(fileCompressorFactoryMock.Object, true, false);

		// assert
		AssertCommonProperties(manifestReport, manifest);
		AssertCompressedSizeEnabled(manifestReport);
		AssertDetailedDisabled(manifestReport);
	}

	[Test]
	public void Ensure_CreateManifestReport_Works_With_Detailed()
	{
		// arrange
		Mock<IFileCompressorFactory> fileCompressorFactoryMock = new Mock<IFileCompressorFactory>();
		Mock<IFileCompressor> fileCompressorMock = new Mock<IFileCompressor>();
		fileCompressorFactoryMock.Setup(x => x.GetCompressor(CompressionAlgorithm.Deflate)).Returns(fileCompressorMock.Object);
		fileCompressorMock.Setup(x => x.GetCompressionLevelValuesWithNames()).Returns(new Dictionary<ushort, string> { { 0, "Optimal" } });
		Manifest manifest = CreateManifest();

		// act
		ManifestReport manifestReport = manifest.CreateManifestReport(fileCompressorFactoryMock.Object, false, true);

		// assert
		AssertCommonProperties(manifestReport, manifest);
		AssertCompressedSizeDisabled(manifestReport);
		AssertDetailedEnabled(manifestReport, false);
	}

	[Test]
	public void Ensure_CreateManifestReport_Works_With_Compressed_Size_And_Detailed()
	{
		// arrange
		Mock<IFileCompressorFactory> fileCompressorFactoryMock = new Mock<IFileCompressorFactory>();
		Mock<IFileCompressor> fileCompressorMock = new Mock<IFileCompressor>();
		fileCompressorFactoryMock.Setup(x => x.GetCompressor(CompressionAlgorithm.Deflate)).Returns(fileCompressorMock.Object);
		fileCompressorMock.Setup(x => x.GetCompressionLevelValuesWithNames()).Returns(new Dictionary<ushort, string> { { 0, "Optimal" } });
		Manifest manifest = CreateManifest();

		// act
		ManifestReport manifestReport = manifest.CreateManifestReport(fileCompressorFactoryMock.Object, true, true);

		// assert
		AssertCommonProperties(manifestReport, manifest);
		AssertCompressedSizeEnabled(manifestReport);
		AssertDetailedEnabled(manifestReport, true);
	}

	private static void AssertCommonProperties(ManifestReport manifestReport, Manifest manifest)
	{
		manifestReport.Should().NotBeNull();
		manifestReport.Version.Should().Be(manifest.Version);
		manifestReport.Created.Should().Be(manifest.CreationDateUtc);
		manifestReport.FilesystemDatesIncluded.Should().Be(true);
		manifestReport.FilesystemPermissionsIncluded.Should().Be(true);
		manifestReport.HashingAlgorithm.Should().Be("XXHash");
		manifestReport.CompressionAlgorithm.Should().Be("Deflate");
		manifestReport.CompressionLevelName.Should().Be("Optimal");
		manifestReport.CompressionLevel.Should().Be(0);
		manifestReport.Comment.Should().Be(manifest.Comment);
		manifestReport.FilesCount.Should().Be(2);
		manifestReport.FilesSizeUncompressed.Should().Be(246);
		manifestReport.UniqueFilesCount.Should().Be(1);
		manifestReport.UniqueFilesSizeUncompressed.Should().Be(123);
		manifestReport.FoldersCount.Should().Be(1);
	}

	private static void AssertCompressedSizeEnabled(ManifestReport manifestReport)
	{
		manifestReport.FilesSizeCompressed.Should().NotBeNull();
		manifestReport.UniqueFilesSizeCompressed.Should().NotBeNull();
		manifestReport.FilesSizeCompressed.Should().Be(100);
		manifestReport.UniqueFilesSizeCompressed.Should().Be(50);
	}

	private static void AssertCompressedSizeDisabled(ManifestReport manifestReport)
	{
		manifestReport.FilesSizeCompressed.Should().BeNull();
		manifestReport.UniqueFilesSizeCompressed.Should().BeNull();
	}

	private static void AssertDetailedEnabled(ManifestReport manifestReport, bool showCompressedSize)
	{
		manifestReport.Folders.Should().NotBeNull();
		manifestReport.Files.Should().NotBeNull();
		manifestReport.Folders.Count.Should().Be(1);
		manifestReport.Files.Count.Should().Be(2);
		manifestReport.Folders[0].RelativePath.Should().Be("subDir");
		manifestReport.Folders[0].Hash.Should().BeNull();
		manifestReport.Folders[0].SizeCompressed.Should().BeNull();
		manifestReport.Folders[0].SizeUncompressed.Should().BeNull();
		manifestReport.Folders[0].Created.Should().Be(new DateTime(2022, 1, 1, 1, 1, 1));
		manifestReport.Folders[0].LastAccess.Should().Be(new DateTime(2022, 1, 2, 1, 1, 1));
		manifestReport.Folders[0].LastWrite.Should().Be(new DateTime(2022, 1, 3, 1, 1, 1));
		manifestReport.Folders[0].Permissions.Should().Be(((uint)UnixFileMode.OtherExecute).ToString());
		manifestReport.Files[0].RelativePath.Should().Be("subDir/test.txt");
		manifestReport.Files[0].Hash.Should().Be("1234567890");
		if (showCompressedSize)
			manifestReport.Files[0].SizeCompressed.Should().Be(50);
		else
			manifestReport.Files[0].SizeCompressed.Should().BeNull();
		manifestReport.Files[0].SizeUncompressed.Should().Be(123);
		manifestReport.Files[0].Created.Should().Be(new DateTime(2022, 1, 7, 1, 1, 1));
		manifestReport.Files[0].LastAccess.Should().Be(new DateTime(2022, 1, 8, 1, 1, 1));
		manifestReport.Files[0].LastWrite.Should().Be(new DateTime(2022, 1, 9, 1, 1, 1));
		manifestReport.Files[0].Permissions.Should().Be(((uint)UnixFileMode.OtherExecute).ToString());
		manifestReport.Files[1].RelativePath.Should().Be("test.txt");
		manifestReport.Files[1].Hash.Should().Be("1234567890");
		if (showCompressedSize)
			manifestReport.Files[1].SizeCompressed.Should().Be(50);
		else
			manifestReport.Files[1].SizeCompressed.Should().BeNull();
		manifestReport.Files[1].SizeUncompressed.Should().Be(123);
		manifestReport.Files[1].Created.Should().Be(new DateTime(2022, 1, 4, 1, 1, 1));
		manifestReport.Files[1].LastAccess.Should().Be(new DateTime(2022, 1, 5, 1, 1, 1));
		manifestReport.Files[1].LastWrite.Should().Be(new DateTime(2022, 1, 6, 1, 1, 1));
		manifestReport.Files[1].Permissions.Should().Be(((uint)UnixFileMode.OtherExecute).ToString());
	}

	private static void AssertDetailedDisabled(ManifestReport manifestReport)
	{
		manifestReport.Folders.Should().BeNull();
		manifestReport.Files.Should().BeNull();
	}

	private static Manifest CreateManifest()
	{
		return new Manifest
		{
			Version = 1,
			CreationDateUtc = DateTime.UtcNow,
			MetaDataOptions = MetaDataOptions.IncludeFileSystemDates | MetaDataOptions.IncludeFileSystemPermissions,
			HashAlgorithm = HashAlgorithm.XXHash,
			CompressionAlgorithm = CompressionAlgorithm.Deflate,
			CompressionLevel = (ushort)CompressionLevel.Optimal,
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
					DataSize = 50,
					Hash = "1234567890",
					FileSystemEntries = new List<ManifestFileSystemEntry>
					{
						new()
						{
							RelativePath = "test.txt",
							CreationDateUtc = new DateTime(2022, 1, 4, 1 , 1, 1),
							LastAccessDateUtc = new DateTime(2022, 1, 5, 1 , 1, 1),
							LastWriteDateUtc = new DateTime(2022, 1, 6, 1 , 1, 1),
							FilePermissions = UnixFileMode.OtherExecute
						},
						new()
						{
							RelativePath = "subDir/test.txt",
							CreationDateUtc = new DateTime(2022, 1, 7, 1 , 1, 1),
							LastAccessDateUtc = new DateTime(2022, 1, 8, 1 , 1, 1),
							LastWriteDateUtc = new DateTime(2022, 1, 9, 1 , 1, 1),
							FilePermissions = UnixFileMode.OtherExecute
						}
					}
				}
			}
		};
	}
}