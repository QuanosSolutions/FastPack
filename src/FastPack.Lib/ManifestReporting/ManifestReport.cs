using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace FastPack.Lib.ManifestReporting;

[XmlRoot("Manifest")]
public class ManifestReport
{
	[JsonPropertyName("version")]
	[XmlElement("Version")]
	public ushort Version { get; set; }
	[JsonPropertyName("created")]
	[XmlElement("Created")]
	public DateTime Created { get; set; }
	[JsonPropertyName("filesystemDatesIncluded")]
	[XmlElement("FilesystemDatesIncluded")]
	public bool FilesystemDatesIncluded { get; set; }
	[JsonPropertyName("filesystemPermissionsIncluded")]
	[XmlElement("FilesystemPermissionsIncluded")]
	public bool FilesystemPermissionsIncluded { get; set; }
	[JsonPropertyName("hashingAlgorithm")]
	[XmlElement("HashingAlgorithm")]
	public string HashingAlgorithm { get; set; }
	[JsonPropertyName("compressionAlgorithm")]
	[XmlElement("CompressionAlgorithm")]
	public string CompressionAlgorithm { get; set; }
	[JsonPropertyName("compressionLevelName")]
	[XmlElement("CompressionLevelName")]
	public string CompressionLevelName { get; set; }
	[JsonPropertyName("compressionLevel")]
	[XmlElement("CompressionLevel")]
	public ushort CompressionLevel { get; set; }
	[JsonPropertyName("comment")]
	[XmlElement("Comment")]
	public string Comment { get; set; }
	[JsonPropertyName("filesCount")]
	[XmlElement("FilesCount")]
	public long FilesCount { get; set; }
	[JsonPropertyName("filesSizeCompressed")]
	[XmlElement("FilesSizeCompressed")]
	public long? FilesSizeCompressed { get; set; }
	[JsonPropertyName("filesSizeUncompressed")]
	[XmlElement("FilesSizeUncompressed")]
	public long FilesSizeUncompressed { get; set; }
	[JsonPropertyName("uniqueFilesCount")]
	[XmlElement("UniqueFilesCount")]
	public long UniqueFilesCount { get; set; }
	[JsonPropertyName("uniqueFilesSizeCompressed")]
	[XmlElement("UniqueFilesSizeCompressed")]
	public long? UniqueFilesSizeCompressed { get; set; }
	[JsonPropertyName("uniqueFilesSizeUncompressed")]
	[XmlElement("UniqueFilesSizeUncompressed")]
	public long UniqueFilesSizeUncompressed { get; set; }
	[JsonPropertyName("FoldersCount")]
	[XmlElement("FoldersCount")]
	public long FoldersCount { get; set; }
	[JsonPropertyName("folders")]
	[XmlArray("Folders")]
	[XmlArrayItem("Folder")]
	public List<ManifestReportEntry> Folders { get; set; }
	[JsonPropertyName("files")]
	[XmlArray("Files")]
	[XmlArrayItem("File")]
	public List<ManifestReportEntry> Files { get; set; }

	// The following properties are only used by the XmlSerializer to determine whether a value has been set.
	[XmlIgnore]
	[JsonIgnore]
	public bool FilesSizeCompressedSpecified => FilesSizeCompressed.HasValue;
	[XmlIgnore]
	[JsonIgnore]
	public bool UniqueFilesSizeCompressedSpecified => FilesSizeCompressed.HasValue;
	[XmlIgnore]
	[JsonIgnore]
	public bool FoldersSpecified => Folders != null;
	[XmlIgnore]
	[JsonIgnore]
	public bool FilesSpecified => Files != null;
}