using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using FastPack.Lib;
using FastPack.Lib.Actions;
using FastPack.Lib.Diff;
using FastPack.Lib.Logging;
using FastPack.Lib.ManifestManagement;
using FastPack.Lib.Matching;
using FastPack.Lib.Options;
using FastPack.Lib.Unpackers;
using FastPack.TestFramework.Common;
using AwesomeAssertions;
using Moq;
using NUnit.Framework;

namespace FastPack.Tests.FastPack.Lib.Actions;

[TestFixture]
internal class DiffActionTests
{
	[Test]
	public async Task Ensure_Run_Works()
	{
		// arrange
		using FastPackTestContext testContext = FastPackTestContext.Create(true);
		Mock<ILogger> loggerMock = new Mock<ILogger>();
		Mock<ITextMatchProviderFactory> textMatchProviderFactoryMock = new Mock<ITextMatchProviderFactory>();
		Mock<ITextMatchProvider> textMatchProviderMock = new Mock<ITextMatchProvider>();
		Mock<IProcessAbstraction> processAbstractionMock = new Mock<IProcessAbstraction>();
		Mock<IArchiveUnpackerFactory> archiveUnpackerFactoryMock = new Mock<IArchiveUnpackerFactory>();
		Mock<IDiffReporterFactory> diffReporterFactoryMock = new Mock<IDiffReporterFactory>();
		Mock<IFilter> filterMock = new Mock<IFilter>();
		Mock<IUnpacker> unpackerMock = new Mock<IUnpacker>();
		Mock<IDiffReporter> diffReporterMock = new Mock<IDiffReporter>();
		textMatchProviderFactoryMock.Setup(x => x.GetProvider(It.IsAny<TextMatchProviderType>())).Returns(textMatchProviderMock.Object);
		textMatchProviderMock.Setup(x => x.IsMatch(It.IsAny<string>(), "Valid", It.IsAny<bool>())).Returns(true);
		textMatchProviderMock.Setup(x => x.IsPatternValid("Valid")).Returns(true);
		textMatchProviderMock.Setup(x => x.IsPatternValid("Invalid")).Returns(false);
		archiveUnpackerFactoryMock.Setup(x => x.GetUnpacker(It.IsAny<Stream>(), It.IsAny<ILogger>())).ReturnsAsync(unpackerMock.Object);
		diffReporterFactoryMock.Setup(x => x.GetDiffReporter(It.IsAny<OutputFormat>(), It.IsAny<ILogger>())).Returns(diffReporterMock.Object);
		filterMock.Setup(x => x.Apply(It.IsAny<IEnumerable<DiffEntry>>(), It.IsAny<IFilterOptions>(),
				It.IsAny<char>(), It.IsAny<Func<DiffEntry, string>>(), It.IsAny<Func<DiffEntry, bool>>()))
			.Returns<IEnumerable<DiffEntry>, IFilterOptions, char, Func<DiffEntry, string>,
				Func<DiffEntry, bool>>((infos, options, arg3, arg4, arg5) =>
			{
				bool test = arg5(infos.First());
				string relativePath = arg4(infos.First());
				return infos.ToList();
			});
		int manifestsCreated = 0;
		Manifest firstManifest = new Manifest
		{
			Entries = new List<ManifestEntry>
			{
				new()
				{
					Hash = "123456789",
					OriginalSize = 123,
					FileSystemEntries = new List<ManifestFileSystemEntry>
					{
						new()
						{
							RelativePath = "test.txt",
							FilePermissions = null,
							CreationDateUtc = null,
							LastAccessDateUtc = null,
							LastWriteDateUtc = null,
						},
						new()
						{
							RelativePath = "test2.txt",
							FilePermissions = null,
							CreationDateUtc = null,
							LastAccessDateUtc = null,
							LastWriteDateUtc = null,
						}
					}
				}
			}
		};
		Manifest secondManifest = new Manifest()
		{
			Entries = new List<ManifestEntry>
			{
				new()
				{
					Hash = "123456789",
					OriginalSize = 123,
					FileSystemEntries = new List<ManifestFileSystemEntry>
					{
						new()
						{
							RelativePath = "test.txt",
							FilePermissions = UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute,
							CreationDateUtc = new DateTime(2022, 6, 1, 1, 1, 1),
							LastAccessDateUtc = new DateTime(2022, 6, 2, 1, 1, 1),
							LastWriteDateUtc = new DateTime(2022, 6, 3, 1, 1, 1),
						},
						new()
						{
							RelativePath = "test3.txt",
							FilePermissions = UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute,
							CreationDateUtc = new DateTime(2022, 6, 1, 1, 1, 1),
							LastAccessDateUtc = new DateTime(2022, 6, 2, 1, 1, 1),
							LastWriteDateUtc = new DateTime(2022, 6, 3, 1, 1, 1),
						}
					}
				}
			}
		};
		unpackerMock.Setup(x => x.GetManifestFromStream(It.IsAny<Stream>())).ReturnsAsync(() =>
		{
			if (manifestsCreated++ == 0)
				return firstManifest;
			return secondManifest;
		});
		DiffOptions diffOptions = new DiffOptions()
		{
			FirstFilePath = Path.Combine(testContext.TestRootDirectoryPath, "first.fup"),
			SecondFilePath = Path.Combine(testContext.TestRootDirectoryPath, "second.fup"),
			IncludeFilters = { "Valid" },
			PrettyPrint = true,
			ExtractionEnabled = false,
			DiffSettings = DiffOptions.DiffSetting.Size | DiffOptions.DiffSetting.Date | DiffOptions.DiffSetting.Permission
		};
		await File.WriteAllTextAsync(diffOptions.FirstFilePath, "");
		await File.WriteAllTextAsync(diffOptions.SecondFilePath, "");
		DiffAction diffAction = new DiffAction(loggerMock.Object, diffOptions)
		{
			ProcessAbstraction = processAbstractionMock.Object,
			TextMatchProviderFactory = textMatchProviderFactoryMock.Object,
			ArchiveUnpackerFactory = archiveUnpackerFactoryMock.Object,
			DiffReporterFactory = diffReporterFactoryMock.Object,
			Filter = filterMock.Object,
		};

		// act
		int returnCode = await diffAction.Run();

		// assert
		returnCode.Should().Be(0);
		loggerMock.Verify(x => x.InfoLine(It.IsAny<string>()));
		textMatchProviderMock.Verify(x => x.IsPatternValid("Valid"), Times.Exactly(1));
		archiveUnpackerFactoryMock.Verify(x => x.GetUnpacker(It.IsAny<Stream>(), It.IsAny<ILogger>()), Times.Exactly(2));
		unpackerMock.Verify(x => x.GetManifestFromStream(It.IsAny<Stream>()), Times.Exactly(2));
		filterMock.Verify(x => x.Apply(It.IsAny<IEnumerable<DiffEntry>>(), It.IsAny<IFilterOptions>(),
			It.IsAny<char>(), It.IsAny<Func<DiffEntry, string>>(), It.IsAny<Func<DiffEntry, bool>>()), Times.Exactly(2));
		diffReporterMock.Verify(x => x.PrintReport(It.IsAny<DiffReport>(), true));
		loggerMock.VerifyNoOtherCalls();
		textMatchProviderFactoryMock.VerifyNoOtherCalls();
		textMatchProviderMock.VerifyNoOtherCalls();
		processAbstractionMock.VerifyNoOtherCalls();
		archiveUnpackerFactoryMock.VerifyNoOtherCalls();
		diffReporterFactoryMock.VerifyNoOtherCalls();
		filterMock.VerifyNoOtherCalls();
		unpackerMock.VerifyNoOtherCalls();
		diffReporterMock.VerifyNoOtherCalls();
	}

