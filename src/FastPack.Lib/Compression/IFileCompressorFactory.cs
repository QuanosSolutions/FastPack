using FastPack.Lib.ManifestManagement;

namespace FastPack.Lib.Compression;

internal interface IFileCompressorFactory
{
	IFileCompressor GetCompressor(CompressionAlgorithm algorithm);
}