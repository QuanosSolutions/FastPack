using System.Diagnostics.CodeAnalysis;

namespace FastPack.Lib.Options;

[ExcludeFromCodeCoverage]
public class UnpackOptions : FilterOptions, IOptions
{
	public string InputFilePath { get; set; }
	public string OutputDirectoryPath { get; set; }
	public int? MaxDegreeOfParallelism { get; set; }
	public long? MaxMemory { get; set; }
	public bool DryRun { get; set; }
	public bool DetailedDryRun { get; set; }
	public OutputFormat DryRunOutputFormat { get; set; }
	public bool PrettyPrint { get; set; }
	public bool QuietMode { get; set; }
	public bool ShowProgress { get; set; } = true;
	public bool RestoreDates { get; set; } = true;
	public bool RestorePermissions { get; set; } = true;
	public bool IgnoreDiskSpaceCheck { get; set; }
}