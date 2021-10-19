using System;
using System.Text;
using cifs_ng.lib;
using cifs_ng.lib.ext;
using cifs_ng.lib.security;
using cifs_ng.lib.threading;
using jcifs.lib;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Security;
using Logger = org.slf4j.Logger;
using LoggerFactory = org.slf4j.LoggerFactory;
using CIFSContext = jcifs.CIFSContext;
using CIFSException = jcifs.CIFSException;
using SMBUtil = jcifs.@internal.util.SMBUtil;
using NtlmFlags = jcifs.ntlmssp.NtlmFlags;
using Type1Message = jcifs.ntlmssp.Type1Message;
using Type2Message = jcifs.ntlmssp.Type2Message;
using Type3Message = jcifs.ntlmssp.Type3Message;
using Crypto = jcifs.util.Crypto;
using Hexdump = jcifs.util.Hexdump;

/* jcifs smb client library in Java
 * Copyright (C) 2008  "Michael B. Allen" <jcifs at samba dot org>
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

namespace jcifs.smb {





	/// <summary>
	/// For initiating NTLM authentication (including NTLMv2). If you want to add NTLMv2 authentication support to something
	/// this is what you want to use. See the code for details. Note that JCIFS does not implement the acceptor side of NTLM
	/// authentication.
	/// 
	/// </summary>
	public class NtlmContext : SSPContext {

		private const string S2C_SIGN_CONSTANT = "session key to server-to-client signing key magic constant";
		private const string S2C_SEAL_CONSTANT = "session key to server-to-client sealing key magic constant";

		private const string C2S_SIGN_CONSTANT = "session key to client-to-server signing key magic constant";
		private const string C2S_SEAL_CONSTANT = "session key to client-to-server sealing key magic constant";

		private static readonly Logger log = LoggerFactory.getLogger(typeof(NtlmContext));

		/// 
		public static DerObjectIdentifier NTLMSSP_OID;

		static NtlmContext() {
			try {
				NTLMSSP_OID = new DerObjectIdentifier("1.3.6.1.4.1.311.2.2.10");
			}
			catch (System.ArgumentException e) {
				log.error("Failed to parse OID", e);
			}
		}

		private NtlmPasswordAuthenticator auth;
		private int ntlmsspFlags;
		private string workstation;
		private bool isEstablishedField = false;
		private byte[] serverChallenge = null;
		private byte[] masterKey = null;
		private string netbiosName = null;

		private readonly bool requireKeyExchange;
		private readonly AtomicInteger signSequence = new AtomicInteger(0);
		private readonly AtomicInteger verifySequence = new AtomicInteger(0);
		private int state = 1;

		private CIFSContext transportContext;

		private string targetName;
		private byte[] type1Bytes;

		private byte[] signKey;
		private byte[] verifyKey;
		private byte[] sealClientKey;
		private byte[] sealServerKey;

		private Cipher sealClientHandle;
		private Cipher sealServerHandle;


		/// <param name="tc">
		///            context to use </param>
		/// <param name="auth">
		///            credentials </param>
		/// <param name="doSigning">
		///            whether signing is requested </param>
		public NtlmContext(CIFSContext tc, NtlmPasswordAuthenticator auth, bool doSigning) {
			this.transportContext = tc;
			this.auth = auth;
			this.ntlmsspFlags = this.ntlmsspFlags | NtlmFlags.NTLMSSP_REQUEST_TARGET | NtlmFlags.NTLMSSP_NEGOTIATE_EXTENDED_SESSIONSECURITY | NtlmFlags.NTLMSSP_NEGOTIATE_128;
			if (!auth.isAnonymous()) {
				this.ntlmsspFlags |= NtlmFlags.NTLMSSP_NEGOTIATE_SIGN | NtlmFlags.NTLMSSP_NEGOTIATE_ALWAYS_SIGN | NtlmFlags.NTLMSSP_NEGOTIATE_KEY_EXCH;
			}
			else if (auth.isGuest()) {
				this.ntlmsspFlags |= NtlmFlags.NTLMSSP_NEGOTIATE_KEY_EXCH;
			}
			else {
				this.ntlmsspFlags |= NtlmFlags.NTLMSSP_NEGOTIATE_ANONYMOUS;
			}
			this.requireKeyExchange = doSigning;
			this.workstation = tc.getConfig().getNetbiosHostname();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.smb.SSPContext#getSupportedMechs() </seealso>
		public virtual DerObjectIdentifier[] getSupportedMechs() {
			return new DerObjectIdentifier[] {NTLMSSP_OID};
		}


		public override string ToString() {
			string ret = "NtlmContext[auth=" + this.auth + ",ntlmsspFlags=0x" + Hexdump.toHexString(this.ntlmsspFlags, 8) + ",workstation=" + this.workstation + ",isEstablished=" + this.isEstablishedField + ",state=" + this.state + ",serverChallenge=";
			if (this.serverChallenge == null) {
				ret += "null";
			}
			else {
				ret += Hexdump.toHexString(this.serverChallenge);
			}
			ret += ",signingKey=";
			if (this.masterKey == null) {
				ret += "null";
			}
			else {
				ret += Hexdump.toHexString(this.masterKey);
			}
			ret += "]";
			return ret;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.smb.SSPContext#getFlags() </seealso>
		public virtual int getFlags() {
			return 0;
		}


		/// 
		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.smb.SSPContext#isSupported(org.bouncycastle.asn1.ASN1ObjectIdentifier) </seealso>
		public virtual bool isSupported(DerObjectIdentifier mechanism) {
			return NTLMSSP_OID.Equals(mechanism);
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.smb.SSPContext#isPreferredMech(org.bouncycastle.asn1.ASN1ObjectIdentifier) </seealso>
		public virtual bool isPreferredMech(DerObjectIdentifier mechanism) {
			return this.auth.isPreferredMech(mechanism);
		}


		public virtual bool isEstablished() {
			return this.isEstablishedField;
		}


		/// <returns> the server's challenge </returns>
		public virtual byte[] getServerChallenge() {
			return this.serverChallenge;
		}


		public virtual byte[] getSigningKey() {
			return this.masterKey;
		}


		public virtual string getNetbiosName() {
			return this.netbiosName;
		}


		/// <param name="targetName">
		///            the target's SPN </param>
		public virtual void setTargetName(string targetName) {
			this.targetName = targetName;
		}


		/// throws SmbException
		public virtual byte[] initSecContext(byte[] token, int offset, int len) {
			switch (this.state) {
			case 1:
				return makeNegotiate(token);
			case 2:
				return makeAuthenticate(token);
			default:
				throw new SmbException("Invalid state");
			}
		}


		/// throws SmbException
		protected internal virtual byte[] makeAuthenticate(byte[] token) {
			try {
				Type2Message msg2 = new Type2Message(token);

				if (log.isTraceEnabled()) {
					log.trace(msg2.ToString());
					log.trace(Hexdump.toHexString(token));
				}

				this.serverChallenge = msg2.getChallenge();

				if (this.requireKeyExchange) {
					if (this.transportContext.getConfig().isEnforceSpnegoIntegrity() && (!msg2.getFlag(NtlmFlags.NTLMSSP_NEGOTIATE_KEY_EXCH) || !msg2.getFlag(NtlmFlags.NTLMSSP_NEGOTIATE_EXTENDED_SESSIONSECURITY))) {
						throw new SmbUnsupportedOperationException("Server does not support extended NTLMv2 key exchange");
					}

					if (!msg2.getFlag(NtlmFlags.NTLMSSP_NEGOTIATE_128)) {
						throw new SmbUnsupportedOperationException("Server does not support 128-bit keys");
					}
				}

				this.ntlmsspFlags &= msg2.getFlags();
				Type3Message msg3 = createType3Message(msg2);
				msg3.setupMIC(this.type1Bytes, token);

				byte[] @out = msg3.toByteArray();

				if (log.isTraceEnabled()) {
					log.trace(msg3.ToString());
					log.trace(Hexdump.toHexString(token));
				}

				this.masterKey = msg3.getMasterKey();

				if (this.masterKey != null && (this.ntlmsspFlags & NtlmFlags.NTLMSSP_NEGOTIATE_EXTENDED_SESSIONSECURITY) != 0) {
					initSessionSecurity(msg3.getMasterKey());
				}

				this.isEstablishedField = true;
				this.state++;
				return @out;
			}
			catch (SmbException e) {
				throw e;
			}
			catch (Exception e) {
				throw new SmbException(e.Message, e);
			}
		}


		/// <param name="msg2">
		/// @return </param>
		/// <exception cref="GeneralSecurityException"> </exception>
		/// <exception cref="CIFSException"> </exception>
		/// throws GeneralSecurityException, jcifs.CIFSException
		protected internal virtual Type3Message createType3Message(Type2Message msg2) {
			if (this.auth is NtlmNtHashAuthenticator) {
				return new Type3Message(this.transportContext, msg2, this.targetName, this.auth.getNTHash(), this.auth.getUserDomain(), this.auth.getUsername(), this.workstation, this.ntlmsspFlags);
			}

			return new Type3Message(this.transportContext, msg2, this.targetName, this.auth.isGuest() ? this.transportContext.getConfig().getGuestPassword() : this.auth.getPassword(), this.auth.isGuest() ? null : this.auth.getUserDomain(), this.auth.isGuest() ? this.transportContext.getConfig().getGuestUsername() : this.auth.getUsername(), this.workstation, this.ntlmsspFlags, this.auth.isGuest() || !this.auth.isAnonymous());
		}


		protected internal virtual byte[] makeNegotiate(byte[] token) {
			Type1Message msg1 = new Type1Message(this.transportContext, this.ntlmsspFlags, this.auth.getUserDomain(), this.workstation);
			byte[] @out = msg1.toByteArray();
			this.type1Bytes = @out;

			if (log.isTraceEnabled()) {
				log.trace(msg1.ToString());
				log.trace(Hexdump.toHexString(@out));
			}

			this.state++;
			return @out;
		}


		protected internal virtual void initSessionSecurity(byte[] mk)
		{
			this.signKey = deriveKey(mk, C2S_SIGN_CONSTANT);
			this.verifyKey = deriveKey(mk, S2C_SIGN_CONSTANT);

			if (log.isDebugEnabled()) {
				log.debug("Sign key is " + Hexdump.toHexString(this.signKey));
				log.debug("Verify key is " + Hexdump.toHexString(this.verifyKey));
			}

			this.sealClientKey = deriveKey(mk, C2S_SEAL_CONSTANT);
			this.sealClientHandle = Crypto.getArcfour(this.sealClientKey);
			if (log.isDebugEnabled()) {
				log.debug("Seal key is " + Hexdump.toHexString(this.sealClientKey));
			}

			this.sealServerKey = deriveKey(mk, S2C_SEAL_CONSTANT);
			this.sealServerHandle = Crypto.getArcfour(this.sealServerKey);

			if (log.isDebugEnabled()) {
				log.debug("Server seal key is " + Hexdump.toHexString(this.sealServerKey));
			}
		}


		private static byte[] deriveKey(byte[] masterKey, string cnst) {
			MessageDigest md5 = Crypto.getMD5();
			md5.update(masterKey);
			md5.update(cnst.getBytes(Encoding.ASCII));
			md5.update((byte) 0);
			return md5.digest();
		}


		public virtual bool supportsIntegrity() {
			return true;
		}


		public virtual bool isMICAvailable() {
			return !this.auth.isGuest() && this.signKey != null && this.verifyKey != null;
		}


		/// throws jcifs.CIFSException
		public virtual byte[] calculateMIC(byte[] data) {
			byte[] sk = this.signKey;
			if (sk == null) {
				throw new CIFSException("Signing is not initialized");
			}

			int seqNum = this.signSequence.ReturnValueAndIncrement();
			byte[] seqBytes = new byte[4];
			SMBUtil.writeInt4(seqNum, seqBytes, 0);

			MessageDigest mac = Crypto.getHMACT64(sk);
			mac.update(seqBytes); // sequence
			mac.update(data); // data
			byte[] dgst = mac.digest();
			byte[] trunc = new byte[8];
			Array.Copy(dgst, 0, trunc, 0, 8);

			if (log.isDebugEnabled()) {
				log.debug("Digest " + Hexdump.toHexString(dgst));
				log.debug("Truncated " + Hexdump.toHexString(trunc));
			}

			if ((this.ntlmsspFlags & NtlmFlags.NTLMSSP_NEGOTIATE_KEY_EXCH) != 0) {
				try {
					trunc = this.sealClientHandle.doFinal(trunc);
					if (log.isDebugEnabled()) {
						log.debug("Encrypted " + Hexdump.toHexString(trunc));
					}
				}
				catch (GeneralSecurityException e) {
					throw new CIFSException("Failed to encrypt MIC", e);
				}
			}

			byte[] sig = new byte[16];
			SMBUtil.writeInt4(1, sig, 0); // version
			Array.Copy(trunc, 0, sig, 4, 8); // checksum
			SMBUtil.writeInt4(seqNum, sig, 12); // seqNum

			return sig;
		}


		/// throws jcifs.CIFSException
		public virtual void verifyMIC(byte[] data, byte[] mic) {
			byte[] sk = this.verifyKey;
			if (sk == null) {
				throw new CIFSException("Signing is not initialized");
			}

			int ver = SMBUtil.readInt4(mic, 0);
			if (ver != 1) {
				throw new SmbUnsupportedOperationException("Invalid signature version");
			}

			MessageDigest mac = Crypto.getHMACT64(sk);
			int seq = SMBUtil.readInt4(mic, 12);
			mac.update(mic, 12, 4); // sequence
			byte[] dgst = mac.digest(data); // data
			byte[] trunc = dgst.sub(0, 8);

			if (log.isDebugEnabled()) {
				log.debug("Digest " + Hexdump.toHexString(dgst));
				log.debug("Truncated " + Hexdump.toHexString(trunc));
			}

			bool encrypted = (this.ntlmsspFlags & NtlmFlags.NTLMSSP_NEGOTIATE_KEY_EXCH) != 0;
			if (encrypted) {
				try {
					trunc = this.sealServerHandle.doFinal(trunc);
					if (log.isDebugEnabled()) {
						log.debug("Decrypted " + Hexdump.toHexString(trunc));
					}
				}
				catch (GeneralSecurityException e) {
					throw new CIFSException("Failed to decrypt MIC", e);
				}
			}

			int expectSeq = this.verifySequence.ReturnValueAndIncrement();
			if (expectSeq != seq) {
				throw new CIFSException(string.Format("Invalid MIC sequence, expect {0:D} have {1:D}", expectSeq, seq));
			}

			byte[] verify = new byte[8];
			Array.Copy(mic, 4, verify, 0, 8);
			if (!MessageDigest.isEqual(trunc, verify)) {
				if (log.isDebugEnabled()) {
					log.debug(string.Format("Seq = {0:D} ver = {1:D} encrypted = {2}", seq, ver, encrypted));
					log.debug(string.Format("Expected MIC {0} != {1}", Hexdump.toHexString(trunc), Hexdump.toHexString(verify)));
				}
				throw new CIFSException("Invalid MIC");
			}

		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.smb.SSPContext#dispose() </seealso>
		/// throws SmbException
		public virtual void dispose() {
			this.isEstablishedField = false;
			this.sealClientHandle = null;
			this.sealServerHandle = null;
			this.sealClientKey = null;
			this.sealServerKey = null;
			this.masterKey = null;
			this.signKey = null;
			this.verifyKey = null;
			this.type1Bytes = null;
		}
	}

}