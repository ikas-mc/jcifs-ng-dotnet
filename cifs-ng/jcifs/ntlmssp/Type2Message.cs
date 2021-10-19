using jcifs.util;

using System;
using System.Collections.Generic;
using System.IO;
using cifs_ng.lib.ext;
using Logger = org.slf4j.Logger;
using LoggerFactory = org.slf4j.LoggerFactory;
using CIFSContext = jcifs.CIFSContext;

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
	/// Represents an NTLMSSP Type-2 message.
	/// </summary>
	public class Type2Message : NtlmMessage {

		private static readonly Logger log = LoggerFactory.getLogger(typeof(Type2Message));

		private byte[] challenge;
		private string target;
		private byte[] context;
		private byte[] targetInformation;

		private static readonly IDictionary<string, byte[]> TARGET_INFO_CACHE = new Dictionary<string, byte[]>();


		private static byte[] getDefaultTargetInfo(CIFSContext tc) {
			string domain = tc.getConfig().getDefaultDomain();
			byte[] ti = TARGET_INFO_CACHE.get(domain);
			if (ti != null) {
				return ti;
			}

			ti = makeTargetInfo(tc, domain);
			TARGET_INFO_CACHE[domain] = ti;
			return ti;
		}


		/// <param name="domain"> </param>
		/// <param name="domainLength"> </param>
		/// <param name="server">
		/// @return </param>
		private static byte[] makeTargetInfo(CIFSContext tc, string domainStr) {
			byte[] domain = new byte[0];
			if (domainStr != null) {
				try {
					domain = domainStr.getBytes(UNI_ENCODING);
				}
				catch (IOException ex) {
					log.debug("Failed to get domain bytes", ex);
				}
			}
			int domainLength = domain.Length;
			byte[] server = new byte[0];
			string host = tc.getNameServiceClient().getLocalHost().getHostName();
			if (host != null) {
				try {
					server = host.getBytes(UNI_ENCODING);
				}
				catch (IOException ex) {
					log.debug("Failed to get host bytes", ex);
				}
			}
			int serverLength = server.Length;
			byte[] targetInfo = new byte[(domainLength > 0 ? domainLength + 4 : 0) + (serverLength > 0 ? serverLength + 4 : 0) + 4];
			int offset = 0;
			if (domainLength > 0) {
				writeUShort(targetInfo, offset, 2);
				offset += 2;
				writeUShort(targetInfo, offset, domainLength);
				offset += 2;
				Array.Copy(domain, 0, targetInfo, offset, domainLength);
				offset += domainLength;
			}
			if (serverLength > 0) {
				writeUShort(targetInfo, offset, 1);
				offset += 2;
				writeUShort(targetInfo, offset, serverLength);
				offset += 2;
				Array.Copy(server, 0, targetInfo, offset, serverLength);
			}
			return targetInfo;
		}


		/// <summary>
		/// Creates a Type-2 message using default values from the current
		/// environment.
		/// </summary>
		/// <param name="tc">
		///            context to use </param>
		public Type2Message(CIFSContext tc) : this(tc, getDefaultFlags(tc), null, null) {
		}


		/// <summary>
		/// Creates a Type-2 message in response to the given Type-1 message
		/// using default values from the current environment.
		/// </summary>
		/// <param name="tc">
		///            context to use </param>
		/// <param name="type1">
		///            The Type-1 message which this represents a response to. </param>
		public Type2Message(CIFSContext tc, Type1Message type1) : this(tc, type1, null, null) {
		}


		/// <summary>
		/// Creates a Type-2 message in response to the given Type-1 message.
		/// </summary>
		/// <param name="tc">
		///            context to use </param>
		/// <param name="type1">
		///            The Type-1 message which this represents a response to. </param>
		/// <param name="challenge">
		///            The challenge from the domain controller/server. </param>
		/// <param name="target">
		///            The authentication target. </param>
		public Type2Message(CIFSContext tc, Type1Message type1, byte[] challenge, string target) : this(tc, getDefaultFlags(tc, type1), challenge, (type1 != null &&target == null && type1.getFlag(NtlmFlags.NTLMSSP_REQUEST_TARGET)) ? tc.getConfig().getDefaultDomain() : target) {
		}


		/// <summary>
		/// Creates a Type-2 message with the specified parameters.
		/// </summary>
		/// <param name="tc">
		///            context to use </param>
		/// <param name="flags">
		///            The flags to apply to this message. </param>
		/// <param name="challenge">
		///            The challenge from the domain controller/server. </param>
		/// <param name="target">
		///            The authentication target. </param>
		public Type2Message(CIFSContext tc, int flags, byte[] challenge, string target) {
			setFlags(flags);
			setChallenge(challenge);
			setTarget(target);
			if (target != null) {
				setTargetInformation(getDefaultTargetInfo(tc));
			}
		}


		/// <summary>
		/// Creates a Type-2 message using the given raw Type-2 material.
		/// </summary>
		/// <param name="material">
		///            The raw Type-2 material used to construct this message. </param>
		/// <exception cref="IOException">
		///             If an error occurs while parsing the material. </exception>
		/// throws java.io.IOException
		public Type2Message(byte[] material) {
			parse(material);
		}


		/// <summary>
		/// Returns the default flags for a generic Type-2 message in the
		/// current environment.
		/// </summary>
		/// <param name="tc">
		///            context to use </param>
		/// <returns> An <code>int</code> containing the default flags. </returns>
		public static int getDefaultFlags(CIFSContext tc) {
			return NtlmFlags.NTLMSSP_NEGOTIATE_NTLM | NtlmFlags.NTLMSSP_NEGOTIATE_VERSION | (tc.getConfig().isUseUnicode() ? NtlmFlags.NTLMSSP_NEGOTIATE_UNICODE : NtlmFlags.NTLMSSP_NEGOTIATE_OEM);
		}


		/// <summary>
		/// Returns the default flags for a Type-2 message created in response
		/// to the given Type-1 message in the current environment.
		/// </summary>
		/// <param name="tc">
		///            context to use </param>
		/// <param name="type1">
		///            request message
		/// </param>
		/// <returns> An <code>int</code> containing the default flags. </returns>
		public static int getDefaultFlags(CIFSContext tc, Type1Message type1) {
			if (type1 == null) {
				return getDefaultFlags(tc);
			}
			int flags = NtlmFlags.NTLMSSP_NEGOTIATE_NTLM | NtlmFlags.NTLMSSP_NEGOTIATE_VERSION;
			int type1Flags = type1.getFlags();
			flags |= ((type1Flags & NtlmFlags.NTLMSSP_NEGOTIATE_UNICODE) != 0) ? NtlmFlags.NTLMSSP_NEGOTIATE_UNICODE : NtlmFlags.NTLMSSP_NEGOTIATE_OEM;
			if ((type1Flags & NtlmFlags.NTLMSSP_REQUEST_TARGET) != 0) {
				string domain = tc.getConfig().getDefaultDomain();
				if (domain != null) {
					flags |= NtlmFlags.NTLMSSP_REQUEST_TARGET | NtlmFlags.NTLMSSP_TARGET_TYPE_DOMAIN;
				}
			}
			return flags;
		}


		/// <summary>
		/// Returns the challenge for this message.
		/// </summary>
		/// <returns> A <code>byte[]</code> containing the challenge. </returns>
		public virtual byte[] getChallenge() {
			return this.challenge;
		}


		/// <summary>
		/// Sets the challenge for this message.
		/// </summary>
		/// <param name="challenge">
		///            The challenge from the domain controller/server. </param>
		public virtual void setChallenge(byte[] challenge) {
			this.challenge = challenge;
		}


		/// <summary>
		/// Returns the authentication target.
		/// </summary>
		/// <returns> A <code>String</code> containing the authentication target. </returns>
		public virtual string getTarget() {
			return this.target;
		}


		/// <summary>
		/// Sets the authentication target.
		/// </summary>
		/// <param name="target">
		///            The authentication target. </param>
		public virtual void setTarget(string target) {
			this.target = target;
		}


		/// <summary>
		/// Returns the target information block.
		/// </summary>
		/// <returns> A <code>byte[]</code> containing the target information block.
		///         The target information block is used by the client to create an
		///         NTLMv2 response. </returns>
		public virtual byte[] getTargetInformation() {
			return this.targetInformation;
		}


		/// <summary>
		/// Sets the target information block.
		/// The target information block is used by the client to create
		/// an NTLMv2 response.
		/// </summary>
		/// <param name="targetInformation">
		///            The target information block. </param>
		public virtual void setTargetInformation(byte[] targetInformation) {
			this.targetInformation = targetInformation;
		}


		/// <summary>
		/// Returns the local security context.
		/// </summary>
		/// <returns> A <code>byte[]</code> containing the local security
		///         context. This is used by the client to negotiate local
		///         authentication. </returns>
		public virtual byte[] getContext() {
			return this.context;
		}


		/// <summary>
		/// Sets the local security context. This is used by the client
		/// to negotiate local authentication.
		/// </summary>
		/// <param name="context">
		///            The local security context. </param>
		public virtual void setContext(byte[] context) {
			this.context = context;
		}


		/// throws java.io.IOException
		public override byte[] toByteArray() {
			int size = 48;
			int flags = getFlags();
			string targetName = getTarget();
			byte[] targetInformationBytes = getTargetInformation();
			byte[] targetBytes = new byte[0];

			if (getFlag(NtlmFlags.NTLMSSP_REQUEST_TARGET)) {
				if (targetName != null && targetName.Length != 0) {
					targetBytes = (flags & NtlmFlags.NTLMSSP_NEGOTIATE_UNICODE) != 0 ? targetName.getBytes(UNI_ENCODING) : targetName.ToUpper().getBytes(getOEMEncoding());
					size += targetBytes.Length;
				}
				else {
					var x = unchecked((int) 0xffffffff);
					flags &= (x ^ NtlmFlags.NTLMSSP_REQUEST_TARGET);
				}
			}

			if (targetInformationBytes != null) {
				size += targetInformationBytes.Length;
				flags |= NtlmFlags.NTLMSSP_NEGOTIATE_TARGET_INFO;
			}

			if (getFlag(NtlmFlags.NTLMSSP_NEGOTIATE_VERSION)) {
				size += 8;
			}

			byte[] type2 = new byte[size];
			int pos = 0;

			Array.Copy(NTLMSSP_SIGNATURE, 0, type2, pos, NTLMSSP_SIGNATURE.Length);
			pos += NTLMSSP_SIGNATURE.Length;

			writeULong(type2, pos, NTLMSSP_TYPE2);
			pos += 4;

			// TargetNameFields
			int targetNameOff = writeSecurityBuffer(type2, pos, targetBytes);
			pos += 8;

			writeULong(type2, pos, flags);
			pos += 4;

			// ServerChallenge
			byte[] challengeBytes = getChallenge();
			Array.Copy(challengeBytes != null ? challengeBytes : new byte[8], 0, type2, pos, 8);
			pos += 8;

			// Reserved
			byte[] contextBytes = getContext();
			Array.Copy(contextBytes != null ? contextBytes : new byte[8], 0, type2, pos, 8);
			pos += 8;

			// TargetInfoFields
			int targetInfoOff = writeSecurityBuffer(type2, pos, targetInformationBytes);
			pos += 8;

			if (getFlag(NtlmFlags.NTLMSSP_NEGOTIATE_VERSION)) {
				Array.Copy(NTLMSSP_VERSION, 0, type2, pos, NTLMSSP_VERSION.Length);
				pos += NTLMSSP_VERSION.Length;
			}

			pos += writeSecurityBufferContent(type2, pos, targetNameOff, targetBytes);
			pos += writeSecurityBufferContent(type2, pos, targetInfoOff, targetInformationBytes);

			return type2;
		}


		public override string ToString() {
			string targetString = getTarget();
			byte[] challengeBytes = getChallenge();
			byte[] contextBytes = getContext();
			byte[] targetInformationBytes = getTargetInformation();

			return "Type2Message[target=" + targetString + ",challenge=" + (challengeBytes == null ? "null" : "<" + challengeBytes.Length + " bytes>") + ",context=" + (contextBytes == null ? "null" : "<" + contextBytes.Length + " bytes>") + ",targetInformation=" + (targetInformationBytes == null ? "null" : "<" + targetInformationBytes.Length + " bytes>") + ",flags=0x" + Hexdump.toHexString(getFlags(), 8) + "]";
		}


		/// throws java.io.IOException
		private void parse(byte[] input) {
			int pos = 0;
			for (int i = 0; i < 8; i++) {
				if (input[i] != NTLMSSP_SIGNATURE[i]) {
					throw new IOException("Not an NTLMSSP message.");
				}
			}
			pos += 8;

			if (readULong(input, pos) != NTLMSSP_TYPE2) {
				throw new IOException("Not a Type 2 message.");
			}
			pos += 4;

			int flags = readULong(input, pos + 8);
			setFlags(flags);

			byte[] targetName = readSecurityBuffer(input, pos);
			int targetNameOff = readULong(input, pos + 4);
			if (targetName.Length != 0) {
				setTarget(targetName.toString((flags & NtlmFlags.NTLMSSP_NEGOTIATE_UNICODE) != 0 ? UNI_ENCODING : getOEMEncoding()));
			}
			pos += 12; // 8 for target, 4 for flags

			if (!allZeros8(input, pos)) {
				byte[] challengeBytes = new byte[8];
				Array.Copy(input, pos, challengeBytes, 0, challengeBytes.Length);
				setChallenge(challengeBytes);
			}
			pos += 8;

			if (targetNameOff < pos + 8 || input.Length < pos + 8) {
				// no room for Context/Reserved
				return;
			}

			if (!allZeros8(input, pos)) {
				byte[] contextBytes = new byte[8];
				Array.Copy(input, pos, contextBytes, 0, contextBytes.Length);
				setContext(contextBytes);
			}
			pos += 8;

			if (targetNameOff < pos + 8 || input.Length < pos + 8) {
				// no room for target info
				return;
			}

			byte[] targetInfo = readSecurityBuffer(input, pos);
			if (targetInfo.Length != 0) {
				setTargetInformation(targetInfo);
			}
		}


		private static bool allZeros8(byte[] input, int pos) {
			for (int i = pos; i < pos + 8; i++) {
				if (input[i] != 0) {
					return false;
				}
			}
			return true;
		}

	}

}