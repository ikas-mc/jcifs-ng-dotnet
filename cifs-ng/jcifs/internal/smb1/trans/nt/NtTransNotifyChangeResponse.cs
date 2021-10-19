using System.Collections.Generic;
using Configuration = jcifs.Configuration;
using FileNotifyInformation = jcifs.FileNotifyInformation;
using NotifyResponse = jcifs.@internal.NotifyResponse;
using SMBProtocolDecodingException = jcifs.@internal.SMBProtocolDecodingException;

/* jcifs smb client library in Java
 * Copyright (C) 2005  "Michael B. Allen" <jcifs at samba dot org>
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

namespace jcifs.@internal.smb1.trans.nt {




	/// 
	public class NtTransNotifyChangeResponse : SmbComNtTransactionResponse, NotifyResponse {

		private IList<FileNotifyInformation> notifyInformation = new List<FileNotifyInformation>();


		/// 
		/// <param name="config"> </param>
		public NtTransNotifyChangeResponse(Configuration config) : base(config) {
		}


		/// <returns> the notifyInformation </returns>
		public IList<FileNotifyInformation> getNotifyInformation() {
			return this.notifyInformation;
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


		/// throws jcifs.internal.SMBProtocolDecodingException
		protected internal override int readParametersWireFormat(byte[] buffer, int bufferIndex, int len) {
			int start = bufferIndex;

			int elemStart = start;

			FileNotifyInformationImpl i = new FileNotifyInformationImpl();
			bufferIndex += i.decode(buffer, bufferIndex, len);
			this.notifyInformation.Add(i);

			while (i.getNextEntryOffset() > 0) {
				bufferIndex = elemStart + i.getNextEntryOffset();
				elemStart = bufferIndex;

				i = new FileNotifyInformationImpl();
				bufferIndex += i.decode(buffer, bufferIndex, len);
				this.notifyInformation.Add(i);
			}

			return bufferIndex - start;
		}


		protected internal override int readDataWireFormat(byte[] buffer, int bufferIndex, int len) {
			return 0;
		}


		public override string ToString() {
			return "NtTransQuerySecurityResponse[" + base.ToString() + "]";
		}
	}

}