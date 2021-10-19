using System.Text;

/*
 * © 2017 Matthias Bläsing <mblaesing@doppel-helix.eu>
 * 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 * 
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */

namespace jcifs.@internal.util {

	/// 
	public sealed class StringUtil {

		/// 
		private StringUtil() {
		}


		/// <summary>
		/// Implementation of <seealso cref="java.lang.String.join"/> backported for JDK7.
		/// </summary>
		/// <param name="delimiter"> </param>
		/// <param name="elements"> </param>
		/// <returns> elements separated by delimiter </returns>
		public static string join(string delimiter, params string[] elements) {
			StringBuilder sb = new StringBuilder();
			foreach (string element in elements) {
				if (sb.Length > 0) {
					if (delimiter != null) {
						sb.Append(delimiter);
					}
					else {
						sb.Append("null");
					}
				}
				sb.Append(element);
			}
			return sb.ToString();
		}
	}

}