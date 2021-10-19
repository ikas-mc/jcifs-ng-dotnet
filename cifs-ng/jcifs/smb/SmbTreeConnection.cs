using jcifs.@internal;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using cifs_ng.lib;
using cifs_ng.lib.socket;
using cifs_ng.lib.threading;
using Logger = org.slf4j.Logger;
using LoggerFactory = org.slf4j.LoggerFactory;
using CIFSContext = jcifs.CIFSContext;
using CIFSException = jcifs.CIFSException;
using Configuration = jcifs.Configuration;
using DfsReferralData = jcifs.DfsReferralData;
using RuntimeCIFSException = jcifs.RuntimeCIFSException;
using SmbConstants = jcifs.SmbConstants;
using SmbResourceLocator = jcifs.SmbResourceLocator;
using SmbTreeHandle = jcifs.SmbTreeHandle;
using CommonServerMessageBlockRequest = jcifs.@internal.CommonServerMessageBlockRequest;
using CommonServerMessageBlockResponse = jcifs.@internal.CommonServerMessageBlockResponse;
using RequestWithPath = jcifs.@internal.RequestWithPath;
using DfsReferralDataInternal = jcifs.@internal.dfs.DfsReferralDataInternal;
using SmbComClose = jcifs.@internal.smb1.com.SmbComClose;
using SmbComFindClose2 = jcifs.@internal.smb1.com.SmbComFindClose2;
using NtTransQuerySecurityDesc = jcifs.@internal.smb1.trans.nt.NtTransQuerySecurityDesc;
using TransportException = jcifs.util.transport.TransportException;

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





	/// <summary>
	/// This class encapsulates the logic for switching tree connections
	/// 
	/// Switching trees can occur either when the tree has been disconnected by failure or idle-timeout - as well as on
	/// DFS referrals.
	/// 
	/// @author mbechler
	/// 
	/// </summary>
	internal class SmbTreeConnection {

		private static readonly Logger log = LoggerFactory.getLogger(typeof(SmbTreeConnection));

		private readonly CIFSContext ctx;
		private readonly SmbTreeConnection @delegate;
		private SmbTreeImpl tree;
		private volatile bool treeAcquired;
		private volatile bool delegateAcquired;

		private SmbTransportInternal exclusiveTransport;
		private bool nonPooled;

		private readonly AtomicLong usageCount = new AtomicLong();

		private static readonly Random RAND = new Random();


		protected internal SmbTreeConnection(CIFSContext ctx) {
			this.ctx = ctx;
			this.@delegate = null;
		}


		protected internal SmbTreeConnection(SmbTreeConnection treeConnection) {
			this.ctx = treeConnection.ctx;
			this.@delegate = treeConnection;
		}


		internal static SmbTreeConnection create(CIFSContext c) {
			if (c.getConfig().isTraceResourceUsage()) {
				return new SmbTreeConnectionTrace(c);
			}
			return new SmbTreeConnection(c);
		}


		internal static SmbTreeConnection create(SmbTreeConnection c) {
			if (c.ctx.getConfig().isTraceResourceUsage()) {
				return new SmbTreeConnectionTrace(c);
			}
			return new SmbTreeConnection(c);
		}


		/// <returns> the active configuration </returns>
		public virtual Configuration getConfig() {
			return this.ctx.getConfig();
		}


		private SmbTreeImpl getTree() {
			lock (this) {
				SmbTreeImpl t = this.tree;
				if (t != null) {
					return t.acquire(false);
				}
				else if (this.@delegate != null) {
					this.tree = this.@delegate.getTree();
					return this.tree;
				}
				return t;
			}
		}


		/// <summary>
		/// @return
		/// </summary>
		private SmbTreeImpl getTreeInternal() {
			lock (this) {
				SmbTreeImpl t = this.tree;
				if (t != null) {
					return t;
				}
				if (this.@delegate != null) {
					return this.@delegate.getTreeInternal();
				}
				return null;
			}
		}


		/// <param name="t"> </param>
		private void switchTree(SmbTreeImpl t) {
			lock (this) {
				using (SmbTreeImpl old = getTree()) {
					if (old == t) {
						return;
					}
					bool wasAcquired = this.treeAcquired;
					log.debug("Switching tree");
					if (t != null) {
						log.debug("Acquired tree on switch " + t);
						t.acquire();
						this.treeAcquired = true;
					}
					else {
						this.treeAcquired = false;
					}
        
					this.tree = t;
					if (old != null) {
						if (wasAcquired) {
							// release
							old.release(true);
						}
					}
					if (this.@delegate != null && this.delegateAcquired) {
						log.debug("Releasing delegate");
						this.delegateAcquired = false;
						this.@delegate.release();
					}
				}
			}
		}


		/// <returns> tree connection with increased usage count </returns>
		public virtual SmbTreeConnection acquire() {
			long usage = this.usageCount.IncrementValueAndReturn();
			if (log.isTraceEnabled()) {
				log.trace("Acquire tree connection " + usage + " " + this);
			}

			if (usage == 1) {
				lock (this) {
					using (SmbTreeImpl t = getTree()) {
						if (t != null) {
							if (!this.treeAcquired) {
								if (log.isDebugEnabled()) {
									log.debug("Acquire tree on first usage " + t);
								}
								t.acquire();
								this.treeAcquired = true;
							}
						}
					}
					if (this.@delegate != null && !this.delegateAcquired) {
						log.debug("Acquire delegate on first usage");
						this.@delegate.acquire();
						this.delegateAcquired = true;
					}
				}
			}

			return this;

		}


		/// 
		public virtual void release() {
			long usage = this.usageCount.DecrementValueAndReturn();
			if (log.isTraceEnabled()) {
				log.trace("Release tree connection " + usage + " " + this);
			}

			if (usage == 0) {
				lock (this) {
					using (SmbTreeImpl t = getTree()) {
						if (this.treeAcquired && t != null) {
							if (log.isDebugEnabled()) {
								log.debug("Tree connection no longer in use, release tree " + t);
							}
							this.treeAcquired = false;
							t.release();
						}
					}
					if (this.@delegate != null && this.delegateAcquired) {
						this.delegateAcquired = false;
						this.@delegate.release();
					}
				}

				SmbTransportInternal et = this.exclusiveTransport;
				if (et != null) {
					lock (this) {
						try {
							log.debug("Disconnecting exclusive transport");
							this.exclusiveTransport = null;
							this.tree = null;
							this.treeAcquired = false;
							et.Dispose();
							et.disconnect(false, false);
						}
						catch (Exception e) {
							log.error("Failed to close exclusive transport", e);
						}
					}
				}
			}
			else if (usage < 0) {
				log.error("Usage count dropped below zero " + this);
				throw new RuntimeCIFSException("Usage count dropped below zero");
			}
		}


		protected internal virtual void checkRelease() {
			if (isConnected() && this.usageCount.Value != 0) {
				log.warn("Tree connection was not properly released " + this);
			}
		}


		internal virtual void disconnect(bool inError) {
			lock (this) {
				using (SmbSessionImpl session = getSession()) {
					if (session == null) {
						return;
					}
					using (SmbTransportImpl transport = (SmbTransportImpl)session.getTransport()) {
						lock (transport) {
							SmbTreeImpl t = getTreeInternal();
							if (t != null) {
								try {
									t.treeDisconnect(inError, true);
								}
								finally {
									this.tree = null;
									this.treeAcquired = false;
								}
							}
							else {
								this.@delegate.disconnect(inError);
							}
						}
					}
				}
			}
		}


		/// throws jcifs.CIFSException
		internal virtual T send<T>(SmbResourceLocatorImpl loc, CommonServerMessageBlockRequest request, T response, params RequestParam[] @params) where T : CommonServerMessageBlockResponse {
			return send(loc, request, response, @params.Length == 0 ? new HashSet<RequestParam>(0):@params.ToHashSet());
		}


		/// throws jcifs.CIFSException
		internal virtual T send<T>(SmbResourceLocatorImpl loc, CommonServerMessageBlockRequest request, T response, ISet<RequestParam> @params) where T : CommonServerMessageBlockResponse {
			CIFSException last = null;
			RequestWithPath rpath = (request is RequestWithPath) ? (RequestWithPath) request : null;
			string savedPath = rpath != null ? rpath.getPath() : null;
			string savedFullPath = rpath != null ? rpath.getFullUNCPath() : null;

			string fullPath = "\\" + loc.getServer() + "\\" + loc.getShare() + loc.getUNCPath();
			int maxRetries = this.ctx.getConfig().getMaxRequestRetries();
			for (int retries = 1; retries <= maxRetries; retries++) {

				if (rpath != null) {
					rpath.setFullUNCPath(null, null, fullPath);
				}

				try {
					return send0(loc, request, response, @params);
				}
				catch (SmbException smbe) {
					// Retrying only makes sense if the invalid parameter is an tree id. If we have a stale file descriptor
					// retrying make no sense, as it will never become available again.
					if (@params.Contains(RequestParam.NO_RETRY) || (!(smbe.InnerException is TransportException)) && smbe.getNtStatus() != NtStatus.NT_STATUS_INVALID_PARAMETER) {
						log.debug("Not retrying", smbe);
						throw smbe;
					}
					log.debug("send", smbe);
					last = smbe;
				}
				catch (CIFSException e) {
					log.debug("send", e);
					last = e;
				}
				// If we get here, we got the 'The Parameter is incorrect' error or a transport exception
				// Disconnect and try again from scratch.

				if (log.isDebugEnabled()) {
					log.debug(string.Format("Retrying ({0:D}/{1:D}) request {2}", retries, maxRetries, request));
				}

				// should we disconnect the transport here? otherwise we make an additional attempt to detect that if the
				// server closed the connection as a result
				log.debug("Disconnecting tree on send retry", last);
				disconnect(true);

				if (retries >= maxRetries) {
					break;
				}

				try {
					if (retries != 1) {
						// backoff, but don't delay the first attempt as there are various reasons that can be fixed
						// immediately
						Thread.Sleep(500 + RAND.Next(1000));
					}
				}
				catch (ThreadInterruptedException e) {
					log.debug("interrupted sleep in send", e);
				}

				if (request != null) {
					log.debug("Restting request");
					request.reset();
				}
				if (rpath != null) {
					// resolveDfs() and tree.send() modify the request packet.
					// I want to restore it before retrying. request.reset()
					// restores almost everything that was modified, except the path.
					rpath.setPath(savedPath);
					rpath.setFullUNCPath(rpath.getDomain(), rpath.getServer(), savedFullPath);
				}
				if (response != null) {
					//TODO
					((CommonServerMessageBlock)response).reset();
				}

				try {
						using (SmbTreeHandle th = connectWrapException(loc)) {
						log.debug("Have new tree connection for retry");
						}
				}
				catch (SmbException e) {
					log.debug("Failed to connect tree on retry", e);
					last = e;
				}
			}

			if (last != null) {
				log.debug("All attempts have failed, last exception", last);
				throw last;
			}
			throw new SmbException("All attempts failed, but no exception");
		}


		/// throws CIFSException, DfsReferral
		private T send0<T>(SmbResourceLocatorImpl loc, CommonServerMessageBlockRequest request, T response, ISet<RequestParam> @params) where T : CommonServerMessageBlockResponse {
			for (int limit = 10; limit > 0; limit--) {
				if (request is RequestWithPath) {
					ensureDFSResolved(loc, (RequestWithPath) request);
				}
				try {
						using (SmbTreeImpl t = getTree()) {
						if (t == null) {
							throw new CIFSException("Failed to get tree connection");
						};
						return t.send(request, response, @params);
						}
				}
				catch (DfsReferral dre) {
					if (dre.getData().unwrap<DfsReferralDataInternal>(typeof(DfsReferralDataInternal)).isResolveHashes()) {
						throw dre;
					}
					request.reset();
					log.trace("send0", dre);
				}
			}

			throw new CIFSException("Loop in DFS referrals");
		}


		/// <param name="loc"> </param>
		/// <returns> tree handle </returns>
		/// <exception cref="SmbException"> </exception>
		/// throws SmbException
		public virtual SmbTreeHandleImpl connectWrapException(SmbResourceLocatorImpl loc) {
			try {
				return connect(loc);
			}
			catch (UnknownHostException uhe) {
				throw new SmbException("Failed to connect to server", uhe);
			}
			catch (SmbException se) {
				throw se;
			}
			catch (IOException ioe) {
				throw new SmbException("Failed to connect to server", ioe);
			}
		}


		/// <param name="loc"> </param>
		/// <returns> tree handle </returns>
		/// <exception cref="IOException"> </exception>
		/// throws java.io.IOException
		public virtual SmbTreeHandleImpl connect(SmbResourceLocatorImpl loc) {
			lock (this) {
				using (SmbSessionImpl session = getSession()) {
					if (isConnected()) {
						using (SmbTransportImpl transport = (SmbTransportImpl)session.getTransport()) {
							if (transport.isDisconnected() || transport.getRemoteHostName()== null) {
								/*
								 * Tree/session thinks it is connected but transport disconnected
								 * under it, reset tree to reflect the truth.
								 */
								log.debug("Disconnecting failed tree and session");
								disconnect(true);
							}
						}
					}
        
					if (isConnected()) {
						log.trace("Already connected");
						return new SmbTreeHandleImpl(loc, this);
					}
        
					return connectHost(loc, loc.getServerWithDfs());
				}
        
			}
		}


		/// <returns> whether we have a valid tree connection </returns>
		public virtual bool isConnected() {
			lock (this) {
				SmbTreeImpl t = getTreeInternal();
				return t != null && t.isConnected();
			}
		}


		/// 
		/// <param name="loc"> </param>
		/// <param name="host"> </param>
		/// <returns> tree handle </returns>
		/// <exception cref="IOException"> </exception>
		/// throws java.io.IOException
		public virtual SmbTreeHandleImpl connectHost(SmbResourceLocatorImpl loc, string host) {
			lock (this) {
				return connectHost(loc, host, null);
			}
		}


		/// 
		/// <param name="loc"> </param>
		/// <param name="host"> </param>
		/// <param name="referral"> </param>
		/// <returns> tree handle </returns>
		/// <exception cref="IOException"> </exception>
		/// throws java.io.IOException
		public virtual SmbTreeHandleImpl connectHost(SmbResourceLocatorImpl loc, string host, DfsReferralData referral) {
			lock (this) {
				string targetDomain = null;
				using (SmbTreeImpl t = getTree()) {
					if (t != null) {
						if (log.isDebugEnabled()) {
							log.debug("Tree is " + t);
						}
        
						if (Equals(loc.getShare(), t.getShare())) {
							using (SmbSessionImpl session = t.getSession()) {
								targetDomain = session.getTargetDomain();
								if (!session.isFailed())
								{
									using (SmbTransportImpl trans = (SmbTransportImpl)session.getTransport())
									using (	SmbTreeImpl ct = connectTree(loc, host, t.getShare(), trans, t, null)) {
										switchTree(ct);
										return new SmbTreeHandleImpl(loc, this);
									}
								}
								log.debug("Session no longer valid");
							}
						}
					}
				}
        
				string hostName = loc.getServerWithDfs();
				string path = (loc.getType() == SmbConstants.TYPE_SHARE || loc.getUNCPath()== null || "\\".Equals(loc.getUNCPath())) ? null : loc.getUNCPath();
				string share = loc.getShare();
        
				DfsReferralData start = referral != null ? referral : this.ctx.getDfs().resolve(this.ctx, hostName, loc.getShare(), path);
				DfsReferralData dr = start;
				IOException last = null;
				do {
					if (dr != null) {
						targetDomain = dr.getDomain();
						host = dr.getServer().ToLowerInvariant();
						share = dr.getShare();
					}
        
					try {
        
						if (this.nonPooled) {
							if (log.isDebugEnabled()) {
								log.debug("Using exclusive transport for " + this);
							}
							this.exclusiveTransport = this.ctx.getTransportPool().getSmbTransport(this.ctx, host, loc.getPort(), true, loc.shouldForceSigning()).unwrap<SmbTransportInternal>(typeof(SmbTransportInternal));
							SmbTransportInternal trans = this.exclusiveTransport;
							using (SmbSessionInternal smbSession = trans.getSmbSession(this.ctx, host, targetDomain).unwrap<SmbSessionInternal>(typeof(SmbSessionInternal)))
							using (	SmbTreeImpl uct = smbSession.getSmbTree(share, null).unwrap<SmbTreeImpl>(typeof(SmbTreeImpl)))
							using (SmbTreeImpl ct = connectTree(loc, host, share, trans, uct, dr)) {
        
								if (dr != null) {
									ct.setTreeReferral(dr);
									if (dr != start) {
										dr.unwrap<DfsReferralDataInternal>(typeof(DfsReferralDataInternal)).replaceCache();
									}
								}
								switchTree(ct);
								return new SmbTreeHandleImpl(loc, this);
							}
						}
        
						using (SmbTransportInternal trans = this.ctx.getTransportPool().getSmbTransport(this.ctx, host, loc.getPort(), false, loc.shouldForceSigning()).unwrap<SmbTransportInternal>(typeof(SmbTransportInternal)))
						using (	SmbSessionInternal smbSession = trans.getSmbSession(this.ctx, host, targetDomain).unwrap<SmbSessionInternal>(typeof(SmbSessionInternal)))
						using (	SmbTreeImpl uct = smbSession.getSmbTree(share, null).unwrap<SmbTreeImpl>(typeof(SmbTreeImpl)))
						using (SmbTreeImpl ct = connectTree(loc, host, share, trans, uct, dr)) {
							if (dr != null) {
								ct.setTreeReferral(dr);
								if (dr != start) {
									dr.unwrap<DfsReferralDataInternal>(typeof(DfsReferralDataInternal)).replaceCache();
								}
							}
							switchTree(ct);
							return new SmbTreeHandleImpl(loc, this);
						}
					}
					catch (IOException e) {
						last = e;
						log.debug("Referral failed, trying next", e);
					}
        
					if (dr != null) {
						dr = dr.next();
					}
				} while (dr != start);
				throw last;
			}
		}


		/// <param name="loc"> </param>
		/// <param name="addr"> </param>
		/// <param name="trans"> </param>
		/// <param name="t"> </param>
		/// <exception cref="CIFSException"> </exception>
		/// throws jcifs.CIFSException
		private SmbTreeImpl connectTree(SmbResourceLocatorImpl loc, string addr, string share, SmbTransportInternal trans, SmbTreeImpl t, DfsReferralData referral) {
			if (log.isDebugEnabled() && trans.isSigningOptional() && !loc.isIPC() && !this.ctx.getConfig().isSigningEnforced()) {
				log.debug("Signatures for file enabled but not required " + this);
			}

			if (referral != null) {
				t.markDomainDfs();
			}

			try {
				if (log.isTraceEnabled()) {
					log.trace("doConnect: " + addr);
				}
				t.treeConnect(null, (CommonServerMessageBlockResponse)null);
				return t.acquire();
			}
			catch (SmbAuthException sae) {
				log.debug("Authentication failed", sae);
				return retryAuthentication(loc, share, trans, t, referral, sae);
			}
		}


		/// throws SmbAuthException, jcifs.CIFSException
		private SmbTreeImpl retryAuthentication(SmbResourceLocatorImpl loc, string share, SmbTransportInternal trans, SmbTreeImpl t, DfsReferralData referral, SmbAuthException sae) {
			using (SmbSessionImpl treesess = t.getSession()) {
				if (treesess.getCredentials().isAnonymous() || treesess.getCredentials().isGuest()) {
					// refresh anonymous session or fallback to anonymous from guest login
					try {
							using (SmbSessionInternal s = trans.getSmbSession(this.ctx.withAnonymousCredentials(), treesess.getTargetHost(), treesess.getTargetDomain()).unwrap<SmbSessionInternal>(typeof(SmbSessionInternal)))
							using (	SmbTreeImpl tr = s.getSmbTree(share, null).unwrap<SmbTreeImpl>(typeof(SmbTreeImpl))) 
							{
							tr.treeConnect(null, (CommonServerMessageBlockResponse)null);
							log.debug("Anonymous retry succeeded");
							return tr.acquire();
							}
					}
					catch (Exception e) {
						log.debug("Retry also failed", e);
						throw sae;
					}
				}
				else if (this.ctx.renewCredentials(loc.getURL().ToString(), sae)) {
					log.debug("Trying to renew credentials after auth error");
					using (SmbSessionInternal s = trans.getSmbSession(this.ctx, treesess.getTargetHost(), treesess.getTargetDomain()).unwrap<SmbSessionInternal>(typeof(SmbSessionInternal)))
					using (	SmbTreeImpl tr = s.getSmbTree(share, null).unwrap<SmbTreeImpl>(typeof(SmbTreeImpl))) {
						if (referral != null) {
							tr.markDomainDfs();
						}
						tr.treeConnect(null, (CommonServerMessageBlockResponse)null);
						return tr.acquire();
					}
				}
				else {
					throw sae;
				}
			}
		}


		/// throws jcifs.CIFSException
		internal virtual SmbResourceLocator ensureDFSResolved(SmbResourceLocatorImpl loc) {
			return ensureDFSResolved(loc, null);
		}


		/// throws jcifs.CIFSException
		internal virtual SmbResourceLocator ensureDFSResolved(SmbResourceLocatorImpl loc, RequestWithPath request) {
			if (request is SmbComClose) {
				return loc;
			}

			for (int retries = 0; retries < 1 + this.ctx.getConfig().getMaxRequestRetries(); retries++) {
				try {
					return resolveDfs0(loc, request);
				}
				catch (SmbException smbe) {
					// The connection may have been dropped?
					if (smbe.getNtStatus() != NtStatus.NT_STATUS_NOT_FOUND && !(smbe.InnerException is TransportException)) {
						throw smbe;
					}
					log.debug("resolveDfs", smbe);
				}
				// If we get here, we apparently have a bad connection.
				// Disconnect and try again.
				if (log.isDebugEnabled()) {
					log.debug("Retrying (" + retries + ") resolveDfs: " + request);
				}
				log.debug("Disconnecting tree on DFS retry");
				disconnect(true);
				try {
					Thread.Sleep(500 + RAND.Next(5000));
				}
				catch (ThreadInterruptedException e) {
					log.debug("resolveDfs", e);
				}

				using (SmbTreeHandle th = connectWrapException(loc)) {
				}
			}

			return loc;
		}


		/// throws jcifs.CIFSException
		private SmbResourceLocator resolveDfs0(SmbResourceLocatorImpl loc, RequestWithPath request) {
			using (SmbTreeHandleImpl th = connectWrapException(loc))
			using (	SmbSessionImpl session = (SmbSessionImpl)th.getSession())
			using (	SmbTransportImpl transport = (SmbTransportImpl)session.getTransport())
			using (	SmbTreeImpl t = getTree()) {
				transport.ensureConnected();

				string rpath = request != null ? request.getPath() : loc.getUNCPath();
				string rfullpath = request != null ? request.getFullUNCPath() : ('\\' + loc.getServer() + '\\' + loc.getShare() + loc.getUNCPath());
				if (t.isInDomainDfs() || !t.isPossiblyDfs()) {
					if (t.isInDomainDfs()) {
						// need to adjust request path
						DfsReferralData dr1 = t.getTreeReferral();
						if (dr1 != null) {
							if (log.isDebugEnabled()) {
								log.debug(string.Format("Need to adjust request path {0} (full: {1}) -> {2}", rpath, rfullpath, dr1));
							}
							string dunc = loc.handleDFSReferral(dr1, rpath);
							if (request != null) {
								request.setPath(dunc);
							}
							return loc;
						}

						// fallthrough to normal handling
						log.debug("No tree referral but in DFS");
					}
					else {
						log.trace("Not in DFS");
						return loc;
					}
				}

				if (request != null) {
					request.setFullUNCPath(session.getTargetDomain(), session.getTargetHost(), rfullpath);
				}

				// for standalone DFS we could be checking for a referral here, too
				DfsReferralData dr = this.ctx.getDfs().resolve(this.ctx, loc.getServer(), loc.getShare(), loc.getUNCPath());
				if (dr != null) {
					if (log.isDebugEnabled()) {
						log.debug("Resolved " + rfullpath + " -> " + dr);
					}

					string dunc = loc.handleDFSReferral(dr, rpath);
					if (request != null) {
						request.setPath(dunc);
					}

					if (!t.getShare().Equals(dr.getShare())) {
						// this should only happen for standalone roots or if the DC/domain root lookup failed
						IOException last;
						DfsReferralData start = dr;
						do {
							if (log.isDebugEnabled()) {
								log.debug("Need to switch tree for " + dr);
							}
							try {
									using (SmbTreeHandleImpl nt = connectHost(loc, session.getTargetHost(), dr)) {
									log.debug("Switched tree");
									return loc;
									}
							}
							catch (IOException e) {
								log.debug("Failed to connect tree", e);
								last = e;
							}
							dr = dr.next();
						} while (dr != start);
						throw new CIFSException("All referral tree connections failed", last);
					}

					return loc;
				}
				else if (t.isInDomainDfs() && !(request is NtTransQuerySecurityDesc) && !(request is SmbComClose) && !(request is SmbComFindClose2)) {
					if (log.isDebugEnabled()) {
						log.debug("No referral available for  " + rfullpath);
					}
					throw new CIFSException("No referral but in domain DFS " + rfullpath);
				}
				else {
					log.trace("Not in DFS");
					return loc;
				}
			}
		}


		/// <summary>
		/// Use a exclusive connection for this tree
		/// 
		/// If an exclusive connection is used the caller must make sure that the tree handle is kept alive,
		/// otherwise the connection will be disconnected once the usage drops to zero.
		/// </summary>
		/// <param name="np">
		///            whether to use an exclusive connection </param>
		internal virtual void setNonPooled(bool np) {
			this.nonPooled = np;
		}


		/// <returns> the currently connected tid </returns>
		public virtual long getTreeId() {
			SmbTreeImpl t = getTreeInternal();
			if (t == null) {
				return -1;
			}
			return t.getTreeNum();
		}


		/// 
		/// <summary>
		/// Only call this method while holding a tree handle
		/// </summary>
		/// <returns> session that this file has been loaded through </returns>
		public virtual SmbSessionImpl getSession() {
			SmbTreeImpl t = getTreeInternal();
			if (t != null) {
				return t.getSession();
			}
			return null;
		}


		/// 
		/// <summary>
		/// Only call this method while holding a tree handle
		/// </summary>
		/// <param name="cap"> </param>
		/// <returns> whether the capability is available </returns>
		/// <exception cref="SmbException"> </exception>
		/// throws SmbException
		public virtual bool hasCapability(int cap) {
			using (SmbSessionImpl s = getSession()) {
				if (s != null) {
					using (SmbTransportImpl transport = (SmbTransportImpl)s.getTransport()) {
						return transport.hasCapability(cap);
					}
				}
				throw new SmbException("Not connected");
			}
		}


		/// <summary>
		/// Only call this method while holding a tree handle
		/// </summary>
		/// <returns> the connected tree type </returns>
		public virtual int getTreeType() {
			using (SmbTreeImpl t = getTree()) {
				return t.getTreeType();
			}
		}


		/// 
		/// <summary>
		/// Only call this method while holding a tree handle
		/// </summary>
		/// <returns> the share we are connected to </returns>
		public virtual string getConnectedShare() {
			using (SmbTreeImpl t = getTree()) {
				return t.getShare();
			}
		}


		/// 
		/// <summary>
		/// Only call this method while holding a tree handle
		/// </summary>
		/// <param name="other"> </param>
		/// <returns> whether the connection refers to the same tree </returns>
		public virtual bool isSame(SmbTreeConnection other) {
			using (SmbTreeImpl t1 = getTree())
			using (	SmbTreeImpl t2 = other.getTree()) {
				return t1 == t2;
			}
		}

	}

}