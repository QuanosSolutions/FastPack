using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FastPack.Lib;
using FastPack.Lib.Compression;
using FastPack.Lib.Logging;
using FastPack.Lib.ManifestManagement;
using FastPack.Lib.ManifestManagement.Serialization;
using FastPack.Lib.ManifestReporting;
using FastPack.Lib.Options;
using FastPack.Lib.Unpackers;
using FastPack.TestFramework;
using FastPack.TestFramework.Common;
using AwesomeAssertions;
using Moq;
using NUnit.Framework;
using System.Collections;

namespace FastPack.Tests.FastPack.Lib.Unpackers;

[TestFixture]
internal class ArchiveUnpackerV1Tests
{
	private class UnpackOptionsTestData
	{
		public static IEnumerable TestCases
		{
			get
			{
				yield return new TestCaseData(OptimizeForCopyOnWriteFilesystem.On);
				yield return new TestCaseData(OptimizeForCopyOnWriteFilesystem.Off);
			}
		}
	}
	
	[Test]
	public async Task Ensure_GetManifestFromStream_Works()
	{
		// arrange
		await using MemoryStream memoryStream = new MemoryStream();
		Mock<ILogger> loggerMock = new Mock<ILogger>();
		Mock<IArchiveSerializerFactory> archiveSerializerFactory = new Mock<IArchiveSerializerFactory>();
		Mock<IArchiveFileReader> archiveFileReader = new Mock<IArchiveFileReader>();
		archiveSerializerFactory.Setup(x => x.GetFileReader(1)).Returns(archiveFileReader.Object);
		archiveFileReader.Setup(x => x.ReadManifest(memoryStream)).ReturnsAsync(new Manifest());
		IUnpacker unpacker = new ArchiveUnpackerV1(loggerMock.Object);
		((ArchiveUnpackerV1)unpacker).SerializerFactory = archiveSerializerFactory.Object;

		// act
		Manifest manifestFromStream = await unpacker.GetManifestFromStream(memoryStream);

		// assert
		manifestFromStream.Should().NotBeNull();
		archiveFileReader.Verify(x => x.ReadManifest(memoryStream), Times.Exactly(1));
		archiveFileReader.VerifyNoOtherCalls();
		archiveSerializerFactory.Verify(x => x.GetFileReader(1), Times.Exactly(1));
		archiveSerializerFactory.VerifyNoOtherCalls();
	}

	[Test]
	[TestCaseSource(typeof(UnpackOptionsTestData), nameof(UnpackOptionsTestData.TestCases))]
	public async Task Ensure_Extract_TestCase1_Works(OptimizeForCopyOnWriteFilesystem optimizeForCopyOnWriteFilesystem)
	{
		// arrange
		Mock<ILogger> loggerMock = new Mock<ILogger>();
		Mock<IFileCompressorFactory> fileCompressorFactoryMock = new Mock<IFileCompressorFactory>();
		fileCompressorFactoryMock.Setup(x => x.GetCompressor(CompressionAlgorithm.Deflate)).Returns(new DeflateFileCompressor());
		using FastPackTestContext testContext = FastPackTestContext.Create(true);
		string filePath = testContext.GetTempFileName(".fup", true);
		await TestData.WriteToFile("FastPack.Lib.Unpackers/TestCase1.fup", filePath, GetType().Assembly);
		string outputDirectory = Path.Combine(testContext.TestRootDirectoryPath, "output");
		IUnpacker unpacker = new ArchiveUnpackerV1(loggerMock.Object);
		((ArchiveUnpackerV1)unpacker).FileCompressorFactory = fileCompressorFactoryMock.Object;

		// act
		int extract = await unpacker.Extract(filePath, new UnpackOptions { OutputDirectoryPath = outputDirectory, MaxDegreeOfParallelism = 4, ShowProgress = false, OptimizeForCopyOnWriteFilesystem = optimizeForCopyOnWriteFilesystem});

		// assert
		extract.Should().Be(0);
		File.Exists(Path.Combine(outputDirectory, "test.txt")).Should().Be(true);
		new FileInfo(Path.Combine(outputDirectory, "test.txt")).Length.Should().Be(3);
		loggerMock.Verify(x => x.InfoLine(It.IsAny<string>()));
		loggerMock.Verify(x => x.StartTextProgress(It.IsAny<string>()));
		loggerMock.Verify(x => x.FinishTextProgress(It.IsAny<string>()));
		fileCompressorFactoryMock.Verify(x => x.GetCompressor(CompressionAlgorithm.Deflate), Times.Exactly(1));
		fileCompressorFactoryMock.VerifyNoOtherCalls();
		loggerMock.VerifyNoOtherCalls();
	}

