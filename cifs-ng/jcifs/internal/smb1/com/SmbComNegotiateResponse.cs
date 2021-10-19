using System;
using Logger = org.slf4j.Logger;
using LoggerFactory = org.slf4j.LoggerFactory;
using CIFSContext = jcifs.CIFSContext;
using DialectVersion = jcifs.DialectVersion;
using SmbConstants = jcifs.SmbConstants;
using CommonServerMessageBlock = jcifs.@internal.CommonServerMessageBlock;
using SmbNegotiationRequest = jcifs.@internal.SmbNegotiationRequest;
using SmbNegotiationResponse = jcifs.@internal.SmbNegotiationResponse;
using ServerMessageBlock = jcifs.@internal.smb1.ServerMessageBlock;
using SmbComTransaction = jcifs.@internal.smb1.trans.SmbComTransaction;
using SMBUtil = jcifs.@internal.util.SMBUtil;
using Hexdump = jcifs.util.Hexdump;
using Strings = jcifs.util.Strings;
using Response = jcifs.util.transport.Response;

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
	public class SmbComNegotiateResponse : ServerMessageBlock, SmbNegotiationResponse {

		private static readonly Logger log = LoggerFactory.getLogger(typeof(SmbComNegotiateResponse));

		private int dialectIndex;

		/* Negotiated values */
		private ServerData server;
		private int negotiatedFlags2;
		private int maxMpxCount;
		private int snd_buf_size;
		private int recv_buf_size;
		private int tx_buf_size;

		private int capabilities;
		private int sessionKey = 0x00000000;
		private bool useUnicode;


		/// 
		/// <param name="ctx"> </param>
		public SmbComNegotiateResponse(CIFSContext ctx) : base(ctx.getConfig()) {
			this.server = new ServerData();
			this.capabilities = ctx.getConfig().getCapabilities();
			this.negotiatedFlags2 = ctx.getConfig().getFlags2();
			this.maxMpxCount = ctx.getConfig().getMaxMpxCount();
			this.snd_buf_size = ctx.getConfig().getSendBufferSize();
			this.recv_buf_size = ctx.getConfig().getReceiveBufferSize();
			this.tx_buf_size = ctx.getConfig().getTransactionBufferSize();
			this.useUnicode = ctx.getConfig().isUseUnicode();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.SmbNegotiationResponse#getSelectedDialect() </seealso>
		public virtual DialectVersion getSelectedDialect() {
			return DialectVersion.SMB1;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.SmbNegotiationResponse#getTransactionBufferSize() </seealso>
		public virtual int getTransactionBufferSize() {
			return this.tx_buf_size;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.SmbNegotiationResponse#getInitialCredits() </seealso>
		public virtual int getInitialCredits() {
			return getNegotiatedMpxCount();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.SmbNegotiationResponse#canReuse(jcifs.CIFSContext, boolean) </seealso>
		public virtual bool canReuse(CIFSContext tc, bool forceSigning) {
			return this.getConfig().Equals(tc.getConfig());
		}


		/// <returns> the dialectIndex </returns>
		public virtual int getDialectIndex() {
			return this.dialectIndex;
		}


		/// <returns> the negotiated capbilities </returns>
		public virtual int getNegotiatedCapabilities() {
			return this.capabilities;
		}


		/// 
		/// <returns> negotiated send buffer size </returns>
		public virtual int getNegotiatedSendBufferSize() {
			return this.snd_buf_size;
		}


		/// 
		/// <returns> negotiated multiplex count </returns>
		public virtual int getNegotiatedMpxCount() {
			return this.maxMpxCount;
		}


		/// 
		/// <returns> negotiated session key </returns>
		public virtual int getNegotiatedSessionKey() {
			return this.sessionKey;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.SmbNegotiationResponse#getReceiveBufferSize() </seealso>
		public virtual int getReceiveBufferSize() {
			return this.recv_buf_size;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.SmbNegotiationResponse#getSendBufferSize() </seealso>
		public virtual int getSendBufferSize() {
			return this.snd_buf_size;
		}


		/// <returns> negotiated flags2 </returns>
		public virtual int getNegotiatedFlags2() {
			return this.negotiatedFlags2;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.SmbNegotiationResponse#haveCapabilitiy(int) </seealso>
		public virtual bool haveCapabilitiy(int cap) {
			return (this.capabilities & cap) == cap;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.SmbNegotiationResponse#isDFSSupported() </seealso>
		public virtual bool isDFSSupported() {
			return !getConfig().isDfsDisabled() && haveCapabilitiy(SmbConstants.CAP_DFS);
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.SmbNegotiationResponse#isSigningNegotiated() </seealso>
		public virtual bool isSigningNegotiated() {
			return (this.negotiatedFlags2 & SmbConstants.FLAGS2_SECURITY_SIGNATURES) == SmbConstants.FLAGS2_SECURITY_SIGNATURES;
		}


		public virtual bool isValid(CIFSContext ctx, SmbNegotiationRequest req) {
			if (getDialectIndex() > 10) {
				return false;
			}

			if ((this.server.scapabilities & SmbConstants.CAP_EXTENDED_SECURITY) != SmbConstants.CAP_EXTENDED_SECURITY && this.server.encryptionKeyLength != 8 && ctx.getConfig().getLanManCompatibility() == 0) {
				log.warn("Unexpected encryption key length: " + this.server.encryptionKeyLength);
				return false;
			}

			if (req.isSigningEnforced() || this.server.signaturesRequired || (this.server.signaturesEnabled && ctx.getConfig().isSigningEnabled())) {
				this.negotiatedFlags2 |= SmbConstants.FLAGS2_SECURITY_SIGNATURES;
				if (req.isSigningEnforced() || isSigningRequired()) {
					this.negotiatedFlags2 |= SmbConstants.FLAGS2_SECURITY_REQUIRE_SIGNATURES;
				}
			}
			else {
				this.negotiatedFlags2 &= 0xFFFF ^ SmbConstants.FLAGS2_SECURITY_SIGNATURES;
				this.negotiatedFlags2 &= 0xFFFF ^ SmbConstants.FLAGS2_SECURITY_REQUIRE_SIGNATURES;
			}

			if (log.isDebugEnabled()) {
				log.debug("Signing " + ((this.negotiatedFlags2 & SmbConstants.FLAGS2_SECURITY_SIGNATURES) != 0 ? "enabled " : "not-enabled ") + ((this.negotiatedFlags2 & SmbConstants.FLAGS2_SECURITY_REQUIRE_SIGNATURES) != 0 ? "required" : "not-required"));
			}

			this.maxMpxCount = Math.Min(this.maxMpxCount, this.server.smaxMpxCount);
			if (this.maxMpxCount < 1) {
				this.maxMpxCount = 1;
			}
			this.snd_buf_size = Math.Min(this.snd_buf_size, this.server.maxBufferSize);
			this.recv_buf_size = Math.Min(this.recv_buf_size, this.server.maxBufferSize);
			this.tx_buf_size = Math.Min(this.tx_buf_size, this.server.maxBufferSize);

			this.capabilities &= this.server.scapabilities;
			if ((this.server.scapabilities & SmbConstants.CAP_EXTENDED_SECURITY) == SmbConstants.CAP_EXTENDED_SECURITY) {
				this.capabilities |= SmbConstants.CAP_EXTENDED_SECURITY; // & doesn't copy high bit
			}

			if (ctx.getConfig().isUseUnicode() || ctx.getConfig().isForceUnicode()) {
				this.capabilities |= SmbConstants.CAP_UNICODE;
			}

			if ((this.capabilities & SmbConstants.CAP_UNICODE) == 0) {
				// server doesn't want unicode
				if (ctx.getConfig().isForceUnicode()) {
					this.capabilities |= SmbConstants.CAP_UNICODE;
					this.useUnicode = true;
				}
				else {
					this.useUnicode = false;
					this.negotiatedFlags2 &= 0xFFFF ^ SmbConstants.FLAGS2_UNICODE;
				}
			}
			else {
				this.useUnicode = ctx.getConfig().isUseUnicode();
			}

			if (this.useUnicode) {
				log.debug("Unicode is enabled");
			}
			else {
				log.debug("Unicode is disabled");
			}
			return true;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.SmbNegotiationResponse#setupRequest(jcifs.internal.CommonServerMessageBlock) </seealso>
		public virtual void setupRequest(CommonServerMessageBlock request) {

			if (!(request is ServerMessageBlock)) {
				return;
			}

			ServerMessageBlock req = (ServerMessageBlock) request;

			req.addFlags2(this.negotiatedFlags2);
			req.setUseUnicode(req.isForceUnicode() || this.useUnicode);
			if (req.isUseUnicode()) {
				req.addFlags2(SmbConstants.FLAGS2_UNICODE);
			}

			if (req is SmbComTransaction) {
				((SmbComTransaction) req).setMaxBufferSize(this.snd_buf_size);
			}
		}


		/// 
		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.SmbNegotiationResponse#setupResponse(jcifs.util.transport.Response) </seealso>
		public virtual void setupResponse(Response resp) {
			if (!(resp is ServerMessageBlock)) {
				return;
			}
			((ServerMessageBlock) resp).setUseUnicode(this.useUnicode);
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.SmbNegotiationResponse#isSigningEnabled() </seealso>
		public virtual bool isSigningEnabled() {
			return this.server.signaturesEnabled || this.server.signaturesRequired;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.SmbNegotiationResponse#isSigningRequired() </seealso>
		public virtual bool isSigningRequired() {
			return this.server.signaturesRequired;
		}


		/// <returns> the server </returns>
		public virtual ServerData getServerData() {
			return this.server;
		}


		protected internal override int writeParameterWordsWireFormat(byte[] dst, int dstIndex) {
			return 0;
		}


		protected internal override int writeBytesWireFormat(byte[] dst, int dstIndex) {
			return 0;
		}


		protected internal override int readParameterWordsWireFormat(byte[] buffer, int bufferIndex) {
			int start = bufferIndex;

			this.dialectIndex = SMBUtil.readInt2(buffer, bufferIndex);
			bufferIndex += 2;
			if (this.dialectIndex > 10) {
				return bufferIndex - start;
			}
			this.server.securityMode = buffer[bufferIndex++] & 0xFF;
			this.server.security = this.server.securityMode & 0x01;
			this.server.encryptedPasswords = (this.server.securityMode & 0x02) == 0x02;
			this.server.signaturesEnabled = (this.server.securityMode & 0x04) == 0x04;
			this.server.signaturesRequired = (this.server.securityMode & 0x08) == 0x08;
			this.server.smaxMpxCount = SMBUtil.readInt2(buffer, bufferIndex);
			bufferIndex += 2;
			this.server.maxNumberVcs = SMBUtil.readInt2(buffer, bufferIndex);
			bufferIndex += 2;
			this.server.maxBufferSize = SMBUtil.readInt4(buffer, bufferIndex);
			bufferIndex += 4;
			this.server.maxRawSize = SMBUtil.readInt4(buffer, bufferIndex);
			bufferIndex += 4;
			this.server.sessKey = SMBUtil.readInt4(buffer, bufferIndex);
			bufferIndex += 4;
			this.server.scapabilities = SMBUtil.readInt4(buffer, bufferIndex);
			bufferIndex += 4;
			this.server.serverTime = SMBUtil.readTime(buffer, bufferIndex);
			bufferIndex += 8;
			int tzOffset = SMBUtil.readInt2(buffer, bufferIndex);
			// tzOffset is signed!
			if (tzOffset > short.MaxValue) {
				tzOffset = -1 * (65536 - tzOffset);
			}
			this.server.serverTimeZone = tzOffset;
			bufferIndex += 2;
			this.server.encryptionKeyLength = buffer[bufferIndex++] & 0xFF;

			return bufferIndex - start;
		}


		protected internal override int readBytesWireFormat(byte[] buffer, int bufferIndex) {
			int start = bufferIndex;

			if ((this.server.scapabilities & SmbConstants.CAP_EXTENDED_SECURITY) == 0) {
				this.server.encryptionKey = new byte[this.server.encryptionKeyLength];
				Array.Copy(buffer, bufferIndex, this.server.encryptionKey, 0, this.server.encryptionKeyLength);
				bufferIndex += this.server.encryptionKeyLength;
				if (this.byteCount > this.server.encryptionKeyLength) {
					int len = 0;
					if ((this.negotiatedFlags2 & SmbConstants.FLAGS2_UNICODE) == SmbConstants.FLAGS2_UNICODE) {
						len = Strings.findUNITermination(buffer, bufferIndex, 256);
						this.server.oemDomainName = Strings.fromUNIBytes(buffer, bufferIndex, len);
					}
					else {
						len = Strings.findTermination(buffer, bufferIndex, 256);
						this.server.oemDomainName = Strings.fromOEMBytes(buffer, bufferIndex, len, getConfig());
					}
					bufferIndex += len;
				}
				else {
					this.server.oemDomainName = "";
				}
			}
			else {
				this.server.guid = new byte[16];
				Array.Copy(buffer, bufferIndex, this.server.guid, 0, 16);
				bufferIndex += this.server.guid.Length;
				this.server.oemDomainName = "";

				if (this.byteCount > 16) {
					// have initial spnego token
					this.server.encryptionKeyLength = this.byteCount - 16;
					this.server.encryptionKey = new byte[this.server.encryptionKeyLength];
					Array.Copy(buffer, bufferIndex, this.server.encryptionKey, 0, this.server.encryptionKeyLength);
					if (log.isDebugEnabled()) {
						log.debug(string.Format("Have initial token {0}", Hexdump.toHexString(this.server.encryptionKey, 0, this.server.encryptionKeyLength)));
					}
				}
			}

			return bufferIndex - start;
		}


		public override string ToString() {
			return "SmbComNegotiateResponse[" + base.ToString() + ",wordCount=" + this.wordCount + ",dialectIndex=" + this.dialectIndex + ",securityMode=0x" + Hexdump.toHexString(this.server.securityMode, 1) + ",security=" + (this.server.security == SmbConstants.SECURITY_SHARE ? "share" : "user") + ",encryptedPasswords=" + this.server.encryptedPasswords + ",maxMpxCount=" + this.server.smaxMpxCount + ",maxNumberVcs=" + this.server.maxNumberVcs + ",maxBufferSize=" + this.server.maxBufferSize + ",maxRawSize=" + this.server.maxRawSize + ",sessionKey=0x" + Hexdump.toHexString(this.server.sessKey, 8) + ",capabilities=0x" + Hexdump.toHexString(this.server.scapabilities, 8) + ",serverTime=" + DateTimeOffset.FromUnixTimeMilliseconds(this.server.serverTime) + ",serverTimeZone=" + this.server.serverTimeZone + ",encryptionKeyLength=" + this.server.encryptionKeyLength + ",byteCount=" + this.byteCount + ",oemDomainName=" + this.server.oemDomainName + "]";
		}

	}

}