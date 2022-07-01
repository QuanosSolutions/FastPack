using System.Collections.Generic;

namespace FastPack.Lib.TypeExtensions;

internal static class DictionaryExtensions
{
	public static void AddMultiple<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey[] keys, TValue value)
	{
		foreach (TKey key in keys)
			dictionary.Add(key, value);
	}
}