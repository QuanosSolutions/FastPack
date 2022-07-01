using System;
using System.Globalization;

namespace FastPack.Lib.TypeExtensions;

internal static class LongExtensions
{
	public static string GetBytesReadable(this long value)
	{
		long absoluteValue = Math.Abs(value);
		string suffix;
		double readable;
		if (absoluteValue >= 0x1000000000000000)
		{
			suffix = "EB";
			readable = (value >> 50);
		}
		else if (absoluteValue >= 0x4000000000000)
		{
			suffix = "PB";
			readable = (value >> 40);
		}
		else if (absoluteValue >= 0x10000000000)
		{
			suffix = "TB";
			readable = (value >> 30);
		}
		else if (absoluteValue >= 0x40000000)
		{
			suffix = "GB";
			readable = (value >> 20);
		}
		else if (absoluteValue >= 0x100000)
		{
			suffix = "MB";
			readable = (value >> 10);
		}
		else if (absoluteValue >= 0x400)
		{
			suffix = "KB";
			readable = value;
		}
		else
			return value.ToString("0 B", CultureInfo.InvariantCulture);
		readable /= 1024;
		return readable.ToString("0.### ", CultureInfo.InvariantCulture) + suffix;
	}
}