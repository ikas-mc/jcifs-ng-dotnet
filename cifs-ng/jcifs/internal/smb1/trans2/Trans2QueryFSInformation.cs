using jcifs.@internal.fscc;
using Configuration = jcifs.Configuration;
using FileSystemInformation = jcifs.@internal.fscc.FileSystemInformation;
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
	public class Trans2QueryFSInformation : SmbComTransaction {

		private int informationLevel;


		/// 
		/// <param name="config"> </param>
		/// <param name="informationLevel"> </param>
		public Trans2QueryFSInformation(Configuration config, int informationLevel) : base(config, SMB_COM_TRANSACTION2, TRANS2_QUERY_FS_INFORMATION) {
			this.informationLevel = informationLevel;
			this.totalParameterCount = 2;
			this.totalDataCount = 0;
			this.maxParameterCount = 0;
			this.maxDataCount = 800;
			this.maxSetupCount = 0;
		}


		protected internal override int writeSetupWireFormat(byte[] dst, int dstIndex) {
			dst[dstIndex++] = this.getSubCommand();
			dst[dstIndex++] = (byte) 0x00;
			return 2;
		}


		protected internal override int writeParametersWireFormat(byte[] dst, int dstIndex) {
			int start = dstIndex;

			SMBUtil.writeInt2(mapInformationLevel(this.informationLevel), dst, dstIndex);
			dstIndex += 2;

			/*
			 * windows98 has what appears to be another 4 0's followed by the share
			 * name as a zero terminated ascii string "\TMP" + '\0'
			 *
			 * As is this works, but it deviates from the spec section 4.1.6.6 but
			 * maybe I should put it in. Wonder what NT does?
			 */

			return dstIndex - start;
		}


		/// <param name="il">
		/// @return </param>
		private static int mapInformationLevel(int il) {
			switch (il) {
			case FileSystemInformationConstants.SMB_INFO_ALLOCATION:
				return 0x1;
			case FileSystemInformationConstants.FS_SIZE_INFO:
				return 0x103;
			}
			throw new System.ArgumentException("Unhandled information level");
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
			return "Trans2QueryFSInformation[" + base.ToString() + ",informationLevel=0x" + Hexdump.toHexString(this.informationLevel, 3) + "]";
		}
	}

}