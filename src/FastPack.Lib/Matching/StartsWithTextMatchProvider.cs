using System;
using System.Collections.Generic;

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

	public IEnumerable<string> NormalizePathFilters(IEnumerable<string> paths, char desiredPathSeparator)
	{
		foreach (string path in paths)
			yield return path.Replace('/', desiredPathSeparator).Replace('\\', desiredPathSeparator);
	}
}