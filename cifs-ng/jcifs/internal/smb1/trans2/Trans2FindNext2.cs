using Configuration = jcifs.Configuration;
using SmbComTransaction = jcifs.@internal.smb1.trans.SmbComTransaction;
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

namespace jcifs.@internal.smb1.trans2 {



	/// 
	public class Trans2FindNext2 : SmbComTransaction {

		private int sid, informationLevel, resumeKey, tflags;
		private string filename;
		private long maxItems;


		/// 
		/// <param name="config"> </param>
		/// <param name="sid"> </param>
		/// <param name="resumeKey"> </param>
		/// <param name="filename"> </param>
		/// <param name="batchCount"> </param>
		/// <param name="batchSize"> </param>
		public Trans2FindNext2(Configuration config, int sid, int resumeKey, string filename, int batchCount, int batchSize) : base(config, SMB_COM_TRANSACTION2, TRANS2_FIND_NEXT2) {
			this.sid = sid;
			this.resumeKey = resumeKey;
			this.filename = filename;
			this.informationLevel = Trans2FindFirst2.SMB_FILE_BOTH_DIRECTORY_INFO;
			this.tflags = 0x00;
			this.maxParameterCount = 8;
			this.maxItems = batchCount;
			this.maxDataCount = batchSize;
			this.maxSetupCount = 0;
		}


		protected internal override void reset(int rk, string lastName) {
			base.reset();
			this.resumeKey = rk;
			this.filename = lastName;
			this.flags2 = 0;
		}


		protected internal override int writeSetupWireFormat(byte[] dst, int dstIndex) {
			dst[dstIndex++] = getSubCommand();
			dst[dstIndex++] = (byte) 0x00;
			return 2;
		}


		protected internal override int writeParametersWireFormat(byte[] dst, int dstIndex) {
			int start = dstIndex;

			SMBUtil.writeInt2(this.sid, dst, dstIndex);
			dstIndex += 2;
			SMBUtil.writeInt2(this.maxItems, dst, dstIndex);
			dstIndex += 2;
			SMBUtil.writeInt2(this.informationLevel, dst, dstIndex);
			dstIndex += 2;
			SMBUtil.writeInt4(this.resumeKey, dst, dstIndex);
			dstIndex += 4;
			SMBUtil.writeInt2(this.tflags, dst, dstIndex);
			dstIndex += 2;
			dstIndex += writeString(this.filename, dst, dstIndex);

			return dstIndex - start;
		}


		protected internal override int writeDataWireFormat(byte[] dst, int dstIndex) {
			return 0;
		}


		protected internal override int readSetupWireFormat(byte[] buffer, int bufferIndex, int len) {
			return 0;
		}


		protected internal override int readParametersWireFormat(byte[] buffer, int bufferIndex, int len) {
			return 0;
		}


		protected internal override int readDataWireFormat(byte[] buffer, int bufferIndex, int len) {
			return 0;
		}


		public override string ToString() {
			return "Trans2FindNext2[" + base.ToString() + ",sid=" + this.sid + ",searchCount=" + getConfig().getListSize() + ",informationLevel=0x" + Hexdump.toHexString(this.informationLevel, 3) + ",resumeKey=0x" + Hexdump.toHexString(this.resumeKey, 4) + ",flags=0x" + Hexdump.toHexString(this.tflags, 2) + ",filename=" + this.filename + "]";
		}
	}

}