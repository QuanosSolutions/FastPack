using System.IO;
using System.Threading.Tasks;

namespace FastPack.Lib.ManifestManagement.Serialization;

public interface IArchiveFileReader
{
	Task<Manifest> ReadManifest(Stream inputStream);
}