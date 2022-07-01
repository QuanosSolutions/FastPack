using System.Threading.Tasks;
using FastPack.Lib.Logging;

namespace FastPack.Options.Parsers;

internal class VersionOptionsParser : EmptyOptionsParser
{
	public VersionOptionsParser(ILogger logger) : base(logger)
	{
	}

	public override async Task PrintHelp()
	{
		await Logger.InfoLine("Usage: FastPack -a version");
		await Logger.InfoLine();
		await Logger.InfoLine("Description:");
		await Logger.InfoLine("  Shows information about the version of FastPack.");
		await Logger.InfoLine();
	}
}