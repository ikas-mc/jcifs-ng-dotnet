using cifs_ng.lib.ext;
using SmbConstants = jcifs.SmbConstants;
using FileEntry = jcifs.smb.FileEntry;
using Hexdump = jcifs.util.Hexdump;

/* jcifs smb client library in Java
 * Copyright (C) 2007  "Michael B. Allen" <jcifs at samba dot org>
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

namespace jcifs.@internal.smb1.net {



	/// <summary>
	/// Internal use only
	/// 
	/// @internal
	/// </summary>
	public class SmbShareInfo : FileEntry {

		protected internal string netName;
		protected internal int type;
		protected internal string remark;


		/// 
		public SmbShareInfo() {
		}


		/// 
		/// <param name="netName"> </param>
		/// <param name="type"> </param>
		/// <param name="remark"> </param>
		public SmbShareInfo(string netName, int type, string remark) {
			this.netName = netName;
			this.type = type;
			this.remark = remark;
		}


		public virtual string getName() {
			return this.netName;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.smb.FileEntry#getFileIndex() </seealso>
		public virtual int getFileIndex() {
			return 0;
		}


		public virtual int getType() {
			/*
			 * 0x80000000 means hidden but SmbFile.isHidden() checks for $ at end
			 */
			switch (this.type & 0xFFFF) {
			case 1:
				return SmbConstants.TYPE_PRINTER;
			case 3:
				return SmbConstants.TYPE_NAMED_PIPE;
			}
			return SmbConstants.TYPE_SHARE;
		}


		public virtual int getAttributes() {
			return SmbConstants.ATTR_READONLY | SmbConstants.ATTR_DIRECTORY;
		}


		public virtual long createTime() {
			return 0L;
		}


		public virtual long lastModified() {
			return 0L;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.smb.FileEntry#lastAccess() </seealso>
		public virtual long lastAccess() {
			return 0L;
		}


		public virtual long length() {
			return 0L;
		}


		public override bool Equals(object obj) {
			if (obj is SmbShareInfo) {
				SmbShareInfo si = (SmbShareInfo) obj;
				return Equals(this.netName, si.netName);
			}
			return false;
		}


		public override int GetHashCode() {
			return RuntimeHelp.hashCode(this.netName);
		}


		public override string ToString() {
			return "SmbShareInfo[" + "netName=" + this.netName + ",type=0x" + Hexdump.toHexString(this.type, 8) + ",remark=" + this.remark + "]";
		}
	}

}