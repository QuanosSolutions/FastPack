using System;
using FastPack.Lib.Actions;
using FastPack.Lib.Logging;

namespace FastPack.Options.Parsers;

internal class OptionsParserFactory : IOptionsParserFactory
{
	public IOptionsParser Create(ActionType actionType, ILogger logger)
	{
		return actionType switch {
			ActionType.About => new AboutOptionsParser(logger),
			ActionType.Help => new HelpOptionsParser(logger),
			ActionType.Diff => new DiffOptionsParser(logger),
			ActionType.Info => new InfoOptionsParser(logger),
			ActionType.Licenses => new LicensesOptionsParser(logger),
			ActionType.Pack => new PackOptionsParser(logger),
			ActionType.Unpack => new UnpackOptionsParser(logger),
			ActionType.Version => new VersionOptionsParser(logger),
			_ => throw new NotImplementedException($"OptionsParser for {actionType} is not implemented, but should be!")
		};
	}
}