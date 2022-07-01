using System.IO;

namespace FastPack.CmdLine;

internal class FileToCommandLineConverterFactory : IFileToCommandLineConverterFactory
{
	public IFileToCommandLineConverter GetConverter(string filePath)
	{
		string fileExtension = Path.GetExtension(filePath)?.ToLowerInvariant();

		return fileExtension switch {
			".json" => new JsonFileToCommandLineConverter(),
			".xml" => new XmlFileToCommandLineConverter(),
			_ => new TextFileToCommandLineConverter()
		};
	}
}