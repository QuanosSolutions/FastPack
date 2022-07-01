using System;
using FastPack.Lib.Logging;
using FastPack.Lib.Options;

namespace FastPack.Lib.Actions;

public class ActionFactory : IActionFactory
{
	public IAction CreateAction(ActionType actionType, IOptions options, ILogger logger)
	{
		return actionType switch {
			ActionType.About => new AboutAction(logger),
			ActionType.Pack => new PackAction(logger, (PackOptions)options),
			ActionType.Diff => new DiffAction(logger, (DiffOptions)options),
			ActionType.Unpack => new UnpackAction(logger, (UnpackOptions)options),
			ActionType.Info => new InfoAction(logger, (InfoOptions)options),
			ActionType.Help => null,
			ActionType.Licenses => new LicensesAction(logger),
			ActionType.Version => new VersionAction(logger),
			_ => throw new NotSupportedException($"Action type is not supported: {actionType}")
		};
	}
}