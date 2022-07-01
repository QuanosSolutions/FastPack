using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FastPack.CmdLine;

public class JsonFileToCommandLineConverter : IFileToCommandLineConverter
{
	public async Task<string[]> ConvertToArgs(string filePath)
	{
		return JsonSerializer.Deserialize(await File.ReadAllTextAsync(filePath, Encoding.UTF8), ParameterDocumentJsonContext.Default.ParameterDocument)!.Args;
	}
}