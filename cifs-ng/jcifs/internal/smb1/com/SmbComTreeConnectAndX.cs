using jcifs.smb;

using System;
using System.Text;
using cifs_ng.lib.ext;
using CIFSContext = jcifs.CIFSContext;
using Configuration = jcifs.Configuration;
using RuntimeCIFSException = jcifs.RuntimeCIFSException;
using SmbConstants = jcifs.SmbConstants;
using AndXServerMessageBlock = jcifs.@internal.smb1.AndXServerMessageBlock;
using ServerMessageBlock = jcifs.@internal.smb1.ServerMessageBlock;
using SMBUtil = jcifs.@internal.util.SMBUtil;
using NtlmPasswordAuthenticator = jcifs.smb.NtlmPasswordAuthenticator;
using Hexdump = jcifs.util.Hexdump;

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
	public class SmbComTreeConnectAndX : AndXServerMessageBlock {

		private bool disconnectTid = false;
		private string service;
		private byte[] password;
		private int passwordLength;
		private CIFSContext ctx;
		private ServerData server;


		/// 
		/// <param name="ctx"> </param>
		/// <param name="server"> </param>
		/// <param name="path"> </param>
		/// <param name="service"> </param>
		/// <param name="andx"> </param>
		public SmbComTreeConnectAndX(CIFSContext ctx, ServerData server, string path, string service, ServerMessageBlock andx) : base(ctx.getConfig(), SMB_COM_TREE_CONNECT_ANDX, andx) {
			this.ctx = ctx;
			this.server = server;
			this.path = path;
			this.service = service;
		}


		protected internal override int getBatchLimit(Configuration cfg, byte cmd) {
			int c = cmd & 0xFF;
			switch (c) {
			case SMB_COM_CHECK_DIRECTORY:
				return cfg.getBatchLimit("TreeConnectAndX.CheckDirectory");
			case SMB_COM_CREATE_DIRECTORY:
				return cfg.getBatchLimit("TreeConnectAndX.CreateDirectory");
			case SMB_COM_DELETE:
				return cfg.getBatchLimit("TreeConnectAndX.Delete");
			case SMB_COM_DELETE_DIRECTORY:
				return cfg.getBatchLimit("TreeConnectAndX.DeleteDirectory");
			case SMB_COM_OPEN_ANDX:
				return cfg.getBatchLimit("TreeConnectAndX.OpenAndX");
			case SMB_COM_RENAME:
				return cfg.getBatchLimit("TreeConnectAndX.Rename");
			case SMB_COM_TRANSACTION:
				return cfg.getBatchLimit("TreeConnectAndX.Transaction");
			case SMB_COM_QUERY_INFORMATION:
				return cfg.getBatchLimit("TreeConnectAndX.QueryInformation");
			}
			return 0;
		}


		protected internal override int writeParameterWordsWireFormat(byte[] dst, int dstIndex) {
			if (this.server.security == SmbConstants.SECURITY_SHARE && this.ctx.getCredentials() is NtlmPasswordAuthenticator) {
				NtlmPasswordAuthenticator pwAuth = (NtlmPasswordAuthenticator) this.ctx.getCredentials();
				if (isExternalAuth(pwAuth)) {
					this.passwordLength = 1;
				}
				else if (this.server.encryptedPasswords) {
					// encrypted
					try {
						this.password = pwAuth.getAnsiHash(this.ctx, this.server.encryptionKey);
					}
					//TODO 
					catch (Exception e) {
						throw new RuntimeCIFSException("Failed to encrypt password", e);
					}
					this.passwordLength = this.password.Length;
				}
				else if (this.ctx.getConfig().isDisablePlainTextPasswords()) {
					throw new RuntimeCIFSException("Plain text passwords are disabled");
				}
				else {
					// plain text
					this.password = new byte[(pwAuth.getPassword().Length + 1) * 2];
					this.passwordLength = writeString(pwAuth.getPassword(), this.password, 0);
				}
			}
			else {
				// no password in tree connect
				this.passwordLength = 1;
			}

			dst[dstIndex++] = this.disconnectTid ? (byte) 0x01 : (byte) 0x00;
			dst[dstIndex++] = (byte) 0x00;
			SMBUtil.writeInt2(this.passwordLength, dst, dstIndex);
			return 4;
		}


		private static bool isExternalAuth(NtlmPasswordAuthenticator pwAuth) {
			return pwAuth is NtlmPasswordAuthentication && !((NtlmPasswordAuthentication) pwAuth).areHashesExternal() && pwAuth.getPassword().Length == 0;
		}


		protected internal override int writeBytesWireFormat(byte[] dst, int dstIndex) {
			int start = dstIndex;
			if (this.server.security == SmbConstants.SECURITY_SHARE && this.ctx.getCredentials() is NtlmPasswordAuthenticator) {
				NtlmPasswordAuthenticator pwAuth = (NtlmPasswordAuthenticator) this.ctx.getCredentials();
				if (isExternalAuth(pwAuth)) {
					dst[dstIndex++] = (byte) 0x00;
				}
				else {
					Array.Copy(this.password, 0, dst, dstIndex, this.passwordLength);
					dstIndex += this.passwordLength;
				}
			}
			else {
				// no password in tree connect
				dst[dstIndex++] = (byte) 0x00;
			}
			dstIndex += writeString(this.path, dst, dstIndex);

			byte[] source;
			try
			{
				 source = service.getBytes(Encoding.ASCII);
			}
			catch (Exception) {
				return 0;
			}
		
			Array.Copy(source, 0, dst, dstIndex, this.service.Length);
			
			dstIndex += this.service.Length;
			dst[dstIndex++] = (byte) '\0';

			return dstIndex - start;
		}


		protected internal override int readParameterWordsWireFormat(byte[] buffer, int bufferIndex) {
			return 0;
		}


		protected internal override int readBytesWireFormat(byte[] buffer, int bufferIndex) {
			return 0;
		}


		public override string ToString() {
			string result = "SmbComTreeConnectAndX[" + base.ToString() + ",disconnectTid=" + this.disconnectTid + ",passwordLength=" + this.passwordLength + ",password=" + Hexdump.toHexString(this.password, this.passwordLength, 0) + ",path=" + this.path + ",service=" + this.service + "]";
			return result;
		}
	}

}