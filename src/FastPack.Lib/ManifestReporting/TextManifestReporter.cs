using System.Globalization;
using System.Threading.Tasks;
using FastPack.Lib.Compression;
using FastPack.Lib.Logging;
using FastPack.Lib.ManifestManagement;

namespace FastPack.Lib.ManifestReporting;

internal class TextManifestReporter: IManifestReporter
{
	private ILogger Logger { get; }
	private IFileCompressorFactory FileCompressorFactory { get; } = new FileCompressorFactory();

	public TextManifestReporter(ILogger logger)
	{
		Logger = logger;
	}

	public async Task PrintReport(Manifest manifest, bool showCompressedSize, bool showDetailed, bool prettyPrint)
	{
		ManifestReport manifestReport = manifest.CreateManifestReport(FileCompressorFactory, showCompressedSize, showDetailed);
		await PrintReport(manifestReport);
	}

	private async Task PrintReport(ManifestReport manifestReport)
	{
		await Logger.InfoLine();
		await Logger.InfoLine("Manifest Info");
		await Logger.InfoLine("========================================================");
		await Logger.InfoLine($"  Version: {manifestReport.Version}");
		await Logger.InfoLine($"  Created: {manifestReport.Created.ToLocalTime().ToString(CultureInfo.InvariantCulture)}");
		await Logger.InfoLine("  Meta data:");
		await Logger.InfoLine($"    Filesystem dates included: {FormatBoolean(manifestReport.FilesystemDatesIncluded)}");
		await Logger.InfoLine($"    Filesystem permissions included: {FormatBoolean(manifestReport.FilesystemPermissionsIncluded)}");
		await Logger.InfoLine($"  Hashing algorithm: {manifestReport.HashingAlgorithm}");
		await Logger.InfoLine($"  Compression algorithm: {manifestReport.CompressionAlgorithm}");
		await Logger.InfoLine($"  Compression level: {manifestReport.CompressionLevelName} ({manifestReport.CompressionLevel})");
		await Logger.InfoLine($"  Comment: {manifestReport.Comment}");
		await Logger.InfoLine($"  Files: {manifestReport.FilesCount}");
		if (manifestReport.FilesSizeCompressed.HasValue)
			await Logger.InfoLine($"  Size of files (compressed): {manifestReport.FilesSizeCompressed.Value} bytes");
		await Logger.InfoLine($"  Size of files (uncompressed): {manifestReport.FilesSizeUncompressed} bytes");
		await Logger.InfoLine($"  Unique files: {manifestReport.UniqueFilesCount}");
		if (manifestReport.UniqueFilesSizeCompressed.HasValue)
			await Logger.InfoLine($"  Size of unique files (compressed): {manifestReport.UniqueFilesSizeCompressed.Value} bytes");
		await Logger.InfoLine($"  Size of unique files (uncompressed): {manifestReport.UniqueFilesSizeUncompressed} bytes");
		await Logger.InfoLine($"  Folders: {manifestReport.FoldersCount}");
		if (manifestReport.Folders != null)
		{
			await Logger.InfoLine();
			await Logger.InfoLine("Folders");
			await Logger.InfoLine("========================================================");
			foreach (ManifestReportEntry folder in manifestReport.Folders)
				await PrintReport(folder);
		}
		if (manifestReport.Files != null)
		{
			await Logger.InfoLine();
			await Logger.InfoLine("Files");
			await Logger.InfoLine("========================================================");
			foreach (ManifestReportEntry file in manifestReport.Files)
				await PrintReport(file);
		}
	}

	private async Task PrintReport(ManifestReportEntry entry)
	{
		await Logger.InfoLine("  " + entry.RelativePath);
		if (entry.Hash != null)
			await Logger.InfoLine($"    Hash: {entry.Hash}");
		if (entry.SizeCompressed.HasValue)
			await Logger.InfoLine($"    Size (compressed): {entry.SizeCompressed.Value} bytes");
		if (entry.SizeUncompressed.HasValue)
			await Logger.InfoLine($"    Size (uncompressed): {entry.SizeUncompressed.Value} bytes");
		if (entry.Created.HasValue)
			await Logger.InfoLine($"    Created: {entry.Created.Value.ToLocalTime().ToString(CultureInfo.InvariantCulture)}");
		if (entry.LastAccess.HasValue)
			await Logger.InfoLine($"    Last Access: {entry.LastAccess.Value.ToLocalTime().ToString(CultureInfo.InvariantCulture)}");
		if (entry.LastWrite.HasValue)
			await Logger.InfoLine($"    Last Write: {entry.LastWrite.Value.ToLocalTime().ToString(CultureInfo.InvariantCulture)}");
		if (entry.Permissions != null)
			await Logger.InfoLine($"    Permissions: {entry.Permissions}");
	}

	private string FormatBoolean(bool value)
	{
		return value ? "Yes" : "No";
	}
}