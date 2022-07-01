using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace FastPack.Lib.Logging
{
	[ExcludeFromCodeCoverage]
	internal class ProcessAbstraction: IProcessAbstraction
	{
		public void Start(string fileName, string arguments)
		{
			Process.Start(fileName, arguments);
		}
	}
}
