using System;
using System.Net;
using System.Text;
using cifs_ng.lib;
using cifs_ng.lib.ext;
using cifs_ng.lib.socket;
using Logger = org.slf4j.Logger;
using LoggerFactory = org.slf4j.LoggerFactory;
using Address = jcifs.Address;
using CIFSContext = jcifs.CIFSContext;
using CIFSException = jcifs.CIFSException;
using DfsReferralData = jcifs.DfsReferralData;
using NetbiosAddress = jcifs.NetbiosAddress;
using RuntimeCIFSException = jcifs.RuntimeCIFSException;
using SmbConstants = jcifs.SmbConstants;
using SmbResourceLocator = jcifs.SmbResourceLocator;
using StringUtil = jcifs.@internal.util.StringUtil;
using NbtAddress = jcifs.netbios.NbtAddress;
using UniAddress = jcifs.netbios.UniAddress;

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
namespace jcifs.smb {





	/// 
	/// 
	/// <summary>
	/// This mainly tracks two locations:
	/// - canonical URL path: path component of the URL: this is used to reconstruct URLs to resources and is not adjusted by
	/// DFS referrals. (E.g. a resource with a DFS root's parent will still point to the DFS root not the share it's actually
	/// located in).
	/// - share + uncpath within it: This is the relevant information for most SMB requests. Both are adjusted by DFS
	/// referrals. Nested resources will inherit the information already resolved by the parent resource.
	/// 
	/// Invariant:
	/// A directory resource must have a trailing slash/backslash for both URL and UNC path at all times.
	/// 
	/// @author mbechler
	/// 
	/// </summary>
	internal class SmbResourceLocatorImpl : SmbResourceLocatorInternal, ICloneable {

		private static readonly Logger log = LoggerFactory.getLogger(typeof(SmbResourceLocatorImpl));

		private readonly URL url;

		private DfsReferralData dfsReferral = null; // For getDfsPath() and getServerWithDfs()

		private string unc; // Initially null; set by getUncPath; never ends with '/'
		private string canon; // Initially null; set by getUncPath; dir must end with '/'
		private string share; // Can be null

		private Address[] addresses;
		private int addressIndex;
		private int type;

		private CIFSContext ctx;