	[Test]
	public async Task Ensure_Run_Works_With_Extraction()
	{
		// arrange
		using FastPackTestContext testContext = FastPackTestContext.Create(true);
		Mock<ILogger> loggerMock = new Mock<ILogger>();
		Mock<IProcessAbstraction> processAbstractionMock = new Mock<IProcessAbstraction>();
		string firstFilePath = testContext.GetTempFileName(".fup", true);
		string secondFilePath = testContext.GetTempFileName(".fup", true);
		await TestData.WriteToFile("FastPack.Lib.Diff/first.fup", firstFilePath, GetType().Assembly);
		await TestData.WriteToFile("FastPack.Lib.Diff/second.fup", secondFilePath, GetType().Assembly);

		DiffOptions diffOptions = new DiffOptions
		{
			FirstFilePath = firstFilePath,
			SecondFilePath = secondFilePath,
			PrettyPrint = true,
			ExtractionEnabled = true,
			DiffSettings = DiffOptions.DiffSetting.Size | DiffOptions.DiffSetting.Date | DiffOptions.DiffSetting.Permission,
			ExtractionDirectoryPath = Path.Combine(testContext.TestRootDirectoryPath, "extraction"),
		};
		DiffAction diffAction = new DiffAction(loggerMock.Object, diffOptions)
		{
			ProcessAbstraction = processAbstractionMock.Object,
		};

		// act
		int returnCode = await diffAction.Run();

		// assert
		returnCode.Should().Be(0);
		File.Exists(Path.Combine(diffOptions.ExtractionDirectoryPath, "Added", "test3.txt")).Should().Be(true);
		File.Exists(Path.Combine(diffOptions.ExtractionDirectoryPath, "Removed", "test2.txt")).Should().Be(true);
		File.Exists(Path.Combine(diffOptions.ExtractionDirectoryPath, "Changed", "1", "test.txt")).Should().Be(true);
		File.Exists(Path.Combine(diffOptions.ExtractionDirectoryPath, "Changed", "2", "test.txt")).Should().Be(true);
		loggerMock.Verify(x => x.InfoLine(It.IsAny<string>()));
		loggerMock.Verify(x => x.StartTextProgress(It.IsAny<string>()));
		if (Environment.UserInteractive && !Console.IsOutputRedirected)
			loggerMock.Verify(x => x.ReportTextProgress(It.IsAny<double>(), It.IsAny<string>()));
		loggerMock.Verify(x => x.FinishTextProgress(It.IsAny<string>()));
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			processAbstractionMock.Verify(x => x.Start("explorer.exe", It.IsAny<string>()), Times.Exactly(1));
		loggerMock.VerifyNoOtherCalls();
		processAbstractionMock.VerifyNoOtherCalls();
	}

