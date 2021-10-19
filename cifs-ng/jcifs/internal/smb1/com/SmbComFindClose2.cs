using Configuration = jcifs.Configuration;
using ServerMessageBlock = jcifs.@internal.smb1.ServerMessageBlock;
using SMBUtil = jcifs.@internal.util.SMBUtil;

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
	public class SmbComFindClose2 : ServerMessageBlock {

		private int sid;


		/// 
		/// <param name="config"> </param>
		/// <param name="sid"> </param>
		public SmbComFindClose2(Configuration config, int sid) : base(config, SMB_COM_FIND_CLOSE2) {
			this.sid = sid;
		}


		protected internal override int writeParameterWordsWireFormat(byte[] dst, int dstIndex) {
			SMBUtil.writeInt2(this.sid, dst, dstIndex);
			return 2;
		}


		protected internal override int writeBytesWireFormat(byte[] dst, int dstIndex) {
			return 0;
		}


		protected internal override int readParameterWordsWireFormat(byte[] buffer, int bufferIndex) {
			return 0;
		}


		protected internal override int readBytesWireFormat(byte[] buffer, int bufferIndex) {
			return 0;
		}


		public override string ToString() {
			return "SmbComFindClose2[" + base.ToString() + ",sid=" + this.sid + "]";
		}
	}

}