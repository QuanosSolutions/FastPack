using System.IO;
using System.Threading.Tasks;
using FastPack.Lib.ManifestManagement;
using FastPack.Lib.Options;

namespace FastPack.Lib.Unpackers;

public interface IUnpacker
{
	public Task<int> Extract(string inputFile, UnpackOptions options);
	public Task<Manifest> GetManifestFromStream(Stream inputStream);
}