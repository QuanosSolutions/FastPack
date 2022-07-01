using System.Text.Json.Serialization;

namespace FastPack.CmdLine;

internal class ParameterDocument
{
	[JsonPropertyName("args")]
	public string[] Args { get; set; }
}