		/// 
		/// <param name="ctx"> </param>
		/// <param name="u"> </param>
		public SmbResourceLocatorImpl(CIFSContext ctx, URL u) {
			this.ctx = ctx;
			this.url = u;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= java.lang.Object#clone() </seealso>
		public  object Clone() {
			SmbResourceLocatorImpl loc = new SmbResourceLocatorImpl(this.ctx, this.url);
			loc.canon = this.canon;
			loc.share = this.share;
			loc.dfsReferral = this.dfsReferral;
			loc.unc = this.unc;
			if (this.addresses != null) {
				loc.addresses = new UniAddress[this.addresses.Length];
				Array.Copy(this.addresses, 0, loc.addresses, 0, this.addresses.Length);
			}
			loc.addressIndex = this.addressIndex;
			loc.type = this.type;
			return loc;
		}


		/// <param name="context"> </param>
		/// <param name="name"> </param>
		internal virtual void resolveInContext(SmbResourceLocator context, string name) {
			string shr = context.getShare();
			if (shr != null) {
				this.dfsReferral = context.getDfsReferral();
			}
			int last = name.Length - 1;
			bool trailingSlash = false;
			if (last >= 0 && name[last] == '/') {
				trailingSlash = true;
				name = name.Substring(0, last);
			}
			if (shr == null) {
				string[] nameParts = name.Split("/", StringSplitOptions.RemoveEmptyEntries);

				// server is set through URL, however it's still in the name
				int pos = 0;
				if (context.getServer()==null) {
					pos = 1;
				}

				// first remaining path element would be share
				if (nameParts.Length > pos) {
					this.share = nameParts[pos++];
				}

				// all other remaining path elements are actual path
				if (nameParts.Length > pos) {
					string[] remainParts = new string[nameParts.Length - pos];
					Array.Copy(nameParts, pos, remainParts, 0, nameParts.Length - pos);
					this.unc = "\\" + StringUtil.join("\\", remainParts) + (trailingSlash ? "\\" : "");
					this.canon = "/" + this.share + "/" + StringUtil.join("/", remainParts) + (trailingSlash ? "/" : "");
				}
				else {
					this.unc = "\\";
					if (this.share!=null) {
						this.canon = "/" + this.share + (trailingSlash ? "/" : "");
					}
					else {
						this.canon = "/";
					}
				}
			}
			else {
				string uncPath = context.getUNCPath();
				if (uncPath.Equals("\\")) {
					// context share != null, so the remainder is path
					this.unc = '\\' + name.Replace('/', '\\') + (trailingSlash ? "\\" : "");
					this.canon = context.getURLPath() + name + (trailingSlash ? "/" : "");
					this.share = shr;
				}
				else {
					this.unc = uncPath + name.Replace('/', '\\') + (trailingSlash ? "\\" : "");
					this.canon = context.getURLPath() + name + (trailingSlash ? "/" : "");
					this.share = shr;
				}
			}
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.SmbResourceLocator#getDfsReferral() </seealso>
		public virtual DfsReferralData getDfsReferral() {
			return this.dfsReferral;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.SmbResourceLocator#getName() </seealso>

		public virtual string getName() {
			string urlpath = getURLPath();
			string shr = getShare();
			if (urlpath.Length > 1) {
				int i = urlpath.Length - 2;
				while (urlpath[i] != '/') {
					i--;
				}
				return urlpath.Substring(i + 1);
			}
			else if (shr != null) {
				return shr + '/';
			}
			else if (this.url.Host?.Length > 0) {
				return this.url.Host + '/';
			}
			else {
				return "smb://";
			}
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.SmbResourceLocator#getParent() </seealso>
		public virtual string getParent() {
			string str = this.url.Authority;

			if (str != null && str.Length > 0) {
				StringBuilder sb = new StringBuilder("smb://");

				sb.Append(str);

				string urlpath = getURLPath();
				if (urlpath.Length > 1) {
					sb.Append(urlpath);
				}
				else {

					sb.Append('/');
				}

				str = sb.ToString();

				int i = str.Length - 2;
				while (str[i] != '/') {
					i--;
				}

				return str.Substring(0, i + 1);
			}

			return "smb://";
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.SmbResourceLocator#getPath() </seealso>

		public virtual string getPath() {
			return this.url.ToString();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.SmbResourceLocator#getCanonicalURL() </seealso>
		public virtual string getCanonicalURL() {
			string str = this.url.Authority;
			if (str != null && str.Length > 0) {
				return "smb://" + this.url.Authority + this.getURLPath();
			}
			return "smb://";
		}


		public virtual string getUNCPath() {
			if (this.unc==null) {
				canonicalizePath();
			}
			return this.unc;
		}


		public virtual string getURLPath() {
			if (this.unc==null) {
				canonicalizePath();
			}
			return this.canon;
		}


		public virtual string getShare() {
			if (this.unc==null) {
				canonicalizePath();
			}
			return this.share;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.SmbResourceLocator#getServerWithDfs() </seealso>
		public virtual string getServerWithDfs() {
			if (this.dfsReferral != null) {
				return this.dfsReferral.getServer();
			}
			return getServer();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.SmbResourceLocator#getServer() </seealso>
		public virtual string getServer() {
			string str = this.url.Host;
			if (str.Length == 0) {
				return null;
			}
			return str;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.SmbResourceLocator#getDfsPath() </seealso>
		public virtual string getDfsPath() {
			if (this.dfsReferral == null) {
				return null;
			}
			return "smb://" + this.dfsReferral.getServer() + "/" + this.dfsReferral.getShare() + this.getUNCPath().Replace('\\', '/');
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.SmbResourceLocator#getPort() </seealso>
		public virtual int getPort() {
			return this.url.Port;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.SmbResourceLocator#getURL() </seealso>
		public virtual URL getURL() {
			return this.url;
		}


		/// 
		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.smb.SmbResourceLocatorInternal#shouldForceSigning() </seealso>
		public virtual bool shouldForceSigning() {
			return this.ctx.getConfig().isIpcSigningEnforced() && !this.ctx.getCredentials().isAnonymous() && isIPC();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.SmbResourceLocator#isIPC() </seealso>
		public virtual bool isIPC() {
			string shr = this.getShare();
			if (shr == null || "IPC$".Equals(getShare())) {
				if (log.isDebugEnabled()) {
					log.debug("Share is IPC " + this.share);
				}
				return true;
			}
			return false;
		}


		/// <param name="t"> </param>
		internal virtual void updateType(int t) {
			this.type = t;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.SmbResourceLocator#getType() </seealso>
		/// throws jcifs.CIFSException
		public virtual int getType() {
			if (this.type == 0) {
				if (getUNCPath().Length > 1) {
					this.type = SmbConstants.TYPE_FILESYSTEM;
				}
				else if (getShare()!=null) {
					if (getShare().Equals("IPC$")) {
						this.type = SmbConstants.TYPE_NAMED_PIPE;
					}
					else {
						this.type = SmbConstants.TYPE_SHARE;
					}
				}
				else if (string.IsNullOrEmpty(this.url.Authority) ) {
					this.type = SmbConstants.TYPE_WORKGROUP;
				}
				else {
					try {
						NetbiosAddress nbaddr = getAddress().unwrap<NetbiosAddress>(typeof(NetbiosAddress));
						if (nbaddr != null) {
							int code = nbaddr.getNameType();
							if (code == 0x1d || code == 0x1b) {
								this.type = SmbConstants.TYPE_WORKGROUP;
								return this.type;
							}
						}
					}
					catch (CIFSException e) {
						//TODO ex
						if (!(e.InnerException is UriFormatException)) {
							throw e;
						}
						log.debug("Unknown host", e);
					}
					this.type = SmbConstants.TYPE_SERVER;
				}
			}
			return this.type;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.SmbResourceLocator#isWorkgroup() </seealso>
		/// throws jcifs.CIFSException
		public virtual bool isWorkgroup() {
			if (this.type == SmbConstants.TYPE_WORKGROUP || this.url.Host.Length == 0) {
				this.type = SmbConstants.TYPE_WORKGROUP;
				return true;
			}

			if (getShare()==null) {
				NetbiosAddress addr = getAddress().unwrap<NetbiosAddress>(typeof(NetbiosAddress));
				if (addr != null) {
					int code = addr.getNameType();
					if (code == 0x1d || code == 0x1b) {
						this.type = SmbConstants.TYPE_WORKGROUP;
						return true;
					}
				}
				this.type = SmbConstants.TYPE_SERVER;
			}
			return false;
		}


		/// throws jcifs.CIFSException
		public virtual Address getAddress() {
			if (this.addressIndex == 0) {
				return getFirstAddress();
			}
			return this.addresses[this.addressIndex - 1];
		}


		internal static string queryLookup(string query, string param) {
			char[] @in = query.ToCharArray();
			int i, ch, st, eq;

			st = eq = 0;
			for (i = 0; i < @in.Length; i++) {
				ch = @in[i];
				if (ch == '&') {
					if (eq > st) {
						string p = new string(@in, st, eq - st);
						if (p.Equals(param, StringComparison.OrdinalIgnoreCase)) {
							eq++;
							return new string(@in, eq, i - eq);
						}
					}
					st = i + 1;
				}
				else if (ch == '=') {
					eq = i;
				}
			}
			if (eq > st) {
				string p = new string(@in, st, eq - st);
				if (p.Equals(param, StringComparison.OrdinalIgnoreCase)) {
					eq++;
					return new string(@in, eq, @in.Length - eq);
				}
			}

			return null;
		}


		/// throws jcifs.CIFSException
		internal virtual Address getFirstAddress() {
			this.addressIndex = 0;

			if (this.addresses == null) {
				string host = this.url.Host;
				string path = this.url.getPath();
				string query = this.url.Query;
				try {
					if (!string.IsNullOrEmpty(query)) {
						string server = queryLookup(query, "server");
						if (!string.IsNullOrEmpty(server)) {
							this.addresses = new UniAddress[1];
							this.addresses[0] = this.ctx.getNameServiceClient().getByName(server);
						}
						string address = queryLookup(query, "address");
						if (!string.IsNullOrEmpty(address)) {
							var ip = IPAddress.Parse(address);
							this.addresses = new UniAddress[1];
							//TODO 1 host
							this.addresses[0] = new UniAddress(ip);
						}
					}
					else if (host.Length == 0) {
						try {
							Address addr = this.ctx.getNameServiceClient().getNbtByName(NbtAddress.MASTER_BROWSER_NAME, 0x01, null);
							this.addresses = new UniAddress[1];
							this.addresses[0] = this.ctx.getNameServiceClient().getByName(addr.getHostAddress());
						}
						catch (UnknownHostException uhe) {
							log.debug("Unknown host", uhe);
							if (string.IsNullOrEmpty(this.ctx.getConfig().getDefaultDomain())) {
								throw uhe;
							}
							this.addresses = this.ctx.getNameServiceClient().getAllByName(this.ctx.getConfig().getDefaultDomain(), true);
						}
					}
					else if (path.Length == 0 || path.Equals("/")) {
						this.addresses = this.ctx.getNameServiceClient().getAllByName(host, true);
					}
					else {
						this.addresses = this.ctx.getNameServiceClient().getAllByName(host, false);
					}
				}
				catch (UnknownHostException e) {
					throw new CIFSException("Failed to lookup address for name " + host, e);
				}
			}

			return getNextAddress();
		}


		internal virtual Address getNextAddress() {
			Address addr = null;
			if (this.addressIndex < this.addresses.Length) {
				addr = this.addresses[this.addressIndex++];
			}
			return addr;
		}


		internal virtual bool hasNextAddress() {
			return this.addressIndex < this.addresses.Length;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.SmbResourceLocator#isRoot() </seealso>
		public virtual bool isRoot() {
			// length == 0 should not happen
			return getShare()==null && getUNCPath().Length <= 1;
		}


		internal virtual bool isRootOrShare() {
			// length == 0 should not happen
			return getUNCPath().Length <= 1;
		}


		/// <exception cref="MalformedURLException">
		///  </exception>
		private void canonicalizePath() {
			lock (this) {
				char[] @in = this.url.getPath().ToCharArray();
				char[] @out = new char[@in.Length];
				int length = @in.Length, prefixLen = 0, state = 0;
        
				/*
				 * The canonicalization routine
				 */
				for (int i = 0; i < length; i++) {
					switch (state) {
					case 0:
						if (@in[i] != '/') {
							// Checked exception (e.g. MalformedURLException) would be better
							// but this would be a nightmare API wise
							throw new RuntimeCIFSException("Invalid smb: URL: " + this.url);
						}
						@out[prefixLen++] = @in[i];
						state = 1;
						break;
					case 1:
						if (@in[i] == '/') {
							break;
						}
						else if (@in[i] == '.' && ((i + 1) >= length || @in[i + 1] == '/')) {
							i++;
							break;
						}
						else if ((i + 1) < length && @in[i] == '.' && @in[i + 1] == '.' && ((i + 2) >= length || @in[i + 2] == '/')) {
							i += 2;
							if (prefixLen == 1) {
								break;
							}
							do {
								prefixLen--;
							} while (prefixLen > 1 && @out[prefixLen - 1] != '/');
							break;
						}
						state = 2;
						goto case 2;
					case 2:
						if (@in[i] == '/') {
							state = 1;
						}
						@out[prefixLen++] = @in[i];
						break;
					}
				}
        
				this.canon = new string(@out, 0, prefixLen);
				if (prefixLen > 1) {
					prefixLen--;
					int firstSep = this.canon.IndexOf('/', 1);
					if (firstSep < 0) {
						this.share = this.canon.Substring(1);
						this.unc = "\\";
					}
					else if (firstSep == prefixLen) {
						this.share = this.canon.Substring(1, firstSep - 1);
						this.unc = "\\";
					}
					else {
						this.share = this.canon.Substring(1, firstSep - 1);
						this.unc = this.canon.Substring(firstSep, (prefixLen + 1) - firstSep).Replace('/', '\\');
					}
				}
				else {
					this.canon = "/";
					this.share = null;
					this.unc = "\\";
				}
			}
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= java.lang.Object#hashCode() </seealso>
		public override int GetHashCode() {
			int hash;
			try {
				hash = getAddress().GetHashCode();
			}
			catch (CIFSException) {
				hash = getServer().ToUpper().GetHashCode();
			}
			return hash + getURLPath().ToUpper().GetHashCode();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= java.lang.Object#equals(java.lang.Object) </seealso>
		public override bool Equals(object obj) {
			if (!(obj is SmbResourceLocatorImpl)) {
				return false;
			}

			SmbResourceLocatorImpl o = (SmbResourceLocatorImpl) obj;

			/*
			 * If uncertain, pathNamesPossiblyEqual returns true.
			 * Comparing canonical paths is definitive.
			 */
			if (pathNamesPossiblyEqual(this.url.getPath(), o.url.getPath())) {
				if (getURLPath().Equals(o.getURLPath(), StringComparison.OrdinalIgnoreCase)) {
					try {
						return getAddress().Equals(o.getAddress());
					}
					catch (CIFSException uhe) {
						log.debug("Unknown host", uhe);
						return getServer().Equals(o.getServer(), StringComparison.OrdinalIgnoreCase);
					}
				}
			}
			return false;
		}


		private static bool pathNamesPossiblyEqual(string path1, string path2) {
			int p1, p2, l1, l2;

			// if unsure return this method returns true

			p1 = path1.LastIndexOf('/');
			p2 = path2.LastIndexOf('/');
			l1 = path1.Length - p1;
			l2 = path2.Length - p2;

			// anything with dots voids comparison
			if (l1 > 1 && path1[p1 + 1] == '.') {
				return true;
			}
			if (l2 > 1 && path2[p2 + 1] == '.') {
				return true;
			}

			//TODO 
			return l1 == l2 && path1.compare(true, p1, path2, p2, l1);
		}


		/// 
		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.smb.SmbResourceLocatorInternal#overlaps(jcifs.SmbResourceLocator) </seealso>
		/// throws jcifs.CIFSException
		public virtual bool overlaps(SmbResourceLocator other) {
			string tp = getCanonicalURL();
			string op = other.getCanonicalURL();
			return getAddress().Equals(other.getAddress()) && tp.compare(true, 0, op, 0, Math.Min(tp.Length, op.Length));
		}


		/// <param name="dr"> </param>
		/// <param name="reqPath"> </param>
		/// <returns> UNC path the redirect leads to </returns>
		public virtual string handleDFSReferral(DfsReferralData dr, string reqPath) {
			if (Equals(this.dfsReferral, dr)) {
				return this.unc;
			}
			this.dfsReferral = dr;

			string oldUncPath = getUNCPath();
			int pathConsumed = dr.getPathConsumed();
			if (pathConsumed < 0) {
				log.warn("Path consumed out of range " + pathConsumed);
				pathConsumed = 0;
			}
			else if (pathConsumed > this.unc.Length) {
				log.warn("Path consumed out of range " + pathConsumed);
				pathConsumed = oldUncPath.Length;
			}

			if (log.isDebugEnabled()) {
				log.debug("UNC is '" + oldUncPath + "'");
				log.debug("Consumed '" + oldUncPath.Substring(0, pathConsumed) + "'");
			}
			string dunc = oldUncPath.Substring(pathConsumed);
			if (log.isDebugEnabled()) {
				log.debug("Remaining '" + dunc + "'");
			}

			if (dunc.Equals("") || dunc.Equals("\\")) {
				dunc = "\\";
				this.type = SmbConstants.TYPE_SHARE;
			}
			if (dr.getPath().Length > 0) {
				dunc = "\\" + dr.getPath() + dunc;
			}

			if (dunc[0] != '\\') {
				log.warn("No slash at start of remaining DFS path " + dunc);
			}

			this.unc = dunc;
			if (dr.getShare()!=null && dr.getShare().Length > 0) {
				this.share = dr.getShare();
			}
			if (reqPath != null && reqPath.EndsWith("\\", StringComparison.Ordinal) && !dunc.EndsWith("\\", StringComparison.Ordinal)) {
				dunc += "\\";
			}
			return dunc;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= java.lang.Object#toString() </seealso>
		public override string ToString() {
			StringBuilder sb = new StringBuilder(this.url.ToString());
			sb.Append('[');
			if (this.unc!=null) {
				sb.Append("unc=");
				sb.Append(this.unc);
			}
			if (this.canon!=null) {
				sb.Append("canon=");
				sb.Append(this.canon);
			}
			if (this.dfsReferral != null) {
				sb.Append("dfsReferral=");
				sb.Append(this.dfsReferral);
			}
			sb.Append(']');
			return sb.ToString();
		}

	}

}