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
}