using System;
using System.IO;
using System.Threading.Tasks;
using FastPack.Lib;
using FastPack.Lib.Logging;
using FastPack.Lib.ManifestManagement;
using FastPack.Lib.ManifestManagement.Serialization;
using FastPack.Lib.Unpackers;
using FastPack.TestFramework;
using AwesomeAssertions;
using Moq;
using NUnit.Framework;

namespace FastPack.Tests.FastPack.Lib.Unpackers;

[TestFixture]
internal class ArchiveUnpackerFactoryTests
{
	[Test]
	public void Ensure_GetUnpacker_Creates_ArchiveUnpackerV1()
	{
		// arrange
		IArchiveUnpackerFactory archiveUnpackerFactory = new ArchiveUnpackerFactory();
		Mock<ILogger> loggerMock = new Mock<ILogger>();

		// act
		IUnpacker unpacker = archiveUnpackerFactory.GetUnpacker(1, loggerMock.Object);

		// assert
		unpacker.Should().NotBeNull();
		unpacker.Should().BeOfType<ArchiveUnpackerV1>();
	}

	[Test]
	public async Task Ensure_GetUnpacker_Creates_ArchiveUnpackerV1_With_Stream()
	{
		// arrange
		IArchiveUnpackerFactory archiveUnpackerFactory = new ArchiveUnpackerFactory();
		Mock<ILogger> loggerMock = new Mock<ILogger>();
		await using MemoryStream memoryStream = new MemoryStream();
		Manifest manifest = new Manifest();
		IArchiveManifestWriter archiveManifestWriter = new BinaryArchiveManifestWriterV1();
		IArchiveFileWriter archiveFileWriter = new BinaryArchiveFileWriterV1(archiveManifestWriter);
		await archiveFileWriter.WriteHeader(memoryStream, manifest);
		await archiveManifestWriter.WriteManifest(memoryStream, manifest);
		memoryStream.Position = 0;

		// act
		IUnpacker unpacker = await archiveUnpackerFactory.GetUnpacker(memoryStream, loggerMock.Object);

		// assert
		unpacker.Should().NotBeNull();
		unpacker.Should().BeOfType<ArchiveUnpackerV1>();
	}

	[Test]
	public async Task Ensure_GetUnpacker_Creates_ArchiveUnpackerV1_With_Path()
	{
		// arrange
		IArchiveUnpackerFactory archiveUnpackerFactory = new ArchiveUnpackerFactory();
		Mock<ILogger> loggerMock = new Mock<ILogger>();
		await TestUtil.UseRandomTempDirectory(async tempDirectory => {
			string testFileName = "test.fup";
			string filePath = Path.Combine(tempDirectory, testFileName);
			await using (FileStream stream = new(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None, Constants.BufferSize, Constants.OpenFileStreamsAsync))
			{
				Manifest manifest = new Manifest();
				IArchiveManifestWriter archiveManifestWriter = new BinaryArchiveManifestWriterV1();
				IArchiveFileWriter archiveFileWriter = new BinaryArchiveFileWriterV1(archiveManifestWriter);
				await archiveFileWriter.WriteHeader(stream, manifest);
				await archiveManifestWriter.WriteManifest(stream, manifest);
			}

			// act
			IUnpacker unpacker = await archiveUnpackerFactory.GetUnpacker(filePath, loggerMock.Object);

			// assert
			unpacker.Should().NotBeNull();
			unpacker.Should().BeOfType<ArchiveUnpackerV1>();
		});
	}

	[Test]
	public async Task Ensure_GetUnpacker_Throws_Exception_When_Stream_Is_Corrupt()
	{
		// arrange
		IArchiveUnpackerFactory archiveUnpackerFactory = new ArchiveUnpackerFactory();
		Mock<ILogger> loggerMock = new Mock<ILogger>();
		await using MemoryStream memoryStream = new MemoryStream();
		await memoryStream.WriteAsync(new byte[] {0x00, 0x00}, 0, 2);
		memoryStream.Position = 0;

		// act
		Func<Task> func = async () => await archiveUnpackerFactory.GetUnpacker(memoryStream, loggerMock.Object);

		// assert
		await func.Should().ThrowAsync<NotSupportedException>().WithMessage("Stream does not contain valid FastPack data.");
	}

	[Test]
	public async Task Ensure_GetUnpacker_Throws_Exception_When_Signature_Is_Corrupt()
	{
		// arrange
		IArchiveUnpackerFactory archiveUnpackerFactory = new ArchiveUnpackerFactory();
		Mock<ILogger> loggerMock = new Mock<ILogger>();
		await using MemoryStream memoryStream = new MemoryStream();
		await memoryStream.WriteAsync(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, 0, 12);
		memoryStream.Position = 0;

		// act
		Func<Task> func = async () => await archiveUnpackerFactory.GetUnpacker(memoryStream, loggerMock.Object);

		// assert
		await func.Should().ThrowAsync<NotSupportedException>().WithMessage("Stream does not contain valid FastPack data.");
	}

	[Test]
	public async Task Ensure_GetUnpacker_Throws_Exception_When_Path_Is_Null()
	{
		// arrange
		IArchiveUnpackerFactory archiveUnpackerFactory = new ArchiveUnpackerFactory();
		Mock<ILogger> loggerMock = new Mock<ILogger>();
		string path = null;

		// act
		Func<Task> func = async () => await archiveUnpackerFactory.GetUnpacker(path, loggerMock.Object);

		// assert
		await func.Should().ThrowAsync<ArgumentNullException>().WithMessage("Value cannot be null. (Parameter 'filePath')");
	}

	[Test]
	public void Ensure_GetUnpacker_ThrowsExceptionForUnknownVersion()
	{
		// arrange
		IArchiveUnpackerFactory archiveUnpackerFactory = new ArchiveUnpackerFactory();
		Mock<ILogger> loggerMock = new Mock<ILogger>();

		// act
		Action act = () => archiveUnpackerFactory.GetUnpacker(9999, loggerMock.Object);

		// assert
		act.Should().Throw<NotSupportedException>().WithMessage("There is no unpacker for version 9999.");
	}
}