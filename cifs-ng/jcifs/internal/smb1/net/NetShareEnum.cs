using System;
using System.Text;
using cifs_ng.lib.ext;
using Configuration = jcifs.Configuration;
using SmbComTransaction = jcifs.@internal.smb1.trans.SmbComTransaction;
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

namespace jcifs.@internal.smb1.net {



	/// 
	/// 
	public class NetShareEnum : SmbComTransaction {

		private const string DESCR = "WrLeh\u0000B13BWz\u0000";


		/// 
		/// <param name="config"> </param>
		public NetShareEnum(Configuration config) : base(config, SMB_COM_TRANSACTION, NET_SHARE_ENUM) {
			this.name = "\\PIPE\\LANMAN";
			this.maxParameterCount = 8;

			// maxDataCount = 4096; why was this set?
			this.maxSetupCount = (byte) 0x00;
			this.setupCount = 0;
			this.timeout = 5000;
		}


		protected internal override int writeSetupWireFormat(byte[] dst, int dstIndex) {
			return 0;
		}


		protected internal override int writeParametersWireFormat(byte[] dst, int dstIndex) {
			int start = dstIndex;
			byte[] descr;

			try {
				descr = DESCR.getBytes(Encoding.ASCII);
			}
			catch (Exception) {
				return 0;
			}

			SMBUtil.writeInt2(NET_SHARE_ENUM, dst, dstIndex);
			dstIndex += 2;
			Array.Copy(descr, 0, dst, dstIndex, descr.Length);
			dstIndex += descr.Length;
			SMBUtil.writeInt2(0x0001, dst, dstIndex);
			dstIndex += 2;
			SMBUtil.writeInt2(this.maxDataCount, dst, dstIndex);
			dstIndex += 2;

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
			return "NetShareEnum[" + base.ToString() + "]";
		}
	}

}