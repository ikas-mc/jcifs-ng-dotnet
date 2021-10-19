using System;
using System.Collections.Generic;
using CIFSContext = jcifs.CIFSContext;
using Configuration = jcifs.Configuration;
using DialectVersion = jcifs.DialectVersion;
using SmbNegotiationRequest = jcifs.@internal.SmbNegotiationRequest;
using jcifs.@internal.smb2;
using Smb2Constants = jcifs.@internal.smb2.Smb2Constants;
using SMBUtil = jcifs.@internal.util.SMBUtil;

/*
 * © 2017 AgNO3 Gmbh & Co. KG
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
namespace jcifs.@internal.smb2.nego {




	/// <summary>
	/// @author mbechler
	/// 
	/// </summary>
	public class Smb2NegotiateRequest : ServerMessageBlock2Request<Smb2NegotiateResponse>, SmbNegotiationRequest {

		private int[] dialects;
		private int capabilities;
		private byte[] clientGuid = new byte[16];
		private int securityMode;
		private NegotiateContextRequest[] negotiateContexts;
		private byte[] preauthSalt;


		/// <param name="config"> </param>
		/// <param name="securityMode"> </param>
		public Smb2NegotiateRequest(Configuration config, int securityMode) : base(config, SMB2_NEGOTIATE) {
			this.securityMode = securityMode;

			if (!config.isDfsDisabled()) {
				this.capabilities |= Smb2Constants.SMB2_GLOBAL_CAP_DFS;
			}

			if (config.isEncryptionEnabled() && config.getMaximumVersion() != null && config.getMaximumVersion().atLeast(DialectVersion.SMB300)) {
				this.capabilities |= Smb2Constants.SMB2_GLOBAL_CAP_ENCRYPTION;
			}

			ISet<DialectVersion> dvs = DialectVersion.range(DialectVersion.max(DialectVersion.SMB202, config.getMinimumVersion()), config.getMaximumVersion());

			this.dialects = new int[dvs.Count];
			int i = 0;
			foreach (DialectVersion ver in dvs) {
				this.dialects[i] = ver.getDialect();
				i++;
			}

			if (config.getMaximumVersion().atLeast(DialectVersion.SMB210)) {
				Array.Copy(config.getMachineId(), 0, this.clientGuid, 0, this.clientGuid.Length);
			}

			IList<NegotiateContextRequest> negoContexts = new List<NegotiateContextRequest>();
			if (config.getMaximumVersion() != null && config.getMaximumVersion().atLeast(DialectVersion.SMB311)) {
				byte[] salt = new byte[32];
				config.getRandom().NextBytes(salt);
				negoContexts.Add(new PreauthIntegrityNegotiateContext(config, new int[] {PreauthIntegrityNegotiateContext.HASH_ALGO_SHA512}, salt));
				this.preauthSalt = salt;

				if (config.isEncryptionEnabled()) {
					negoContexts.Add(new EncryptionNegotiateContext(config, new int[] {EncryptionNegotiateContext.CIPHER_AES128_GCM, EncryptionNegotiateContext.CIPHER_AES128_CCM}));
				}
			}

			this.negotiateContexts = ((List<NegotiateContextRequest>)negoContexts).ToArray();
		}


		/// <returns> the securityMode </returns>
		public virtual int getSecurityMode() {
			return this.securityMode;
		}


		public virtual bool isSigningEnforced() {
			return (getSecurityMode() & Smb2Constants.SMB2_NEGOTIATE_SIGNING_REQUIRED) != 0;
		}


		/// <returns> the capabilities </returns>
		public virtual int getCapabilities() {
			return this.capabilities;
		}


		/// <returns> the dialects </returns>
		public virtual int[] getDialects() {
			return this.dialects;
		}


		/// <returns> the clientGuid </returns>
		public virtual byte[] getClientGuid() {
			return this.clientGuid;
		}


		/// <returns> the negotiateContexts </returns>
		public virtual NegotiateContextRequest[] getNegotiateContexts() {
			return this.negotiateContexts;
		}


		/// <returns> the preauthSalt </returns>
		public virtual byte[] getPreauthSalt() {
			return this.preauthSalt;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb2.ServerMessageBlock2Request#createResponse(jcifs.Configuration,
		///      jcifs.internal.smb2.ServerMessageBlock2Request) </seealso>
		protected  override Smb2NegotiateResponse createResponse(CIFSContext tc, ServerMessageBlock2Request<Smb2NegotiateResponse> req) {
			return new Smb2NegotiateResponse(tc.getConfig());
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.CommonServerMessageBlockRequest#size() </seealso>
		public override int size() {
			int size = Smb2Constants.SMB2_HEADER_LENGTH + 36 + size8(2 * this.dialects.Length, 4);
			if (this.negotiateContexts != null) {
				foreach (NegotiateContextRequest ncr in this.negotiateContexts) {
					size += 8 + size8(ncr.size());
				}
			}
			return size8(size);
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb2.ServerMessageBlock2#writeBytesWireFormat(byte[], int) </seealso>
		protected  override int writeBytesWireFormat(byte[] dst, int dstIndex) {
			int start = dstIndex;
			SMBUtil.writeInt2(36, dst, dstIndex);
			SMBUtil.writeInt2(this.dialects.Length, dst, dstIndex + 2);
			dstIndex += 4;

			SMBUtil.writeInt2(this.securityMode, dst, dstIndex);
			SMBUtil.writeInt2(0, dst, dstIndex + 2); // Reserved
			dstIndex += 4;

			SMBUtil.writeInt4(this.capabilities, dst, dstIndex);
			dstIndex += 4;

			Array.Copy(this.clientGuid, 0, dst, dstIndex, 16);
			dstIndex += 16;

			// if SMB 3.11 support negotiateContextOffset/negotiateContextCount
			int negotitateContextOffsetOffset = 0;
			if (this.negotiateContexts == null || this.negotiateContexts.Length == 0) {
				SMBUtil.writeInt8(0, dst, dstIndex);
			}
			else {
				negotitateContextOffsetOffset = dstIndex;
				SMBUtil.writeInt2(this.negotiateContexts.Length, dst, dstIndex + 4);
				SMBUtil.writeInt2(0, dst, dstIndex + 6);
			}
			dstIndex += 8;

			foreach (int dialect in this.dialects) {
				SMBUtil.writeInt2(dialect, dst, dstIndex);
				dstIndex += 2;
			}

			dstIndex += pad8(dstIndex);

			if (this.negotiateContexts != null && this.negotiateContexts.Length != 0) {
				SMBUtil.writeInt4(dstIndex - getHeaderStart(), dst, negotitateContextOffsetOffset);
				foreach (NegotiateContextRequest nc in this.negotiateContexts) {
					SMBUtil.writeInt2(nc.getContextType(), dst, dstIndex);
					int lenOffset = dstIndex + 2;
					dstIndex += 4;
					SMBUtil.writeInt4(0, dst, dstIndex);
					dstIndex += 4; // Reserved
					int dataLen = size8(nc.encode(dst, dstIndex));
					SMBUtil.writeInt2(dataLen, dst, lenOffset);
					dstIndex += dataLen;
				}
			}
			return dstIndex - start;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb2.ServerMessageBlock2#readBytesWireFormat(byte[], int) </seealso>
		protected  override int readBytesWireFormat(byte[] buffer, int bufferIndex) {
			return 0;
		}

	}

}