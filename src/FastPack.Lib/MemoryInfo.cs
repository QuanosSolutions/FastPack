using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FastPack.Lib.Logging;

namespace FastPack.Lib;

[ExcludeFromCodeCoverage]
internal static class MemoryInfo
{
	public static async Task<long> GetAvailableMemoryInBytes(ILogger logger)
	{
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			return await GetMemoryInfoForWindows(logger);
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			return await GetMemoryInfoForLinux(logger);
		if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			return await GetMemoryInfoForOsx(logger);
		return await GetMemoryInfoForUnsupportedSystem(logger);
	}

	private static async Task<long> GetMemoryInfoForUnsupportedSystem(ILogger logger)
	{
		//System.OperatingSystem
		await logger.DebugLine($"[MemoryInfo] Found unsupported OS '{Environment.OSVersion.Platform}'. Falling back to default default values.");

		// we use 0 here because this way FastPack works, even when the OS could not be handled - Though its slower
		return 0;
	}

	#region Windows

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	private class MEMORYSTATUSEX
	{
		public uint dwLength;
		public uint dwMemoryLoad;
		public ulong ullTotalPhys;
		public ulong ullAvailPhys;
		public ulong ullTotalPageFile;
		public ulong ullAvailPageFile;
		public ulong ullTotalVirtual;
		public ulong ullAvailVirtual;
		public ulong ullAvailExtendedVirtual;
		public MEMORYSTATUSEX()
		{
			this.dwLength = (uint)Marshal.SizeOf(this);
		}
	}

	[return: MarshalAs(UnmanagedType.Bool)]
	[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

	private static async Task<long> GetMemoryInfoForWindows(ILogger logger)
	{
		MEMORYSTATUSEX memStatus = new();
		if (GlobalMemoryStatusEx(memStatus))
			return (long)memStatus.ullAvailPhys;

		await logger.DebugLine($"[MemoryInfo] Could not query available memory.. Falling back to default values.");
		return 0;
	}

	#endregion

	#region OSX

	[DllImport("libc")]
	static extern int sysctlbyname(string name, out IntPtr oldp, ref IntPtr oldlenp, IntPtr newp, IntPtr newlen);

	private static async Task<long> GetMemoryInfoForOsx(ILogger logger)
	{
		IntPtr sizeOfLineSize = (IntPtr)IntPtr.Size;

		if (sysctlbyname("hw.usermem", out IntPtr lineSize, ref sizeOfLineSize, IntPtr.Zero, IntPtr.Zero) == 0)
			return lineSize.ToInt64();

		await logger.DebugLine("[MemoryInfo] Failed to retrieve available memory size. Falling back to default values.");
		return 0;
	}

	#endregion

	#region Linux

	private static async Task<long> GetMemoryInfoForLinux(ILogger logger)
	{
		const string memoryFilePath = "/proc/meminfo";

		if (!File.Exists(memoryFilePath))
		{
			await logger.DebugLine($"[MemoryInfo] File not found '{memoryFilePath}'. Falling back to default values.");
			return 0;
		}

		Match lineMatch = (await File.ReadAllLinesAsync(memoryFilePath)).Select(l => Regex.Match(l, @"^MemAvailable:\s*(?<number>\d+)\s*(?<unit>\w+)*", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)).FirstOrDefault(m => m.Success);

		if (lineMatch != null)
		{
			long freeBytes = Convert.ToInt64(lineMatch.Groups["number"].Value, CultureInfo.InvariantCulture);

			// currently "kb" seems to be fixed
			if (lineMatch.Groups["unit"].Success && lineMatch.Groups["unit"].Value.Equals("kb", StringComparison.InvariantCultureIgnoreCase))
				freeBytes *= 1024;

			return freeBytes;
		}
		else
		{
			await logger.DebugLine($"[MemoryInfo] 'MemAvailable:' not found in '{memoryFilePath}'. Falling back to default values.");
			return 0;
		}
	}

	#endregion
}