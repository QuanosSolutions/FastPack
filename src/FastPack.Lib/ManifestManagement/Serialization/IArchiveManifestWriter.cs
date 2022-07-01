using System.IO;
using System.Threading.Tasks;

namespace FastPack.Lib.ManifestManagement.Serialization;

public interface IArchiveManifestWriter
{
	Task WriteManifest(Stream outputStream, Manifest manifest);
}