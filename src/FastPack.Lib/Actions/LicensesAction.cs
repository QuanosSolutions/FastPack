using System;
using System.Threading.Tasks;
using FastPack.Lib.Licenses;
using FastPack.Lib.Logging;

namespace FastPack.Lib.Actions;

internal class LicensesAction: IAction
{
	private ILogger Logger { get; }

	public LicensesAction(ILogger logger)
	{
		Logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async Task<int> Run()
	{
		await Logger.InfoLine("FastPack License");
		await Logger.InfoLine("================");
		await Logger.InfoLine(await LicenseTextProvider.GetFastPackLicenseText("  "));
		await Logger.InfoLine();
		await Logger.InfoLine("3rd party Licenses");
		await Logger.InfoLine("==================");
		await Logger.InfoLine();
		foreach (string thirdPartyLicenseText in await LicenseTextProvider.GetThirdPartyLicenseTexts())
		{
			await Logger.InfoLine();
			await Logger.InfoLine(thirdPartyLicenseText);
		}

		await Logger.InfoLine();

		return 0;
	}
}