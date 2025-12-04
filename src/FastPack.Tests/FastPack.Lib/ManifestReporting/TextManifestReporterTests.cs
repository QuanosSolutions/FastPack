using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using FastPack.Lib.Hashing;
using FastPack.Lib.Logging;
using FastPack.Lib.ManifestManagement;
using FastPack.Lib.ManifestReporting;
using Moq;
using NUnit.Framework;

namespace FastPack.Tests.FastPack.Lib.ManifestReporting;

[TestFixture]
internal class TextManifestReporterTests
{
	[Test]
	public async Task Ensure_PrintReport_Works()
	{
		// arrange
		Mock<ILogger> loggerMock = new Mock<ILogger>();
		IManifestReporter manifestReporter = new TextManifestReporter(loggerMock.Object);
		Manifest manifest = new Manifest
		{
			CreationDateUtc = new DateTime(2022, 1, 1, 1, 1, 1),
			MetaDataOptions = MetaDataOptions.IncludeFileSystemDates
		};

		// act
		await manifestReporter.PrintReport(manifest, false, false, false);

		// assert
		loggerMock.Verify(x => x.InfoLine("Manifest Info"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("========================================================"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("  Version: 1"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("  Created: " + manifest.CreationDateUtc.ToLocalTime().ToString(CultureInfo.InvariantCulture)), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("  Meta data:"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("    Filesystem dates included: Yes"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("    Filesystem permissions included: No"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("  Hashing algorithm: XXHash"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("  Compression algorithm: Deflate"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("  Compression level: Optimal (0)"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("  Comment: "), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("  Files: 0"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("  Size of files (uncompressed): 0 bytes"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("  Unique files: 0"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("  Size of unique files (uncompressed): 0 bytes"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("  Folders: 0"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine(null), Times.Exactly(1));
		loggerMock.VerifyNoOtherCalls();
	}

	[Test]
	public async Task Ensure_PrintReport_Works_Complex()
	{
		// arrange
		Mock<ILogger> loggerMock = new Mock<ILogger>();
		IManifestReporter manifestReporter = new TextManifestReporter(loggerMock.Object);
		Manifest manifest = new Manifest
		{
			Version = 1,
			CreationDateUtc = new DateTime(2022, 1, 1, 1, 1, 1),
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
		await manifestReporter.PrintReport(manifest, true, true, false);

		// assert
		loggerMock.Verify(x => x.InfoLine("Manifest Info"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("========================================================"), Times.Exactly(3));
		loggerMock.Verify(x => x.InfoLine("  Version: 1"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("  Created: " + manifest.CreationDateUtc.ToLocalTime().ToString(CultureInfo.InvariantCulture)), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("  Meta data:"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("    Filesystem dates included: Yes"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("    Filesystem permissions included: Yes"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("  Hashing algorithm: XXHash"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("  Compression algorithm: Deflate"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("  Compression level: Optimal (0)"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("  Comment: This is the comment"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("  Files: 2"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("  Size of files (compressed): 100 bytes"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("  Size of files (uncompressed): 246 bytes"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("  Unique files: 1"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("  Size of unique files (compressed): 50 bytes"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("  Size of unique files (uncompressed): 123 bytes"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("  Folders: 1"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("Folders"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("  subDir"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("    Created: " + manifest.Entries[0].FileSystemEntries[0].CreationDateUtc.Value.ToLocalTime().ToString(CultureInfo.InvariantCulture)), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("    Last Access: " + manifest.Entries[0].FileSystemEntries[0].LastAccessDateUtc.Value.ToLocalTime().ToString(CultureInfo.InvariantCulture)), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("    Last Write: " + manifest.Entries[0].FileSystemEntries[0].LastWriteDateUtc.Value.ToLocalTime().ToString(CultureInfo.InvariantCulture)), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("    Permissions: 1"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("Files"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("  subDir/test.txt"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("    Hash: 1234567890"), Times.Exactly(2));
		loggerMock.Verify(x => x.InfoLine("    Size (compressed): 50 bytes"), Times.Exactly(2));
		loggerMock.Verify(x => x.InfoLine("    Size (uncompressed): 123 bytes"), Times.Exactly(2));
		loggerMock.Verify(x => x.InfoLine("    Created: " + manifest.Entries[1].FileSystemEntries[1].CreationDateUtc.Value.ToLocalTime().ToString(CultureInfo.InvariantCulture)), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("    Last Access: " + manifest.Entries[1].FileSystemEntries[1].LastAccessDateUtc.Value.ToLocalTime().ToString(CultureInfo.InvariantCulture)), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("    Last Write: " + manifest.Entries[1].FileSystemEntries[1].LastWriteDateUtc.Value.ToLocalTime().ToString(CultureInfo.InvariantCulture)), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("    Permissions: 4"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("  test.txt"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("    Created: " + manifest.Entries[1].FileSystemEntries[0].CreationDateUtc.Value.ToLocalTime().ToString(CultureInfo.InvariantCulture)), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("    Last Access: " + manifest.Entries[1].FileSystemEntries[0].LastAccessDateUtc.Value.ToLocalTime().ToString(CultureInfo.InvariantCulture)), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("    Last Write: " + manifest.Entries[1].FileSystemEntries[0].LastWriteDateUtc.Value.ToLocalTime().ToString(CultureInfo.InvariantCulture)), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("    Permissions: 2"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine(null), Times.Exactly(3));
		loggerMock.VerifyNoOtherCalls();
	}
}