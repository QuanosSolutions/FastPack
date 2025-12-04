using System.Collections.Generic;
using FastPack.Lib.Matching;
using AwesomeAssertions;
using NUnit.Framework;

namespace FastPack.Tests.FastPack.Lib.Matching;

[TestFixture]
public class GlobTextMatchProviderTests
{
	[Test]
	public void Test_IsPatternValid_With_Valid_Pattern()
	{
		new GlobTextMatchProvider().IsPatternValid("dll/**").Should().BeTrue();
	}

	[Test]
	public void Test_IsPatternValid_With_Invalid_Pattern()
	{
		new GlobTextMatchProvider().IsPatternValid(null).Should().BeFalse();
		new GlobTextMatchProvider().IsPatternValid(string.Empty).Should().BeFalse();
	}

	[Test, TestCaseSource(nameof(GetTestCaseData))]
	public void Test_IsMatch(MatchTestData testData)
	{
		new GlobTextMatchProvider().IsMatch(testData.Text, testData.Pattern, testData.CaseSensitive).Should().Be(testData.ExpectedResult);
	}
	
	private static IEnumerable<MatchTestData> GetTestCaseData()
	{
		yield return new MatchTestData("this is a simple text", "*simple*", true, true);
		yield return new MatchTestData("this is a simple text", "*SimPle*", false, true);
		yield return new MatchTestData("b.dll", "**/*.dll", false, true);
		yield return new MatchTestData("a/b.dll", "**/*.dll", false, true);
		yield return new MatchTestData("a/b/c.dll", "**/b/c.dll", false, true);
		yield return new MatchTestData("a/b/c.dll", "**/b/*.dll", false, true);
		yield return new MatchTestData("a/b/c.dll", "**/b/*.*", false, true);
		yield return new MatchTestData("a/b/c.dll", "**/b/**/*.dll", false, true);
		yield return new MatchTestData("a/a2/b/b2/c.dll", "**/b/**/*.dll", false, true);
		yield return new MatchTestData("a/b.dll", "*.dll", false, false);
		yield return new MatchTestData("f*ck/it", "f?ck/**", false, true);
		yield return new MatchTestData("f*ck/it", "f?ck/it", false, true);
		yield return new MatchTestData("xyz", "[xyz][xyz][xyz]", false, true);
		yield return new MatchTestData("xyz", "[yz][xyz][xyz]", false, false);
		yield return new MatchTestData("xyz", "[xyz][xz][xyz]", false, false);
		yield return new MatchTestData("xyz", "[xyz][xyz][xy]", false, false);
		yield return new MatchTestData("xyz", "*[!xy]", false, true);
		yield return new MatchTestData("xyz", "[!yz]*", false, true);
		yield return new MatchTestData("xyz", "*[!a-y]", false, true);
		yield return new MatchTestData("xyz", "*[a-y]", false, false);
	}
}