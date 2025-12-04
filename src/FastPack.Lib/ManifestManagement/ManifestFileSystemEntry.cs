using System;
using System.IO;

namespace FastPack.Lib.ManifestManagement;

public class ManifestFileSystemEntry
{
	public string RelativePath { get; set; }
	public UnixFileMode? FilePermissions { get; set; }
	public DateTime? CreationDateUtc { get; set; }
	public DateTime? LastAccessDateUtc { get; set; }
	public DateTime? LastWriteDateUtc { get; set; }
}