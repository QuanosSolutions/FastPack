using System.Threading.Tasks;
using FastPack.Lib.Logging;

namespace FastPack.Options.Parsers;

internal class HelpOptionsParser : EmptyOptionsParser
{
	public HelpOptionsParser(ILogger logger) : base(logger)
	{
	}

	public override async Task PrintHelp()
	{
		await Logger.InfoLine("Usage: FastPack [options]");
		await Logger.InfoLine();
		await Logger.InfoLine("Options:");
		await Logger.InfoLine("  -a|--action");
		await Logger.InfoLine("    Specifies the action to execute");
		await Logger.InfoLine("    Valid values: about, help, pack, unpack, diff, info, licenses, version");
		await Logger.InfoLine("    Default: pack");
		await Logger.InfoLine("    Example: -a unpack");
		await Logger.InfoLine("    Example: --action unpack");
		await Logger.InfoLine();
		await Logger.InfoLine("  -q|--quiet");
		await Logger.InfoLine("    Enables quiet-mode, only errors are shown");
		await Logger.InfoLine();
		await Logger.InfoLine("  -lp|--looseparams");
		await Logger.InfoLine("    Ignores unknown parameters");
		await Logger.InfoLine();
		await Logger.InfoLine("  -h|--help|-?|/?");
		await Logger.InfoLine("    Shows general or action related help");
		await Logger.InfoLine();
		await Logger.InfoLine("  @AnyFileName");
		await Logger.InfoLine("    Can be used at any position to provide a file with input parameters");
		await Logger.InfoLine("    The provided file can have different formats that are detected by their file extension.");
		await Logger.InfoLine("    Supported formats: JSON (*.json), XML (*.xml), Text (*.*)");
		await Logger.InfoLine("    Default: Text");
		await Logger.InfoLine("    Example: @params.json");
		await Logger.InfoLine();
		await Logger.InfoLine("Actions:");
		await Logger.InfoLine("  -a about");
		await Logger.InfoLine("    Shows information about FastPack and the contributors.");
		await Logger.InfoLine();
		await Logger.InfoLine("  -a help");
		await Logger.InfoLine("    Shows this help output.");
		await Logger.InfoLine();
		await Logger.InfoLine("  -a pack");
		await Logger.InfoLine("    Used to compress the content of directories.");
		await Logger.InfoLine();
		await Logger.InfoLine("  -a unpack");
		await Logger.InfoLine("    Used to decompress the content of an archive.");
		await Logger.InfoLine();
		await Logger.InfoLine("  -a diff");
		await Logger.InfoLine("    Shows differences between 2 archives.");
		await Logger.InfoLine();
		await Logger.InfoLine("  -a info");
		await Logger.InfoLine("    Shows information of an archive.");
		await Logger.InfoLine();
		await Logger.InfoLine("  -a licenses");
		await Logger.InfoLine("    Shows the licenses of all 3rd party components used in FastPack.");
		await Logger.InfoLine();
		await Logger.InfoLine("  -a version");
		await Logger.InfoLine("    Shows information about the version of FastPack.");
		await Logger.InfoLine();
	}
}