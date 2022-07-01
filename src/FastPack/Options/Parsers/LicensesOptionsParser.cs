using System.Threading.Tasks;
using FastPack.Lib.Logging;

namespace FastPack.Options.Parsers;

internal class LicensesOptionsParser : EmptyOptionsParser
{
	public LicensesOptionsParser(ILogger logger) : base(logger)
	{
	}

	public override async Task PrintHelp()
	{
		await Logger.InfoLine("Usage: FastPack -a licenses");
		await Logger.InfoLine();
		await Logger.InfoLine("Description:");
		await Logger.InfoLine("  Shows information about FastPack and 3rd party license.");
		await Logger.InfoLine();
	}
}