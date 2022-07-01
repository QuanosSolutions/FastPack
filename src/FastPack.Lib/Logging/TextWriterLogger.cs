using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace FastPack.Lib.Logging;

public class TextWriterLogger: ILogger
{
	public bool Quiet { get; set; }
	private TextWriter DebugTextWriter { get; }
	private TextWriter InfoTextWriter { get; }
	private TextWriter ErrorTextWriter { get; }
	internal string DebugLinePrefix { get; set; } = "[DEBUG] ";
	internal string ErrorLinePrefix { get; set; } = "[ERROR] ";

	public TextWriterLogger(TextWriter infoTextWriter, TextWriter debugTextWriter = null, TextWriter errorTextWriter = null)
	{
		InfoTextWriter = infoTextWriter;
		DebugTextWriter = debugTextWriter ?? infoTextWriter;
		ErrorTextWriter = errorTextWriter ?? infoTextWriter;
	}

	public virtual async Task Debug(string message)
	{
		if (Quiet)
			return;
		await DebugTextWriter.WriteAsync(message);
	}

	public virtual async Task DebugLine(string message)
	{
		if (Quiet)
			return;
		await DebugTextWriter.WriteLineAsync($"{DebugLinePrefix}{message}");
	}

	public virtual async Task Info(string message)
	{
		if (Quiet)
			return;
		await InfoTextWriter.WriteAsync(message);
	}

	public virtual async Task InfoLine(string message)
	{
		if (Quiet)
			return;
		await InfoTextWriter.WriteLineAsync(message);
	}

	public async Task StartTextProgress(string message)
	{
		if (Quiet)
			return;
		await InfoLine(message);
	}

	private readonly Stopwatch _progressStopWatch = Stopwatch.StartNew();
	protected virtual long MinProgressUpdateInterval => 100; // ms

	public async Task ReportTextProgress(double percentage, string prefixText)
	{
		if (Quiet)
			return;
		if (Math.Abs(percentage - 100) < double.Epsilon)
		{
			await WriteTextProgress(percentage, prefixText);
			return;
		}

		// make sure updates are not to often - yes we skip values here
		if (_progressStopWatch.ElapsedMilliseconds < MinProgressUpdateInterval)
			return;

		_progressStopWatch.Restart();
		await WriteTextProgress(percentage, prefixText);
	}

	public async Task FinishTextProgress(string message)
	{
		if (Quiet)
			return;
		await InfoLine(message);
	}

	protected virtual async Task WriteTextProgress(double percentage, string prefixText)
	{
		FormattableString formattableString = $"{prefixText}{percentage:F}%";
		await InfoLine(formattableString.ToString(CultureInfo.InvariantCulture));
	}

	public virtual async Task Error(string message)
	{
		await ErrorTextWriter.WriteAsync(message);
	}

	public virtual async Task ErrorLine(string message)
	{
		await ErrorTextWriter.WriteLineAsync($"{ErrorLinePrefix}{message}");
	}

	public void SetQuiet(bool quiet = true)
	{
		Quiet = quiet;
	}
}