using System.Threading.Tasks;
using FastPack.Lib.ManifestManagement;

namespace FastPack.Lib.ManifestReporting;

internal interface IManifestReporter
{
	Task PrintReport(Manifest manifest, bool showCompressedSize, bool showDetailed, bool prettyPrint);
}