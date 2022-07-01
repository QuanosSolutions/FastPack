using System.Threading.Tasks;

namespace FastPack.CmdLine;

public interface IFileToCommandLineConverter
{
	public Task<string[]> ConvertToArgs(string filePath);
}