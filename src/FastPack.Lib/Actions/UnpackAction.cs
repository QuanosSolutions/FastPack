using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FastPack.Lib.Logging;
using FastPack.Lib.Matching;
using FastPack.Lib.Options;
using FastPack.Lib.Unpackers;

namespace FastPack.Lib.Actions;

public class UnpackAction : IAction
{
	private ILogger Logger { get; }
	private UnpackOptions Options { get; }
	internal IArchiveUnpackerFactory ArchiveUnpackerFactory { get; set; } = new ArchiveUnpackerFactory();
	internal ITextMatchProviderFactory TextMatchProviderFactory { get; set; } = new TextMatchProviderFactory();

	public UnpackAction(ILogger logger, UnpackOptions options)
	{
		Logger = logger ?? throw new ArgumentNullException(nameof(logger));
		Options = options ?? throw new ArgumentNullException(nameof(options));
	}

	public async Task<int> Run()
	{
		ValidateOptions();

		return await (await ArchiveUnpackerFactory.GetUnpacker(Options.InputFilePath, Logger)).Extract(Options.InputFilePath, Options);
	}

	private void ValidateOptions()
	{
		Options.MaxDegreeOfParallelism ??= Environment.ProcessorCount;

		if (Options.OutputDirectoryPath == null)
			throw new InvalidOptionException("Please provide the output directory path.", nameof(Options.OutputDirectoryPath), ErrorConstants.Unpack_OutputDirectoryPath_Missing);

		Options.OutputDirectoryPath = Path.GetFullPath(Options.OutputDirectoryPath);
		
		if (Options.InputFilePath == null)
			throw new InvalidOptionException("Please provide the input file.", nameof(Options.InputFilePath), ErrorConstants.Unpack_InputFilePath_Missing);

		Options.InputFilePath = Path.GetFullPath(Options.InputFilePath);
		if (!File.Exists(Options.InputFilePath))
			throw new InvalidOptionException($"File not found: '{Options.InputFilePath}'", nameof(Options.InputFilePath), ErrorConstants.Unpack_InputFilePath_NoFound);
		
		if (Options.IncludeFilters.Any() || Options.ExcludeFilters.Any())
		{
			ITextMatchProvider textMatchProvider = TextMatchProviderFactory.GetProvider(Options.FilterType);

			Options.IncludeFilters.ForEach(filter => {
				if (!textMatchProvider.IsPatternValid(filter))
					throw new InvalidOptionException($"Invalid include filter: {filter}", nameof(Options.IncludeFilters), ErrorConstants.Unpack_Invalid_IncludeFilter);
			});

			Options.ExcludeFilters.ForEach(filter => {
				if (!textMatchProvider.IsPatternValid(filter))
					throw new InvalidOptionException($"Invalid exclude filter: {filter}", nameof(Options.ExcludeFilters), ErrorConstants.Unpack_Invalid_ExcludeFilter);
			});
		}
	}
}