	[Test]
	public async Task Ensure_Run_Works_With_Extraction_Same_File()
	{
		// arrange
		using FastPackTestContext testContext = FastPackTestContext.Create(true);
		Mock<ILogger> loggerMock = new Mock<ILogger>();
		Mock<IProcessAbstraction> processAbstractionMock = new Mock<IProcessAbstraction>();
		string firstFilePath = testContext.GetTempFileName(".fup", true);
		string secondFilePath = testContext.GetTempFileName(".fup", true);
		await TestData.WriteToFile("FastPack.Lib.Diff/first.fup", firstFilePath, GetType().Assembly);
		await TestData.WriteToFile("FastPack.Lib.Diff/first.fup", secondFilePath, GetType().Assembly);

		DiffOptions diffOptions = new DiffOptions
		{
			FirstFilePath = firstFilePath,
			SecondFilePath = secondFilePath,
			PrettyPrint = true,
			ExtractionEnabled = true,
			DiffSettings = DiffOptions.DiffSetting.Size | DiffOptions.DiffSetting.Date | DiffOptions.DiffSetting.Permission,
			ExtractionDirectoryPath = Path.Combine(testContext.TestRootDirectoryPath, "extraction"),
		};
		DiffAction diffAction = new DiffAction(loggerMock.Object, diffOptions)
		{
			ProcessAbstraction = processAbstractionMock.Object,
		};

		// act
		int returnCode = await diffAction.Run();

		// assert
		returnCode.Should().Be(0);
		loggerMock.Verify(x => x.InfoLine(It.IsAny<string>()));
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			processAbstractionMock.Verify(x => x.Start("explorer.exe", It.IsAny<string>()), Times.Exactly(1));
		loggerMock.VerifyNoOtherCalls();
		processAbstractionMock.VerifyNoOtherCalls();
	}

	[Test]
	public async Task Ensure_Run_Throws_Exception_For_FirstFilePath_Null()
	{
		// arrange
		Mock<ILogger> loggerMock = new Mock<ILogger>();
		DiffOptions diffOptions = new DiffOptions()
		{
			FirstFilePath = null,
		};

		// act
		Func<Task> func = async () => await new DiffAction(loggerMock.Object, diffOptions).Run();

		// assert
		await func.Should().ThrowAsync<InvalidOptionException>().WithMessage("Please provide the first file.");
	}

	[Test]
	public async Task Ensure_Run_Throws_Exception_For_FirstFilePath_NotExisting()
	{
		// arrange
		Mock<ILogger> loggerMock = new Mock<ILogger>();
		DiffOptions diffOptions = new DiffOptions()
		{
			FirstFilePath = "test.fup",
		};

		// act
		Func<Task> func = async () => await new DiffAction(loggerMock.Object, diffOptions).Run();

		// assert
		await func.Should().ThrowAsync<InvalidOptionException>();
	}

	[Test]
	public async Task Ensure_Run_Throws_Exception_For_SecondFilePath_Null()
	{
		// arrange
		using FastPackTestContext testContext = FastPackTestContext.Create(true);
		Mock<ILogger> loggerMock = new Mock<ILogger>();
		DiffOptions diffOptions = new DiffOptions()
		{
			FirstFilePath = Path.Combine(testContext.TestRootDirectoryPath, "first.fup"),
			SecondFilePath = null,
		};
		await File.WriteAllTextAsync(diffOptions.FirstFilePath, "");

		// act
		Func<Task> func = async () => await new DiffAction(loggerMock.Object, diffOptions).Run();

		// assert
		await func.Should().ThrowAsync<InvalidOptionException>().WithMessage("Please provide the second file.");
	}

