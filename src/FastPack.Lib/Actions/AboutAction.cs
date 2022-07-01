using System;
using System.Text;
using System.Threading.Tasks;
using FastPack.Lib.Licenses;
using FastPack.Lib.Logging;

namespace FastPack.Lib.Actions;

internal class AboutAction : IAction
{
	private ILogger Logger { get; }
	private const string EMailInBase64 = "RmFzdFBhY2tAcXVhbm9zLmNvbQ==";
	internal IConsoleAbstraction Console { get; set; } = new ConsoleAbstraction();

	public AboutAction(ILogger logger)
	{
		Logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async Task<int> Run()
	{
		await CenterLine("About FastPack");
		await CenterLine("".PadRight(Console.GetWindowWidth(), '='));
		await Logger.InfoLine();
		await Logger.InfoLine(GetAboutText());
		await Logger.InfoLine("Repository");
		await Logger.InfoLine("  - https://github.com/QuanosSolutions/FastPack");
		await Logger.InfoLine();
		await Logger.InfoLine("License");
		await Logger.InfoLine("  - MIT");
		await Logger.InfoLine();
		await Logger.InfoLine("Contributors");
		await Logger.InfoLine("  - Werner Welsch (Quanos Content Solutions GmbH)");
		await Logger.InfoLine("  - Jens Hofmann (Quanos Content Solutions GmbH)");
		await Logger.InfoLine();
		await Logger.InfoLine("Contact");
		await Logger.InfoLine($"  - {Encoding.UTF8.GetString(Convert.FromBase64String(EMailInBase64))}");
		await Logger.InfoLine();
		await Logger.InfoLine("Sponsors");
		await Logger.InfoLine("  - Quanos Solutions GmbH | https://quanos.com/");
		await Logger.InfoLine();
		await Logger.InfoLine();
		await Logger.InfoLine("Press:");
		await Logger.InfoLine("  - Any key, except [L] and [3] to quit");
		await Logger.InfoLine("  - [L] to show our license text");
		await Logger.InfoLine("  - [3] to show the license of 3rd party libraries");

		ConsoleKeyInfo pressedKey = Console.ReadKey(true);

		switch (pressedKey.Key)
		{
			case ConsoleKey.L:
				await ShowOwnLicenseText();
				break;
			case ConsoleKey.D3:
			case ConsoleKey.NumPad3:
				await Show3rdPartyLicenseTexts();
				break;
		}

		return 0;
	}

	protected internal virtual async Task ShowOwnLicenseText()
	{
		await Logger.InfoLine();
		await Logger.InfoLine(await LicenseTextProvider.GetFastPackLicenseText());
		await Logger.InfoLine();
	}

	protected internal virtual async Task Show3rdPartyLicenseTexts()
	{
		foreach (string thirdPartyLicenseText in await LicenseTextProvider.GetThirdPartyLicenseTexts())
		{
			await Logger.InfoLine();
			await Logger.InfoLine(thirdPartyLicenseText);
		}

		await Logger.InfoLine();
	}

	protected internal virtual string GetAboutText()
	{
		return @"
FastPack is a lightning fast deduplication (de)compressor that is particularly useful for compressing and decompressing build artifacts containing a small to high degree of duplicate files. By default file/directory timestamps as well as meta data are preserved and restored. During decompression the restoration of file/directory timestamps and meta data can be skipped.

This tool was created as an internal tool by our DevOps-Crew to compress the ~27GB build artifacts of Quanos Content Solutions GmbH (https://www.quanos.com) as fast as possible (20s) into a file with reasonable size (500 MB), so it can be attached as a build artifact to each build run and later be extracted in a fast time (40s).

By default FastPack uses **Deflate** for compression with a compression level of 'Optimal' for compression. The compression algorithm and level can be specified with the pack-action. Currently the only supported compression algorithm is 'Deflate' and 'NoCompression'. We're looking for contributors to add reasonable compression algorithms.
".TrimStart();
	}

	private async Task CenterLine(string text)
	{
		if (text.Length >= Console.GetWindowWidth())
		{
			await Logger.InfoLine(text);
			return;
		}

		int totalLength = ((Console.GetWindowWidth() - text.Length) / 2)+text.Length;
		await Logger.InfoLine(text.PadLeft(totalLength));
	}
}