using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;

namespace FastPack.Lib.Logging;

public class ConsoleLogger : ILogger
{
	private IConsoleAbstraction ConsoleAbstraction { get; }
	private bool DebugEnabled { get; }
	private bool Quiet { get; set; }
	private Stopwatch ProgressStopWatch { get; } = Stopwatch.StartNew();
	private long MinProgressUpdateInterval { get; } = 100; // ms
	internal string DebugLinePrefix { get; set; } = "[DEBUG] ";
	internal string ErrorLinePrefix { get; set; } = "[ERROR] ";

	public ConsoleLogger(IConsoleAbstraction consoleAbstraction, bool debugEnabled =false)
	{
		ConsoleAbstraction = consoleAbstraction;
		DebugEnabled = debugEnabled;
	}

	public async Task Debug(string message)
	{
		if (!DebugEnabled)
			return;
		await Info(message);
	}

	public async Task DebugLine(string message)
	{
		if (!DebugEnabled)
			return;
		await InfoLine($"{DebugLinePrefix}{message}");
	}

	public async Task Info(string message)
	{
		if (Quiet)
			return;
		await ConsoleAbstraction.ConsoleOutWriteAsync(message);
	}

	public async Task InfoLine(string message = null)
	{
		if (Quiet)
			return;
		await ConsoleAbstraction.ConsoleOutWriteLineAsync(message);
	}

	public async Task StartTextProgress(string message)
	{
		if (Quiet)
			return;
		await Info($"\r{message}");
	}

	public async Task ReportTextProgress(double percentage, string prefixText)
	{
		if (Quiet)
			return;
		FormattableString formattableString = $"\r{prefixText}{percentage:F}%";
		async Task WriteTextProgress() => await Info(formattableString.ToString(CultureInfo.InvariantCulture).PadRight(ConsoleAbstraction.GetWindowWidth()));

		if (Math.Abs(percentage - 100) < double.Epsilon)
		{
			await WriteTextProgress();
			return;
		}

		// make sure updates are not to often - yes we skip values here
		if (ProgressStopWatch.ElapsedMilliseconds < MinProgressUpdateInterval)
			return;

		ProgressStopWatch.Restart();
		await WriteTextProgress();
	}

	public async Task FinishTextProgress(string message)
	{
		if (Quiet)
			return;
		await InfoLine($"\r{message}".PadRight(ConsoleAbstraction.GetWindowWidth()));
	}

	public async Task Error(string message)
	{
		ConsoleAbstraction.SetForegroundColor(ConsoleColor.Red);
		await ConsoleAbstraction.ConsoleErrorWriteAsync(message);
		ConsoleAbstraction.ResetColor();
	}

	public async Task ErrorLine(string message)
	{
		ConsoleAbstraction.SetForegroundColor(ConsoleColor.Red);
		await ConsoleAbstraction.ConsoleErrorWriteLineAsync($"{ErrorLinePrefix}{message}");
		ConsoleAbstraction.ResetColor();
	}

	public void SetQuiet(bool quiet = true)
	{
		Quiet = quiet;
	}
}