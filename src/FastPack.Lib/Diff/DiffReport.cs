using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace FastPack.Lib.Diff;

[XmlRoot("Diff")]
public class DiffReport
{
	[JsonPropertyName("addedCount")]
	[XmlElement("AddedCount")]
	public int AddedCount { get; set; }
	[JsonPropertyName("removedCount")]
	[XmlElement("RemovedCount")]
	public int RemovedCount { get; set; }
	[JsonPropertyName("changedSizeCount")]
	[XmlElement("ChangedSizeCount")]
	public int? ChangedSizeCount { get; set; }
	[JsonPropertyName("changedDatesCount")]
	[XmlElement("ChangedDatesCount")]
	public int? ChangedDatesCount { get; set; }
	[JsonPropertyName("changedPermissionsCount")]
	[XmlElement("ChangedPermissionsCount")]
	public int? ChangedPermissionsCount { get; set; }
	[JsonPropertyName("totalChangedCount")]
	[XmlElement("TotalChangedCount")]
	public long TotalChangedCount { get; set; }
	[JsonPropertyName("addedEntries")]
	[XmlArray("AddedEntries")]
	[XmlArrayItem("DiffEntry")]
	public List<DiffEntry> AddedEntries { get; set; }
	[JsonPropertyName("removedEntries")]
	[XmlArray("RemovedEntries")]
	[XmlArrayItem("DiffEntry")]
	public List<DiffEntry> RemovedEntries { get; set; }
	[JsonPropertyName("changedSizeEntries")]
	[XmlArray("ChangedSizeEntries")]
	[XmlArrayItem("DiffEntryChange")]
	public List<DiffEntryChange> ChangedSizeEntries { get; set; }
	[JsonPropertyName("changedDatesEntries")]
	[XmlArray("ChangedDatesEntries")]
	[XmlArrayItem("DiffEntryChange")]
	public List<DiffEntryChange> ChangedDatesEntries { get; set; }
	[JsonPropertyName("changedPermissionsEntries")]
	[XmlArray("ChangedPermissionsEntries")]
	[XmlArrayItem("DiffEntryChange")]
	public List<DiffEntryChange> ChangedPermissionsEntries { get; set; }

	// The following properties are only used by the XmlSerializer to determine whether a value has been set.
	[XmlIgnore]
	[JsonIgnore]
	public bool ChangedSizeCountSpecified => ChangedSizeCount.HasValue;
	[XmlIgnore]
	[JsonIgnore]
	public bool ChangedDatesCountSpecified => ChangedDatesCount.HasValue;
	[XmlIgnore]
	[JsonIgnore]
	public bool ChangedPermissionsCountSpecified => ChangedPermissionsCount.HasValue;
	[XmlIgnore]
	[JsonIgnore]
	public bool ChangedSizeEntriesSpecified => ChangedSizeEntries != null;
	[XmlIgnore]
	[JsonIgnore]
	public bool ChangedDatesEntriesSpecified => ChangedDatesEntries != null;
	[XmlIgnore]
	[JsonIgnore]
	public bool ChangedPermissionsEntriesSpecified => ChangedPermissionsEntries != null;
}