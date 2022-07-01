using System.Diagnostics.CodeAnalysis;

namespace FastPack.Lib.Options;

[ExcludeFromCodeCoverage]
public class InfoOptions : IOptions
{
	public string InputFilePath { get; set; }
	public bool Detailed { get; set; }
	public OutputFormat OutputFormat { get; set; }
	public bool PrettyPrint { get; set; }
	public bool QuietMode { get; set; }
}