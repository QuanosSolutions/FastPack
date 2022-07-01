using System.Threading.Tasks;

namespace FastPack.Lib.Diff;

internal interface IDiffReporter
{
	Task PrintReport(DiffReport diffReport, bool prettyPrint);
}