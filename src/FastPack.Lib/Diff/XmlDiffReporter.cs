using System.Threading.Tasks;
using FastPack.Lib.Logging;
using FastPack.Lib.TypeExtensions;

namespace FastPack.Lib.Diff;

internal class XmlDiffReporter : IDiffReporter
{
	private ILogger Logger { get; }

	public XmlDiffReporter(ILogger logger)
	{
		Logger = logger;
	}

	public async Task PrintReport(DiffReport diffReport, bool prettyPrint)
	{
		Logger.SetQuiet(false);
		await Logger.Info(diffReport.SerializeToXml(prettyPrint));
		Logger.SetQuiet();
	}
}