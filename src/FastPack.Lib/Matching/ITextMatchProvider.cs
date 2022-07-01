namespace FastPack.Lib.Matching;

internal interface ITextMatchProvider
{
	bool IsMatch(string text, string pattern, bool caseSensitive);
	bool IsPatternValid(string pattern);
}