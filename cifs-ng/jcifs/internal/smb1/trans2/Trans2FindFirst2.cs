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
	/// <summary>
	/// @author mbechler
	/// 
	/// </summary>
	public class Trans2FindFirst2 : SmbComTransaction {

		// flags

		internal const int FLAGS_CLOSE_AFTER_THIS_REQUEST = 0x01;
		internal const int FLAGS_CLOSE_IF_END_REACHED = 0x02;
		internal const int FLAGS_RETURN_RESUME_KEYS = 0x04;
		internal const int FLAGS_RESUME_FROM_PREVIOUS_END = 0x08;
		internal const int FLAGS_FIND_WITH_BACKUP_INTENT = 0x10;

		private int searchAttributes;
		private int tflags;
		private int informationLevel;
		private int searchStorageType = 0;
		private int maxItems;
		private string wildcard;

		// information levels

		internal const int SMB_INFO_STANDARD = 1;
		internal const int SMB_INFO_QUERY_EA_SIZE = 2;
		internal const int SMB_INFO_QUERY_EAS_FROM_LIST = 3;
		internal const int SMB_FIND_FILE_DIRECTORY_INFO = 0x101;
		internal const int SMB_FIND_FILE_FULL_DIRECTORY_INFO = 0x102;
		internal const int SMB_FILE_NAMES_INFO = 0x103;
		internal const int SMB_FILE_BOTH_DIRECTORY_INFO = 0x104;


		/// 
		/// <param name="config"> </param>
		/// <param name="filename"> </param>
		/// <param name="wildcard"> </param>
		/// <param name="searchAttributes"> </param>
		/// <param name="batchCount"> </param>
		/// <param name="batchSize"> </param>
		public Trans2FindFirst2(Configuration config, string filename, string wildcard, int searchAttributes, int batchCount, int batchSize) : base(config, SMB_COM_TRANSACTION2, TRANS2_FIND_FIRST2) {
			if (filename.Equals("\\")) {
				this.path = filename;
			}
			else if (filename[filename.Length - 1] != '\\') {
				this.path = filename + "\\";
			}
			else {
				this.path = filename;
			}
			this.wildcard = wildcard;
			this.searchAttributes = searchAttributes & 0x37; // generally ignored tho

			this.tflags = 0x00;
			this.informationLevel = SMB_FILE_BOTH_DIRECTORY_INFO;

			this.totalDataCount = 0;
			this.maxParameterCount = 10;
			this.maxItems = batchCount;
			this.maxDataCount = batchSize;
			this.maxSetupCount = 0;
		}


		protected internal override int writeSetupWireFormat(byte[] dst, int dstIndex) {
			dst[dstIndex++] = getSubCommand();
			dst[dstIndex++] = (byte) 0x00;
			return 2;
		}


		protected internal override int writeParametersWireFormat(byte[] dst, int dstIndex) {
			int start = dstIndex;

			SMBUtil.writeInt2(this.searchAttributes, dst, dstIndex);
			dstIndex += 2;
			SMBUtil.writeInt2(this.maxItems, dst, dstIndex);
			dstIndex += 2;
			SMBUtil.writeInt2(this.tflags, dst, dstIndex);
			dstIndex += 2;
			SMBUtil.writeInt2(this.informationLevel, dst, dstIndex);
			dstIndex += 2;
			SMBUtil.writeInt4(this.searchStorageType, dst, dstIndex);
			dstIndex += 4;
			dstIndex += writeString(this.path + this.wildcard, dst, dstIndex);

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
			return "Trans2FindFirst2[" + base.ToString() + ",searchAttributes=0x" + Hexdump.toHexString(this.searchAttributes, 2) + ",searchCount=" + this.maxItems + ",flags=0x" + Hexdump.toHexString(this.tflags, 2) + ",informationLevel=0x" + Hexdump.toHexString(this.informationLevel, 3) + ",searchStorageType=" + this.searchStorageType + ",filename=" + this.path + "]";
		}
	}

}