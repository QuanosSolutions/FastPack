using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using FastPack.Lib.Compression;
using FastPack.TestFramework;
using AwesomeAssertions;
using NUnit.Framework;

namespace FastPack.Tests.FastPack.Lib.Compression;

[TestFixture]
public class DeflateFileCompressorTests
{
	[Test]
	public void Ensure_GetDefaultCompressionLevel_Works()
	{
		// arrange
		IFileCompressor fileCompressor = new DeflateFileCompressor();

		// act
		ushort compressionLevel = fileCompressor.GetDefaultCompressionLevel();

		// assert
		compressionLevel.Should().Be((ushort)CompressionLevel.Optimal);
	}

	[Test]
	public void Ensure_GetCompressionLevelValuesWithNames_Works()
	{
		// arrange
		IFileCompressor fileCompressor = new DeflateFileCompressor();

		// act
		IDictionary<ushort, string> compressionLevelValuesWithNames = fileCompressor.GetCompressionLevelValuesWithNames();

		// assert
		foreach (CompressionLevel compressionLevel in Enum.GetValues(typeof(CompressionLevel)))
		{
			compressionLevelValuesWithNames.Should().ContainKey((ushort)compressionLevel);
			compressionLevelValuesWithNames[(ushort)compressionLevel].Should().Be(compressionLevel.ToString());
		}
	}

	[Test]
	public async Task Ensure_CompressFile_Works_In_Memory()
	{
		// arrange
		IFileCompressor fileCompressor = new DeflateFileCompressor();

		await TestUtil.UseRandomTempDirectory(async tempDirectory =>
		{
			string testFileName = "test.txt";
			string fileToCompress = Path.Combine(tempDirectory, testFileName);
			string stringToCompress = "test test test test test test test test test test";
			await File.WriteAllTextAsync(fileToCompress, stringToCompress);

			// act
			Stream compressedStream = await fileCompressor.CompressFile(tempDirectory, testFileName, (ushort)CompressionLevel.Optimal);

			// assert
			compressedStream.Should().NotBeNull();
			compressedStream.Should().BeOfType<MemoryStream>();
			compressedStream.Length.Should().BeLessThan(stringToCompress.Length);
		});
	}

	[Test]
	public async Task Ensure_CompressFile_Works_With_Target_Stream()
	{
		// arrange
		IFileCompressor fileCompressor = new DeflateFileCompressor();

		await TestUtil.UseRandomTempDirectory(async tempDirectory =>
		{
			string testFileName = "test.txt";
			string fileToCompress = Path.Combine(tempDirectory, testFileName);
			string stringToCompress = "test test test test test test test test test test";
			await File.WriteAllTextAsync(fileToCompress, stringToCompress);
			await using MemoryStream memoryStream = new MemoryStream();

			// act
			Stream compressedStream = await fileCompressor.CompressFile(tempDirectory, testFileName, (ushort)CompressionLevel.Optimal, memoryStream);

			// assert
			compressedStream.Should().BeNull();
			memoryStream.Length.Should().BeGreaterThan(0);
			memoryStream.Length.Should().BeLessThan(stringToCompress.Length);
		});
	}

	[Test]
	public async Task Ensure_DecompressFile_Without_Target_Stream_Works()
	{
		// arrange
		IFileCompressor fileCompressor = new DeflateFileCompressor();
		string stringToCompress = "test test test test test test test test test test";
		byte[] stringAsBytes = System.Text.Encoding.UTF8.GetBytes(stringToCompress);
		await using MemoryStream initialMemoryStream = new MemoryStream();
		initialMemoryStream.Write(stringAsBytes, 0, stringAsBytes.Length);
		initialMemoryStream.Position = 0;
		await using MemoryStream compressedStream = new MemoryStream();
		await using (DeflateStream deflateStream = new(compressedStream, CompressionLevel.Optimal, true))
		{
			await initialMemoryStream.CopyToAsync(deflateStream);
		}
		compressedStream.Position = 0;

		// act
		byte[] decompressedBytes = await fileCompressor.DecompressFile(compressedStream);

		// assert
		decompressedBytes.Should().NotBeNull();
		decompressedBytes.Length.Should().Be(stringAsBytes.Length);
		System.Text.Encoding.UTF8.GetString(decompressedBytes).Should().Be(stringToCompress);
	}

	[Test]
	public async Task Ensure_DecompressFile_With_Target_Stream_Works()
	{
		// arrange
		IFileCompressor fileCompressor = new DeflateFileCompressor();
		string stringToCompress = "test test test test test test test test test test";
		byte[] stringAsBytes = System.Text.Encoding.UTF8.GetBytes(stringToCompress);
		await using MemoryStream initialMemoryStream = new MemoryStream();
		initialMemoryStream.Write(stringAsBytes, 0, stringAsBytes.Length);
		initialMemoryStream.Position = 0;
		await using MemoryStream compressedStream = new MemoryStream();
		await using (DeflateStream deflateStream = new(compressedStream, CompressionLevel.Optimal, true))
		{
			await initialMemoryStream.CopyToAsync(deflateStream);
		}
		compressedStream.Position = 0;
		await using MemoryStream targetStream = new MemoryStream();

		// act
		await fileCompressor.DecompressFile(compressedStream, targetStream);

		// assert
		targetStream.Position = 0;
		targetStream.Length.Should().Be(stringAsBytes.Length);
		System.Text.Encoding.UTF8.GetString(targetStream.GetBuffer(), 0, (int)targetStream.Length).Should().Be(stringToCompress);
	}
}