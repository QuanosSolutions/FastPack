namespace FastPack.Lib.ManifestManagement.Serialization;

public interface IArchiveSerializerFactory
{
	IArchiveFileReader GetFileReader(ushort version);
	IArchiveFileWriter GetFileWriter(ushort version);
	IArchiveManifestWriter GetManifestWriter(ushort version);
}