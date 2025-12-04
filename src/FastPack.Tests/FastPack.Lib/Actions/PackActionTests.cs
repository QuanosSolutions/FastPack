using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FastPack.Lib;
using FastPack.Lib.Actions;
using FastPack.Lib.Compression;
using FastPack.Lib.Hashing;
using FastPack.Lib.Logging;
using FastPack.Lib.ManifestManagement;
using FastPack.Lib.ManifestManagement.Serialization;
using FastPack.Lib.ManifestReporting;
using FastPack.Lib.Matching;
using FastPack.Lib.Options;
using FastPack.TestFramework;
using AwesomeAssertions;
using Moq;
using NUnit.Framework;

namespace FastPack.Tests.FastPack.Lib.Actions;

[TestFixture]
internal class PackActionTests
{

	[Test]
	public async Task Ensure_Run_Works()
	{
		// arrange
		Mock<ILogger> loggerMock = new Mock<ILogger>();
		Mock<IManifestReporterFactory> manifestReporterFactoryMock = new Mock<IManifestReporterFactory>();
		Mock<IArchiveSerializerFactory> archiveSerializerFactoryMock = new Mock<IArchiveSerializerFactory>();
		Mock<IArchiveFileWriter> archiveFileWriterMock = new Mock<IArchiveFileWriter>();
		Mock<IManifestReporter> manifestReporterMock = new Mock<IManifestReporter>();
		Mock<IFilter> filterMock = new Mock<IFilter>();
		manifestReporterFactoryMock.Setup(x => x.GetManifestReporter(It.IsAny<OutputFormat>(), It.IsAny<ILogger>()))
			.Returns(manifestReporterMock.Object);
		filterMock.Setup(x => x.Apply(It.IsAny<IEnumerable<FileSystemInfo>>(), It.IsAny<IFilterOptions>(),
				It.IsAny<char>(), It.IsAny<Func<FileSystemInfo, string>>(), It.IsAny<Func<FileSystemInfo, bool>>()))
			.Returns<IEnumerable<FileSystemInfo>, IFilterOptions, char, Func<FileSystemInfo, string>,
				Func<FileSystemInfo, bool>>((infos, options, arg3, arg4, arg5) =>
				{
					bool test = arg5(infos.First());
					string relativePath = arg4(infos.First());
					return infos.ToList();
				});
		archiveSerializerFactoryMock.Setup(x => x.GetFileWriter(It.IsAny<ushort>()))
			.Returns(archiveFileWriterMock.Object);
		await TestUtil.UseRandomTempDirectory(async tempDirectory =>
		{
			string inputDirectory = Path.Combine(tempDirectory, "input");
			Directory.CreateDirectory(inputDirectory);
			string outputFile = Path.Combine(tempDirectory, "ouput", "output.fup");
			PackOptions packOptions = new PackOptions
			{
				InputDirectoryPath = inputDirectory,
				OutputFilePath = outputFile,
				MaxDegreeOfParallelism = 2,
				MaxMemory = 8
			};
			PackAction packAction = new PackAction(loggerMock.Object, packOptions)
			{
				ArchiveSerializerFactory = archiveSerializerFactoryMock.Object,
				ManifestReporterFactory = new Lazy<IManifestReporterFactory>(() => manifestReporterFactoryMock.Object),
				Filter = filterMock.Object,
			};
			await File.WriteAllTextAsync(Path.Combine(inputDirectory, "test.txt"), "test", Encoding.UTF8);
			await File.WriteAllTextAsync(Path.Combine(inputDirectory, "test2.txt"), "test", Encoding.UTF8);
			await File.WriteAllTextAsync(Path.Combine(inputDirectory, "test3.txt"), "test3", Encoding.UTF8);
			await File.WriteAllTextAsync(Path.Combine(inputDirectory, "test4.txt"), "test4", Encoding.UTF8);
			Directory.CreateDirectory(Path.Combine(inputDirectory, "subDir"));
			await File.WriteAllTextAsync(Path.Combine(inputDirectory, "subDir", "test.txt"), "test test test", Encoding.UTF8);

			// act
			int returnCode = await packAction.Run();

			// assert
			returnCode.Should().Be(0);
			manifestReporterMock.VerifyNoOtherCalls();
			filterMock.Verify(x => x.Apply(It.IsAny<IEnumerable<FileSystemInfo>>(), It.IsAny<IFilterOptions>(),
				It.IsAny<char>(), It.IsAny<Func<FileSystemInfo, string>>(), It.IsAny<Func<FileSystemInfo, bool>>()));
			archiveSerializerFactoryMock.Verify(x => x.GetFileWriter(It.IsAny<ushort>()), Times.Exactly(1));
			archiveFileWriterMock.Verify(x => x.WriteHeader(It.IsAny<Stream>(), It.IsAny<Manifest>()), Times.Exactly(1));
			archiveFileWriterMock.Verify(x => x.WriteFileData(It.IsAny<Stream>(), It.IsAny<ManifestEntry>(), It.IsAny<Func<Stream, Task>>()), Times.Exactly(4));
			archiveFileWriterMock.Verify(x => x.WriteManifest(It.IsAny<Stream>(), It.IsAny<Manifest>()), Times.Exactly(1));
			filterMock.VerifyNoOtherCalls();
			manifestReporterFactoryMock.VerifyNoOtherCalls();
			archiveFileWriterMock.VerifyNoOtherCalls();
			archiveSerializerFactoryMock.VerifyNoOtherCalls();
		});
	}


