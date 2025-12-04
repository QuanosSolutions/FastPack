using System;
using FastPack.Lib.ManifestManagement.Serialization;
using AwesomeAssertions;
using NUnit.Framework;

namespace FastPack.Tests.FastPack.Lib.ManifestManagement.Serialization;

[TestFixture]
internal class ArchiveSerializerFactoryTests
{
	[Test]
	public void Ensure_GetFileReader_Creates_Version_1()
	{
		// arrange
		IArchiveSerializerFactory archiveSerializerFactory = new ArchiveSerializerFactory();

		// act
		IArchiveFileReader archiveFileReader = archiveSerializerFactory.GetFileReader(1);

		// assert
		archiveFileReader.Should().NotBeNull();
		archiveFileReader.Should().BeOfType<BinaryArchiveFileReaderV1>();
	}

	[Test]
	public void Ensure_GetFileWriter_Creates_Version_1()
	{
		// arrange
		IArchiveSerializerFactory archiveSerializerFactory = new ArchiveSerializerFactory();

		// act
		IArchiveFileWriter archiveFileWriter = archiveSerializerFactory.GetFileWriter(1);

		// assert
		archiveFileWriter.Should().NotBeNull();
		archiveFileWriter.Should().BeOfType<BinaryArchiveFileWriterV1>();
	}

	[Test]
	public void Ensure_GetManifestWriter_Creates_Version_1()
	{
		// arrange
		IArchiveSerializerFactory archiveSerializerFactory = new ArchiveSerializerFactory();

		// act
		IArchiveManifestWriter archiveManifestWriter = archiveSerializerFactory.GetManifestWriter(1);

		// assert
		archiveManifestWriter.Should().NotBeNull();
		archiveManifestWriter.Should().BeOfType<BinaryArchiveManifestWriterV1>();
	}

	[Test]
	public void Ensure_GetFileReader_ThrowsExceptionForUnknownVersion()
	{
		// arrange
		IArchiveSerializerFactory archiveSerializerFactory = new ArchiveSerializerFactory();

		// act
		Action act = () => archiveSerializerFactory.GetFileReader(9999);

		// assert
		act.Should().Throw<NotSupportedException>().WithMessage("There is no archive file reader for version 9999");
	}

	[Test]
	public void Ensure_GetFileWriter_ThrowsExceptionForUnknownVersion()
	{
		// arrange
		IArchiveSerializerFactory archiveSerializerFactory = new ArchiveSerializerFactory();

		// act
		Action act = () => archiveSerializerFactory.GetFileWriter(9999);

		// assert
		act.Should().Throw<NotSupportedException>().WithMessage("There is no archive file writer for version 9999");
	}

	[Test]
	public void Ensure_GetManifestWriter_ThrowsExceptionForUnknownVersion()
	{
		// arrange
		IArchiveSerializerFactory archiveSerializerFactory = new ArchiveSerializerFactory();

		// act
		Action act = () => archiveSerializerFactory.GetManifestWriter(9999);

		// assert
		act.Should().Throw<NotSupportedException>().WithMessage("There is no archive manifest writer for version 9999");
	}
}