	[Test]
	[TestCaseSource(typeof(UnpackOptionsTestData), nameof(UnpackOptionsTestData.TestCases))]
	public async Task Ensure_Extract_TestCase2_Works(OptimizeForCopyOnWriteFilesystem optimizeForCopyOnWriteFilesystem)
	{
		// arrange
		Mock<ILogger> loggerMock = new Mock<ILogger>();
		using FastPackTestContext testContext = FastPackTestContext.Create(true);
		string filePath = testContext.GetTempFileName(".fup", true);
		await TestData.WriteToFile("FastPack.Lib.Unpackers/TestCase2.fup", filePath, GetType().Assembly);
		string outputDirectory = Path.Combine(testContext.TestRootDirectoryPath, "output");
		IUnpacker unpacker = new ArchiveUnpackerV1(loggerMock.Object);

		// act
		int extract = await unpacker.Extract(filePath, new UnpackOptions { OutputDirectoryPath = outputDirectory, MaxDegreeOfParallelism = 4, OptimizeForCopyOnWriteFilesystem = optimizeForCopyOnWriteFilesystem});

		// assert
		extract.Should().Be(0);
		Directory.Exists(Path.Combine(outputDirectory, "subDir")).Should().Be(true);
		File.Exists(Path.Combine(outputDirectory, "test.txt")).Should().Be(true);
		File.Exists(Path.Combine(outputDirectory, "subDir", "test.txt")).Should().Be(true);
		File.Exists(Path.Combine(outputDirectory, "subDir", "biggerFile.txt")).Should().Be(true);
		new FileInfo(Path.Combine(outputDirectory, "test.txt")).Length.Should().Be(3);
		new FileInfo(Path.Combine(outputDirectory, "subDir", "test.txt")).Length.Should().Be(3);
		new FileInfo(Path.Combine(outputDirectory, "subDir", "biggerFile.txt")).Length.Should().Be(1520);
		loggerMock.Verify(x => x.InfoLine(It.IsAny<string>()));
		loggerMock.Verify(x => x.StartTextProgress(It.IsAny<string>()));
		if (Environment.UserInteractive && !Console.IsOutputRedirected)
			loggerMock.Verify(x => x.ReportTextProgress(It.IsAny<double>(), It.IsAny<string>()));
		loggerMock.Verify(x => x.FinishTextProgress(It.IsAny<string>()));
		loggerMock.VerifyNoOtherCalls();
	}

