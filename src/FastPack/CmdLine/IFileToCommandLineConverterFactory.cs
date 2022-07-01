namespace FastPack.CmdLine;

internal interface IFileToCommandLineConverterFactory
{
	IFileToCommandLineConverter GetConverter(string filePath);
}