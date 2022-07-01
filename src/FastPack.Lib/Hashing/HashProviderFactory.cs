using System;

namespace FastPack.Lib.Hashing;

public class HashProviderFactory : IHashProviderFactory
{
	public IHashProvider GetHashProvider(HashAlgorithm algorithm)
	{
		return algorithm switch {
			HashAlgorithm.XXHash => new XXHashProvider(),
			_ => throw new NotSupportedException($"There is no provider for hash algorithm {algorithm}.")
		};
	}
}