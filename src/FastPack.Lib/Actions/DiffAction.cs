using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using FastPack.Lib.Diff;
using FastPack.Lib.Logging;
using FastPack.Lib.ManifestManagement;
using FastPack.Lib.Matching;
using FastPack.Lib.Options;
using FastPack.Lib.Unpackers;

namespace FastPack.Lib.Actions;

public class DiffAction : IAction
{
	private ILogger Logger { get; }
	public DiffOptions Options { get; }
	internal IProcessAbstraction ProcessAbstraction { get; set; } = new ProcessAbstraction();
	internal IArchiveUnpackerFactory ArchiveUnpackerFactory { get; set; } = new ArchiveUnpackerFactory();
	internal IDiffReporterFactory DiffReporterFactory { get; set; } = new DiffReporterFactory();
	internal ITextMatchProviderFactory TextMatchProviderFactory { get; set; } = new TextMatchProviderFactory();
	internal IFilter Filter { get; set; } = new Filter();

	public DiffAction(ILogger logger, DiffOptions options)
	{
		Logger = logger ?? throw new ArgumentNullException(nameof(logger));
		Options = options ?? throw new ArgumentNullException(nameof(options));
	}

	public async Task<int> Run()
	{
		ValidateOptions();

		await Logger.InfoLine("Calculating differences...");

		Manifest firstManifest = await GetManifestFromFile(Options.FirstFilePath);
		Manifest secondManifest = await GetManifestFromFile(Options.SecondFilePath);

		Dictionary<string, DiffEntry> firstManifestFileSystemEntryMapping = CreateDiffEntries(firstManifest);
		Dictionary<string, DiffEntry> secondManifestFileSystemEntryMapping = CreateDiffEntries(secondManifest);

		List<DiffEntry> removedEntries = firstManifestFileSystemEntryMapping.ExceptBy(secondManifestFileSystemEntryMapping.Keys, pair => pair.Key).Select(x => x.Value).ToList();
		List<DiffEntry> addedEntries = secondManifestFileSystemEntryMapping.ExceptBy(firstManifestFileSystemEntryMapping.Keys, pair => pair.Key).Select(x => x.Value).ToList();

		long totalChangedCount = addedEntries.Count + removedEntries.Count;

		DiffReport diffReport = new() {
			AddedCount = addedEntries.Count,
			RemovedCount = removedEntries.Count,
			AddedEntries = addedEntries.ToList(),
			RemovedEntries = removedEntries.ToList()
		};

		if (Options.DiffSettings.HasFlag(DiffOptions.DiffSetting.Size))
		{
			List<DiffEntryChange> changes = CreateSizeChangedEntries(firstManifestFileSystemEntryMapping, secondManifestFileSystemEntryMapping);
			totalChangedCount += changes.Count;
			diffReport.ChangedSizeEntries = changes;
			diffReport.ChangedSizeCount = changes.Count;
		}

		if (Options.DiffSettings.HasFlag(DiffOptions.DiffSetting.Date))
		{
			List<DiffEntryChange> changes = CreateDatesChangedEntries(firstManifestFileSystemEntryMapping, secondManifestFileSystemEntryMapping);
			totalChangedCount += changes.Count;
			diffReport.ChangedDatesEntries = changes;
			diffReport.ChangedDatesCount = changes.Count;
		}

		if (Options.DiffSettings.HasFlag(DiffOptions.DiffSetting.Permission))
		{
			List<DiffEntryChange> changes = CreatePermissionsChangedEntries(firstManifestFileSystemEntryMapping, secondManifestFileSystemEntryMapping);
			totalChangedCount += changes.Count;
			diffReport.ChangedPermissionsEntries = changes;
			diffReport.ChangedPermissionsCount = changes.Count;
		}

		diffReport.TotalChangedCount = totalChangedCount;

		IDiffReporter diffReporter = DiffReporterFactory.GetDiffReporter(Options.OutputFormat, Logger);
		await diffReporter.PrintReport(diffReport, Options.PrettyPrint);

		if (Options.ExtractionEnabled)
			await ExtractFiles(Options, diffReport.AddedEntries, diffReport.RemovedEntries, diffReport.ChangedSizeEntries);

		return 0;
	}

