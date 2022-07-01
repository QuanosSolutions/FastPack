using System;
using FastPack.Lib.Logging;

namespace FastPack.Lib.ManifestReporting;

internal class ManifestReporterFactory: IManifestReporterFactory
{
	public IManifestReporter GetManifestReporter(OutputFormat outputFormat, ILogger logger)
	{
		return outputFormat switch
		{
			OutputFormat.Text => new TextManifestReporter(logger),
			OutputFormat.Json => new JsonManifestReporter(logger),
			OutputFormat.Xml => new XmlManifestReporter(logger),
			_ => throw new NotSupportedException($"There is no manifest reporter for output type {outputFormat}.")
		};
	}
}