using System;
using System.Collections.Generic;
using FastPack.Lib.Hashing;

namespace FastPack.Lib.ManifestManagement;

public class Manifest
{
	public ushort Version { get; set; } = Constants.CurrentManifestVersion;
	public HashAlgorithm HashAlgorithm { get; set; } = HashAlgorithm.XXHash;
	public CompressionAlgorithm CompressionAlgorithm { get; set; } = CompressionAlgorithm.Deflate;
	public ushort CompressionLevel { get; set; }
	public DateTime CreationDateUtc { get; set; } = DateTime.UtcNow;
	public MetaDataOptions MetaDataOptions { get; set; } = MetaDataOptions.IncludeFileSystemDates | MetaDataOptions.IncludeFileSystemPermissions;
	public string Comment { get; set; }
	public List<ManifestEntry> Entries { get; set; } = new();
}