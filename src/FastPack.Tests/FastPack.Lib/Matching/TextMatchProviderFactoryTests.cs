using System;
using System.Collections.Generic;
using FastPack.Lib.Matching;
using AwesomeAssertions;
using NUnit.Framework;

namespace FastPack.Tests.FastPack.Lib.Matching;

[TestFixture]
public class TextMatchProviderFactoryTests
{
	[Test]
	public void EnsureCorrectTypes()
	{
		TextMatchProviderFactory factory = new();
		Dictionary<TextMatchProviderType, Type> typeMapping = new() {
			{TextMatchProviderType.Glob, typeof(GlobTextMatchProvider)},
			{TextMatchProviderType.Regex, typeof(RegexTextMatchProvider)},
			{TextMatchProviderType.StartsWith, typeof(StartsWithTextMatchProvider)},
		};

		foreach (TextMatchProviderType providerType in Enum.GetValues<TextMatchProviderType>())
		{
			factory.GetProvider(providerType).Should().BeOfType(typeMapping[providerType]);
		}
	}

	[Test]
	public void Ensure_GetProvider_ThrowsExceptionForUnknownTextMatchProviderType()
	{
		// arrange
		TextMatchProviderFactory factory = new();

		// act
		Action act = () => factory.GetProvider((TextMatchProviderType)9999);

		// assert
		act.Should().Throw<NotSupportedException>().WithMessage("Text match provider is not supported: 9999");
	}
}