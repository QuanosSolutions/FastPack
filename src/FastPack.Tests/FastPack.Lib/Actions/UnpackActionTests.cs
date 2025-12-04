using System;
using System.IO;
using System.Threading.Tasks;
using AwesomeAssertions;
using FastPack.Lib;
using FastPack.Lib.Actions;
using FastPack.Lib.Hashing;
using FastPack.Lib.Logging;
using FastPack.Lib.Matching;
using FastPack.Lib.Options;
using FastPack.Lib.Unpackers;
using FastPack.TestFramework;
using FastPack.TestFramework.Common;
using Moq;
using NUnit.Framework;

namespace FastPack.Tests.FastPack.Lib.Actions
{
	[TestFixture]
	public class UnpackActionTests
	{
		private const string FluentAssertionsFileHash = "16481563206961488444";

		[Test]
		public async Task UnpackPredefinedFupPackage()
		{
			await TestUtil.UseRandomTempDirectory(async outputDirectory => {
				// we have a directory where to extract data. This gets deleted afterwards
				await TestUtil.UseFileWithRandomNameFromTestData("IntegrationData/example.fup", async fupFilePath => {
					// embedded file is extracted to the temp folder

					UnpackAction unpackAction = new(new TextWriterLogger(TestContext.Out), new UnpackOptions { InputFilePath = fupFilePath, OutputDirectoryPath = outputDirectory });
					await unpackAction.Run();

					string extractedFilePath = Path.Combine(outputDirectory, "FluentAssertions.dll");
					File.Exists(extractedFilePath).Should().BeTrue();

					string fileHash = await CryptoUtil.CalculateFileHash(new XXHashProvider(), extractedFilePath);
					fileHash.Should().Be(FluentAssertionsFileHash);
				});
			});
		}

		[Test]
		public async Task UnpackPredefinedFupPackageRepackAndUnpackAgain()
		{
			await TestUtil.UseRandomTempDirectory(async sourceDirectory => {
				await TestUtil.UseFileWithRandomNameFromTestData("IntegrationData/example.fup", async fupFilePath => {
					UnpackAction unpackAction = new(new TextWriterLogger(TestContext.Out), new UnpackOptions { InputFilePath = fupFilePath, OutputDirectoryPath = sourceDirectory });
					await unpackAction.Run();

					// FluentAssertions.dll is now extracted, pack it again
					await TestUtil.UseRandomTempDirectory(async newFupFileDirectory => {
						string newFupFilePath = Path.Combine(newFupFileDirectory, "example.fup");
						await new PackAction(new TextWriterLogger(TestContext.Out), new PackOptions { InputDirectoryPath = sourceDirectory, OutputFilePath = newFupFilePath }).Run();

						// FluentAssertions.dll is now packed, extract it again
						await TestUtil.UseRandomTempDirectory(async targetDirectory => {
							UnpackAction unpackAction2 = new(new TextWriterLogger(TestContext.Out), new UnpackOptions { InputFilePath = newFupFilePath, OutputDirectoryPath = targetDirectory });
							await unpackAction2.Run();

							string extractedFilePath = Path.Combine(targetDirectory, "FluentAssertions.dll");
							File.Exists(extractedFilePath).Should().BeTrue();

							string fileHash = await CryptoUtil.CalculateFileHash(new XXHashProvider(), extractedFilePath);
							fileHash.Should().Be(FluentAssertionsFileHash);
						});
					});
				});
			});
		}

		[Test]
		public void Null_Logger_In_Constructor_Throws()
		{
			Func<UnpackAction> construction = () => new UnpackAction(null, new UnpackOptions());

			construction.Should().Throw<ArgumentNullException>().Where(e => e.ParamName.Equals("logger"));
		}

		[Test]
		public void Null_Options_In_Constructor_Throws()
		{
			Func<UnpackAction> construction = () => new UnpackAction(Mock.Of<ILogger>(), null);

			construction.Should().Throw<ArgumentNullException>().Where(e => e.ParamName.Equals("options"));
		}

