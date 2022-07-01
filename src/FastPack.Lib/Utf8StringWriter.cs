using System.IO;
using System.Text;

namespace FastPack.Lib;

internal class Utf8StringWriter : StringWriter
{
	public override Encoding Encoding => Encoding.UTF8;
}