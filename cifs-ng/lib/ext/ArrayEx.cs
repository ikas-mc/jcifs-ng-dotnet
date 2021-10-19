/*
 *	jcifs-ng for dotnet
 *  ikas-mc@github 2021
 */

using System;
using System.Text;
namespace cifs_ng.lib.ext {
	public static class ArrayEx {

		public static T[] sub<T>(this T[] src,int start, int length) {
			var dest = new T[length];
			Array.Copy(src, start, dest, 0, length);
			return dest;
		}

		public static string joinToString(this Array array) {
			if (null == array) {
				return string.Empty;
			}

			if (array.Length == 1) {
				return array.GetValue(0).ToString();
			}

			var sb = new StringBuilder();
			foreach (var item in array) {
				sb.Append(item.ToString()).Append(",");
			}
			return sb.ToString();
		}
	}
}