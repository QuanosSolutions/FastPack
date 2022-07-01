using System;
using System.Reflection;
using System.Threading.Tasks;
using FastPack.Lib.Logging;

namespace FastPack.Lib.Actions;

internal class VersionAction : IAction
{
	private ILogger Logger { get; }

	public VersionAction(ILogger logger)
	{
		Logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async Task<int> Run()
	{
		await Logger.InfoLine(Assembly.GetEntryAssembly()!.GetName()!.Version!.ToString(3));

		return 0;
	}
}