	[Test]
	public async Task Ensure_Run_Throws_Exception_For_SecondFilePath_NotExisting()
	{
		// arrange
		using FastPackTestContext testContext = FastPackTestContext.Create(true);
		Mock<ILogger> loggerMock = new Mock<ILogger>();
		DiffOptions diffOptions = new DiffOptions()
		{
			FirstFilePath = Path.Combine(testContext.TestRootDirectoryPath, "first.fup"),
			SecondFilePath = Path.Combine(testContext.TestRootDirectoryPath, "second.fup"),
		};
		await File.WriteAllTextAsync(diffOptions.FirstFilePath, "");

		// act
		Func<Task> func = async () => await new DiffAction(loggerMock.Object, diffOptions).Run();

		// assert
		await func.Should().ThrowAsync<InvalidOptionException>();
	}

	[Test]
	public async Task Ensure_Run_Throws_Exception_For_Invalid_IncludeFilters()
	{
		// arrange
		using FastPackTestContext testContext = FastPackTestContext.Create(true);
		Mock<ILogger> loggerMock = new Mock<ILogger>();
		Mock<ITextMatchProviderFactory> textMatchProviderFactoryMock = new Mock<ITextMatchProviderFactory>();
		Mock<ITextMatchProvider> textMatchProviderMock = new Mock<ITextMatchProvider>();
		textMatchProviderFactoryMock.Setup(x => x.GetProvider(It.IsAny<TextMatchProviderType>())).Returns(textMatchProviderMock.Object);
		textMatchProviderMock.Setup(x => x.IsPatternValid("Valid")).Returns(true);
		textMatchProviderMock.Setup(x => x.IsPatternValid("Invalid")).Returns(false);
		DiffOptions diffOptions = new DiffOptions()
		{
			FirstFilePath = Path.Combine(testContext.TestRootDirectoryPath, "first.fup"),
			SecondFilePath = Path.Combine(testContext.TestRootDirectoryPath, "second.fup"),
			IncludeFilters = { "Valid", "Invalid" }
		};
		await File.WriteAllTextAsync(diffOptions.FirstFilePath, "");
		await File.WriteAllTextAsync(diffOptions.SecondFilePath, "");
		DiffAction diffAction = new DiffAction(loggerMock.Object, diffOptions)
		{
			TextMatchProviderFactory = textMatchProviderFactoryMock.Object
		};

		// act
		Func<Task> func = async () => await diffAction.Run();

		// assert
		await func.Should().ThrowAsync<InvalidOptionException>().WithMessage("Invalid include filter: Invalid");
	}

	[Test]
	public async Task Ensure_Run_Throws_Exception_For_Invalid_ExcludeFilters()
	{
		// arrange
		using FastPackTestContext testContext = FastPackTestContext.Create(true);
		Mock<ILogger> loggerMock = new Mock<ILogger>();
		Mock<ITextMatchProviderFactory> textMatchProviderFactoryMock = new Mock<ITextMatchProviderFactory>();
		Mock<ITextMatchProvider> textMatchProviderMock = new Mock<ITextMatchProvider>();
		textMatchProviderFactoryMock.Setup(x => x.GetProvider(It.IsAny<TextMatchProviderType>())).Returns(textMatchProviderMock.Object);
		textMatchProviderMock.Setup(x => x.IsPatternValid("Valid")).Returns(true);
		textMatchProviderMock.Setup(x => x.IsPatternValid("Invalid")).Returns(false);
		DiffOptions diffOptions = new DiffOptions()
		{
			FirstFilePath = Path.Combine(testContext.TestRootDirectoryPath, "first.fup"),
			SecondFilePath = Path.Combine(testContext.TestRootDirectoryPath, "second.fup"),
			ExcludeFilters = { "Valid", "Invalid" }
		};
		await File.WriteAllTextAsync(diffOptions.FirstFilePath, "");
		await File.WriteAllTextAsync(diffOptions.SecondFilePath, "");
		DiffAction diffAction = new DiffAction(loggerMock.Object, diffOptions)
		{
			TextMatchProviderFactory = textMatchProviderFactoryMock.Object
		};

		// act
		Func<Task> func = async () => await diffAction.Run();

		// assert
		await func.Should().ThrowAsync<InvalidOptionException>().WithMessage("Invalid exclude filter: Invalid");
	}
}