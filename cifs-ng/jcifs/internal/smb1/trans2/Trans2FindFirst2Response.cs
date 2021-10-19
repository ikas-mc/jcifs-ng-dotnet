using Configuration = jcifs.Configuration;
using SMBProtocolDecodingException = jcifs.@internal.SMBProtocolDecodingException;
using FileBothDirectoryInfo = jcifs.@internal.fscc.FileBothDirectoryInfo;
using SmbComTransaction = jcifs.@internal.smb1.trans.SmbComTransaction;
using SmbComTransactionResponse = jcifs.@internal.smb1.trans.SmbComTransactionResponse;
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

namespace jcifs.@internal.smb1.trans2 {



	/// 
	public class Trans2FindFirst2Response : SmbComTransactionResponse {

		// information levels

		internal const int SMB_INFO_STANDARD = 1;
		internal const int SMB_INFO_QUERY_EA_SIZE = 2;
		internal const int SMB_INFO_QUERY_EAS_FROM_LIST = 3;
		internal const int SMB_FIND_FILE_DIRECTORY_INFO = 0x101;
		internal const int SMB_FIND_FILE_FULL_DIRECTORY_INFO = 0x102;
		internal const int SMB_FILE_NAMES_INFO = 0x103;
		internal const int SMB_FILE_BOTH_DIRECTORY_INFO = 0x104;

		private int sid;
		private bool isEndOfSearchField;
		private int eaErrorOffset;
		private int lastNameOffset, lastNameBufferIndex;
		private string lastName;
		private int resumeKey;


		/// 
		/// <param name="config"> </param>
		public Trans2FindFirst2Response(Configuration config) : base(config, SMB_COM_TRANSACTION2, SmbComTransaction.TRANS2_FIND_FIRST2) {
		}


		/// <returns> the sid </returns>
		public int getSid() {
			return this.sid;
		}


		/// <returns> the isEndOfSearch </returns>
		public bool isEndOfSearch() {
			return this.isEndOfSearchField;
		}


		/// <returns> the lastName </returns>
		public string getLastName() {
			return this.lastName;
		}


		/// <returns> the resumeKey </returns>
		public int getResumeKey() {
			return this.resumeKey;
		}


		protected internal override int writeSetupWireFormat(byte[] dst, int dstIndex) {
			return 0;
		}


		protected internal override int writeParametersWireFormat(byte[] dst, int dstIndex) {
			return 0;
		}


		protected internal override int writeDataWireFormat(byte[] dst, int dstIndex) {
			return 0;
		}


		protected internal override int readSetupWireFormat(byte[] buffer, int bufferIndex, int len) {
			return 0;
		}


		protected internal override int readParametersWireFormat(byte[] buffer, int bufferIndex, int len) {
			int start = bufferIndex;

			if (this.getSubCommand() == SmbComTransaction.TRANS2_FIND_FIRST2) {
				this.sid = SMBUtil.readInt2(buffer, bufferIndex);
				bufferIndex += 2;
			}
			this.setNumEntries(SMBUtil.readInt2(buffer, bufferIndex));
			bufferIndex += 2;
			this.isEndOfSearchField = (buffer[bufferIndex] & 0x01) == 0x01 ? true : false;
			bufferIndex += 2;
			this.eaErrorOffset = SMBUtil.readInt2(buffer, bufferIndex);
			bufferIndex += 2;
			this.lastNameOffset = SMBUtil.readInt2(buffer, bufferIndex);
			bufferIndex += 2;

			return bufferIndex - start;
		}


		/// throws jcifs.internal.SMBProtocolDecodingException
		protected internal override int readDataWireFormat(byte[] buffer, int bufferIndex, int len) {
			FileBothDirectoryInfo e;

			this.lastNameBufferIndex = bufferIndex + this.lastNameOffset;

			FileBothDirectoryInfo[] results = new FileBothDirectoryInfo[getNumEntries()];
			for (int i = 0; i < getNumEntries(); i++) {
				results[i] = e = new FileBothDirectoryInfo(getConfig(), isUseUnicode());

				e.decode(buffer, bufferIndex, len);

				/*
				 * lastNameOffset ends up pointing to either to
				 * the exact location of the filename(e.g. Win98)
				 * or to the start of the entry containing the
				 * filename(e.g. NT). Ahhrg! In either case the
				 * lastNameOffset falls between the start of the
				 * entry and the next entry.
				 */

				if (this.lastNameBufferIndex >= bufferIndex && (e.getNextEntryOffset() == 0 || this.lastNameBufferIndex < (bufferIndex + e.getNextEntryOffset()))) {
					this.lastName = e.getFilename();
					this.resumeKey = e.getFileIndex();
				}

				bufferIndex += e.getNextEntryOffset();
			}

			setResults(results);

			/*
			 * last nextEntryOffset for NT 4(but not 98) is 0 so we must
			 * use dataCount or our accounting will report an error for NT :~(
			 */
			return getDataCount();
		}


		public override string ToString() {
			string c;
			if (this.getSubCommand() == SmbComTransaction.TRANS2_FIND_FIRST2) {
				c = "Trans2FindFirst2Response[";
			}
			else {
				c = "Trans2FindNext2Response[";
			}
			return c + base.ToString() + ",sid=" + this.sid + ",searchCount=" + getNumEntries() + ",isEndOfSearch=" + this.isEndOfSearchField + ",eaErrorOffset=" + this.eaErrorOffset + ",lastNameOffset=" + this.lastNameOffset + ",lastName=" + this.lastName + "]";
		}
	}

}