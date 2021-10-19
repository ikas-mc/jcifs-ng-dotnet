using jcifs;
using jcifs.@internal;
using jcifs.smb;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using cifs_ng.lib;
using cifs_ng.lib.ext;
using cifs_ng.lib.security;
using cifs_ng.lib.threading;
using Logger = org.slf4j.Logger;
using LoggerFactory = org.slf4j.LoggerFactory;
using CIFSContext = jcifs.CIFSContext;
using CIFSException = jcifs.CIFSException;
using Configuration = jcifs.Configuration;
using DialectVersion = jcifs.DialectVersion;
using RuntimeCIFSException = jcifs.RuntimeCIFSException;
using SmbConstants = jcifs.SmbConstants;
using SmbSession = jcifs.SmbSession;
using CommonServerMessageBlock = jcifs.@internal.CommonServerMessageBlock;
using CommonServerMessageBlockRequest = jcifs.@internal.CommonServerMessageBlockRequest;
using CommonServerMessageBlockResponse = jcifs.@internal.CommonServerMessageBlockResponse;
using RequestWithPath = jcifs.@internal.RequestWithPath;
using SMBSigningDigest = jcifs.@internal.SMBSigningDigest;
using SMB1SigningDigest = jcifs.@internal.smb1.SMB1SigningDigest;
using ServerMessageBlock = jcifs.@internal.smb1.ServerMessageBlock;
using SmbComBlankResponse = jcifs.@internal.smb1.com.SmbComBlankResponse;
using SmbComLogoffAndX = jcifs.@internal.smb1.com.SmbComLogoffAndX;
using SmbComNegotiateResponse = jcifs.@internal.smb1.com.SmbComNegotiateResponse;
using SmbComSessionSetupAndX = jcifs.@internal.smb1.com.SmbComSessionSetupAndX;
using SmbComSessionSetupAndXResponse = jcifs.@internal.smb1.com.SmbComSessionSetupAndXResponse;
using SmbComTreeConnectAndX = jcifs.@internal.smb1.com.SmbComTreeConnectAndX;
using ServerMessageBlock2 = jcifs.@internal.smb2.ServerMessageBlock2;
using jcifs.@internal.smb2;
using Org.BouncyCastle.Security;
using Smb2Constants = jcifs.@internal.smb2.Smb2Constants;
using Smb2SigningDigest = jcifs.@internal.smb2.Smb2SigningDigest;
using Smb2NegotiateResponse = jcifs.@internal.smb2.nego.Smb2NegotiateResponse;
using Smb2LogoffRequest = jcifs.@internal.smb2.session.Smb2LogoffRequest;
using Smb2SessionSetupRequest = jcifs.@internal.smb2.session.Smb2SessionSetupRequest;
using Smb2SessionSetupResponse = jcifs.@internal.smb2.session.Smb2SessionSetupResponse;
using Hexdump = jcifs.util.Hexdump;

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

	/// 
	 internal sealed class SmbSessionImpl : SmbSessionInternal {

		private static readonly Logger log = LoggerFactory.getLogger(typeof(SmbSessionImpl));

		/*
		 * 0 - not connected
		 * 1 - connecting
		 * 2 - connected
		 * 3 - disconnecting
		 */
		private readonly AtomicInteger connectionState = new AtomicInteger();
		private int uid;
		private IList<SmbTreeImpl> trees;

		private readonly SmbTransportImpl transport;
		private long expiration;
		private string netbiosName = null;

		private CIFSContext transportContext;

		private CredentialsInternal credentials;
		private byte[] sessionKey;
		private bool extendedSecurity;

		private readonly AtomicLong usageCount = new AtomicLong(1);
		private readonly AtomicBoolean transportAcquired = new AtomicBoolean(true);

		private long sessionId;

		private SMBSigningDigest digest;

		private readonly string targetDomain;
		private readonly string targetHost;

		private byte[] preauthIntegrityHash;


		internal SmbSessionImpl(CIFSContext tf, string targetHost, string targetDomain, SmbTransportImpl transport) {
			this.transportContext = tf;
			this.targetDomain = targetDomain;
			this.targetHost = targetHost;
			this.transport = (SmbTransportImpl)transport.acquire();
			this.trees = new List<SmbTreeImpl>();
			this.credentials = (CredentialsInternal)tf.getCredentials().unwrap<CredentialsInternal>(typeof(CredentialsInternal)).Clone();
		}


		/// <returns> the configuration used by this session </returns>
		public Configuration getConfig() {
			return this.transportContext.getConfig();
		}


		/// <returns> the targetDomain </returns>
		public string getTargetDomain() {
			return this.targetDomain;
		}


		/// <returns> the targetHost </returns>
		public string getTargetHost() {
			return this.targetHost;
		}


		/// <returns> whether the session is in use </returns>
		public bool isInUse() {
			return this.usageCount.Value > 0;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.SmbSession#unwrap(java.lang.Class) </seealso>
		public T unwrap<T>(Type type) where T : SmbSession {
			if (this is T v) {
				return v;
			}
			throw new System.InvalidCastException();
		}


		/// <returns> session increased usage count </returns>
		public SmbSessionImpl acquire() {
			long usage = this.usageCount.IncrementValueAndReturn();
			if (log.isTraceEnabled()) {
				log.trace("Acquire session " + usage + " " + this);
			}

			if (usage == 1) {
				lock (this) {
					if (this.transportAcquired.CompareAndSet(false, true)) {
						log.debug("Reacquire transport");
						this.transport.acquire();
					}
				}
			}

			return this;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= java.lang.Object#finalize() </seealso>
		/// throws Throwable
		~SmbSessionImpl() {
			if (isConnected() && this.usageCount.Value != 0) {
				log.warn("Session was not properly released");
			}
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= java.lang.AutoCloseable#Dispose() </seealso>
		public void Dispose() {
			release();
		}


		/// 
		public void release() {
			long usage = this.usageCount.DecrementValueAndReturn();
			if (log.isTraceEnabled()) {
				log.trace("Release session " + usage + " " + this);
			}

			if (usage == 0) {
				if (log.isDebugEnabled()) {
					log.debug("Usage dropped to zero, release connection " + this.transport);
				}
				lock (this) {
					if (this.transportAcquired.CompareAndSet(true, false)) {
						this.transport.release();
					}
				}
			}
			else if (usage < 0) {
				throw new RuntimeCIFSException("Usage count dropped below zero");
			}
		}


		/// <returns> the sessionKey </returns>
		/// <exception cref="CIFSException"> </exception>
		/// throws jcifs.CIFSException
		public byte[] getSessionKey() {
			if (this.sessionKey == null) {
				throw new CIFSException("No session key available");
			}
			return this.sessionKey;
		}


		public SmbTree getSmbTree(string share, string service) {
			if (share == null) {
				share = "IPC$";
			}

			lock (this.trees) {
				foreach (SmbTreeImpl t1 in this.trees) {
					if (t1.matches(share, service)) {
						return t1.acquire();
					}
				}
				SmbTreeImpl t = new SmbTreeImpl(this, share, service);
				t.acquire();
				this.trees.Add(t);
				return t;
			}
		}


		/// <summary>
		/// Establish a tree connection with the configured logon share
		/// </summary>
		/// <exception cref="SmbException"> </exception>
		public void treeConnectLogon() {
			string logonShare = getContext().getConfig().getLogonShare();
			if (logonShare == null || logonShare.Length == 0) {
				throw new SmbException("Logon share is not defined");
			}
			try {
					using (SmbTreeImpl t = (SmbTreeImpl)getSmbTree(logonShare, null)) {
						t.treeConnect<CommonServerMessageBlockResponse>(null, null);
					}
			}
			catch (CIFSException e) {
				throw SmbException.wrap(e);
			}
		}


		internal bool isSignatureSetupRequired() {
			SMBSigningDigest cur = getDigest();
			if (cur != null) {
				return false;
			}
			else if (this.transport.isSigningEnforced()) {
				return true;
			}
			return this.transport.getNegotiateResponse().isSigningNegotiated();
		}


		/// <param name="digest">
		///            the digest to set </param>
		/// <exception cref="SmbException"> </exception>
		private void setDigest(SMBSigningDigest digest) {
			if (this.transport.isSMB2()) {
				this.digest = digest;
			}
			else {
				this.transport.setDigest(digest);
			}
		}


		/// <returns> the digest </returns>
		/// <exception cref="SmbException"> </exception>
		public SMBSigningDigest getDigest() {
			if (this.digest != null) {
				return this.digest;
			}
			return this.transport.getDigest();
		}


		/// <param name="tf"> </param>
		/// <param name="tdom"> </param>
		/// <param name="thost">
		/// @return </param>
		internal bool matches(CIFSContext tf, string thost, string tdom) {
			return Equals(this.getCredentials(), tf.getCredentials()) && Equals(this.targetHost, thost) && Equals(this.targetDomain, tdom);
		}


		/// throws jcifs.CIFSException
		internal T send<T>(CommonServerMessageBlockRequest request, T response) where T : CommonServerMessageBlockResponse {
			return send(request, response, new HashSet<RequestParam>(0));
		}


		/// throws jcifs.CIFSException
		internal T send<T>(CommonServerMessageBlockRequest request, T response, ISet<RequestParam> @params) where T : CommonServerMessageBlockResponse {
			using (SmbTransportImpl trans = (SmbTransportImpl)getTransport()) {
				if (response != null) {
					response.clearReceived();
					response.setExtendedSecurity(this.extendedSecurity);
				}

				try {
					if (@params.Contains(RequestParam.NO_TIMEOUT)) {
						this.expiration = -1;
					}
					else {
						this.expiration = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + this.transportContext.getConfig().getSoTimeout();
					}

					T chainedResponse;
					try {
						chainedResponse = sessionSetup(request, response);
					}
					//TODO 1 ex
					catch (GeneralSecurityException e) {
						throw new SmbException("Session setup failed", e);
					}

					if (chainedResponse != null && chainedResponse.isReceived()) {
						return chainedResponse;
					}

					if (request is SmbComTreeConnectAndX) {
						SmbComTreeConnectAndX tcax = (SmbComTreeConnectAndX) request;
						if (this.netbiosName!=null && tcax.getPath().EndsWith("\\IPC$", StringComparison.Ordinal)) {
							/*
							 * Some pipes may require that the hostname in the tree connect
							 * be the netbios name. So if we have the netbios server name
							 * from the NTLMSSP type 2 message, and the share is IPC$, we
							 * assert that the tree connect path uses the netbios hostname.
							 */
							tcax.setPath("\\\\" + this.netbiosName + "\\IPC$");
						}
					}

					request.setSessionId(this.sessionId);
					request.setUid(this.uid);

					if (request.getDigest() == null) {
						request.setDigest(getDigest());
					}

					if (request is RequestWithPath) {
						RequestWithPath rpath = (RequestWithPath) request;
						((RequestWithPath) request).setFullUNCPath(getTargetDomain(), getTargetHost(), rpath.getFullUNCPath());
					}

					try {
						if (log.isTraceEnabled()) {
							log.trace("Request " + request);
						}
						try {
							response = this.transport.send(request, response, @params);
						}
						catch (SmbException e) {
							//TODO unchecked
							if ((e.getNtStatus() != unchecked((int)0xC000035C) && e.getNtStatus() != 0xC000203) || !trans.isSMB2()) {
								throw;
							}
							log.debug("Session expired, trying reauth", e);
							return reauthenticate(trans, this.targetDomain, request, response, @params);
						}
						if (log.isTraceEnabled()) {
							log.trace("Response " + response);
						}
						return response;
					}
					catch (DfsReferral r) {
						if (log.isDebugEnabled()) {
							log.debug("Have referral " + r);
						}
						throw r;
					}
					catch (SmbException se) {
						if (log.isTraceEnabled()) {
							log.trace("Send failed", se);
							log.trace("Request: " + request);
							log.trace("Response: " + response);
						}
						throw se;
					}
				}
				finally {
					request.setDigest(null);
					this.expiration = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + this.transportContext.getConfig().getSoTimeout();
				}
			}
		}


		internal T sessionSetup<T>(CommonServerMessageBlockRequest chained, T chainedResponse) where T : CommonServerMessageBlock {
			using (SmbTransportImpl trans =(SmbTransportImpl) getTransport()) {
				lock (trans) {

					while (!this.connectionState.CompareAndSet(0, 1)) {
						int st = this.connectionState.Value;
						if (st == 2 || st == 3) { // connected or disconnecting
							return chainedResponse;
						}
						try {
							Monitor.Wait(this.transport);
						}
						catch (ThreadInterruptedException ie) {
							throw new SmbException(ie.Message, ie);
						}
					}

					try {
						trans.ensureConnected();

						/*
						 * Session Setup And X Request / Response
						 */

						if (log.isDebugEnabled()) {
							log.debug("sessionSetup: " + this.credentials);
						}

						/*
						 * We explicitly set uid to 0 here to prevent a new
						 * SMB_COM_SESSION_SETUP_ANDX from having it's uid set to an
						 * old value when the session is re-established. Otherwise a
						 * "The parameter is incorrect" error can occur.
						 */
						this.uid = 0;

						if (trans.isSMB2()) {
		//TODO type  return sessionSetupSMB2(trans, this.targetDomain, (jcifs.internal.smb2.ServerMessageBlock2Request<?>) chained, chainedResponse);
							//TODO 1 type
							return sessionSetupSMB2(trans, this.targetDomain, (ServerMessageBlock2) chained, chainedResponse);
						}

						//TODO 
						if (!(chainedResponse is ServerMessageBlock serverMessage))
						{
							throw new InvalidCastException();
						}

						sessionSetupSMB1(trans, this.targetDomain, (ServerMessageBlock) chained,  serverMessage);
						return chainedResponse;
					}
					catch (Exception se) {
						log.debug("Session setup failed", se);
						if (this.connectionState.CompareAndSet(1, 0)) {
							// only try to logoff if we have not completed the session setup, ignore errors from chained
							// responses
							logoff(true, true);
						}
						throw se;
					}
					finally {
						Monitor.PulseAll(trans);
					}
				}
			}
		}


		/// <param name="trans"> </param>
		/// <param name="chain"> </param>
		/// <param name="andxResponse"> </param>
		/// <exception cref="SmbException"> </exception>
		 private  T sessionSetupSMB2<T>(SmbTransportImpl trans, string tdomain, ServerMessageBlock2 chain, T andxResponse) where T : CommonServerMessageBlock {
			Smb2NegotiateResponse negoResp = (Smb2NegotiateResponse) trans.getNegotiateResponse();
			Smb2SessionSetupRequest request = null;
			Smb2SessionSetupResponse response = null;
			SmbException ex = null;
			SSPContext ctx = null;
			byte[] token = negoResp.getSecurityBlob();
			int securityMode = ((negoResp.getSecurityMode() & Smb2Constants.SMB2_NEGOTIATE_SIGNING_REQUIRED) != 0) || trans.isSigningEnforced() ? Smb2Constants.SMB2_NEGOTIATE_SIGNING_REQUIRED : Smb2Constants.SMB2_NEGOTIATE_SIGNING_ENABLED;
			bool anonymous = this.credentials.isAnonymous();
			long sessId = 0;

			bool preauthIntegrity = negoResp.getSelectedDialect().atLeast(DialectVersion.SMB311);
			this.preauthIntegrityHash = preauthIntegrity ? trans.getPreauthIntegrityHash() : null;

			if (this.preauthIntegrityHash != null && log.isDebugEnabled()) {
				log.debug("Initial session preauth hash " + Hexdump.toHexString(this.preauthIntegrityHash));
			}

			while (true) {
				Subject s = this.credentials.getSubject();
				if (ctx == null) {
					ctx = createContext(trans, tdomain, negoResp, !anonymous, s);
				}
				token = createToken(ctx, token, s);

				if (token != null) {
					request = new Smb2SessionSetupRequest(this.getContext(), securityMode, negoResp.getCommonCapabilities(), 0, token);
					// here, messages are rejected with NOT_SUPPORTED if we start signing as soon as we can, wait until
					// session setup complete

					request.setSessionId(sessId);
					request.retainPayload();

					try {
						response = trans.send(request, (Smb2SessionSetupResponse)null, EnumSet<RequestParam>.of(RequestParam.RETAIN_PAYLOAD));
						sessId = response.getSessionId();
					}
					catch (SmbAuthException sae) {
						throw sae;
					}
					catch (SmbException e) {
						Smb2SessionSetupResponse sessResponse = (Smb2SessionSetupResponse)request.getResponse();
						if (e.getNtStatus() == NtStatus.NT_STATUS_INVALID_PARAMETER) {
							// a relatively large range of samba versions has a bug causing
							// an invalid parameter error when a SPNEGO MIC is in place and auth fails
							throw new SmbAuthException("Login failed", e);
						}
						else if (!sessResponse.isReceived() || sessResponse.isError() || (sessResponse.getStatus() != NtStatus.NT_STATUS_OK && sessResponse.getStatus() != NtStatus.NT_STATUS_MORE_PROCESSING_REQUIRED)) {
							throw e;
						}
						ex = e;
						response = sessResponse;
					}

					if (!this.getConfig().isAllowGuestFallback() && response.isLoggedInAsGuest() && !(this.credentials.isGuest() || this.credentials.isAnonymous())) {
						throw new SmbAuthException(NtStatus.NT_STATUS_LOGON_FAILURE);
					}
					else if (!this.credentials.isAnonymous() && response.isLoggedInAsGuest()) {
						anonymous = true;
					}

					if ((response.getSessionFlags() & Smb2SessionSetupResponse.SMB2_SESSION_FLAG_ENCRYPT_DATA) != 0) {
						throw new SmbUnsupportedOperationException("Server requires encryption, not yet supported.");
					}

					if (preauthIntegrity) {
						byte[] reqBytes = request.getRawPayload();
						this.preauthIntegrityHash = trans.calculatePreauthHash(reqBytes, 0, reqBytes.Length, this.preauthIntegrityHash);

						if (response.getStatus() == NtStatus.NT_STATUS_MORE_PROCESSING_REQUIRED) {
							byte[] respBytes = response.getRawPayload();
							this.preauthIntegrityHash = trans.calculatePreauthHash(respBytes, 0, respBytes.Length, this.preauthIntegrityHash);
						}
					}

					token = response.getBlob();
				}

				if (ctx.isEstablished()) {
					log.debug("Context is established");
					setNetbiosName(ctx.getNetbiosName());
					byte[] sk = ctx.getSigningKey();
					if (sk != null) {
						// session key is truncated to 16 bytes, right padded with 0 if shorter
						byte[] key = new byte[16];
						Array.Copy(sk, 0, key, 0, Math.Min(16, sk.Length));
						this.sessionKey = key;
					}

					bool signed = response != null && response.isSigned();
					if (!anonymous && (isSignatureSetupRequired() || signed)) {
						byte[] signingKey = ctx.getSigningKey();
						if (signingKey != null && response != null) {
							if (this.preauthIntegrityHash != null && log.isDebugEnabled()) {
								log.debug("Final preauth integrity hash " + Hexdump.toHexString(this.preauthIntegrityHash));
							}
							Smb2SigningDigest dgst = new Smb2SigningDigest(this.sessionKey, negoResp.getDialectRevision(), this.preauthIntegrityHash);
							// verify the server signature here, this is not done automatically as we don't set the
							// request digest
							// Ignore a missing signature for SMB < 3.0, as
							// - the specification does not clearly require that (it does for SMB3+)
							// - there seem to be server implementations (known: EMC Isilon) that do not sign the final
							// response
							if (negoResp.getSelectedDialect().atLeast(DialectVersion.SMB300) || response.isSigned()) {
								response.setDigest(dgst);
								byte[] payload = response.getRawPayload();
								if (!response.verifySignature(payload, 0, payload.Length)) {
									throw new SmbException("Signature validation failed");
								}
							}
							setDigest(dgst);
						}
						else if (trans.getContext().getConfig().isSigningEnabled()) {
							throw new SmbException("Signing enabled but no session key available");
						}
					}
					else if (log.isDebugEnabled()) {
						log.debug("No digest setup " + anonymous + " B " + isSignatureSetupRequired());
					}
					setSessionSetup(response);
					if (ex != null) {
						throw ex;
					}
					return (T)(response != null ? response.getNextResponse() : null);
				}
			}
		}


		private static byte[] createToken( SSPContext ctx,  byte[] token, Subject s)  {
			if (s != null) {
				try {
					return Subject.doAs(s, () =>
					{
						return ctx.initSecContext(token, 0, token == null ? 0 : token.Length);
					});
				}
				catch (Exception e) {
					//TODO 
					throw new SmbException("Unexpected exception during context initialization", e);
				}
			}
			return ctx.initSecContext(token, 0, token == null ? 0 : token.Length);
		}


		/// <param name="trans"> </param>
		/// <param name="tdomain"> </param>
		/// <param name="negoResp"> </param>
		/// <param name="ctx"> </param>
		/// <param name="doSigning"> </param>
		/// <param name="s">
		/// @return </param>
		/// <exception cref="SmbException"> </exception>
		protected SSPContext createContext(SmbTransportImpl trans, string tdomain,  Smb2NegotiateResponse negoResp,  bool doSigning, Subject s)  {

			string host = getTargetHost();
			if (host == null) {
				host = trans.getRemoteAddress().getHostAddress();
				try {
					host = trans.getRemoteAddress().getHostName();
				}
				catch (Exception e) {
					log.debug("Failed to resolve host name", e);
				}
			}

			if (log.isDebugEnabled()) {
				log.debug("Remote host is " + host);
			}

			if (s == null) {
				return this.credentials.createContext(getContext(), tdomain, host, negoResp.getSecurityBlob(), doSigning);
			}

			try {
				return Subject.doAs(s, () =>
				{
					return getCredentials().createContext(getContext(), tdomain, host, negoResp.getSecurityBlob(), doSigning);
				});
			}
			catch (Exception e) {
				//TODO 
				throw new SmbException("Unexpected exception during context initialization", e);
			}
		}


		/// <param name="request"> </param>
		/// <param name="response"> </param>
		/// <param name="params">
		/// @return </param>
		/// <exception cref="CIFSException"> </exception>
		/// throws jcifs.CIFSException
		private  T reauthenticate<T>(SmbTransportImpl trans, string tdomain, CommonServerMessageBlockRequest chain, T andxResponse, ISet<RequestParam> @params) where  T:CommonServerMessageBlock{
			SmbException ex = null;
			Smb2SessionSetupResponse response = null;
			Smb2NegotiateResponse negoResp = (Smb2NegotiateResponse) trans.getNegotiateResponse();
			byte[] token = negoResp.getSecurityBlob();
			int securityMode = negoResp.getSecurityMode();
			bool anonymous = this.credentials.isAnonymous();
			bool doSigning = securityMode != 0 && !anonymous;
			long newSessId = 0;
			long curSessId = this.sessionId;

			lock (trans) {
				this.credentials.refresh();
				Subject s = this.credentials.getSubject();
				SSPContext ctx = createContext(trans, tdomain, negoResp, doSigning, s);
				while (true) {
					token = createToken(ctx, token, s);

					if (token != null) {
						Smb2SessionSetupRequest request = new Smb2SessionSetupRequest(getContext(), negoResp.getSecurityMode(), negoResp.getCommonCapabilities(), curSessId, token);

						if (chain != null) {
							request.chain((ServerMessageBlock2) chain);
						}

						request.setDigest(this.digest);
						request.setSessionId(curSessId);

						try {
							response = trans.send(request, (Smb2SessionSetupResponse)null, EnumSet<RequestParam>.of(RequestParam.RETAIN_PAYLOAD));
							newSessId = response.getSessionId();

							if (newSessId != curSessId) {
								throw new SmbAuthException("Server did not reauthenticate after expiration");
							}
						}
						catch (SmbAuthException sae) {
							throw sae;
						}
						catch (SmbException e) {
							Smb2SessionSetupResponse sessResponse = (Smb2SessionSetupResponse)request.getResponse();
							if (!sessResponse.isReceived() || sessResponse.isError() || (sessResponse.getStatus() != NtStatus.NT_STATUS_OK && sessResponse.getStatus() != NtStatus.NT_STATUS_MORE_PROCESSING_REQUIRED)) {
								throw e;
							}
							ex = e;
							response = sessResponse;
						}

						if (!this.getConfig().isAllowGuestFallback() && response.isLoggedInAsGuest() && !(this.credentials.isGuest() || this.credentials.isAnonymous())) {
							throw new SmbAuthException(NtStatus.NT_STATUS_LOGON_FAILURE);
						}
						else if (!this.credentials.isAnonymous() && response.isLoggedInAsGuest()) {
							anonymous = true;
						}

						if (request.getDigest() != null) {
							/* success - install the signing digest */
							log.debug("Setting digest");
							setDigest(request.getDigest());
						}

						token = response.getBlob();
					}

					if (ex != null) {
						throw ex;
					}

					if (ctx.isEstablished()) {
						setSessionSetup(response);
						CommonServerMessageBlockResponse cresp = (CommonServerMessageBlockResponse)(response != null ? response.getNextResponse() : null);
						if (cresp != null && cresp.isReceived()) {
							return (T) cresp;
						}
						if (chain != null) {
							return (T)this.transport.send(chain, (CommonServerMessageBlockResponse) null, @params);
						}
						return default;
					}
				}
			}
		}


		/// throws jcifs.CIFSException
		public void reauthenticate()  {
			using (SmbTransportImpl trans =(SmbTransportImpl) getTransport()) {
				reauthenticate(trans, this.targetDomain, null, (CommonServerMessageBlockResponse)null, new HashSet<RequestParam>(0));
			}
		}


		/// <param name="trans"> </param>
		/// <param name="andx"> </param>
		/// <param name="andxResponse"> </param>
		private void sessionSetupSMB1( SmbTransportImpl trans, string tdomain, ServerMessageBlock andx, ServerMessageBlock andxResponse)  {
			SmbException ex = null;
			SmbComSessionSetupAndX request = null;
			SmbComSessionSetupAndXResponse response = null;
			SSPContext ctx = null;
			byte[] token = new byte[0];
			int state = 10;
			SmbComNegotiateResponse negoResp = (SmbComNegotiateResponse) trans.getNegotiateResponse();
			bool anonymous = this.credentials.isAnonymous();
			do {
				switch (state) {
				case 10: // NTLM

					if (trans.hasCapability(SmbConstants.CAP_EXTENDED_SECURITY)) {
						log.debug("Extended security negotiated");
						state = 20; // NTLMSSP
						break;
					}
					else if (getContext().getConfig().isForceExtendedSecurity()) {
						throw new SmbException("Server does not supported extended security");
					}

					log.debug("Performing legacy session setup");
					if (!(this.credentials is NtlmPasswordAuthenticator)) {
						throw new SmbAuthException("Incompatible credentials");
					}

					NtlmPasswordAuthenticator npa = (NtlmPasswordAuthenticator) this.credentials;

					request = new SmbComSessionSetupAndX(this.getContext(), negoResp, andx, getCredentials());
					// if the connection already has a digest set up this needs to be used
					request.setDigest(getDigest());
					response = new SmbComSessionSetupAndXResponse(getContext().getConfig(), andxResponse);
					response.setExtendedSecurity(false);

					/*
					 * Create SMB signature digest if necessary
					 * Only the first SMB_COM_SESSION_SETUP_ANX with non-null or
					 * blank password initializes signing.
					 */
					if (!anonymous && isSignatureSetupRequired()) {
						if (isExternalAuth(getContext(), npa)) {
							/*
							 * preauthentication
							 */
							using (SmbSessionImpl smbSession = (SmbSessionImpl)trans.getSmbSession(getContext().withDefaultCredentials()))
							using (SmbTreeImpl t = (SmbTreeImpl)smbSession.getSmbTree(getContext().getConfig().getLogonShare(), null)) {
								t.treeConnect(null, (CommonServerMessageBlockResponse)null);
							}
						}
						else {
							log.debug("Initialize signing");
							byte[] signingKey = npa.getSigningKey(getContext(), negoResp.getServerData().encryptionKey);
							if (signingKey == null) {
								throw new SmbException("Need a signature key but the server did not provide one");
							}
							request.setDigest(new SMB1SigningDigest(signingKey, false));
						}
					}

					try {
						trans.send(request, response);
					}
					catch (SmbAuthException sae) {
						throw sae;
					}
					catch (SmbException se) {
						ex = se;
					}

					if (!this.getConfig().isAllowGuestFallback() && response.isLoggedInAsGuest() && negoResp.getServerData().security != SmbConstants.SECURITY_SHARE && !(this.credentials.isGuest() || this.credentials.isAnonymous())) {
						throw new SmbAuthException(NtStatus.NT_STATUS_LOGON_FAILURE);
					}
					else if (!this.credentials.isAnonymous() && response.isLoggedInAsGuest()) {
						anonymous = true;
					}

					if (ex != null) {
						throw ex;
					}

					setUid(response.getUid());

					if (request.getDigest() != null) {
						/* success - install the signing digest */
						setDigest(request.getDigest());
					}
					else if (!anonymous && isSignatureSetupRequired()) {
						throw new SmbException("Signing required but no session key available");
					}

					setSessionSetup(response);
					state = 0;
					break;
				case 20: // NTLMSSP
					Subject s = this.credentials.getSubject();
					bool doSigning = !anonymous && (negoResp.getNegotiatedFlags2() & SmbConstants.FLAGS2_SECURITY_SIGNATURES) != 0;
					byte[] curToken = token;
					if (ctx == null) {
						string host = this.getTargetHost();
						if (host == null) {
							host = trans.getRemoteAddress().getHostAddress();
							try {
								host = trans.getRemoteAddress().getHostName();
							}
							catch (Exception e) {
								log.debug("Failed to resolve host name", e);
							}
						}

						if (log.isDebugEnabled()) {
							log.debug("Remote host is " + host);
						}

						if (s == null) {
							ctx = this.credentials.createContext(getContext(), tdomain, host, negoResp.getServerData().encryptionKey, doSigning);
						}
						else {
							try {
								ctx = Subject.doAs(s, () =>
								{
									return getCredentials()
										.createContext(getContext(), tdomain, host, negoResp.getServerData().encryptionKey, doSigning);
								});
							}
							catch (Exception e) {
								//TODO 
								throw new SmbException("Unexpected exception during context initialization", e);
							}
						}
					}

					SSPContext curCtx = ctx;

					if (log.isTraceEnabled()) {
						log.trace(ctx.ToString());
					}

					try {
						if (s != null) {
							try {
								token = Subject.doAs(s, () =>
								{
									return curCtx.initSecContext(curToken, 0, curToken == null ? 0 : curToken.Length);
								});
							}
							catch (Exception e) {
								throw new SmbException("Unexpected exception during context initialization", e);
							}
						}
						else {
							token = ctx.initSecContext(token, 0, token == null ? 0 : token.Length);
						}
					}
					catch (SmbException se) {
						/*
						 * We must close the transport or the server will be expecting a
						 * Type3Message. Otherwise, when we send a Type1Message it will return
						 * "Invalid parameter".
						 */
						try {
							log.warn("Exception during SSP authentication", se);
							trans.disconnect(true);
						}
						catch (IOException) {
							log.debug("Disconnect failed");
						}
						setUid(0);
						throw se;
					}

					if (token != null) {
						request = new SmbComSessionSetupAndX(this.getContext(), negoResp, null, token);
						// if the connection already has a digest set up this needs to be used
						request.setDigest(getDigest());
						if (doSigning && ctx.isEstablished() && isSignatureSetupRequired()) {
							byte[] signingKey = ctx.getSigningKey();
							if (signingKey != null) {
								request.setDigest(new SMB1SigningDigest(signingKey));
							}
							this.sessionKey = signingKey;
						}
						else {
							log.trace("Not yet initializing signing");
						}

						response = new SmbComSessionSetupAndXResponse(getContext().getConfig(), null);
						response.setExtendedSecurity(true);
						request.setUid(getUid());
						setUid(0);

						try {
							trans.send(request, response);
						}
						catch (SmbAuthException sae) {
							throw sae;
						}
						catch (SmbException se) {
							ex = se;
							if (se.getNtStatus() == NtStatus.NT_STATUS_INVALID_PARAMETER) {
								// a relatively large range of samba versions has a bug causing
								// an invalid parameter error when a SPNEGO MIC is in place and auth fails
								ex = new SmbAuthException("Login failed", se);
							}
							/*
							 * Apparently once a successful NTLMSSP login occurs, the
							 * server will return "Access denied" even if a logoff is
							 * sent. Unfortunately calling disconnect() doesn't always
							 * actually shutdown the connection before other threads
							 * have committed themselves (e.g. InterruptTest example).
							 */
							try {
								trans.disconnect(true);
							}
							catch (Exception e) {
								log.debug("Failed to disconnect transport", e);
							}
						}

						if (!getConfig().isAllowGuestFallback() && response.isLoggedInAsGuest() && !(this.credentials.isGuest() || this.credentials.isAnonymous())) {
							throw new SmbAuthException(NtStatus.NT_STATUS_LOGON_FAILURE);
						}
						else if (!this.credentials.isAnonymous() && response.isLoggedInAsGuest()) {
							anonymous = true;
						}

						if (ex != null) {
							throw ex;
						}

						setUid(response.getUid());

						if (request.getDigest() != null) {
							/* success - install the signing digest */
							log.debug("Setting digest");
							setDigest(request.getDigest());
						}

						token = response.getBlob();
					}

					if (ctx.isEstablished()) {
						log.debug("Context is established");
						setNetbiosName(ctx.getNetbiosName());
						this.sessionKey = ctx.getSigningKey();
						if (request != null && request.getDigest() != null) {
							/* success - install the signing digest */
							setDigest(request.getDigest());
						}
						else if (!anonymous && isSignatureSetupRequired()) {
							byte[] signingKey = ctx.getSigningKey();
							if (signingKey != null && response != null) {
								setDigest(new SMB1SigningDigest(signingKey, 2));
							}
							else if (trans.getContext().getConfig().isSigningEnabled()) {
								throw new SmbException("Signing required but no session key available");
							}
							this.sessionKey = signingKey;
						}
						setSessionSetup(response);
						state = 0;
						break;
					}
					break;
				default:
					throw new SmbException("Unexpected session setup state: " + state);

				}
			} while (state != 0);
		}


		private static bool isExternalAuth(CIFSContext tc, NtlmPasswordAuthenticator npa) {
			return npa is NtlmPasswordAuthentication && ((NtlmPasswordAuthentication) npa).areHashesExternal() && tc.getConfig().getDefaultPassword() != null;
		}


		internal bool logoff(bool inError, bool inUse) {
			bool wasInUse = false;
			try {
					using (SmbTransportImpl trans = (SmbTransportImpl)getTransport()) {
					lock (trans) {
						if (!this.connectionState.CompareAndSet(2, 3)) {
							return false;
						}
        
						if (log.isDebugEnabled()) {
							log.debug("Logging off session on " + trans);
						}
        
						this.netbiosName = null;
        
						lock (this.trees) {
							long us = this.usageCount.Value;
							if ((inUse && us != 1) || (!inUse && us > 0)) {
								log.warn("Logging off session while still in use " + this + ":" + this.trees);
								wasInUse = true;
							}
        
							foreach (SmbTreeImpl t in this.trees) {
								try {
									log.debug("Disconnect tree on logoff");
									wasInUse |= t.treeDisconnect(inError, false);
								}
								catch (Exception e) {
									log.warn("Failed to disconnect tree " + t, e);
								}
							}
						}
        
						if (!inError && trans.isSMB2()) {
							Smb2LogoffRequest request = new Smb2LogoffRequest(getConfig());
							request.setDigest(getDigest());
							request.setSessionId(this.sessionId);
							try {
								this.transport.send((Smb2LogoffRequest)request.ignoreDisconnect(), (CommonServerMessageBlockResponse)null);
							}
							catch (SmbException se) {
								log.debug("Smb2LogoffRequest failed", se);
							}
						}
						else if (!inError) {
							bool shareSecurity = ((SmbComNegotiateResponse) trans.getNegotiateResponse()).getServerData().security == SmbConstants.SECURITY_SHARE;
							if (!shareSecurity) {
								SmbComLogoffAndX request = new SmbComLogoffAndX(getConfig(), null);
								request.setDigest(getDigest());
								request.setUid(getUid());
								try {
									this.transport.send(request, new SmbComBlankResponse(getConfig()));
								}
								catch (SmbException se) {
									log.debug("SmbComLogoffAndX failed", se);
								}
								this.uid = 0;
							}
						}
        
					}
					}
			}
			catch (SmbException e) {
				log.warn("Error in logoff", e);
			}
			finally {
				this.connectionState.Value=(0);
				this.digest = null;
				Monitor.PulseAll(this.transport);
			}
			return wasInUse;
		}


		public override string ToString() {
			return "SmbSession[credentials=" + this.transportContext.getCredentials() + ",targetHost=" + this.targetHost + ",targetDomain=" + this.targetDomain + ",uid=" + this.uid + ",connectionState=" + this.connectionState + ",usage=" + this.usageCount.Value + "]";
		}


		void setUid(int uid) {
			this.uid = uid;
		}


		void setSessionSetup(Smb2SessionSetupResponse response) {
			this.extendedSecurity = true;
			this.connectionState.Value=(2);
			this.sessionId = response.getSessionId();
		}


		void setSessionSetup(SmbComSessionSetupAndXResponse response) {
			this.extendedSecurity = response.isExtendedSecurity();
			this.connectionState.Value=(2);
		}


		void setNetbiosName(string netbiosName) {
			this.netbiosName = netbiosName;
		}


		/// <returns> the context this session is attached to </returns>
		public CIFSContext getContext() {
			return this.transport.getContext();
		}


		/// <returns> the transport this session is attached to </returns>
		public SmbTransport getTransport() {
			return (SmbTransport)this.transport.acquire();
		}


		/// <returns> this session's UID </returns>
		public int getUid() {
			return this.uid;
		}


		/// <returns> this session's expiration time </returns>
		public long? getExpiration() {
			return this.expiration > 0 ? this.expiration : (long?) null;
		}


		/// <returns> this session's credentials </returns>
		public CredentialsInternal getCredentials() {
			return this.credentials;
		}


		/// <returns> whether the session is connected </returns>
		public bool isConnected() {
			return !this.transport.isDisconnected() && this.connectionState.Value == 2;
		}


		/// <returns> whether the session has been lost </returns>
		public bool isFailed() {
			return this.transport.isFailed();
		}

	}

}