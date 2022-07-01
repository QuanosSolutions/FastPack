using FastPack.Lib.Actions;
using FastPack.Lib.Logging;

namespace FastPack.Options.Parsers;

internal interface IOptionsParserFactory
{
	IOptionsParser Create(ActionType actionType, ILogger logger);
}