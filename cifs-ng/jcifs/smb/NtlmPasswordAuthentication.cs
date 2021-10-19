using System;
using System.Linq;
using cifs_ng.lib.ext;
using CIFSContext = jcifs.CIFSContext;

/* jcifs smb client library in Java
 * Copyright (C) 2002  "Michael B. Allen" <jcifs at samba dot org>
 *                  "Eric Glass" <jcifs at samba dot org>
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
	/// This class stores and encrypts NTLM user credentials. The default
	/// credentials are retrieved from the <tt>jcifs.smb.client.domain</tt>,
	/// <tt>jcifs.smb.client.username</tt>, and <tt>jcifs.smb.client.password</tt>
	/// properties.
	/// <para>
	/// Read <a href="../../../authhandler.html">jCIFS Exceptions and
	/// NtlmAuthenticator</a> for related information.
	/// 
	/// </para>
	/// </summary>
	/// @deprecated use <seealso cref="NtlmPasswordAuthenticator"/> instead 
	[Obsolete("use <seealso cref=\"NtlmPasswordAuthenticator\"/> instead"), Serializable]
	public class NtlmPasswordAuthentication : NtlmPasswordAuthenticator {

		/// 
		private const long serialVersionUID = -2832037191318016836L;

		private byte[] ansiHash;
		private byte[] unicodeHash;
		private bool hashesExternal = false;
		private CIFSContext context;


		/// 
		private NtlmPasswordAuthentication() {
		}


		/// <summary>
		/// Construct anonymous credentials
		/// </summary>
		/// <param name="tc"> </param>
		public NtlmPasswordAuthentication(CIFSContext tc) : this(tc, "", "", "") {
		}


		/// <summary>
		/// Create an <tt>NtlmPasswordAuthentication</tt> object from the userinfo
		/// component of an SMB URL like "<tt>domain;user:pass</tt>". This constructor
		/// is used internally be jCIFS when parsing SMB URLs.
		/// </summary>
		/// <param name="tc"> </param>
		/// <param name="userInfo"> </param>
		public NtlmPasswordAuthentication(CIFSContext tc, string userInfo) : base(userInfo, tc.getConfig().getDefaultDomain(), tc.getConfig().getDefaultUsername()!=null ? tc.getConfig().getDefaultUsername() : "GUEST", tc.getConfig().getDefaultPassword()!=null ? tc.getConfig().getDefaultPassword() : "") {
			this.context = tc;
		}


		/// <summary>
		/// Create an <tt>NtlmPasswordAuthentication</tt> object from a
		/// domain, username, and password. Parameters that are <tt>null</tt>
		/// will be substituted with <tt>jcifs.smb.client.domain</tt>,
		/// <tt>jcifs.smb.client.username</tt>, <tt>jcifs.smb.client.password</tt>
		/// property values.
		/// </summary>
		/// <param name="tc">
		///            context to use </param>
		/// <param name="domain"> </param>
		/// <param name="username"> </param>
		/// <param name="password"> </param>
		public NtlmPasswordAuthentication(CIFSContext tc, string domain, string username, string password) : base(domain != null ? domain : tc.getConfig().getDefaultDomain(), username != null ? username : (tc.getConfig().getDefaultUsername()!=null ? tc.getConfig().getDefaultUsername() : "GUEST"), password != null ? password : (tc.getConfig().getDefaultPassword()!=null ? tc.getConfig().getDefaultPassword() : ""), AuthenticationType.NULL) {
			this.context = tc;
		}


		/// <summary>
		/// Create an <tt>NtlmPasswordAuthentication</tt> object with raw password
		/// hashes. This is used exclusively by the <tt>jcifs.http.NtlmSsp</tt>
		/// class which is in turn used by NTLM HTTP authentication functionality.
		/// </summary>
		/// <param name="domain"> </param>
		/// <param name="username"> </param>
		/// <param name="challenge"> </param>
		/// <param name="ansiHash"> </param>
		/// <param name="unicodeHash"> </param>
		public NtlmPasswordAuthentication(string domain, string username, byte[] challenge, byte[] ansiHash, byte[] unicodeHash) : base(domain, username, null) {
			if (domain == null ||username == null || ansiHash == null || unicodeHash == null) {
				throw new System.ArgumentException("External credentials cannot be null");
			}
			this.ansiHash = ansiHash;
			this.unicodeHash = unicodeHash;
			this.hashesExternal = true;
		}


		protected internal virtual CIFSContext getContext() {
			return this.context;
		}


		public override object Clone() {
			NtlmPasswordAuthentication cloned = new NtlmPasswordAuthentication();
			cloneInternal(cloned, this);
			return cloned;
		}


		/// <param name="to"> </param>
		/// <param name="from"> </param>
		protected internal static void cloneInternal(NtlmPasswordAuthentication to, NtlmPasswordAuthentication from) {
			to.context = from.context;
			if (from.hashesExternal) {
				to.hashesExternal = true;
				to.ansiHash = from.ansiHash?.sub(0,from.ansiHash.Length);
				to.unicodeHash = from.unicodeHash?.sub(0, from.unicodeHash.Length);
			}
			else {
				NtlmPasswordAuthenticator.cloneInternal(to, from);
			}
		}


		/// <summary>
		/// Compares two <tt>NtlmPasswordAuthentication</tt> objects for
		/// equality. Two <tt>NtlmPasswordAuthentication</tt> objects are equal if
		/// their caseless domain and username fields are equal and either both hashes are external and they are equal or
		/// both internally supplied passwords are equal. If one <tt>NtlmPasswordAuthentication</tt> object has external
		/// hashes (meaning negotiated via NTLM HTTP Authentication) and the other does not they will not be equal. This is
		/// technically not correct however the server 8 byte challenge would be required to compute and compare the password
		/// hashes but that it not available with this method.
		/// </summary>
		public override bool Equals(object obj) {
			if (base.Equals(obj)) {
				if (!(obj is NtlmPasswordAuthentication)) {
					return !this.areHashesExternal();
				}
				NtlmPasswordAuthentication ntlm = (NtlmPasswordAuthentication) obj;
				if (this.areHashesExternal() && ntlm.areHashesExternal()) {
					return this.ansiHash.SequenceEqual(ntlm.ansiHash) && this.unicodeHash.SequenceEqual(ntlm.unicodeHash);
					/*
					 * This still isn't quite right. If one npa object does not have external
					 * hashes and the other does then they will not be considered equal even
					 * though they may be.
					 */
				}
				return true;
			}
			return false;
		}


		/// <returns> whether the hashes are externally supplied </returns>
		public virtual bool areHashesExternal() {
			return this.hashesExternal;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.smb.NtlmPasswordAuthenticator#getAnsiHash(jcifs.CIFSContext, byte[]) </seealso>
		/// throws java.security.GeneralSecurityException
		public override byte[] getAnsiHash(CIFSContext tc, byte[] chlng) {
			if (this.hashesExternal) {
				return this.ansiHash;
			}
			return base.getAnsiHash(tc, chlng);
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.smb.NtlmPasswordAuthenticator#getUnicodeHash(jcifs.CIFSContext, byte[]) </seealso>
		/// throws java.security.GeneralSecurityException
		public override byte[] getUnicodeHash(CIFSContext tc, byte[] chlng) {
			if (this.hashesExternal) {
				return this.unicodeHash;
			}
			return base.getUnicodeHash(tc, chlng);
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.smb.NtlmPasswordAuthenticator#getUserSessionKey(jcifs.CIFSContext, byte[]) </seealso>
		public override byte[] getUserSessionKey(CIFSContext tc, byte[] chlng) {
			if (this.hashesExternal) {
				return null;
			}
			return base.getUserSessionKey(tc, chlng);
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.smb.NtlmPasswordAuthenticator#getUserSessionKey(jcifs.CIFSContext, byte[], byte[], int) </seealso>
		/// throws SmbException
		public override void getUserSessionKey(CIFSContext tc, byte[] chlng, byte[] dest, int offset) {
			if (this.hashesExternal) {
				return;
			}
			base.getUserSessionKey(tc, chlng, dest, offset);
		}
	}

}