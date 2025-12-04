using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FastPack.CmdLine;
using FastPack.Lib.Actions;
using FastPack.Lib.Logging;
using FastPack.Options;
using FastPack.Options.Parsers;
using FastPack.TestFramework.Common;
using AwesomeAssertions;
using Moq;
using NUnit.Framework;

namespace FastPack.Tests.FastPack.Options.Parsers
{
	[TestFixture]
	public class FastPackActionOptionsTests
	{
		[Test]
		public async Task Test_Empty_Args()
		{
			FastPackActionOptionsParser parser = new(Mock.Of<ILogger>());
			FastPackActionOptions options = await parser.CreateFromArgs(Array.Empty<string>());

			options.Action.Should().Be(ActionType.Pack);
			options.HelpRequested.Should().BeFalse();
			options.IsStrictParamsMode.Should().BeTrue();
			options.StrippedArgs.Should().BeEmpty();
		}

		[Test]
		public async Task Test_Loose_Params_Mode()
		{
			FastPackActionOptionsParser parser = new(Mock.Of<ILogger>());
			FastPackActionOptions[] options = {await parser.CreateFromArgs(new []{"-lp"}), await parser.CreateFromArgs(new[] { "--looseparams" }) };

			foreach (FastPackActionOptions option in options)
			{
				option.IsStrictParamsMode.Should().BeFalse();

				option.Action.Should().Be(ActionType.Pack);
				option.HelpRequested.Should().BeFalse();
				option.StrippedArgs.Should().BeEmpty();
			}
		}

		[Test]
		public async Task Test_Version_As_Single_Argument()
		{
			FastPackActionOptionsParser parser = new(Mock.Of<ILogger>());
			FastPackActionOptions options = await parser.CreateFromArgs(new []{"--version"});
			options.Action.Should().Be(ActionType.Version);
			options.StrippedArgs.Should().BeEmpty();
		}

		[Test]
		public async Task Test_Version_With_Multiple_Arguments()
		{
			FastPackActionOptionsParser parser = new(Mock.Of<ILogger>());
			FastPackActionOptions options = await parser.CreateFromArgs(new[] { "-a", "pack", "--version" });
			options.Action.Should().Be(ActionType.Pack);
			options.StrippedArgs.Should().BeEquivalentTo("--version");
		}

		[Test]
		public async Task Test_Help_Requested()
		{
			FastPackActionOptionsParser parser = new(Mock.Of<ILogger>());
			FastPackActionOptions[] options = {
				await parser.CreateFromArgs(new[] { "-h" }),
				await parser.CreateFromArgs(new[] { "--help" }),
				await parser.CreateFromArgs(new[] { "-?" }),
				await parser.CreateFromArgs(new[] { "/?" }),
			};

			foreach (FastPackActionOptions option in options)
			{
				option.Action.Should().Be(ActionType.Help);
				option.HelpRequested.Should().BeTrue();
				option.StrippedArgs.Should().BeEmpty();
			}
		}

		[Test]
		public async Task Test_Help_Requested_WithAction()
		{
			FastPackActionOptionsParser parser = new(Mock.Of<ILogger>());
			FastPackActionOptions[] options = {
				await parser.CreateFromArgs(new[] { "-h", "-a", "about" }),
				await parser.CreateFromArgs(new[] { "-a", "about", "--help" }),
				await parser.CreateFromArgs(new[] { "-?", "--action", "about" }),
				await parser.CreateFromArgs(new[] { "--action", "about", "/?" }),
			};

			foreach (FastPackActionOptions option in options)
			{
				option.Action.Should().Be(ActionType.About);
				option.HelpRequested.Should().BeTrue();
				option.StrippedArgs.Should().BeEmpty();
			}
		}

		[Test]
		public async Task Missing_Action_Parameter_Results_In_Help()
		{
			Mock<ILogger> loggerMock = new();
			FastPackActionOptionsParser parser = new(loggerMock.Object);
			FastPackActionOptions options = await parser.CreateFromArgs(new[] {"-a"});

			options.Action.Should().Be(ActionType.Help);
			options.HelpRequested.Should().BeFalse();

			loggerMock.Verify(l => l.ErrorLine(It.IsAny<string>()));
		}

		[Test]
		public async Task Wrong_Action_Parameter_Results_In_Help()
		{
			Mock<ILogger> loggerMock = new();
			FastPackActionOptionsParser parser = new(loggerMock.Object);
			FastPackActionOptions options = await parser.CreateFromArgs(new[] { "-a", "ThisActionDoesNotExist" });

			options.Action.Should().Be(ActionType.Help);
			options.HelpRequested.Should().BeFalse();

			loggerMock.Verify(l => l.ErrorLine(It.IsAny<string>()));
		}

		[Test]
		public async Task Test_AllActions()
		{
			FastPackActionOptionsParser parser = new(Mock.Of<ILogger>());
			foreach (ActionType actionType in Enum.GetValues<ActionType>())
			{
				FastPackActionOptions options = await parser.CreateFromArgs(new[] {"-a", actionType.ToString()});
				options.Action.Should().Be(actionType);
			}
		}

		[Test]
		public async Task Empty_File_Parameter_Results_In_Help()
		{
			Mock<ILogger> loggerMock = new();
			FastPackActionOptionsParser parser = new(loggerMock.Object);
			FastPackActionOptions options = await parser.CreateFromArgs(new[] { "@" });

			options.Action.Should().Be(ActionType.Help);
			options.HelpRequested.Should().BeFalse();
			
			loggerMock.Verify(l => l.ErrorLine(It.IsRegex("file is missing", RegexOptions.IgnoreCase)));
		}

		[Test]
		public async Task Not_Existing_File_For_File_Parameter_Results_In_Help()
		{
			Mock<ILogger> loggerMock = new();
			FastPackActionOptionsParser parser = new(loggerMock.Object);
			FastPackActionOptions options = await parser.CreateFromArgs(new[] { "@ThisFileDoesNotExist" });

			options.Action.Should().Be(ActionType.Help);
			options.HelpRequested.Should().BeFalse();

			loggerMock.Verify(l => l.ErrorLine(It.IsRegex("does not exist", RegexOptions.IgnoreCase)));
		}

		[Test]
		public async Task Test_FileParameters()
		{
			using FastPackTestContext testContext = FastPackTestContext.Create(true);
			string parameterFilePath = testContext.GetTempFileName(".json", true, true, true);

			string[] argumentsInFile = new[] {"-a", "unpack"};
			Mock<IFileToCommandLineConverter> fileToCommandLineConverter = new();
			fileToCommandLineConverter.Setup(c => c.ConvertToArgs(Path.GetFullPath(parameterFilePath))).ReturnsAsync(argumentsInFile);
			Mock<IFileToCommandLineConverterFactory> fileToCommandLineConverterFactoryMock = new();
			fileToCommandLineConverterFactoryMock.Setup(f => f.GetConverter(It.IsAny<string>())).Returns(fileToCommandLineConverter.Object);

			FastPackActionOptionsParser parser = new(Mock.Of<ILogger>()) {FileToCommandLineConverterFactory = fileToCommandLineConverterFactoryMock.Object};
			FastPackActionOptions options = await parser.CreateFromArgs(new[] {"-i", "input", $"@{parameterFilePath}" , "-o", "output"});

			options.Action.Should().Be(ActionType.Unpack);
			options.StrippedArgs.Should().BeEquivalentTo("-i", "input", "-o", "output");
		}
	}
}
