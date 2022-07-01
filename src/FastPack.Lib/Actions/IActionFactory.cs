using FastPack.Lib.Logging;
using FastPack.Lib.Options;

namespace FastPack.Lib.Actions;

public interface IActionFactory
{
	IAction CreateAction(ActionType actionType, IOptions options, ILogger logger);
}