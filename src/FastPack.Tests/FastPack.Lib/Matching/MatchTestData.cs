namespace FastPack.Tests.FastPack.Lib.Matching;

public class MatchTestData
{
	public string Text { get; set; }
	public string Pattern { get; set; }
	public bool CaseSensitive { get; set; }
	public bool ExpectedResult { get; set; }

	public MatchTestData(string text, string pattern, bool caseSensitive, bool expectedResult)
	{
		Text = text;
		Pattern = pattern;
		CaseSensitive = caseSensitive;
		ExpectedResult = expectedResult;
	}

	public override string ToString()
	{
		return $"[IgnoreCase: {!CaseSensitive}] [Pattern: {Pattern}] [Text: {Text}]";
	}
}