using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
	private FilePermissionInfo FilePermissionInfo { get; } = new();
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

		await Logger.InfoLine($"Using {options.MaxDegreeOfParallelism} of {Environment.ProcessorCount} logical cores.");
		await Logger.InfoLine($"Unpacking '{inputFile}'...");
			
		await Logger.StartTextProgress("Reading manifest ...");
		Manifest manifest = await GetManifestFromFile(inputFile);
		await Logger.FinishTextProgress($"Got manifest in {currentStopwatch.Elapsed}.");

		if (!Directory.Exists(options.OutputDirectoryPath))
			Directory.CreateDirectory(options.OutputDirectoryPath);

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
		await UnpackFiles(options, manifest.Entries.Where(x => x.Type == EntryType.File), manifest, inputFile);
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
		    (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && !RuntimeInformation.IsOSPlatform(OSPlatform.OSX)))
			return;
		await Parallel.ForEachAsync(directories, async (target, _) =>
		{
			string targetDirectory = Path.Combine(options.OutputDirectoryPath, target.RelativePath);
			FilePermissionInfo.SetFilePermissions(targetDirectory, target.FilePermissions!.Value);
			await Task.CompletedTask;
		});
	}

	private async Task UnpackFiles(UnpackOptions options, IEnumerable<ManifestEntry> manifestEntries, Manifest manifest, string inputFile)
	{
		IFileCompressor fileCompressor = FileCompressorFactory.GetCompressor(manifest.CompressionAlgorithm);
		
		List<ManifestEntry> allFileEntries = manifestEntries.Where(x => x.Type == EntryType.File).ToList();
		
		int filesProcessed = 0;
		IProgress<int> unpackProgress = new Progress<int>(current => ShowProgress(current, allFileEntries.Sum(e => e.FileSystemEntries.Count), "Unpack progress: ").Wait());
        
        await Parallel.ForEachAsync(allFileEntries, new ParallelOptions { MaxDegreeOfParallelism = options.MaxDegreeOfParallelism.Value }, async (fileEntry, _) =>
        {
	        ManifestFileSystemEntry firstEntry = fileEntry.FileSystemEntries.First();

	        string firstTargetFile = Path.Combine(options.OutputDirectoryPath, firstEntry.RelativePath);
	        await using (FileStream fileStream = new(firstTargetFile, FileMode.Create, FileAccess.Write, FileShare.None, Constants.BufferSize, Constants.OpenFileStreamsAsync))
		        await ReadFromDataStream(inputFile, fileEntry, async decompressionStream => await fileCompressor.DecompressFile(decompressionStream, fileStream));

	        if (options.ShowProgress)
	        {
		        Interlocked.Increment(ref filesProcessed);
		        unpackProgress.Report(filesProcessed);
	        }

	        foreach (ManifestFileSystemEntry entry in fileEntry.FileSystemEntries.Skip(1))
	        {
		        string nextTargetFile = Path.Combine(options.OutputDirectoryPath, entry.RelativePath);
		        File.Copy(firstTargetFile, nextTargetFile, true);
		        SetMetadata(manifest, nextTargetFile, entry, options);
		     
		        if (!options.ShowProgress)
			        continue;
		        
		        Interlocked.Increment(ref filesProcessed);
		        unpackProgress.Report(filesProcessed);
	        }
	        
	        SetMetadata(manifest, firstTargetFile, firstEntry, options);
        });
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
		    (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux) &&
		     !RuntimeInformation.IsOSPlatform(OSPlatform.OSX)))
			return;
		FilePermissionInfo.SetFilePermissions(targetFile, manifestFileSystemEntry.FilePermissions!.Value);
	}

	private async Task ReadFromDataStream(string inputFile, ManifestEntry manifestEntry, Func<Stream, Task> readFromStreamAction)
	{
		await using Stream fileStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read, FileShare.Read, Constants.BufferSize, Constants.OpenFileStreamsAsync);
		await using SubStream archiveSubStream = new(fileStream, manifestEntry.DataIndex, manifestEntry.DataSize);
		await readFromStreamAction(archiveSubStream);
	}
}