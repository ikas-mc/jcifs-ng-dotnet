using jcifs.@internal.fscc;
using Configuration = jcifs.Configuration;
using FileInformation = jcifs.@internal.fscc.FileInformation;
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
	public class Trans2QueryPathInformation : SmbComTransaction {

		private readonly int informationLevel;


		/// 
		/// <param name="config"> </param>
		/// <param name="filename"> </param>
		/// <param name="informationLevel"> </param>
		public Trans2QueryPathInformation(Configuration config, string filename, int informationLevel) : base(config, SMB_COM_TRANSACTION2, TRANS2_QUERY_PATH_INFORMATION) {
			this.path = filename;
			this.informationLevel = informationLevel;
			this.totalDataCount = 0;
			this.maxParameterCount = 2;
			this.maxDataCount = 40;
			this.maxSetupCount = (byte) 0x00;
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
			dst[dstIndex++] = (byte) 0x00;
			dst[dstIndex++] = (byte) 0x00;
			dst[dstIndex++] = (byte) 0x00;
			dst[dstIndex++] = (byte) 0x00;
			dstIndex += writeString(this.path, dst, dstIndex);

			return dstIndex - start;
		}


		/// <param name="informationLevel2">
		/// @return </param>
		internal static long mapInformationLevel(int il) {
			switch (il) {
			case FileInformationConstants.FILE_BASIC_INFO:
				return 0x0101;
			case FileInformationConstants.FILE_STANDARD_INFO:
				return 0x0102;
			case FileInformationConstants.FILE_ENDOFFILE_INFO:
				return 0x0104;
			}
			throw new System.ArgumentException("Unsupported information level " + il);
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
			return "Trans2QueryPathInformation[" + base.ToString() + ",informationLevel=0x" + Hexdump.toHexString(this.informationLevel, 3) + ",filename=" + this.path + "]";
		}
	}

}