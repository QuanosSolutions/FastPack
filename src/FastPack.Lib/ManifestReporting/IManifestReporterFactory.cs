using FastPack.Lib.Logging;

namespace FastPack.Lib.ManifestReporting;

internal interface IManifestReporterFactory
{
	IManifestReporter GetManifestReporter(OutputFormat outputFormat, ILogger logger);
}