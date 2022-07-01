using System;
using FastPack.Lib.Logging;

namespace FastPack.Lib.Diff;

internal class DiffReporterFactory: IDiffReporterFactory
{
	public IDiffReporter GetDiffReporter(OutputFormat outputFormat, ILogger logger)
	{
		return outputFormat switch
		{
			OutputFormat.Text => new TextDiffReporter(logger, new ConsoleAbstraction()),
			OutputFormat.Json => new JsonDiffReporter(logger),
			OutputFormat.Xml => new XmlDiffReporter(logger),
			_ => throw new NotSupportedException($"There is no diff reporter for output type {outputFormat}.")
		};
	}
}