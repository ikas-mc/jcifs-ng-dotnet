/*
 *	jcifs-ng for dotnet
 *  ikas-mc@github 2021
 */

using System;
using System.Collections.Generic;
namespace cifs_ng.lib.security {
	public class Subject {
		public ISet<Principal> getPrincipals() {
			return null;
		}

		public static T doAs<T>(Subject subject, Func<T> action) {
			return action();
		}
	}
}