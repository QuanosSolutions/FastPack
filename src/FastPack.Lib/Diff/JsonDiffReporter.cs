using System.Threading.Tasks;
using FastPack.Lib.Logging;
using FastPack.Lib.TypeExtensions;

namespace FastPack.Lib.Diff;

internal class JsonDiffReporter : IDiffReporter
{
	private ILogger Logger { get; }

	public JsonDiffReporter(ILogger logger)
	{
		Logger = logger;
	}

	public async Task PrintReport(DiffReport diffReport, bool prettyPrint)
	{
		Logger.SetQuiet(false);
		await Logger.Info(diffReport.SerializeToJson(new DiffReportJsonContext(JsonHelper.GetSerializerOptions(prettyPrint)).DiffReport));
		Logger.SetQuiet();
	}
}