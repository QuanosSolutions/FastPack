using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace FastPack.Lib.Diff;

public class DiffEntry
{
	[JsonPropertyName("relativePath")]
	[XmlElement("RelativePath")]
	public string RelativePath { get; set; }
	[JsonPropertyName("hash")]
	[XmlElement("Hash")]
	public string Hash { get; set; }
	[JsonPropertyName("size")]
	[XmlElement("Size")]
	public long Size { get; set; }
	[JsonPropertyName("permissions")]
	[XmlElement("Permissions")]
	public string Permissions { get; set; }
	[JsonPropertyName("created")]
	[XmlElement("Created")]
	public DateTime? Created { get; set; }
	[JsonPropertyName("lastAccess")]
	[XmlElement("LastAccess")]
	public DateTime? LastAccess { get; set; }
	[JsonPropertyName("lastWrite")]
	[XmlElement("LastWrite")]
	public DateTime? LastWrite { get; set; }

	// The following properties are only used by the XmlSerializer to determine whether a value has been set.
	[ExcludeFromCodeCoverage]
	[XmlIgnore]
	[JsonIgnore]
	public bool PermissionsSpecified => Permissions != null;
	[ExcludeFromCodeCoverage]
	[XmlIgnore]
	[JsonIgnore]
	public bool CreatedSpecified => Created.HasValue;
	[ExcludeFromCodeCoverage]
	[XmlIgnore]
	[JsonIgnore]
	public bool LastAccessSpecified => LastAccess.HasValue;
	[ExcludeFromCodeCoverage]
	[XmlIgnore]
	[JsonIgnore]
	public bool LastWriteSpecified => LastWrite.HasValue;
}