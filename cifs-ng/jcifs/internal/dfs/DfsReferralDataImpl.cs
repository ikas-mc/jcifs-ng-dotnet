using jcifs;
using System;
using System.Collections.Generic;
using cifs_ng.lib.ext;
using Logger = org.slf4j.Logger;
using LoggerFactory = org.slf4j.LoggerFactory;
using Trans2GetDfsReferralResponse = jcifs.@internal.smb1.trans2.Trans2GetDfsReferralResponse;

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
namespace jcifs.@internal.dfs {


	/// <summary>
	/// @author mbechler
	/// 
	/// </summary>
	public class DfsReferralDataImpl : DfsReferralDataInternal {

		private static readonly Logger log = LoggerFactory.getLogger(typeof(DfsReferralDataImpl));

		private int pathConsumed;
		private long ttl;
		private string server; // Server
		private string share; // Share
		private string link;
		private string path; // Path relative to tree from which this referral was thrown

		private long expiration;
		private int rflags;

		private bool resolveHashes;

		private DfsReferralDataImpl nextField;
		private IDictionary<string, DfsReferralDataInternal> map;
		private string key;
		private string domain;

		private bool intermediateField;


		/// 
		public DfsReferralDataImpl() {
			this.nextField = this;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.DfsReferralData#unwrap(java.lang.Class) </seealso>
		public virtual T unwrap<T>(Type type) {
			if (this is T v) {
				return v;
			}
			throw new System.InvalidCastException();
		}


		public virtual long getExpiration() {
			return this.expiration;
		}


		public virtual int getPathConsumed() {
			return this.pathConsumed;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.DfsReferralData#getDomain() </seealso>
		public virtual string getDomain() {
			return this.domain;
		}


		/// <param name="domain">
		///            the domain to set </param>
		public virtual void setDomain(string domain) {
			this.domain = domain;
		}


		public virtual string getLink() {
			return this.link;
		}


		public virtual void setLink(string link) {
			this.link = link;
		}


		/// <returns> the key </returns>
		public virtual string getKey() {
			return this.key;
		}


		/// <param name="key">
		///            the key to set </param>
		public virtual void setKey(string key) {
			this.key = key;
		}


		public virtual string getServer() {
			return this.server;
		}


		public virtual string getShare() {
			return this.share;
		}


		public virtual string getPath() {
			return this.path;
		}


		/// <returns> the rflags </returns>
		public virtual int getFlags() {
			return this.rflags;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.dfs.DfsReferralDataInternal#setCacheMap(java.util.Map) </seealso>
		public virtual void setCacheMap(IDictionary<string, DfsReferralDataInternal> map) {
			this.map = map;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.dfs.DfsReferralDataInternal#replaceCache() </seealso>
		public virtual void replaceCache() {
			if (this.map != null && (this.key!= null)) {
				this.map[this.key] = this;
			}
		}


		public  DfsReferralData next() {
			return this.nextField;
		}
		
		DfsReferralDataInternal DfsReferralDataInternal.next() {
			return this.nextField;
		}


		/// 
		/// <param name="dr"> </param>
		public virtual void append(DfsReferralDataInternal dr) {
			DfsReferralDataImpl dri = (DfsReferralDataImpl) dr;
			dri.nextField = this.nextField;
			this.nextField = dri;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.dfs.DfsReferralDataInternal#stripPathConsumed(int) </seealso>
		public virtual void stripPathConsumed(int i) {
			if (i > this.pathConsumed) {
				throw new System.ArgumentException("Stripping more than consumed");
			}
			this.pathConsumed -= i;
		}


		public virtual void fixupDomain(string dom) {
			string s = getServer();
			if (s.IndexOf('.') < 0 && s.ToUpperInvariant().Equals(s)) {
				string fqdn = s + "." + dom;
				if (log.isDebugEnabled()) {
					log.debug(string.Format("Applying DFS netbios name hack {0} -> {1} ", s, fqdn));
				}
				this.server = fqdn;
			}
		}


		public virtual void fixupHost(string fqdn) {
			string s = getServer();
			if (s.IndexOf('.') < 0 && s.ToUpperInvariant().Equals(s)) {
				if (fqdn.StartsWith(s.ToLowerInvariant() + ".", StringComparison.Ordinal)) {
					if (log.isDebugEnabled()) {
						log.debug("Adjusting server name " + s + " to " + fqdn);
					}
					this.server = fqdn;
				}
				else {
					log.warn("Have unmappable netbios name " + s);
				}
			}
		}


		/// <returns> the resolveHashes </returns>
		public virtual bool isResolveHashes() {
			return this.resolveHashes;
		}


		/// 
		public virtual void intermediate() {
			this.intermediateField = true;
		}


		/// <returns> the intermediate </returns>
		public virtual bool isIntermediate() {
			return this.intermediateField;
		}


		public override string ToString() {
			return "DfsReferralData[pathConsumed=" + this.pathConsumed + ",server=" + this.server + ",share=" + this.share + ",link=" + this.link + ",path=" + this.path + ",ttl=" + this.ttl + ",expiration=" + this.expiration + ",remain=" + (this.expiration - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()) + "]";
		}


		/// <returns> the ttl </returns>
		public virtual long getTtl() {
			return this.ttl;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= java.lang.Object#hashCode() </seealso>
		public override  int GetHashCode() {
			return RuntimeHelp.hashCode(this.server, this.share, this.path, this.pathConsumed);
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= java.lang.Object#equals(java.lang.Object) </seealso>
		public override bool Equals(object obj) {
			if (!(obj is DfsReferralData)) {
				return false;
			}
			DfsReferralData other = (DfsReferralData) obj;

			return Equals(getServer(), other.getServer()) && Equals(getShare(), other.getShare()) && Equals(getPath(), other.getPath()) && Equals(getPathConsumed(), other.getPathConsumed());
		}


		/// <param name="ref"> </param>
		/// <param name="reqPath"> </param>
		/// <param name="expire"> </param>
		/// <param name="consumed"> </param>
		/// <returns> referral data </returns>
		public static DfsReferralDataImpl fromReferral(Referral @ref, string reqPath, long expire, int consumed) {
			DfsReferralDataImpl dr = new DfsReferralDataImpl();
			string[] arr = new string[4];
			dr.ttl = @ref.getTtl();
			dr.rflags = @ref.getRFlags();
			dr.expiration = expire;
			if ((dr.rflags & Trans2GetDfsReferralResponse.FLAGS_NAME_LIST_REFERRAL) == Trans2GetDfsReferralResponse.FLAGS_NAME_LIST_REFERRAL) {
				string[] expandedNames = @ref.getExpandedNames();
				if (expandedNames.Length > 0) {
					dr.server = expandedNames[0].Substring(1).ToLower();
				}
				else {
					dr.server = @ref.getSpecialName().Substring(1).ToLower();
				}
				if (log.isDebugEnabled()) {
					log.debug("Server " + dr.server + " path " + reqPath + " remain " + reqPath.Substring(consumed) + " path consumed " + consumed);
				}
				dr.pathConsumed = consumed;
			}
			else {
				if (log.isDebugEnabled()) {
					log.debug("Node " + @ref.getNode() + " path " + reqPath + " remain " + reqPath.Substring(consumed) + " path consumed " + consumed);
				}
				dfsPathSplit(@ref.getNode(), arr);
				dr.server = arr[1];
				dr.share = arr[2];
				dr.path = arr[3];
				dr.pathConsumed = consumed;

				/*
				 * Samba has a tendency to return pathConsumed values so that they consume a trailing slash of the
				 * requested path. Normalize this here.
				 */
				if (reqPath[consumed - 1] == '\\') {
					if (log.isDebugEnabled()) {
						log.debug("Server consumed trailing slash of request path, adjusting");
					}
					dr.pathConsumed--;
				}

				if (log.isDebugEnabled()) {
					string cons = reqPath.Substring(0, consumed);
					log.debug("Request " + reqPath + " ref path " + dr.path + " consumed " + dr.pathConsumed + ": " + cons);
				}
			}

			return dr;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.dfs.DfsReferralDataInternal#combine(jcifs.DfsReferralData) </seealso>
		public virtual DfsReferralDataInternal combine(DfsReferralData n) {
			DfsReferralDataImpl dr = new DfsReferralDataImpl();
			dr.server = n.getServer();
			dr.share = n.getShare();
			dr.expiration = n.getExpiration();
			dr.path = n.getPath();
			dr.pathConsumed = this.pathConsumed + n.getPathConsumed();
			if (this.path!= null) {
				dr.pathConsumed -= ((this.path!=null) ? this.path.Length + 1 : 0);
			}
			dr.domain = n.getDomain();
			return dr;
		}


		/*
		 * Split DFS path like \fs1.example.com\root5\link2\foo\bar.txt into at
		 * most 3 components (not including the first index which is always empty):
		 * result[0] = ""
		 * result[1] = "fs1.example.com"
		 * result[2] = "root5"
		 * result[3] = "link2\foo\bar.txt"
		 */
		private static int dfsPathSplit(string path, string[] result) {
			int ri = 0, rlast = result.Length - 1;
			int i = 0, b = 0, len = path.Length;
			int strip = 0;

			do {
				if (ri == rlast) {
					result[rlast] = path.Substring(b);
					return strip;
				}
				if (i == len || path[i] == '\\') {
					result[ri++] = path.Substring(b, i - b);
					strip++;
					b = i + 1;
				}
			} while (i++ < len);

			while (ri < result.Length) {
				result[ri++] = "";
			}

			return strip;
		}

	}

}