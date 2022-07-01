using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using FastPack.Lib.Diff;
using FastPack.Lib.Logging;
using Moq;
using NUnit.Framework;

namespace FastPack.Tests.FastPack.Lib.Diff;

[TestFixture]
public class XmlDiffReporterTests
{
	[Test]
	public async Task Ensure_PrintReport_Works()
	{
		// arrange
		Mock<ILogger> loggerMock = new Mock<ILogger>();
		IDiffReporter diffReporter = new XmlDiffReporter(loggerMock.Object);
		DiffReport diffReport = new DiffReport
		{
			AddedEntries = new List<DiffEntry>(),
			RemovedEntries = new List<DiffEntry>(),
		};
		string expectedMessage = "<?xml version=\"1.0\" encoding=\"utf-8\"?><Diff><AddedCount>0</AddedCount><RemovedCount>0</RemovedCount><TotalChangedCount>0</TotalChangedCount><AddedEntries /><RemovedEntries /></Diff>";

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
		IDiffReporter diffReporter = new XmlDiffReporter(loggerMock.Object);
		DiffReport diffReport = new DiffReport
		{
			AddedEntries = new List<DiffEntry>(),
			RemovedEntries = new List<DiffEntry>(),
		};
		string expectedMessage = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Diff>\r\n  <AddedCount>0</AddedCount>\r\n  <RemovedCount>0</RemovedCount>\r\n  <TotalChangedCount>0</TotalChangedCount>\r\n  <AddedEntries />\r\n  <RemovedEntries />\r\n</Diff>";
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