using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FastPack.Lib.Logging;
using FastPack.Lib.TypeExtensions;

namespace FastPack.Lib.Diff;

internal class TextDiffReporter : IDiffReporter
{
	private ILogger Logger { get; }
	private IConsoleAbstraction ConsoleAbstraction { get; }

	public TextDiffReporter(ILogger logger, IConsoleAbstraction consoleAbstraction)
	{
		Logger = logger;
		ConsoleAbstraction = consoleAbstraction;
	}

	public async Task PrintReport(DiffReport diffReport, bool prettyPrint)
	{
		if (diffReport.AddedEntries.Any())
			await PrintChangedEntries("Added", "+", diffReport.AddedEntries, ConsoleColor.Green);

		if (diffReport.RemovedEntries.Any())
			await PrintChangedEntries("Removed", "-", diffReport.RemovedEntries, ConsoleColor.Red);

		if (diffReport.ChangedSizeEntries != null && diffReport.ChangedSizeEntries.Any())
			await PrintSizeChangedEntries(diffReport.ChangedSizeEntries);

		if (diffReport.ChangedDatesEntries != null && diffReport.ChangedDatesEntries.Any())
			await PrintDatesChangedEntries(diffReport.ChangedDatesEntries);

		if (diffReport.ChangedPermissionsEntries != null && diffReport.ChangedPermissionsEntries.Any())
			await PrintPermissionsChangedEntries(diffReport.ChangedPermissionsEntries);

		await Logger.InfoLine();

		if (diffReport.TotalChangedCount == 0)
			await Logger.InfoLine("Diff finished with no differences.");
		else if (diffReport.TotalChangedCount == 1)
			await Logger.InfoLine("Diff finished with 1 difference.");
		else
			await Logger.InfoLine($"Diff finished with {diffReport.TotalChangedCount} differences.");
	}

	private async Task PrintChangedEntries(string type, string marker, List<DiffEntry> entries, ConsoleColor? lineColor)
	{
		await Logger.InfoLine();
		await Logger.InfoLine($"{type} [{entries.Count}]");

		Dictionary<string, List<DiffEntry>> groupedEntries = entries.GroupBy(e => Path.GetFileName(e.RelativePath), e => e).ToDictionary(g => g.Key, g => g.Select(e => e).ToList());

		foreach (KeyValuePair<string, List<DiffEntry>> groupEntry in groupedEntries.OrderBy(e => e.Value.First().RelativePath))
		{
			if (groupEntry.Value.Count > 1)
				await WriteFileLine($"{marker} [{groupEntry.Key}]", lineColor);

			foreach (string lineContent in groupEntry.Value.OrderBy(e => e.RelativePath).Select(manifestFileSystemEntry => $"{marker} {manifestFileSystemEntry.RelativePath}").Select(lineContent => groupEntry.Value.Count > 1 ? $"\t{lineContent}" : lineContent))
				await WriteFileLine(lineContent, lineColor);
		}
	}

	private async Task PrintSizeChangedEntries(List<DiffEntryChange> entries)
	{
		await Logger.InfoLine();
		await Logger.InfoLine($"Size Changed [{entries.Count}]");

		Dictionary<string, List<DiffEntryChange>> groupedChangedEntries = entries.GroupBy(p => p.Second.Hash).ToDictionary(g => g.Key, g => g.ToList());

		foreach (KeyValuePair<string, List<DiffEntryChange>> groupEntry in groupedChangedEntries)
		{
			DiffEntryChange firstEntry = groupEntry.Value.First();

			string sizeText = $"[{firstEntry.First.Size.GetBytesReadable()} => {firstEntry.Second.Size.GetBytesReadable()}, {(firstEntry.Second.Size - firstEntry.First.Size).GetBytesReadable()}]";

			if (groupEntry.Value.Count > 1)
				await WriteFileLine($"- [{Path.GetFileName(firstEntry.First.RelativePath)}] {sizeText}", ConsoleColor.Yellow);

			foreach (DiffEntryChange diffEntry in groupEntry.Value)
			{
				string lineContent = $"- {diffEntry.Second.RelativePath}";
				lineContent = groupEntry.Value.Count > 1 ? $"\t{lineContent}" : $"{lineContent} {sizeText}";
				await WriteFileLine(lineContent, ConsoleColor.Yellow);
			}
		}
	}

	private async Task PrintDatesChangedEntries(List<DiffEntryChange> entries)
	{
		await Logger.InfoLine();
		await Logger.InfoLine($"Dates Changed [{entries.Count}]");

		foreach (DiffEntryChange diffEntryChange in entries)
		{
			await WriteFileLine($"- {diffEntryChange.Second.RelativePath}", ConsoleColor.Yellow);
			if (diffEntryChange.First.Created != diffEntryChange.Second.Created)
				await WriteFileLine($"\t- Creation Date: {GetDatesReadable(diffEntryChange.First.Created)} => {GetDatesReadable(diffEntryChange.Second.Created)}", ConsoleColor.Yellow);
			if (diffEntryChange.First.LastAccess != diffEntryChange.Second.LastAccess)
				await WriteFileLine($"\t- Last Access Date: {GetDatesReadable(diffEntryChange.First.LastAccess)} => {GetDatesReadable(diffEntryChange.Second.LastAccess)}", ConsoleColor.Yellow);
			if (diffEntryChange.First.LastWrite != diffEntryChange.Second.LastWrite)
				await WriteFileLine($"\t- Last Write Date: {GetDatesReadable(diffEntryChange.First.LastWrite)} => {GetDatesReadable(diffEntryChange.Second.LastWrite)}", ConsoleColor.Yellow);
		}
	}

	private async Task PrintPermissionsChangedEntries(List<DiffEntryChange> entries)
	{
		await Logger.InfoLine();
		await Logger.InfoLine($"Permissions Changed [{entries.Count}]");

		foreach (DiffEntryChange diffEntryChange in entries)
			await WriteFileLine($"- {diffEntryChange.Second.RelativePath} [{GetPermissionReadable(diffEntryChange.First.Permissions)} => {GetPermissionReadable(diffEntryChange.Second.Permissions)}]", ConsoleColor.Yellow);
	}

	private async Task WriteFileLine(string lineContent, ConsoleColor? lineColor)
	{
		if (lineColor.HasValue && Logger is ConsoleLogger && ConsoleAbstraction != null)
			ConsoleAbstraction.SetForegroundColor(lineColor.Value);
		try
		{
			await Logger.InfoLine(lineContent);
		}
		finally
		{
			if (lineColor.HasValue && Logger is ConsoleLogger && ConsoleAbstraction != null)
				ConsoleAbstraction.ResetColor();
		}
	}

	private string GetDatesReadable(DateTime? value)
	{
		if (!value.HasValue)
			return "[null]";
		return value.Value.ToString(CultureInfo.InvariantCulture);
	}
	
	private string GetPermissionReadable(string value)
	{
		if (value == null)
			return "[null]";
		return value;
	}
}