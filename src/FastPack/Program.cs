using System;
using System.Linq;
using System.Threading.Tasks;
using FastPack.Lib;
using FastPack.Lib.Actions;
using FastPack.Lib.Logging;
using FastPack.Lib.Options;
using FastPack.Options;
using FastPack.Options.Parsers;

namespace FastPack;

class Program
{
	internal IActionFactory ActionFactory { get; set; } = new ActionFactory();
	internal IOptionsParserFactory OptionsParserFactory { get; set; } = new OptionsParserFactory();

	static async Task<int> Main(string[] args)
	{
		return await new Program().Run(args);
	}

	private async Task<int> Run(string[] args)
	{
		ILogger logger = GetLogger(ref args);
		try
		{
			if (!args.Any())
			{
				await new HelpOptionsParser(logger).PrintHelp();
				return ErrorConstants.UnspecificError;
			}

			FastPackActionOptions fastPackActionOptions = await new FastPackActionOptionsParser(logger).CreateFromArgs(args);
			IOptionsParser optionsParser = OptionsParserFactory.Create(fastPackActionOptions.Action, logger);
			IOptions options = await optionsParser.CreateFromArgs(fastPackActionOptions.StrippedArgs, fastPackActionOptions.IsStrictParamsMode, logger);

			// if options couldn't be parsed without errors
			if (options == null)
				return ErrorConstants.UnspecificError;

			if (options.QuietMode)
				logger.SetQuiet();

			IAction action = ActionFactory.CreateAction(fastPackActionOptions.Action, options, logger);

			if (fastPackActionOptions.HelpRequested || action == null)
			{
				await optionsParser.PrintHelp();
				return 0;
			}
			
			return await action.Run();
		}
		catch (InvalidOptionException invalidOptionException)
		{
			await logger.ErrorLine(invalidOptionException.Message);
			return invalidOptionException.ExceptionCode;
		}
		catch (Exception ex)
		{
			await (logger ?? new ConsoleLogger(new ConsoleAbstraction())).ErrorLine(ex.ToString());
			return 1;
		}
	}

	private static ILogger GetLogger(ref string[] args)
	{
		bool IsQuietMode(string s) => s.Equals("-q", StringComparison.InvariantCultureIgnoreCase) || s.Equals("--quiet", StringComparison.InvariantCultureIgnoreCase);
		bool IsDebugMode(string s) => s.Equals("--debug", StringComparison.InvariantCultureIgnoreCase);

		bool isDebugMode = args.Any(IsDebugMode);
		if (isDebugMode)
			args = args.Where(a => !IsDebugMode(a)).ToArray();

		ILogger logger = new ConsoleLogger(new ConsoleAbstraction(), isDebugMode);

		if (args.Any(IsQuietMode))
		{
			args = args.Where(a => !IsQuietMode(a)).ToArray();
			logger.SetQuiet();
		}

		return logger;
	}
}