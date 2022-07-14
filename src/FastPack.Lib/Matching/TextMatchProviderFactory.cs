using System;

namespace FastPack.Lib.Matching;

internal class TextMatchProviderFactory : ITextMatchProviderFactory
{
	public ITextMatchProvider GetProvider(TextMatchProviderType type)
	{
		return type switch {
			TextMatchProviderType.Glob => new GlobTextMatchProvider(),
			TextMatchProviderType.Regex => new RegexTextMatchProvider(),
			TextMatchProviderType.StartsWith => new StartsWithTextMatchProvider(),
			_ => throw new NotSupportedException($"Text match provider is not supported: {type}")
		};
	}
}