using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace FastPack.CmdLine;

public class XmlFileToCommandLineConverter : IFileToCommandLineConverter
{
	public Task<string[]> ConvertToArgs(string filePath)
	{
		return Task.FromResult(XDocument.Load(filePath).XPathSelectElements("/args/arg").Select(x => x.Value).ToArray());
	}
}