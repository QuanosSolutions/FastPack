namespace FastPack.Lib.Matching;

internal interface ITextMatchProviderFactory
{
	ITextMatchProvider GetProvider(TextMatchProviderType type);
}