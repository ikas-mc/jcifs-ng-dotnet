using jcifs.@internal.fscc;

using System;
using CIFSException = jcifs.CIFSException;
using Configuration = jcifs.Configuration;
using SMBProtocolDecodingException = jcifs.@internal.SMBProtocolDecodingException;
using FileBasicInfo = jcifs.@internal.fscc.FileBasicInfo;
using FileInformation = jcifs.@internal.fscc.FileInformation;
using FileInternalInfo = jcifs.@internal.fscc.FileInternalInfo;
using FileStandardInfo = jcifs.@internal.fscc.FileStandardInfo;
using SmbComTransaction = jcifs.@internal.smb1.trans.SmbComTransaction;
using SmbComTransactionResponse = jcifs.@internal.smb1.trans.SmbComTransactionResponse;

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
	public class Trans2QueryPathInformationResponse : SmbComTransactionResponse {

		private readonly int informationLevel;
		private FileInformation info;


		/// 
		/// <param name="config"> </param>
		/// <param name="informationLevel"> </param>
		public Trans2QueryPathInformationResponse(Configuration config, int informationLevel) : base(config) {
			this.informationLevel = informationLevel;
			this.setSubCommand(SmbComTransaction.TRANS2_QUERY_PATH_INFORMATION);
		}


		/// <returns> the info </returns>
		public FileInformation getInfo() {
			return this.info;
		}


		/// 
		/// <param name="type"> </param>
		/// <returns> the info </returns>
		/// <exception cref="CIFSException"> </exception>
		/// throws jcifs.CIFSException
		public virtual T getInfo<T>(Type type) where T : FileInformation {
			if (!type.IsAssignableFrom(this.info.GetType())) {
				throw new CIFSException("Incompatible file information class");
			}
			return (T) this.info;
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
			// observed two zero bytes here with at least win98
			return 2;
		}


		/// throws jcifs.internal.SMBProtocolDecodingException
		protected internal override int readDataWireFormat(byte[] buffer, int bufferIndex, int len) {
			int start = bufferIndex;
			FileInformation inf = createFileInformation();
			if (inf != null) {
				bufferIndex += inf.decode(buffer, bufferIndex, getDataCount());
				this.info = inf;
			}
			return bufferIndex - start;
		}


		private FileInformation createFileInformation() {
			FileInformation inf;
			switch (this.informationLevel) {
			case FileInformationConstants.FILE_BASIC_INFO:
				inf = new FileBasicInfo();
				break;
			case FileInformationConstants.FILE_STANDARD_INFO:
				inf = new FileStandardInfo();
				break;
			case FileInformationConstants.FILE_INTERNAL_INFO:
				inf = new FileInternalInfo();
				break;
			default:
				return null;
			}
			return inf;
		}


		public override string ToString() {
			return "Trans2QueryPathInformationResponse[" + base.ToString() + "]";
		}
	}

}