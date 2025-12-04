using System.Collections.Generic;
using FastPack.Lib.Matching;
using AwesomeAssertions;
using NUnit.Framework;

namespace FastPack.Tests.FastPack.Lib.Matching
{
	[TestFixture]
	public class StartsWithTextMatchProviderTests
	{
		[Test]
		public void Test_IsPatternValid_With_Valid_Pattern()
		{
			new StartsWithTextMatchProvider().IsPatternValid("everything is valid").Should().BeTrue();
		}

		[Test, TestCaseSource(nameof(GetMatchingTestCaseData))]
		public void Test_IsMatch(MatchTestData testData)
		{
			new StartsWithTextMatchProvider().IsMatch(testData.Text, testData.Pattern, testData.CaseSensitive).Should().Be(testData.ExpectedResult);
		}

		private static IEnumerable<MatchTestData> GetMatchingTestCaseData()
		{
			yield return new MatchTestData("this is a simple text", "T", false, true);
			yield return new MatchTestData("this is a simple text", "t", true, true);
			yield return new MatchTestData("this is a simple text", "this", true, true);
			yield return new MatchTestData("this is a simple text", "this is a simple text", true, true);
			yield return new MatchTestData("this is a simple text", "This", false, true);
			yield return new MatchTestData("this is a simple text", "ThiS Is A SimplE TexT", false, true);
			yield return new MatchTestData("this is a simple text", " this", false, false);
			yield return new MatchTestData("this is a simple text", "", false, true);
			yield return new MatchTestData("this is a simple text", " ", false, false);
			yield return new MatchTestData("this is a simple text", "this is a simple text", false, true);
		}
	}
}
