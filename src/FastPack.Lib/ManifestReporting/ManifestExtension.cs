using System;
using System.Collections.Generic;
using System.Linq;
using FastPack.Lib.Compression;
using FastPack.Lib.ManifestManagement;

namespace FastPack.Lib.ManifestReporting;

internal static class ManifestExtension
{
	public static ManifestReport CreateManifestReport(this Manifest manifest, IFileCompressorFactory fileCompressorFactory, bool showCompressedSize, bool showDetailed)
	{
		string compressionLevelName = fileCompressorFactory.GetCompressor(manifest.CompressionAlgorithm).GetCompressionLevelValuesWithNames()[manifest.CompressionLevel];
		ManifestReport manifestReport = new() {
			Version = manifest.Version,
			Created = manifest.CreationDateUtc,
			FilesystemDatesIncluded = manifest.MetaDataOptions.HasFlag(MetaDataOptions.IncludeFileSystemDates),
			FilesystemPermissionsIncluded = manifest.MetaDataOptions.HasFlag(MetaDataOptions.IncludeFileSystemPermissions),
			HashingAlgorithm = manifest.HashAlgorithm.ToString(),
			CompressionAlgorithm = manifest.CompressionAlgorithm.ToString(),
			CompressionLevelName = compressionLevelName,
			CompressionLevel = manifest.CompressionLevel,
			Comment = manifest.Comment,
			FilesCount = manifest.Entries.Where(e => e.Type == EntryType.File).Sum(e => e.FileSystemEntries.Count),
			FilesSizeUncompressed = manifest.Entries.Sum(e => e.OriginalSize * e.FileSystemEntries.Count),
			UniqueFilesCount = manifest.Entries.Count(e => e.Type == EntryType.File),
			UniqueFilesSizeUncompressed = manifest.Entries.Sum(e => e.OriginalSize),
			FoldersCount = manifest.Entries.Where(e => e.Type == EntryType.Directory).Sum(e => e.FileSystemEntries.Count)
		};
		if (showCompressedSize)
		{
			manifestReport.FilesSizeCompressed = manifest.Entries.Sum(e => e.DataSize * e.FileSystemEntries.Count);
			manifestReport.UniqueFilesSizeCompressed = manifest.Entries.Sum(e => e.DataSize);
		}

		if (showDetailed)
		{
			manifestReport.Folders = new List<ManifestReportEntry>();
			manifestReport.Files = new List<ManifestReportEntry>();
			foreach (ManifestFileSystemEntry entry in manifest.Entries.Where(x => x.Type == EntryType.Directory).SelectMany(x => x.FileSystemEntries).OrderBy(x => x.RelativePath))
				manifestReport.Folders.Add(CreateManifestReportEntry(entry, manifest, false));
			foreach ((ManifestEntry manifestEntry, ManifestFileSystemEntry fileSystemEntry) in manifest.Entries.Where(x => x.Type == EntryType.File).SelectMany(x => x.FileSystemEntries.Select(y => (ManifestEntry: x, FileSystemEntry: y))).OrderBy(x => x.FileSystemEntry.RelativePath))
				manifestReport.Files.Add(CreateManifestReportEntry(fileSystemEntry, manifest, showCompressedSize, manifestEntry));
		}
		return manifestReport;
	}

	private static ManifestReportEntry CreateManifestReportEntry(ManifestFileSystemEntry fileSystemEntry, Manifest manifest, bool showCompressedSize, ManifestEntry manifestEntry = null)
	{
		ManifestReportEntry manifestReportEntry = new() {
			RelativePath = fileSystemEntry.RelativePath
		};

		if (manifestEntry != null)
		{
			manifestReportEntry.Hash = manifestEntry.Hash;
			if (showCompressedSize)
				manifestReportEntry.SizeCompressed = manifestEntry.DataSize;
			manifestReportEntry.SizeUncompressed = manifestEntry.OriginalSize;
		}

		if (manifest.MetaDataOptions.HasFlag(MetaDataOptions.IncludeFileSystemDates))
		{
			manifestReportEntry.Created = fileSystemEntry.CreationDateUtc;
			manifestReportEntry.LastAccess = fileSystemEntry.LastAccessDateUtc;
			manifestReportEntry.LastWrite = fileSystemEntry.LastWriteDateUtc;
		}

		if (manifest.MetaDataOptions.HasFlag(MetaDataOptions.IncludeFileSystemPermissions))
			manifestReportEntry.Permissions = Convert.ToString((uint)fileSystemEntry.FilePermissions!.Value, 8);

		return manifestReportEntry;
	}
}