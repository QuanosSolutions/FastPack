using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using FastPack.Lib.Logging;

namespace FastPack.Lib;

[ExcludeFromCodeCoverage]
internal static class DiskSpaceInfo
{
	public static async Task<long?> GetAvailableSpaceForPathInBytes(string path, ILogger logger)
	{
		path = DetermineExistingParentDirectory(path);
		if (path == null)
			return null;

		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			return await GetAvailableSpaceForPathForWindows(path, logger);
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			return await GetAvailableSpaceForPathForUnix(path, logger);
		return null;
	}

	private static async Task<long?> GetAvailableSpaceForPathForUnix(string path, ILogger logger)
	{
		DriveInfo driveInfo = new(path);
		if (driveInfo.IsReady)
			return driveInfo.AvailableFreeSpace;
		await logger.DebugLine("[DiskSpaceInfo] Failed to retrieve available disk space. Returning null.");
		return null;
	}

	private static async Task<long?> GetAvailableSpaceForPathForWindows(string path, ILogger logger)
	{
		if (GetDiskFreeSpaceEx(path, out var freeBytesAvailable, out _, out _))
			return (long)freeBytesAvailable;
		await logger.DebugLine("[DiskSpaceInfo] Failed to retrieve available disk space. Returning null.");
		return null;
	}

	[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
	[return: MarshalAs(UnmanagedType.Bool)]
	static extern bool GetDiskFreeSpaceEx(string lpDirectoryName, out ulong lpFreeBytesAvailable, out ulong lpTotalNumberOfBytes, out ulong lpTotalNumberOfFreeBytes);

	private static string DetermineExistingParentDirectory(string path)
	{
		if (path == null)
			return null;
		if (File.Exists(path))
			return Path.GetDirectoryName(path);
		return Directory.Exists(path) ? path : DetermineExistingParentDirectory(Path.GetDirectoryName(path));
	}
}