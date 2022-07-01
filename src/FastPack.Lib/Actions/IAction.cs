using System.Threading.Tasks;

namespace FastPack.Lib.Actions;

public interface IAction
{
	public Task<int> Run();
}