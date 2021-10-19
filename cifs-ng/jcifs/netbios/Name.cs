using System;
using System.Text;
using cifs_ng.lib.ext;
using Configuration = jcifs.Configuration;
using NetbiosName = jcifs.NetbiosName;
using Hexdump = jcifs.util.Hexdump;
using Strings = jcifs.util.Strings;

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

namespace jcifs.netbios {



	/// 
	/// 
	public class Name : NetbiosName {

		private const int TYPE_OFFSET = 31;
		private const int SCOPE_OFFSET = 33;

		/// <summary>
		/// Name
		/// </summary>
		public string name;
		/// <summary>
		/// Scope id
		/// </summary>
		public string scope;
		/// <summary>
		/// Type
		/// </summary>
		public int hexCode;
		internal int srcHashCode; /*
	                      * srcHashCode must be set by name resolution
	                      * routines before entry into addressCache
	                      */
		private Configuration config;


		internal Name(Configuration cfg) {
			this.config = cfg;
		}


		/// <returns> the name </returns>
		public virtual string getName() {
			return this.name;
		}


		/// <returns> scope id </returns>
		public virtual string getScope() {
			return this.scope;
		}


		/// 
		/// <returns> the name type </returns>
		public virtual int getNameType() {
			return this.hexCode;
		}


		/// 
		/// <param name="cfg"> </param>
		/// <param name="name"> </param>
		/// <param name="hexCode"> </param>
		/// <param name="scope"> </param>
		public Name(Configuration cfg, string name, int hexCode, string scope) {
			this.config = cfg;
			if (name.Length > 15) {
				name = name.Substring(0, 15);
			}
			this.name = name.ToUpper();
			this.hexCode = hexCode;
			this.scope = scope != null && scope.Length > 0 ? scope : cfg.getNetbiosScope();
			this.srcHashCode = 0;
		}


		/// <param name="cfg"> </param>
		/// <param name="name"> </param>
		public Name(Configuration cfg, NetbiosName name) {
			this.config = cfg;
			this.name = name.getName();
			this.hexCode = name.getNameType();
			this.scope = name.getScope();
			if (name is Name) {
				this.srcHashCode = ((Name) name).srcHashCode;
			}
		}


		/// 
		/// <returns> whether this is the unknown address </returns>
		public virtual bool isUnknown() {
			return "0.0.0.0".Equals(this.name) && this.hexCode == 0 && this.scope==null;
		}


		internal virtual int writeWireFormat(byte[] dst, int dstIndex) {
			// write 0x20 in first byte
			dst[dstIndex] = 0x20;

			byte[] tmp = Strings.getOEMBytes(this.name, this.config);
			int i;
			for (i = 0; i < tmp.Length; i++) {
				dst[dstIndex + (2 * i + 1)] = (byte)(((tmp[i] & 0xF0) >> 4) + 0x41);
				dst[dstIndex + (2 * i + 2)] = (byte)((tmp[i] & 0x0F) + 0x41);
			}
			for (; i < 15; i++) {
				dst[dstIndex + (2 * i + 1)] = (byte) 0x43;
				dst[dstIndex + (2 * i + 2)] = (byte) 0x41;
			}
			dst[dstIndex + TYPE_OFFSET] = (byte)(((this.hexCode & 0xF0) >> 4) + 0x41);
			dst[dstIndex + TYPE_OFFSET + 1] = (byte)((this.hexCode & 0x0F) + 0x41);
			return SCOPE_OFFSET + writeScopeWireFormat(dst, dstIndex + SCOPE_OFFSET);
		}


		internal virtual int readWireFormat(byte[] src, int srcIndex) {

			byte[] tmp = new byte[SCOPE_OFFSET];
			int length = 15;
			for (int i = 0; i < 15; i++) {
				tmp[i] = (byte)(((src[srcIndex + (2 * i + 1)] & 0xFF) - 0x41) << 4);
				tmp[i] |= unchecked((byte)(((src[srcIndex + (2 * i + 2)] & 0xFF) - 0x41) & 0x0F));
				if (tmp[i] != (byte) ' ') {
					length = i + 1;
				}
			}
			this.name = Strings.fromOEMBytes(tmp, 0, length, this.config);
			this.hexCode = ((src[srcIndex + TYPE_OFFSET] & 0xFF) - 0x41) << 4;
			this.hexCode |= ((src[srcIndex + TYPE_OFFSET + 1] & 0xFF) - 0x41) & 0x0F;
			return SCOPE_OFFSET + readScopeWireFormat(src, srcIndex + SCOPE_OFFSET);
		}


		internal virtual int writeScopeWireFormat(byte[] dst, int dstIndex) {
			if (this.scope==null) {
				dst[dstIndex] = (byte) 0x00;
				return 1;
			}

			// copy new scope in
			dst[dstIndex++] = (byte) '.';
			Array.Copy(Strings.getOEMBytes(this.scope, this.config), 0, dst, dstIndex, this.scope.Length);
			dstIndex += this.scope.Length;

			dst[dstIndex++] = (byte) 0x00;

			// now go over scope backwards converting '.' to label length

			int i = dstIndex - 2;
			int e = i - this.scope.Length;
			int c = 0;

			do {
				if (dst[i] == (byte)'.') {
					dst[i] = (byte) c;
					c = 0;
				}
				else {
					c++;
				}
			} while (i-- > e);
			return this.scope.Length + 2;
		}


		internal virtual int readScopeWireFormat(byte[] src, int srcIndex) {
			int start = srcIndex;
			int n;
			StringBuilder sb;

			if ((n = src[srcIndex++] & 0xFF) == 0) {
				this.scope = null;
				return 1;
			}

			sb = new StringBuilder(Strings.fromOEMBytes(src, srcIndex, n, this.config));
			srcIndex += n;
			while ((n = src[srcIndex++] & 0xFF) != 0) {
				sb.Append('.').Append(Strings.fromOEMBytes(src, srcIndex, n, this.config));
				srcIndex += n;
			}
			this.scope = sb.ToString();

			return srcIndex - start;
		}


		public override int GetHashCode() {
			int result;

			result = this.name.GetHashCode();
			result += 65599 * this.hexCode;
			result += 65599 * this.srcHashCode; /*
	                                             * hashCode is different depending
	                                             * on where it came from
	                                             */
			if (this.scope!=null && this.scope.Length != 0) {
				result += this.scope.GetHashCode();
			}
			return result;
		}


		public override bool Equals(object obj) {
			Name n;

			if (!(obj is Name)) {
				return false;
			}
			n = (Name) obj;
			if (this.scope==null && n.scope==null) {
				return this.name.Equals(n.name) && this.hexCode == n.hexCode;
			}
			return this.name.Equals(n.name) && this.hexCode == n.hexCode && this.scope.Equals(n.scope);
		}


		public override string ToString() {
			StringBuilder sb = new StringBuilder();
			string n = this.name;

			// fix MSBROWSE name
			if (n == null) {
				n = "null";
			}
			else if (n[0] == (char)0x01) {
				char[] c = n.ToCharArray();
				c[0] = '.';
				c[1] = '.';
				c[14] = '.';
				n = new string(c);
			}

			sb.Append(n).Append("<").Append(Hexdump.toHexString(this.hexCode, 2)).Append(">");
			if (this.scope!=null) {
				sb.Append(".").Append(this.scope);
			}
			return sb.ToString();
		}
	}

}