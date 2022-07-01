using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FastPack.Lib.Diff;
using FastPack.Lib.Logging;
using Moq;
using NUnit.Framework;

namespace FastPack.Tests.FastPack.Lib.Diff;

[TestFixture]
internal class TextDiffReporterTests
{
	[Test]
	public async Task Ensure_PrintReport_Works()
	{
		// arrange
		Mock<IConsoleAbstraction> consoleAbstractionMock = new Mock<IConsoleAbstraction>();
		Mock<ILogger> loggerMock = new Mock<ILogger>();
		IDiffReporter diffReporter = new TextDiffReporter(loggerMock.Object, consoleAbstractionMock.Object);
		DiffReport diffReport = new DiffReport
		{
			AddedEntries = new List<DiffEntry>(),
			RemovedEntries = new List<DiffEntry>(),
		};

		// act
		await diffReporter.PrintReport(diffReport, false);

		// assert
		loggerMock.Verify(x => x.InfoLine(null), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("Diff finished with no differences."), Times.Exactly(1));
		loggerMock.VerifyNoOtherCalls();
		consoleAbstractionMock.VerifyNoOtherCalls();
	}

	[Test]
	public async Task Ensure_PrintReport_Works_OneChange()
	{
		// arrange
		Mock<IConsoleAbstraction> consoleAbstractionMock = new Mock<IConsoleAbstraction>();
		Mock<ILogger> loggerMock = new Mock<ILogger>();
		IDiffReporter diffReporter = new TextDiffReporter(loggerMock.Object, consoleAbstractionMock.Object);
		DiffReport diffReport = new DiffReport
		{
			AddedEntries = new List<DiffEntry>
			{
				new DiffEntry
				{
					Created = new DateTime(2022, 6, 1, 0, 0, 0),
					LastAccess =  new DateTime(2022, 7, 1, 0, 0, 0),
					LastWrite =  new DateTime(2022, 6, 15, 0, 0, 0),
					Hash = "1234567890",
					RelativePath = "file1.txt",
					Size = 123
				}
			},
			RemovedEntries = new List<DiffEntry>(),
			AddedCount = 1,
			TotalChangedCount = 1
		};

		// act
		await diffReporter.PrintReport(diffReport, false);

		// assert
		loggerMock.Verify(x => x.InfoLine(null), Times.Exactly(2));
		loggerMock.Verify(x => x.InfoLine("Added [1]"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("+ file1.txt"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("Diff finished with 1 difference."), Times.Exactly(1));
		loggerMock.VerifyNoOtherCalls();
		consoleAbstractionMock.VerifyNoOtherCalls();
	}

	[Test]
	public async Task Ensure_PrintReport_Works_Complex()
	{
		// arrange
		Mock<IConsoleAbstraction> consoleAbstractionMock = new Mock<IConsoleAbstraction>();
		Mock<ILogger> loggerMock = new Mock<ILogger>();
		IDiffReporter diffReporter = new TextDiffReporter(loggerMock.Object, consoleAbstractionMock.Object);
		DiffReport diffReport = new DiffReport
		{
			AddedEntries = new List<DiffEntry>
			{
				new DiffEntry
				{
					Created = new DateTime(2022, 6, 1, 0, 0, 0),
					LastAccess =  new DateTime(2022, 7, 1, 0, 0, 0),
					LastWrite =  new DateTime(2022, 6, 15, 0, 0, 0),
					Hash = "1234567890",
					RelativePath = "file1.txt",
					Size = 123
				}
			},
			RemovedEntries = new List<DiffEntry>
			{
				new DiffEntry
				{
					Created = new DateTime(2021, 6, 1, 0, 0, 0),
					LastAccess =  new DateTime(2021, 7, 1, 0, 0, 0),
					LastWrite =  new DateTime(2021, 6, 15, 0, 0, 0),
					Hash = "1231231231",
					RelativePath = "file2.txt",
					Size = 321
				},
				new DiffEntry
				{
					Created = new DateTime(2021, 6, 1, 0, 0, 0),
					LastAccess =  new DateTime(2021, 7, 1, 0, 0, 0),
					LastWrite =  new DateTime(2021, 6, 15, 0, 0, 0),
					Hash = "1231231231",
					RelativePath = "file3.txt",
					Size = 321
				},
				new DiffEntry
				{
					Created = new DateTime(2021, 6, 1, 0, 0, 0),
					LastAccess =  new DateTime(2021, 7, 1, 0, 0, 0),
					LastWrite =  new DateTime(2021, 6, 15, 0, 0, 0),
					Hash = "1231231231",
					RelativePath = "subdir/file3.txt",
					Size = 321
				}
			},
			ChangedSizeEntries = new List<DiffEntryChange>
			{
				new DiffEntryChange
				{
					First = new DiffEntry
					{
						Created = new DateTime(2021, 6, 1, 0, 0, 0),
						LastAccess =  new DateTime(2021, 7, 1, 0, 0, 0),
						LastWrite =  new DateTime(2021, 6, 15, 0, 0, 0),
						Hash = "1111111111",
						RelativePath = "file4.txt",
						Size = 444
					},
					Second = new DiffEntry
					{
						Created = new DateTime(2021, 6, 1, 0, 0, 0),
						LastAccess =  new DateTime(2021, 8, 1, 0, 0, 0),
						LastWrite =  new DateTime(2021, 8, 1, 0, 0, 0),
						Hash = "22222222222",
						RelativePath = "file4.txt",
						Size = 555
					}
				},
				new DiffEntryChange
				{
					First = new DiffEntry
					{
						Created = new DateTime(2021, 6, 1, 0, 0, 0),
						LastAccess =  new DateTime(2021, 7, 1, 0, 0, 0),
						LastWrite =  new DateTime(2021, 6, 15, 0, 0, 0),
						Hash = "141414141414",
						RelativePath = "file5.txt",
						Size = 6124
					},
					Second = new DiffEntry
					{
						Created = new DateTime(2021, 6, 1, 0, 0, 0),
						LastAccess =  new DateTime(2021, 8, 1, 0, 0, 0),
						LastWrite =  new DateTime(2021, 8, 1, 0, 0, 0),
						Hash = "151515151515",
						RelativePath = "file5.txt",
						Size = 8124
					}
				},
				new DiffEntryChange
				{
					First = new DiffEntry
					{
						Created = new DateTime(2021, 6, 1, 0, 0, 0),
						LastAccess =  new DateTime(2021, 7, 1, 0, 0, 0),
						LastWrite =  new DateTime(2021, 6, 15, 0, 0, 0),
						Hash = "141414141414",
						RelativePath = "subdir/file5.txt",
						Size = 6124
					},
					Second = new DiffEntry
					{
						Created = new DateTime(2021, 6, 1, 0, 0, 0),
						LastAccess =  new DateTime(2021, 8, 1, 0, 0, 0),
						LastWrite =  new DateTime(2021, 8, 1, 0, 0, 0),
						Hash = "151515151515",
						RelativePath = "subdir/file5.txt",
						Size = 8124
					}
				}
			},
			ChangedDatesEntries = new List<DiffEntryChange>
			{
				new DiffEntryChange
				{
					First = new DiffEntry
					{
						Created = new DateTime(2021, 6, 1, 0, 0, 0),
						LastAccess =  new DateTime(2021, 7, 1, 0, 0, 0),
						LastWrite =  new DateTime(2021, 6, 15, 0, 0, 0),
						Hash = "7777777777",
						RelativePath = "file6.txt",
						Size = 6124
					},
					Second = new DiffEntry
					{
						Created = new DateTime(2021, 6, 1, 1, 0, 0),
						LastAccess =  new DateTime(2021, 8, 1, 1, 0, 0),
						LastWrite =  new DateTime(2021, 8, 1, 1, 0, 0),
						Hash = "7777777777",
						RelativePath = "file6.txt",
						Size = 6124
					}
				},
				new DiffEntryChange
				{
					First = new DiffEntry
					{
						Created = null,
						LastAccess =  null,
						LastWrite =  null,
						Hash = "88888888888",
						RelativePath = "file7.txt",
						Size = 6124
					},
					Second = new DiffEntry
					{
						Created = new DateTime(2021, 6, 1, 1, 0, 0),
						LastAccess =  new DateTime(2021, 8, 1, 1, 0, 0),
						LastWrite =  new DateTime(2021, 8, 1, 1, 0, 0),
						Hash = "88888888888",
						RelativePath = "file7.txt",
						Size = 6124
					}
				}
			},
			ChangedPermissionsEntries = new List<DiffEntryChange>
			{
				new DiffEntryChange
				{
					First = new DiffEntry
					{
						Hash = "9999999999",
						RelativePath = "file8.txt",
						Permissions = "00777",
						Size = 6124
					},
					Second = new DiffEntry
					{
						Hash = "9999999999",
						RelativePath = "file8.txt",
						Permissions = "00644",
						Size = 6124
					}
				},
				new DiffEntryChange
				{
					First = new DiffEntry
					{
						Hash = "171717171717",
						RelativePath = "file9.txt",
						Permissions = null,
						Size = 6124
					},
					Second = new DiffEntry
					{
						Hash = "171717171717",
						RelativePath = "file9.txt",
						Permissions = "00644",
						Size = 6124
					}
				},
			},
			AddedCount = 1,
			RemovedCount = 3,
			ChangedSizeCount = 3,
			ChangedDatesCount = 2,
			ChangedPermissionsCount = 2,
			TotalChangedCount = 11
		};

		// act
		await diffReporter.PrintReport(diffReport, false);

		// assert
		loggerMock.Verify(x => x.InfoLine("Added [1]"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("+ file1.txt"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("Removed [3]"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("- file2.txt"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("- [file3.txt]"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("	- file3.txt"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("	- subdir/file3.txt"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("Size Changed [3]"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("- file4.txt [444 B => 555 B, 111 B]"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("- [file5.txt] [5.98 KB => 7.934 KB, 1.953 KB]"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("	- file5.txt"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("	- subdir/file5.txt"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("Dates Changed [2]"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("- file6.txt"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("	- Creation Date: 06/01/2021 00:00:00 => 06/01/2021 01:00:00"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("	- Last Access Date: 07/01/2021 00:00:00 => 08/01/2021 01:00:00"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("	- Last Write Date: 06/15/2021 00:00:00 => 08/01/2021 01:00:00"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("- file7.txt"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("	- Creation Date: [null] => 06/01/2021 01:00:00"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("	- Last Access Date: [null] => 08/01/2021 01:00:00"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("	- Last Write Date: [null] => 08/01/2021 01:00:00"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("Permissions Changed [2]"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("- file8.txt [00777 => 00644]"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("- file9.txt [[null] => 00644]"), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine("Diff finished with 11 differences."), Times.Exactly(1));
		loggerMock.Verify(x => x.InfoLine(null), Times.Exactly(6));
		loggerMock.VerifyNoOtherCalls();
		consoleAbstractionMock.VerifyNoOtherCalls();
	}

	[Test]
	public async Task Ensure_PrintReport_Integrates_With_ConsoleLogger()
	{
		// arrange
		Mock<IConsoleAbstraction> consoleAbstractionMock = new Mock<IConsoleAbstraction>();
		Mock<IConsoleAbstraction> consoleAbstractionForDiffReporterMock = new Mock<IConsoleAbstraction>();
		ILogger logger = new ConsoleLogger(consoleAbstractionMock.Object);
		IDiffReporter diffReporter = new TextDiffReporter(logger, consoleAbstractionForDiffReporterMock.Object);
		DiffReport diffReport = new DiffReport
		{
			AddedEntries = new List<DiffEntry>
			{
				new DiffEntry
				{
					Created = new DateTime(2022, 6, 1, 0, 0, 0),
					LastAccess =  new DateTime(2022, 7, 1, 0, 0, 0),
					LastWrite =  new DateTime(2022, 6, 15, 0, 0, 0),
					Hash = "1234567890",
					RelativePath = "file1.txt",
					Size = 123
				}
			},
			RemovedEntries = new List<DiffEntry>
			{
				new DiffEntry
				{
					Created = new DateTime(2022, 6, 1, 0, 0, 0),
					LastAccess =  new DateTime(2022, 7, 1, 0, 0, 0),
					LastWrite =  new DateTime(2022, 6, 15, 0, 0, 0),
					Hash = "1234567890",
					RelativePath = "file2.txt",
					Size = 123
				}
			},
			AddedCount = 1,
			RemovedCount = 1,
			TotalChangedCount = 2
		};

		// act
		await diffReporter.PrintReport(diffReport, false);

		// assert
		consoleAbstractionMock.Verify(x => x.ConsoleOutWriteLineAsync(null), Times.Exactly(3));
		consoleAbstractionMock.Verify(x => x.ConsoleOutWriteLineAsync("Added [1]"), Times.Exactly(1));
		consoleAbstractionMock.Verify(x => x.ConsoleOutWriteLineAsync("+ file1.txt"), Times.Exactly(1));
		consoleAbstractionMock.Verify(x => x.ConsoleOutWriteLineAsync("Removed [1]"), Times.Exactly(1));
		consoleAbstractionMock.Verify(x => x.ConsoleOutWriteLineAsync("- file2.txt"), Times.Exactly(1));
		consoleAbstractionMock.Verify(x => x.ConsoleOutWriteLineAsync("Diff finished with 2 differences."), Times.Exactly(1));
		consoleAbstractionMock.VerifyNoOtherCalls();
		consoleAbstractionForDiffReporterMock.Verify(x => x.SetForegroundColor(ConsoleColor.Green), Times.Exactly(1));
		consoleAbstractionForDiffReporterMock.Verify(x => x.SetForegroundColor(ConsoleColor.Red), Times.Exactly(1));
		consoleAbstractionForDiffReporterMock.Verify(x => x.ResetColor(), Times.Exactly(2));
		consoleAbstractionForDiffReporterMock.VerifyNoOtherCalls();
	}
}