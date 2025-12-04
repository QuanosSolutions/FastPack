using System;
using FastPack.Lib.Hashing;
using AwesomeAssertions;
using NUnit.Framework;

namespace FastPack.Tests.FastPack.Lib.Hashing;

[TestFixture]
public class HashProviderFactoryTests
{
	[Test]
	public void Ensure_GetHashProvider_CreatesXXHash()
	{
		// arrange
		IHashProviderFactory hashProviderFactory = new HashProviderFactory();

		// act
		IHashProvider hashProvider = hashProviderFactory.GetHashProvider(HashAlgorithm.XXHash);

		// assert
		hashProvider.Should().NotBeNull();
		hashProvider.Should().BeOfType<XXHashProvider>();
	}

	[Test]
	public void Ensure_GetHashProvider_ThrowsExceptionForUnknownHashAlgorithm()
	{
		// arrange
		IHashProviderFactory hashProviderFactory = new HashProviderFactory();

		// act
		Action act = () => hashProviderFactory.GetHashProvider((HashAlgorithm)9999);

		// assert
		act.Should().Throw<NotSupportedException>().WithMessage("There is no provider for hash algorithm 9999.");
	}
}