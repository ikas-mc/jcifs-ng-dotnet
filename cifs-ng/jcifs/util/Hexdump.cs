/* jcifs smb client library in Java
 * Copyright (C) 2000  "Michael B. Allen" <jcifs at samba dot org>
 *                     "Christopher R. Hertel" <jcifs at samba dot org>
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

namespace jcifs.util {


	public class Hexdump {

		/// 
		public static readonly char[] HEX_DIGITS = new char[] {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'};


		/// <summary>
		/// This is an alternative to the <code>java.lang.Integer.toHexString</code>
		/// method. It is an efficient relative that also will pad the left side so
		/// that the result is <code>size</code> digits.
		/// </summary>
		/// <param name="val"> </param>
		/// <param name="size"> </param>
		/// <returns> hex string </returns>
		public static string toHexString(int val, int size) {
			char[] c = new char[size];
			toHexChars(val, c, 0, size);
			return new string(c);
		}


		/// <param name="val"> </param>
		/// <param name="size"> </param>
		/// <returns> hex string </returns>
		public static string toHexString(long val, int size) {
			char[] c = new char[size];
			toHexChars(val, c, 0, size);
			return new string(c);
		}


		/// 
		/// <param name="src"> </param>
		/// <param name="srcIndex"> </param>
		/// <param name="size"> </param>
		/// <returns> hex string </returns>
		public static string toHexString(byte[] src, int srcIndex, int size) {
			char[] c = new char[2 * size];
			for (int i = 0, j = 0; i < size; i++) {
				c[j++] = HEX_DIGITS[(src[srcIndex + i] >> 4) & 0x0F];
				c[j++] = HEX_DIGITS[src[srcIndex + i] & 0x0F];
			}
			return new string(c);
		}


		/// <param name="data"> </param>
		/// <returns> hex string </returns>
		public static string toHexString(byte[] data) {
			return toHexString(data, 0, data.Length);
		}


		/// <summary>
		/// This is the same as {@link jcifs.util.Hexdump#toHexString(int val, int
		/// size)} but provides a more practical form when trying to avoid {@link
		/// java.lang.String} concatenation and <seealso cref="System.Text.StringBuilder"/>.
		/// </summary>
		/// <param name="val"> </param>
		/// <param name="dst"> </param>
		/// <param name="dstIndex"> </param>
		/// <param name="size"> </param>
		public static void toHexChars(int val, char[] dst, int dstIndex, int size) {
			while (size > 0) {
				int i = dstIndex + size - 1;
				if (i < dst.Length) {
					dst[i] = HEX_DIGITS[val & 0x000F];
				}
				if (val != 0) {
					val = (int)((uint)val >> 4);
				}
				size--;
			}
		}


		/// <param name="val"> </param>
		/// <param name="dst"> </param>
		/// <param name="dstIndex"> </param>
		/// <param name="size"> </param>
		public static void toHexChars(long val, char[] dst, int dstIndex, int size) {
			while (size > 0) {
				dst[dstIndex + size - 1] = HEX_DIGITS[(int)(val & 0x000FL)];
				if (val != 0) {
					val = (long)((ulong)val >> 4);
				}
				size--;
			}
		}

	}

}