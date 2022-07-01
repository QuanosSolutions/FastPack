using System;

namespace FastPack.Lib.ManifestManagement;

public class ManifestFileSystemEntry
{
	public string RelativePath { get; set; }
	public uint? FilePermissions { get; set; }
	public DateTime? CreationDateUtc { get; set; }
	public DateTime? LastAccessDateUtc { get; set; }
	public DateTime? LastWriteDateUtc { get; set; }
}