	[Test]
	[TestCaseSource(typeof(UnpackOptionsTestData), nameof(UnpackOptionsTestData.TestCases))]
	public async Task Ensure_Extract_TestCase3_Works(OptimizeForCopyOnWriteFilesystem optimizeForCopyOnWriteFilesystem)
	{
		// arrange
		Mock<ILogger> loggerMock = new Mock<ILogger>();
		using FastPackTestContext testContext = FastPackTestContext.Create(true);
		string filePath = testContext.GetTempFileName(".fup", true);
		await TestData.WriteToFile("FastPack.Lib.Unpackers/TestCase3.fup", filePath, GetType().Assembly);
		string outputDirectory = Path.Combine(testContext.TestRootDirectoryPath, "output");
		IUnpacker unpacker = new ArchiveUnpackerV1(loggerMock.Object);

		// act
		int extract = await unpacker.Extract(filePath, new UnpackOptions { OutputDirectoryPath = outputDirectory, MaxDegreeOfParallelism = 1, ShowProgress = false, OptimizeForCopyOnWriteFilesystem = optimizeForCopyOnWriteFilesystem});

		// assert
		extract.Should().Be(0);
		Directory.Exists(Path.Combine(outputDirectory, "subDir")).Should().Be(true);
		Directory.Exists(Path.Combine(outputDirectory, "subDir2")).Should().Be(true);
		Directory.Exists(Path.Combine(outputDirectory, "subDir3")).Should().Be(true);
		Directory.Exists(Path.Combine(outputDirectory, "subDir4")).Should().Be(true);
		Directory.Exists(Path.Combine(outputDirectory, "subDir5")).Should().Be(true);
		Directory.Exists(Path.Combine(outputDirectory, "subDir6")).Should().Be(true);
		File.Exists(Path.Combine(outputDirectory, "test.txt")).Should().Be(true);
		File.Exists(Path.Combine(outputDirectory, "bigFile.txt")).Should().Be(true);
		File.Exists(Path.Combine(outputDirectory, "biggerFile.txt")).Should().Be(true);
		File.Exists(Path.Combine(outputDirectory, "biggestFile.txt")).Should().Be(true);
		File.Exists(Path.Combine(outputDirectory, "subDir", "test.txt")).Should().Be(true);
		File.Exists(Path.Combine(outputDirectory, "subDir", "bigFile.txt")).Should().Be(true);
		File.Exists(Path.Combine(outputDirectory, "subDir", "biggerFile.txt")).Should().Be(true);
		File.Exists(Path.Combine(outputDirectory, "subDir", "biggestFile.txt")).Should().Be(true);
		File.Exists(Path.Combine(outputDirectory, "subDir2", "test.txt")).Should().Be(true);
		File.Exists(Path.Combine(outputDirectory, "subDir2", "bigFile.txt")).Should().Be(true);
		File.Exists(Path.Combine(outputDirectory, "subDir2", "biggerFile.txt")).Should().Be(true);
		File.Exists(Path.Combine(outputDirectory, "subDir2", "biggestFile.txt")).Should().Be(true);
		File.Exists(Path.Combine(outputDirectory, "subDir3", "test.txt")).Should().Be(true);
		File.Exists(Path.Combine(outputDirectory, "subDir3", "bigFile.txt")).Should().Be(true);
		File.Exists(Path.Combine(outputDirectory, "subDir3", "biggerFile.txt")).Should().Be(true);
		File.Exists(Path.Combine(outputDirectory, "subDir3", "biggestFile.txt")).Should().Be(true);
		File.Exists(Path.Combine(outputDirectory, "subDir4", "test.txt")).Should().Be(true);
		File.Exists(Path.Combine(outputDirectory, "subDir4", "bigFile.txt")).Should().Be(true);
		File.Exists(Path.Combine(outputDirectory, "subDir4", "biggerFile.txt")).Should().Be(true);
		File.Exists(Path.Combine(outputDirectory, "subDir4", "biggestFile.txt")).Should().Be(true);
		File.Exists(Path.Combine(outputDirectory, "subDir5", "test.txt")).Should().Be(true);
		File.Exists(Path.Combine(outputDirectory, "subDir5", "bigFile.txt")).Should().Be(true);
		File.Exists(Path.Combine(outputDirectory, "subDir5", "biggerFile.txt")).Should().Be(true);
		File.Exists(Path.Combine(outputDirectory, "subDir5", "biggestFile.txt")).Should().Be(true);
		File.Exists(Path.Combine(outputDirectory, "subDir6", "test.txt")).Should().Be(true);
		File.Exists(Path.Combine(outputDirectory, "subDir6", "bigFile.txt")).Should().Be(true);
		File.Exists(Path.Combine(outputDirectory, "subDir6", "biggerFile.txt")).Should().Be(true);
		File.Exists(Path.Combine(outputDirectory, "subDir6", "biggestFile.txt")).Should().Be(true);
		new FileInfo(Path.Combine(outputDirectory, "test.txt")).Length.Should().Be(3);
		new FileInfo(Path.Combine(outputDirectory, "subDir", "test.txt")).Length.Should().Be(3);
		new FileInfo(Path.Combine(outputDirectory, "subDir", "bigFile.txt")).Length.Should().Be(639916);
		new FileInfo(Path.Combine(outputDirectory, "subDir", "biggerFile.txt")).Length.Should().Be(1330000);
		new FileInfo(Path.Combine(outputDirectory, "subDir", "biggestFile.txt")).Length.Should().Be(4239375);
		new FileInfo(Path.Combine(outputDirectory, "subDir2", "test.txt")).Length.Should().Be(3);
		new FileInfo(Path.Combine(outputDirectory, "subDir2", "bigFile.txt")).Length.Should().Be(639916);
		new FileInfo(Path.Combine(outputDirectory, "subDir2", "biggerFile.txt")).Length.Should().Be(1330000);
		new FileInfo(Path.Combine(outputDirectory, "subDir2", "biggestFile.txt")).Length.Should().Be(4239375);
		new FileInfo(Path.Combine(outputDirectory, "subDir3", "test.txt")).Length.Should().Be(3);
		new FileInfo(Path.Combine(outputDirectory, "subDir3", "bigFile.txt")).Length.Should().Be(639916);
		new FileInfo(Path.Combine(outputDirectory, "subDir3", "biggerFile.txt")).Length.Should().Be(1330000);
		new FileInfo(Path.Combine(outputDirectory, "subDir3", "biggestFile.txt")).Length.Should().Be(4239375);
		new FileInfo(Path.Combine(outputDirectory, "subDir4", "test.txt")).Length.Should().Be(3);
		new FileInfo(Path.Combine(outputDirectory, "subDir4", "bigFile.txt")).Length.Should().Be(639916);
		new FileInfo(Path.Combine(outputDirectory, "subDir4", "biggerFile.txt")).Length.Should().Be(1330000);
		new FileInfo(Path.Combine(outputDirectory, "subDir4", "biggestFile.txt")).Length.Should().Be(4239375);
		new FileInfo(Path.Combine(outputDirectory, "subDir5", "test.txt")).Length.Should().Be(3);
		new FileInfo(Path.Combine(outputDirectory, "subDir5", "bigFile.txt")).Length.Should().Be(639916);
		new FileInfo(Path.Combine(outputDirectory, "subDir5", "biggerFile.txt")).Length.Should().Be(1330000);
		new FileInfo(Path.Combine(outputDirectory, "subDir5", "biggestFile.txt")).Length.Should().Be(4239375);
		new FileInfo(Path.Combine(outputDirectory, "subDir6", "test.txt")).Length.Should().Be(3);
		new FileInfo(Path.Combine(outputDirectory, "subDir6", "bigFile.txt")).Length.Should().Be(639916);
		new FileInfo(Path.Combine(outputDirectory, "subDir6", "biggerFile.txt")).Length.Should().Be(1330000);
		new FileInfo(Path.Combine(outputDirectory, "subDir6", "biggestFile.txt")).Length.Should().Be(4239375);
		loggerMock.Verify(x => x.InfoLine(It.IsAny<string>()));
		loggerMock.Verify(x => x.StartTextProgress(It.IsAny<string>()));
		loggerMock.Verify(x => x.FinishTextProgress(It.IsAny<string>()));
		loggerMock.VerifyNoOtherCalls();
	}

