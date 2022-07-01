using System.Collections.Generic;
using FastPack.Options;
using FastPack.Options.Parsers;

namespace FastPack.Tests.FastPack.Options.Parsers;

internal class HelpOptionsParserTests : OptionsParserTestBase<HelpOptionsParser, EmptyOptions>
{
	// ReSharper disable once UnusedMember.Local
	// Use by base class through reflection
	private static IEnumerable<OptionsParserTestCaseData<EmptyOptions>> GetTestCaseData(bool strictMode)
	{
		yield break;
	}
}