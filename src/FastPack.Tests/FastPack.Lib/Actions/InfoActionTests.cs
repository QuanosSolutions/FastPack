using System;
using System.IO;
using System.Threading.Tasks;
using FastPack.Lib;
using FastPack.Lib.Actions;
using FastPack.Lib.Logging;
using FastPack.Lib.ManifestManagement;
using FastPack.Lib.ManifestReporting;
using FastPack.Lib.Options;
using FastPack.Lib.Unpackers;
using FastPack.TestFramework.Common;
using AwesomeAssertions;
using Moq;
using NUnit.Framework;

namespace FastPack.Tests.FastPack.Lib.Actions
{
	[TestFixture]
	public class InfoActionTests
	{
		[SetUp]
		public void Setup()
		{
			
		}

		[Test]
		public async Task Exception_Is_Thrown_For_Null_InputFilePath()
		{
			InfoOptions infoOptions = new();
			InfoAction infoAction = new(Mock.Of<ILogger>(), infoOptions);

			Func<Task<int>> runMethod = () => infoAction.Run();
			await runMethod.Should()
				.ThrowAsync<InvalidOptionException>()
				.Where(e => nameof(infoOptions.InputFilePath).Equals(e.PropertyName))
				.Where(e => e.ExceptionCode.Equals(ErrorConstants.Diff_InputFilePath_Missing));
		}

		[Test]
		public async Task Exception_Is_Thrown_For_InputFilePath_Not_Found()
		{
			string inputFileName = Guid.NewGuid().ToString("N");
			string inputFilePath = Path.GetFullPath(inputFileName);
			InfoOptions infoOptions = new() { InputFilePath = inputFileName };
			InfoAction infoAction = new(Mock.Of<ILogger>(), infoOptions);

			Func<Task<int>> runMethod = () => infoAction.Run();
			await runMethod.Should()
				.ThrowAsync<InvalidOptionException>()
				.Where(e => nameof(infoOptions.InputFilePath).Equals(e.PropertyName))
				.Where(e => e.ExceptionCode.Equals(ErrorConstants.Diff_InputFilePath_NoFound))
				.Where(e => e.Message.Contains(inputFilePath));
		}

		[Test]
		public async Task Run_With_Default_Options()
		{
			await RunWithOptions(new InfoOptions());
		}

		[Test]
		public async Task Run_With_Detailed()
		{
			await RunWithOptions(new InfoOptions {Detailed = true});
		}

		[Test]
		public async Task Run_With_Detailed_False()
		{
			await RunWithOptions(new InfoOptions { Detailed = false });
		}

		[Test]
		public async Task Run_With_PrettyPrint()
		{
			await RunWithOptions(new InfoOptions { PrettyPrint = true });
		}

		[Test]
		public async Task Run_With_PrettyPrint_False()
		{
			await RunWithOptions(new InfoOptions { PrettyPrint = false });
		}

		[Test]
		public async Task Run_With_OutputFormat_Json()
		{
			await RunWithOptions(new InfoOptions { OutputFormat = OutputFormat.Json});
		}

		[Test]
		public async Task Run_With_OutputFormat_Xml()
		{
			await RunWithOptions(new InfoOptions { OutputFormat = OutputFormat.Xml });
		}

		[Test]
		public async Task Run_With_OutputFormat_Text()
		{
			await RunWithOptions(new InfoOptions { OutputFormat = OutputFormat.Text });
		}

		private async Task RunWithOptions(InfoOptions options)
		{
			ILogger logger = Mock.Of<ILogger>();
			using FastPackTestContext testContext = FastPackTestContext.Create(true);
			string fupFilePath = testContext.GetTempFileName(".fup", true, true, true);
			options.InputFilePath = fupFilePath;
			Manifest expectedManifest = new();

			Mock<IUnpacker> unpackerMock = new();
			unpackerMock.Setup(u => u.GetManifestFromStream(It.IsAny<Stream>()))
				.ReturnsAsync(expectedManifest);

			Mock<IArchiveUnpackerFactory> archiveUnpackerFactoryMock = new();
			archiveUnpackerFactoryMock.Setup(f => f.GetUnpacker(It.IsAny<Stream>(), logger))
				.ReturnsAsync(unpackerMock.Object);

			Mock<IManifestReporter> manifestReporterMock = new();

			Mock<IManifestReporterFactory> manifestReporterFactoryMock = new();
			manifestReporterFactoryMock.Setup(f => f.GetManifestReporter(options.OutputFormat, logger))
				.Returns(manifestReporterMock.Object);

			InfoAction infoAction = new(logger, options) {
				ArchiveUnpackerFactory = archiveUnpackerFactoryMock.Object,
				ManifestReporterFactory = manifestReporterFactoryMock.Object
			};

			int runResult = await infoAction.Run();

			runResult.Should().Be(0);

			// verify method calls
			unpackerMock.Verify(u => u.GetManifestFromStream(It.IsAny<Stream>()), Times.Once);
			manifestReporterMock.Verify(mr => mr.PrintReport(expectedManifest, true, options.Detailed, options.PrettyPrint), Times.Once);
			archiveUnpackerFactoryMock.Verify(f => f.GetUnpacker(It.IsAny<Stream>(), logger), Times.Once);
			manifestReporterFactoryMock.Verify(f => f.GetManifestReporter(options.OutputFormat, logger), Times.Once);

			// verify no other methods were called
			unpackerMock.VerifyNoOtherCalls();
			archiveUnpackerFactoryMock.VerifyNoOtherCalls();
			manifestReporterFactoryMock.VerifyNoOtherCalls();
			manifestReporterMock.VerifyNoOtherCalls();
		}
	}
}
