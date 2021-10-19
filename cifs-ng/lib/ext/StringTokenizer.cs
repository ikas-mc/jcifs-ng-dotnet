/*
 *	jcifs-ng for dotnet
 *  ikas-mc@github 2021
 */

namespace cifs_ng.lib.ext {
	public class StringTokenizer {
		private readonly string[] tokens;
		private int position;

		public StringTokenizer(string s, string separator) {
			tokens = s.Split(separator);
		}

		public int countTokens() {
			return tokens.Length;
		}

		public string nextToken() {
			var value = tokens[position];
			position++;
			return value;
		}

		public bool hasMoreTokens() {
			return position < tokens.Length;
		}
	}
}