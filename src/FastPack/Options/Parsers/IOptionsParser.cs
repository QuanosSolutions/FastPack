using System.Threading.Tasks;
using FastPack.Lib.Logging;
using FastPack.Lib.Options;

namespace FastPack.Options.Parsers;

public interface IOptionsParser
{
	Task<IOptions> CreateFromArgs(string[] args, bool strictMode, ILogger logger);
	Task PrintHelp();
}