	private void ValidateOptions()
	{
		if (Options.FirstFilePath == null)
			throw new InvalidOptionException("Please provide the first file.", nameof(Options.FirstFilePath), ErrorConstants.Diff_FirstFilePath_Missing);

		Options.FirstFilePath = Path.GetFullPath(Options.FirstFilePath);
		if (!File.Exists(Options.FirstFilePath))
			throw new InvalidOptionException($"File not found: '{Options.FirstFilePath}'", nameof(Options.FirstFilePath),  ErrorConstants.Diff_FirstFilePath_NoFound);
		
		if (Options.SecondFilePath == null)
			throw new InvalidOptionException("Please provide the second file.", nameof(Options.SecondFilePath), ErrorConstants.Diff_SecondFilePath_Missing);

		Options.SecondFilePath = Path.GetFullPath(Options.SecondFilePath);
		if (!File.Exists(Options.SecondFilePath))
			throw new InvalidOptionException($"File not found: '{Options.SecondFilePath}'", nameof(Options.SecondFilePath), ErrorConstants.Diff_SecondFilePath_NoFound);

		if (Options.ExtractionEnabled)
			Options.ExtractionDirectoryPath = string.IsNullOrEmpty(Options.ExtractionDirectoryPath)
				? Path.Combine(Path.GetTempPath(), Path.GetRandomFileName())
				: Path.GetFullPath(Options.ExtractionDirectoryPath);

		if (Options.IncludeFilters.Any() || Options.ExcludeFilters.Any())
		{
			ITextMatchProvider textMatchProvider = TextMatchProviderFactory.GetProvider(Options.FilterType);

			Options.IncludeFilters.ForEach(filter => {
				if (!textMatchProvider.IsPatternValid(filter))
					throw new InvalidOptionException($"Invalid include filter: {filter}", nameof(Options.IncludeFilters), ErrorConstants.Pack_Invalid_IncludeFilter);
			});

			Options.ExcludeFilters.ForEach(filter => {
				if (!textMatchProvider.IsPatternValid(filter))
					throw new InvalidOptionException($"Invalid exclude filter: {filter}", nameof(Options.IncludeFilters), ErrorConstants.Pack_Invalid_ExcludeFilter);
			});
		}

		Options.QuietMode = Options.OutputFormat is OutputFormat.Json or OutputFormat.Xml;
	}

	private Dictionary<string, DiffEntry> CreateDiffEntries(Manifest firstManifest)
	{
		List<DiffEntry> diffEntries = firstManifest.Entries.SelectMany(g =>
			g.FileSystemEntries.Select(e =>
				new DiffEntry
				{
					Hash = g.Hash,
					Size = g.OriginalSize,
					RelativePath = e.RelativePath,
					Permissions = !e.FilePermissions.HasValue ? null : Convert.ToString((uint)e.FilePermissions.Value, 8),
					Created = e.CreationDateUtc,
					LastAccess = e.LastAccessDateUtc,
					LastWrite = e.LastWriteDateUtc
				})).ToList();
		diffEntries = Filter.Apply(diffEntries, Options, Path.DirectorySeparatorChar, e => e.RelativePath, _ => false);
		return diffEntries.ToDictionary(e => e.RelativePath, e => e);
	}

