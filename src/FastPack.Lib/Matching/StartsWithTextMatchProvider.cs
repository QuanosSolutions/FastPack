using System;

namespace FastPack.Lib.Matching;

public class StartsWithTextMatchProvider : ITextMatchProvider
{
	public bool IsMatch(string text, string pattern, bool caseSensitive)
	{
		return text.StartsWith(pattern, caseSensitive ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase);
	}

	public bool IsPatternValid(string pattern)
	{
		return true;
	}
}