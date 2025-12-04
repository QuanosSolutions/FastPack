using System.IO;
using System.Threading.Tasks;
using FastPack.Lib.ManifestManagement;
using FastPack.Lib.ManifestManagement.Serialization;
using AwesomeAssertions;
using Moq;
using NUnit.Framework;

namespace FastPack.Tests.FastPack.Lib.ManifestManagement.Serialization;

[TestFixture]
internal class BinaryArchiveFileWriterV1Tests
{
	[Test]
	public async Task Ensure_WriteHeader_Works()
	{
		// arrange
		Mock<IArchiveManifestWriter> archiveManifestWriterMock = new Mock<IArchiveManifestWriter>();
		IArchiveFileWriter archiveFileWriter = new BinaryArchiveFileWriterV1(archiveManifestWriterMock.Object);
		await using MemoryStream memoryStream = new MemoryStream();
		Manifest manifest = new Manifest { Version = 1 };

		// act
		await archiveFileWriter.WriteHeader(memoryStream, manifest);

		// assert
		memoryStream.Position = 0;
		byte[] buffer = memoryStream.GetBuffer();
		memoryStream.Length.Should().Be(18);
		buffer[0].Should().Be(0x46);
		buffer[1].Should().Be(0x41);
		buffer[2].Should().Be(0x53);
		buffer[3].Should().Be(0x54);
		buffer[4].Should().Be(0x50);
		buffer[5].Should().Be(0x41);
		buffer[6].Should().Be(0x43);
		buffer[7].Should().Be(0x4B);
		buffer[8].Should().Be(0x01);
		buffer[9].Should().Be(0x00);
		buffer[10].Should().Be(0x00);
		buffer[11].Should().Be(0x00);
		buffer[12].Should().Be(0x00);
		buffer[13].Should().Be(0x00);
		buffer[15].Should().Be(0x00);
		buffer[16].Should().Be(0x00);
		buffer[17].Should().Be(0x00);
		archiveManifestWriterMock.VerifyNoOtherCalls();
	}

	[Test]
	public async Task Ensure_WriteFileData_Works()
	{
		// arrange
		Mock<IArchiveManifestWriter> archiveManifestWriterMock = new Mock<IArchiveManifestWriter>();
		IArchiveFileWriter archiveFileWriter = new BinaryArchiveFileWriterV1(archiveManifestWriterMock.Object);
		string stringToWrite = "test";
		byte[] stringAsBytes = System.Text.Encoding.UTF8.GetBytes(stringToWrite);
		await using MemoryStream memoryStream = new MemoryStream();
		memoryStream.Write(stringAsBytes, 0, stringAsBytes.Length);
		memoryStream.Position = 0;
		ManifestEntry manifestEntry = new ManifestEntry();

		// act
		await archiveFileWriter.WriteFileData(memoryStream, manifestEntry, stream =>
		{
			stream.Position = 4;
			return Task.CompletedTask;
		});

		// assert
		manifestEntry.DataIndex.Should().Be(0);
		manifestEntry.DataSize.Should().Be(4);
		archiveManifestWriterMock.VerifyNoOtherCalls();
	}

	[Test]
	public async Task Ensure_WriteManifest_Works()
	{
		// arrange
		Mock<IArchiveManifestWriter> archiveManifestWriterMock = new Mock<IArchiveManifestWriter>();
		IArchiveFileWriter archiveFileWriter = new BinaryArchiveFileWriterV1(archiveManifestWriterMock.Object);
		Manifest manifest = new Manifest { Version = 1 };
		await using MemoryStream memoryStream = new MemoryStream();

		// act
		await archiveFileWriter.WriteManifest(memoryStream, manifest);

		// assert
		archiveManifestWriterMock.Verify(x => x.WriteManifest(memoryStream, manifest));
		archiveManifestWriterMock.VerifyNoOtherCalls();
	}
}