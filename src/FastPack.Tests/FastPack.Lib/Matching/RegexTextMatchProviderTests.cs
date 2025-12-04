using System.Collections.Generic;
using FastPack.Lib.Matching;
using AwesomeAssertions;
using NUnit.Framework;

namespace FastPack.Tests.FastPack.Lib.Matching;

[TestFixture]
public class RegexTextMatchProviderTests
{
	[Test]
	public void Test_IsPatternValid_With_Valid_Pattern()
	{
		new RegexTextMatchProvider().IsPatternValid(".*").Should().BeTrue();
	}

	[Test]
	public void Test_IsPatternValid_With_Invalid_Pattern()
	{
		new RegexTextMatchProvider().IsPatternValid("[").Should().BeFalse();
		new RegexTextMatchProvider().IsPatternValid(@"\").Should().BeFalse();
	}

	[Test, TestCaseSource(nameof(GetTestCaseData))]
	public void Test_IsMatch(MatchTestData testData)
	{
		new RegexTextMatchProvider().IsMatch(testData.Text, testData.Pattern, testData.CaseSensitive).Should().Be(testData.ExpectedResult);
	}
	
	private static IEnumerable<MatchTestData> GetTestCaseData()
	{
		yield return new MatchTestData("this is a simple text", "simple", true, true);
		yield return new MatchTestData("this is a simple text", "SimPle", false, true);
		yield return new MatchTestData("this is a simple text", @"(\w+\s*)+", false, true);
	}
}