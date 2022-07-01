using FastPack.Lib.Logging;

namespace FastPack.Lib.Diff;

internal interface IDiffReporterFactory
{
	IDiffReporter GetDiffReporter(OutputFormat outputFormat, ILogger logger);
}