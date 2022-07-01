using System.Threading.Tasks;
using FastPack.Lib.Compression;
using FastPack.Lib.Logging;
using FastPack.Lib.ManifestManagement;
using FastPack.Lib.TypeExtensions;

namespace FastPack.Lib.ManifestReporting;

internal class XmlManifestReporter: IManifestReporter
{
	private ILogger Logger { get; }
	private IFileCompressorFactory FileCompressorFactory { get; } = new FileCompressorFactory();

	public XmlManifestReporter(ILogger logger)
	{
		Logger = logger;
	}

	public async Task PrintReport(Manifest manifest, bool showCompressedSize, bool showDetailed, bool prettyPrint)
	{
		Logger.SetQuiet(false);
		ManifestReport manifestReport = manifest.CreateManifestReport(FileCompressorFactory, showCompressedSize, showDetailed);
		await Logger.Info(manifestReport.SerializeToXml(prettyPrint));
		Logger.SetQuiet();
	}
}