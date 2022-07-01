using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using FastPack.Lib;
using FastPack.Lib.Logging;
using FastPack.Lib.Options;
using FastPack.Lib.TypeExtensions;

namespace FastPack.Options.Parsers;

public class InfoOptionsParser : IOptionsParser
{
	private ILogger Logger { get; }

	public InfoOptionsParser(ILogger logger)
	{
		Logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	async Task<IOptions> IOptionsParser.CreateFromArgs(string[] args, bool strictMode, ILogger logger)
	{
		return await CreateFromArgs(args, strictMode, logger);
	}

	public async Task PrintHelp()
	{
		await Logger.InfoLine("Usage: FastPack -a info [options]");
		await Logger.InfoLine();
		await Logger.InfoLine("Options:");
		await Logger.InfoLine("  -i|--input");
		await Logger.InfoLine("    The fup file for which to show information");
		await Logger.InfoLine("    Valid values: a path to a file");
		await Logger.InfoLine("    Required: true");
		await Logger.InfoLine($"    Example: -i \"{GetInputFileString()}\"");
		await Logger.InfoLine();
		await Logger.InfoLine("  -d|--detailed");
		await Logger.InfoLine("    Flag for showing detailed information like folders and files.");
		await Logger.InfoLine("    Default: false");
		await Logger.InfoLine("    Required: false");
		await Logger.InfoLine("    Example: -d");
		await Logger.InfoLine();
		await Logger.InfoLine("  -f|--format");
		await Logger.InfoLine("    The output format of the manifest information.");
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

	[ExcludeFromCodeCoverage]
	private string GetInputFileString()
	{
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			return "/home/user/file.fup";
		if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			return "/Users/user/file.fup";
		return @"C:\Users\user\file.up";
	}

	public async Task<InfoOptions> CreateFromArgs(string[] args, bool strictMode, ILogger logger)
	{
		InfoOptions options = new();

		ArgumentsParser argumentsParser = new(args, logger, strictMode);
		Dictionary<string, Func<string, Task<bool>>> parameterProcessingMap = new(StringComparer.InvariantCultureIgnoreCase);
		parameterProcessingMap.AddMultiple(new[] { "-i", "--input" }, async _ => await argumentsParser.ProcessTextParameterValue(v => options.InputFilePath = v));
		parameterProcessingMap.AddMultiple(new[] { "-d", "--detailed" }, _ => Task.FromResult(options.Detailed = true));
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