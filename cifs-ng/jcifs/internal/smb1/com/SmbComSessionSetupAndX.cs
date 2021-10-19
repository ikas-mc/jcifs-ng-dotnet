using System;
using cifs_ng.lib.ext;
using CIFSContext = jcifs.CIFSContext;
using Configuration = jcifs.Configuration;
using SmbConstants = jcifs.SmbConstants;
using AndXServerMessageBlock = jcifs.@internal.smb1.AndXServerMessageBlock;
using ServerMessageBlock = jcifs.@internal.smb1.ServerMessageBlock;
using SMBUtil = jcifs.@internal.util.SMBUtil;
using NtlmPasswordAuthenticator = jcifs.smb.NtlmPasswordAuthenticator;
using SmbException = jcifs.smb.SmbException;

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
	public class SmbComSessionSetupAndX : AndXServerMessageBlock {

		private byte[] lmHash, ntHash, blob = null;
		private string accountName, primaryDomain;
		private SmbComNegotiateResponse negotiated;
		private int capabilities;


		/// 
		/// <param name="tc"> </param>
		/// <param name="negotiated"> </param>
		/// <param name="andx"> </param>
		/// <param name="cred"> </param>
		/// <exception cref="SmbException"> </exception>
		/// <exception cref="GeneralSecurityException"> </exception>
		/// throws SmbException, java.security.GeneralSecurityException
		public SmbComSessionSetupAndX(CIFSContext tc, SmbComNegotiateResponse negotiated, ServerMessageBlock andx, object cred) : base(tc.getConfig(), SMB_COM_SESSION_SETUP_ANDX, andx) {
			this.negotiated = negotiated;
			this.capabilities = negotiated.getNegotiatedCapabilities();
			ServerData server = negotiated.getServerData();
			if (server.security == SmbConstants.SECURITY_USER) {
				if (cred is NtlmPasswordAuthenticator) {
					NtlmPasswordAuthenticator a = (NtlmPasswordAuthenticator) cred;
					if (a.isAnonymous()) {
						this.lmHash = new byte[0];
						this.ntHash = new byte[0];
						this.capabilities &= ~SmbConstants.CAP_EXTENDED_SECURITY;
						if (a.isGuest()) {
							this.accountName = a.getUsername();
							if (this.isUseUnicode()) {
								this.accountName = this.accountName.ToUpper();
							}
							this.primaryDomain = a.getUserDomain()!=null ? a.getUserDomain().ToUpper() : "?";
						}
						else {
							this.accountName = "";
							this.primaryDomain = "";
						}
					}
					else {
						this.accountName = a.getUsername();
						if (this.isUseUnicode()) {
							this.accountName = this.accountName.ToUpper();
						}
						this.primaryDomain = a.getUserDomain()!=null ? a.getUserDomain().ToUpper() : "?";
						if (server.encryptedPasswords) {
							this.lmHash = a.getAnsiHash(tc, server.encryptionKey);
							this.ntHash = a.getUnicodeHash(tc, server.encryptionKey);
							// prohibit HTTP auth attempts for the null session
							if (this.lmHash.Length == 0 && this.ntHash.Length == 0) {
								throw new Exception("Null setup prohibited.");
							}
						}
						else if (tc.getConfig().isDisablePlainTextPasswords()) {
							throw new Exception("Plain text passwords are disabled");
						}
						else {
							// plain text
							string password = a.getPassword();
							this.lmHash = new byte[(password.Length + 1) * 2];
							this.ntHash = new byte[0];
							writeString(password, this.lmHash, 0);
						}
					}

				}
				else if (cred is byte[]) {
					this.blob = (byte[]) cred;
				}
				else {
					throw new SmbException("Unsupported credential type " + cred?.GetType());
				}
			}
			else if (server.security == SmbConstants.SECURITY_SHARE) {
				if (cred is NtlmPasswordAuthenticator) {
					NtlmPasswordAuthenticator a = (NtlmPasswordAuthenticator) cred;
					this.lmHash = new byte[0];
					this.ntHash = new byte[0];
					if (!a.isAnonymous()) {
						this.accountName = a.getUsername();
						if (this.isUseUnicode()) {
							this.accountName = this.accountName.ToUpper();
						}
						this.primaryDomain = a.getUserDomain()!=null ? a.getUserDomain().ToUpper() : "?";
					}
					else {
						this.accountName = "";
						this.primaryDomain = "";
					}
				}
				else {
					throw new SmbException("Unsupported credential type");
				}
			}
			else {
				throw new SmbException("Unsupported");
			}
		}


		protected internal override int getBatchLimit(Configuration cfg, byte cmd) {
			return cmd == SMB_COM_TREE_CONNECT_ANDX ? cfg.getBatchLimit("SessionSetupAndX.TreeConnectAndX") : 0;
		}


		protected internal override int writeParameterWordsWireFormat(byte[] dst, int dstIndex) {
			int start = dstIndex;

			SMBUtil.writeInt2(this.negotiated.getNegotiatedSendBufferSize(), dst, dstIndex);
			dstIndex += 2;
			SMBUtil.writeInt2(this.negotiated.getNegotiatedMpxCount(), dst, dstIndex);
			dstIndex += 2;
			SMBUtil.writeInt2(getConfig().getVcNumber(), dst, dstIndex);
			dstIndex += 2;
			SMBUtil.writeInt4(this.negotiated.getNegotiatedSessionKey(), dst, dstIndex);
			dstIndex += 4;
			if (this.blob != null) {
				SMBUtil.writeInt2(this.blob.Length, dst, dstIndex);
				dstIndex += 2;
			}
			else {
				SMBUtil.writeInt2(this.lmHash.Length, dst, dstIndex);
				dstIndex += 2;
				SMBUtil.writeInt2(this.ntHash.Length, dst, dstIndex);
				dstIndex += 2;
			}
			dst[dstIndex++] = (byte) 0x00;
			dst[dstIndex++] = (byte) 0x00;
			dst[dstIndex++] = (byte) 0x00;
			dst[dstIndex++] = (byte) 0x00;
			SMBUtil.writeInt4(this.capabilities, dst, dstIndex);
			dstIndex += 4;

			return dstIndex - start;
		}


		protected internal override int writeBytesWireFormat(byte[] dst, int dstIndex) {
			int start = dstIndex;

			if (this.blob != null) {
				Array.Copy(this.blob, 0, dst, dstIndex, this.blob.Length);
				dstIndex += this.blob.Length;
			}
			else {
				Array.Copy(this.lmHash, 0, dst, dstIndex, this.lmHash.Length);
				dstIndex += this.lmHash.Length;
				Array.Copy(this.ntHash, 0, dst, dstIndex, this.ntHash.Length);
				dstIndex += this.ntHash.Length;

				dstIndex += writeString(this.accountName, dst, dstIndex);
				dstIndex += writeString(this.primaryDomain, dst, dstIndex);
			}
			dstIndex += writeString(getConfig().getNativeOs(), dst, dstIndex);
			dstIndex += writeString(getConfig().getNativeLanman(), dst, dstIndex);

			return dstIndex - start;
		}


		protected internal override int readParameterWordsWireFormat(byte[] buffer, int bufferIndex) {
			return 0;
		}


		protected internal override int readBytesWireFormat(byte[] buffer, int bufferIndex) {
			return 0;
		}


		public override string ToString() {
			string result = "SmbComSessionSetupAndX[" + base.ToString() + ",snd_buf_size=" + this.negotiated.getNegotiatedSendBufferSize() + ",maxMpxCount=" + this.negotiated.getNegotiatedMpxCount() + ",VC_NUMBER=" + getConfig().getVcNumber() + ",sessionKey=" + this.negotiated.getNegotiatedSessionKey() + ",lmHash.length=" + (this.lmHash == null ? 0 : this.lmHash.Length) + ",ntHash.length=" + (this.ntHash == null ? 0 : this.ntHash.Length) + ",capabilities=" + this.capabilities + ",accountName=" + this.accountName + ",primaryDomain=" + this.primaryDomain + ",NATIVE_OS=" + getConfig().getNativeOs() + ",NATIVE_LANMAN=" + getConfig().getNativeLanman() + "]";
			return result;
		}
	}

}