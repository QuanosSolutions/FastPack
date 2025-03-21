using System;
using System.IO;
using System.Threading.Tasks;
using Standart.Hash.xxHash;

namespace FastPack.Lib.Hashing;

// ReSharper disable once InconsistentNaming
public class XXHashProvider : IHashProvider
{
	// Reduces hashing time by ~40% on NTFS NVME disk compared to default of 8KiB
	private const int HashBufferSize = 512 * 1024;
	public async Task<string> CalculateHash(Stream stream)
	{
		return (await xxHash64.ComputeHashAsync(stream, HashBufferSize)).ToString();
	}

	public string HashBytesToString(ReadOnlySpan<byte> bytes)
	{
		return BitConverter.ToUInt64(bytes).ToString();
	}

	public byte[] HashStringToBytes(ReadOnlySpan<char> chars)
	{
		return BitConverter.GetBytes(ulong.Parse(chars));
	}

	public int GetHashSizeInBytes()
	{
		return sizeof(ulong);
	}
}