		[Test]
		public async Task Null_OutputDirectoryPath_Throws_Exceptions()
		{
			// arrange
			UnpackOptions options = new() {OutputDirectoryPath = null};
			UnpackAction action = new(Mock.Of<ILogger>(), options);
			Func<Task<int>> actionRun = () => action.Run();

			// act + assert
			await actionRun.Should().ThrowAsync<InvalidOptionException>()
				.Where(e => e.ExceptionCode == ErrorConstants.Unpack_OutputDirectoryPath_Missing)
				.Where(e => e.PropertyName.Equals(nameof(options.OutputDirectoryPath)))
				.Where(e => e.Message.Equals("Please provide the output directory path."));
		}


		[Test]
		public async Task Ensure_OutputDirectoryPath_Is_Made_Absolute()
		{
			// arrange
			UnpackOptions options = new() { OutputDirectoryPath = "relative" };
			UnpackAction action = new(Mock.Of<ILogger>(), options);
			Path.IsPathRooted(options.OutputDirectoryPath).Should().BeFalse();

			try
			{
				await action.Run();
			}
			catch
			{
				Path.IsPathRooted(options.OutputDirectoryPath).Should().BeTrue();
			}
		}

		[Test]
		public async Task Null_InputFilePath_Throws_Exceptions()
		{
			// arrange
			UnpackOptions options = new() {
				OutputDirectoryPath = ".",
				InputFilePath = null
			};
			UnpackAction action = new(Mock.Of<ILogger>(), options);
			Func<Task<int>> actionRun = () => action.Run();

			// act + assert
			await actionRun.Should().ThrowAsync<InvalidOptionException>()
				.Where(e => e.ExceptionCode == ErrorConstants.Unpack_InputFilePath_Missing)
				.Where(e => e.PropertyName.Equals(nameof(options.InputFilePath)))
				.Where(e => e.Message.Equals("Please provide the input file."));
		}


		[Test]
		public async Task Ensure_InputFilePath_Is_Made_Absolute()
		{
			using FastPackTestContext testContext = FastPackTestContext.Create(true);

			// arrange
			UnpackOptions options = new() {
				OutputDirectoryPath = ".",
				InputFilePath = "relativePath"
			};
			UnpackAction action = new(Mock.Of<ILogger>(), options);
			Path.IsPathRooted(options.InputFilePath).Should().BeFalse();

			try
			{
				await action.Run();
			}
			catch 
			{
				Path.IsPathRooted(options.InputFilePath).Should().BeTrue();
			}
		}

		[Test]
		public async Task Not_Existing_InputFilePath_Throws_Exceptions()
		{
			// arrange
			UnpackOptions options = new() {
				OutputDirectoryPath = ".",
				InputFilePath = "missing_file"
			};
			UnpackAction action = new(Mock.Of<ILogger>(), options);
			Func<Task<int>> actionRun = () => action.Run();

			// act + assert
			await actionRun.Should().ThrowAsync<InvalidOptionException>()
				.Where(e => e.ExceptionCode == ErrorConstants.Unpack_InputFilePath_NoFound)
				.Where(e => e.PropertyName.Equals(nameof(options.InputFilePath)))
				.Where(e => e.Message.StartsWith("File not found: ", StringComparison.InvariantCulture));
		}

		[Test]
		public async Task Ensure_MaxDegreeOfParallelism_Is_Not_Changed_If_Not_Null()
		{
			// arrange
			UnpackOptions options = new() {
				MaxDegreeOfParallelism = 1,
			};
			UnpackAction action = new(Mock.Of<ILogger>(), options);

			try
			{
				await action.Run();
			}
			catch
			{
				options.MaxDegreeOfParallelism.Should().Be(1);
			}
		}

