using System.Text.Json.Serialization;

namespace FastPack.Lib.Diff;

[JsonSerializable(typeof(DiffReport))]
internal partial class DiffReportJsonContext : JsonSerializerContext
{
}