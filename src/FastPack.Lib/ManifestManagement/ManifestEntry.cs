using System.Collections.Generic;

namespace FastPack.Lib.ManifestManagement;

public class ManifestEntry
{
	public EntryType Type { get; set; }
	public long DataIndex { get; set; }
	public long DataSize { get; set; }
	public long OriginalSize { get; set; }
	public string Hash { get; set; }
	public List<ManifestFileSystemEntry> FileSystemEntries { get; set; } = new();
}