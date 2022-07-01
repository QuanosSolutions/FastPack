using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FastPack.Lib;

internal static class LimitParallelProducerConsumer
{
	public static async Task ProcessData<TSource, TProduced, TId>(IEnumerable<TSource> sourceList,
		Func<TSource, Task<IEnumerable<TProduced>>> produceFunc,
		Func<TProduced, Task> consumeFunc,
		Func<IEnumerable<TSource>, TSource, bool> producePredicate,
		Func<TSource, IEnumerable<TId>> getSourceIdFunc,
		Func<TProduced, TId> getProducedIdFunc,
		int maxDegreeOfProducerParallelism,
		int maxDegreeOfConsumerParallelism)
	{
		ConcurrentDictionary<TId, TSource> productionDictionary = new();
		SemaphoreSlim throttleSemaphore = new(1, 1);
		SemaphoreSlim signalConsumeSemaphore = new(0, 1);
		await ParallelProducerConsumer.ProcessData(sourceList,
			async source =>
			{
				await throttleSemaphore.WaitAsync();
				try
				{
					while (!producePredicate(productionDictionary.Values, source))
						await signalConsumeSemaphore.WaitAsync();

					foreach (TId id in getSourceIdFunc(source))
						productionDictionary.TryAdd(id, source);
				}
				finally
				{
					throttleSemaphore.Release();
				}

				return await produceFunc(source);
			},
			async produced =>
			{
				await consumeFunc(produced);
				productionDictionary.TryRemove(getProducedIdFunc(produced), out _);
				try
				{
					signalConsumeSemaphore.Release();
				}
				catch (Exception)
				{
					// This exception can occur of no producer is waiting
				}
			},
			maxDegreeOfProducerParallelism,
			maxDegreeOfConsumerParallelism);
	}
}