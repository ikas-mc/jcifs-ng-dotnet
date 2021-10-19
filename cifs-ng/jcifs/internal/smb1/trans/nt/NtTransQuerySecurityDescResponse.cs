using System.IO;
using Configuration = jcifs.Configuration;
using RuntimeCIFSException = jcifs.RuntimeCIFSException;
using SecurityDescriptor = jcifs.@internal.dtyp.SecurityDescriptor;
using SMBUtil = jcifs.@internal.util.SMBUtil;

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
	public class NtTransQuerySecurityDescResponse : SmbComNtTransactionResponse {

		private SecurityDescriptor securityDescriptor;


		/// 
		/// <param name="config"> </param>
		public NtTransQuerySecurityDescResponse(Configuration config) : base(config) {
		}


		/// <returns> the securityDescriptor </returns>
		public SecurityDescriptor getSecurityDescriptor() {
			return this.securityDescriptor;
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
			this.length = SMBUtil.readInt4(buffer, bufferIndex);
			return 4;
		}


		protected internal override int readDataWireFormat(byte[] buffer, int bufferIndex, int len) {
			int start = bufferIndex;

			if (this.getErrorCode() != 0) {
				return 4;
			}

			try {
				this.securityDescriptor = new SecurityDescriptor();
				bufferIndex += this.securityDescriptor.decode(buffer, bufferIndex, len);
			}
			catch (IOException ioe) {
				throw new RuntimeCIFSException(ioe.Message);
			}

			return bufferIndex - start;
		}


		public override string ToString() {
			return "NtTransQuerySecurityResponse[" + base.ToString() + "]";
		}
	}

}