using System.Text.Json.Serialization;

namespace FastPack.CmdLine;

[JsonSerializable(typeof(ParameterDocument))]
internal partial class ParameterDocumentJsonContext : JsonSerializerContext
{
}