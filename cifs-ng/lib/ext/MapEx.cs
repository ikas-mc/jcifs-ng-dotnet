/*
 *	jcifs-ng for dotnet
 *  ikas-mc@github 2021
 */

using System.Collections.Generic;
namespace cifs_ng.lib.ext {
	public static class MapEx {
		public static V get<T, V>(this IDictionary<T, V> dictionary, T key) {
			dictionary.TryGetValue(key, out var value);
			return value;
		}

		public static V put<T, V>(this IDictionary<T, V> dictionary, T key, V value) {
			if (dictionary.TryGetValue(key, out var oldValue)) {
				dictionary[key] = value;
				return oldValue;
			}

			dictionary.Add(key, value);
			return default;
		}
	}
}