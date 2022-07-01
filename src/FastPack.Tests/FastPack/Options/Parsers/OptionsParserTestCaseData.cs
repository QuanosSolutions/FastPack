using FastPack.Lib.Options;

namespace FastPack.Tests.FastPack.Options.Parsers;

public class OptionsParserTestCaseData<T> where T: IOptions
{
	public string[] Args { get; set; }
	public bool StrictMode { get; set; } = true;
	public T ExpectedOptions { get; set; }

	public override string ToString()
	{
		return $"[Args: {string.Join(' ', Args)}]";
	}
}