	[Test]
	public async Task Ensure_Extract_TestCase3_Works_With_Filters()
	{
		// arrange
		Mock<ILogger> loggerMock = new Mock<ILogger>();
		Mock<Filter> filterMock = new Mock<Filter>();
		filterMock.Setup(x => x.Apply(It.IsAny<IEnumerable<Unpacker.EntryWithGroup>>(), It.IsAny<IFilterOptions>(),
			It.IsAny<char>(), It.IsAny<Func<Unpacker.EntryWithGroup, string>>(),
			It.IsAny<Func<Unpacker.EntryWithGroup, bool>>())).CallBase();
		using FastPackTestContext testContext = FastPackTestContext.Create(true);
		string filePath = testContext.GetTempFileName(".fup", true);
		await TestData.WriteToFile("FastPack.Lib.Unpackers/TestCase3.fup", filePath, GetType().Assembly);
		string outputDirectory = Path.Combine(testContext.TestRootDirectoryPath, "output");
		IUnpacker unpacker = new ArchiveUnpackerV1(loggerMock.Object);
		((Unpacker)unpacker).Filter = filterMock.Object;

		// act
		int extract = await unpacker.Extract(filePath, new UnpackOptions { OutputDirectoryPath = outputDirectory, MaxDegreeOfParallelism = 1, ShowProgress = false, IncludeFilters = { "subDir" }, ExcludeFilters = { "**/biggestFile.txt" } });

		// assert
		extract.Should().Be(0);
		Directory.Exists(Path.Combine(outputDirectory, "subDir")).Should().Be(true);
		Directory.Exists(Path.Combine(outputDirectory, "subDir2")).Should().Be(false);
		Directory.Exists(Path.Combine(outputDirectory, "subDir3")).Should().Be(false);
		Directory.Exists(Path.Combine(outputDirectory, "subDir4")).Should().Be(false);
		Directory.Exists(Path.Combine(outputDirectory, "subDir5")).Should().Be(false);
		Directory.Exists(Path.Combine(outputDirectory, "subDir6")).Should().Be(false);
		File.Exists(Path.Combine(outputDirectory, "test.txt")).Should().Be(false);
		File.Exists(Path.Combine(outputDirectory, "bigFile.txt")).Should().Be(false);
		File.Exists(Path.Combine(outputDirectory, "biggerFile.txt")).Should().Be(false);
		File.Exists(Path.Combine(outputDirectory, "biggestFile.txt")).Should().Be(false);
		File.Exists(Path.Combine(outputDirectory, "subDir", "test.txt")).Should().Be(true);
		File.Exists(Path.Combine(outputDirectory, "subDir", "bigFile.txt")).Should().Be(true);
		File.Exists(Path.Combine(outputDirectory, "subDir", "biggerFile.txt")).Should().Be(true);
		File.Exists(Path.Combine(outputDirectory, "subDir", "biggestFile.txt")).Should().Be(false);
		File.Exists(Path.Combine(outputDirectory, "subDir2", "test.txt")).Should().Be(false);
		File.Exists(Path.Combine(outputDirectory, "subDir2", "bigFile.txt")).Should().Be(false);
		File.Exists(Path.Combine(outputDirectory, "subDir2", "biggerFile.txt")).Should().Be(false);
		File.Exists(Path.Combine(outputDirectory, "subDir2", "biggestFile.txt")).Should().Be(false);
		File.Exists(Path.Combine(outputDirectory, "subDir3", "test.txt")).Should().Be(false);
		File.Exists(Path.Combine(outputDirectory, "subDir3", "bigFile.txt")).Should().Be(false);
		File.Exists(Path.Combine(outputDirectory, "subDir3", "biggerFile.txt")).Should().Be(false);
		File.Exists(Path.Combine(outputDirectory, "subDir3", "biggestFile.txt")).Should().Be(false);
		File.Exists(Path.Combine(outputDirectory, "subDir4", "test.txt")).Should().Be(false);
		File.Exists(Path.Combine(outputDirectory, "subDir4", "bigFile.txt")).Should().Be(false);
		File.Exists(Path.Combine(outputDirectory, "subDir4", "biggerFile.txt")).Should().Be(false);
		File.Exists(Path.Combine(outputDirectory, "subDir4", "biggestFile.txt")).Should().Be(false);
		File.Exists(Path.Combine(outputDirectory, "subDir5", "test.txt")).Should().Be(false);
		File.Exists(Path.Combine(outputDirectory, "subDir5", "bigFile.txt")).Should().Be(false);
		File.Exists(Path.Combine(outputDirectory, "subDir5", "biggerFile.txt")).Should().Be(false);
		File.Exists(Path.Combine(outputDirectory, "subDir5", "biggestFile.txt")).Should().Be(false);
		File.Exists(Path.Combine(outputDirectory, "subDir6", "test.txt")).Should().Be(false);
		File.Exists(Path.Combine(outputDirectory, "subDir6", "bigFile.txt")).Should().Be(false);
		File.Exists(Path.Combine(outputDirectory, "subDir6", "biggerFile.txt")).Should().Be(false);
		File.Exists(Path.Combine(outputDirectory, "subDir6", "biggestFile.txt")).Should().Be(false);
		new FileInfo(Path.Combine(outputDirectory, "subDir", "test.txt")).Length.Should().Be(3);
		new FileInfo(Path.Combine(outputDirectory, "subDir", "bigFile.txt")).Length.Should().Be(639916);
		new FileInfo(Path.Combine(outputDirectory, "subDir", "biggerFile.txt")).Length.Should().Be(1330000);
		loggerMock.Verify(x => x.InfoLine(It.IsAny<string>()));
		loggerMock.Verify(x => x.StartTextProgress(It.IsAny<string>()));
		loggerMock.Verify(x => x.FinishTextProgress(It.IsAny<string>()));
		filterMock.Verify(x => x.Apply(It.IsAny<IEnumerable<Unpacker.EntryWithGroup>>(), It.IsAny<IFilterOptions>(),
			It.IsAny<char>(), It.IsAny<Func<Unpacker.EntryWithGroup, string>>(),
			It.IsAny<Func<Unpacker.EntryWithGroup, bool>>()), Times.Exactly(1));
		loggerMock.VerifyNoOtherCalls();
		filterMock.VerifyNoOtherCalls();
	}

