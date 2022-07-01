using System;
using FastPack.Lib.ManifestManagement;

namespace FastPack.Lib.Compression;

internal class FileCompressorFactory : IFileCompressorFactory
{
	public IFileCompressor GetCompressor(CompressionAlgorithm algorithm)
	{
		return algorithm switch {
			CompressionAlgorithm.Deflate => new DeflateFileCompressor(),
			CompressionAlgorithm.NoCompression => new NoCompressionFileCompressor(),
			_ => throw new NotSupportedException($"Compressor type is not supported: {algorithm}")
		};
	}
}