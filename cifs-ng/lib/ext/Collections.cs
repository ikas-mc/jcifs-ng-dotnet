/*
 *	jcifs-ng for dotnet
 *  ikas-mc@github 2021
 */

using System.Collections.Generic;
using System.Collections.Immutable;
namespace cifs_ng.lib.ext {
	public class Collections {
		public static IDictionary<T, TE> unmodifiableMap<T, TE>(IDictionary<T, TE> dictionary) {
			return dictionary.ToImmutableDictionary();
		}
	}
}