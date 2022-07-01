using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace FastPack.Lib.Diff;

public class DiffEntryChange
{
	[JsonPropertyName("first")]
	[XmlElement("First")]
	public DiffEntry First { get; set; }
	[JsonPropertyName("second")]
	[XmlElement("Second")]
	public DiffEntry Second { get; set; }
}