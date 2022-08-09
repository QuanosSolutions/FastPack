using System.Collections.Generic;
using DotNet.Globbing;

namespace FastPack.Lib.Matching;

class GlobTextMatchProvider : ITextMatchProvider
{
	public bool IsMatch(string text, string pattern, bool caseSensitive)
	{
		return Glob.Parse(pattern, new GlobOptions { Evaluation = new EvaluationOptions { CaseInsensitive = !caseSensitive} }).IsMatch(text);
	}

	public bool IsPatternValid(string pattern)
	{
		try
		{
			Glob.Parse(pattern);
			return true;
		}
		catch
		{
			return false;
		}
	}

	public IEnumerable<string> NormalizePathFilters(IEnumerable<string> paths, char desiredPathSeparator)
	{
		// we dont have to do anything here. The glob implementation is smart enough to support slashes and backslashes
		return paths;
	}
}