		[Test]
		public async Task Ensure_MaxDegreeOfParallelism_Is_Set_To_ProcessorCount_If_Null()
		{
			// arrange
			UnpackOptions options = new() {
				MaxDegreeOfParallelism = null,
			};
			UnpackAction action = new(Mock.Of<ILogger>(), options);

			try
			{
				await action.Run();
			}
			catch
			{
				options.MaxDegreeOfParallelism.Should().Be(Environment.ProcessorCount);
			}
		}
		
		[Test]
		public async Task Ensure_MaxMemory_Is_Set_To_NonNull_If_Null()
		{
			// arrange
			UnpackOptions options = new() {
				MaxDegreeOfParallelism = null,
			};
			UnpackAction action = new(Mock.Of<ILogger>(), options);

			try
			{
				await action.Run();
			}
			catch
			{
				options.MaxDegreeOfParallelism.Should().NotBeNull();
			}
		}

		[Test]
		public async Task Ensure_Valid_Filters_Do_Not_Throw()
		{
			// arrange
			using FastPackTestContext testContext = FastPackTestContext.Create(true);
			string inputFilePath = testContext.GetTempFileName(".fup", true, true, true);
			
			UnpackOptions options = new() {
				OutputDirectoryPath = ".",
				InputFilePath = inputFilePath,
				IncludeFilters = { "ValidInclude" },
				ExcludeFilters = { "ValidExclude" }
			};

			Mock<ILogger> loggerMock = new();
			Mock<IUnpacker> unpackerMock = new();

			Mock<IArchiveUnpackerFactory> unpackerFactoryMock = new();
			unpackerFactoryMock.Setup(f => f.GetUnpacker(It.IsAny<string>(), loggerMock.Object)).ReturnsAsync(unpackerMock.Object);

			Mock<ITextMatchProvider> textMatchProviderMock = new();
			textMatchProviderMock.Setup(p => p.IsPatternValid(It.IsAny<string>())).Returns(true);

			Mock <ITextMatchProviderFactory> textMatchProviderFactoryMock = new();
			textMatchProviderFactoryMock.Setup(f => f.GetProvider(It.IsAny<TextMatchProviderType>())).Returns(textMatchProviderMock.Object);
			
			UnpackAction action = new(loggerMock.Object, options) {
				ArchiveUnpackerFactory = unpackerFactoryMock.Object,
				TextMatchProviderFactory = textMatchProviderFactoryMock.Object
			};

			Func<Task<int>> actionRun = () => action.Run();

			// act + assert
			await actionRun.Should().NotThrowAsync();

			textMatchProviderMock.Verify(p => p.IsPatternValid("ValidInclude"), Times.Once);
			textMatchProviderMock.Verify(p => p.IsPatternValid("ValidExclude"), Times.Once);

			options.IncludeFilters.Should().BeEquivalentTo("ValidInclude");
			options.ExcludeFilters.Should().BeEquivalentTo("ValidExclude");
		}

		[Test]
		public async Task Ensure_Invalid_IncludeFilters_Does_Throw()
		{
			// arrange
			using FastPackTestContext testContext = FastPackTestContext.Create(true);
			string inputFilePath = testContext.GetTempFileName(".fup", true, true, true);

			UnpackOptions options = new() {
				OutputDirectoryPath = ".",
				InputFilePath = inputFilePath,
				IncludeFilters = { "InvalidInclude" },
			};

			Mock<ILogger> loggerMock = new();
			Mock<IUnpacker> unpackerMock = new();

			Mock<IArchiveUnpackerFactory> unpackerFactoryMock = new();
			unpackerFactoryMock.Setup(f => f.GetUnpacker(It.IsAny<string>(), loggerMock.Object)).ReturnsAsync(unpackerMock.Object);

			Mock<ITextMatchProvider> textMatchProviderMock = new();
			textMatchProviderMock.Setup(p => p.IsPatternValid(It.IsAny<string>())).Returns(false);

			Mock<ITextMatchProviderFactory> textMatchProviderFactoryMock = new();
			textMatchProviderFactoryMock.Setup(f => f.GetProvider(It.IsAny<TextMatchProviderType>())).Returns(textMatchProviderMock.Object);

			UnpackAction action = new(loggerMock.Object, options) {
				ArchiveUnpackerFactory = unpackerFactoryMock.Object,
				TextMatchProviderFactory = textMatchProviderFactoryMock.Object
			};

			Func<Task<int>> actionRun = () => action.Run();

			// act + assert
			await actionRun.Should().ThrowAsync<InvalidOptionException>()
				.Where(e => e.ExceptionCode == ErrorConstants.Unpack_Invalid_IncludeFilter)
				.Where(e => e.PropertyName.Equals(nameof(options.IncludeFilters)))
				.Where(e => e.Message.StartsWith("Invalid include filter:", StringComparison.InvariantCulture));

		}

