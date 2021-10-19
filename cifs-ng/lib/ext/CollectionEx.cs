/*
 *	jcifs-ng for dotnet
 *  ikas-mc@github 2021
 */

using System;
using System.Collections.Generic;
using System.Linq;
namespace cifs_ng.lib.ext {
	public static class CollectionEx {
		public static bool RemoveAll<T>(this ICollection<T> c1, Func<T, bool> filter) {
			//TODO 
			var sets = c1.Where(filter).ToList();
			var changed = false;
			foreach (T item in sets) {
				if (c1.Contains(item)) {
					c1.Remove(item);
					changed = true;
				}
			}
			return changed;
		}
	}
}