	[Test]
	public async Task Ensure_Run_Works_2()
	{
		// arrange
		Mock<ILogger> loggerMock = new Mock<ILogger>();
		Mock<IManifestReporterFactory> manifestReporterFactoryMock = new Mock<IManifestReporterFactory>();
		Mock<IManifestReporter> manifestReporterMock = new Mock<IManifestReporter>();
		Mock<IFilter> filterMock = new Mock<IFilter>();
		manifestReporterFactoryMock.Setup(x => x.GetManifestReporter(It.IsAny<OutputFormat>(), It.IsAny<ILogger>()))
			.Returns(manifestReporterMock.Object);
		filterMock.Setup(x => x.Apply(It.IsAny<IEnumerable<FileSystemInfo>>(), It.IsAny<IFilterOptions>(),
				It.IsAny<char>(), It.IsAny<Func<FileSystemInfo, string>>(), It.IsAny<Func<FileSystemInfo, bool>>()))
			.Returns<IEnumerable<FileSystemInfo>, IFilterOptions, char, Func<FileSystemInfo, string>,
				Func<FileSystemInfo, bool>>((infos, options, arg3, arg4, arg5) =>
				{
					bool test = arg5(infos.First());
					string relativePath = arg4(infos.First());
					return infos.ToList();
				});
		await TestUtil.UseRandomTempDirectory(async tempDirectory =>
		{
			string inputDirectory = Path.Combine(tempDirectory, "input");
			Directory.CreateDirectory(inputDirectory);
			string outputFile = Path.Combine(tempDirectory, "ouput", "output.fup");
			PackOptions packOptions = new PackOptions
			{
				InputDirectoryPath = inputDirectory,
				OutputFilePath = outputFile,
				MaxDegreeOfParallelism = 2,
				MaxMemory = 8,
				ShowProgress = false
			};
			PackAction packAction = new PackAction(loggerMock.Object, packOptions)
			{
				ManifestReporterFactory = new Lazy<IManifestReporterFactory>(() => manifestReporterFactoryMock.Object),
				Filter = filterMock.Object,
			};
			await File.WriteAllTextAsync(Path.Combine(inputDirectory, "test.txt"), "test", Encoding.UTF8);
			await File.WriteAllTextAsync(Path.Combine(inputDirectory, "test2.txt"), "test", Encoding.UTF8);
			await File.WriteAllTextAsync(Path.Combine(inputDirectory, "test3.txt"), "test3", Encoding.UTF8);
			await File.WriteAllTextAsync(Path.Combine(inputDirectory, "test4.txt"), "test4", Encoding.UTF8);
			Directory.CreateDirectory(Path.Combine(inputDirectory, "subDir"));
			await File.WriteAllTextAsync(Path.Combine(inputDirectory, "subDir", "test.txt"), "test test test", Encoding.UTF8);

			// act
			int returnCode = await packAction.Run();

			// assert
			returnCode.Should().Be(0);
			manifestReporterMock.VerifyNoOtherCalls();
			filterMock.Verify(x => x.Apply(It.IsAny<IEnumerable<FileSystemInfo>>(), It.IsAny<IFilterOptions>(),
				It.IsAny<char>(), It.IsAny<Func<FileSystemInfo, string>>(), It.IsAny<Func<FileSystemInfo, bool>>()));
			filterMock.VerifyNoOtherCalls();
			manifestReporterFactoryMock.VerifyNoOtherCalls();
		});
	}

