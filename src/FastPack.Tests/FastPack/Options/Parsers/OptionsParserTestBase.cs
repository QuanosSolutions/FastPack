using System;
using System.Linq;
using System.Threading.Tasks;
using FastPack.Lib.Logging;
using FastPack.Lib.Options;
using FastPack.Options.Parsers;
using AwesomeAssertions;
using Moq;
using NUnit.Framework;

namespace FastPack.Tests.FastPack.Options.Parsers
{
	public abstract class OptionsParserTestBase<TParser, TOptions>
		where TParser : IOptionsParser
		where TOptions: IOptions
	{

		[Test, TestCaseSource("GetTestCaseData", new object[] { true })]
		public async Task TestParseStrictMode(OptionsParserTestCaseData<TOptions> testData)
		{
			ILogger logger = Mock.Of<ILogger>();
			IOptions options = await GetParser(logger).CreateFromArgs(testData.Args, testData.StrictMode, logger);

			options.Should().NotBeNull("parsing should have worked");
			options.Should().BeEquivalentTo(testData.ExpectedOptions);
		}

		[Test, TestCaseSource("GetTestCaseData", new object[] { false })]
		public async Task TestParseNonStrictMode(OptionsParserTestCaseData<TOptions> testData)
		{
			ILogger logger = Mock.Of<ILogger>();
			string[] notExistingArgs = { "-DoesNotExist", "--ThisAlsoNot", "AndIamNot2" };
			IOptions options = await GetParser(logger).CreateFromArgs(notExistingArgs.Concat(testData.Args).Concat(notExistingArgs).ToArray(), testData.StrictMode, logger);

			options.Should().NotBeNull("parsing should have worked");
			options.Should().BeEquivalentTo(testData.ExpectedOptions);
		}

		

		[Test]
		public async Task TestPrintHelp()
		{
			Mock<ILogger> loggerMock = new();
			await GetParser(loggerMock.Object).PrintHelp();
			loggerMock.Verify(x => x.InfoLine(It.IsAny<string>()));
			loggerMock.VerifyNoOtherCalls();
		}

		protected TParser GetParser(ILogger logger)
		{
			return (TParser)Activator.CreateInstance(typeof(TParser), logger);
		}
	}
}
