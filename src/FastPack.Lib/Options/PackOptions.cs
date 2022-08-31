using System;
using System.Diagnostics.CodeAnalysis;
using FastPack.Lib.Hashing;
using FastPack.Lib.ManifestManagement;

namespace FastPack.Lib.Options;

[ExcludeFromCodeCoverage]
public class PackOptions: FilterOptions, IOptions
{
	private bool _showProgress = Environment.UserInteractive && !Console.IsOutputRedirected;
	public string InputDirectoryPath { get; set; }
	public string OutputFilePath { get; set; }
	public int? MaxDegreeOfParallelism { get; set; }
	public long? MaxMemory { get; set; }
	public string Comment { get; set; }
	public CompressionAlgorithm CompressionAlgorithm { get; set; } = CompressionAlgorithm.Deflate;
	public HashAlgorithm HashAlgorithm { get; set; } = HashAlgorithm.XXHash;
	public ushort? CompressionLevel { get; set; }
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
}