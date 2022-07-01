using System.Text.Json;
using System.Text.Json.Serialization;

namespace FastPack.Lib;

internal static class JsonHelper
{
	public static JsonSerializerOptions GetSerializerOptions(bool prettyPrint)
	{
		return new JsonSerializerOptions
		{
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
			WriteIndented = prettyPrint,
		};
	}
}