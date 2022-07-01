using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace FastPack.Lib;

internal static class ParallelProducerConsumer
{
	public static async Task ProcessData<TSource, TProduced>(IEnumerable<TSource> sourceList,
		Func<TSource, Task<IEnumerable<TProduced>>> produceFunc,
		Func<TProduced, Task> consumeFunc,
		int maxDegreeOfProducerParallelism,
		int maxDegreeOfConsumerParallelism)
	{
		BufferBlock<TProduced> bufferBlock = new();
		Task consumerTask = Consume(bufferBlock, consumeFunc, maxDegreeOfConsumerParallelism);
		await Produce(sourceList, produceFunc, bufferBlock, maxDegreeOfProducerParallelism);
		await consumerTask;
	}

	private static async Task Produce<TSource, TProduced>(IEnumerable<TSource> sourceList,
		Func<TSource, Task<IEnumerable<TProduced>>> produceFunc,
		ITargetBlock<TProduced> target,
		int maxDegreeOfParallelism)
	{
		await Parallel.ForEachAsync(sourceList, new ParallelOptions {MaxDegreeOfParallelism = maxDegreeOfParallelism }, async (source, _) =>
		{
			IEnumerable<TProduced> producedItems = await produceFunc(source);
			foreach (TProduced produced in producedItems)
				target.Post(produced);
		});
		target.Complete();
	}

	private static async Task Consume<TProduced>(IReceivableSourceBlock<TProduced> source,
		Func<TProduced, Task> consumeFunc,
		int maxDegreeOfParallelism)
	{
		List<Task> tasks = new();
		for (int i = 0; i < maxDegreeOfParallelism; i++)
			tasks.Add(Task.Run(async () =>
			{
				while (await source.OutputAvailableAsync())
				{
					while (source.TryReceive(out TProduced data))
					{
						await consumeFunc(data);
					}
				}
			}));
		await Task.WhenAll(tasks);
	}
}