using System;
using Configuration = jcifs.Configuration;
using SMBProtocolDecodingException = jcifs.@internal.SMBProtocolDecodingException;

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

namespace jcifs.@internal.smb1.trans {



	/// 
	public class TransCallNamedPipeResponse : SmbComTransactionResponse {

		private readonly byte[] outputBuffer;


		/// <param name="config"> </param>
		/// <param name="inB"> </param>
		public TransCallNamedPipeResponse(Configuration config, byte[] inB) : base(config) {
			this.outputBuffer = inB;
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
			if (len > this.outputBuffer.Length) {
				throw new SMBProtocolDecodingException("Payload exceeds buffer size");
			}
			Array.Copy(buffer, bufferIndex, this.outputBuffer, 0, len);
			return len;
		}


		public override string ToString() {
			return "TransCallNamedPipeResponse[" + base.ToString() + "]";
		}


		/// 
		/// <returns> response data length </returns>
		public virtual int getResponseLength() {
			return getDataCount();
		}
	}

}