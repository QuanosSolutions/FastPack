using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FastPack.Lib.Compression;
using FastPack.Lib.Logging;
using FastPack.Lib.ManifestManagement;
using FastPack.Lib.ManifestManagement.Serialization;
using FastPack.Lib.ManifestReporting;
using FastPack.Lib.Options;
using FastPack.Lib.TypeExtensions;

namespace FastPack.Lib.Unpackers;

internal class ArchiveUnpackerV1 : Unpacker
{
	private ILogger Logger { get; }

	public ArchiveUnpackerV1(ILogger logger)
	{
		Logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	[ExcludeFromCodeCoverage]
	internal IArchiveSerializerFactory SerializerFactory { get; set; } = new ArchiveSerializerFactory();
	internal IFileCompressorFactory FileCompressorFactory { get; set; } = new FileCompressorFactory();
	internal Lazy<IManifestReporterFactory> ManifestReporterFactory { get; set; } = new(() => new ManifestReporterFactory());

	public override Task<Manifest> GetManifestFromStream(Stream inputStream)
	{
		return SerializerFactory.GetFileReader(1).ReadManifest(inputStream);
	}

	public override async Task<int> Extract(string inputFile, UnpackOptions options)
	{
		Stopwatch overallStopWatch = Stopwatch.StartNew();
		Stopwatch currentStopwatch = Stopwatch.StartNew();
		
		// Beware: Create output directory before checking CoW support because on Linux the folder must already exist 
		// to be able to retrieve its volume.
		if (!Directory.Exists(options.OutputDirectoryPath))
			Directory.CreateDirectory(options.OutputDirectoryPath);

		bool isCopyOnWriteEnabled = IsCopyOnWriteEnabled(options);

		await Logger.InfoLine($"Using {options.MaxDegreeOfParallelism} of {Environment.ProcessorCount} logical cores.");
		await Logger.InfoLine($"Using unpack algorithm optimized for copy-on-write filesystems: {isCopyOnWriteEnabled}");
		await Logger.InfoLine($"Unpacking '{inputFile}'...");
			
		await Logger.StartTextProgress("Reading manifest ...");
		Manifest manifest = await GetManifestFromFile(inputFile);
		await Logger.FinishTextProgress($"Got manifest in {currentStopwatch.Elapsed}.");

		currentStopwatch.Restart();
		await Logger.StartTextProgress("Filtering files and directories to extract...");
		FilterEntries(manifest.Entries, options);
		await Logger.FinishTextProgress($"Filtered files and directories to extract in {currentStopwatch.Elapsed}.");

		if (!options.IgnoreDiskSpaceCheck)
		{
			long? availableDiskSpace = await DiskSpaceInfo.GetAvailableSpaceForPathInBytes(options.OutputDirectoryPath, Logger);
			if (availableDiskSpace.HasValue)
			{
				long neededDiskSpace = manifest.Entries.Sum(e => e.OriginalSize * e.FileSystemEntries.Count);
				if (neededDiskSpace > availableDiskSpace.Value)
				{
					await Logger.ErrorLine($"There is not enough disk space available in '{options.OutputDirectoryPath}' to extract the archive. Needed space: {neededDiskSpace.GetBytesReadable()}, available space: {availableDiskSpace.Value.GetBytesReadable()}.");
					await Logger.InfoLine("You can disable the disk space check by setting the option.");
					return ErrorConstants.Unpack_Not_Enough_Disk_Space;
				}
			}
		}

		if (options.DryRun)
		{
			IManifestReporter manifestReporter = ManifestReporterFactory.Value.GetManifestReporter(options.DryRunOutputFormat, Logger);
			await manifestReporter.PrintReport(manifest, true, options.DetailedDryRun, options.PrettyPrint);
			return 0;
		}

		currentStopwatch.Restart();
		await Logger.StartTextProgress("Creating directories ...");
		List<ManifestFileSystemEntry> directories = manifest.Entries.Where(x => x.Type == EntryType.Directory).SelectMany(x => x.FileSystemEntries).ToList();
		await CreateDirectories(options, directories);
		await Logger.FinishTextProgress($"Created directories in {currentStopwatch.Elapsed}.");

		currentStopwatch.Restart();
		await Logger.StartTextProgress("Extracting files ...");
		await UnpackFiles(options, manifest.Entries.Where(x => x.Type == EntryType.File), manifest, inputFile, isCopyOnWriteEnabled);
		await Logger.FinishTextProgress($"Extracted files in {currentStopwatch.Elapsed}.");

		if (options.RestoreDates || options.RestorePermissions)
		{
			currentStopwatch.Restart();
			await Logger.StartTextProgress("Setting metadata of directories ...");
			await SetDirectoryMetaData(options, directories, manifest);
			await Logger.FinishTextProgress($"Set metadata of directories in {currentStopwatch.Elapsed}.");
			currentStopwatch.Restart();
		}

		await Logger.InfoLine($"Unpack finished in {overallStopWatch.Elapsed}.");
		return 0;
	}

	private async Task<Manifest> GetManifestFromFile(string inputFile)
	{
		await using Stream fileStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read, FileShare.Read, Constants.BufferSize, Constants.OpenFileStreamsAsync);
		return await GetManifestFromStream(fileStream);
	}

