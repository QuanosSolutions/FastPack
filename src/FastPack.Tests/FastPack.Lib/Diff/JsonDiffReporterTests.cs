using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using FastPack.Lib.Diff;
using FastPack.Lib.Logging;
using Moq;
using NUnit.Framework;

namespace FastPack.Tests.FastPack.Lib.Diff;

[TestFixture]
public class JsonDiffReporterTests
{
	[Test]
	public async Task Ensure_PrintReport_Works()
	{
		// arrange
		Mock<ILogger> loggerMock = new Mock<ILogger>();
		IDiffReporter diffReporter = new JsonDiffReporter(loggerMock.Object);
		DiffReport diffReport = new DiffReport
		{
			AddedEntries = new List<DiffEntry>(),
			RemovedEntries = new List<DiffEntry>(),
		};
		string expectedMessage = "{\"addedCount\":0,\"removedCount\":0,\"totalChangedCount\":0,\"addedEntries\":[],\"removedEntries\":[]}";

		// act
		await diffReporter.PrintReport(diffReport, false);

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
		IDiffReporter diffReporter = new JsonDiffReporter(loggerMock.Object);
		DiffReport diffReport = new DiffReport
		{
			AddedEntries = new List<DiffEntry>(),
			RemovedEntries = new List<DiffEntry>(),
		};
		string expectedMessage = "{\r\n  \"addedCount\": 0,\r\n  \"removedCount\": 0,\r\n  \"totalChangedCount\": 0,\r\n  \"addedEntries\": [],\r\n  \"removedEntries\": []\r\n}";
		if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			expectedMessage = expectedMessage.Replace("\r\n", "\n");
		}

		// act
		await diffReporter.PrintReport(diffReport, true);

		// assert
		loggerMock.Verify(x => x.SetQuiet(false), Times.Exactly(1));
		loggerMock.Verify(x => x.Info(expectedMessage), Times.Exactly(1));
		loggerMock.Verify(x => x.SetQuiet(true), Times.Exactly(1));
		loggerMock.VerifyNoOtherCalls();
	}
}