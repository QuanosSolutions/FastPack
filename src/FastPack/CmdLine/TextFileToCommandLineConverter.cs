using System.IO;
using System.Threading.Tasks;

namespace FastPack.CmdLine;

public class TextFileToCommandLineConverter : IFileToCommandLineConverter
{
	public Task<string[]> ConvertToArgs(string filePath)
	{
		return File.ReadAllLinesAsync(filePath);
	}
}