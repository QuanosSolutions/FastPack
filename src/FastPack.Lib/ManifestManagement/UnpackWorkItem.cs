namespace FastPack.Lib.ManifestManagement;

internal class UnpackWorkItem
{
	public ManifestFileSystemEntry ManifestFileSystemEntry { get; set; }
	public SharedUnpackWorkItemState SharedState { get; set; }
}

internal class SharedUnpackWorkItemState
{
	public byte[] UnpackedData { get; set; }
	public int WorkItemReferences { get; set; }
}