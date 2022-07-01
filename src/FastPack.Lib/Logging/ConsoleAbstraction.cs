using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace FastPack.Lib.Logging
{
	[ExcludeFromCodeCoverage]
	internal class ConsoleAbstraction: IConsoleAbstraction
	{
		public async Task ConsoleOutWriteAsync(string value)
		{
			await Console.Out.WriteAsync(value);
		}

		public async Task ConsoleOutWriteLineAsync(string value)
		{
			await Console.Out.WriteLineAsync(value);
		}

		public async Task ConsoleErrorWriteAsync(string value)
		{
			await Console.Error.WriteAsync(value);
		}

		public async Task ConsoleErrorWriteLineAsync(string value)
		{
			await Console.Error.WriteLineAsync(value);
		}

		public void SetForegroundColor(ConsoleColor color)
		{
			Console.ForegroundColor = color;
		}

		public void ResetColor()
		{
			Console.ResetColor();
		}

		public int GetWindowWidth()
		{
			return Console.WindowWidth;
		}

		public ConsoleKeyInfo ReadKey(bool intercept = false)
		{
			return Console.ReadKey(intercept);
		}
	}
}