	[Test]
	public async Task Ensure_Extract_With_DryRun_Works()
	{
		// arrange
		Mock<ILogger> loggerMock = new Mock<ILogger>();
		Mock<IManifestReporterFactory> manifestReporterFactoryMock = new Mock<IManifestReporterFactory>();
		Mock<IManifestReporter> manifestReporterMock = new Mock<IManifestReporter>();
		manifestReporterFactoryMock.Setup(x => x.GetManifestReporter(OutputFormat.Text, loggerMock.Object)).Returns(manifestReporterMock.Object);
		IUnpacker unpacker = new ArchiveUnpackerV1(loggerMock.Object);

		await TestUtil.UseRandomTempDirectory(async tempDirectory => {
			string testFileName = "test.fup";
			string filePath = Path.Combine(tempDirectory, testFileName);
			string outputDirectory = Path.Combine(tempDirectory, "output");
			Manifest manifest = new Manifest
			{
				MetaDataOptions = MetaDataOptions.None,
				Entries = new List<ManifestEntry>
				{
					new()
					{
						Type = EntryType.File,
						Hash = "123456789",
						DataSize = 150,
						OriginalSize = 200,
						FileSystemEntries = new List<ManifestFileSystemEntry>
						{
							new()
							{
								RelativePath = "test.txt",
							}
						}
					}
				}
			};
			await using (FileStream stream = new(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None, Constants.BufferSize, Constants.OpenFileStreamsAsync))
			{
				IArchiveManifestWriter archiveManifestWriter = new BinaryArchiveManifestWriterV1();
				IArchiveFileWriter archiveFileWriter = new BinaryArchiveFileWriterV1(archiveManifestWriter);
				await archiveFileWriter.WriteHeader(stream, manifest);
				await archiveManifestWriter.WriteManifest(stream, manifest);
			}

			// act
			int extractTest1 = await unpacker.Extract(filePath, new UnpackOptions { OutputDirectoryPath = outputDirectory, MaxDegreeOfParallelism = 4, DryRun = true, PrettyPrint = true, DetailedDryRun = true });
			((ArchiveUnpackerV1)unpacker).ManifestReporterFactory = new Lazy<IManifestReporterFactory>(() => manifestReporterFactoryMock.Object);
			int extractTest2 = await unpacker.Extract(filePath, new UnpackOptions { OutputDirectoryPath = outputDirectory, MaxDegreeOfParallelism = 4, DryRun = true, PrettyPrint = true, DetailedDryRun = true });

			// assert
			extractTest1.Should().Be(0);
			extractTest2.Should().Be(0);
			loggerMock.Verify(x => x.InfoLine(It.IsAny<string>()));
			loggerMock.Verify(x => x.StartTextProgress(It.IsAny<string>()));
			loggerMock.Verify(x => x.FinishTextProgress(It.IsAny<string>()));
			manifestReporterMock.Verify(x => x.PrintReport(It.IsAny<Manifest>(), true, true, true));
			manifestReporterFactoryMock.Verify(x => x.GetManifestReporter(OutputFormat.Text, loggerMock.Object), Times.Exactly(1));
			manifestReporterMock.VerifyNoOtherCalls();
			manifestReporterFactoryMock.VerifyNoOtherCalls();
			loggerMock.VerifyNoOtherCalls();
		});
	}

