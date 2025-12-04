using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AwesomeAssertions;
using FastPack.Lib.Logging;
using FastPack.Options.Parsers;
using Moq;
using NUnit.Framework;

namespace FastPack.Tests.FastPack.Options.Parsers
{
	[TestFixture]
	public class ArgumentsParserTests
	{
		[Test]
		public async Task Empty_Arguments_Are_No_Problem_For_Parse()
		{
			ArgumentsParser argumentsParser = CreateParser(new string[] { }, false);
			bool result = await argumentsParser.Parse(new Dictionary<string, Func<string, Task<bool>>>());

			result.Should().BeTrue();
		}

		[Test]
		public async Task Missing_Mapping_Is_No_Problem_For_Parse_In_NonStrict_Mode()
		{
			ArgumentsParser argumentsParser = CreateParser(new string[] { "KEY_WITH_NO_MAPPING" }, false);
			bool result = await argumentsParser.Parse(new Dictionary<string, Func<string, Task<bool>>>());

			result.Should().BeTrue();
		}

		[Test]
		public async Task Missing_Mapping_Returns_False_In_Strict_Mode()
		{
			Mock<ILogger> loggerMock = new();
			ArgumentsParser argumentsParser = CreateParser(new[] { "KEY_WITH_NO_MAPPING" }, true, loggerMock.Object);

			bool result = await argumentsParser.Parse(new Dictionary<string, Func<string, Task<bool>>>());
			result.Should().BeFalse();

			loggerMock.Verify(l => l.ErrorLine("Unknown parameter: KEY_WITH_NO_MAPPING"));
		}

		[Test]
		public async Task All_Parameters_Are_Processed()
		{
			HashSet<string> args = new HashSet<string> {"a", "b", "c"};
			ArgumentsParser argumentsParser = CreateParser(args.ToArray(), false);

			Dictionary<string, Func<string, Task<bool>>> parameterProcessingMap = new Dictionary<string, Func<string, Task<bool>>> {
				{"a", p => Task.FromResult(args.Remove(p)) },
				{"b", p => Task.FromResult(args.Remove(p)) },
				{"c", p => Task.FromResult(args.Remove(p)) }
			};

			bool result = await argumentsParser.Parse(parameterProcessingMap);

			result.Should().BeTrue();
			args.Should().BeEmpty("all parameters should be removed");
		}

		private static ArgumentsParser CreateParser(string[] args, bool strictMode, ILogger logger = null)
		{
			return new ArgumentsParser(args, logger ?? Mock.Of<ILogger>(), strictMode);
		}
	}
}
