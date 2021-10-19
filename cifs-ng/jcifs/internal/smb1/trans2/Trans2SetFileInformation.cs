using Configuration = jcifs.Configuration;
using FileBasicInfo = jcifs.@internal.fscc.FileBasicInfo;
using FileInformation = jcifs.@internal.fscc.FileInformation;
using SmbComTransaction = jcifs.@internal.smb1.trans.SmbComTransaction;
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

namespace jcifs.@internal.smb1.trans2 {



	/// 
	public class Trans2SetFileInformation : SmbComTransaction {

		private readonly int fid;
		private readonly FileInformation info;


		/// <param name="config"> </param>
		/// <param name="fid"> </param>
		/// <param name="info">
		///  </param>
		public Trans2SetFileInformation(Configuration config, int fid, FileInformation info) : base(config, SMB_COM_TRANSACTION2, TRANS2_SET_FILE_INFORMATION) {
			this.fid = fid;
			this.info = info;
			this.maxParameterCount = 6;
			this.maxDataCount = 0;
			this.maxSetupCount = (byte) 0x00;
		}


		/// 
		/// <param name="config"> </param>
		/// <param name="fid"> </param>
		/// <param name="attributes"> </param>
		/// <param name="createTime"> </param>
		/// <param name="lastWriteTime"> </param>
		/// <param name="lastAccessTime"> </param>
		public Trans2SetFileInformation(Configuration config, int fid, int attributes, long createTime, long lastWriteTime, long lastAccessTime) : this(config, fid, new FileBasicInfo(createTime, lastAccessTime, lastWriteTime, 0L, attributes | 0x80)) {
		}


		protected internal override int writeSetupWireFormat(byte[] dst, int dstIndex) {
			dst[dstIndex++] = this.getSubCommand();
			dst[dstIndex++] = (byte) 0x00;
			return 2;
		}


		protected internal override int writeParametersWireFormat(byte[] dst, int dstIndex) {
			int start = dstIndex;

			SMBUtil.writeInt2(this.fid, dst, dstIndex);
			dstIndex += 2;
			SMBUtil.writeInt2(Trans2QueryPathInformation.mapInformationLevel(this.info.getFileInformationLevel()), dst, dstIndex);
			dstIndex += 2;
			SMBUtil.writeInt2(0, dst, dstIndex);
			dstIndex += 2;

			return dstIndex - start;
		}


		protected internal override int writeDataWireFormat(byte[] dst, int dstIndex) {
			int start = dstIndex;
			dstIndex += this.info.encode(dst, dstIndex);

			/* 6 zeros observed with NT */
			SMBUtil.writeInt8(0L, dst, dstIndex);
			dstIndex += 6;

			/*
			 * Also observed 4 byte alignment but we stick
			 * with the default for jCIFS which is 2
			 */

			return dstIndex - start;
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
			return "Trans2SetFileInformation[" + base.ToString() + ",fid=" + this.fid + "]";
		}
	}

}