	[Test]
	public async Task Ensure_Extract_Stops_When_Space_Is_Not_Enough()
	{
		// arrange
		Mock<ILogger> loggerMock = new Mock<ILogger>();
		IUnpacker unpacker = new ArchiveUnpackerV1(loggerMock.Object);

		await TestUtil.UseRandomTempDirectory(async tempDirectory => {
			string testFileName = "test.fup";
			string filePath = Path.Combine(tempDirectory, testFileName);
			string outputDirectory = Path.Combine(tempDirectory, "output");
			Manifest manifest = new Manifest
			{
				MetaDataOptions = MetaDataOptions.None,
				Entries = new List<ManifestEntry>
				{
					new()
					{
						Type = EntryType.File,
						Hash = "123456789",
						DataSize = long.MaxValue,
						OriginalSize = long.MaxValue,
						FileSystemEntries = new List<ManifestFileSystemEntry>
						{
							new()
							{
								RelativePath = "test.txt",
							}
						}
					}
				}
			};
			await using (FileStream stream = new(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None, Constants.BufferSize, Constants.OpenFileStreamsAsync))
			{
				IArchiveManifestWriter archiveManifestWriter = new BinaryArchiveManifestWriterV1();
				IArchiveFileWriter archiveFileWriter = new BinaryArchiveFileWriterV1(archiveManifestWriter);
				await archiveFileWriter.WriteHeader(stream, manifest);
				await archiveManifestWriter.WriteManifest(stream, manifest);
			}

			// act
			int extract = await unpacker.Extract(filePath, new UnpackOptions { OutputDirectoryPath = outputDirectory, MaxDegreeOfParallelism = 4 });

			// assert
			extract.Should().Be(ErrorConstants.Unpack_Not_Enough_Disk_Space);
			loggerMock.Verify(x => x.InfoLine(It.IsAny<string>()));
			loggerMock.Verify(x => x.StartTextProgress(It.IsAny<string>()));
			loggerMock.Verify(x => x.FinishTextProgress(It.IsAny<string>()));
			loggerMock.Verify(x => x.ErrorLine(It.IsAny<string>()));
			loggerMock.VerifyNoOtherCalls();
		});
	}

	[Test]
	public void Ensure_Constructor_Throws_Exception_When_Logger_Is_Null()
	{
		// arrange

		// act
		Action action = () => new ArchiveUnpackerV1(null);

		// assert
		action.Should().Throw<ArgumentNullException>().WithMessage("Value cannot be null. (Parameter 'logger')");
	}
}