using System;
using System.IO;
using System.Threading.Tasks;

namespace FastPack.Lib.ManifestManagement.Serialization;

public interface IArchiveFileWriter
{
	Task WriteHeader(Stream outputStream, Manifest manifest);
	Task WriteFileData(Stream outputStream, ManifestEntry manifestEntry, Func<Stream, Task> writeStreamAction);
	Task WriteManifest(Stream outputStream, Manifest manifest);
}