	[Test]
	public async Task Ensure_Run_Works_With_DryRun()
	{
		// arrange
		Mock<ILogger> loggerMock = new Mock<ILogger>();
		Mock<IManifestReporterFactory> manifestReporterFactoryMock = new Mock<IManifestReporterFactory>();
		Mock<IArchiveSerializerFactory> archiveSerializerFactoryMock = new Mock<IArchiveSerializerFactory>();
		Mock<IHashProviderFactory> hashProviderFactoryMock = new Mock<IHashProviderFactory>();
		Mock<IHashProvider> hashProviderMock = new Mock<IHashProvider>();
		Mock<IManifestReporter> manifestReporterMock = new Mock<IManifestReporter>();
		Mock<IFilter> filterMock = new Mock<IFilter>();
		manifestReporterFactoryMock.Setup(x => x.GetManifestReporter(It.IsAny<OutputFormat>(), It.IsAny<ILogger>()))
			.Returns(manifestReporterMock.Object);
		filterMock.Setup(x => x.Apply(It.IsAny<IEnumerable<FileSystemInfo>>(), It.IsAny<IFilterOptions>(),
				It.IsAny<char>(), It.IsAny<Func<FileSystemInfo, string>>(), It.IsAny<Func<FileSystemInfo, bool>>()))
			.Returns<IEnumerable<FileSystemInfo>, IFilterOptions, char, Func<FileSystemInfo, string>,
				Func<FileSystemInfo, bool>>((infos, options, arg3, arg4, arg5) =>
			{
				bool test = arg5(infos.First());
				string relativePath = arg4(infos.First());
				return infos.ToList();
			});
		hashProviderFactoryMock.Setup(x => x.GetHashProvider(It.IsAny<HashAlgorithm>())).Returns(hashProviderMock.Object);
		hashProviderMock.Setup(x => x.CalculateHash(It.IsAny<Stream>())).ReturnsAsync("Hash");
		await TestUtil.UseRandomTempDirectory(async tempDirectory =>
		{
			string inputDirectory = Path.Combine(tempDirectory, "input");
			Directory.CreateDirectory(inputDirectory);
			string outputFile = Path.Combine(tempDirectory, "ouput", "output.fup");
			PackOptions packOptions = new PackOptions
			{
				InputDirectoryPath = inputDirectory,
				OutputFilePath = outputFile,
				DryRun = true,
				ShowProgress = false
			};
			PackAction packAction = new PackAction(loggerMock.Object, packOptions)
			{
				ArchiveSerializerFactory = archiveSerializerFactoryMock.Object,
				HashProviderFactory = hashProviderFactoryMock.Object,
				Filter = filterMock.Object,
			};
			await File.WriteAllTextAsync(Path.Combine(inputDirectory, "test.txt"), "test", Encoding.UTF8);
			Directory.CreateDirectory(Path.Combine(inputDirectory, "subDir"));

			// act
			int returnCode1 = await packAction.Run();
			packAction.ManifestReporterFactory = new Lazy<IManifestReporterFactory>(() => manifestReporterFactoryMock.Object);
			int returnCode2 = await packAction.Run();

			// assert
			returnCode1.Should().Be(0);
			returnCode2.Should().Be(0);
			manifestReporterMock.Verify(x => x.PrintReport(It.IsAny<Manifest>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Exactly(1));
			manifestReporterMock.VerifyNoOtherCalls();
			filterMock.Verify(x => x.Apply(It.IsAny<IEnumerable<FileSystemInfo>>(), It.IsAny<IFilterOptions>(),
				It.IsAny<char>(), It.IsAny<Func<FileSystemInfo, string>>(), It.IsAny<Func<FileSystemInfo, bool>>()));
			hashProviderFactoryMock.Verify(x => x.GetHashProvider(It.IsAny<HashAlgorithm>()), Times.Exactly(2));
			hashProviderMock.Verify(x => x.CalculateHash(It.IsAny<Stream>()), Times.Exactly(2));
			filterMock.VerifyNoOtherCalls();
			manifestReporterFactoryMock.VerifyNoOtherCalls();
			archiveSerializerFactoryMock.VerifyNoOtherCalls();
			hashProviderFactoryMock.VerifyNoOtherCalls();
			hashProviderMock.VerifyNoOtherCalls();
		});
	}

	[Test]
	public async Task Ensure_Run_Throws_Exception_For_InputDirectoryPath_Null()
	{
		// arrange
		Mock<ILogger> loggerMock = new Mock<ILogger>();
		PackOptions packOptions = new PackOptions
		{
			InputDirectoryPath = null,
		};

		// act
		Func<Task> func = async () => await new PackAction(loggerMock.Object, packOptions).Run();

		// assert
		await func.Should().ThrowAsync<InvalidOptionException>().WithMessage("Please provide the input directory path.");
	}

	[Test]
	public async Task Ensure_Run_Throws_Exception_For_Not_Existing_InputDirectory()
	{
		// arrange
		Mock<ILogger> loggerMock = new Mock<ILogger>();
		PackOptions packOptions = new PackOptions
		{
			InputDirectoryPath = "1234567890",
		};

		// act
		Func<Task> func = async () => await new PackAction(loggerMock.Object, packOptions).Run();

		// assert
		await func.Should().ThrowAsync<InvalidOptionException>();
	}

	[Test]
	public async Task Ensure_Run_Throws_Exception_For_OutputFilePath_Null()
	{
		// arrange
		Mock<ILogger> loggerMock = new Mock<ILogger>();
		PackOptions packOptions = new PackOptions
		{
			InputDirectoryPath = ".",
			OutputFilePath = null
		};

		// act
		Func<Task> func = async () => await new PackAction(loggerMock.Object, packOptions).Run();

		// assert
		await func.Should().ThrowAsync<InvalidOptionException>().WithMessage("Please provide the output file.");
	}

	[Test]
	public async Task Ensure_Run_Throws_Exception_For_Invalid_IncludeFilters()
	{
		// arrange
		Mock<ILogger> loggerMock = new Mock<ILogger>();
		Mock<ITextMatchProviderFactory> textMatchProviderFactoryMock = new Mock<ITextMatchProviderFactory>();
		Mock<ITextMatchProvider> textMatchProviderMock = new Mock<ITextMatchProvider>();
		textMatchProviderFactoryMock.Setup(x => x.GetProvider(It.IsAny<TextMatchProviderType>())).Returns(textMatchProviderMock.Object);
		textMatchProviderMock.Setup(x => x.IsPatternValid("Valid")).Returns(true);
		textMatchProviderMock.Setup(x => x.IsPatternValid("Invalid")).Returns(false);
		PackOptions packOptions = new PackOptions
		{
			InputDirectoryPath = ".",
			OutputFilePath = "test.fup",
			IncludeFilters = { "Valid", "Invalid" }
		};
		PackAction packAction = new PackAction(loggerMock.Object, packOptions)
		{
			TextMatchProviderFactory = textMatchProviderFactoryMock.Object
		};

		// act
		Func<Task> func = async () => await packAction.Run();

		// assert
		await func.Should().ThrowAsync<InvalidOptionException>().WithMessage("Invalid include filter: Invalid");
	}

	[Test]
	public async Task Ensure_Run_Throws_Exception_For_Invalid_ExcludeFilters()
	{
		// arrange
		Mock<ILogger> loggerMock = new Mock<ILogger>();
		Mock<ITextMatchProviderFactory> textMatchProviderFactoryMock = new Mock<ITextMatchProviderFactory>();
		Mock<ITextMatchProvider> textMatchProviderMock = new Mock<ITextMatchProvider>();
		textMatchProviderFactoryMock.Setup(x => x.GetProvider(It.IsAny<TextMatchProviderType>())).Returns(textMatchProviderMock.Object);
		textMatchProviderMock.Setup(x => x.IsPatternValid("Valid")).Returns(true);
		textMatchProviderMock.Setup(x => x.IsPatternValid("Invalid")).Returns(false);
		PackOptions packOptions = new PackOptions
		{
			InputDirectoryPath = ".",
			OutputFilePath = "test.fup",
			ExcludeFilters = { "Valid", "Invalid" }
		};
		PackAction packAction = new PackAction(loggerMock.Object, packOptions)
		{
			TextMatchProviderFactory = textMatchProviderFactoryMock.Object
		};

		// act
		Func<Task> func = async () => await packAction.Run();

		// assert
		await func.Should().ThrowAsync<InvalidOptionException>().WithMessage("Invalid exclude filter: Invalid");
	}

	[Test]
	public async Task Ensure_Run_Throws_Exception_For_Invalid_CompressionLevel()
	{
		// arrange
		Mock<ILogger> loggerMock = new Mock<ILogger>();
		Mock<ITextMatchProviderFactory> textMatchProviderFactoryMock = new Mock<ITextMatchProviderFactory>();
		Mock<IFileCompressorFactory> fileCompressorFactoryMock = new Mock<IFileCompressorFactory>();
		Mock<ITextMatchProvider> textMatchProviderMock = new Mock<ITextMatchProvider>();
		Mock<IFileCompressor> fileCompressorMock = new Mock<IFileCompressor>();
		textMatchProviderFactoryMock.Setup(x => x.GetProvider(It.IsAny<TextMatchProviderType>())).Returns(textMatchProviderMock.Object);
		textMatchProviderMock.Setup(x => x.IsPatternValid("Valid")).Returns(true);
		fileCompressorFactoryMock.Setup(x => x.GetCompressor(It.IsAny<CompressionAlgorithm>())).Returns(fileCompressorMock.Object);
		fileCompressorMock.Setup(x => x.GetCompressionLevelValuesWithNames()).Returns(new Dictionary<ushort, string> {{0, "MockValue"}});
		PackOptions packOptions = new PackOptions
		{
			InputDirectoryPath = ".",
			OutputFilePath = "test.fup",
			ExcludeFilters = { "Valid" },
			CompressionLevel = 9999,
		};
		PackAction packAction = new PackAction(loggerMock.Object, packOptions)
		{
			TextMatchProviderFactory = textMatchProviderFactoryMock.Object,
			FileCompressorFactory = fileCompressorFactoryMock.Object,
		};

		// act
		Func<Task> func = async () => await packAction.Run();

		// assert
		await func.Should().ThrowAsync<InvalidOptionException>().WithMessage("The provided compression level '9999' is invalid. Valid values are: MockValue (0)");
	}
}