using System.Text.Json.Serialization;

namespace FastPack.Lib.ManifestReporting;

[JsonSerializable(typeof(ManifestReport))]
internal partial class ManifestReportJsonContext : JsonSerializerContext
{
}