using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using FastPack.Lib.Compression;
using FastPack.Lib.Hashing;
using FastPack.Lib.Logging;
using FastPack.Lib.ManifestManagement;
using FastPack.Lib.ManifestManagement.Serialization;
using FastPack.Lib.ManifestReporting;
using FastPack.Lib.Matching;
using FastPack.Lib.Options;
using FastPack.Lib.TypeExtensions;

namespace FastPack.Lib.Actions;

public class PackAction : IAction
{
	private ILogger Logger { get; }
	private PackOptions Options { get; }
	internal IArchiveSerializerFactory ArchiveSerializerFactory { get; set; } = new ArchiveSerializerFactory();
	internal IHashProviderFactory HashProviderFactory { get; set; } = new HashProviderFactory();
	internal IFileCompressorFactory FileCompressorFactory { get; set; } = new FileCompressorFactory();
	internal ITextMatchProviderFactory TextMatchProviderFactory { get; set; } = new TextMatchProviderFactory();
	internal Lazy<IManifestReporterFactory> ManifestReporterFactory { get; set; } = new(() => new ManifestReporterFactory());
	internal IFilter Filter { get; set; } = new Filter();

	public PackAction(ILogger logger, PackOptions options)
	{
		Logger = logger;
		Options = options;
	}

	public async Task<int> Run()
	{
		IFileCompressor compressor = await ValidateOptions();

		string outputDir = Path.GetDirectoryName(Options.OutputFilePath);
		if (!Directory.Exists(outputDir))
			Directory.CreateDirectory(outputDir!);

		Stopwatch overallStopWatch = Stopwatch.StartNew();
		Stopwatch currentStopwatch = Stopwatch.StartNew();

		await Logger.InfoLine($"Using {Options.MaxDegreeOfParallelism} of {Environment.ProcessorCount} logical cores.");
		await Logger.StartTextProgress("Determining files and directories ...");
		FileSystemInfo[] fileSystemInfos = new DirectoryInfo(Options.InputDirectoryPath).GetFileSystemInfos("*", SearchOption.AllDirectories);
		await Logger.FinishTextProgress($"Found files in {currentStopwatch.Elapsed}...");
		currentStopwatch.Restart();

		await Logger.StartTextProgress("Filtering files and directories ...");
		List<FileSystemInfo> filteredFileSystemInfos = Filter.Apply(fileSystemInfos, Options, Path.DirectorySeparatorChar, e => Path.GetRelativePath(Options.InputDirectoryPath, e.FullName), e => e is DirectoryInfo);
		await Logger.FinishTextProgress($"Filtered files and directories in {currentStopwatch.Elapsed}.");
		currentStopwatch.Restart();

		List<FileInfo> filteredFileInfos = filteredFileSystemInfos.OfType<FileInfo>().ToList();
		List<DirectoryInfo> filteredDirectoryInfos = filteredFileSystemInfos.OfType<DirectoryInfo>().ToList();

		// Calculate stream hashes and unique files...
		ConcurrentDictionary<string, string> dict = new();
		IHashProvider hashProvider = HashProviderFactory.GetHashProvider(Options.HashAlgorithm);
		int processedHashes = 0;

		await Logger.StartTextProgress($"Determining hashes of {filteredFileInfos.Count} files ...");
		IProgress<int> hashProgress = new Progress<int>(current => ShowProgress(current, filteredFileInfos.Count, "Hashing progress: ").Wait());
		await Parallel.ForEachAsync(filteredFileInfos, new ParallelOptions { MaxDegreeOfParallelism = Options.MaxDegreeOfParallelism!.Value }, async (file, _) =>
		{
			dict[file.FullName] = await CryptoUtil.CalculateFileHash(hashProvider, file.FullName);

			if (!Options.ShowProgress)
				return;

			Interlocked.Increment(ref processedHashes);
			hashProgress.Report(processedHashes);
		});
		await Logger.FinishTextProgress($"Determined hashes in {currentStopwatch.Elapsed}.");

		ushort compressionLevel = Options.CompressionLevel ?? compressor.GetDefaultCompressionLevel();

		Manifest manifest = new() {
			Comment = Options.Comment,
			CompressionAlgorithm = Options.CompressionAlgorithm,
			CompressionLevel = compressionLevel,
			CreationDateUtc = DateTime.UtcNow,
			HashAlgorithm = Options.HashAlgorithm,
			MetaDataOptions = MetaDataOptions.IncludeFileSystemDates,
			Version = Constants.CurrentManifestVersion
		};

		SetMetaDataOptionsForUnix(manifest);

		// calculate unique files
		currentStopwatch.Restart();
		await Logger.StartTextProgress("Determining unique files ...");
		List<ManifestEntry> manifestEntries = await CalculateUniqueFiles(Options, filteredFileInfos, dict, manifest);
		await Logger.FinishTextProgress($"Determined unique files in {currentStopwatch.Elapsed}.");

		currentStopwatch.Restart();
		await Logger.StartTextProgress("Determining directories ...");
		if (filteredDirectoryInfos.Any())
			manifestEntries.Add(GetManifestEntryDirectories(filteredDirectoryInfos, Options, manifest));
		await Logger.FinishTextProgress($"Determined directories in {currentStopwatch.Elapsed}.");

		currentStopwatch.Restart();
		manifest.Entries = manifestEntries;

		if (Options.DryRun)
		{
			IManifestReporter manifestReporter = ManifestReporterFactory.Value.GetManifestReporter(Options.DryRunOutputFormat, Logger);
			await manifestReporter.PrintReport(manifest, false, Options.DetailedDryRun, Options.PrettyPrint);
			return 0;
		}

		IArchiveFileWriter archiveFileWriter = ArchiveSerializerFactory.GetFileWriter(Constants.CurrentManifestVersion); // as soon as we have different versions here, pass them via command line and also here

		// we order descending here, because a large file at first will fully occupy the consumer (sequential file write)
		List<ManifestEntryWithCompressedStream> allFileEntries = manifestEntries.Where(x => x.Type == EntryType.File).OfType<ManifestEntryWithCompressedStream>().OrderByDescending(x => x.OriginalSize).ToList();

		// Write FUP file...
		await using (FileStream stream = new(Options.OutputFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None, Constants.BufferSize, Constants.OpenFileStreamsAsync))
		{
			await archiveFileWriter.WriteHeader(stream, manifest);

			// Math.Min(options.MaxMemory ?? 0, int.MaxValue) is used to force all files larger than 2 GB to be processed sequentially - Otherwise the MemoryStream would crash
			List<ManifestEntryWithCompressedStream> parallelFileEntries = allFileEntries.Where(e => e.OriginalSize <= Math.Min(Options.MaxMemory ?? 0, int.MaxValue)).ToList();
			List<ManifestEntryWithCompressedStream> sequentialFileEntries = allFileEntries.Where(e => e.OriginalSize > Math.Min(Options.MaxMemory ?? 0, int.MaxValue)).ToList();

			int filesProcessed = 0;
			IProgress<int> fileProgress = new Progress<int>(current => ShowProgress(current, allFileEntries.Count + 1, "Compression progress: ").Wait());
			// process parallel entries
			await LimitParallelProducerConsumer.ProcessData(parallelFileEntries,
				async manifestEntry => {
					manifestEntry.CompressedStream = await compressor.CompressFile(Options.InputDirectoryPath, manifestEntry.FileSystemEntries.First().RelativePath, compressionLevel);
					return manifestEntry.ToIEnumerable();
				},
				async manifestEntry => {
						// the stream is already compressed .. just write it to the zipstream
					manifestEntry.CompressedStream.Seek(0, SeekOrigin.Begin);
					await archiveFileWriter.WriteFileData(stream, manifestEntry, async outputStream => await manifestEntry.CompressedStream.CopyToAsync(outputStream));

						// free memory
					manifestEntry.CompressedStream = null;
					if (!Options.ShowProgress)
						return;

					Interlocked.Increment(ref filesProcessed);
					fileProgress.Report(filesProcessed);
				},
				(items, current) => {
					long currentSize = items.Sum(x => x.OriginalSize);
					return currentSize + current.OriginalSize <= Options.MaxMemory;
				},
				source => source.Hash.ToIEnumerable(),
				produced => produced.Hash,
				Options.MaxDegreeOfParallelism!.Value,
				1);

			// process sequential entries
			foreach (ManifestEntryWithCompressedStream manifestEntry in sequentialFileEntries)
			{
				await archiveFileWriter.WriteFileData(stream, manifestEntry, async outputStream => await compressor.CompressFile(Options.InputDirectoryPath, manifestEntry.FileSystemEntries.First().RelativePath, compressionLevel, outputStream));

				if (!Options.ShowProgress)
					continue;

				Interlocked.Increment(ref filesProcessed);
				fileProgress.Report(filesProcessed);
			}

			// reorder entries by their DataIndex to make sure that during extract we don't have to seek too far and too much
			manifest.Entries = manifest.Entries.OrderByDescending(e => e.DataIndex).ToList();
			await archiveFileWriter.WriteManifest(stream, manifest);

			if (Options.ShowProgress)
			{
				Interlocked.Increment(ref filesProcessed);
				fileProgress.Report(filesProcessed);
			}
		}

		await Logger.FinishTextProgress($"FastPack packed {allFileEntries.Count} unique files (of {filteredFileInfos.Count}) and {filteredDirectoryInfos.Count} directories in {currentStopwatch.Elapsed}.");
		await Logger.FinishTextProgress($"Finished in {overallStopWatch.Elapsed}.");

		return 0;
	}

