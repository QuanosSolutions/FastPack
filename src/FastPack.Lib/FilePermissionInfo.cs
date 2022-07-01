using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace FastPack.Lib;

[ExcludeFromCodeCoverage]
public class FilePermissionInfo
{
	private OSPlatform? Platform { get; }

	public FilePermissionInfo()
	{
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			Platform = OSPlatform.Linux;
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			Platform = OSPlatform.OSX;
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			Platform = OSPlatform.Windows;
		else
			Platform = null;
	}

	public uint? GetFilePermissions(string path)
	{
		if (!Platform.HasValue || Platform.Value != OSPlatform.Linux && Platform.Value != OSPlatform.OSX)
			return null;
		if (Mono.Unix.Native.Syscall.stat(path, out var stat) != 0)
			throw new InvalidOperationException($"Stat call failed for {path}");
		return (uint)stat.st_mode;
	}

	public void SetFilePermissions(string path, uint permissions)
	{
		if (!Platform.HasValue || Platform.Value != OSPlatform.Linux && Platform.Value != OSPlatform.OSX)
			return;
		if (Mono.Unix.Native.Syscall.chmod(path, (Mono.Unix.Native.FilePermissions)permissions) != 0)
			throw new InvalidOperationException($"chmod call failed for {path}");
	}
}