using System;
using System.IO;
using Configuration = jcifs.Configuration;
using RuntimeCIFSException = jcifs.RuntimeCIFSException;
using SmbNegotiationRequest = jcifs.@internal.SmbNegotiationRequest;
using ServerMessageBlock = jcifs.@internal.smb1.ServerMessageBlock;
using Strings = jcifs.util.Strings;

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

namespace jcifs.@internal.smb1.com {




	/// 
	public class SmbComNegotiate : ServerMessageBlock, SmbNegotiationRequest {

		private readonly bool signingEnforced;
		private string[] dialects;


		/// 
		/// <param name="config"> </param>
		/// <param name="signingEnforced"> </param>
		public SmbComNegotiate(Configuration config, bool signingEnforced) : base(config, SMB_COM_NEGOTIATE) {
			this.signingEnforced = signingEnforced;
			setFlags2(config.getFlags2());

			if (config.getMinimumVersion().isSMB2()) {
				this.dialects = new string[] {"SMB 2.???", "SMB 2.002"};
			}
			else if (config.getMaximumVersion().isSMB2()) {
				this.dialects = new string[] {"NT LM 0.12", "SMB 2.???", "SMB 2.002"};
			}
			else {
				this.dialects = new string[] {"NT LM 0.12"};
			}
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.SmbNegotiationRequest#isSigningEnforced() </seealso>
		public virtual bool isSigningEnforced() {
			return this.signingEnforced;
		}


		protected internal override int writeParameterWordsWireFormat(byte[] dst, int dstIndex) {
			return 0;
		}


		protected internal override int writeBytesWireFormat(byte[] dst, int dstIndex) {
			MemoryStream bos = new MemoryStream();

			foreach (string dialect in this.dialects) {
				bos.WriteByte(0x02);
				try {
					bos.Write(Strings.getASCIIBytes(dialect), 0, Strings.getASCIIBytes(dialect).Length);
				}
				catch (IOException e) {
					throw new RuntimeCIFSException(e);
				}
				bos.WriteByte(0x0);
			}

			var data = bos.ToArray();
			Array.Copy(data, 0, dst, dstIndex, data.Length);
			return data.Length;
		}


		protected internal override int readParameterWordsWireFormat(byte[] buffer, int bufferIndex) {
			return 0;
		}


		protected internal override int readBytesWireFormat(byte[] buffer, int bufferIndex) {
			return 0;
		}


		public override string ToString() {
			return "SmbComNegotiate[" + base.ToString() + ",wordCount=" + this.wordCount + ",dialects=NT LM 0.12]";
		}
	}

}