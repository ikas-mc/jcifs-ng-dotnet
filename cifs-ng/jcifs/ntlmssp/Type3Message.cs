using jcifs.util;

using System;
using System.Collections.Generic;
using System.IO;
using cifs_ng.lib;
using cifs_ng.lib.ext;
using cifs_ng.lib.security;
using jcifs.lib;
using CIFSContext = jcifs.CIFSContext;
using CIFSException = jcifs.CIFSException;
using SmbConstants = jcifs.SmbConstants;
using AvFlags = jcifs.ntlmssp.av.AvFlags;
using AvPair = jcifs.ntlmssp.av.AvPair;
using AvPairs = jcifs.ntlmssp.av.AvPairs;
using AvSingleHost = jcifs.ntlmssp.av.AvSingleHost;
using AvTargetName = jcifs.ntlmssp.av.AvTargetName;
using AvTimestamp = jcifs.ntlmssp.av.AvTimestamp;
using NtlmUtil = jcifs.smb.NtlmUtil;
using Crypto = jcifs.util.Crypto;

/* jcifs smb client library in Java
 * Copyright (C) 2002  "Michael B. Allen" <jcifs at samba dot org>
 *                 "Eric Glass" <jcifs at samba dot org>
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

namespace jcifs.ntlmssp {




	/// <summary>
	/// Represents an NTLMSSP Type-3 message.
	/// </summary>
	public class Type3Message : NtlmMessage {

		private byte[] lmResponse;
		private byte[] ntResponse;
		private string domain;
		private string user;
		private string workstation;
		private byte[] masterKey = null;
		private byte[] sessionKey = null;
		private byte[] mic = null;
		private bool micRequired;


		/// <summary>
		/// Creates a Type-3 message using default values from the current
		/// environment.
		/// </summary>
		/// <param name="tc">
		///            context to use </param>
		public Type3Message(CIFSContext tc) {
			setFlags(getDefaultFlags(tc));
			setDomain(tc.getConfig().getDefaultDomain());
			setUser(tc.getConfig().getDefaultUsername());
			setWorkstation(tc.getNameServiceClient().getLocalHost().getHostName());
		}


		/// <summary>
		/// Creates a Type-3 message in response to the given Type-2 message.
		/// </summary>
		/// <param name="tc">
		///            context to use </param>
		/// <param name="type2">
		///            The Type-2 message which this represents a response to. </param>
		/// <param name="targetName">
		///            SPN of the target system, optional </param>
		/// <param name="password">
		///            The password to use when constructing the response. </param>
		/// <param name="domain">
		///            The domain in which the user has an account. </param>
		/// <param name="user">
		///            The username for the authenticating user. </param>
		/// <param name="workstation">
		///            The workstation from which authentication is
		///            taking place. </param>
		/// <param name="flags"> </param>
		/// <exception cref="GeneralSecurityException"> </exception>
		/// <exception cref="CIFSException"> </exception>
		/// throws GeneralSecurityException, jcifs.CIFSException
		public Type3Message(CIFSContext tc, Type2Message type2, string targetName, string password, string domain, string user, string workstation, int flags) : this(tc, type2, targetName, password, domain, user, workstation, flags, false) {
			// keep old behavior of anonymous auth when no password is provided
		}


		/// <summary>
		/// Creates a Type-3 message in response to the given Type-2 message.
		/// </summary>
		/// <param name="tc">
		///            context to use </param>
		/// <param name="type2">
		///            The Type-2 message which this represents a response to. </param>
		/// <param name="targetName">
		///            SPN of the target system, optional </param>
		/// <param name="password">
		///            The password to use when constructing the response. </param>
		/// <param name="domain">
		///            The domain in which the user has an account. </param>
		/// <param name="user">
		///            The username for the authenticating user. </param>
		/// <param name="workstation">
		///            The workstation from which authentication is
		///            taking place. </param>
		/// <param name="flags"> </param>
		/// <param name="nonAnonymous">
		///            actually perform authentication with empty password </param>
		/// <exception cref="GeneralSecurityException"> </exception>
		/// <exception cref="CIFSException"> </exception>
		/// throws GeneralSecurityException, jcifs.CIFSException
		public Type3Message(CIFSContext tc, Type2Message type2, string targetName, string password, string domain, string user, string workstation, int flags, bool nonAnonymous) : this(tc, type2, targetName, null, password, domain, user, workstation, flags, nonAnonymous) {
		}


		/// <summary>
		/// Creates a Type-3 message in response to the given Type-2 message.
		/// </summary>
		/// <param name="tc">
		///            context to use </param>
		/// <param name="type2">
		///            The Type-2 message which this represents a response to. </param>
		/// <param name="targetName">
		///            SPN of the target system, optional </param>
		/// <param name="passwordHash">
		///            The NT password hash to use when constructing the response. </param>
		/// <param name="domain">
		///            The domain in which the user has an account. </param>
		/// <param name="user">
		///            The username for the authenticating user. </param>
		/// <param name="workstation">
		///            The workstation from which authentication is
		///            taking place. </param>
		/// <param name="flags"> </param>
		/// <exception cref="GeneralSecurityException"> </exception>
		/// <exception cref="CIFSException"> </exception>
		/// throws CIFSException, java.security.GeneralSecurityException
		public Type3Message(CIFSContext tc, Type2Message type2, string targetName, byte[] passwordHash, string domain, string user, string workstation, int flags) : this(tc, type2, targetName, passwordHash, null, domain, user, workstation, flags, true) {
		}


		/// <summary>
		/// Creates a Type-3 message in response to the given Type-2 message.
		/// </summary>
		/// <param name="tc">
		///            context to use </param>
		/// <param name="type2">
		///            The Type-2 message which this represents a response to. </param>
		/// <param name="targetName">
		///            SPN of the target system, optional </param>
		/// <param name="passwordHash">
		///            The NT password hash, takes precedence over password (which is no longer required unless legacy LM
		///            authentication is needed) </param>
		/// <param name="password">
		///            The password to use when constructing the response. </param>
		/// <param name="domain">
		///            The domain in which the user has an account. </param>
		/// <param name="user">
		///            The username for the authenticating user. </param>
		/// <param name="workstation">
		///            The workstation from which authentication is
		///            taking place. </param>
		/// <param name="flags"> </param>
		/// <param name="nonAnonymous">
		///            actually perform authentication with empty password </param>
		/// <exception cref="GeneralSecurityException"> </exception>
		/// <exception cref="CIFSException"> </exception>
		/// throws GeneralSecurityException, jcifs.CIFSException
		public Type3Message(CIFSContext tc, Type2Message type2, string targetName, byte[] passwordHash, string password, string domain, string user, string workstation, int flags, bool nonAnonymous) {
			setFlags(flags | getDefaultFlags(tc, type2));
			setWorkstation(workstation);
			setDomain(domain);
			setUser(user);

			if ((password == null && passwordHash == null) || (!nonAnonymous && (password != null && password.Length == 0))) {
				setLMResponse(null);
				setNTResponse(null);
				return;
			}

			if (passwordHash == null) {
				passwordHash = NtlmUtil.getNTHash(password);
			}

			switch (tc.getConfig().getLanManCompatibility()) {
			case 0:
			case 1:
				if (!getFlag(NtlmFlags.NTLMSSP_NEGOTIATE_EXTENDED_SESSIONSECURITY)) {
					setLMResponse(getLMResponse(tc, type2, password));
					setNTResponse(getNTResponse(tc, type2, passwordHash));
				}
				else {
					// NTLM2 Session Response

					byte[] clientChallenge = new byte[24];
					tc.getConfig().getRandom().NextBytes(clientChallenge);
					Array.Clear(clientChallenge,8,16);

					byte[] ntlm2Response = NtlmUtil.getNTLM2Response(passwordHash, type2.getChallenge(), clientChallenge);

					setLMResponse(clientChallenge);
					setNTResponse(ntlm2Response);

					byte[] sessionNonce = new byte[16];
					Array.Copy(type2.getChallenge(), 0, sessionNonce, 0, 8);
					Array.Copy(clientChallenge, 0, sessionNonce, 8, 8);

					MessageDigest md4 = Crypto.getMD4();
					md4.update(passwordHash);
					byte[] userSessionKey1 = md4.digest();

					MessageDigest hmac1 = Crypto.getHMACT64(userSessionKey1);
					hmac1.update(sessionNonce);
					byte[] ntlm2SessionKey = hmac1.digest();

					if (getFlag(NtlmFlags.NTLMSSP_NEGOTIATE_KEY_EXCH)) {
						this.masterKey = new byte[16];
						tc.getConfig().getRandom().NextBytes(this.masterKey);

						byte[] exchangedKey = new byte[16];
						Cipher arcfour = Crypto.getArcfour(ntlm2SessionKey);
						arcfour.update(this.masterKey, 0, 16, exchangedKey, 0);
						setEncryptedSessionKey(exchangedKey);
					}
					else {
						this.masterKey = ntlm2SessionKey;
					}
				}
				break;
			case 2:
				byte[] nt = getNTResponse(tc, type2, passwordHash);
				setLMResponse(nt);
				setNTResponse(nt);
				break;
			case 3:
			case 4:
			case 5:
				byte[] ntlmClientChallengeInfo = type2.getTargetInformation();
				IList<AvPair> avPairs = ntlmClientChallengeInfo != null ? AvPairs.decode(ntlmClientChallengeInfo) : null;

				// if targetInfo has an MsvAvTimestamp
				// client should not send LmChallengeResponse
				bool haveTimestamp = AvPairs.contains(avPairs, AvPair.MsvAvTimestamp);
				if (!haveTimestamp) {
					byte[] lmClientChallenge = new byte[8];
					tc.getConfig().getRandom().NextBytes(lmClientChallenge);
					setLMResponse(getLMv2Response(tc, type2, domain, user, passwordHash, lmClientChallenge));
				}
				else {
					setLMResponse(new byte[24]);
				}

				if (avPairs != null) {
					// make sure to set the TARGET_INFO flag as we are sending
					setFlag(NtlmFlags.NTLMSSP_NEGOTIATE_TARGET_INFO, true);
				}

				byte[] responseKeyNT = NtlmUtil.nTOWFv2(domain, user, passwordHash);
				byte[] ntlmClientChallenge = new byte[8];
				tc.getConfig().getRandom().NextBytes(ntlmClientChallenge);

				long ts = (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + SmbConstants.MILLISECONDS_BETWEEN_1970_AND_1601) * 10000;
				if (haveTimestamp) {
					ts = ((AvTimestamp) AvPairs.get(avPairs, AvPair.MsvAvTimestamp)).getTimestamp();
				}

				setNTResponse(getNTLMv2Response(tc, type2, responseKeyNT, ntlmClientChallenge, makeAvPairs(tc, targetName, avPairs, haveTimestamp, ts), ts));

				MessageDigest hmac = Crypto.getHMACT64(responseKeyNT);
				hmac.update(this.ntResponse, 0, 16); // only first 16 bytes of ntResponse
				byte[] userSessionKey = hmac.digest();

				if (getFlag(NtlmFlags.NTLMSSP_NEGOTIATE_KEY_EXCH)) {
					this.masterKey = new byte[16];
					tc.getConfig().getRandom().NextBytes(this.masterKey);

					byte[] encryptedKey = new byte[16];
					Cipher rc4 = Crypto.getArcfour(userSessionKey);
					rc4.update(this.masterKey, 0, 16, encryptedKey, 0);
					setEncryptedSessionKey(encryptedKey);
				}
				else {
					this.masterKey = userSessionKey;
				}

				break;
			default:
				setLMResponse(getLMResponse(tc, type2, password));
				setNTResponse(getNTResponse(tc, type2, passwordHash));
			break;
			}

		}


		private byte[] makeAvPairs(CIFSContext tc, string targetName, IList<AvPair> serverAvPairs, bool haveServerTimestamp, long ts) {
			if (!tc.getConfig().isEnforceSpnegoIntegrity() && serverAvPairs == null) {
				return null;
			}
			else if (serverAvPairs == null) {
				serverAvPairs = new List<AvPair>();
			}

			if (getFlag(NtlmFlags.NTLMSSP_NEGOTIATE_SIGN) && (tc.getConfig().isEnforceSpnegoIntegrity() || (haveServerTimestamp && !tc.getConfig().isDisableSpnegoIntegrity()))) {
				// should provide MIC
				this.micRequired = true;
				this.mic = new byte[16];
				int curFlags = 0;
				AvFlags cur = (AvFlags) AvPairs.get(serverAvPairs, AvPair.MsvAvFlags);
				if (cur != null) {
					curFlags = cur.getFlags();
				}
				curFlags |= 0x2; // MAC present
				AvPairs.replace(serverAvPairs, new AvFlags(curFlags));
			}

			AvPairs.replace(serverAvPairs, new AvTimestamp(ts));

			if (targetName != null) {
				AvPairs.replace(serverAvPairs, new AvTargetName(targetName));
			}

			// possibly add channel bindings
			AvPairs.replace(serverAvPairs, new AvPair(0xa, new byte[16]));
			AvPairs.replace(serverAvPairs, new AvSingleHost(tc.getConfig()));

			return AvPairs.encode(serverAvPairs);
		}


		/// <summary>
		/// Sets the MIC
		/// </summary>
		/// <param name="type1"> </param>
		/// <param name="type2"> </param>
		/// <exception cref="GeneralSecurityException"> </exception>
		/// <exception cref="IOException"> </exception>
		/// throws GeneralSecurityException, java.io.IOException
		public virtual void setupMIC(byte[] type1, byte[] type2) {
			byte[] sk = this.masterKey;
			if (sk == null) {
				return;
			}
			MessageDigest mac = Crypto.getHMACT64(sk);
			mac.update(type1);
			mac.update(type2);
			byte[] type3 = toByteArray();
			mac.update(type3);
			setMic(mac.digest());
		}


		/// <summary>
		/// Creates a Type-3 message with the specified parameters.
		/// </summary>
		/// <param name="flags">
		///            The flags to apply to this message. </param>
		/// <param name="lmResponse">
		///            The LanManager/LMv2 response. </param>
		/// <param name="ntResponse">
		///            The NT/NTLMv2 response. </param>
		/// <param name="domain">
		///            The domain in which the user has an account. </param>
		/// <param name="user">
		///            The username for the authenticating user. </param>
		/// <param name="workstation">
		///            The workstation from which authentication is
		///            taking place. </param>
		public Type3Message(int flags, byte[] lmResponse, byte[] ntResponse, string domain, string user, string workstation) {
			setFlags(flags);
			setLMResponse(lmResponse);
			setNTResponse(ntResponse);
			setDomain(domain);
			setUser(user);
			setWorkstation(workstation);
		}


		/// <summary>
		/// Creates a Type-3 message using the given raw Type-3 material.
		/// </summary>
		/// <param name="material">
		///            The raw Type-3 material used to construct this message. </param>
		/// <exception cref="IOException">
		///             If an error occurs while parsing the material. </exception>
		/// throws java.io.IOException
		public Type3Message(byte[] material) {
			parse(material);
		}


		/// <summary>
		/// Returns the default flags for a generic Type-3 message in the
		/// current environment.
		/// </summary>
		/// <param name="tc">
		///            context to use </param>
		/// <returns> An <code>int</code> containing the default flags. </returns>
		public static int getDefaultFlags(CIFSContext tc) {
			return NtlmFlags.NTLMSSP_NEGOTIATE_NTLM | NtlmFlags.NTLMSSP_NEGOTIATE_VERSION | (tc.getConfig().isUseUnicode() ? NtlmFlags.NTLMSSP_NEGOTIATE_UNICODE : NtlmFlags.NTLMSSP_NEGOTIATE_OEM);
		}


		/// <summary>
		/// Returns the default flags for a Type-3 message created in response
		/// to the given Type-2 message in the current environment.
		/// </summary>
		/// <param name="tc">
		///            context to use </param>
		/// <param name="type2">
		///            The Type-2 message. </param>
		/// <returns> An <code>int</code> containing the default flags. </returns>
		public static int getDefaultFlags(CIFSContext tc, Type2Message type2) {
			if (type2 == null) {
				return getDefaultFlags(tc);
			}
			int flags = NtlmFlags.NTLMSSP_NEGOTIATE_NTLM | NtlmFlags.NTLMSSP_NEGOTIATE_VERSION;
			flags |= type2.getFlag(NtlmFlags.NTLMSSP_NEGOTIATE_UNICODE) ? NtlmFlags.NTLMSSP_NEGOTIATE_UNICODE : NtlmFlags.NTLMSSP_NEGOTIATE_OEM;
			return flags;
		}


		/// <summary>
		/// Returns the LanManager/LMv2 response.
		/// </summary>
		/// <returns> A <code>byte[]</code> containing the LanManager response. </returns>
		public virtual byte[] getLMResponse() {
			return this.lmResponse;
		}


		/// <summary>
		/// Sets the LanManager/LMv2 response for this message.
		/// </summary>
		/// <param name="lmResponse">
		///            The LanManager response. </param>
		public virtual void setLMResponse(byte[] lmResponse) {
			this.lmResponse = lmResponse;
		}


		/// <summary>
		/// Returns the NT/NTLMv2 response.
		/// </summary>
		/// <returns> A <code>byte[]</code> containing the NT/NTLMv2 response. </returns>
		public virtual byte[] getNTResponse() {
			return this.ntResponse;
		}


		/// <summary>
		/// Sets the NT/NTLMv2 response for this message.
		/// </summary>
		/// <param name="ntResponse">
		///            The NT/NTLMv2 response. </param>
		public virtual void setNTResponse(byte[] ntResponse) {
			this.ntResponse = ntResponse;
		}


		/// <summary>
		/// Returns the domain in which the user has an account.
		/// </summary>
		/// <returns> A <code>String</code> containing the domain for the user. </returns>
		public virtual string getDomain() {
			return this.domain;
		}


		/// <summary>
		/// Sets the domain for this message.
		/// </summary>
		/// <param name="domain">
		///            The domain. </param>
		public virtual void setDomain(string domain) {
			this.domain = domain;
		}


		/// <summary>
		/// Returns the username for the authenticating user.
		/// </summary>
		/// <returns> A <code>String</code> containing the user for this message. </returns>
		public virtual string getUser() {
			return this.user;
		}


		/// <summary>
		/// Sets the user for this message.
		/// </summary>
		/// <param name="user">
		///            The user. </param>
		public virtual void setUser(string user) {
			this.user = user;
		}


		/// <summary>
		/// Returns the workstation from which authentication is being performed.
		/// </summary>
		/// <returns> A <code>String</code> containing the workstation. </returns>
		public virtual string getWorkstation() {
			return this.workstation;
		}


		/// <summary>
		/// Sets the workstation for this message.
		/// </summary>
		/// <param name="workstation">
		///            The workstation. </param>
		public virtual void setWorkstation(string workstation) {
			this.workstation = workstation;
		}


		/// <summary>
		/// The real session key if the regular session key is actually
		/// the encrypted version used for key exchange.
		/// </summary>
		/// <returns> A <code>byte[]</code> containing the session key. </returns>
		public virtual byte[] getMasterKey() {
			return this.masterKey;
		}


		/// <summary>
		/// Returns the session key.
		/// 
		/// This is the encrypted session key included in the message,
		/// if the actual session key is desired use <seealso cref="getMasterKey()"/> instead.
		/// </summary>
		/// <returns> A <code>byte[]</code> containing the encrypted session key. </returns>
		public virtual byte[] getEncryptedSessionKey() {
			return this.sessionKey;
		}


		/// <summary>
		/// Sets the session key.
		/// </summary>
		/// <param name="sessionKey">
		///            The session key. </param>
		public virtual void setEncryptedSessionKey(byte[] sessionKey) {
			this.sessionKey = sessionKey;
		}


		/// <returns> A <code>byte[]</code> containing the message integrity code. </returns>
		public virtual byte[] getMic() {
			return this.mic;
		}


		/// <param name="mic">
		///            NTLM mic to set (16 bytes) </param>
		public virtual void setMic(byte[] mic) {
			this.mic = mic;
		}


		/// <returns> whether a MIC should be calulated </returns>
		public virtual bool isMICRequired() {
			return this.micRequired;
		}


		/// throws java.io.IOException
		public override byte[] toByteArray() {
			int size = 64;
			bool unicode = getFlag(NtlmFlags.NTLMSSP_NEGOTIATE_UNICODE);
			string oemCp = unicode ? null : getOEMEncoding();

			string domainName = getDomain();
			byte[] domainBytes = null;
			if (domainName != null && domainName.Length != 0) {
				domainBytes = unicode ? domainName.getBytes(UNI_ENCODING) : domainName.getBytes(oemCp);
				size += domainBytes.Length;
			}

			string userName = getUser();
			byte[] userBytes = null;
			if (userName != null && userName.Length != 0) {
				userBytes = unicode ? userName.getBytes(UNI_ENCODING) : userName.ToUpper().getBytes(oemCp);
				size += userBytes.Length;
			}

			string workstationName = getWorkstation();
			byte[] workstationBytes = null;
			if (workstationName != null && workstationName.Length != 0) {
				workstationBytes = unicode ? workstationName.getBytes(UNI_ENCODING) : workstationName.ToUpper().getBytes(oemCp);
				size += workstationBytes.Length;
			}

			byte[] micBytes = getMic();
			if (micBytes != null) {
				size += 8 + 16;
			}
			else if (getFlag(NtlmFlags.NTLMSSP_NEGOTIATE_VERSION)) {
				size += 8;
			}

			byte[] lmResponseBytes = getLMResponse();
			size += (lmResponseBytes != null) ? lmResponseBytes.Length : 0;

			byte[] ntResponseBytes = getNTResponse();
			size += (ntResponseBytes != null) ? ntResponseBytes.Length : 0;

			byte[] sessionKeyBytes = getEncryptedSessionKey();
			size += (sessionKeyBytes != null) ? sessionKeyBytes.Length : 0;

			byte[] type3 = new byte[size];
			int pos = 0;

			Array.Copy(NTLMSSP_SIGNATURE, 0, type3, 0, 8);
			pos += 8;

			writeULong(type3, pos, NTLMSSP_TYPE3);
			pos += 4;

			int lmOff = writeSecurityBuffer(type3, 12, lmResponseBytes);
			pos += 8;
			int ntOff = writeSecurityBuffer(type3, 20, ntResponseBytes);
			pos += 8;
			int domOff = writeSecurityBuffer(type3, 28, domainBytes);
			pos += 8;
			int userOff = writeSecurityBuffer(type3, 36, userBytes);
			pos += 8;
			int wsOff = writeSecurityBuffer(type3, 44, workstationBytes);
			pos += 8;
			int skOff = writeSecurityBuffer(type3, 52, sessionKeyBytes);
			pos += 8;

			writeULong(type3, pos, getFlags());
			pos += 4;

			if (getFlag(NtlmFlags.NTLMSSP_NEGOTIATE_VERSION)) {
				Array.Copy(NTLMSSP_VERSION, 0, type3, pos, NTLMSSP_VERSION.Length);
				pos += NTLMSSP_VERSION.Length;
			}
			else if (micBytes != null) {
				pos += NTLMSSP_VERSION.Length;
			}

			if (micBytes != null) {
				Array.Copy(micBytes, 0, type3, pos, 16);
				pos += 16;
			}

			pos += writeSecurityBufferContent(type3, pos, lmOff, lmResponseBytes);
			pos += writeSecurityBufferContent(type3, pos, ntOff, ntResponseBytes);
			pos += writeSecurityBufferContent(type3, pos, domOff, domainBytes);
			pos += writeSecurityBufferContent(type3, pos, userOff, userBytes);
			pos += writeSecurityBufferContent(type3, pos, wsOff, workstationBytes);
			pos += writeSecurityBufferContent(type3, pos, skOff, sessionKeyBytes);

			return type3;

		}


		public override string ToString() {
			string userString = getUser();
			string domainString = getDomain();
			string workstationString = getWorkstation();
			byte[] lmResponseBytes = getLMResponse();
			byte[] ntResponseBytes = getNTResponse();
			byte[] sessionKeyBytes = getEncryptedSessionKey();

			return "Type3Message[domain=" + domainString + ",user=" + userString + ",workstation=" + workstationString + ",lmResponse=" + (lmResponseBytes == null ? "null" : "<" + lmResponseBytes.Length + " bytes>") + ",ntResponse=" + (ntResponseBytes == null ? "null" : "<" + ntResponseBytes.Length + " bytes>") + ",sessionKey=" + (sessionKeyBytes == null ? "null" : "<" + sessionKeyBytes.Length + " bytes>") + ",flags=0x" + Hexdump.toHexString(getFlags(), 8) + "]";
		}


		/// <summary>
		/// Constructs the LanManager response to the given Type-2 message using
		/// the supplied password.
		/// </summary>
		/// <param name="tc">
		///            context to use </param>
		/// <param name="type2">
		///            The Type-2 message. </param>
		/// <param name="password">
		///            The password. </param>
		/// <returns> A <code>byte[]</code> containing the LanManager response. </returns>
		/// <exception cref="GeneralSecurityException"> </exception>
		/// throws java.security.GeneralSecurityException
		public static byte[] getLMResponse(CIFSContext tc, Type2Message type2, string password) {
			if (type2 == null ||password == null) {
				return null;
			}
			return NtlmUtil.getPreNTLMResponse(tc, password, type2.getChallenge());
		}


		/// 
		/// <param name="tc"> </param>
		/// <param name="type2"> </param>
		/// <param name="domain"> </param>
		/// <param name="user"> </param>
		/// <param name="password"> </param>
		/// <param name="clientChallenge"> </param>
		/// <returns> the calculated response </returns>
		/// <exception cref="GeneralSecurityException"> </exception>
		/// throws java.security.GeneralSecurityException
		public static byte[] getLMv2Response(CIFSContext tc, Type2Message type2, string domain, string user, string password, byte[] clientChallenge) {
			if (password == null) {
				return null;
			}
			return getLMv2Response(tc, type2, domain, user, NtlmUtil.getNTHash(password), clientChallenge);
		}


		/// 
		/// <param name="tc"> </param>
		/// <param name="type2"> </param>
		/// <param name="domain"> </param>
		/// <param name="user"> </param>
		/// <param name="passwordHash">
		///            NT password hash </param>
		/// <param name="clientChallenge"> </param>
		/// <returns> the calculated response </returns>
		/// <exception cref="GeneralSecurityException"> </exception>
		/// throws java.security.GeneralSecurityException
		public static byte[] getLMv2Response(CIFSContext tc, Type2Message type2, string domain, string user, byte[] passwordHash, byte[] clientChallenge) {
			if (type2 == null ||domain == null ||user == null || passwordHash == null || clientChallenge == null) {
				return null;
			}
			return NtlmUtil.getLMv2Response(domain, user, passwordHash, type2.getChallenge(), clientChallenge);
		}


		/// 
		/// <param name="tc">
		///            context to use </param>
		/// <param name="type2">
		///            The Type-2 message. </param>
		/// <param name="responseKeyNT"> </param>
		/// <param name="clientChallenge"> </param>
		/// <param name="clientChallengeInfo"> </param>
		/// <param name="ts">
		///            timestamp (nanos since 1601) </param>
		/// <returns> A <code>byte[]</code> containing the NTLMv2 response. </returns>
		public static byte[] getNTLMv2Response(CIFSContext tc, Type2Message type2, byte[] responseKeyNT, byte[] clientChallenge, byte[] clientChallengeInfo, long ts) {
			if (type2 == null || responseKeyNT == null || clientChallenge == null) {
				return null;
			}
			return NtlmUtil.getNTLMv2Response(responseKeyNT, type2.getChallenge(), clientChallenge, ts, clientChallengeInfo);
		}


		/// <summary>
		/// Constructs the NT response to the given Type-2 message using
		/// the supplied password.
		/// </summary>
		/// <param name="tc">
		///            context to use </param>
		/// <param name="type2">
		///            The Type-2 message. </param>
		/// <param name="password">
		///            The password. </param>
		/// <returns> A <code>byte[]</code> containing the NT response. </returns>
		/// <exception cref="GeneralSecurityException"> </exception>
		/// throws java.security.GeneralSecurityException
		public static byte[] getNTResponse(CIFSContext tc, Type2Message type2, string password) {
			if (password == null) {
				return null;
			}
			return getNTResponse(tc, type2, NtlmUtil.getNTHash(password));
		}


		/// <summary>
		/// Constructs the NT response to the given Type-2 message using
		/// the supplied password.
		/// </summary>
		/// <param name="tc">
		///            context to use </param>
		/// <param name="type2">
		///            The Type-2 message. </param>
		/// <param name="passwordHash">
		///            The NT password hash. </param>
		/// <returns> A <code>byte[]</code> containing the NT response. </returns>
		/// <exception cref="GeneralSecurityException"> </exception>
		/// throws java.security.GeneralSecurityException
		public static byte[] getNTResponse(CIFSContext tc, Type2Message type2, byte[] passwordHash) {
			if (type2 == null || passwordHash == null) {
				return null;
			}
			return NtlmUtil.getNTLMResponse(passwordHash, type2.getChallenge());
		}


		/// throws java.io.IOException
		private void parse(byte[] material) {
			int pos = 0;
			for (int i = 0; i < 8; i++) {
				if (material[i] != NTLMSSP_SIGNATURE[i]) {
					throw new IOException("Not an NTLMSSP message.");
				}
			}

			pos += 8;
			if (readULong(material, pos) != NTLMSSP_TYPE3) {
				throw new IOException("Not a Type 3 message.");
			}
			pos += 4;

			byte[] lmResponseBytes = readSecurityBuffer(material, pos);
			setLMResponse(lmResponseBytes);
			int lmResponseOffset = readULong(material, pos + 4);
			pos += 8;

			byte[] ntResponseBytes = readSecurityBuffer(material, pos);
			setNTResponse(ntResponseBytes);
			int ntResponseOffset = readULong(material, pos + 4);
			pos += 8;

			byte[] domainBytes = readSecurityBuffer(material, pos);
			int domainOffset = readULong(material, pos + 4);
			pos += 8;

			byte[] userBytes = readSecurityBuffer(material, pos);
			int userOffset = readULong(material, pos + 4);
			pos += 8;

			byte[] workstationBytes = readSecurityBuffer(material, pos);
			int workstationOffset = readULong(material, pos + 4);
			pos += 8;

			bool end = false;
			int flags;
			string charset;
			if (lmResponseOffset < pos + 12 || ntResponseOffset < pos + 12 || domainOffset < pos + 12 || userOffset < pos + 12 || workstationOffset < pos + 12) {
				// no room for SK/Flags
				flags = NtlmFlags.NTLMSSP_NEGOTIATE_NTLM | NtlmFlags.NTLMSSP_NEGOTIATE_OEM;
				setFlags(flags);
				charset = getOEMEncoding();
				end = true;
			}
			else {
				setEncryptedSessionKey(readSecurityBuffer(material, pos));
				pos += 8;

				flags = readULong(material, pos);
				setFlags(flags);
				pos += 4;

				charset = ((flags & NtlmFlags.NTLMSSP_NEGOTIATE_UNICODE) != 0) ? UNI_ENCODING : getOEMEncoding();
			}

			setDomain(domainBytes.toString(charset));
			setUser(userBytes.toString(charset));
			setWorkstation(workstationBytes.toString(charset));

			int micLen = pos + 24; // Version + MIC
			if (end || lmResponseOffset < micLen || ntResponseOffset < micLen || domainOffset < micLen || userOffset < micLen || workstationOffset < micLen) {
				return;
			}

			pos += 8; // Version

			byte[] m = new byte[16];
			Array.Copy(material, pos, m, 0, m.Length);
			setMic(m);
		}

	}

}