	private async Task ExtractFiles(DiffOptions options, List<DiffEntry> addedEntries, List<DiffEntry> removedEntries, List<DiffEntryChange> changedEntries)
	{
		await Logger.InfoLine($"Starting extraction to: {options.ExtractionDirectoryPath}");
		Directory.CreateDirectory(options.ExtractionDirectoryPath);

		List<string> addFilePaths = addedEntries.Select(p => p.RelativePath).ToList();
		List<string> removedFilePaths = removedEntries.Select(p => p.RelativePath).ToList();
		await UnpackFiles("added files", options.SecondFilePath, Path.Join(options.ExtractionDirectoryPath, "Added"), addFilePaths);
		await UnpackFiles("removed files", options.FirstFilePath, Path.Join(options.ExtractionDirectoryPath, "Removed"), removedFilePaths);

		if (changedEntries != null)
		{
			List<(string First, string Second)> changedFilePaths = changedEntries.Select(p => (First: p.First.RelativePath, Second: p.First.RelativePath)).ToList();
			List<string> changeFilePathsOld = changedFilePaths.Select(p => p.First).ToList();
			List<string> changeFilePathsNew = changedFilePaths.Select(p => p.Second).ToList();

			await UnpackFiles("changed files - old ones", options.FirstFilePath, Path.Join(options.ExtractionDirectoryPath, "Changed", "1"), changeFilePathsOld);
			await UnpackFiles("changed files - new ones", options.SecondFilePath, Path.Join(options.ExtractionDirectoryPath, "Changed", "2"), changeFilePathsNew);
		}

		await Logger.InfoLine("Finished extraction - Opening target folder..");
		await Logger.InfoLine();
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			ProcessAbstraction.Start("explorer.exe", options.ExtractionDirectoryPath);
	}

	private async Task UnpackFiles(string name, string fupFilePath, string targetFolder, List<string> filePaths)
	{
		if (!filePaths.Any())
			return;

		UnpackOptions unpackOptions = new() {
			InputFilePath = fupFilePath,
			OutputDirectoryPath = targetFolder,
			FilterType = TextMatchProviderType.Glob
		};

		unpackOptions.IncludeFilters.AddRange(filePaths);
		await Logger.InfoLine($"Extracting {name} ..");
		await new UnpackAction(Logger, unpackOptions).Run();
	}

	private List<DiffEntryChange> CreateSizeChangedEntries(Dictionary<string, DiffEntry> firstDiffEntryMapping, Dictionary<string, DiffEntry> secondDiffEntryMapping)
	{
		return CreatedChangedEntries(firstDiffEntryMapping, secondDiffEntryMapping, diffEntryChange => diffEntryChange.First.Size != diffEntryChange.Second.Size);
	}

	private List<DiffEntryChange> CreateDatesChangedEntries(Dictionary<string, DiffEntry> firstDiffEntryMapping, Dictionary<string, DiffEntry> secondDiffEntryMapping)
	{
		return CreatedChangedEntries(firstDiffEntryMapping, secondDiffEntryMapping, diffEntryChange => diffEntryChange.First.Created != diffEntryChange.Second.Created || diffEntryChange.First.LastAccess != diffEntryChange.Second.LastAccess || diffEntryChange.First.LastWrite != diffEntryChange.Second.LastWrite);
	}

	private List<DiffEntryChange> CreatePermissionsChangedEntries(Dictionary<string, DiffEntry> firstDiffEntryMapping, Dictionary<string, DiffEntry> secondDiffEntryMapping)
	{
		return CreatedChangedEntries(firstDiffEntryMapping, secondDiffEntryMapping, diffEntryChange => diffEntryChange.First.Permissions != diffEntryChange.Second.Permissions);
	}

	private List<DiffEntryChange> CreatedChangedEntries(Dictionary<string, DiffEntry> firstDiffEntryMapping, Dictionary<string, DiffEntry> secondDiffEntryMapping, Func<DiffEntryChange, bool> predicate)
	{
		Dictionary<string, DiffEntry> commonEntries = firstDiffEntryMapping.IntersectBy(secondDiffEntryMapping.Keys, pair => pair.Key).ToDictionary(p => p.Key, p => p.Value);
		return commonEntries.Keys.Select(k => new DiffEntryChange { First = firstDiffEntryMapping[k], Second = secondDiffEntryMapping[k] }).Where(predicate).ToList();
	}

	private async Task<Manifest> GetManifestFromFile(string filePath)
	{
		await using FileStream fileStream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, Constants.BufferSize, Constants.OpenFileStreamsAsync);
		return await (await ArchiveUnpackerFactory.GetUnpacker(fileStream, Logger)).GetManifestFromStream(fileStream);
	}
}