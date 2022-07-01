using System;

namespace FastPack.Lib.ManifestManagement.Serialization;

public class ArchiveSerializerFactory : IArchiveSerializerFactory
{
	public IArchiveFileReader GetFileReader(ushort version)
	{
		return version switch {
			1 => new BinaryArchiveFileReaderV1(),
			_ => throw new NotSupportedException($"There is no archive file reader for version {version}")
		};
	}

	public IArchiveFileWriter GetFileWriter(ushort version)
	{
		return version switch {
			1 => new BinaryArchiveFileWriterV1(GetManifestWriter(version)),
			_ => throw new NotSupportedException($"There is no archive file writer for version {version}")
		};
	}

	public IArchiveManifestWriter GetManifestWriter(ushort version)
	{
		return version switch {
			1 => new BinaryArchiveManifestWriterV1(),
			_ => throw new NotSupportedException($"There is no archive manifest writer for version {version}")
		};
	}
}