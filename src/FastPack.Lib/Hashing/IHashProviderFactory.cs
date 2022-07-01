namespace FastPack.Lib.Hashing;

public interface IHashProviderFactory
{
	IHashProvider GetHashProvider(HashAlgorithm algorithm);
}