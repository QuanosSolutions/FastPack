using System;
using System.IO;
using System.Threading.Tasks;
using FastPack.Lib.Logging;
using FastPack.Lib.ManifestManagement;
using FastPack.Lib.ManifestReporting;
using FastPack.Lib.Options;
using FastPack.Lib.Unpackers;

namespace FastPack.Lib.Actions;

public class InfoAction : IAction
{
	public ILogger Logger { get; }
	public InfoOptions Options { get; }
	internal IArchiveUnpackerFactory ArchiveUnpackerFactory { get; set; } = new ArchiveUnpackerFactory();
	internal IManifestReporterFactory ManifestReporterFactory { get; set; } = new ManifestReporterFactory();

	public InfoAction(ILogger logger, InfoOptions options)
	{
		Logger = logger ?? throw new ArgumentNullException(nameof(logger));
		Options = options ?? throw new ArgumentNullException(nameof(options));
	}

	public async Task<int> Run()
	{
		ValidateOptions();
			
		Manifest manifest = await GetManifestFromFile(Options.InputFilePath);
		IManifestReporter manifestReporter = ManifestReporterFactory.GetManifestReporter(Options.OutputFormat, Logger);
		await manifestReporter.PrintReport(manifest, true, Options.Detailed, Options.PrettyPrint);

		return 0;
	}

	private void ValidateOptions()
	{
		if (Options.InputFilePath == null)
			throw new InvalidOptionException("Please provide the input file.", nameof(Options.InputFilePath),
				ErrorConstants.Diff_InputFilePath_Missing);

		Options.InputFilePath = Path.GetFullPath(Options.InputFilePath);
		if (!File.Exists(Options.InputFilePath))
			throw new InvalidOptionException($"File not found: '{Options.InputFilePath}'", nameof(Options.InputFilePath),
				ErrorConstants.Diff_InputFilePath_NoFound);
	}

	private async Task<Manifest> GetManifestFromFile(string filePath)
	{
		await using FileStream fileStream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, Constants.BufferSize, Constants.OpenFileStreamsAsync);
		return await (await ArchiveUnpackerFactory.GetUnpacker(fileStream, Logger)).GetManifestFromStream(fileStream);
	}
}