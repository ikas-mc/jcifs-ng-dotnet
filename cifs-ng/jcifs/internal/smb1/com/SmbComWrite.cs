using System;
using Configuration = jcifs.Configuration;
using ServerMessageBlock = jcifs.@internal.smb1.ServerMessageBlock;
using SMBUtil = jcifs.@internal.util.SMBUtil;

/* jcifs smb client library in Java
 * Copyright (C) 2003  "Michael B. Allen" <jcifs at samba dot org>
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
	public class SmbComWrite : ServerMessageBlock {

		private int fid, count, offset, remaining, off;
		private byte[] b;


		/// 
		/// <param name="config"> </param>
		public SmbComWrite(Configuration config) : base(config, SMB_COM_WRITE) {
		}


		/// 
		/// <param name="config"> </param>
		/// <param name="fid"> </param>
		/// <param name="offset"> </param>
		/// <param name="remaining"> </param>
		/// <param name="b"> </param>
		/// <param name="off"> </param>
		/// <param name="len"> </param>
		public SmbComWrite(Configuration config, int fid, int offset, int remaining, byte[] b, int off, int len) : base(config, SMB_COM_WRITE) {
			this.fid = fid;
			this.count = len;
			this.offset = offset;
			this.remaining = remaining;
			this.b = b;
			this.off = off;
		}


		/// 
		/// <param name="fid"> </param>
		/// <param name="offset"> </param>
		/// <param name="remaining"> </param>
		/// <param name="b"> </param>
		/// <param name="off"> </param>
		/// <param name="len"> </param>
		public void setParam(int fid, long offset, int remaining, byte[] b, int off, int len) {
			this.fid = fid;
			this.offset = unchecked((int)(offset & 0xFFFFFFFFL));
			this.remaining = remaining;
			this.b = b;
			this.off = off;
			this.count = len;
			this.digest = null; /*
	                             * otherwise recycled commands
	                             * like writeandx will choke if session
	                             * closes in between
	                             */
		}


		protected internal override int writeParameterWordsWireFormat(byte[] dst, int dstIndex) {
			int start = dstIndex;

			SMBUtil.writeInt2(this.fid, dst, dstIndex);
			dstIndex += 2;
			SMBUtil.writeInt2(this.count, dst, dstIndex);
			dstIndex += 2;
			SMBUtil.writeInt4(this.offset, dst, dstIndex);
			dstIndex += 4;
			SMBUtil.writeInt2(this.remaining, dst, dstIndex);
			dstIndex += 2;

			return dstIndex - start;
		}


		protected internal override int writeBytesWireFormat(byte[] dst, int dstIndex) {
			int start = dstIndex;

			dst[dstIndex++] = (byte) 0x01; // BufferFormat
			SMBUtil.writeInt2(this.count, dst, dstIndex); // DataLength?
			dstIndex += 2;
			Array.Copy(this.b, this.off, dst, dstIndex, this.count);
			dstIndex += this.count;

			return dstIndex - start;
		}


		protected internal override int readParameterWordsWireFormat(byte[] buffer, int bufferIndex) {
			return 0;
		}


		protected internal override int readBytesWireFormat(byte[] buffer, int bufferIndex) {
			return 0;
		}


		public override string ToString() {
			return "SmbComWrite[" + base.ToString() + ",fid=" + this.fid + ",count=" + this.count + ",offset=" + this.offset + ",remaining=" + this.remaining + "]";
		}
	}

}