using System.Collections.Generic;
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

	public IEnumerable<string> NormalizePathFilters(IEnumerable<string> paths, char desiredPathSeparator)
	{
		string regexEscapedBackSlash = Regex.Escape(@"\");
		foreach (string path in paths)
		{
			yield return desiredPathSeparator switch {
				// replace forward slashes with regex escaped backslashes if the desired path separator is a backslash
				'\\' => path.Replace("/", regexEscapedBackSlash),
				// replace regex escaped backslashes with forward slashes if the desired path separator is a forward slash
				'/' => path.Replace(regexEscapedBackSlash, "/"),
				_ => path
			};
		}
	}
}