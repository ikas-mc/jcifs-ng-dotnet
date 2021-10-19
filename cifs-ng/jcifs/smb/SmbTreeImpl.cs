using jcifs;
using jcifs.@internal;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using cifs_ng.lib.ext;
using cifs_ng.lib.threading;
using Logger = org.slf4j.Logger;
using LoggerFactory = org.slf4j.LoggerFactory;
using CIFSContext = jcifs.CIFSContext;
using CIFSException = jcifs.CIFSException;
using DfsReferralData = jcifs.DfsReferralData;
using DialectVersion = jcifs.DialectVersion;
using RuntimeCIFSException = jcifs.RuntimeCIFSException;
using SmbConstants = jcifs.SmbConstants;
using SmbTree = jcifs.SmbTree;
using CommonServerMessageBlockRequest = jcifs.@internal.CommonServerMessageBlockRequest;
using CommonServerMessageBlockResponse = jcifs.@internal.CommonServerMessageBlockResponse;
using RequestWithPath = jcifs.@internal.RequestWithPath;
using SmbNegotiationResponse = jcifs.@internal.SmbNegotiationResponse;
using TreeConnectResponse = jcifs.@internal.TreeConnectResponse;
using ServerMessageBlock = jcifs.@internal.smb1.ServerMessageBlock;
using SmbComBlankResponse = jcifs.@internal.smb1.com.SmbComBlankResponse;
using SmbComNegotiateResponse = jcifs.@internal.smb1.com.SmbComNegotiateResponse;
using SmbComTreeConnectAndX = jcifs.@internal.smb1.com.SmbComTreeConnectAndX;
using SmbComTreeConnectAndXResponse = jcifs.@internal.smb1.com.SmbComTreeConnectAndXResponse;
using SmbComTreeDisconnect = jcifs.@internal.smb1.com.SmbComTreeDisconnect;
using SmbComTransaction = jcifs.@internal.smb1.trans.SmbComTransaction;
using Trans2FindFirst2 = jcifs.@internal.smb1.trans2.Trans2FindFirst2;
using Trans2FindFirst2Response = jcifs.@internal.smb1.trans2.Trans2FindFirst2Response;
using ServerMessageBlock2 = jcifs.@internal.smb2.ServerMessageBlock2;
using Smb2IoctlRequest = jcifs.@internal.smb2.ioctl.Smb2IoctlRequest;
using Smb2IoctlResponse = jcifs.@internal.smb2.ioctl.Smb2IoctlResponse;
using ValidateNegotiateInfoRequest = jcifs.@internal.smb2.ioctl.ValidateNegotiateInfoRequest;
using ValidateNegotiateInfoResponse = jcifs.@internal.smb2.ioctl.ValidateNegotiateInfoResponse;
using Smb2NegotiateRequest = jcifs.@internal.smb2.nego.Smb2NegotiateRequest;
using Smb2NegotiateResponse = jcifs.@internal.smb2.nego.Smb2NegotiateResponse;
using Smb2TreeConnectRequest = jcifs.@internal.smb2.tree.Smb2TreeConnectRequest;
using Smb2TreeDisconnectRequest = jcifs.@internal.smb2.tree.Smb2TreeDisconnectRequest;

