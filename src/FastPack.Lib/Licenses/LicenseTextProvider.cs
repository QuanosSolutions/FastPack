using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FastPack.Lib.Licenses;

internal class LicenseTextProvider
{
	private static Assembly ThisAssembly { get; } = Assembly.GetAssembly(typeof(LicenseTextProvider));
	private static string ThisNamespace { get; } = typeof(LicenseTextProvider).Namespace;

	public static async Task<string> GetFastPackLicenseText(string linePrefix = null)
	{
		if (string.IsNullOrEmpty(linePrefix))
			return await GetEmbeddedFileText("FastPackLicense.txt");

		StringBuilder stringBuilder = new();
		foreach (string licenseTextLine in await GetEmbeddedFileLines("FastPackLicense.txt"))
			stringBuilder.AppendLine($"{linePrefix}{licenseTextLine}");

		return stringBuilder.ToString();
	}

	public static async Task<IEnumerable<string>> GetThirdPartyLicenseTexts()
	{
		List<string> lines = new();
		string json = await GetEmbeddedFileText("ThirdParty/ThirdPartyLicenses.json");
		foreach (JsonElement licenseElement in JsonDocument.Parse(json).RootElement.GetProperty("libs").EnumerateArray())
		{
			string name = licenseElement.GetProperty("name").GetString();
			string relativeFilePath = licenseElement.GetProperty("file").GetString();

			lines.Add(await GetThirdPartyLicenseText(name, $"ThirdParty/{relativeFilePath}"));
		}
		return lines;
	}

	private static async Task<string> GetThirdPartyLicenseText(string name, string filename)
	{
		StringBuilder stringBuilder = new();
		stringBuilder.AppendLine(name);
		stringBuilder.AppendLine("".PadRight(name.Length, '='));
		foreach (string licenseTextLine in await GetEmbeddedFileLines(filename))
			stringBuilder.AppendLine($"  {licenseTextLine}");

		return stringBuilder.ToString();
	}

	private static async Task<string> GetEmbeddedFileText(string relativePath)
	{
		await using Stream stream = ThisAssembly.GetManifestResourceStream(GetNamespace(relativePath));
		using StreamReader reader = new(stream!);
		return await reader.ReadToEndAsync();
	}

	private static async Task<IEnumerable<string>> GetEmbeddedFileLines(string relativePath)
	{
		List<string> lines = new();
		await using Stream stream = ThisAssembly.GetManifestResourceStream(GetNamespace(relativePath));
		using StreamReader reader = new(stream!);
		do
		{
			string line = await reader.ReadLineAsync();

			if (line == null)
				break;

			lines.Add(line);
		}
		while (true);
		return lines;
	}

	private static string GetNamespace(string relativePath)
	{
		return $"{ThisNamespace}.{relativePath.Replace('/', '.')}";
	}
}