	[ExcludeFromCodeCoverage]
	private static void SetMetaDataOptionsForUnix(Manifest manifest)
	{
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			manifest.MetaDataOptions |= MetaDataOptions.IncludeFileSystemPermissions;
	}

	private async Task<IFileCompressor> ValidateOptions()
	{
		if (Options.InputDirectoryPath == null)
			throw new InvalidOptionException("Please provide the input directory path.", nameof(Options.InputDirectoryPath), ErrorConstants.Pack_InputDirectoryPath_Missing);

		Options.InputDirectoryPath = Path.GetFullPath(Options.InputDirectoryPath);
		if (!Directory.Exists(Options.InputDirectoryPath))
			throw new InvalidOptionException($"Could not find input directory '{Options.InputDirectoryPath}'...", nameof(Options.InputDirectoryPath), ErrorConstants.Pack_InputDirectoryPath_NoFound);

		if (Options.OutputFilePath == null)
			throw new InvalidOptionException("Please provide the output file.", nameof(Options.OutputFilePath), ErrorConstants.Pack_OutputFilePath_Missing);

		Options.OutputFilePath = Path.GetFullPath(Options.OutputFilePath);
		Options.MaxDegreeOfParallelism ??= Environment.ProcessorCount;
		Options.MaxMemory ??= (long)Math.Floor(await MemoryInfo.GetAvailableMemoryInBytes(Logger) * 0.8); // the default is 80% of the available memory

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

		IFileCompressor compressor = FileCompressorFactory.GetCompressor(Options.CompressionAlgorithm);
		IDictionary<ushort, string> compressionLevelValuesWithNames = compressor.GetCompressionLevelValuesWithNames();

		if (Options.CompressionLevel.HasValue && !compressionLevelValuesWithNames.ContainsKey(Options.CompressionLevel.Value))
		{
			string validValuesText = string.Join(", ", compressionLevelValuesWithNames.Select(kv => $"{kv.Value} ({kv.Key})"));
			throw new InvalidOptionException($"The provided compression level '{Options.CompressionLevel}' is invalid. Valid values are: {validValuesText}", nameof(Options.CompressionLevel), ErrorConstants.Pack_Invalid_CompressionLevel);
		}

		return compressor;
	}