	private async Task CreateDirectories(UnpackOptions options, List<ManifestFileSystemEntry> directories)
	{
		if (directories.Count == 0)
			return;
		await Parallel.ForEachAsync(directories, new ParallelOptions { MaxDegreeOfParallelism = options.MaxDegreeOfParallelism!.Value }, async (target, _) => {
			string targetPath = Path.Combine(options.OutputDirectoryPath, target.RelativePath);
			Directory.CreateDirectory(targetPath);
			await Task.CompletedTask;
		});
	}

	private async Task SetDirectoryMetaData(UnpackOptions options, List<ManifestFileSystemEntry> directories, Manifest manifest)
	{
		if (directories.Count == 0)
			return;

		if (options.RestoreDates && manifest.MetaDataOptions.HasFlag(MetaDataOptions.IncludeFileSystemDates))
		{
			await Parallel.ForEachAsync(directories, async (target, _) => {
				string targetDirectory = Path.Combine(options.OutputDirectoryPath, target.RelativePath);
				Directory.SetLastWriteTimeUtc(targetDirectory, target.LastWriteDateUtc!.Value);
				Directory.SetLastAccessTimeUtc(targetDirectory, target.LastAccessDateUtc!.Value);
				Directory.SetCreationTimeUtc(targetDirectory, target.CreationDateUtc!.Value);
				await Task.CompletedTask;
			});
		}
		await SetDirectoryPermissionsOnUnixSystems(options, directories, manifest);
	}

	[ExcludeFromCodeCoverage]
	private async Task SetDirectoryPermissionsOnUnixSystems(UnpackOptions options, List<ManifestFileSystemEntry> directories, Manifest manifest)
	{
		if (!options.RestorePermissions ||
		    !manifest.MetaDataOptions.HasFlag(MetaDataOptions.IncludeFileSystemPermissions) ||
		    OperatingSystem.IsWindows())
			return;
		await Parallel.ForEachAsync(directories, async (target, _) =>
		{
			string targetDirectory = Path.Combine(options.OutputDirectoryPath, target.RelativePath);
			if (target.FilePermissions.HasValue)
			{
#pragma warning disable CA1416 // Validate platform compatibility: We checked above that we are not on windows
				File.SetUnixFileMode(targetDirectory, target.FilePermissions.Value);
#pragma warning restore CA1416
			}
			await Task.CompletedTask;
		});
	}

	private async Task UnpackFiles(UnpackOptions options, IEnumerable<ManifestEntry> manifestEntries, Manifest manifest, string inputFile, bool isCopyOnWriteEnabled)
	{
		IFileCompressor fileCompressor = FileCompressorFactory.GetCompressor(manifest.CompressionAlgorithm);
		
		List<ManifestEntry> allFileEntries = manifestEntries.Where(x => x.Type == EntryType.File).ToList();
		
		int filesProcessed = 0;
		IProgress<int> unpackProgress = new Progress<int>(current => ShowProgress(current, allFileEntries.Count, "Unpack progress: ").Wait());

		await Parallel.ForEachAsync(allFileEntries, new ParallelOptions {
				MaxDegreeOfParallelism = options.MaxDegreeOfParallelism.Value
		}, async (fileEntry, _) => {
			if (isCopyOnWriteEnabled)
				await DecompressManifestEntryCow(options, fileEntry, manifest, inputFile, fileCompressor);
			else
				await DecompressManifestEntry(options, fileEntry, manifest, inputFile, fileCompressor);

			if (!options.ShowProgress)
				return;

			Interlocked.Increment(ref filesProcessed);
			unpackProgress.Report(filesProcessed);
		});
	}

	private static async Task<FileStream[]> OpenFileStreams(IEnumerable<ManifestFileSystemEntry> entries, UnpackOptions options)
	{
		// Open FileStreams for every file. If opening of one file fails we need to dispose the files that were 
		// already opened successfully.
		List<FileStream> openedStreams = new();
		try
		{
			foreach (var entry in entries)
			{
				var path = Path.Combine(options.OutputDirectoryPath, entry.RelativePath);
				openedStreams.Add(File.Open(path, FileMode.Create, FileAccess.Write));
			}
		}
		catch
		{
			foreach (var openedStream in openedStreams)
			{
				await openedStream.DisposeAsync();
			}
			throw;
		}
		return openedStreams.ToArray();
	}
	
