using jcifs.@internal.fscc;

using System;
using CIFSException = jcifs.CIFSException;
using Configuration = jcifs.Configuration;
using AllocInfo = jcifs.@internal.AllocInfo;
using SMBProtocolDecodingException = jcifs.@internal.SMBProtocolDecodingException;
using FileFsFullSizeInformation = jcifs.@internal.fscc.FileFsFullSizeInformation;
using FileFsSizeInformation = jcifs.@internal.fscc.FileFsSizeInformation;
using FileSystemInformation = jcifs.@internal.fscc.FileSystemInformation;
using SmbInfoAllocation = jcifs.@internal.fscc.SmbInfoAllocation;
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
	public class Trans2QueryFSInformationResponse : SmbComTransactionResponse {

		private int informationLevel;
		private FileSystemInformation info;


		/// 
		/// <param name="config"> </param>
		/// <param name="informationLevel"> </param>
		public Trans2QueryFSInformationResponse(Configuration config, int informationLevel) : base(config) {
			this.informationLevel = informationLevel;
			this.setCommand(SMB_COM_TRANSACTION2);
			this.setSubCommand(SmbComTransaction.TRANS2_QUERY_FS_INFORMATION);
		}


		/// <returns> the informationLevel </returns>
		public virtual int getInformationLevel() {
			return this.informationLevel;
		}


		/// <returns> the filesystem info </returns>
		public virtual FileSystemInformation getInfo() {
			return this.info;
		}


		/// <param name="clazz"> </param>
		/// <returns> the filesystem info </returns>
		/// <exception cref="CIFSException"> </exception>
		/// throws jcifs.CIFSException
		public virtual T getInfo<T>(Type clazz) where T : FileSystemInformation {
			if (!clazz.IsAssignableFrom(this.info.GetType())) {
				throw new CIFSException("Incompatible file information class");
			}
			return (T) getInfo();
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
			return 0;
		}


		/// throws jcifs.internal.SMBProtocolDecodingException
		protected internal override int readDataWireFormat(byte[] buffer, int bufferIndex, int len) {
			int start = bufferIndex;
			AllocInfo inf = createInfo();
			if (inf != null) {
				bufferIndex += inf.decode(buffer, bufferIndex, getDataCount());
				this.info = inf;
			}
			return bufferIndex - start;
		}


		/// <summary>
		/// @return
		/// </summary>
		private AllocInfo createInfo() {
			AllocInfo inf;
			switch (this.informationLevel) {
			case FileSystemInformationConstants.SMB_INFO_ALLOCATION:
				inf = new SmbInfoAllocation();
				break;
			case FileSystemInformationConstants.FS_SIZE_INFO:
				inf = new FileFsSizeInformation();
				break;
			case FileSystemInformationConstants.FS_FULL_SIZE_INFO:
				inf = new FileFsFullSizeInformation();
				break;
			default:
				return null;
			}
			return inf;
		}


		public override string ToString() {
			return "Trans2QueryFSInformationResponse[" + base.ToString() + "]";
		}

	}

}