	private async Task ShowProgress(int current, int total, string prefixText)
	{
		double percentage = (double)current / total * 100;
		await Logger.ReportTextProgress(percentage, prefixText);
	}

	private ManifestEntry GetManifestEntryDirectories(List<DirectoryInfo> directoryInfos, PackOptions options, Manifest manifest)
	{
		ManifestEntry manifestEntry = new() {
			Type = EntryType.Directory,
			FileSystemEntries = new List<ManifestFileSystemEntry>(directoryInfos.Select(x =>
			{
				ManifestFileSystemEntry manifestFileSystemEntry = new() {
					RelativePath = Path.GetRelativePath(options.InputDirectoryPath, x.FullName).Replace(Path.DirectorySeparatorChar, Constants.FastpackManifestDirectorySeparatorChar),
					CreationDateUtc = x.CreationTimeUtc,
					LastAccessDateUtc = x.LastAccessTimeUtc,
					LastWriteDateUtc = x.LastWriteTimeUtc
				};
				if (manifest.MetaDataOptions.HasFlag(MetaDataOptions.IncludeFileSystemDates))
				{
					manifestFileSystemEntry.CreationDateUtc = x.CreationTimeUtc;
					manifestFileSystemEntry.LastAccessDateUtc = x.LastAccessTimeUtc;
					manifestFileSystemEntry.LastWriteDateUtc = x.LastWriteTimeUtc;
				}
				if (manifest.MetaDataOptions.HasFlag(MetaDataOptions.IncludeFileSystemPermissions) && !OperatingSystem.IsWindows())
					manifestFileSystemEntry.FilePermissions = File.GetUnixFileMode(x.FullName);
				return manifestFileSystemEntry;
			}))
		};
		return manifestEntry;
	}