	private async Task DecompressManifestEntry(UnpackOptions options, ManifestEntry manifestEntry, Manifest manifest,
			string inputFile, IFileCompressor fileCompressor)
	{
		var fileStreams = await OpenFileStreams(manifestEntry.FileSystemEntries, options);
		try
		{
			const int decompressionChunkSize = 2 * 1024 * 1024;
			await ReadFromDataStream(inputFile, manifestEntry, async decompressionStream =>
					await fileCompressor.DecompressFileChunked(decompressionStream, decompressionChunkSize,
							async (memory) => {
								foreach (var fileStream in fileStreams)
								{
									await fileStream.WriteAsync(memory);
								}
							}));

			foreach (var entry in manifestEntry.FileSystemEntries)
			{
				string path = Path.Combine(options.OutputDirectoryPath, entry.RelativePath);
				SetMetadata(manifest, path, entry, options);
			}
		}
		finally
		{
			foreach (var fileStream in fileStreams)
			{
				await fileStream.DisposeAsync();
			}
		}
	}

	private async Task DecompressManifestEntryCow(UnpackOptions options, ManifestEntry manifestEntry, Manifest manifest,
			string inputFile, IFileCompressor fileCompressor)
	{
		ManifestFileSystemEntry firstEntry = manifestEntry.FileSystemEntries.First();

		string firstTargetFile = Path.Combine(options.OutputDirectoryPath, firstEntry.RelativePath);
		await using (FileStream fileStream = new(firstTargetFile, FileMode.Create, FileAccess.Write, FileShare.None, Constants.BufferSize, Constants.OpenFileStreamsAsync))
			await ReadFromDataStream(inputFile, manifestEntry, async decompressionStream => await fileCompressor.DecompressFile(decompressionStream, fileStream));

		foreach (ManifestFileSystemEntry entry in manifestEntry.FileSystemEntries.Skip(1))
		{
			string nextTargetFile = Path.Combine(options.OutputDirectoryPath, entry.RelativePath);
			File.Copy(firstTargetFile, nextTargetFile, true);
			SetMetadata(manifest, nextTargetFile, entry, options);
		}
		SetMetadata(manifest, firstTargetFile, firstEntry, options);
	}

	private async Task ShowProgress(int current, int total, string prefixText)
	{
		double percentage = (double)current / total * 100;
		await Logger.ReportTextProgress(percentage, prefixText);
	}

	private void SetMetadata(Manifest manifest, string targetFile, ManifestFileSystemEntry manifestFileSystemEntry, UnpackOptions options)
	{
		if (options.RestoreDates && manifest.MetaDataOptions.HasFlag(MetaDataOptions.IncludeFileSystemDates))
		{
			File.SetLastWriteTimeUtc(targetFile, manifestFileSystemEntry.LastWriteDateUtc!.Value);
			File.SetLastAccessTimeUtc(targetFile, manifestFileSystemEntry.LastAccessDateUtc!.Value);
			File.SetCreationTimeUtc(targetFile, manifestFileSystemEntry.CreationDateUtc!.Value);
		}

		SetPermissionsOnUnixSystems(manifest, targetFile, manifestFileSystemEntry, options);
	}

	[ExcludeFromCodeCoverage]
	private void SetPermissionsOnUnixSystems(Manifest manifest, string targetFile, ManifestFileSystemEntry manifestFileSystemEntry, UnpackOptions options)
	{
		if (!options.RestorePermissions ||
		    !manifest.MetaDataOptions.HasFlag(MetaDataOptions.IncludeFileSystemPermissions) ||
		    OperatingSystem.IsWindows())
			return;
		if (manifestFileSystemEntry.FilePermissions.HasValue)
		{
			File.SetUnixFileMode(targetFile, manifestFileSystemEntry.FilePermissions.Value);
		}
	}

	private async Task ReadFromDataStream(string inputFile, ManifestEntry manifestEntry, Func<Stream, Task> readFromStreamAction)
	{
		await using Stream fileStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read, FileShare.Read, Constants.BufferSize, Constants.OpenFileStreamsAsync);
		await using SubStream archiveSubStream = new(fileStream, manifestEntry.DataIndex, manifestEntry.DataSize);
		await readFromStreamAction(archiveSubStream);
	}

	private static bool IsCopyOnWriteEnabled(UnpackOptions options)
	{
		return options.OptimizeForCopyOnWriteFilesystem == OptimizeForCopyOnWriteFilesystem.On ||
				options.OptimizeForCopyOnWriteFilesystem == OptimizeForCopyOnWriteFilesystem.Auto &&
				CopyOnWriteDiskInfo.DirectorySupportsCopyOnWrite(options.OutputDirectoryPath);
	}
}