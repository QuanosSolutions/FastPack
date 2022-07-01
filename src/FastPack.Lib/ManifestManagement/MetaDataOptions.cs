using System;

namespace FastPack.Lib.ManifestManagement;

[Flags]
public enum MetaDataOptions : byte
{
	None = 0x0,
	IncludeFileSystemDates = 0x1,
	IncludeFileSystemPermissions = 0x2,
}