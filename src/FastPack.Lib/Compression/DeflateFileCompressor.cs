using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace FastPack.Lib.Compression;

public class DeflateFileCompressor : IFileCompressor
{
	public async Task DecompressFile(Stream sourceStream, Stream targetStream)
	{
		await using Stream stream = new DeflateStream(sourceStream, CompressionMode.Decompress);
		await stream.CopyToAsync(targetStream);
	}

	public async Task<byte[]> DecompressFile(Stream sourceStream)
	{
		await using Stream stream = new DeflateStream(sourceStream, CompressionMode.Decompress);
		MemoryStream memoryStream = new((int)sourceStream.Length);
		await stream.CopyToAsync(memoryStream);

		byte[] bytes = memoryStream.GetBuffer();
		Array.Resize(ref bytes, (int)memoryStream.Length); // this is benchmarked in MemoryStreamGetBytesComparison
		return bytes;
	}

	public async Task<Stream> CompressFile(string inputDirectory, string inputFile, ushort compressionLevel, Stream targetStream = null)
	{
		bool inMemory = targetStream == null;
		targetStream ??= new MemoryStream();

		await using (DeflateStream deflateStream = new(targetStream, (CompressionLevel) compressionLevel, true))
		{
			await using (Stream inputStream = new FileStream(Path.Combine(inputDirectory, inputFile), FileMode.Open, FileAccess.Read, FileShare.Read, Constants.BufferSize, Constants.OpenFileStreamsAsync))
			{
				await inputStream.CopyToAsync(deflateStream);
			}
		}

		return inMemory ? targetStream : null;
	}

	public ushort GetDefaultCompressionLevel()
	{
		return (ushort) CompressionLevel.Optimal;
	}

	public IDictionary<ushort, string> GetCompressionLevelValuesWithNames()
	{
		Dictionary<ushort, string> result = new();

		foreach (CompressionLevel compressionLevel in Enum.GetValues(typeof(CompressionLevel)))
			result[(ushort) compressionLevel] = compressionLevel.ToString();

		return result;
	}
}