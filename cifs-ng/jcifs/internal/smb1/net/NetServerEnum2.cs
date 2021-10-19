using System;
using System.Text;
using cifs_ng.lib.ext;
using Configuration = jcifs.Configuration;
using SmbComTransaction = jcifs.@internal.smb1.trans.SmbComTransaction;
using SMBUtil = jcifs.@internal.util.SMBUtil;

/* jcifs smb client library in Java
 * Copyright (C) 2000  "Michael B. Allen" <jcifs at samba dot org>
 *                             Gary Rambo <grambo aventail.com>
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
	public class NetServerEnum2 : SmbComTransaction {

		/// 
		public const int SV_TYPE_ALL = unchecked((int)0xFFFFFFFF);

		/// 
		public const int SV_TYPE_DOMAIN_ENUM = unchecked((int)0x80000000);

		internal static readonly string[] DESCR = new string[] {"WrLehDO\u0000B16BBDz\u0000", "WrLehDz\u0000B16BBDz\u0000"};

		internal string domain, lastName = null;
		internal int serverTypes;


		/// 
		/// <param name="config"> </param>
		/// <param name="domain"> </param>
		/// <param name="serverTypes"> </param>
		public NetServerEnum2(Configuration config, string domain, int serverTypes) : base(config, SMB_COM_TRANSACTION, NET_SERVER_ENUM2) {
			this.domain = domain;
			this.serverTypes = serverTypes;
			this.name = "\\PIPE\\LANMAN";

			this.maxParameterCount = 8;
			this.maxDataCount = 16384;
			this.maxSetupCount = (byte) 0x00;
			this.setupCount = 0;
			this.timeout = 5000;
		}


		protected internal override void reset(int key, string lastN) {
			base.reset();
			this.lastName = lastN;
		}


		protected internal override int writeSetupWireFormat(byte[] dst, int dstIndex) {
			return 0;
		}


		protected internal override int writeParametersWireFormat(byte[] dst, int dstIndex) {
			int start = dstIndex;
			byte[] descr;
			int which = getSubCommand() == NET_SERVER_ENUM2 ? 0 : 1;

			try {
				descr = DESCR[which].getBytes(Encoding.ASCII);
			}
			catch (Exception) {
				return 0;
			}

			SMBUtil.writeInt2(getSubCommand() & 0xFF, dst, dstIndex);
			dstIndex += 2;
			Array.Copy(descr, 0, dst, dstIndex, descr.Length);
			dstIndex += descr.Length;
			SMBUtil.writeInt2(0x0001, dst, dstIndex);
			dstIndex += 2;
			SMBUtil.writeInt2(this.maxDataCount, dst, dstIndex);
			dstIndex += 2;
			SMBUtil.writeInt4(this.serverTypes, dst, dstIndex);
			dstIndex += 4;
			dstIndex += writeString(this.domain.ToUpper(), dst, dstIndex, false);
			if (which == 1) {
				dstIndex += writeString(this.lastName.ToUpper(), dst, dstIndex, false);
			}

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
			return "NetServerEnum2[" + base.ToString() + ",name=" + this.name + ",serverTypes=" + (this.serverTypes == SV_TYPE_ALL ? "SV_TYPE_ALL" : "SV_TYPE_DOMAIN_ENUM") + "]";
		}
	}

}