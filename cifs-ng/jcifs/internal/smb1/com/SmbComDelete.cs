using Configuration = jcifs.Configuration;
using SmbConstants = jcifs.SmbConstants;
using ServerMessageBlock = jcifs.@internal.smb1.ServerMessageBlock;
using SMBUtil = jcifs.@internal.util.SMBUtil;
using Hexdump = jcifs.util.Hexdump;

/* jcifs smb client library in Java
 * Copyright (C) 2000  "Michael B. Allen" <jcifs at samba dot org>
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

namespace jcifs.@internal.smb1.com {



	/// 
	public class SmbComDelete : ServerMessageBlock {

		private int searchAttributes;


		/// 
		/// <param name="config"> </param>
		/// <param name="path"> </param>
		public SmbComDelete(Configuration config, string path) : base(config, SMB_COM_DELETE, path) {
			this.searchAttributes = SmbConstants.ATTR_HIDDEN | SmbConstants.ATTR_HIDDEN | SmbConstants.ATTR_SYSTEM;
		}


		protected internal override int writeParameterWordsWireFormat(byte[] dst, int dstIndex) {
			SMBUtil.writeInt2(this.searchAttributes, dst, dstIndex);
			return 2;
		}


		protected internal override int writeBytesWireFormat(byte[] dst, int dstIndex) {
			int start = dstIndex;

			dst[dstIndex++] = (byte) 0x04;
			dstIndex += writeString(this.path, dst, dstIndex);

			return dstIndex - start;
		}


		protected internal override int readParameterWordsWireFormat(byte[] buffer, int bufferIndex) {
			return 0;
		}


		protected internal override int readBytesWireFormat(byte[] buffer, int bufferIndex) {
			return 0;
		}


		public override string ToString() {
			return "SmbComDelete[" + base.ToString() + ",searchAttributes=0x" + Hexdump.toHexString(this.searchAttributes, 4) + ",fileName=" + this.path + "]";
		}
	}

}