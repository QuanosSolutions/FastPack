using System;
using System.IO;
using System.Threading.Tasks;

namespace FastPack.Lib.Hashing;

public interface IHashProvider
{
	public Task<string> CalculateHash(Stream stream);
	public string HashBytesToString(ReadOnlySpan<byte> bytes);
	public byte[] HashStringToBytes(ReadOnlySpan<char> chars);
	public int GetHashSizeInBytes();
}