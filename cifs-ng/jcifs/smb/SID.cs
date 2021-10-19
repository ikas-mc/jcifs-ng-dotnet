using jcifs;
using jcifs.util;

using System;
using System.IO;
using cifs_ng.lib.ext;
using Logger = org.slf4j.Logger;
using LoggerFactory = org.slf4j.LoggerFactory;
using CIFSContext = jcifs.CIFSContext;
using RuntimeCIFSException = jcifs.RuntimeCIFSException;
using rpc = jcifs.dcerpc.rpc;
using SMBUtil = jcifs.@internal.util.SMBUtil;
using Hexdump = jcifs.util.Hexdump;

/* jcifs smb client library in Java
 * Copyright (C) 2006  "Michael B. Allen" <jcifs at samba dot org>
 *                     "Eric Glass" <jcifs at samba dot org>
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
	/// Internal representation of SIDs
	/// 
	/// A Windows SID is a numeric identifier used to represent Windows
	/// accounts. SIDs are commonly represented using a textual format such as
	/// <tt>S-1-5-21-1496946806-2192648263-3843101252-1029</tt> but they may
	/// also be resolved to yield the name of the associated Windows account
	/// such as <tt>Administrators</tt> or <tt>MYDOM\alice</tt>.
	/// <para>
	/// Consider the following output of <tt>examples/SidLookup.java</tt>:
	/// 
	/// <pre>
	///        toString: S-1-5-21-4133388617-793952518-2001621813-512
	/// toDisplayString: WNET\Domain Admins
	///         getType: 2
	///     getTypeText: Domain group
	///   getDomainName: WNET
	///  getAccountName: Domain Admins
	/// </pre>
	/// 
	/// @internal
	/// </para>
	/// </summary>
	public class SID : rpc.sid_t, jcifs.SID {

		private static readonly Logger log = LoggerFactory.getLogger(typeof(SID));

		internal static readonly string[] SID_TYPE_NAMES = new string[] {"0", "User", "Domain group", "Domain", "Local group", "Builtin group", "Deleted", "Invalid", "Unknown"};

		/// 
		public const int SID_FLAG_RESOLVE_SIDS = 0x0001;

		/// <summary>
		/// Well known SID: EVERYONE
		/// </summary>
		public static SID EVERYONE = null;

		/// <summary>
		/// Well known SID: CREATOR_OWNER
		/// </summary>
		public static SID CREATOR_OWNER = null;

		/// <summary>
		/// Well known SID: SYSTEM
		/// </summary>
		public static SID SYSTEM = null;

		static SID() {
			try {
				EVERYONE = new SID("S-1-1-0");
				CREATOR_OWNER = new SID("S-1-3-0");
				SYSTEM = new SID("S-1-5-18");
			}
			catch (SmbException se) {
				log.error("Failed to create builtin SIDs", se);
			}
		}


		/// <summary>
		/// Convert a sid_t to byte array
		/// </summary>
		/// <param name="sid"> </param>
		/// <returns> byte encoded form </returns>
		public static byte[] toByteArray(rpc.sid_t sid) {
			byte[] dst = new byte[1 + 1 + 6 + sid.sub_authority_count * 4];
			int di = 0;
			dst[di++] = sid.revision;
			dst[di++] = sid.sub_authority_count;
			Array.Copy(sid.identifier_authority, 0, dst, di, 6);
			di += 6;
			for (int ii = 0; ii < sid.sub_authority_count; ii++) {
				Encdec.enc_uint32le(sid.sub_authority[ii], dst, di);
				di += 4;
			}
			return dst;
		}

		internal int type;
		internal string domainName = null;
		internal string acctName = null;
		internal string origin_server = null;
		internal CIFSContext origin_ctx = null;


		/// <summary>
		/// Construct a SID from it's binary representation.
		/// 
		/// </summary>
		/// <param name="src"> </param>
		/// <param name="si"> </param>
		public SID(byte[] src, int si) {
			this.revision = src[si++];
			this.sub_authority_count = src[si++];
			this.identifier_authority = new byte[6];
			Array.Copy(src, si, this.identifier_authority, 0, 6);
			si += 6;
			if (this.sub_authority_count > 100) {
				throw new RuntimeCIFSException("Invalid SID sub_authority_count");
			}
			this.sub_authority = new int[this.sub_authority_count];
			for (int i = 0; i < this.sub_authority_count; i++) {
				this.sub_authority[i] = SMBUtil.readInt4(src, si);
				si += 4;
			}
		}


		/// <summary>
		/// Construct a SID from it's textual representation such as
		/// <tt>S-1-5-21-1496946806-2192648263-3843101252-1029</tt>.
		/// </summary>
		/// <param name="textual"> </param>
		/// <exception cref="SmbException"> </exception>
		/// throws SmbException
		public SID(string textual) {
			StringTokenizer st = new StringTokenizer(textual, "-");
			if (st.countTokens() < 3 || !st.nextToken().Equals("S")) {
				// need S-N-M
				throw new SmbException("Bad textual SID format: " + textual);
			}

			this.revision = byte.Parse(st.nextToken());
			string tmp = st.nextToken();
			long id = 0;
			if (tmp.StartsWith("0x", StringComparison.Ordinal)) {
				id = Convert.ToInt64(tmp.Substring(2), 16);
			}
			else {
				id = long.Parse(tmp);
			}

			this.identifier_authority = new byte[6];
			for (int i = 5; id > 0; i--) {
				this.identifier_authority[i] = (byte)(id % 256);
				id >>= 8;
			}

			this.sub_authority_count = (byte) st.countTokens();
			if (this.sub_authority_count > 0) {
				this.sub_authority = new int[this.sub_authority_count];
				for (int i = 0; i < this.sub_authority_count; i++) {
					this.sub_authority[i] = unchecked((int)(long.Parse(st.nextToken()) & 0xFFFFFFFFL));
				}
			}
		}


		/// <summary>
		/// Construct a SID from a domain SID and an RID
		/// (relative identifier). For example, a domain SID
		/// <tt>S-1-5-21-1496946806-2192648263-3843101252</tt> and RID <tt>1029</tt> would
		/// yield the SID <tt>S-1-5-21-1496946806-2192648263-3843101252-1029</tt>.
		/// </summary>
		/// <param name="domsid"> </param>
		/// <param name="rid"> </param>
		public SID(SID domsid, int rid) {
			this.revision = domsid.revision;
			this.identifier_authority = domsid.identifier_authority;
			this.sub_authority_count = (byte)(domsid.sub_authority_count + 1);
			this.sub_authority = new int[this.sub_authority_count];
			int i;
			for (i = 0; i < domsid.sub_authority_count; i++) {
				this.sub_authority[i] = domsid.sub_authority[i];
			}
			this.sub_authority[i] = rid;
		}


		/// <summary>
		/// Construct a relative SID
		/// </summary>
		/// <param name="domsid"> </param>
		/// <param name="id"> </param>
		public SID(SID domsid, SID id) {
			this.revision = domsid.revision;
			this.identifier_authority = domsid.identifier_authority;
			this.sub_authority_count = (byte)(domsid.sub_authority_count + id.sub_authority_count);
			this.sub_authority = new int[this.sub_authority_count];
			int i;
			for (i = 0; i < domsid.sub_authority_count; i++) {
				this.sub_authority[i] = domsid.sub_authority[i];
			}
			for (i = domsid.sub_authority_count; i < domsid.sub_authority_count + id.sub_authority_count; i++) {
				this.sub_authority[i] = id.sub_authority[i - domsid.sub_authority_count];
			}
		}


		/// 
		/// <param name="sid"> </param>
		/// <param name="type"> </param>
		/// <param name="domainName"> </param>
		/// <param name="acctName"> </param>
		/// <param name="decrementAuthority"> </param>
		public SID(rpc.sid_t sid, int type, string domainName, string acctName, bool decrementAuthority) {
			this.revision = sid.revision;
			this.sub_authority_count = sid.sub_authority_count;
			this.identifier_authority = sid.identifier_authority;
			this.sub_authority = sid.sub_authority;
			this.type = type;
			this.domainName = domainName;
			this.acctName = acctName;

			if (decrementAuthority) {
				this.sub_authority_count--;
				this.sub_authority = new int[this.sub_authority_count];
				for (int i = 0; i < this.sub_authority_count; i++) {
					this.sub_authority[i] = sid.sub_authority[i];
				}
			}
		}


		/// 
		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.SID#unwrap(java.lang.Class) </seealso>
		public virtual T unwrap<T>(Type t) {
			if (this is T v) {
				return v;
			}
			throw new System.InvalidCastException();
		}


		/// 
		/// <returns> encoded SID </returns>
		public virtual byte[] toByteArray() {
			return toByteArray(this);
		}


		/// 
		/// <returns> whether the SID is empty (no sub-authorities) </returns>
		public virtual bool isEmpty() {
			return this.sub_authority_count == 0;
		}


		/// 
		/// <returns> whether the SID is blank (all sub-authorities zero) </returns>
		public virtual bool isBlank() {
			bool blank = true;
			foreach (int sub in this.sub_authority) {
				blank = blank && (sub == 0);
			}
			return blank;
		}


		/// 
		/// <returns> domain SID </returns>
		public virtual jcifs.SID getDomainSid() {
			return new SID(this, SIDConstants.SID_TYPE_DOMAIN, this.domainName, null, getType() != SIDConstants.SID_TYPE_DOMAIN);
		}


		/// <summary>
		/// Get the RID
		/// 
		/// This is the last subauthority identifier
		/// </summary>
		/// <returns> the RID </returns>
		public virtual int getRid() {
			if (getType() == SIDConstants.SID_TYPE_DOMAIN) {
				throw new System.ArgumentException("This SID is a domain sid");
			}
			return this.sub_authority[this.sub_authority_count - 1];
		}


		public virtual int getType() {
			if ((this.origin_server!= null)) {
				resolveWeak();
			}
			return this.type;
		}


		public virtual string getTypeText() {
			if ((this.origin_server!= null)) {
				resolveWeak();
			}
			return SID_TYPE_NAMES[this.type];
		}


		public virtual string getDomainName() {
			if ((this.origin_server!= null)) {
				resolveWeak();
			}
			if (this.type == SIDConstants.SID_TYPE_UNKNOWN) {
				string full = ToString();
				return full.Substring(0, full.Length - getAccountName().Length - 1);
			}
			return this.domainName;
		}


		public virtual string getAccountName() {
			if ((this.origin_server!= null)) {
				resolveWeak();
			}
			if (this.type == SIDConstants.SID_TYPE_UNKNOWN) {
				return "" + this.sub_authority[this.sub_authority_count - 1];
			}
			if (this.type == SIDConstants.SID_TYPE_DOMAIN) {
				return "";
			}
			return this.acctName;
		}


		public override int GetHashCode() {
			int hcode = this.identifier_authority[5];
			for (int i = 0; i < this.sub_authority_count; i++) {
				hcode += 65599 * this.sub_authority[i];
			}
			return hcode;
		}


		public override bool Equals(object obj) {
			if (obj is SID) {
				SID sid = (SID) obj;
				if (sid == this) {
					return true;
				}
				if (sid.sub_authority_count == this.sub_authority_count) {
					int i = this.sub_authority_count;
					while (i-- > 0) {
						if (sid.sub_authority[i] != this.sub_authority[i]) {
							return false;
						}
					}
					for (i = 0; i < 6; i++) {
						if (sid.identifier_authority[i] != this.identifier_authority[i]) {
							return false;
						}
					}

					return sid.revision == this.revision;
				}
			}
			return false;
		}


		/// <summary>
		/// Return the numeric representation of this sid such as
		/// <tt>S-1-5-21-1496946806-2192648263-3843101252-1029</tt>.
		/// </summary>
		public override string ToString() {
			string ret = "S-" + (this.revision & 0xFF) + "-";

			if (this.identifier_authority[0] != (byte) 0 || this.identifier_authority[1] != (byte) 0) {
				ret += "0x";
				ret += Hexdump.toHexString(this.identifier_authority, 0, 6);
			}
			else {
				int shift = 0;
				long id = 0;
				for (int i = 5; i > 1; i--) {
					id += (this.identifier_authority[i] & 0xFFL) << shift;
					shift += 8;
				}
				ret += id;
			}

			for (int i = 0; i < this.sub_authority_count; i++) {
				ret += "-" + (this.sub_authority[i] & 0xFFFFFFFFL);
			}

			return ret;
		}


		public virtual string toDisplayString() {
			if ((this.origin_server!= null)) {
				resolveWeak();
			}
			if ((this.domainName!= null)) {
				string str;

				if (this.type == SIDConstants.SID_TYPE_DOMAIN) {
					str = this.domainName;
				}
				else if (this.type == SIDConstants.SID_TYPE_WKN_GRP || this.domainName.Equals("BUILTIN")) {
					if (this.type == SIDConstants.SID_TYPE_UNKNOWN) {
						str = ToString();
					}
					else {
						str = this.acctName;
					}
				}
				else {
					str = this.domainName + "\\" + this.acctName;
				}

				return str;
			}
			return ToString();
		}


		/// <summary>
		/// Manually resolve this SID. Normally SIDs are automatically
		/// resolved. However, if a SID is constructed explicitly using a SID
		/// constructor, JCIFS will have no knowledge of the server that created the
		/// SID and therefore cannot possibly resolve it automatically. In this case,
		/// this method will be necessary.
		/// </summary>
		/// <param name="authorityServerName">
		///            The FQDN of the server that is an authority for the SID. </param>
		/// <param name="tc">
		///            Context to use </param>
		/// <exception cref="IOException"> </exception>
		/// throws java.io.IOException
		public virtual void resolve(string authorityServerName, CIFSContext tc) {
			SID[] sids = new SID[1];
			sids[0] = this;
			tc.getSIDResolver().resolveSids(tc, authorityServerName, sids);
		}


		internal virtual void resolveWeak() {
			if (this.origin_server!=null) {
				try {
					resolve(this.origin_server, this.origin_ctx);
				}
				catch (IOException ioe) {
					log.debug("Failed to resolve SID", ioe);
				}
				finally {
					this.origin_server = null;
					this.origin_ctx = null;
				}
			}
		}


		/// <summary>
		/// Get members of the group represented by this SID, if it is one.
		/// </summary>
		/// <param name="authorityServerName"> </param>
		/// <param name="tc"> </param>
		/// <param name="flags"> </param>
		/// <returns> the members of the group </returns>
		/// <exception cref="IOException"> </exception>
		/// throws java.io.IOException
		public virtual jcifs.SID[] getGroupMemberSids(string authorityServerName, CIFSContext tc, int flags) {
			if (this.type != SIDConstants.SID_TYPE_DOM_GRP && this.type != SIDConstants.SID_TYPE_ALIAS) {
				return new SID[0];
			}

			return tc.getSIDResolver().getGroupMemberSids(tc, authorityServerName, getDomainSid(), getRid(), flags);
		}


		/// <param name="context"> </param>
		/// <param name="server"> </param>
		public virtual void initContext(string server, CIFSContext context) {
			this.origin_ctx = context;
			this.origin_server = server;
		}

	}

}