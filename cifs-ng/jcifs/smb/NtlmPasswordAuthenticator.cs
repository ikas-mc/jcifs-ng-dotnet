using jcifs;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using cifs_ng.lib;
using cifs_ng.lib.ext;
using cifs_ng.lib.security;
using jcifs.lib;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Security;
using Logger = org.slf4j.Logger;
using LoggerFactory = org.slf4j.LoggerFactory;
using CIFSContext = jcifs.CIFSContext;
using CIFSException = jcifs.CIFSException;
using Credentials = jcifs.Credentials;
using RuntimeCIFSException = jcifs.RuntimeCIFSException;
using NegTokenInit = jcifs.spnego.NegTokenInit;
using Crypto = jcifs.util.Crypto;
using Strings = jcifs.util.Strings;

/*
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
	/// This class stores and encrypts NTLM user credentials.
	/// 
	/// Contrary to <seealso cref="NtlmPasswordAuthentication"/> this does not cause guest authentication
	/// when the "guest" username is supplied. Use <seealso cref="AuthenticationType"/> instead.
	/// 
	/// @author mbechler
	/// </summary>
	[Serializable]
	public class NtlmPasswordAuthenticator : Principal, CredentialsInternal {

		/// 
		private const long serialVersionUID = -4090263879887877186L;

		private static readonly Logger log = LoggerFactory.getLogger(typeof(NtlmPasswordAuthenticator));

		private AuthenticationType type;
		private string domain;
		private string username;
		private string password;
		private byte[] clientChallenge = null;


		/// <summary>
		/// Construct anonymous credentials
		/// </summary>
		public NtlmPasswordAuthenticator() : this(AuthenticationType.NULL) {
		}


		public NtlmPasswordAuthenticator(AuthenticationType type) {
			this.domain = "";
			this.username = "";
			this.password = "";
			this.type = type;
		}


		/// <summary>
		/// Create username/password credentials
		/// </summary>
		/// <param name="username"> </param>
		/// <param name="password"> </param>
		public NtlmPasswordAuthenticator(string username, string password) : this(null, username, password) {
		}


		/// <summary>
		/// Create username/password credentials with specified domain
		/// </summary>
		/// <param name="domain"> </param>
		/// <param name="username"> </param>
		/// <param name="password"> </param>
		public NtlmPasswordAuthenticator(string domain, string username, string password) : this(domain, username, password, AuthenticationType.USER) {
		}


		/// <summary>
		/// Create username/password credentials with specified domain
		/// </summary>
		/// <param name="domain"> </param>
		/// <param name="username"> </param>
		/// <param name="password"> </param>
		/// <param name="type">
		///            authentication type </param>
		public NtlmPasswordAuthenticator(string domain, string username, string password, AuthenticationType type) {
			if (username != null) {
				int ci = username.IndexOf('@');
				if (ci > 0) {
					domain = username.Substring(ci + 1);
					username = username.Substring(0, ci);
				}
				else {
					ci = username.IndexOf('\\');
					if (ci > 0) {
						domain = username.Substring(0, ci);
						username = username.Substring(ci + 1);
					}
				}
			}

			this.domain = domain ?? "";
			this.username = username ?? "";
			this.password = password ?? "";
			if (type == AuthenticationType.NULL) {
				this.type = guessAuthenticationType();
			}
			else {
				this.type = type;
			}
		}


		protected internal NtlmPasswordAuthenticator(string userInfo, string defDomain, string defUser, string defPassword) : this(userInfo, defDomain, defUser, defPassword, AuthenticationType.NULL) {
		}


		/// <param name="userInfo"> </param>
		protected internal NtlmPasswordAuthenticator(string userInfo, string defDomain, string defUser, string defPassword, AuthenticationType type) {
			string dom = null, user = null, pass = null;
			if (userInfo != null) {
				try {
					userInfo = unescape(userInfo);
				}
				catch (Exception uee) {
					throw new RuntimeCIFSException(uee);
				}
				int i, u;
				int end = userInfo.Length;
				for (i = 0, u = 0; i < end; i++) {
					char c = userInfo[i];
					if (c == ';') {
						dom = userInfo.Substring(0, i);
						u = i + 1;
					}
					else if (c == ':') {
						pass = userInfo.Substring(i + 1);
						break;
					}
				}
				user = userInfo.Substring(u, i - u);
			}

			this.domain = dom ?? (defDomain ?? "");
			this.username = user ?? (defUser ?? "");
			this.password = pass ?? (defPassword ?? "");

			if (type == AuthenticationType.NULL) {
				this.type = guessAuthenticationType();
			}
			else {
				this.type = type;
			}
		}


		/// <summary>
		/// @return
		/// </summary>
		protected internal virtual AuthenticationType guessAuthenticationType() {
			AuthenticationType t = AuthenticationType.USER;
			if ("guest".Equals(this.username, StringComparison.OrdinalIgnoreCase)) {
				t = AuthenticationType.GUEST;
			}
			else if ((getUserDomain()==null || getUserDomain().Length == 0) && getUsername().Length == 0 && (getPassword().Length == 0)) {
				t = AuthenticationType.NULL;
			}
			return t;
		}


		public virtual T unwrap<T>(Type t) {
			if (this is T v) {
				return v;
			}
			return default;
		}


		public virtual Subject getSubject() {
			return null;
		}


		/// throws jcifs.CIFSException
		public virtual void refresh() {
		}
		/// throws SmbException
		public virtual SSPContext createContext(CIFSContext tc, string targetDomain, string host, byte[] initialToken, bool doSigning) {
			if (tc.getConfig().isUseRawNTLM()) {
				return setupTargetName(tc, host, new NtlmContext(tc, this, doSigning));
			}

			try {
				if (initialToken != null && initialToken.Length > 0) {
					NegTokenInit tok = new NegTokenInit(initialToken);
					if (log.isDebugEnabled()) {
						log.debug("Have initial token " + tok);
					}
					if (tok.getMechanisms() != null)
					{
						ISet<DerObjectIdentifier> mechs = tok.getMechanisms().ToHashSet();
						if (!mechs.Contains(NtlmContext.NTLMSSP_OID)) {
							throw new SmbUnsupportedOperationException("Server does not support NTLM authentication");
						}
					}
				}
			}
			catch (SmbException e) {
				throw e;
			}
			catch (IOException e1) {
				log.debug("Ignoring invalid initial token", e1);
			}

			return new SpnegoContext(tc.getConfig(), setupTargetName(tc, host, new NtlmContext(tc, this, doSigning)));
		}


		private static SSPContext setupTargetName(CIFSContext tc, string host, NtlmContext ntlmContext) {
			if (host != null && tc.getConfig().isSendNTLMTargetName()) {
				ntlmContext.setTargetName(string.Format("cifs/{0}", host));
			}
			return ntlmContext;
		}


		public virtual object Clone() {
			NtlmPasswordAuthenticator cloned = new NtlmPasswordAuthenticator();
			cloneInternal(cloned, this);
			return cloned;
		}


		protected internal static void cloneInternal(NtlmPasswordAuthenticator cloned, NtlmPasswordAuthenticator toClone) {
			cloned.domain = toClone.domain;
			cloned.username = toClone.username;
			cloned.password = toClone.password;
			cloned.type = toClone.type;
		}


		/// <summary>
		/// Returns the domain.
		/// </summary>
		public virtual string getUserDomain() {
			return this.domain;
		}


		/// 
		/// <returns> the original specified user domain </returns>
		public virtual string getSpecifiedUserDomain() {
			return this.domain;
		}


		/// <summary>
		/// Returns the username.
		/// </summary>
		/// <returns> the username </returns>
		public virtual string getUsername() {
			return this.username;
		}


		/// <summary>
		/// Returns the password in plain text or <tt>null</tt> if the raw password
		/// hashes were used to construct this <tt>NtlmPasswordAuthentication</tt>
		/// object which will be the case when NTLM HTTP Authentication is
		/// used. There is no way to retrieve a users password in plain text unless
		/// it is supplied by the user at runtime.
		/// </summary>
		/// <returns> the password </returns>
		public virtual string getPassword() {
			return this.password;
		}


		/// <summary>
		/// Return the domain and username in the format:
		/// <tt>domain\\username</tt>. This is equivalent to <tt>toString()</tt>.
		/// </summary>
		public  string getName() {
			bool d = this.domain!=null && this.domain.Length > 0;
			return d ? this.domain + "\\" + this.username : this.username;
		}


		/// <summary>
		/// Compares two <tt>NtlmPasswordAuthentication</tt> objects for equality.
		/// 
		/// Two <tt>NtlmPasswordAuthentication</tt> objects are equal if their caseless domain and username fields are equal
		/// </summary>
		/// <seealso cref= java.lang.Object#equals(java.lang.Object) </seealso>
		public override bool Equals(object obj) {
			if (obj is NtlmPasswordAuthenticator) {
				NtlmPasswordAuthenticator ntlm = (NtlmPasswordAuthenticator) obj;
				string domA = ntlm.getUserDomain()!=null ? ntlm.getUserDomain().ToUpper() : null;
				string domB = this.getUserDomain()!=null ? this.getUserDomain().ToUpper() : null;
				return ntlm.type == this.type && Equals(domA, domB) && ntlm.getUsername().Equals(this.getUsername(), StringComparison.OrdinalIgnoreCase) && Equals(getPassword(), ntlm.getPassword());
			}
			return false;
		}


		/// <summary>
		/// Return the upcased username hash code.
		/// </summary>
		public override int GetHashCode() {
			return getName().ToUpper().GetHashCode();
		}


		/// <summary>
		/// Return the domain and username in the format:
		/// <tt>domain\\username</tt>. This is equivalent to <tt>getName()</tt>.
		/// </summary>
		public override string ToString() {
			return getName();
		}


		public virtual bool isAnonymous() {
			return this.type == AuthenticationType.NULL;
		}


		public virtual bool isGuest() {
			return this.type == AuthenticationType.GUEST;
		}


		/// throws NumberFormatException, java.io.UnsupportedEncodingException
		internal static string unescape(string str) {
			char ch;
			int i, j, state, len;
			char[] @out;
			byte[] b = new byte[1];

			if (str == null) {
				return null;
			}

			len = str.Length;
			@out = new char[len];
			state = 0;
			for (i = j = 0; i < len; i++) {
				switch (state) {
				case 0:
					ch = str[i];
					if (ch == '%') {
						state = 1;
					}
					else {
						@out[j++] = ch;
					}
					break;
				case 1:
					/*
					 * Get ASCII hex value and convert to platform dependent
					 * encoding like EBCDIC perhaps
					 */
					b[0] = unchecked((byte)(Convert.ToInt32(str.Substring(i, 2), 16) & 0xFF));
					@out[j++] = b.toString(0, 1, Strings.ASCII_ENCODING)[0];
					i++;
					state = 0;
				break;
				}
			}

			return new string(@out, 0, j);
		}


		/// <param name="mechanism"> </param>
		/// <returns> whether the given mechanism is the preferred one for this credential </returns>
		public virtual bool isPreferredMech(DerObjectIdentifier mechanism) {
			return NtlmContext.NTLMSSP_OID.Equals(mechanism);
		}


		/// <summary>
		/// Computes the 24 byte ANSI password hash given the 8 byte server challenge.
		/// </summary>
		/// <param name="tc"> </param>
		/// <param name="chlng"> </param>
		/// <returns> the hash for the given challenge </returns>
		/// <exception cref="GeneralSecurityException"> </exception>
		/// throws java.security.GeneralSecurityException
		public virtual byte[] getAnsiHash(CIFSContext tc, byte[] chlng) {
			switch (tc.getConfig().getLanManCompatibility()) {
			case 0:
			case 1:
				return NtlmUtil.getPreNTLMResponse(tc, this.password, chlng);
			case 2:
				return NtlmUtil.getNTLMResponse(this.password, chlng);
			case 3:
			case 4:
			case 5:
				if (this.clientChallenge == null) {
					this.clientChallenge = new byte[8];
					tc.getConfig().getRandom().NextBytes(this.clientChallenge);
				}
				return NtlmUtil.getLMv2Response(this.domain, this.username, this.password, chlng, this.clientChallenge);
			default:
				return NtlmUtil.getPreNTLMResponse(tc, this.password, chlng);
			}
		}


		/// <summary>
		/// Computes the 24 byte Unicode password hash given the 8 byte server challenge.
		/// </summary>
		/// <param name="tc"> </param>
		/// <param name="chlng"> </param>
		/// <returns> the hash for the given challenge </returns>
		/// <exception cref="GeneralSecurityException"> </exception>
		/// throws java.security.GeneralSecurityException
		public virtual byte[] getUnicodeHash(CIFSContext tc, byte[] chlng) {
			switch (tc.getConfig().getLanManCompatibility()) {
			case 0:
			case 1:
			case 2:
				return NtlmUtil.getNTLMResponse(this.password, chlng);
			case 3:
			case 4:
			case 5:
				return new byte[0];
			default:
				return NtlmUtil.getNTLMResponse(this.password, chlng);
			}
		}


		/// <param name="tc"> </param>
		/// <param name="chlng"> </param>
		/// <returns> the signing key </returns>
		/// <exception cref="SmbException"> </exception>
		/// <exception cref="GeneralSecurityException"> </exception>
		/// throws SmbException, java.security.GeneralSecurityException
		public virtual byte[] getSigningKey(CIFSContext tc, byte[] chlng) {
			switch (tc.getConfig().getLanManCompatibility()) {
			case 0:
			case 1:
			case 2:
				byte[] signingKey = new byte[40];
				getUserSessionKey(tc, chlng, signingKey, 0);
				Array.Copy(getUnicodeHash(tc, chlng), 0, signingKey, 16, 24);
				return signingKey;
			case 3:
			case 4:
			case 5:
				/*
				 * This code is only called if extended security is not on. This will
				 * all be cleaned up an normalized in JCIFS 2.x.
				 */
				throw new SmbException("NTLMv2 requires extended security (jcifs.smb.client.useExtendedSecurity must be true if jcifs.smb.lmCompatibility >= 3)");
			}
			return null;
		}


		/// <summary>
		/// Returns the effective user session key.
		/// </summary>
		/// <param name="tc"> </param>
		/// <param name="chlng">
		///            The server challenge. </param>
		/// <returns> A <code>byte[]</code> containing the effective user session key,
		///         used in SMB MAC signing and NTLMSSP signing and sealing. </returns>
		public virtual byte[] getUserSessionKey(CIFSContext tc, byte[] chlng) {
			byte[] key = new byte[16];
			try {
				getUserSessionKey(tc, chlng, key, 0);
			}
			catch (Exception ex) {
				log.error("Failed to get session key", ex);
			}
			return key;
		}


		/// <summary>
		/// Calculates the effective user session key.
		/// </summary>
		/// <param name="tc">
		///            context to use </param>
		/// <param name="chlng">
		///            The server challenge. </param>
		/// <param name="dest">
		///            The destination array in which the user session key will be
		///            placed. </param>
		/// <param name="offset">
		///            The offset in the destination array at which the
		///            session key will start. </param>
		/// <exception cref="SmbException"> </exception>
		/// throws SmbException
		public virtual void getUserSessionKey(CIFSContext tc, byte[] chlng, byte[] dest, int offset) {
			try {
				MessageDigest md4 = Crypto.getMD4();
				byte[] ntHash = getNTHash();
				switch (tc.getConfig().getLanManCompatibility()) {
				case 0:
				case 1:
				case 2:
					md4.update(ntHash);
					md4.update(dest, offset, 16);
					break;
				case 3:
				case 4:
				case 5:
					lock (this) {
						if (this.clientChallenge == null) {
							this.clientChallenge = new byte[8];
							tc.getConfig().getRandom().NextBytes(this.clientChallenge);
						}
					}

					MessageDigest hmac = Crypto.getHMACT64(ntHash);
					hmac.update(Strings.getUNIBytes(this.username.ToUpper()));
					hmac.update(Strings.getUNIBytes(this.domain.ToUpper()));
					byte[] ntlmv2Hash = hmac.digest();
					hmac = Crypto.getHMACT64(ntlmv2Hash);
					hmac.update(chlng);
					hmac.update(this.clientChallenge);
					MessageDigest userKey = Crypto.getHMACT64(ntlmv2Hash);
					userKey.update(hmac.digest());
					userKey.digest(dest, offset, 16);
					break;
				default:
					md4.update(ntHash);
					md4.digest(dest, offset, 16);
					break;
				}
			}
			catch (Exception e) {
				throw new SmbException("", e);
			}
		}


		/// <summary>
		/// @return
		/// </summary>
		protected internal virtual byte[] getNTHash() {
			MessageDigest md4 = Crypto.getMD4();
			md4.update(Strings.getUNIBytes(this.password));
			byte[] ntHash = md4.digest();
			return ntHash;
		}

		/// <summary>
		/// Authentication strategy
		/// 
		/// 
		/// </summary>
		public enum AuthenticationType {
			/// <summary>
			/// Null/anonymous authentication
			/// 
			/// Login with no credentials
			/// </summary>
			NULL,
			/// <summary>
			/// Guest authentication
			/// 
			/// Allows login with invalid credentials (username and/or password)
			/// Fallback to anonymous authentication is permitted
			/// </summary>
			GUEST,
			/// <summary>
			/// Regular user authentication
			/// </summary>
			USER
		}
	}

}