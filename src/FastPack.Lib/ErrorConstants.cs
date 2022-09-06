namespace FastPack.Lib;

public class ErrorConstants
{
	public const int UnspecificError = -1;
	public const int MissingAction = -2;

	// Diff-Constants 100-199
	public const int Diff_FirstFilePath_Missing = -100;
	public const int Diff_SecondFilePath_Missing = -101;
	public const int Diff_FirstFilePath_NoFound = -102;
	public const int Diff_SecondFilePath_NoFound = -103;

	// Info-Constants 200-299
	public const int Diff_InputFilePath_Missing = -200;
	public const int Diff_InputFilePath_NoFound = -201;

	// Pack-Constants 300-399
	public const int Pack_InputDirectoryPath_Missing = -300;
	public const int Pack_InputDirectoryPath_NoFound = -301;
	public const int Pack_OutputFilePath_Missing = -302;
	public const int Pack_Invalid_CompressionLevel = -303;
	public const int Pack_Invalid_IncludeFilter = -304;
	public const int Pack_Invalid_ExcludeFilter = -305;

	// Unpack-Constants 400-499
	public const int Unpack_InputFilePath_Missing = -300;
	public const int Unpack_InputFilePath_NoFound = -301;
	public const int Unpack_OutputDirectoryPath_Missing = -302;
	public const int Unpack_Invalid_IncludeFilter = -303;
	public const int Unpack_Invalid_ExcludeFilter = -304;
	public const int Unpack_Not_Enough_Disk_Space = -305;

}