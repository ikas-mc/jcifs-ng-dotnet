/* jcifs msrpc client library in Java
 * Copyright (C) 2006  "Michael B. Allen" <jcifs at samba dot org>
 *                     "Eric Glass" <jcifs at samba dot org>
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

namespace jcifs.dcerpc {

	/// <summary>
	/// Unicode string type wrapper
	/// 
	/// </summary>
	public class UnicodeString : rpc.unicode_string {

		internal bool zterm;


		/// 
		/// <param name="zterm">
		///            whether the string should be zero terminated </param>
		public UnicodeString(bool zterm) {
			this.zterm = zterm;
		}


		/// 
		/// <param name="rus">
		///            wrapped string </param>
		/// <param name="zterm">
		///            whether the string should be zero terminated </param>
		public UnicodeString(rpc.unicode_string rus, bool zterm) {
			this.length = rus.length;
			this.maximum_length = rus.maximum_length;
			this.buffer = rus.buffer;
			this.zterm = zterm;
		}


		/// 
		/// <param name="str">
		///            wrapped string </param>
		/// <param name="zterm">
		///            whether the string should be zero terminated </param>
		public UnicodeString(string str, bool zterm) {
			this.zterm = zterm;

			int len = str.Length;
			int zt = zterm ? 1 : 0;

			this.length = this.maximum_length = (short)((len + zt) * 2);
			this.buffer = new short[len + zt];

			int i;
			for (i = 0; i < len; i++) {
				this.buffer[i] = (short) str[i];
			}
			if (zterm) {
				this.buffer[i] = (short) 0;
			}
		}


		public override string ToString() {
			int len = this.length / 2 - (this.zterm ? 1 : 0);
			char[] ca = new char[len];
			for (int i = 0; i < len; i++) {
				ca[i] = (char) this.buffer[i];
			}
			return new string(ca, 0, len);
		}
	}

}