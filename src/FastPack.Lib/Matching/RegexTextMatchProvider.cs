using System.Text.RegularExpressions;

namespace FastPack.Lib.Matching;

class RegexTextMatchProvider : ITextMatchProvider
{
	private const RegexOptions DefaultOptions = RegexOptions.Singleline | RegexOptions.CultureInvariant;
	public bool IsMatch(string text, string pattern, bool caseSensitive)
	{
		return Regex.IsMatch(text, pattern, caseSensitive ? DefaultOptions : DefaultOptions | RegexOptions.IgnoreCase);
	}

	public bool IsPatternValid(string pattern)
	{
		try
		{
			// ReSharper disable once ObjectCreationAsStatement
			new Regex(pattern);
			return true;
		}
		catch
		{
			return false;
		}
	}
}