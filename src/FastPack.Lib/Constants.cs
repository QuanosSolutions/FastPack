namespace FastPack.Lib;

public static class Constants
{
	public const ulong FastpackSignature = 5423250189552140614; // F A S T P A C K
	public const int FastpackSignatureSize = sizeof(ulong);
	public const int FastpackFileVersionSize = sizeof(ushort);
	public const int FastpackFileDataIndex = FastpackSignatureSize + FastpackFileVersionSize;
	public const char FastpackManifestDirectorySeparatorChar = '/';
	public const int BufferSize = 4096;
	public static bool OpenFileStreamsAsync = true;
	public static ushort CurrentManifestVersion = 1;
}