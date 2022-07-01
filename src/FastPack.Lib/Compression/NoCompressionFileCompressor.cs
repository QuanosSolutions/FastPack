using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FastPack.Lib.Compression;

public class NoCompressionFileCompressor : IFileCompressor
{
	public async Task DecompressFile(Stream sourceStream, Stream targetStream)
	{
		await sourceStream.CopyToAsync(targetStream);
	}

	public async Task<byte[]> DecompressFile(Stream sourceStream)
	{
		MemoryStream memoryStream = new((int)sourceStream.Length);
		await sourceStream.CopyToAsync(memoryStream);

		byte[] bytes = memoryStream.GetBuffer();
		Array.Resize(ref bytes, (int)memoryStream.Length); // this is benchmarked in MemoryStreamGetBytesComparison
		return bytes;
	}

	public async Task<Stream> CompressFile(string inputDirectory, string inputFile, ushort compressionLevel, Stream targetStream = null)
	{
		bool inMemory = targetStream == null;
		targetStream ??= new MemoryStream();

		await using (Stream inputStream = new FileStream(Path.Combine(inputDirectory, inputFile), FileMode.Open, FileAccess.Read, FileShare.Read, Constants.BufferSize, Constants.OpenFileStreamsAsync))
			await inputStream.CopyToAsync(targetStream);

		return inMemory ? targetStream : null;
	}

	public ushort GetDefaultCompressionLevel()
	{
		return 0;
	}

	public IDictionary<ushort, string> GetCompressionLevelValuesWithNames()
	{
		return new Dictionary<ushort, string> {{0, "NoCompression"}};
	}
}