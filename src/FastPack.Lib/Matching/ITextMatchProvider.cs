using System.Collections.Generic;

namespace FastPack.Lib.Matching;

internal interface ITextMatchProvider
{
	bool IsMatch(string text, string pattern, bool caseSensitive);
	bool IsPatternValid(string pattern);
	IEnumerable<string> NormalizePathFilters(IEnumerable<string> paths, char desiredPathSeparator);
}