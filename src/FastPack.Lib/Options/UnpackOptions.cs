using System;
using System.Diagnostics.CodeAnalysis;

namespace FastPack.Lib.Options;

[ExcludeFromCodeCoverage]
public class UnpackOptions : FilterOptions, IOptions
{
	private bool _showProgress = Environment.UserInteractive && !Console.IsOutputRedirected;
	public string InputFilePath { get; set; }
	public string OutputDirectoryPath { get; set; }
	public int? MaxDegreeOfParallelism { get; set; }
	public bool DryRun { get; set; }
	public bool DetailedDryRun { get; set; }
	public OutputFormat DryRunOutputFormat { get; set; }
	public bool PrettyPrint { get; set; }
	public bool QuietMode { get; set; }
	public bool ShowProgress
	{
		get => _showProgress;
		set => _showProgress = value && Environment.UserInteractive && !Console.IsOutputRedirected;
	}
	public bool RestoreDates { get; set; } = true;
	public bool RestorePermissions { get; set; } = true;
	public bool IgnoreDiskSpaceCheck { get; set; }
	public bool OptimizeForCopyOnWriteFilesystem { get; set; }
}