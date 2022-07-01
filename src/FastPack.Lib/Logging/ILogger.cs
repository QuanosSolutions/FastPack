using System.Threading.Tasks;

namespace FastPack.Lib.Logging;

public interface ILogger
{
	public Task Debug(string message);
	public Task DebugLine(string message = null);
	public Task Info(string message);
	public Task InfoLine(string message = null);
	public Task StartTextProgress(string message);
	public Task ReportTextProgress(double percentage, string prefixText);
	public Task FinishTextProgress(string message);
	public Task Error(string message);
	public Task ErrorLine(string message = null);
	public void SetQuiet(bool quiet = true);
}