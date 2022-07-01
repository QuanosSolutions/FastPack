using FastPack.Lib.Actions;

namespace FastPack.Options;

internal class FastPackActionOptions
{
	public ActionType Action { get; set; }
	public string[] StrippedArgs { get; set; }
	public bool HelpRequested { get; set; }
	public bool IsStrictParamsMode { get; set; }
}