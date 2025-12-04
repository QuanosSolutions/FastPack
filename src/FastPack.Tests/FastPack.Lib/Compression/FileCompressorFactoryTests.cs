using System;
using FastPack.Lib.Compression;
using FastPack.Lib.ManifestManagement;
using AwesomeAssertions;
using NUnit.Framework;

namespace FastPack.Tests.FastPack.Lib.Compression;

[TestFixture]
public class FileCompressorFactoryTests
{
	[Test]
	public void Ensure_GetCompressor_Creates_DeflateFileCompressor()
	{
		// arrange
		IFileCompressorFactory fileCompressorFactory = new FileCompressorFactory();

		// act
		IFileCompressor fileCompressor = fileCompressorFactory.GetCompressor(CompressionAlgorithm.Deflate);

		// assert
		fileCompressor.Should().NotBeNull();
		fileCompressor.Should().BeOfType<DeflateFileCompressor>();
	}

	[Test]
	public void Ensure_GetCompressor_Creates_NoCompressionFileCompressor()
	{
		// arrange
		IFileCompressorFactory fileCompressorFactory = new FileCompressorFactory();

		// act
		IFileCompressor fileCompressor = fileCompressorFactory.GetCompressor(CompressionAlgorithm.NoCompression);

		// assert
		fileCompressor.Should().NotBeNull();
		fileCompressor.Should().BeOfType<NoCompressionFileCompressor>();
	}

	[Test]
	public void Ensure_GetCompressor_ThrowsExceptionForUnknownHashAlgorithm()
	{
		// arrange
		IFileCompressorFactory fileCompressorFactory = new FileCompressorFactory();

		// act
		Action act = () => fileCompressorFactory.GetCompressor((CompressionAlgorithm)9999);

		// assert
		act.Should().Throw<NotSupportedException>().WithMessage("Compressor type is not supported: 9999");
	}
}