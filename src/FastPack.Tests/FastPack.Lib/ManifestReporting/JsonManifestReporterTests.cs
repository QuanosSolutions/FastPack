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
public class JsonManifestReporterTests
{
	[Test]
	public async Task Ensure_PrintReport_Works()
	{
		// arrange
		Mock<ILogger> loggerMock = new Mock<ILogger>();
		IManifestReporter manifestReporter = new JsonManifestReporter(loggerMock.Object);
		Manifest manifest = new Manifest
		{
			Version = 1,
			CreationDateUtc = new DateTime(2022, 1, 1, 1, 1 ,1)
		};
		string expectedMessage = "{\"version\":1,\"created\":\"2022-01-01T01:01:01\",\"filesystemDatesIncluded\":true,\"filesystemPermissionsIncluded\":true,\"hashingAlgorithm\":\"XXHash\",\"compressionAlgorithm\":\"Deflate\",\"compressionLevelName\":\"Optimal\",\"compressionLevel\":0,\"filesCount\":0,\"filesSizeUncompressed\":0,\"uniqueFilesCount\":0,\"uniqueFilesSizeUncompressed\":0,\"FoldersCount\":0}";

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
		IManifestReporter manifestReporter = new JsonManifestReporter(loggerMock.Object);
		Manifest manifest = new Manifest
		{
			Version = 1,
			CreationDateUtc = new DateTime(2022, 1, 1, 1, 1, 1)
		};
		string expectedMessage = "{\r\n  \"version\": 1,\r\n  \"created\": \"2022-01-01T01:01:01\",\r\n  \"filesystemDatesIncluded\": true,\r\n  \"filesystemPermissionsIncluded\": true,\r\n  \"hashingAlgorithm\": \"XXHash\",\r\n  \"compressionAlgorithm\": \"Deflate\",\r\n  \"compressionLevelName\": \"Optimal\",\r\n  \"compressionLevel\": 0,\r\n  \"filesCount\": 0,\r\n  \"filesSizeUncompressed\": 0,\r\n  \"uniqueFilesCount\": 0,\r\n  \"uniqueFilesSizeUncompressed\": 0,\r\n  \"FoldersCount\": 0\r\n}";
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