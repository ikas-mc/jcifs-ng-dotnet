/*
 *	jcifs-ng for dotnet
 *  ikas-mc@github 2021
 */

using System.Text;
namespace cifs_ng.lib.ext {
	public static class StringExt {
		//TODO 
		private static readonly Encoding DefaultEncoding = Encoding.UTF8;

		public static bool compare(this string str, bool ignoreCase, int leftIndex, string other, int rightIndex, int len) {
			if (leftIndex < 0 || rightIndex < 0 || leftIndex + len > str.Length || rightIndex + len > other.Length)
				return false;
			return string.Compare(str, leftIndex, other, rightIndex, len, ignoreCase) == 0;
		}

		public static string toString(this byte[] bytes, int index, int length) {
			return DefaultEncoding.GetString(bytes, index, length);
		}

		public static string toString(this byte[] bytes, string encoding) {
			return Encoding.GetEncoding(encoding).GetString(bytes, 0, bytes.Length);
		}

		public static byte[] getBytes(this string s) {
			return DefaultEncoding.GetBytes(s);
		}

		public static byte[] getBytes(this string s, string encoding) {
			return Encoding.GetEncoding(encoding).GetBytes(s);
		}

		public static byte[] getBytes(this string s, Encoding encoding) {
			return encoding.GetBytes(s);
		}

		public static string toString(this byte[] bytes, int index, int length, string encoding) {
			return Encoding.GetEncoding(encoding).GetString(bytes, index, length);
		}

		public static string toString(this byte[] bytes, int index, int length, Encoding encoding) {
			return encoding.GetString(bytes, index, length);
		}

	}
}