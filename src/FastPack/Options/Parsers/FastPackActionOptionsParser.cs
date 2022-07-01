using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FastPack.CmdLine;
using FastPack.Lib.Actions;
using FastPack.Lib.Logging;

namespace FastPack.Options.Parsers
{
	internal class FastPackActionOptionsParser
	{
		private ILogger Logger { get; }
		internal IFileToCommandLineConverterFactory FileToCommandLineConverterFactory { get; set; } = new FileToCommandLineConverterFactory();

		public FastPackActionOptionsParser(ILogger logger)
		{
			Logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task<FastPackActionOptions> CreateFromArgs(string[] commandLineArgs)
		{
			bool IsLooseParameterMode(string s) => s.Equals("-lp", StringComparison.InvariantCultureIgnoreCase) || s.Equals("--looseparams", StringComparison.InvariantCultureIgnoreCase);
			List<string> strippedArgs = new();
			ActionType? finalAction = null;
			bool helpRequested = false;

			string[] args = commandLineArgs;

			if (args.Any(a => a.StartsWith("@")))
				(finalAction, args) = await GetArgumentsFromFilesAndCmdLine(args);

			bool isLooseParameterMode = args.Any(IsLooseParameterMode);
			if (isLooseParameterMode)
				args = args.Where(a => !IsLooseParameterMode(a)).ToArray();

			// if no error occurred during file parsing above
			if (!finalAction.HasValue)
			{
				(finalAction, helpRequested) = await GetAction(args, strippedArgs);
				finalAction ??= helpRequested ? ActionType.Help : ActionType.Pack;
			}

			return new FastPackActionOptions { Action = finalAction.Value, HelpRequested = helpRequested, StrippedArgs = strippedArgs.ToArray(), IsStrictParamsMode = !isLooseParameterMode };
		}

		private async Task<(ActionType? Action, string[] Arguments)> GetArgumentsFromFilesAndCmdLine(string[] args)
		{
			(ActionType? Action, string[] Arguments) errorResult = (ActionType.Help, args);

			List<string> convertedArguments = new();

			foreach (string arg in args)
			{
				if (!arg.StartsWith("@"))
				{
					convertedArguments.Add(arg);
					continue;
				}

				string argumentsFile = arg[1..];

				if (string.IsNullOrWhiteSpace(argumentsFile))
				{
					await Logger.ErrorLine($"File is missing for file parameter: {args[0]}");
					return errorResult;
				}

				argumentsFile = Path.GetFullPath(argumentsFile);

				if (!File.Exists(argumentsFile))
				{
					await Logger.ErrorLine($"File '{argumentsFile}' does not exist.");
					return errorResult;
				}

				convertedArguments.AddRange(await FileToCommandLineConverterFactory.GetConverter(argumentsFile).ConvertToArgs(argumentsFile));
			}

			return (null, convertedArguments.ToArray());
		}

		private async Task<(ActionType? Action, bool HelpRequested)> GetAction(string[] args, List<string> strippedArgs)
		{
			ActionType? finalAction = null;
			bool helpRequested = false;

			int index = 0;
			while (index < args.Length)
			{
				switch (args[index].ToLowerInvariant())
				{
					case "--version":
						if (args.Length == 1)
							finalAction = ActionType.Version;
						else
							strippedArgs.Add(args[index]);
						break;
					case "-a":
					case "--action":
						index++;
						if (index >= args.Length)
						{
							await Logger.ErrorLine($"Missing value for parameter: {args[index-1]}");
							finalAction = ActionType.Help;
							continue;
						}
						
						string actionText = args[index];

						if (!Enum.TryParse(actionText, true, out ActionType action))
						{
							await Logger.ErrorLine($"Value '{actionText}' is not valid for parameter: {args[index - 1]}. Valid values: {string.Join(", ", Enum.GetValues<ActionType>())}");
							finalAction = ActionType.Help;
							continue;
						}

						finalAction = action;
						break;
					case "-h":
					case "--help":
					case "/?":
					case "-?":
						helpRequested = true;
						break;
					default:
						strippedArgs.Add(args[index]);
						break;
				}

				index++;
			}

			return (finalAction, helpRequested);
		}
	}
}
