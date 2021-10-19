using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using cifs_ng.lib.ext;
using Logger = org.slf4j.Logger;
using LoggerFactory = org.slf4j.LoggerFactory;
using CIFSContext = jcifs.CIFSContext;
using DfsReferralData = jcifs.DfsReferralData;
using DfsResolver = jcifs.DfsResolver;
using SmbTransport = jcifs.SmbTransport;
using DfsReferralDataImpl = jcifs.@internal.dfs.DfsReferralDataImpl;
using DfsReferralDataInternal = jcifs.@internal.dfs.DfsReferralDataInternal;

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
	/// Caching DFS resolver implementation
	/// 
	/// @internal
	/// </summary>
	public class DfsImpl : DfsResolver {

		private static readonly DfsReferralDataImpl NEGATIVE_ENTRY = new DfsReferralDataImpl();

		private class CacheEntry <T> {

			internal long expiration;
			internal IDictionary<string, T> map;


			internal CacheEntry(long ttl) {
				this.expiration = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + ttl * 1000L;
				this.map = new ConcurrentDictionary<string, T>();
			}
		}

		private class NegativeCacheEntry <T> : CacheEntry<T> {

			/// <param name="ttl"> </param>
			internal NegativeCacheEntry(long ttl) : base(ttl) {
			}

		}

		private static readonly Logger log = LoggerFactory.getLogger(typeof(DfsImpl));
		private const string DC_ENTRY = "dc";

		private CacheEntry<IDictionary<string, CacheEntry<DfsReferralDataInternal>>> _domains = null; /*
	                                                                                           * aka trusted domains cache
	                                                                                           */
		private readonly object domainsLock = new object();

		private readonly IDictionary<string, CacheEntry<DfsReferralDataInternal>> dcCache = new Dictionary<string, CacheEntry<DfsReferralDataInternal>>();
		private readonly object dcLock = new object();

		private CacheEntry<DfsReferralDataInternal> referrals = null;
		private readonly object referralsLock = new object();


		/// <param name="tc">
		///  </param>
		public DfsImpl(CIFSContext tc) {
		}


		/// throws SmbAuthException
		private IDictionary<string, IDictionary<string, CacheEntry<DfsReferralDataInternal>>> getTrustedDomains(CIFSContext tf) {
			if (tf.getConfig().isDfsDisabled() || tf.getCredentials().getUserDomain()==null || tf.getCredentials().getUserDomain().Length == 0) {
				return null;
			}

			if (this._domains != null && DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() > this._domains.expiration) {
				this._domains = null;
			}
			if (this._domains != null) {
				return this._domains.map;
			}
			try {
				string authDomain = tf.getCredentials().getUserDomain();
				// otherwise you end up with a wrong server name for kerberos
				// seems to be correct according to
				// https://lists.samba.org/archive/samba-technical/2009-August/066486.html
				// UniAddress addr = UniAddress.getByName(authDomain, true, tf);
				// SmbTransport trans = tf.getTransportPool().getSmbTransport(tf, addr, 0);
				using (SmbTransport dc = getDc(tf, authDomain)) {
					CacheEntry<IDictionary<string, CacheEntry<DfsReferralDataInternal>>> entry = new CacheEntry<IDictionary<string, CacheEntry<DfsReferralDataInternal>>>(tf.getConfig().getDfsTtl() * 10L);
					DfsReferralData initial = null;
					SmbTransportInternal trans = dc != null ? dc.unwrap<SmbTransportInternal>(typeof(SmbTransportInternal)) : null;
					if (trans != null) {
						// get domain referral
						initial = trans.getDfsReferrals(tf.withAnonymousCredentials(), "", trans.getRemoteHostName(), authDomain, 0);
					}
					if (initial != null) {
						DfsReferralDataInternal start = initial.unwrap<DfsReferralDataInternal>(typeof(DfsReferralDataInternal));
						DfsReferralDataInternal dr = start;
						do {
							string domain = dr.getServer().ToLower();
							entry.map[domain] = new Dictionary<string, CacheEntry<DfsReferralDataInternal>>();
							if (log.isTraceEnabled()) {
								log.trace("Inserting cache entry for domain " + domain + ": " + dr);
							}
							dr = dr.next();
						} while (dr != start);
						this._domains = entry;
						return this._domains.map;
					}
				}
			}
			catch (IOException ioe) {
				if (log.isDebugEnabled()) {
					log.debug("getting trusted domains failed: " + tf.getCredentials().getUserDomain(), ioe);
				}
				CacheEntry<IDictionary<string, CacheEntry<DfsReferralDataInternal>>> entry = new CacheEntry<IDictionary<string, CacheEntry<DfsReferralDataInternal>>>(tf.getConfig().getDfsTtl() * 10L);
				this._domains = entry;
				if (tf.getConfig().isDfsStrictView() && ioe is SmbAuthException) {
					throw (SmbAuthException) ioe;
				}
				return this._domains.map;
			}
			return null;
		}


		/// 
		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.DfsResolver#isTrustedDomain(jcifs.CIFSContext, java.lang.String) </seealso>
		/// throws SmbAuthException
		public virtual bool isTrustedDomain(CIFSContext tf, string domain) {
			lock (this.domainsLock) {
				IDictionary<string, IDictionary<string, CacheEntry<DfsReferralDataInternal>>> domains = getTrustedDomains(tf);
				if (domains == null) {
					return false;
				}
				domain = domain.ToLowerInvariant();
				return domains.get(domain) != null;
			}
		}


		/// throws SmbAuthException
		private DfsReferralData getDcReferrals(CIFSContext tf, string domain) {
			if (tf.getConfig().isDfsDisabled()) {
				return null;
			}
			string dom = domain.ToLowerInvariant();
			lock (this.dcLock) {
				CacheEntry<DfsReferralDataInternal> ce = this.dcCache.get(dom);
				if (ce != null && DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() > ce.expiration) {
					ce = null;
				}
				if (ce != null) {
					DfsReferralDataInternal ri = ce.map.get(DC_ENTRY);
					if (ri == NEGATIVE_ENTRY) {
						return null;
					}
					return ri;
				}
				ce = new CacheEntry<DfsReferralDataInternal>(tf.getConfig().getDfsTtl());
				try {
					using (SmbTransportInternal trans = tf.getTransportPool().getSmbTransport(tf, domain, 0, false, false).unwrap<SmbTransportInternal>(typeof(SmbTransportInternal))) {
						lock (trans) {
							DfsReferralData dr = trans.getDfsReferrals(tf.withAnonymousCredentials(), "\\" + dom, domain, dom, 1);

							if (dr != null) {
								if (log.isDebugEnabled()) {
									log.debug("Got DC referral " + dr);
								}
								DfsReferralDataInternal dri = dr.unwrap<DfsReferralDataInternal>(typeof(DfsReferralDataInternal));
								ce.map[DC_ENTRY] = dri;
								this.dcCache[dom] = ce;
								return dr;
							}
						}
					}
				}
				catch (IOException ioe) {
					if (log.isDebugEnabled()) {
						log.debug(string.Format("Getting domain controller for {0} failed", domain), ioe);
					}
					ce.map[DC_ENTRY] = NEGATIVE_ENTRY;
					if (tf.getConfig().isDfsStrictView() && ioe is SmbAuthException) {
						throw (SmbAuthException) ioe;
					}
				}
				ce.map[DC_ENTRY] = NEGATIVE_ENTRY;
				this.dcCache[dom] = ce;
				return null;
			}

		}


		/// 
		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.DfsResolver#getDc(jcifs.CIFSContext, java.lang.String) </seealso>
		/// throws SmbAuthException
		public virtual SmbTransport getDc(CIFSContext tf, string domain) {
			if (tf.getConfig().isDfsDisabled()) {
				return null;
			}
			SmbTransportImpl transport = getReferralTransport(tf, getDcReferrals(tf, domain));
			if (transport == null && log.isDebugEnabled()) {
				log.debug(string.Format("Failed to connect to domain controller for {0}", domain));
			}
			return transport;
		}


		/// throws SmbAuthException
		private static SmbTransportImpl getReferralTransport(CIFSContext tf, DfsReferralData dr) {
			try {
				if (dr != null) {
					DfsReferralData start = dr;
					IOException e = null;
					do {
						if (dr.getServer()!=null && dr.getServer().Length > 0) {
							try {
								SmbTransportImpl transport = tf.getTransportPool().getSmbTransport(tf, dr.getServer(), 0, false, !tf.getCredentials().isAnonymous() && tf.getConfig().isSigningEnabled() && tf.getConfig().isIpcSigningEnforced()).unwrap<SmbTransportImpl>(typeof(SmbTransportImpl));
								transport.ensureConnected();
								return transport;
							}
							catch (IOException ex) {
								log.debug("Connection failed " + dr.getServer(), ex);
								e = ex;
								dr = dr.next();
								continue;
							}
						}

						log.debug("No server name in referral");
						return null;
					} while (dr != start);
					throw e;
				}
			}
			catch (IOException ioe) {
				if (tf.getConfig().isDfsStrictView() && ioe is SmbAuthException) {
					throw (SmbAuthException) ioe;
				}
			}
			return null;
		}


		/// throws SmbAuthException
		protected internal virtual DfsReferralDataInternal getReferral(CIFSContext tf, SmbTransportInternal trans, string target, string targetDomain, string targetHost, string root, string path) {
			if (tf.getConfig().isDfsDisabled()) {
				return null;
			}

			string p = "\\" + target + "\\" + root;
			if (path != null) {
				p += path;
			}
			try {
				if (log.isDebugEnabled()) {
					log.debug("Fetching referral for " + p);
				}
				DfsReferralData dr = trans.getDfsReferrals(tf, p, targetHost, targetDomain, 0);
				if (dr != null) {

					if (log.isDebugEnabled()) {
						log.debug(string.Format("Referral for {0}: {1}", p, dr));
					}

					return dr.unwrap<DfsReferralDataInternal>(typeof(DfsReferralDataInternal));
				}
			}
			catch (IOException ioe) {
				if (log.isDebugEnabled()) {
					log.debug(string.Format("Getting referral for {0} failed", p), ioe);
				}
				if (tf.getConfig().isDfsStrictView() && ioe is SmbAuthException) {
					throw (SmbAuthException) ioe;
				}
			}
			return null;
		}


		/// 
		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.DfsResolver#resolve(jcifs.CIFSContext, java.lang.String, java.lang.String, java.lang.String) </seealso>
		/// throws SmbAuthException
		public virtual DfsReferralData resolve(CIFSContext tf, string domain, string root, string path) {
			return resolve(tf, domain, root, path, 5);
		}


		/// throws SmbAuthException
		private DfsReferralData resolve(CIFSContext tf, string domain, string root, string path, int depthLimit) {

			if (tf.getConfig().isDfsDisabled() ||root == null || root.Equals("IPC$") || depthLimit <= 0) {
				return null;
			}

			if (domain == null) {
				return null;
			}

			domain = domain.ToLower();

			if (log.isTraceEnabled()) {
				log.trace(string.Format("Resolving \\{0}\\{1}{2}", domain, root, path != null ? path : ""));
			}

			DfsReferralDataInternal dr = null;
			long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
			lock (this.domainsLock) {
				/*
				 * domains that can contain DFS points to maps of roots for each
				 */
				IDictionary<string, IDictionary<string, CacheEntry<DfsReferralDataInternal>>> domains = getTrustedDomains(tf);
				if (domains != null) {
					if (log.isTraceEnabled()) {
						dumpReferralCache(domains);
					}

					root = root.ToLower();
					/*
					 * domain-based DFS root shares to links for each
					 */
					IDictionary<string, CacheEntry<DfsReferralDataInternal>> roots = domains.get(domain);
					if (roots != null) {
						dr = getLinkReferral(tf, domain, root, path, now, roots);
					}

					if (tf.getConfig().isDfsConvertToFQDN() && dr is DfsReferralDataImpl) {
						((DfsReferralDataImpl) dr).fixupDomain(domain);
					}
				}
			}

			if (dr == null && path != null) {
				dr = getStandaloneCached(domain, root, path, now);
			}

			if (dr != null && dr.isIntermediate()) {
				dr = resolveIntermediates(tf, path, depthLimit, dr);
			}

			return dr;

		}


		/// <param name="tf"> </param>
		/// <param name="path"> </param>
		/// <param name="depthLimit"> </param>
		/// <param name="dr">
		/// @return </param>
		/// <exception cref="SmbAuthException"> </exception>
		/// throws SmbAuthException
		private DfsReferralDataInternal resolveIntermediates(CIFSContext tf, string path, int depthLimit, DfsReferralDataInternal dr) {
			DfsReferralDataInternal res = null;
			DfsReferralDataInternal start = dr;
			DfsReferralDataInternal r = start;
			do {
				r = start.next();
				string refPath = dr.getPath()!=null ? '\\' + dr.getPath() : "";
				string nextPath = refPath + (path != null ? path.Substring(r.getPathConsumed()) : "");
				if (log.isDebugEnabled()) {
					log.debug(string.Format("Intermediate referral, server {0} share {1} refPath {2} origPath {3} nextPath {4}", r.getServer(), r.getShare(), r.getPath(), path, nextPath));
				}
				DfsReferralData nextstart = resolve(tf, r.getServer(), r.getShare(), nextPath, depthLimit - 1);
				DfsReferralData next = nextstart;

				if (next != null) {
					do {
						if (log.isDebugEnabled()) {
							log.debug("Next referral is " + next);
						}
						if (res == null) {
							res = r.combine(next);
						}
						else {
							res.append(r.combine(next));
						}
					} while (next != nextstart);
				}
			} while (r != start);

			if (res != null) {
				return res;
			}

			return dr;
		}


		/// <param name="domains"> </param>
		/// <param name="domains"> </param>
		private static void dumpReferralCache(IDictionary<string, IDictionary<string, CacheEntry<DfsReferralDataInternal>>> domains)
		{
			foreach (var keyValuePair in domains)
			{
				log.trace("Domain " + keyValuePair.Key);
				var set = keyValuePair.Value;
				foreach (var cacheEntry in set)
				{
					log.trace("  Root " + cacheEntry.Key);
					var map = cacheEntry.Value.map;
					if (map != null)
					{
						foreach (var dfsReferralDataInternal in map)
						{
							DfsReferralDataInternal start = dfsReferralDataInternal.Value;
							DfsReferralDataInternal r = start;
							do
							{
								log.trace("    " + dfsReferralDataInternal.Key + " => " + dfsReferralDataInternal.Value);
								r = start.next();
							} while (r != start);
						}
					}
				}
			}
		}


		/// <param name="tf"> </param>
		/// <param name="domain"> </param>
		/// <param name="root"> </param>
		/// <param name="path"> </param>
		/// <param name="now"> </param>
		/// <param name="roots">
		/// @return </param>
		/// <exception cref="SmbAuthException"> </exception>
		/// throws SmbAuthException
		private DfsReferralDataInternal getLinkReferral(CIFSContext tf, string domain, string root, string path, long now, IDictionary<string, CacheEntry<DfsReferralDataInternal>> roots) {
			DfsReferralDataInternal dr = null;
			if (log.isTraceEnabled()) {
				log.trace("Is a domain referral for " + domain);
			}

			if (log.isTraceEnabled()) {
				log.trace("Resolving root " + root);
			}
			/*
			 * The link entries contain maps of referrals by path representing DFS links.
			 * Note that paths are relative to the root like "\" and not "\example.com\root".
			 */
			CacheEntry<DfsReferralDataInternal> links = roots.get(root);
			if (links != null && now > links.expiration) {
				if (log.isDebugEnabled()) {
					log.debug("Removing expired " + links.map);
				}
				roots.Remove(root);
				links = null;
			}

			if (links == null) {
				log.trace("Loadings roots");
				string refServerName = domain;
				dr = fetchRootReferral(tf, domain, root, refServerName);
				links = cacheRootReferral(tf, domain, root, roots, dr, links);
			}
			//TODO 1 type 
			else if (links is NegativeCacheEntry<DfsReferralDataInternal>){
				links = null;
			}
			else {
				dr = links.map.get("\\");
			}

			if (links != null) {
				return getLinkReferral(tf, domain, root, path, dr, now, links);
			}
			return dr;
		}


		/// <param name="tf"> </param>
		/// <param name="domain"> </param>
		/// <param name="root"> </param>
		/// <param name="roots"> </param>
		/// <param name="dr"> </param>
		/// <param name="links">
		/// @return </param>
		private static CacheEntry<DfsReferralDataInternal> cacheRootReferral(CIFSContext tf, string domain, string root, IDictionary<string, CacheEntry<DfsReferralDataInternal>> roots, DfsReferralDataInternal dr, CacheEntry<DfsReferralDataInternal> links) {
			if (dr != null) {
				links = new CacheEntry<DfsReferralDataInternal>(tf.getConfig().getDfsTtl());
				links.map["\\"] = dr;
				DfsReferralDataInternal tmp = dr;
				do {
					/*
					 * Store references to the map and key so that
					 * SmbFile.resolveDfs can re-insert the dr list with
					 * the dr that was successful so that subsequent
					 * attempts to resolve DFS use the last successful
					 * referral first.
					 */
					tmp.setCacheMap(links.map);
					tmp.setKey("\\");
					tmp = tmp.next();
				} while (tmp != dr);

				if (log.isDebugEnabled()) {
					log.debug("Have referral " + dr);
				}

				roots[root] = links;
			}
			else {
				roots[root] = new NegativeCacheEntry<DfsReferralDataInternal>(tf.getConfig().getDfsTtl());
			}
			return links;
		}


		/// <param name="tf"> </param>
		/// <param name="domain"> </param>
		/// <param name="root"> </param>
		/// <param name="refServerName">
		/// @return </param>
		/// <exception cref="SmbAuthException"> </exception>
		/// throws SmbAuthException
		private DfsReferralDataInternal fetchRootReferral(CIFSContext tf, string domain, string root, string refServerName) {
			DfsReferralDataInternal dr;
			using (SmbTransport dc = getDc(tf, domain)) {
				if (dc == null) {
					if (log.isDebugEnabled()) {
						log.debug("Failed to get domain controller for " + domain);
					}
					return null;
				}

				SmbTransportInternal trans = dc.unwrap<SmbTransportInternal>(typeof(SmbTransportInternal));
				// the tconHostName is from the DC referral, that referral must be resolved
				// before following deeper ones. Otherwise e.g. samba will return a broken
				// referral.
				lock (trans) {
					try {
						// ensure connected
						trans.ensureConnected();
						refServerName = trans.getRemoteHostName();
					}
					catch (IOException e) {
						log.warn("Failed to connect to domain controller", e);
					}
					dr = getReferral(tf, trans, domain, domain, refServerName, root, null);
				}
			}

			if (log.isTraceEnabled()) {
				log.trace("Have DC referral " + dr);
			}

			if (dr != null && domain.Equals(dr.getServer()) && root.Equals(dr.getShare())) {
				// If we do cache these we never get to the properly cached
				// standalone referral we might have.
				log.warn("Dropping self-referential referral " + dr);
				dr = null;
			}
			return dr;
		}


		/// <param name="tf"> </param>
		/// <param name="domain"> </param>
		/// <param name="root"> </param>
		/// <param name="path"> </param>
		/// <param name="rootDr"> </param>
		/// <param name="now"> </param>
		/// <param name="links">
		/// @return </param>
		/// <exception cref="SmbAuthException"> </exception>
		/// throws SmbAuthException
		private DfsReferralDataInternal getLinkReferral(CIFSContext tf, string domain, string root, string path, DfsReferralDataInternal rootDr, long now, CacheEntry<DfsReferralDataInternal> links) {
			DfsReferralDataInternal dr = rootDr;
			string link;

			if (path == null || path.Length <= 1) {
				/*
				 * Lookup the domain based DFS root target referral. Note the
				 * path is just "\" and not "\example.com\root".
				 */
				link = "\\";
			}
			else if (path[path.Length - 1] == '\\') {
				// strip trailing slash
				link = path.Substring(0, path.Length - 1);
			}
			else {
				link = path;
			}

			if (log.isTraceEnabled()) {
				log.trace("Initial link is " + link);
			}

			if (dr == null || !link.Equals(dr.getLink())) {
				while (true) {
					dr = links.map.get(link);

					if (dr != null) {
						if (log.isTraceEnabled()) {
							log.trace("Found at " + link);
						}
						break;
					}

					// walk up trying to find a match, do not go up to the root
					int nextSep = link.LastIndexOf('\\');
					if (nextSep > 0) {
						link = link.Substring(0, nextSep);
					}
					else {
						if (log.isTraceEnabled()) {
							log.trace("Not found " + link);
						}
						break;
					}
				}
			}

			if (dr != null && now > dr.getExpiration()) {
				if (log.isTraceEnabled()) {
					log.trace("Expiring links " + link);
				}
				links.map.Remove(link);
				dr = null;
			}

			if (dr == null) {
				using (SmbTransportInternal trans = getReferralTransport(tf, rootDr)) {
					if (trans == null) {
						return null;
					}

					dr = getReferral(tf, trans, domain, domain, trans.getRemoteHostName(), root, path);
					if (dr != null) {

						if (tf.getConfig().isDfsConvertToFQDN() && dr is DfsReferralDataImpl) {
							((DfsReferralDataImpl) dr).fixupDomain(domain);
						}

						dr.stripPathConsumed(1 + domain.Length + 1 + root.Length);

						if (dr.getPathConsumed() > (path != null ? path.Length : 0)) {
							log.error("Consumed more than we provided");
						}

						link = path != null && dr.getPathConsumed() > 0 ? path.Substring(0, dr.getPathConsumed()) : "\\";
						dr.setLink(link);
						if (log.isTraceEnabled()) {
							log.trace("Have referral " + dr);
						}
						links.map[link] = dr;
					}
					else {
						log.debug("No referral found for " + link);
					}
				}
			}
			else if (log.isTraceEnabled()) {
				log.trace("Have cached referral for " + dr.getLink() + " " + dr);
			}
			return dr;
		}


		/// <param name="domain"> </param>
		/// <param name="root"> </param>
		/// <param name="path"> </param>
		/// <param name="now">
		/// @return </param>
		private DfsReferralDataInternal getStandaloneCached(string domain, string root, string path, long now) {
			if (log.isTraceEnabled()) {
				log.trace("No match for domain based root, checking standalone " + domain);
			}
			/*
			 * We did not match a domain based root. Now try to match the
			 * longest path in the list of stand-alone referrals.
			 */

			CacheEntry<DfsReferralDataInternal> refs;
			lock (this.referralsLock) {
				refs = this.referrals;
				if (refs == null || now > refs.expiration) {
					refs = new CacheEntry<DfsReferralDataInternal>(0);
				}
				this.referrals = refs;
			}
			string key = "\\" + domain + "\\" + root;
			if (!path.Equals("\\")) {
				key += path;
			}

			key = key.ToLowerInvariant();

			IEnumerator<string> iter = refs.map.Keys.GetEnumerator();
			int searchLen = key.Length;
			while (iter.MoveNext()) {
				string cachedKey = iter.Current;
				int cachedKeyLen = cachedKey.Length;

				bool match = false;
				if (cachedKeyLen == searchLen) {
					match = cachedKey.Equals(key);
				}
				else if (cachedKeyLen < searchLen) {
					match = key.StartsWith(cachedKey, StringComparison.Ordinal);
				}
				else if (log.isTraceEnabled()) {
					log.trace(key + " vs. " + cachedKey);
				}

				if (match) {
					if (log.isDebugEnabled()) {
						log.debug("Matched " + cachedKey);
					}
					return refs.map.get(cachedKey);
				}
			}
			if (log.isTraceEnabled()) {
				log.trace("No match for " + key);
			}
			return null;
		}


		public virtual void cache(CIFSContext tc, string path, DfsReferralData dr) {
			lock (this) {
				if (tc.getConfig().isDfsDisabled() || !(dr is DfsReferralDataInternal)) {
					return;
				}
        
				if (log.isDebugEnabled()) {
					log.debug("Inserting referral for " + path);
				}
        
				int s1 = path.IndexOf('\\', 1);
				int s2 = path.IndexOf('\\', s1 + 1);
        
				if (s1 < 0 || s2 < 0) {
					log.error("Invalid UNC path " + path);
					return;
				}
        
				string server = path.Substring(1, s1 - 1).ToLowerInvariant();
				string share = path.Substring(s1 + 1, s2 - (s1 + 1));
				string key = path.Substring(0, dr.getPathConsumed()).ToLowerInvariant();
        
				DfsReferralDataInternal dri = (DfsReferralDataInternal) dr;
        
				if (tc.getConfig().isDfsConvertToFQDN()) {
					dri.fixupHost(server);
				}
        
				if (log.isDebugEnabled()) {
					log.debug("Adding key " + key + " to " + dr);
				}
        
				/*
				 * Subtract the server and share from the pathConsumed so that
				 * it reflects the part of the relative path consumed and not
				 * the entire path.
				 */
				dri.stripPathConsumed(1 + server.Length + 1 + share.Length);
        
				if (key[key.Length - 1] != '\\') {
					key += '\\';
				}
        
				if (log.isDebugEnabled()) {
					log.debug("Key is " + key);
				}
        
				CacheEntry<DfsReferralDataInternal> refs = this.referrals;
				lock (this.referralsLock) {
					if (refs == null || (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + 10000) > refs.expiration) {
						refs = new CacheEntry<DfsReferralDataInternal>(tc.getConfig().getDfsTtl());
					}
					this.referrals = refs;
				}
				refs.map[key] = dri;
			}
		}
	}

}