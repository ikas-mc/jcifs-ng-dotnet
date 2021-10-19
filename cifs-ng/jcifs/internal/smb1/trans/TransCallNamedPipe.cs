using System;
using Logger = org.slf4j.Logger;
using LoggerFactory = org.slf4j.LoggerFactory;
using Configuration = jcifs.Configuration;

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
	public class TransCallNamedPipe : SmbComTransaction {

		private static readonly Logger log = LoggerFactory.getLogger(typeof(TransCallNamedPipe));

		private byte[] pipeData;
		private int pipeDataOff, pipeDataLen;


		/// 
		/// <param name="config"> </param>
		/// <param name="pipeName"> </param>
		/// <param name="data"> </param>
		/// <param name="off"> </param>
		/// <param name="len"> </param>
		public TransCallNamedPipe(Configuration config, string pipeName, byte[] data, int off, int len) : base(config, SMB_COM_TRANSACTION, TRANS_CALL_NAMED_PIPE) {
			this.name = pipeName;
			this.pipeData = data;
			this.pipeDataOff = off;
			this.pipeDataLen = len;
			this.timeout = unchecked((int)0xFFFFFFFF);
			this.maxParameterCount = 0;
			this.maxDataCount = 0xFFFF;
			this.maxSetupCount = (byte) 0x00;
			this.setupCount = 2;
		}


		protected internal override int writeSetupWireFormat(byte[] dst, int dstIndex) {
			dst[dstIndex++] = this.getSubCommand();
			dst[dstIndex++] = (byte) 0x00;
			// this says "Transaction priority" in netmon
			dst[dstIndex++] = (byte) 0x00; // no FID
			dst[dstIndex++] = (byte) 0x00;
			return 4;
		}


		protected internal override int readSetupWireFormat(byte[] buffer, int bufferIndex, int len) {
			return 0;
		}


		protected internal override int writeParametersWireFormat(byte[] dst, int dstIndex) {
			return 0;
		}


		protected internal override int writeDataWireFormat(byte[] dst, int dstIndex) {
			if ((dst.Length - dstIndex) < this.pipeDataLen) {
				log.debug("TransCallNamedPipe data too long for buffer");
				return 0;
			}
			Array.Copy(this.pipeData, this.pipeDataOff, dst, dstIndex, this.pipeDataLen);
			return this.pipeDataLen;
		}


		protected internal override int readParametersWireFormat(byte[] buffer, int bufferIndex, int len) {
			return 0;
		}


		protected internal override int readDataWireFormat(byte[] buffer, int bufferIndex, int len) {
			return 0;
		}


		public override string ToString() {
			return "TransCallNamedPipe[" + base.ToString() + ",pipeName=" + this.name + "]";
		}
	}

}