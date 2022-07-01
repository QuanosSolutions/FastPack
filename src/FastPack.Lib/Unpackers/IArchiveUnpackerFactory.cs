using System.IO;
using System.Threading.Tasks;
using FastPack.Lib.Logging;

namespace FastPack.Lib.Unpackers;

public interface IArchiveUnpackerFactory
{
	Task<IUnpacker> GetUnpacker(string filePath, ILogger logger);
	Task<IUnpacker> GetUnpacker(Stream stream, ILogger logger);
	IUnpacker GetUnpacker(ushort version, ILogger logger);
}