	private async Task<List<ManifestEntry>> CalculateUniqueFiles(PackOptions options, IEnumerable<FileInfo> files, ConcurrentDictionary<string, string> filenameToHashDictionary, Manifest manifest)
	{
		ConcurrentDictionary<string, ManifestEntryWithCompressedStream> hashes = new();
		await Parallel.ForEachAsync(files, new ParallelOptions { MaxDegreeOfParallelism = options.MaxDegreeOfParallelism!.Value }, async (file, _) =>
		{
			string hash = filenameToHashDictionary[file.FullName];
			ManifestEntryWithCompressedStream entry = hashes.GetOrAdd(hash, fileHash =>
			{
				ManifestEntryWithCompressedStream manifestEntry = new() {
					Hash = fileHash,
					Type = EntryType.File,
					OriginalSize = file.Length,
				};
				return manifestEntry;
			});
			lock (entry.FileSystemEntries)
			{
				ManifestFileSystemEntry manifestFileSystemEntry = new() {
					RelativePath = Path.GetRelativePath(options.InputDirectoryPath, file.FullName).Replace(Path.DirectorySeparatorChar, Constants.FastpackManifestDirectorySeparatorChar)
				};
				if (manifest.MetaDataOptions.HasFlag(MetaDataOptions.IncludeFileSystemDates))
				{
					manifestFileSystemEntry.CreationDateUtc = file.CreationTimeUtc;
					manifestFileSystemEntry.LastAccessDateUtc = file.LastAccessTimeUtc;
					manifestFileSystemEntry.LastWriteDateUtc = file.LastWriteTimeUtc;
				}
				if (manifest.MetaDataOptions.HasFlag(MetaDataOptions.IncludeFileSystemPermissions) && !OperatingSystem.IsWindows())
					manifestFileSystemEntry.FilePermissions = File.GetUnixFileMode(file.FullName);
				entry.FileSystemEntries.Add(manifestFileSystemEntry);
			}
			await Task.CompletedTask;
		});

		return new List<ManifestEntry>(hashes.Values);
	}
}