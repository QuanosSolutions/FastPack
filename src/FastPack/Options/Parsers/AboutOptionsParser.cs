using System.Threading.Tasks;
using FastPack.Lib.Logging;

namespace FastPack.Options.Parsers;

internal class AboutOptionsParser : EmptyOptionsParser
{
	public AboutOptionsParser(ILogger logger) : base(logger)
	{
	}

	public override async Task PrintHelp()
	{
		await Logger.InfoLine("Usage: FastPack -a about");
		await Logger.InfoLine();
		await Logger.InfoLine("Description:");
		await Logger.InfoLine("  Shows information about FastPack and its contributors.");
		await Logger.InfoLine();
	}
}