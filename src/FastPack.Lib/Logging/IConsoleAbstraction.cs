using System;
using System.Threading.Tasks;

namespace FastPack.Lib.Logging;

public interface IConsoleAbstraction
{
	Task ConsoleOutWriteAsync(string value);
	Task ConsoleOutWriteLineAsync(string value);
	Task ConsoleErrorWriteAsync(string value);
	Task ConsoleErrorWriteLineAsync(string value);
	void SetForegroundColor(ConsoleColor color);
	void ResetColor();
	int GetWindowWidth();
	ConsoleKeyInfo ReadKey(bool intercept = false);
}