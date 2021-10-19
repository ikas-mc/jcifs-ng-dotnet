using System;
using cifs_ng.lib;
using cifs_ng.lib.security;
using jcifs.lib;
using CIFSContext = jcifs.CIFSContext;
using Crypto = jcifs.util.Crypto;
using Encdec = jcifs.util.Encdec;
using Strings = jcifs.util.Strings;

/*
 * © 2016 AgNO3 Gmbh & Co. KG
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
	/// Internal use only
	/// 
	/// @author mbechler
	/// @internal
	/// </summary>
	public sealed class NtlmUtil {

		/// 
		private NtlmUtil() {
		}


		/// 
		/// <param name="responseKeyNT"> </param>
		/// <param name="serverChallenge"> </param>
		/// <param name="clientChallenge"> </param>
		/// <param name="nanos1601"> </param>
		/// <param name="avPairs"> </param>
		/// <returns> the calculated response </returns>
		public static byte[] getNTLMv2Response(byte[] responseKeyNT, byte[] serverChallenge, byte[] clientChallenge, long nanos1601, byte[] avPairs) {
			int avPairsLength = avPairs != null ? avPairs.Length : 0;
			byte[] temp = new byte[28 + avPairsLength + 4];

			Encdec.enc_uint32le(0x00000101, temp, 0); // Header
			Encdec.enc_uint32le(0x00000000, temp, 4); // Reserved
			Encdec.enc_uint64le(nanos1601, temp, 8);
			Array.Copy(clientChallenge, 0, temp, 16, 8);
			Encdec.enc_uint32le(0x00000000, temp, 24); // Unknown
			if (avPairs != null) {
				Array.Copy(avPairs, 0, temp, 28, avPairsLength);
			}
			Encdec.enc_uint32le(0x00000000, temp, 28 + avPairsLength); // mystery bytes!

			return NtlmUtil.computeResponse(responseKeyNT, serverChallenge, temp, 0, temp.Length);
		}


		/// 
		/// <param name="responseKeyLM"> </param>
		/// <param name="serverChallenge"> </param>
		/// <param name="clientChallenge"> </param>
		/// <returns> the calculated response </returns>
		public static byte[] getLMv2Response(byte[] responseKeyLM, byte[] serverChallenge, byte[] clientChallenge) {
			return NtlmUtil.computeResponse(responseKeyLM, serverChallenge, clientChallenge, 0, clientChallenge.Length);
		}


		internal static byte[] computeResponse(byte[] responseKey, byte[] serverChallenge, byte[] clientData, int offset, int length) {
			MessageDigest hmac = Crypto.getHMACT64(responseKey);
			hmac.update(serverChallenge);
			hmac.update(clientData, offset, length);
			byte[] mac = hmac.digest();
			byte[] ret = new byte[mac.Length + clientData.Length];
			Array.Copy(mac, 0, ret, 0, mac.Length);
			Array.Copy(clientData, 0, ret, mac.Length, clientData.Length);
			return ret;
		}


		/// 
		/// <param name="domain"> </param>
		/// <param name="username"> </param>
		/// <param name="password">
		/// </param>
		/// <returns> the caclulated mac </returns>
		public static byte[] nTOWFv2(string domain, string username, string password) {
			return nTOWFv2(domain, username, getNTHash(password));
		}


		/// 
		/// <param name="domain"> </param>
		/// <param name="username"> </param>
		/// <param name="passwordHash">
		///            NT password hash
		/// </param>
		/// <returns> the caclulated mac </returns>
		public static byte[] nTOWFv2(string domain, string username, byte[] passwordHash) {
			MessageDigest hmac = Crypto.getHMACT64(passwordHash);
			hmac.update(Strings.getUNIBytes(username.ToUpper()));
			hmac.update(Strings.getUNIBytes(domain));
			return hmac.digest();
		}


		/// <param name="password"> </param>
		/// <returns> nt password hash </returns>
		public static byte[] getNTHash(string password) {
			if (password == null) {
				throw new System.NullReferenceException("Password parameter is required");
			}
			MessageDigest md4 = Crypto.getMD4();
			md4.update(Strings.getUNIBytes(password));
			return md4.digest();
		}


		/// 
		/// <param name="password"> </param>
		/// <returns> the calculated hash </returns>
		public static byte[] nTOWFv1(string password) {
			return getNTHash(password);
		}


		/// 
		/// <param name="passwordHash"> </param>
		/// <param name="serverChallenge"> </param>
		/// <param name="clientChallenge"> </param>
		/// <returns> the calculated response </returns>
		/// <exception cref="GeneralSecurityException"> </exception>
		/// throws java.security.GeneralSecurityException
		public static byte[] getNTLM2Response(byte[] passwordHash, byte[] serverChallenge, byte[] clientChallenge) {
			byte[] sessionHash = new byte[8];

			MessageDigest md5 = Crypto.getMD5();
			md5.update(serverChallenge);
			md5.update(clientChallenge, 0, 8);
			Array.Copy(md5.digest(), 0, sessionHash, 0, 8);

			byte[] key = new byte[21];
			Array.Copy(passwordHash, 0, key, 0, 16);
			byte[] ntResponse = new byte[24];
			NtlmUtil.E(key, sessionHash, ntResponse);
			return ntResponse;
		}


		/// <summary>
		/// Creates the LMv2 response for the supplied information.
		/// </summary>
		/// <param name="domain">
		///            The domain in which the username exists. </param>
		/// <param name="user">
		///            The username. </param>
		/// <param name="password">
		///            The user's password. </param>
		/// <param name="challenge">
		///            The server challenge. </param>
		/// <param name="clientChallenge">
		///            The client challenge (nonce). </param>
		/// <returns> the calculated response </returns>
		/// <exception cref="GeneralSecurityException"> </exception>
		/// throws java.security.GeneralSecurityException
		public static byte[] getLMv2Response(string domain, string user, string password, byte[] challenge, byte[] clientChallenge) {
			return getLMv2Response(domain, user, getNTHash(password), challenge, clientChallenge);
		}


		/// <summary>
		/// Creates the LMv2 response for the supplied information.
		/// </summary>
		/// <param name="domain">
		///            The domain in which the username exists. </param>
		/// <param name="user">
		///            The username. </param>
		/// <param name="passwordHash">
		///            The user's NT hash. </param>
		/// <param name="challenge">
		///            The server challenge. </param>
		/// <param name="clientChallenge">
		///            The client challenge (nonce). </param>
		/// <returns> the calculated response </returns>
		/// <exception cref="GeneralSecurityException"> </exception>
		/// throws java.security.GeneralSecurityException
		public static byte[] getLMv2Response(string domain, string user, byte[] passwordHash, byte[] challenge, byte[] clientChallenge) {
			byte[] response = new byte[24];
			MessageDigest hmac = Crypto.getHMACT64(passwordHash);
			hmac.update(Strings.getUNIBytes(user.ToUpper()));
			hmac.update(Strings.getUNIBytes(domain.ToUpper()));
			hmac = Crypto.getHMACT64(hmac.digest());
			hmac.update(challenge);
			hmac.update(clientChallenge);
			hmac.digest(response, 0, 16);
			Array.Copy(clientChallenge, 0, response, 16, 8);
			return response;
		}


		/// <summary>
		/// Generate the Unicode MD4 hash for the password associated with these credentials.
		/// </summary>
		/// <param name="password"> </param>
		/// <param name="challenge"> </param>
		/// <returns> the calculated response </returns>
		/// <exception cref="GeneralSecurityException"> </exception>
		/// throws java.security.GeneralSecurityException
		public static byte[] getNTLMResponse(string password, byte[] challenge) {
			return getNTLMResponse(getNTHash(password), challenge);
		}


		/// <summary>
		/// Generate the Unicode MD4 hash for the password associated with these credentials.
		/// </summary>
		/// <param name="passwordHash">
		///            NT Hash </param>
		/// <param name="challenge"> </param>
		/// <returns> the calculated response </returns>
		/// <exception cref="GeneralSecurityException"> </exception>
		/// throws java.security.GeneralSecurityException
		public static byte[] getNTLMResponse(byte[] passwordHash, byte[] challenge) {
			byte[] p21 = new byte[21];
			byte[] p24 = new byte[24];
			Array.Copy(passwordHash, 0, p21, 0, 16);
			NtlmUtil.E(p21, challenge, p24);
			return p24;
		}


		/// <summary>
		/// Generate the ANSI DES hash for the password associated with these credentials.
		/// </summary>
		/// <param name="tc"> </param>
		/// <param name="password"> </param>
		/// <param name="challenge"> </param>
		/// <returns> the calculated response </returns>
		/// <exception cref="GeneralSecurityException"> </exception>
		/// throws java.security.GeneralSecurityException
		public static byte[] getPreNTLMResponse(CIFSContext tc, string password, byte[] challenge) {
			byte[] p14 = new byte[14];
			byte[] p21 = new byte[21];
			byte[] p24 = new byte[24];
			byte[] passwordBytes = Strings.getOEMBytes(password, tc.getConfig());
			int passwordLength = passwordBytes.Length;

			// Only encrypt the first 14 bytes of the password for Pre 0.12 NT LM
			if (passwordLength > 14) {
				passwordLength = 14;
			}
			Array.Copy(passwordBytes, 0, p14, 0, passwordLength);
			NtlmUtil.E(p14, NtlmUtil.S8, p21);
			NtlmUtil.E(p21, challenge, p24);
			return p24;
		}

		// KGS!@#$%
		internal static readonly byte[] S8 = new byte[] {(byte) 0x4b, (byte) 0x47, (byte) 0x53, (byte) 0x21, (byte) 0x40, (byte) 0x23, (byte) 0x24, (byte) 0x25};


		/*
		 * Accepts key multiple of 7
		 * Returns enc multiple of 8
		 * Multiple is the same like: 21 byte key gives 24 byte result
		 */
		/// throws javax.crypto.ShortBufferException
		internal static void E(byte[] key, byte[] data, byte[] e) {
			byte[] key7 = new byte[7];
			byte[] e8 = new byte[8];

			for (int i = 0; i < key.Length / 7; i++) {
				Array.Copy(key, i * 7, key7, 0, 7);
				//TODO 
				var des = Crypto.getDES(key7);
				des.update(data,0,data.Length, e8);
				Array.Copy(e8, 0, e, i * 8, 8);
			}
		}

	}

}