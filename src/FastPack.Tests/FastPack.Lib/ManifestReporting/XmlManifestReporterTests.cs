using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using FastPack.Lib.Logging;
using FastPack.Lib.ManifestManagement;
using FastPack.Lib.ManifestReporting;
using Moq;
using NUnit.Framework;

namespace FastPack.Tests.FastPack.Lib.ManifestReporting;

[TestFixture]
public class XmlManifestReporterTests
{
	[Test]
	public async Task Ensure_PrintReport_Works()
	{
		// arrange
		Mock<ILogger> loggerMock = new Mock<ILogger>();
		IManifestReporter manifestReporter = new XmlManifestReporter(loggerMock.Object);
		Manifest manifest = new Manifest
		{
			Version = 1,
			CreationDateUtc = new DateTime(2022, 1, 1, 1, 1 ,1)
		};
		string expectedMessage = "<?xml version=\"1.0\" encoding=\"utf-8\"?><Manifest><Version>1</Version><Created>2022-01-01T01:01:01</Created><FilesystemDatesIncluded>true</FilesystemDatesIncluded><FilesystemPermissionsIncluded>true</FilesystemPermissionsIncluded><HashingAlgorithm>XXHash</HashingAlgorithm><CompressionAlgorithm>Deflate</CompressionAlgorithm><CompressionLevelName>Optimal</CompressionLevelName><CompressionLevel>0</CompressionLevel><FilesCount>0</FilesCount><FilesSizeUncompressed>0</FilesSizeUncompressed><UniqueFilesCount>0</UniqueFilesCount><UniqueFilesSizeUncompressed>0</UniqueFilesSizeUncompressed><FoldersCount>0</FoldersCount></Manifest>";

		// act
		await manifestReporter.PrintReport(manifest, false, false, false);

		// assert
		loggerMock.Verify(x => x.SetQuiet(false), Times.Exactly(1));
		loggerMock.Verify(x => x.Info(expectedMessage), Times.Exactly(1));
		loggerMock.Verify(x => x.SetQuiet(true), Times.Exactly(1));
		loggerMock.VerifyNoOtherCalls();
	}

	[Test]
	public async Task Ensure_PrintReport_Works_With_PrettyPrint()
	{
		// arrange
		Mock<ILogger> loggerMock = new Mock<ILogger>();
		IManifestReporter manifestReporter = new XmlManifestReporter(loggerMock.Object);
		Manifest manifest = new Manifest
		{
			Version = 1,
			CreationDateUtc = new DateTime(2022, 1, 1, 1, 1, 1)
		};
		string expectedMessage = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Manifest>\r\n  <Version>1</Version>\r\n  <Created>2022-01-01T01:01:01</Created>\r\n  <FilesystemDatesIncluded>true</FilesystemDatesIncluded>\r\n  <FilesystemPermissionsIncluded>true</FilesystemPermissionsIncluded>\r\n  <HashingAlgorithm>XXHash</HashingAlgorithm>\r\n  <CompressionAlgorithm>Deflate</CompressionAlgorithm>\r\n  <CompressionLevelName>Optimal</CompressionLevelName>\r\n  <CompressionLevel>0</CompressionLevel>\r\n  <FilesCount>0</FilesCount>\r\n  <FilesSizeUncompressed>0</FilesSizeUncompressed>\r\n  <UniqueFilesCount>0</UniqueFilesCount>\r\n  <UniqueFilesSizeUncompressed>0</UniqueFilesSizeUncompressed>\r\n  <FoldersCount>0</FoldersCount>\r\n</Manifest>";
		if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			expectedMessage = expectedMessage.Replace("\r\n", "\n");
		}

		// act
		await manifestReporter.PrintReport(manifest, false, false, true);

		// assert
		loggerMock.Verify(x => x.SetQuiet(false), Times.Exactly(1));
		loggerMock.Verify(x => x.Info(expectedMessage), Times.Exactly(1));
		loggerMock.Verify(x => x.SetQuiet(true), Times.Exactly(1));
		loggerMock.VerifyNoOtherCalls();
	}
}