using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using FastPack.Lib;
using FastPack.Lib.Logging;
using FastPack.Lib.Matching;
using FastPack.Lib.Options;
using FastPack.Lib.TypeExtensions;

namespace FastPack.Options.Parsers;

public class UnpackOptionsParser : IOptionsParser
{
	private ILogger Logger { get; }

	public UnpackOptionsParser(ILogger logger)
	{
		Logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	async Task<IOptions> IOptionsParser.CreateFromArgs(string[] args, bool strictMode, ILogger logger)
	{
		return await CreateFromArgs(args, strictMode, logger);
	}

	public async Task PrintHelp()
	{
		await Logger.InfoLine("Usage: FastPack -a unpack [-options]");
		await Logger.InfoLine();
		await Logger.InfoLine("Options:");
		await Logger.InfoLine("  -i|--input");
		await Logger.InfoLine("    Path of the file to unpack");
		await Logger.InfoLine("    Valid values: a path to a file");
		await Logger.InfoLine("    Required: true");
		await Logger.InfoLine($"    Example: -i \"{GetInputFileString()}\"");
		await Logger.InfoLine();
		await Logger.InfoLine("  -o|--output");
		await Logger.InfoLine("    Path of the output directory");
		await Logger.InfoLine("    Valid values: a path to a directory");
		await Logger.InfoLine("    Required: true");
		await Logger.InfoLine($"    Example: -o \"{GetOutputDirectoryString()}\"");
		await Logger.InfoLine();
		await Logger.InfoLine("Checkout options (all OPTIONAL)");
		await Logger.InfoLine("  -ef|--exclude-filter");
		await Logger.InfoLine("    Filter to exclude files/directories during unpack");
		await Logger.InfoLine("    Valid values: depends on the filter type parameter (-ft)");
		await Logger.InfoLine("    Required: false");
		await Logger.InfoLine("    Occurrence: multiple");
		await Logger.InfoLine("    Example: -if dll -if dll\\**\\*");
		await Logger.InfoLine();
		await Logger.InfoLine("  -fci|--filter-case-insensitive");
		await Logger.InfoLine("    Make the filter case insensitive (default is: case sensitive)");
		await Logger.InfoLine("    Required: false");
		await Logger.InfoLine("    Example: -fci");
		await Logger.InfoLine();
		await Logger.InfoLine("  -ft|--filter-type");
		await Logger.InfoLine("    The type of filter to use");
		await Logger.InfoLine("    Valid values: Glob, Regex");
		await Logger.InfoLine("    Required: false");
		await Logger.InfoLine("    Default: Glob");
		await Logger.InfoLine("    Example: -ft Regex");
		await Logger.InfoLine();
		await Logger.InfoLine("  -p|--parallelism [-]Number[%]");
		await Logger.InfoLine("    Parallelism settings.");
		await Logger.InfoLine("    Required: false");
		await Logger.InfoLine("    Default:: Is set to max. number of cores");
		await Logger.InfoLine("    Valid values: Can be absolute numbers of cores or percentage values");
		await Logger.InfoLine("    Example: -p -20%, -p 25%, -p -1, -p 8");
		await Logger.InfoLine();
		await Logger.InfoLine("  -mm|--maxmemory");
		await Logger.InfoLine("    Maximum memory used for compression");
		await Logger.InfoLine("    Default: Is set to half of available memory");
		await Logger.InfoLine("    Valid values: Can be absolute numbers of bytes or percentage values of available memory");
		await Logger.InfoLine("    Required: false");
		await Logger.InfoLine("    Example: -20%, 25%, -1G, 8G, 200M, 100K, 100");
		await Logger.InfoLine();
		await Logger.InfoLine("  -dr|--dryrun [detailed]");
		await Logger.InfoLine("    Only perform the action as dry run meaning no files will be written and only information about the actions will be returned.");
		await Logger.InfoLine("    When specifying \"detailed\" the information will be more detailed.");
		await Logger.InfoLine("    Default: not active");
		await Logger.InfoLine("    Required: false");
		await Logger.InfoLine("    Example: -dr");
		await Logger.InfoLine("    Example: -dr detailed");
		await Logger.InfoLine();
		await Logger.InfoLine("  -drf|--dryrunformat");
		await Logger.InfoLine("    The output format of the dry run result.");
		await Logger.InfoLine("    Valid values: Text, Json, Xml");
		await Logger.InfoLine("    Default: Text");
		await Logger.InfoLine("    Required: false");
		await Logger.InfoLine("    Example: -drf Json");
		await Logger.InfoLine();
		await Logger.InfoLine("  --pretty");
		await Logger.InfoLine("    Pretty print of the json or xml output.");
		await Logger.InfoLine("    Default: false");
		await Logger.InfoLine("    Required: false");
		await Logger.InfoLine("    Example: --pretty");
		await Logger.InfoLine();
		await Logger.InfoLine("  -np|--no-progress");
		await Logger.InfoLine("     Disable the display of progress.");
		await Logger.InfoLine("     Default: false");
		await Logger.InfoLine("     Required: false");
		await Logger.InfoLine("     Example: -np");
		await Logger.InfoLine();
		await Logger.InfoLine("  -nd|--no-dates");
		await Logger.InfoLine("    Disable the restoration of file system dates.");
		await Logger.InfoLine("    Default: false");
		await Logger.InfoLine("    Required: false");
		await Logger.InfoLine("    Example: -nd");
		await Logger.InfoLine();
		await Logger.InfoLine("  -nfp|--no-permissions");
		await Logger.InfoLine("    Disable the restoration of file system permissions.");
		await Logger.InfoLine("    Default: false");
		await Logger.InfoLine("    Required: false");
		await Logger.InfoLine("    Example: -nfp");
		await Logger.InfoLine();
		await Logger.InfoLine("  -isc|--ignore-space-check");
		await Logger.InfoLine("    Disable the check for available disk space.");
		await Logger.InfoLine("    Default: false");
		await Logger.InfoLine("    Required: false");
		await Logger.InfoLine("    Example: -isc");
		await Logger.InfoLine();
	}

	[ExcludeFromCodeCoverage]
	private string GetInputFileString()
	{
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			return "/home/user/file.fup";
		if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			return "/Users/user/file.fup";
		return @"C:\Users\user\file.up";
	}

	[ExcludeFromCodeCoverage]
	private string GetOutputDirectoryString()
	{
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			return "/home/user/output-dir";
		if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			return "/Users/user/output-dir";
		return @"C:\Users\user\output-dir";
	}

	public async Task<UnpackOptions> CreateFromArgs(string[] args, bool strictMode, ILogger logger)
	{
		UnpackOptions options = new();

		ArgumentsParser argumentsParser = new(args, logger, strictMode);
		Dictionary<string, Func<string, Task<bool>>> parameterProcessingMap = new(StringComparer.InvariantCultureIgnoreCase);
		parameterProcessingMap.AddMultiple(new[] { "-i", "--input" }, async _ => await argumentsParser.ProcessTextParameterValue(v => options.InputFilePath = v));
		parameterProcessingMap.AddMultiple(new[] { "-o", "--output" }, async _ => await argumentsParser.ProcessTextParameterValue(v => options.OutputDirectoryPath = v));
		parameterProcessingMap.AddMultiple(new[] { "-if", "--include-filter" }, async _ => await argumentsParser.ProcessTextParameterValue(v => options.IncludeFilters.Add(v)));
		parameterProcessingMap.AddMultiple(new[] { "-ef", "--exclude-filter" }, async _ => await argumentsParser.ProcessTextParameterValue(v => options.ExcludeFilters.Add(v)));
		parameterProcessingMap.AddMultiple(new[] { "-fci", "--filter-case-insensitive" }, _ => Task.FromResult(options.FilterCaseInsensitive = true));
		parameterProcessingMap.AddMultiple(new[] { "-ft", "--filter-type" }, async _ => await argumentsParser.ProcessEnumParameterValue<TextMatchProviderType>(v => options.FilterType = v));
		parameterProcessingMap.AddMultiple(new[] { "-p", "--parallelism" }, async _ => await argumentsParser.ProcessNumberOfProcessors(v => options.MaxDegreeOfParallelism = v));
		parameterProcessingMap.AddMultiple(new[] { "-mm", "--maxmemory" }, async _ => await argumentsParser.ProcessMaxMemory(v => options.MaxMemory = v));
		parameterProcessingMap.AddMultiple(new[] { "-dr", "--dryrun" }, async _ => {
			options.DryRun = true;
			return await argumentsParser.ProcessOptionalTextParameterValue(_ => options.DetailedDryRun = true, "detailed", true);
		});
		parameterProcessingMap.AddMultiple(new[] { "-drf", "--dryrunformat" }, async _ => {
			if (!await argumentsParser.ProcessEnumParameterValue<OutputFormat>(v => options.DryRunOutputFormat = v))
				return false;
			options.QuietMode = options.DryRunOutputFormat is OutputFormat.Json or OutputFormat.Xml;
			return true;
		});
		parameterProcessingMap.Add("--pretty", _ => Task.FromResult(options.PrettyPrint = true));
		parameterProcessingMap.AddMultiple(new[] { "-np", "--no-progress" }, _ => { options.ShowProgress = false; return Task.FromResult(true); });
		parameterProcessingMap.AddMultiple(new[] { "-nd", "--no-dates" }, _ => { options.RestoreDates = false; return Task.FromResult(true); });
		parameterProcessingMap.AddMultiple(new[] { "-nfp", "--no-permissions" }, _ => { options.RestorePermissions = false; return Task.FromResult(true); });
		parameterProcessingMap.AddMultiple(new[] { "-isc", "--ignore-space-check" }, _ => Task.FromResult(options.IgnoreDiskSpaceCheck = true));

		return await argumentsParser.Parse(parameterProcessingMap) ? options : null;
	}
}