﻿/*
 *	jcifs-ng for dotnet
 *  ikas-mc@github 2021
 */

namespace cifs_ng.lib {
	public interface Enumeration<T> {

		bool hasMoreElements();

		T nextElement();
	}
}