/* jcifs smb client library in Java
 * Copyright (C) 2000  "Michael B. Allen" <jcifs at samba dot org>
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

	internal class SmbTreeImpl : SmbTreeInternal {

		private static readonly Logger log = LoggerFactory.getLogger(typeof(SmbTreeImpl));

		//TODO 
		private static AtomicInteger TREE_CONN_COUNTER = new AtomicInteger();

		/*
		 * 0 - not connected
		 * 1 - connecting
		 * 2 - connected
		 * 3 - disconnecting
		 */
		private readonly AtomicInteger connectionState = new AtomicInteger();

		private readonly string share;
		private readonly string service0;
		private readonly SmbSessionImpl session;

		private volatile int tid = -1;
		private volatile string service = "?????";
		private volatile bool inDfs, inDomainDfs;
		//TODO 
		private volatile int treeNum; // used by SmbFile.isOpen

		private readonly AtomicLong usageCount = new AtomicLong(0);
		private readonly AtomicBoolean sessionAcquired = new AtomicBoolean(true);

		private readonly bool traceResource;
		private readonly LinkedList<StackFrame[]> acquires;
		private readonly LinkedList<StackFrame[]> releases;

		private DfsReferralData treeReferral;


		internal SmbTreeImpl(SmbSessionImpl session, string share, string service) {
			this.session = session.acquire();
			this.share = share.ToUpper();
			if (!string.IsNullOrEmpty(service) && !service.StartsWith("??", StringComparison.Ordinal)) {
				this.service = service;
			}
			this.service0 = this.service;

			this.traceResource = this.session.getConfig().isTraceResourceUsage();
			if (this.traceResource) {
				this.acquires = new LinkedList<StackFrame[]>();
				this.releases = new LinkedList<StackFrame[]>();
			}
			else {
				this.acquires = null;
				this.releases = null;
			}
		}


		internal virtual bool matches(string shr, string servc) {
			return this.share.Equals(shr, StringComparison.OrdinalIgnoreCase) && (servc== null || servc.StartsWith("??", StringComparison.Ordinal) || this.service.Equals(servc, StringComparison.OrdinalIgnoreCase));
		}


		public override bool Equals(object obj) {
			if (obj is SmbTreeImpl) {
				SmbTreeImpl tree = (SmbTreeImpl) obj;
				return matches(tree.share, tree.service);
			}
			return false;
		}


		public virtual SmbTreeImpl acquire() {
			return acquire(true);
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.SmbTree#unwrap(java.lang.Class) </seealso>
		public virtual T unwrap<T>(Type type) where T : SmbTree {
			if (this is T) {
				return (T) (object)this;
			}
			throw new System.InvalidCastException();
		}


		/// <param name="track"> </param>
		/// <returns> tree with increased usage count </returns>
		public virtual SmbTreeImpl acquire(bool track) {
			long usage = this.usageCount.IncrementValueAndReturn();
			if (log.isTraceEnabled()) {
				log.trace("Acquire tree " + usage + " " + this);
			}

			if (track && this.traceResource) {
				lock (this.acquires) {
					StackTrace st = new System.Diagnostics.StackTrace();
					this.acquires.AddLast(truncateTrace(st.GetFrames()));
				}
			}

			if (usage == 1) {
				lock (this) {
					if (this.sessionAcquired.CompareAndSet(false, true)) {
						log.debug("Reacquire session");
						this.session.acquire();
					}
				}
			}
			return this;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= java.lang.AutoCloseable#Dispose() </seealso>
		public virtual void Dispose() {
			release(false);
		}


		public virtual void release() {
			release(true);
		}


		/// <param name="track"> </param>
		public virtual void release(bool track) {
			long usage = this.usageCount.DecrementValueAndReturn();
			if (log.isTraceEnabled()) {
				log.trace("Release tree " + usage + " " + this);
			}

			if (track && this.traceResource) {
				lock (this.releases) {
					StackTrace st = new System.Diagnostics.StackTrace();
					this.releases.AddLast(truncateTrace(st.GetFrames()));
				}
			}

			if (usage == 0) {
				lock (this) {
					log.debug("Usage dropped to zero, release session");
					if (this.sessionAcquired.CompareAndSet(true, false)) {
						this.session.release();
					}
				}
			}
			else if (usage < 0) {
				log.error("Usage count dropped below zero " + this);
				dumpResource();
				throw new RuntimeCIFSException("Usage count dropped below zero");
			}
		}


		/// <param name="stackTrace">
		/// @return </param>
		private static StackFrame[] truncateTrace(StackFrame[] stackTrace) {

			int s = 2;
			int e = stackTrace.Length;

			for (int i = s; i < e; i++) {
				StackFrame se = stackTrace[i];

				var method = se.GetMethod();
				if (null == method)
				{
					continue;
				}
				string callerClassNameWithNamespace = method.DeclaringType?.FullName;
				if (i == s && object.Equals( typeof(SmbTreeImpl).FullName,callerClassNameWithNamespace) && "close".Equals( method.Name)) {
					s++;
					continue;
				}

				// if (callerClassNameWithNamespace.StartsWith("org.junit.runners.")) {
				// 	e = i - 4;
				// 	break;
				// }
			}

			StackFrame[] res = new StackFrame[e - s];
			Array.Copy(stackTrace, s, res, 0, e - s);
			return res;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= java.lang.Object#finalize() </seealso>
		/// throws Throwable
		~SmbTreeImpl() {
			if (isConnected() && this.usageCount.Value != 0) {
				log.warn("Tree was not properly released");
			}
		}


		/// 
		/// <returns> whether the tree is connected </returns>
		public virtual bool isConnected() {
			return this.tid != -1 && this.session.isConnected() && this.connectionState.Value == 2;
		}


		/// <returns> the type of this tree </returns>
		public virtual int getTreeType() {
			string connectedService = getService();
			if ("LPT1:".Equals(connectedService)) {
				return SmbConstants.TYPE_PRINTER;
			}
			else if ("COMM".Equals(connectedService)) {
				return SmbConstants.TYPE_COMM;
			}
			return SmbConstants.TYPE_SHARE;
		}


		/// <returns> the service </returns>
		public virtual string getService() {
			return this.service;
		}


		/// <returns> the share </returns>
		public virtual string getShare() {
			return this.share;
		}


		/// <returns> whether this is a DFS share </returns>
		public virtual bool isDfs() {
			return this.inDfs;
		}


		/// 
		internal virtual void markDomainDfs() {
			this.inDomainDfs = true;
		}


		/// <returns> whether this tree was accessed using domain DFS </returns>
		public virtual bool isInDomainDfs() {
			return this.inDomainDfs;
		}


		/// <param name="referral"> </param>
		public virtual void setTreeReferral(DfsReferralData referral) {
			this.treeReferral = referral;
		}


		/// <returns> the treeReferral </returns>
		public virtual DfsReferralData getTreeReferral() {
			return this.treeReferral;
		}


		/// <returns> whether this tree may be a DFS share </returns>
		/// <exception cref="SmbException"> </exception>
		/// throws SmbException
		public virtual bool isPossiblyDfs() {
			if (this.connectionState.Value == 2) {
				// we are connected, so we know
				return isDfs();
			}
			using (SmbTransportImpl transport = (SmbTransportImpl)this.session.getTransport()) {
				return transport.getNegotiateResponse().isDFSSupported();
			}
		}


		/// <returns> the session this tree is connected in </returns>
		public virtual SmbSessionImpl getSession() {
			return this.session.acquire();
		}


		/// <returns> the tid </returns>
		public virtual int getTid() {
			return this.tid;
		}


		/// <returns> the tree_num (monotonically increasing counter to track reconnects) </returns>
		public virtual long getTreeNum() {
			return this.treeNum;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= java.lang.Object#hashCode() </seealso>
		public override int GetHashCode() {
			return this.share.GetHashCode() + 7 * this.service.GetHashCode();
		}


		/// throws jcifs.CIFSException
		public virtual T send<T>(Request<T> request, params RequestParam[] @params) where T : CommonServerMessageBlockResponse
		{
			ISet<RequestParam>  sets = null == @params ? new HashSet<RequestParam>(0) : @params.ToHashSet();
			return send((CommonServerMessageBlockRequest) request, default(T), sets);
		}


		/// throws jcifs.CIFSException
		internal virtual T send<T>(CommonServerMessageBlockRequest request, T response) where T : CommonServerMessageBlockResponse {
			return send(request, response, new HashSet<RequestParam>(0));
		}


		/// throws jcifs.CIFSException
		internal virtual T send<T>(CommonServerMessageBlockRequest request, T response, ISet<RequestParam> @params) where T : CommonServerMessageBlockResponse {
			using (SmbSessionImpl sess = getSession())
			using (SmbTransportImpl transport = (SmbTransportImpl)sess.getTransport()) {
				if (response != null) {
					response.clearReceived();
				}

				// try TreeConnectAndX with the request
				// this does not make any sense if we are disconnecting right now
				T chainedResponse = default;
				if (!(request is SmbComTreeDisconnect) && !(request is Smb2TreeDisconnectRequest)) {
					chainedResponse = treeConnect(request, response);
				}
				if (request == null || (chainedResponse != null && chainedResponse.isReceived())) {
					return chainedResponse;
				}

				// fall trough if the tree connection is already established
				// and send it as a separate request instead
				string svc = null;
				int t = this.tid;
				request.setTid(t);

				if (!transport.isSMB2()) {
					ServerMessageBlock req = (ServerMessageBlock) request;
					svc = this.service;
					if (svc== null) {
						// there still is some kind of race condition, where?
						// this used to trigger "invalid operation..."
						throw new SmbException("Service is null in state " + this.connectionState.Value);
					}
					checkRequest(transport, req, svc);

				}

				if (this.isDfs() && !"IPC".Equals(svc) && !"IPC$".Equals(this.share) && request is RequestWithPath) {
					/*
					 * When DFS is in action all request paths are
					 * full UNC paths minus the first backslash like
					 * \server\share\path\to\file
					 * as opposed to normally
					 * \path\to\file
					 */
					RequestWithPath preq = (RequestWithPath) request;
					if (!string.IsNullOrEmpty(preq.getPath())) {
						if (log.isDebugEnabled()) {
							log.debug(string.Format("Setting DFS request path from {0} to {1}", preq.getPath(), preq.getFullUNCPath()));
						}
						preq.setResolveInDfs(true);
						preq.setPath(preq.getFullUNCPath());
					}
				}

				try {
					return sess.send(request, response, @params);
				}
				catch (SmbException se) {
					if (se.getNtStatus() == NtStatus.NT_STATUS_NETWORK_NAME_DELETED) {
						/*
						 * Someone removed the share while we were
						 * connected. Bastards! Disconnect this tree
						 * so that it reconnects cleanly should the share
						 * reappear in this client's lifetime.
						 */
						log.debug("Disconnect tree on NT_STATUS_NETWORK_NAME_DELETED");
						treeDisconnect(true, true);
					}
					throw se;
				}
			}
		}


		/// <param name="transport"> </param>
		/// <param name="request"> </param>
		/// <exception cref="SmbException"> </exception>
		/// throws SmbException
		private static void checkRequest(SmbTransportImpl transport, ServerMessageBlock request, string svc) {
			if (!"A:".Equals(svc)) {
				switch (request.getCommand()) {
				case ServerMessageBlock.SMB_COM_OPEN_ANDX:
				case ServerMessageBlock.SMB_COM_NT_CREATE_ANDX:
				case ServerMessageBlock.SMB_COM_READ_ANDX:
				case ServerMessageBlock.SMB_COM_WRITE_ANDX:
				case ServerMessageBlock.SMB_COM_CLOSE:
				case ServerMessageBlock.SMB_COM_TREE_DISCONNECT:
					break;
				case ServerMessageBlock.SMB_COM_TRANSACTION:
				case ServerMessageBlock.SMB_COM_TRANSACTION2:
					switch (((SmbComTransaction) request).getSubCommand() & 0xFF) {
					case SmbComTransaction.NET_SHARE_ENUM:
					case SmbComTransaction.NET_SERVER_ENUM2:
					case SmbComTransaction.NET_SERVER_ENUM3:
					case SmbComTransaction.TRANS_PEEK_NAMED_PIPE:
					case SmbComTransaction.TRANS_WAIT_NAMED_PIPE:
					case SmbComTransaction.TRANS_CALL_NAMED_PIPE:
					case SmbComTransaction.TRANS_TRANSACT_NAMED_PIPE:
					case SmbComTransaction.TRANS2_GET_DFS_REFERRAL:
						break;
					default:
						throw new SmbException("Invalid operation for " + svc + " service: " + request);
					}
					break;
				default:
					throw new SmbException("Invalid operation for " + svc + " service" + request);
				}
			}
		}


		/// throws jcifs.CIFSException
		internal virtual T treeConnect<T>(CommonServerMessageBlockRequest andx, T andxResponse) where T : CommonServerMessageBlockResponse {
			CommonServerMessageBlockRequest request = null;
			TreeConnectResponse response = null;
			using (SmbSessionImpl sess = getSession())
			using (SmbTransportImpl transport = (SmbTransportImpl)sess.getTransport()) {
				lock (transport) {

					// this needs to be done before the reference to the remote hostname later
					transport.ensureConnected();

					if (waitForState(transport) == 2) {
						// already connected
						return default;
					}
					int before = this.connectionState.Exchange(1);
					if (before == 1) {
						// concurrent connection attempt
						if (waitForState(transport) == 2) {
							// finished connecting
							return default;
						}
						// failure to connect
						throw new SmbException("Tree disconnected while waiting for connection");
					}
					else if (before == 2) {
						// concurrently connected
						return default;
					}

					if (log.isDebugEnabled()) {
						log.debug("Connection state was " + before);
					}

					try {
						/*
						 * The hostname to use in the path is only known for
						 * sure if the NetBIOS session has been successfully
						 * established.
						 */

						string tconHostName = sess.getTargetHost();

						if (tconHostName==null) {
							throw new SmbException("Transport disconnected while waiting for connection");
						}

						SmbNegotiationResponse nego = transport.getNegotiateResponse();

						string unc = "\\\\" + tconHostName + '\\' + this.share;

						/*
						 * IBM iSeries doesn't like specifying a service. Always reset
						 * the service to whatever was determined in the constructor.
						 */
						string svc = this.service0;

						/*
						 * Tree Connect And X Request / Response
						 */

						if (log.isDebugEnabled()) {
							log.debug("treeConnect: unc=" + unc + ",service=" + svc);
						}

						if (transport.isSMB2()) {
							Smb2TreeConnectRequest req = new Smb2TreeConnectRequest(sess.getConfig(), unc);
							if (andx != null) {
								req.chain((ServerMessageBlock2) andx);
							}
							request = req;
						}
						else {
							if (andxResponse!=null&&!(andxResponse is ServerMessageBlock))
							{
								//TODO 
								throw new SmbException("smb1 error");
							}
							response = new SmbComTreeConnectAndXResponse(sess.getConfig(), andxResponse as ServerMessageBlock) ;
							request = new SmbComTreeConnectAndX(sess.getContext(), ((SmbComNegotiateResponse) nego).getServerData(), unc, svc, (ServerMessageBlock) andx);
						}
						
						//TODO 1
						response = sess.send(request, response);
						treeConnected(transport, sess, response);

						if (andxResponse != null && andxResponse.isReceived()) {
							return andxResponse;
						}
						else if (transport.isSMB2()) {
							return (T) response.getNextResponse();
						}
						return default;
					}
					catch (IOException se) {
						if (request != null && ((CommonServerMessageBlock)request).getResponse() != null) {
							// tree connect might still have succeeded
							response = (TreeConnectResponse) ((CommonServerMessageBlock)request).getResponse();
							if (response.isReceived() && !response.isError() && response.getErrorCode() == NtStatus.NT_STATUS_OK) {
								if (!transport.isDisconnected()) {
									treeConnected(transport, sess, response);
								}
								throw se;
							}
						}
						try {
							log.debug("Disconnect tree on treeConnectFailure", se);
							treeDisconnect(true, true);
						}
						finally {
							this.connectionState.Value=(0);
						}
						throw se;
					}
					finally {
						Monitor.PulseAll(transport);
					}
				}
			}
		}


		/// <param name="transport"> </param>
		/// <param name="sess"> </param>
		/// <param name="response"> </param>
		/// <exception cref="IOException"> </exception>
		/// throws jcifs.CIFSException
		private void treeConnected(SmbTransportImpl transport, SmbSessionImpl sess, TreeConnectResponse response) {
			if (!response.isValidTid()) {
				throw new SmbException("TreeID is invalid");
			}
			this.tid = response.getTid();
			string rsvc = response.getService();
			if (rsvc==null && !transport.isSMB2()) {
				throw new SmbException("Service is NULL");
			}

			if (transport.getContext().getConfig().isIpcSigningEnforced() && ("IPC$".Equals(this.getShare()) || "IPC".Equals(rsvc)) && !sess.getCredentials().isAnonymous() && sess.getDigest() == null) {
				throw new SmbException("IPC signing is enforced, but no signing is available");
			}

			this.service = rsvc;
			this.inDfs = response.isShareDfs();
			this.treeNum = TREE_CONN_COUNTER.IncrementValueAndReturn();
			this.connectionState.Value=2; // connected

			try {
				validateNegotiation(transport, sess);
			}
			catch (CIFSException se) {
				try {
					transport.disconnect(true);
				}
				catch (IOException e) {
					log.warn("Failed to disconnect transport", e);
					//se.addSuppressed(e);
				}
				throw se;
			}
		}


		/// <param name="trans"> </param>
		/// <param name="sess"> </param>
		/// <exception cref="CIFSException">
		///  </exception>
		/// throws jcifs.CIFSException
		private void validateNegotiation(SmbTransportImpl trans, SmbSessionImpl sess) {
			if (!trans.isSMB2() || trans.getDigest() == null || !sess.getConfig().isRequireSecureNegotiate()) {
				log.debug("Secure negotiation does not apply");
				return;
			}

			Smb2NegotiateResponse nego = (Smb2NegotiateResponse) trans.getNegotiateResponse();
			if (nego.getSelectedDialect().atLeast(DialectVersion.SMB311)) {
				// have preauth integrity instead
				log.debug("Secure negotiation does not apply, is SMB3.1");
				return;
			}
			Smb2NegotiateRequest negoReq = new Smb2NegotiateRequest(sess.getConfig(), trans.getRequestSecurityMode(nego));

			log.debug("Sending VALIDATE_NEGOTIATE_INFO");
			Smb2IoctlRequest req = new Smb2IoctlRequest(sess.getConfig(), Smb2IoctlRequest.FSCTL_VALIDATE_NEGOTIATE_INFO);
			req.setFlags(Smb2IoctlRequest.SMB2_O_IOCTL_IS_FSCTL);
			req.setInputData(new ValidateNegotiateInfoRequest(negoReq.getCapabilities(), negoReq.getClientGuid(), (short) negoReq.getSecurityMode(), negoReq.getDialects()));

			Smb2IoctlResponse resp;
			try {
				resp = send(req, RequestParam.NO_RETRY);
			}
			catch (SMBSignatureValidationException e) {
				throw new SMBProtocolDowngradeException("Signature error during negotiate validation", e);
			}
			catch (SmbException e) {
				if (log.isDebugEnabled()) {
					log.debug(string.Format("VALIDATE_NEGOTIATE_INFO response code 0x{0:x}", e.getNtStatus()));
				}
				log.trace("VALIDATE_NEGOTIATE_INFO returned error", e);
				if ((req.getResponse().isReceived() && req.getResponse().isVerifyFailed()) || e.getNtStatus() == NtStatus.NT_STATUS_ACCESS_DENIED) {
					// this is the signature error
					throw new SMBProtocolDowngradeException("Signature error during negotiate validation", e);
				}

				// other errors are treated as success
				return;
			}
			ValidateNegotiateInfoResponse @out = resp.getOutputData<ValidateNegotiateInfoResponse>(typeof(ValidateNegotiateInfoResponse));

			if (nego.getSecurityMode() != @out.getSecurityMode() || nego.getCapabilities() != @out.getCapabilities() || nego.getDialectRevision() != @out.getDialect() || !nego.getServerGuid().SequenceEqual(@out.getServerGuid())) {
				log.debug("Secure negotiation failure");
				throw new CIFSException("Mismatched attributes validating negotiate info");
			}

			log.debug("Secure negotiation OK");
		}


		/// <param name="transport">
		/// @return </param>
		/// <exception cref="SmbException"> </exception>
		/// throws SmbException
		private int waitForState(SmbTransportImpl transport) {
			int cs;
			while ((cs = this.connectionState.Value) != 0) {
				if (cs == 2) {
					return cs;
				}
				if (cs == 3) {
					throw new SmbException("Disconnecting during tree connect");
				}
				try {
					log.debug("Waiting for transport");
					Monitor.Wait(transport);
				}
				catch (ThreadInterruptedException ie) {
					throw new SmbException(ie.Message, ie);
				}
			}
			return cs;
		}


		/// 
		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.smb.SmbTreeInternal#connectLogon(jcifs.CIFSContext) </seealso>
		/// throws SmbException
		[Obsolete]
		public virtual void connectLogon(CIFSContext tf) {
			if (tf.getConfig().getLogonShare()==null) {
				try {
					treeConnect((CommonServerMessageBlockRequest)null, (CommonServerMessageBlockResponse)null);
				}
				catch (SmbException e) {
					throw e;
				}
				catch (CIFSException e) {
					throw SmbException.wrap(e);
				}
			}
			else {
				Trans2FindFirst2 req = new Trans2FindFirst2(tf.getConfig(), "\\", "*", SmbConstants.ATTR_DIRECTORY, tf.getConfig().getListCount(), tf.getConfig().getListSize());
				Trans2FindFirst2Response resp = new Trans2FindFirst2Response(tf.getConfig());
				try {
					send(req, resp);
				}
				catch (SmbException e) {
					throw e;
				}
				catch (CIFSException e) {
					throw new SmbException("Logon share connection failed", e);
				}
			}
		}


		internal virtual bool treeDisconnect(bool inError, bool inUse) {
			bool wasInUse = false;
			using (SmbSessionImpl sess = getSession())
			using (	SmbTransportImpl transport = (SmbTransportImpl)sess.getTransport()) {
				lock (transport) {
					int st = this.connectionState.Exchange(3);
					if (st == 2) {
						long l = this.usageCount.Value;
						if ((inUse && l != 1) || (!inUse && l > 0)) {
							log.warn("Disconnected tree while still in use " + this);
							dumpResource();
							wasInUse = true;
							if (sess.getConfig().isTraceResourceUsage()) {
								throw new RuntimeCIFSException("Disconnected tree while still in use");
							}
						}

						if (!inError && this.tid != -1) {
							try {
								if (transport.isSMB2()) {
									Smb2TreeDisconnectRequest req = new Smb2TreeDisconnectRequest(sess.getConfig());
									send((Smb2TreeDisconnectRequest)req.ignoreDisconnect());
								}
								else {
									send(new SmbComTreeDisconnect(sess.getConfig()), new SmbComBlankResponse(sess.getConfig()));
								}
							}
							catch (CIFSException se) {
								log.error("Tree disconnect failed", se);
							}
						}
					}
					this.inDfs = false;
					this.inDomainDfs = false;
					this.connectionState.Value=(0);
					Monitor.PulseAll(transport);
				}
			}
			return wasInUse;
		}


		/// 
		private void dumpResource() {
			if (!this.traceResource) {
				return;
			}

			lock (this.acquires) {
				foreach (StackFrame[] acq in this.acquires) {
					log.debug("Acquire " + acq?.joinToString());
				}
			}

			lock (this.releases) {
				foreach (StackFrame[] rel in this.releases) {
					log.debug("Release " + rel?.joinToString());
				}
			}
		}


		public override string ToString() {
			return "SmbTree[share=" + this.share + ",service=" + this.service + ",tid=" + this.tid + ",inDfs=" + this.inDfs + ",inDomainDfs=" + this.inDomainDfs + ",connectionState=" + this.connectionState + ",usage=" + this.usageCount.Value + "]";
		}

	}

}