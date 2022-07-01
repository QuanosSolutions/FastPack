using System.IO;
using System.Threading.Tasks;
using FastPack.Lib.Hashing;

namespace FastPack.Lib;

public static class CryptoUtil
{
	public static async Task<string> CalculateFileHash(IHashProvider hashProvider, string filePath)
	{
		// Here we explicitly set useAsync to false, as it is faster this way.
		await using FileStream fileStream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, Constants.BufferSize, false);
		return await hashProvider.CalculateHash(fileStream);
	}
}