		[Test]
		public async Task Ensure_Invalid_ExcludeFilters_Does_Throw()
		{
			// arrange
			using FastPackTestContext testContext = FastPackTestContext.Create(true);
			string inputFilePath = testContext.GetTempFileName(".fup", true, true, true);

			UnpackOptions options = new() {
				OutputDirectoryPath = ".",
				InputFilePath = inputFilePath,
				ExcludeFilters = { "InvalidExclude" }
			};

			Mock<ILogger> loggerMock = new();
			Mock<IUnpacker> unpackerMock = new();

			Mock<IArchiveUnpackerFactory> unpackerFactoryMock = new();
			unpackerFactoryMock.Setup(f => f.GetUnpacker(It.IsAny<string>(), loggerMock.Object)).ReturnsAsync(unpackerMock.Object);

			Mock<ITextMatchProvider> textMatchProviderMock = new();
			textMatchProviderMock.Setup(p => p.IsPatternValid(It.IsAny<string>())).Returns(false);

			Mock<ITextMatchProviderFactory> textMatchProviderFactoryMock = new();
			textMatchProviderFactoryMock.Setup(f => f.GetProvider(It.IsAny<TextMatchProviderType>())).Returns(textMatchProviderMock.Object);

			UnpackAction action = new(loggerMock.Object, options) {
				ArchiveUnpackerFactory = unpackerFactoryMock.Object,
				TextMatchProviderFactory = textMatchProviderFactoryMock.Object
			};

			Func<Task<int>> actionRun = () => action.Run();

			// act + assert
			await actionRun.Should().ThrowAsync<InvalidOptionException>()
				.Where(e => e.ExceptionCode == ErrorConstants.Unpack_Invalid_ExcludeFilter)
				.Where(e => e.PropertyName.Equals(nameof(options.ExcludeFilters)))
				.Where(e => e.Message.StartsWith("Invalid exclude filter:", StringComparison.InvariantCulture));
		}

		[Test]
		public async Task Test_Run()
		{
			// arrange
			const int runExitCode = 0;
			using FastPackTestContext testContext = FastPackTestContext.Create(true);
			string inputFilePath = testContext.GetTempFileName(".fup", true, true, true);

			UnpackOptions options = new() {
				OutputDirectoryPath = ".",
				InputFilePath = inputFilePath,
			};

			Mock<ILogger> loggerMock = new();
			Mock<IUnpacker> unpackerMock = new();
			unpackerMock.Setup(u => u.Extract(inputFilePath, options)).ReturnsAsync(runExitCode);

			Mock<IArchiveUnpackerFactory> unpackerFactoryMock = new();
			unpackerFactoryMock.Setup(f => f.GetUnpacker(inputFilePath, loggerMock.Object)).ReturnsAsync(unpackerMock.Object);
			
			UnpackAction action = new(loggerMock.Object, options) {
				ArchiveUnpackerFactory = unpackerFactoryMock.Object,
			};

			int exitCode = await action.Run();
			exitCode.Should().Be(exitCode);

			unpackerFactoryMock.Verify(f => f.GetUnpacker(inputFilePath, loggerMock.Object), Times.Once);
			unpackerMock.Verify(u => u.Extract(inputFilePath, options), Times.Once);
		}
	}
}
