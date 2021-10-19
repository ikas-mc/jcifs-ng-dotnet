using Configuration = jcifs.Configuration;
using AndXServerMessageBlock = jcifs.@internal.smb1.AndXServerMessageBlock;
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
	public class SmbComReadAndX : AndXServerMessageBlock {

		private long offset;
		private int fid;
		internal int openTimeout;
		internal int maxCount, minCount, remaining;


		/// 
		/// <param name="config"> </param>
		public SmbComReadAndX(Configuration config) : base(config, SMB_COM_READ_ANDX) {
			this.openTimeout = unchecked((int)0xFFFFFFFF);
		}


		/// 
		/// <param name="config"> </param>
		/// <param name="fid"> </param>
		/// <param name="offset"> </param>
		/// <param name="maxCount"> </param>
		/// <param name="andx"> </param>
		public SmbComReadAndX(Configuration config, int fid, long offset, int maxCount, ServerMessageBlock andx) : base(config, SMB_COM_READ_ANDX, andx) {
			this.fid = fid;
			this.offset = offset;
			this.maxCount = this.minCount = maxCount;
			this.openTimeout = unchecked((int)0xFFFFFFFF);
		}


		/// <returns> the maxCount </returns>
		public int getMaxCount() {
			return this.maxCount;
		}


		/// <param name="maxCount">
		///            the maxCount to set </param>
		public void setMaxCount(int maxCount) {
			this.maxCount = maxCount;
		}


		/// <returns> the minCount </returns>
		public int getMinCount() {
			return this.minCount;
		}


		/// <param name="minCount">
		///            the minCount to set </param>
		public void setMinCount(int minCount) {
			this.minCount = minCount;
		}


		/// <returns> the remaining </returns>
		public int getRemaining() {
			return this.remaining;
		}


		/// <param name="openTimeout">
		///            the openTimeout to set </param>
		public void setOpenTimeout(int openTimeout) {
			this.openTimeout = openTimeout;
		}


		/// <param name="remaining">
		///            the remaining to set </param>
		public void setRemaining(int remaining) {
			this.remaining = remaining;
		}


		internal virtual void setParam(int fid, long offset, int maxCount) {
			this.fid = fid;
			this.offset = offset;
			this.maxCount = this.minCount = maxCount;
		}


		protected internal override int getBatchLimit(Configuration cfg, byte cmd) {
			return cmd == SMB_COM_CLOSE ? cfg.getBatchLimit("ReadAndX.Close") : 0;
		}


		protected internal override int writeParameterWordsWireFormat(byte[] dst, int dstIndex) {
			int start = dstIndex;

			SMBUtil.writeInt2(this.fid, dst, dstIndex);
			dstIndex += 2;
			SMBUtil.writeInt4(this.offset, dst, dstIndex);
			dstIndex += 4;
			SMBUtil.writeInt2(this.maxCount, dst, dstIndex);
			dstIndex += 2;
			SMBUtil.writeInt2(this.minCount, dst, dstIndex);
			dstIndex += 2;
			SMBUtil.writeInt4(this.openTimeout, dst, dstIndex);
			dstIndex += 4;
			SMBUtil.writeInt2(this.remaining, dst, dstIndex);
			dstIndex += 2;
			SMBUtil.writeInt4(this.offset >> 32, dst, dstIndex);
			dstIndex += 4;

			return dstIndex - start;
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
			return "SmbComReadAndX[" + base.ToString() + ",fid=" + this.fid + ",offset=" + this.offset + ",maxCount=" + this.maxCount + ",minCount=" + this.minCount + ",openTimeout=" + this.openTimeout + ",remaining=" + this.remaining + ",offset=" + this.offset + "]";
		}
	}

}