using System;
using Logger = org.slf4j.Logger;
using LoggerFactory = org.slf4j.LoggerFactory;
using CIFSContext = jcifs.CIFSContext;
using Configuration = jcifs.Configuration;
using DialectVersion = jcifs.DialectVersion;
using CommonServerMessageBlock = jcifs.@internal.CommonServerMessageBlock;
using SMBProtocolDecodingException = jcifs.@internal.SMBProtocolDecodingException;
using SmbNegotiationRequest = jcifs.@internal.SmbNegotiationRequest;
using SmbNegotiationResponse = jcifs.@internal.SmbNegotiationResponse;
using ServerMessageBlock2Response = jcifs.@internal.smb2.ServerMessageBlock2Response;
using Smb2Constants = jcifs.@internal.smb2.Smb2Constants;
using Smb2ReadResponse = jcifs.@internal.smb2.io.Smb2ReadResponse;
using Smb2WriteRequest = jcifs.@internal.smb2.io.Smb2WriteRequest;
using SMBUtil = jcifs.@internal.util.SMBUtil;
using Hexdump = jcifs.util.Hexdump;
using Response = jcifs.util.transport.Response;

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
	public class Smb2NegotiateResponse : ServerMessageBlock2Response, SmbNegotiationResponse {

		private static readonly Logger log = LoggerFactory.getLogger(typeof(Smb2NegotiateResponse));

		private int securityMode;
		private int dialectRevision;
		private byte[] serverGuid = new byte[16];
		private int capabilities;
		private int commonCapabilities;
		private int maxTransactSize;
		private int maxReadSize;
		private int maxWriteSize;
		private long systemTime;
		private long serverStartTime;
		private NegotiateContextResponse[] negotiateContexts;
		private byte[] securityBuffer;
		private DialectVersion selectedDialect;

		private bool supportsEncryption;
		private int selectedCipher = -1;
		private int selectedPreauthHash = -1;


		/// 
		/// <param name="cfg"> </param>
		public Smb2NegotiateResponse(Configuration cfg) : base(cfg) {
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.SmbNegotiationResponse#getInitialCredits() </seealso>
		public virtual int getInitialCredits() {
			return getCredit();
		}


		/// <returns> the dialectRevision </returns>
		public virtual int getDialectRevision() {
			return this.dialectRevision;
		}


		/// <returns> the serverGuid </returns>
		public virtual byte[] getServerGuid() {
			return this.serverGuid;
		}


		/// <returns> the selectedDialect </returns>
		public virtual DialectVersion getSelectedDialect() {
			return this.selectedDialect;
		}


		/// <returns> the selectedCipher </returns>
		public virtual int getSelectedCipher() {
			return this.selectedCipher;
		}


		/// <returns> the selectedPreauthHash </returns>
		public virtual int getSelectedPreauthHash() {
			return this.selectedPreauthHash;
		}


		/// <returns> the server returned capabilities </returns>
		public int getCapabilities() {
			return this.capabilities;
		}


		/// <returns> the common/negotiated capabilieis </returns>
		public int getCommonCapabilities() {
			return this.commonCapabilities;
		}


		/// <returns> initial security blob </returns>
		public virtual byte[] getSecurityBlob() {
			return this.securityBuffer;
		}


		/// <returns> the maxTransactSize </returns>
		public virtual int getMaxTransactSize() {
			return this.maxTransactSize;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.SmbNegotiationResponse#getTransactionBufferSize() </seealso>
		public virtual int getTransactionBufferSize() {
			return getMaxTransactSize();
		}


		/// <returns> the negotiateContexts </returns>
		public virtual NegotiateContextResponse[] getNegotiateContexts() {
			return this.negotiateContexts;
		}


		/// <returns> the serverStartTime </returns>
		public virtual long getServerStartTime() {
			return this.serverStartTime;
		}


		/// <returns> the securityMode </returns>
		public virtual int getSecurityMode() {
			return this.securityMode;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.SmbNegotiationResponse#haveCapabilitiy(int) </seealso>
		public virtual bool haveCapabilitiy(int cap) {
			return (this.commonCapabilities & cap) == cap;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.SmbNegotiationResponse#isDFSSupported() </seealso>
		public virtual bool isDFSSupported() {
			return !getConfig().isDfsDisabled() && haveCapabilitiy(Smb2Constants.SMB2_GLOBAL_CAP_DFS);
		}


		/// 
		/// <returns> whether SMB encryption is supported by the server </returns>
		public virtual bool isEncryptionSupported() {
			return this.supportsEncryption;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.SmbNegotiationResponse#canReuse(jcifs.CIFSContext, boolean) </seealso>
		public virtual bool canReuse(CIFSContext tc, bool forceSigning) {
			return getConfig().Equals(tc.getConfig());
		}


		public virtual bool isValid(CIFSContext tc, SmbNegotiationRequest req) {
			if (!isReceived() || getStatus() != 0) {
				return false;
			}

			if (req.isSigningEnforced() && !isSigningEnabled()) {
				log.error("Signing is enforced but server does not allow it");
				return false;
			}

			if (getDialectRevision() == Smb2Constants.SMB2_DIALECT_ANY) {
				log.error("Server returned ANY dialect");
				return false;
			}

			Smb2NegotiateRequest r = (Smb2NegotiateRequest) req;

			DialectVersion selected = null;
			foreach (DialectVersion dv in DialectVersion.values()) {
				if (!dv.isSMB2()) {
					continue;
				}
				if (dv.getDialect() == getDialectRevision()) {
					selected = dv;
				}
			}

			if (selected == null) {
				log.error("Server returned an unknown dialect");
				return false;
			}

			if (!selected.atLeast(getConfig().getMinimumVersion()) || !selected.atMost(getConfig().getMaximumVersion())) {
				log.error(string.Format("Server selected an disallowed dialect version {0} (min: {1} max: {2})", selected, getConfig().getMinimumVersion(), getConfig().getMaximumVersion()));
				return false;
			}
			this.selectedDialect = selected;

			// Filter out unsupported capabilities
			this.commonCapabilities = r.getCapabilities() & this.capabilities;

			if ((this.commonCapabilities & Smb2Constants.SMB2_GLOBAL_CAP_ENCRYPTION) != 0) {
				this.supportsEncryption = tc.getConfig().isEncryptionEnabled();
			}

			if (this.selectedDialect.atLeast(DialectVersion.SMB311)) {
				if (!checkNegotiateContexts(r, this.commonCapabilities)) {
					return false;
				}
			}

			int maxBufferSize = tc.getConfig().getTransactionBufferSize();
			this.maxReadSize = Math.Min(maxBufferSize - Smb2ReadResponse.OVERHEAD, Math.Min(tc.getConfig().getReceiveBufferSize(), this.maxReadSize)) & ~0x7;
			this.maxWriteSize = Math.Min(maxBufferSize - Smb2WriteRequest.OVERHEAD, Math.Min(tc.getConfig().getSendBufferSize(), this.maxWriteSize)) & ~0x7;
			this.maxTransactSize = Math.Min(maxBufferSize - 512, this.maxTransactSize) & ~0x7;

			return true;
		}


		private bool checkNegotiateContexts(Smb2NegotiateRequest req, int caps) {
			if (this.negotiateContexts == null || this.negotiateContexts.Length == 0) {
				log.error("Response lacks negotiate contexts");
				return false;
			}

			bool foundPreauth = false, foundEnc = false;
			foreach (NegotiateContextResponse ncr in this.negotiateContexts) {
				if (ncr == null) {
					continue;
				}
				else if (!foundEnc && ncr.getContextType() == EncryptionNegotiateContext.NEGO_CTX_ENC_TYPE) {
					foundEnc = true;
					EncryptionNegotiateContext enc = (EncryptionNegotiateContext) ncr;
					if (!checkEncryptionContext(req, enc)) {
						return false;
					}
					this.selectedCipher = enc.getCiphers()[0];
					this.supportsEncryption = true;
				}
				else if (ncr.getContextType() == EncryptionNegotiateContext.NEGO_CTX_ENC_TYPE) {
					log.error("Multiple encryption negotiate contexts");
					return false;
				}
				else if (!foundPreauth && ncr.getContextType() == PreauthIntegrityNegotiateContext.NEGO_CTX_PREAUTH_TYPE) {
					foundPreauth = true;
					PreauthIntegrityNegotiateContext pi = (PreauthIntegrityNegotiateContext) ncr;
					if (!checkPreauthContext(req, pi)) {
						return false;
					}
					this.selectedPreauthHash = pi.getHashAlgos()[0];
				}
				else if (ncr.getContextType() == PreauthIntegrityNegotiateContext.NEGO_CTX_PREAUTH_TYPE) {
					log.error("Multiple preauth negotiate contexts");
					return false;
				}
			}

			if (!foundPreauth) {
				log.error("Missing preauth negotiate context");
				return false;
			}
			if (!foundEnc && (caps & Smb2Constants.SMB2_GLOBAL_CAP_ENCRYPTION) != 0) {
				log.error("Missing encryption negotiate context");
				return false;
			}
			else if (!foundEnc) {
				log.debug("No encryption support");
			}
			return true;
		}


		private static bool checkPreauthContext(Smb2NegotiateRequest req, PreauthIntegrityNegotiateContext pc) {
			if (pc.getHashAlgos() == null || pc.getHashAlgos().Length != 1) {
				log.error("Server returned no hash selection");
				return false;
			}

			PreauthIntegrityNegotiateContext rpc = null;
			foreach (NegotiateContextRequest rnc in req.getNegotiateContexts()) {
				if (rnc is PreauthIntegrityNegotiateContext) {
					rpc = (PreauthIntegrityNegotiateContext) rnc;
				}
			}
			if (rpc == null) {
				return false;
			}

			bool valid = false;
			foreach (int hash in rpc.getHashAlgos()) {
				if (hash == pc.getHashAlgos()[0]) {
					valid = true;
				}
			}
			if (!valid) {
				log.error("Server returned invalid hash selection");
				return false;
			}
			return true;
		}


		private static bool checkEncryptionContext(Smb2NegotiateRequest req, EncryptionNegotiateContext ec) {
			if (ec.getCiphers() == null || ec.getCiphers().Length != 1) {
				log.error("Server returned no cipher selection");
				return false;
			}

			EncryptionNegotiateContext rec = null;
			foreach (NegotiateContextRequest rnc in req.getNegotiateContexts()) {
				if (rnc is EncryptionNegotiateContext) {
					rec = (EncryptionNegotiateContext) rnc;
				}
			}
			if (rec == null) {
				return false;
			}

			bool valid = false;
			foreach (int cipher in rec.getCiphers()) {
				if (cipher == ec.getCiphers()[0]) {
					valid = true;
				}
			}
			if (!valid) {
				log.error("Server returned invalid cipher selection");
				return false;
			}
			return true;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.SmbNegotiationResponse#getReceiveBufferSize() </seealso>
		public virtual int getReceiveBufferSize() {
			return this.maxReadSize;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.SmbNegotiationResponse#getSendBufferSize() </seealso>
		public virtual int getSendBufferSize() {
			return this.maxWriteSize;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.SmbNegotiationResponse#isSigningEnabled() </seealso>
		public virtual bool isSigningEnabled() {
			return (this.securityMode & (Smb2Constants.SMB2_NEGOTIATE_SIGNING_ENABLED)) != 0;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.SmbNegotiationResponse#isSigningRequired() </seealso>
		public virtual bool isSigningRequired() {
			return (this.securityMode & Smb2Constants.SMB2_NEGOTIATE_SIGNING_REQUIRED) != 0;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.SmbNegotiationResponse#isSigningNegotiated() </seealso>
		public virtual bool isSigningNegotiated() {
			return isSigningRequired();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.SmbNegotiationResponse#setupRequest(jcifs.internal.CommonServerMessageBlock) </seealso>
		public virtual void setupRequest(CommonServerMessageBlock request) {
		}
		public virtual void setupResponse(Response resp) {
		}
		/// throws jcifs.internal.SMBProtocolDecodingException
		protected  override int readBytesWireFormat(byte[] buffer, int bufferIndex) {
			int start = bufferIndex;

			int structureSize = SMBUtil.readInt2(buffer, bufferIndex);
			if (structureSize != 65) {
				throw new SMBProtocolDecodingException("Structure size is not 65");
			}

			this.securityMode = SMBUtil.readInt2(buffer, bufferIndex + 2);
			bufferIndex += 4;

			this.dialectRevision = SMBUtil.readInt2(buffer, bufferIndex);
			int negotiateContextCount = SMBUtil.readInt2(buffer, bufferIndex + 2);
			bufferIndex += 4;

			Array.Copy(buffer, bufferIndex, this.serverGuid, 0, 16);
			bufferIndex += 16;

			this.capabilities = SMBUtil.readInt4(buffer, bufferIndex);
			bufferIndex += 4;

			this.maxTransactSize = SMBUtil.readInt4(buffer, bufferIndex);
			bufferIndex += 4;
			this.maxReadSize = SMBUtil.readInt4(buffer, bufferIndex);
			bufferIndex += 4;
			this.maxWriteSize = SMBUtil.readInt4(buffer, bufferIndex);
			bufferIndex += 4;

			this.systemTime = SMBUtil.readTime(buffer, bufferIndex);
			bufferIndex += 8;
			this.serverStartTime = SMBUtil.readTime(buffer, bufferIndex);
			bufferIndex += 8;

			int securityBufferOffset = SMBUtil.readInt2(buffer, bufferIndex);
			int securityBufferLength = SMBUtil.readInt2(buffer, bufferIndex + 2);
			bufferIndex += 4;

			int negotiateContextOffset = SMBUtil.readInt4(buffer, bufferIndex);
			bufferIndex += 4;

			int hdrStart = getHeaderStart();
			if (hdrStart + securityBufferOffset + securityBufferLength < buffer.Length) {
				this.securityBuffer = new byte[securityBufferLength];
				Array.Copy(buffer, hdrStart + securityBufferOffset, this.securityBuffer, 0, securityBufferLength);
				bufferIndex += securityBufferLength;
			}

			int pad = (bufferIndex - hdrStart) % 8;
			bufferIndex += pad;

			if (this.dialectRevision == 0x0311 && negotiateContextOffset != 0 && negotiateContextCount != 0) {
				int ncpos = getHeaderStart() + negotiateContextOffset;
				NegotiateContextResponse[] contexts = new NegotiateContextResponse[negotiateContextCount];
				for (int i = 0; i < negotiateContextCount; i++) {
					int type = SMBUtil.readInt2(buffer, ncpos);
					int dataLen = SMBUtil.readInt2(buffer, ncpos + 2);
					ncpos += 4;
					ncpos += 4; // Reserved
					NegotiateContextResponse ctx = createContext(type);
					if (ctx != null) {
						ctx.decode(buffer, ncpos, dataLen);
						contexts[i] = ctx;
					}
					ncpos += dataLen;
					if (i != negotiateContextCount - 1) {
						ncpos += pad8(ncpos);
					}
				}
				this.negotiateContexts = contexts;
				return Math.Max(bufferIndex, ncpos) - start;
			}

			return bufferIndex - start;
		}


		/// <param name="type">
		/// @return </param>
		protected  static NegotiateContextResponse createContext(int type) {
			switch (type) {
			case EncryptionNegotiateContext.NEGO_CTX_ENC_TYPE:
				return new EncryptionNegotiateContext();
			case PreauthIntegrityNegotiateContext.NEGO_CTX_PREAUTH_TYPE:
				return new PreauthIntegrityNegotiateContext();
			}
			return null;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb2.ServerMessageBlock2#writeBytesWireFormat(byte[], int) </seealso>
		protected  override int writeBytesWireFormat(byte[] dst, int dstIndex) {
			return 0;
		}


		public override string ToString() {
			return "Smb2NegotiateResponse[" + base.ToString() + ",dialectRevision=" + this.dialectRevision + ",securityMode=0x" + Hexdump.toHexString(this.securityMode, 1) + ",capabilities=0x" + Hexdump.toHexString(this.capabilities, 8) + ",serverTime=" + DateTimeOffset.FromUnixTimeMilliseconds(this.systemTime);
		}

	}

}