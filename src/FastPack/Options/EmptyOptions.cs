using System.Diagnostics.CodeAnalysis;
using FastPack.Lib.Options;

namespace FastPack.Options;

internal class EmptyOptions : IOptions
{
	[ExcludeFromCodeCoverage]
	public bool QuietMode { get; set; }
}