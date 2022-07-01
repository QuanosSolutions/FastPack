using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using FastPack.Benchmarks.Hashing;

namespace FastPack.Benchmarks;

class Program
{
	static void Main(string[] args)
	{
		var summary = BenchmarkRunner.Run<HashComparison>(
			DefaultConfig.Instance.WithSummaryStyle(
				DefaultConfig.Instance.SummaryStyle.WithRatioStyle(RatioStyle.Trend)
			)
		);
	}
}