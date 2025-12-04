using System;
using System.IO;
using System.Threading.Tasks;
using FastPack.Lib.ManifestManagement.Serialization;
using AwesomeAssertions;
using NUnit.Framework;

namespace FastPack.Tests.FastPack.Lib.ManifestManagement.Serialization;

[TestFixture]
internal class BinaryArchiveFileReaderV1Tests
{
	[Test]
	public async Task Ensure_ReadManifest_ThrowsExceptionForInvalidSignature()
	{
		// arrange
		IArchiveFileReader archiveFileReader = new BinaryArchiveFileReaderV1();
		await using MemoryStream memoryStream = new MemoryStream();
		await memoryStream.WriteAsync(new byte[]{ 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, 0, 10);
		memoryStream.Position = 0;

		// act
		Func<Task> func = async () => await archiveFileReader.ReadManifest(memoryStream);

		// assert
		await func.Should().ThrowAsync<InvalidDataException>().WithMessage("File is not a valid fup file");
	}
}