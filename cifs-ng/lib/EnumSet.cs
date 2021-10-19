/*
 *	jcifs-ng for dotnet
 *  ikas-mc@github 2021
 */

using System.Collections.Generic;
namespace cifs_ng.lib {
	public class EnumSet<T> : HashSet<T> {
		public EnumSet() {
		}

		public EnumSet(int capacity) : base(capacity) {
		}

		public EnumSet(IEnumerable<T> collection) : base(collection) {
		}

		public static EnumSet<T> noneOf(int size) {
			var set = new EnumSet<T>(size);
			return set;
		}

		public static EnumSet<T> of(T retainPayload) {
			var set = new EnumSet<T>(1);
			set.Add(retainPayload);
			return set;
		}
	}
}