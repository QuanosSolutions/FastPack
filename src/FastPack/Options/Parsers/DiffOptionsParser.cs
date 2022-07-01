using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FastPack.Lib;
using FastPack.Lib.Logging;
using FastPack.Lib.Matching;
using FastPack.Lib.Options;
using FastPack.Lib.TypeExtensions;

namespace FastPack.Options.Parsers;

public class DiffOptionsParser : IOptionsParser
{
	private ILogger Logger { get; }

	public DiffOptionsParser(ILogger logger)
	{
		Logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	async Task<IOptions> IOptionsParser.CreateFromArgs(string[] args, bool strictMode, ILogger logger)
	{
		return await CreateFromArgs(args, strictMode, logger);
	}

	public async Task PrintHelp()
	{
		await Logger.InfoLine("Usage: FastUnpack -a diff [options]");
		await Logger.InfoLine();
		await Logger.InfoLine("Options:");
		await Logger.InfoLine("  -1|--first");
		await Logger.InfoLine("    First file path for comparison");
		await Logger.InfoLine();
		await Logger.InfoLine("  -2|--second");
		await Logger.InfoLine("    Second file path for comparison");
		await Logger.InfoLine();
		await Logger.InfoLine("  -s|--settings");
		await Logger.InfoLine("    Settings for comparison. Parameter can be provided multiple times.");
		await Logger.InfoLine("    Valid values: StructureOnly (always active), Size, Date, Permission");
		await Logger.InfoLine("    Default: StructureOnly");
		await Logger.InfoLine("    [StructureOnly]: Only compares the structure (Added/Removed files)");
		await Logger.InfoLine("    [Size]: Also compares file sizes");
		await Logger.InfoLine("    [Date]: Also compares file system dates");
		await Logger.InfoLine("    [Permission]: Also compares permissions");
		await Logger.InfoLine("    Example: -s Size");
		await Logger.InfoLine("    Example: -s Size -s Date");
		await Logger.InfoLine();
		await Logger.InfoLine("  -if|--include-filter");
		await Logger.InfoLine("    Filter to include files/directories during diff");
		await Logger.InfoLine("    Valid values: depends on the filter type parameter (-ft)");
		await Logger.InfoLine("    Required: false");
		await Logger.InfoLine("    Occurrence: multiple");
		await Logger.InfoLine("    Example: -if dll -if dll\\**\\*");
		await Logger.InfoLine();
		await Logger.InfoLine("  -ef|--exclude-filter");
		await Logger.InfoLine("    Filter to exclude files/directories during diff");
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
		await Logger.InfoLine("  -x|--extract");
		await Logger.InfoLine("    Extract added/remove/changed files");
		await Logger.InfoLine("    Valid values: optional directory path for extraction");
		await Logger.InfoLine("    Default: New temporary folder");
		await Logger.InfoLine("    Example: -x C:\\temp\\diffextract");
		await Logger.InfoLine();
		await Logger.InfoLine("  -f|--format");
		await Logger.InfoLine("    The output format of the diff.");
		await Logger.InfoLine("    Valid values: Text, Json, Xml");
		await Logger.InfoLine("    Default: Text");
		await Logger.InfoLine("    Required: false");
		await Logger.InfoLine("    Example: -f Json");
		await Logger.InfoLine();
		await Logger.InfoLine("  --pretty");
		await Logger.InfoLine("    Pretty print of the json or xml output.");
		await Logger.InfoLine("    Default: false");
		await Logger.InfoLine("    Required: false");
		await Logger.InfoLine("    Example: --pretty");
		await Logger.InfoLine();
	}

	public async Task<DiffOptions> CreateFromArgs(string[] args, bool strictMode, ILogger logger)
	{
		DiffOptions options = new() { DiffSettings = DiffOptions.DiffSetting.StructureOnly };

		ArgumentsParser argumentsParser = new(args, logger, strictMode);
		Dictionary<string, Func<string, Task<bool>>> parameterProcessingMap = new(StringComparer.InvariantCultureIgnoreCase);
		parameterProcessingMap.AddMultiple(new[] {"-1", "--first"}, async _ => await argumentsParser.ProcessTextParameterValue(v => options.FirstFilePath = v));
		parameterProcessingMap.AddMultiple(new[] { "-2", "--second" }, async _ => await argumentsParser.ProcessTextParameterValue(v => options.SecondFilePath = v));
		parameterProcessingMap.AddMultiple(new[] { "-x", "--extract" }, async _ => {
			options.ExtractionEnabled = true;
			return await argumentsParser.ProcessOptionalTextParameterValue(v => options.ExtractionDirectoryPath = v);
		});
		parameterProcessingMap.AddMultiple(new[] { "-s", "--settings" }, async _ => await argumentsParser.ProcessEnumParameterValue<DiffOptions.DiffSetting>(v => options.DiffSettings |= v));
		parameterProcessingMap.AddMultiple(new[] { "-if", "--include-filter" }, async _ => await argumentsParser.ProcessTextParameterValue(v => options.IncludeFilters.Add(v)));
		parameterProcessingMap.AddMultiple(new[] { "-ef", "--exclude-filter" }, async _ => await argumentsParser.ProcessTextParameterValue(v => options.ExcludeFilters.Add(v)));
		parameterProcessingMap.AddMultiple(new[] { "-fci", "--filter-case-insensitive" }, _ => Task.FromResult(options.FilterCaseInsensitive = true));
		parameterProcessingMap.AddMultiple(new[] { "-ft", "--filter-type" }, async _ => await argumentsParser.ProcessEnumParameterValue<TextMatchProviderType>(v => options.FilterType = v));
		parameterProcessingMap.AddMultiple(new[] { "-f", "--format" }, async _ => {
			if (!await argumentsParser.ProcessEnumParameterValue<OutputFormat>(v => options.OutputFormat = v))
				return false;
			options.QuietMode = options.OutputFormat is OutputFormat.Json or OutputFormat.Xml;
			return true;
		});
		parameterProcessingMap.Add("--pretty", _ => Task.FromResult(options.PrettyPrint = true));

		return await argumentsParser.Parse(parameterProcessingMap) ? options : null;
	}
}