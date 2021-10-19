using Configuration = jcifs.Configuration;
using AndXServerMessageBlock = jcifs.@internal.smb1.AndXServerMessageBlock;
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
	public class SmbComReadAndXResponse : AndXServerMessageBlock {

		private byte[] data;
		private int offset, dataCompactionMode, dataLength, dataOffset;


		/// 
		/// <param name="config"> </param>
		public SmbComReadAndXResponse(Configuration config) : base(config) {
		}


		/// 
		/// <param name="config"> </param>
		/// <param name="b"> </param>
		/// <param name="off"> </param>
		public SmbComReadAndXResponse(Configuration config, byte[] b, int off) : base(config) {
			this.data = b;
			this.offset = off;
		}


		internal virtual void setParam(byte[] b, int off) {
			this.data = b;
			this.offset = off;
		}


		/// 
		/// <returns> the read data </returns>
		public byte[] getData() {
			return this.data;
		}


		/// <returns> the offset </returns>
		public int getOffset() {
			return this.offset;
		}


		/// <param name="n"> </param>
		public virtual void adjustOffset(int n) {
			this.offset += n;
		}


		/// <returns> the dataLength </returns>
		public int getDataLength() {
			return this.dataLength;
		}


		/// <returns> the dataOffset </returns>
		public int getDataOffset() {
			return this.dataOffset;
		}


		protected internal override int writeParameterWordsWireFormat(byte[] dst, int dstIndex) {
			return 0;
		}


		protected internal override int writeBytesWireFormat(byte[] dst, int dstIndex) {
			return 0;
		}


		protected internal override int readParameterWordsWireFormat(byte[] buffer, int bufferIndex) {
			int start = bufferIndex;

			bufferIndex += 2; // reserved
			this.dataCompactionMode = SMBUtil.readInt2(buffer, bufferIndex);
			bufferIndex += 4; // 2 reserved
			this.dataLength = SMBUtil.readInt2(buffer, bufferIndex);
			bufferIndex += 2;
			this.dataOffset = SMBUtil.readInt2(buffer, bufferIndex);
			bufferIndex += 12; // 10 reserved

			return bufferIndex - start;
		}


		protected internal override int readBytesWireFormat(byte[] buffer, int bufferIndex) {
			// handled special in SmbTransport.doRecv()
			return 0;
		}


		public override string ToString() {
			return "SmbComReadAndXResponse[" + base.ToString() + ",dataCompactionMode=" + this.dataCompactionMode + ",dataLength=" + this.dataLength + ",dataOffset=" + this.dataOffset + "]";
		}

	}

}