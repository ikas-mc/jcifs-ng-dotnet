using jcifs;
using jcifs.@internal;
using jcifs.util.transport;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using cifs_ng.lib;
using cifs_ng.lib.ext;
using cifs_ng.lib.security;
using cifs_ng.lib.socket;
using cifs_ng.lib.threading;
using Logger = org.slf4j.Logger;
using LoggerFactory = org.slf4j.LoggerFactory;
using Address = jcifs.Address;
using CIFSContext = jcifs.CIFSContext;
using CIFSException = jcifs.CIFSException;
using DfsReferralData = jcifs.DfsReferralData;
using DialectVersion = jcifs.DialectVersion;
using SmbConstants = jcifs.SmbConstants;
using SmbTransport = jcifs.SmbTransport;
using CommonServerMessageBlock = jcifs.@internal.CommonServerMessageBlock;
using CommonServerMessageBlockRequest = jcifs.@internal.CommonServerMessageBlockRequest;
using CommonServerMessageBlockResponse = jcifs.@internal.CommonServerMessageBlockResponse;
using RequestWithPath = jcifs.@internal.RequestWithPath;
using SMBProtocolDecodingException = jcifs.@internal.SMBProtocolDecodingException;
using SMBSigningDigest = jcifs.@internal.SMBSigningDigest;
using SmbNegotiation = jcifs.@internal.SmbNegotiation;
using SmbNegotiationResponse = jcifs.@internal.SmbNegotiationResponse;
using DfsReferralDataImpl = jcifs.@internal.dfs.DfsReferralDataImpl;
using DfsReferralRequestBuffer = jcifs.@internal.dfs.DfsReferralRequestBuffer;
using DfsReferralResponseBuffer = jcifs.@internal.dfs.DfsReferralResponseBuffer;
using Referral = jcifs.@internal.dfs.Referral;
using AndXServerMessageBlock = jcifs.@internal.smb1.AndXServerMessageBlock;
using ServerMessageBlock = jcifs.@internal.smb1.ServerMessageBlock;
using SmbComBlankResponse = jcifs.@internal.smb1.com.SmbComBlankResponse;
using SmbComLockingAndX = jcifs.@internal.smb1.com.SmbComLockingAndX;
using SmbComNegotiate = jcifs.@internal.smb1.com.SmbComNegotiate;
using SmbComNegotiateResponse = jcifs.@internal.smb1.com.SmbComNegotiateResponse;
using SmbComReadAndXResponse = jcifs.@internal.smb1.com.SmbComReadAndXResponse;
using SmbComTransaction = jcifs.@internal.smb1.trans.SmbComTransaction;
using SmbComTransactionResponse = jcifs.@internal.smb1.trans.SmbComTransactionResponse;
using Trans2GetDfsReferral = jcifs.@internal.smb1.trans2.Trans2GetDfsReferral;
using Trans2GetDfsReferralResponse = jcifs.@internal.smb1.trans2.Trans2GetDfsReferralResponse;
using ServerMessageBlock2 = jcifs.@internal.smb2.ServerMessageBlock2;
using jcifs.@internal.smb2;
using jcifs.lib;
using jcifs.lib.io;
using ServerMessageBlock2Response = jcifs.@internal.smb2.ServerMessageBlock2Response;
using Smb2Constants = jcifs.@internal.smb2.Smb2Constants;
using Smb2ReadResponse = jcifs.@internal.smb2.io.Smb2ReadResponse;
using Smb2IoctlRequest = jcifs.@internal.smb2.ioctl.Smb2IoctlRequest;
using Smb2IoctlResponse = jcifs.@internal.smb2.ioctl.Smb2IoctlResponse;
using Smb2OplockBreakNotification = jcifs.@internal.smb2.@lock.Smb2OplockBreakNotification;
using EncryptionNegotiateContext = jcifs.@internal.smb2.nego.EncryptionNegotiateContext;
using Smb2NegotiateRequest = jcifs.@internal.smb2.nego.Smb2NegotiateRequest;
using Smb2NegotiateResponse = jcifs.@internal.smb2.nego.Smb2NegotiateResponse;
using Name = jcifs.netbios.Name;
using NbtException = jcifs.netbios.NbtException;
using SessionRequestPacket = jcifs.netbios.SessionRequestPacket;
using SessionServicePacket = jcifs.netbios.SessionServicePacket;
using Crypto = jcifs.util.Crypto;
using Encdec = jcifs.util.Encdec;
using Hexdump = jcifs.util.Hexdump;
using Request = jcifs.util.transport.Request;
using Response = jcifs.util.transport.Response;
using Semaphore = cifs_ng.lib.threading.Semaphore;
using Transport = jcifs.util.transport.Transport;
using TransportException = jcifs.util.transport.TransportException;

/* jcifs smb client library in Java
 * Copyright (C) 2005  "Michael B. Allen" <jcifs at samba dot org>
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





	/// 
	internal class SmbTransportImpl : Transport, SmbTransportInternal{ //, SmbConstants 

		private static Logger log = LoggerFactory.getLogger(typeof(SmbTransportImpl));

		private bool smb2 = false;
		private IPAddress localAddr;
		private int localPort;
		private Address address;
		private SocketEx socket;
		private int port;
		private readonly AtomicLong mid = new AtomicLong();
		private SocketOutputStream @out;
		private SocketInputStream @in;
		private readonly byte[] sbuf = new byte[1024]; // small local buffer
		private long sessionExpiration;
		private readonly LinkedList<SmbSessionImpl> sessions = new LinkedList<SmbSessionImpl>();

		private string tconHostName = null;

		private readonly CIFSContext transportContext;
		private readonly bool signingEnforced;

		private SmbNegotiationResponse negotiated;

		private SMBSigningDigest digest;

		private readonly Semaphore credits = new Semaphore(1);//new Semaphore(1, true)

		private readonly int desiredCredits = 512;

		private byte[] preauthIntegrityHash = new byte[64];


		internal SmbTransportImpl(CIFSContext tc, Address address, int port, IPAddress localAddr, int localPort, bool forceSigning) {
			this.transportContext = tc;

			this.signingEnforced = forceSigning || this.getContext().getConfig().isSigningEnforced();
			this.sessionExpiration = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + tc.getConfig().getSessionTimeout();

			this.address = address;
			this.port = port;
			this.localAddr = localAddr;
			this.localPort = localPort;

		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.util.transport.Transport#getResponseTimeout() </seealso>
		protected  override int getResponseTimeout(Request req) {
			if (req is CommonServerMessageBlockRequest) {
				int? overrideTimeout = ((CommonServerMessageBlockRequest) req).getOverrideTimeout();
				if (overrideTimeout != null) {
					return overrideTimeout.Value;
				}
			}
			return getContext().getConfig().getResponseTimeout();
		}


		public virtual Address getRemoteAddress() {
			return this.address;
		}


		public virtual string getRemoteHostName() {
			return this.tconHostName;
		}


		/// 
		/// <returns> number of sessions on this transport </returns>
		public virtual int getNumSessions() {
			return this.sessions.Count;
		}


		public virtual int getInflightRequests() {
			return this.responseMap.Count;
		}


		public override bool isDisconnected() {
			var s = this.socket;
			return base.isDisconnected() || s == null || s.isClosed();
		}


		public override bool isFailed() {
			var s = this.socket;
			return base.isFailed() || s == null || s.isClosed();
		}


		/// throws SmbException
		public virtual bool hasCapability(int cap) {
			return getNegotiateResponse().haveCapabilitiy(cap);
		}


		/// <returns> the negotiated </returns>
		/// <exception cref="SmbException"> </exception>
		/// throws SmbException
		internal virtual SmbNegotiationResponse getNegotiateResponse() {
			try {
				if (this.negotiated == null) {
					connect(this.transportContext.getConfig().getResponseTimeout());
				}
			}
			catch (IOException ioe) {
				throw new SmbException(ioe.Message, ioe);
			}
			SmbNegotiationResponse r = this.negotiated;
			if (r == null) {
				throw new SmbException("Connection did not complete, failed to get negotiation response");
			}
			return r;
		}


		/// <returns> whether this is SMB2 transport </returns>
		/// <exception cref="SmbException"> </exception>
		/// throws SmbException
		public virtual bool isSMB2() {
			return this.smb2 || getNegotiateResponse() is Smb2NegotiateResponse;
		}


		/// <param name="digest"> </param>
		public virtual void setDigest(SMBSigningDigest digest) {
			this.digest = digest;
		}


		/// <returns> the digest </returns>
		public virtual SMBSigningDigest getDigest() {
			return this.digest;
		}


		/// <returns> the context associated with this transport connection </returns>
		public virtual CIFSContext getContext() {
			return this.transportContext;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.util.transport.Transport#acquire() </seealso>
		public override Transport acquire() {
			return (SmbTransportImpl) base.acquire();
		}


		/// <returns> the server's encryption key </returns>
		public virtual byte[] getServerEncryptionKey() {
			if (this.negotiated == null) {
				return null;
			}

			if (this.negotiated is SmbComNegotiateResponse) {
				return ((SmbComNegotiateResponse) this.negotiated).getServerData().encryptionKey;
			}
			return null;
		}


		/// throws SmbException
		public virtual bool isSigningOptional() {
			if (this.signingEnforced) {
				return false;
			}
			SmbNegotiationResponse nego = getNegotiateResponse();
			return nego.isSigningNegotiated() && !nego.isSigningRequired();
		}


		/// throws SmbException
		public virtual bool isSigningEnforced() {
			if (this.signingEnforced) {
				return true;
			}
			return getNegotiateResponse().isSigningRequired();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.SmbTransport#unwrap(java.lang.Class) </seealso>
		public virtual T unwrap<T>(Type type) where T : SmbTransport {
			if (this is T transport) {
				return transport;
			}
			throw new System.InvalidCastException();
		}


		/// 
		/// <param name="tf"> </param>
		/// <returns> a session for the context </returns>
		public virtual SmbSession getSmbSession(CIFSContext tf) {
			return getSmbSession(tf, null, null);
		}


		/// 
		/// <param name="tf">
		///            context to use </param>
		/// <returns> a session for the context </returns>
		public virtual SmbSession getSmbSession(CIFSContext tf, string targetHost, string targetDomain) {
			lock (this) {
				long now;
        
				if (log.isTraceEnabled()) {
					log.trace("Currently " + this.sessions.Count + " session(s) active for " + this);
				}
        
				if (targetHost!= null) {
					targetHost = targetHost.ToLowerInvariant();
				}
        
				if (targetDomain!=null) {
					targetDomain = targetDomain.ToUpperInvariant();
				}
        
				IEnumerator<SmbSessionImpl> iter = this.sessions.GetEnumerator();
				while (iter.MoveNext()) {
					SmbSessionImpl session = iter.Current;
					if (session.matches(tf, targetHost, targetDomain)) {
						if (log.isTraceEnabled()) {
							log.trace("Reusing existing session " + session);
						}
						return session.acquire();
					}
					else if (log.isTraceEnabled()) {
						log.trace("Existing session " + session + " does not match " + tf.getCredentials());
					}
				}
        
				/* logoff old sessions */
				if (tf.getConfig().getSessionTimeout() > 0 && this.sessionExpiration < (now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())) {
					this.sessionExpiration = now + tf.getConfig().getSessionTimeout();
					iter = this.sessions.GetEnumerator();
					while (iter.MoveNext()) {
						SmbSessionImpl session2 = iter.Current;
						if (session2.getExpiration() != null && session2.getExpiration().Value < now && !session2.isInUse()) {
							if (log.isDebugEnabled()) {
								log.debug("Closing session after timeout " + session2);
							}
							session2.logoff(false, false);
						}
					}
				}
				SmbSessionImpl session3 = new SmbSessionImpl(tf, targetHost, targetDomain, this);
				if (log.isDebugEnabled()) {
					log.debug("Establishing new session " + session3 + " on " + this.name);
				}
				this.sessions.AddLast(session3);
				return session3;
			}
		}


		internal virtual bool matches(Address addr, int prt, IPAddress laddr, int lprt, string hostName) {
			if (this.state == 5 || this.state == 6) {
				// don't reuse disconnecting/disconnected transports
				return false;
			}
			if (string.IsNullOrEmpty(hostName)) {
				hostName = addr.getHostName();
			}
			return (this.tconHostName== null || hostName.Equals(this.tconHostName, StringComparison.OrdinalIgnoreCase)) && addr.Equals(this.address) && (prt == 0 || prt == this.port || (prt == 445 && this.port == 139)) && (laddr == this.localAddr || (laddr != null && laddr.Equals(this.localAddr))) && lprt == this.localPort;
		}


		/// throws java.io.IOException
		internal virtual void ssn139() {
			CIFSContext tc = this.transportContext;
			Name calledName = new Name(tc.getConfig(), this.address.firstCalledName(), 0x20, null);
			do
			{
				var server = new IPEndPoint(this.address.toInetAddress(), 139);
				this.socket = SocketEx.ofTcpSocket(server.AddressFamily);

				this.socket.SetSocketOption(SocketOptionLevel.Socket,SocketOptionName.ReceiveTimeout,tc.getConfig().getSoTimeout());
				//this.socket.ReceiveTimeout=(tc.getConfig().getSoTimeout());
				if (this.localAddr != null) {
					this.socket.Bind(new IPEndPoint(this.localAddr, this.localPort));
				}

				try
				{
					this.socket.Connect(server,tc.getConfig().getConnTimeout());
					this.@out = this.socket.GetOutputStream();
					this.@in = this.socket.GetInputStream();
				}
				//TODO ex
				catch (Exception e)
				{
					log.error($"connect to server failed,server={this.address.toInetAddress()},port={139},message={e.Message}",e);
					throw;
				}

				SessionServicePacket ssp = new SessionRequestPacket(tc.getConfig(), calledName, tc.getNameServiceClient().getLocalName());
				this.@out.write(this.sbuf, 0, ssp.writeWireFormat(this.sbuf, 0));
				if (readn(this.@in, this.sbuf, 0, 4) < 4) {
					try {
						this.socket.Dispose();
					}
					catch (IOException ioe) {
						log.debug("Failed to close socket", ioe);
					}
					throw new SmbException("EOF during NetBIOS session request");
				}
				switch (this.sbuf[0] & 0xFF) {
				case SessionServicePacket.POSITIVE_SESSION_RESPONSE:
					if (log.isDebugEnabled()) {
						log.debug("session established ok with " + this.address);
					}
					return;
				case SessionServicePacket.NEGATIVE_SESSION_RESPONSE:
					int errorCode = this.@in.read() & 0xFF;
					switch (errorCode) {
					case NbtException.CALLED_NOT_PRESENT:
					case NbtException.NOT_LISTENING_CALLED:
						this.socket.Dispose();
						break;
					default:
						disconnect(true);
						throw new NbtException(NbtException.ERR_SSN_SRVC, errorCode);
					}
					break;
				case -1:
					disconnect(true);
					throw new NbtException(NbtException.ERR_SSN_SRVC, NbtException.CONNECTION_REFUSED);
				default:
					disconnect(true);
					throw new NbtException(NbtException.ERR_SSN_SRVC, 0);
				}
			} while (( calledName.name = this.address.nextCalledName(tc) ) != null );

			throw new IOException("Failed to establish session with " + this.address);
		}


		/// throws java.io.IOException
		private SmbNegotiation negotiate(int prt) {
			/*
			 * We cannot use Transport.sendrecv() yet because
			 * the Transport thread is not setup until doConnect()
			 * returns and we want to suppress all communication
			 * until we have properly negotiated.
			 */
			lock (this.inLock) {
				if (prt == 139) {
					ssn139();
				}
				else {
					if (prt == 0) {
						prt = SmbConstants.DEFAULT_PORT; // 445
					}

					var server = new IPEndPoint(this.address.toInetAddress(), prt);
					this.socket = SocketEx.ofTcpSocket(server.AddressFamily);

					this.socket.SetSocketOption(SocketOptionLevel.Socket,SocketOptionName.ReceiveTimeout,this.transportContext.getConfig().getSoTimeout());
					if (this.localAddr != null) {
						this.socket.Bind(new IPEndPoint(this.localAddr, this.localPort));
					}
					this.socket.Connect(server);
					//this.socket.ReceiveTimeout=(this.transportContext.getConfig().getSoTimeout());

					this.@out = this.socket.GetOutputStream();
					this.@in = this.socket.GetInputStream();
				}

				if (this.credits.drainPermits() == 0) {
					log.debug("It appears we previously lost some credits");
				}

				if (this.smb2 || this.getContext().getConfig().isUseSMB2OnlyNegotiation()) {
					log.debug("Using SMB2 only negotiation");
					return negotiate2(null);
				}

				SmbComNegotiate comNeg = new SmbComNegotiate(getContext().getConfig(), this.signingEnforced);
				int n = negotiateWrite(comNeg, true);
				negotiatePeek();

				SmbNegotiationResponse resp = null;

				if (!this.smb2) {
					if (this.getContext().getConfig().getMinimumVersion().isSMB2()) {
						throw new CIFSException("Server does not support SMB2");
					}
					resp = new SmbComNegotiateResponse(getContext());
					resp.decode(this.sbuf, 4);
					resp.received();

					if (log.isTraceEnabled()) {
						log.trace(resp.ToString());
						log.trace(Hexdump.toHexString(this.sbuf, 4, n));
					}
				}
				else {
					Smb2NegotiateResponse r = new Smb2NegotiateResponse(getContext().getConfig());
					r.decode(this.sbuf, 4);
					r.received();

					if (r.getDialectRevision() == Smb2Constants.SMB2_DIALECT_ANY) {
						return negotiate2(r);
					}
					else if (r.getDialectRevision() != Smb2Constants.SMB2_DIALECT_0202) {
						throw new CIFSException("Server returned invalid dialect verison in multi protocol negotiation");
					}

					int permits1 = r.getInitialCredits();
					if (permits1 > 0) {
						this.credits.Release(permits1);
					}
					Array.Clear(this.sbuf,0,this.sbuf.Length);
					return new SmbNegotiation(new Smb2NegotiateRequest(getContext().getConfig(), this.signingEnforced ? Smb2Constants.SMB2_NEGOTIATE_SIGNING_REQUIRED : Smb2Constants.SMB2_NEGOTIATE_SIGNING_ENABLED), r, null, null);
				}

				int permits2 = resp.getInitialCredits();
				if (permits2 > 0) {
					this.credits.Release(permits2);
				}
				Array.Clear(this.sbuf,0,this.sbuf.Length);
				return new SmbNegotiation(comNeg, resp, null, null);
			}
		}


		/// <summary>
		/// @return </summary>
		/// <exception cref="IOException"> </exception>
		/// throws java.io.IOException
		private int negotiateWrite(CommonServerMessageBlockRequest req, bool setmid) {
			if (setmid) {
				makeKey(req);
			}
			else {
				req.setMid(0);
				this.mid.Value=(1);
			}
			int n = req.encode(this.sbuf, 4);
			Encdec.enc_uint32be(n & 0xFFFF, this.sbuf, 0); // 4 byte ssn msg header

			if (log.isTraceEnabled()) {
				log.trace(req.ToString());
				log.trace(Hexdump.toHexString(this.sbuf, 4, n));
			}

			this.@out.write(this.sbuf, 0, 4 + n);
			this.@out.flush();
			log.trace("Wrote negotiate request");
			return n;
		}


		/// <exception cref="SocketException"> </exception>
		/// <exception cref="IOException"> </exception>
		/// throws SocketException, java.io.IOException
		private void negotiatePeek() {
			/*
			 * Note the Transport thread isn't running yet so we can
			 * read from the socket here.
			 */
			try {
				this.socket.ReceiveTimeout=(this.transportContext.getConfig().getConnTimeout());
				if (peekKey() == null) { // try to read header
					throw new IOException("transport closed in negotiate");
				}
			}
			finally {
				this.socket.ReceiveTimeout=(this.transportContext.getConfig().getSoTimeout());
			}
			int size = Encdec.dec_uint16be(this.sbuf, 2) & 0xFFFF;
			if (size < 33 || (4 + size) > this.sbuf.Length) {
				throw new IOException("Invalid payload size: " + size);
			}
			int hdrSize = this.smb2 ? Smb2Constants.SMB2_HEADER_LENGTH : SmbConstants.SMB1_HEADER_LENGTH;
			readn(this.@in, this.sbuf, 4 + hdrSize, size - hdrSize);
			log.trace("Read negotiate response");
		}


		/// <param name="first"> </param>
		/// <param name="n">
		/// @return </param>
		/// <exception cref="IOException"> </exception>
		/// <exception cref="SocketException"> </exception>
		/// <exception cref="InterruptedException"> </exception>
		/// throws IOException, java.net.SocketException
		private SmbNegotiation negotiate2(Smb2NegotiateResponse first) {
			int size = 0;

			int securityMode = getRequestSecurityMode(first);

			// further negotiation needed
			Smb2NegotiateRequest smb2neg = new Smb2NegotiateRequest(getContext().getConfig(), securityMode);
			Smb2NegotiateResponse r = null;
			byte[] negoReqBuffer = null;
			byte[] negoRespBuffer = null;
			try {
				smb2neg.setRequestCredits(Math.Max(1, this.desiredCredits - (int)this.credits.Permits()));//availablePermits()

				int reqLen = negotiateWrite(smb2neg, first != null);
				bool doPreauth = getContext().getConfig().getMaximumVersion().atLeast(DialectVersion.SMB311);
				if (doPreauth) {
					negoReqBuffer = new byte[reqLen];
					Array.Copy(this.sbuf, 4, negoReqBuffer, 0, reqLen);
				}

				negotiatePeek();

				r = smb2neg.initResponse(getContext());
				int respLen = r.decode(this.sbuf, 4);
				r.received();

				if (doPreauth) {
					negoRespBuffer = new byte[respLen];
					Array.Copy(this.sbuf, 4, negoRespBuffer, 0, respLen);
				}
				else {
					negoReqBuffer = null;
				}

				if (log.isTraceEnabled()) {
					log.trace(r.ToString());
					log.trace(Hexdump.toHexString(this.sbuf, 4, size));
				}
				return new SmbNegotiation(smb2neg, r, negoReqBuffer, negoRespBuffer);
			}
			finally {
				int grantedCredits = r != null ? r.getGrantedCredits() : 0;
				if (grantedCredits == 0) {
					grantedCredits = 1;
				}
				this.credits.Release(grantedCredits);
				Array.Clear(this.sbuf,0,this.sbuf.Length);
			}
		}


		/// <summary>
		/// Connect the transport
		/// </summary>
		/// <exception cref="SmbException"> </exception>
		/// throws SmbException
		public virtual bool ensureConnected() {
			try {
				return base.connect(this.transportContext.getConfig().getResponseTimeout());
			}
			catch (TransportException te) {
				throw new SmbException("Failed to connect: " + this.address, te);
			}

		}


		/// throws java.io.IOException
		protected internal override void doConnect() {
			/*
			 * Negotiate Protocol Request / Response
			 */
			if (log.isDebugEnabled()) {
				log.debug("Connecting in state " + this.state + " addr " + this.address.getHostAddress());
			}

			SmbNegotiation resp;
			try {
				resp = negotiate(this.port);
			}
			catch (IOException ce) {
				if (getContext().getConfig().isPort139FailoverEnabled()) {
					this.port = (this.port == 0 || this.port == SmbConstants.DEFAULT_PORT) ? 139 : SmbConstants.DEFAULT_PORT;
					this.smb2 = false;
					this.mid.Value=0;
					resp = negotiate(this.port);
				}
				else {
					throw ce;
				}
			}

			if (resp == null || resp.getResponse() == null) {
				throw new SmbException("Failed to connect.");
			}

			if (log.isDebugEnabled()) {
				log.debug("Negotiation response on " + this.name + " :" + resp);
			}

			if (!resp.getResponse().isValid(getContext(), resp.getRequest())) {
				throw new SmbException("This client is not compatible with the server.");
			}

			bool serverRequireSig = resp.getResponse().isSigningRequired();
			bool serverEnableSig = resp.getResponse().isSigningEnabled();
			if (log.isDebugEnabled()) {
				log.debug("Signature negotiation enforced " + this.signingEnforced + " (server " + serverRequireSig + ") enabled " + this.getContext().getConfig().isSigningEnabled() + " (server " + serverEnableSig + ")");
			}

			/* Adjust negotiated values */
			this.tconHostName = this.address.getHostName();
			this.negotiated = resp.getResponse();
			if (resp.getResponse().getSelectedDialect().atLeast(DialectVersion.SMB311)) {
				updatePreauthHash(resp.getRequestRaw());
				updatePreauthHash(resp.getResponseRaw());
				if (log.isDebugEnabled()) {
					log.debug("Preauth hash after negotiate " + Hexdump.toHexString(this.preauthIntegrityHash));
				}
			}
		}


		/// throws java.io.IOException
		protected internal virtual void doDisconnect(bool hard) {
			lock (this) {
				doDisconnect(hard, false);
			}
		}


		/// throws java.io.IOException
		protected internal override bool doDisconnect(bool hard, bool inUse) {
			lock (this) {
			
				bool wasInUse = false;
				long l = getUsageCount();
				if ((inUse && l != 1) || (!inUse && l > 0)) {
					log.warn("Disconnecting transport while still in use " + this + ": " + this.sessions);
					wasInUse = true;
				}
        
				if (log.isDebugEnabled()) {
					log.debug("Disconnecting transport " + this);
				}
        
				try {
					if (log.isTraceEnabled()) {
						log.trace("Currently " + this.sessions.Count + " session(s) active for " + this);
					}

					using (IEnumerator<SmbSessionImpl> iter = this.sessions.GetEnumerator())
					{
						while (iter.MoveNext()) {
							SmbSessionImpl ssn = iter.Current;
							try {
								wasInUse |= ssn.logoff(hard, false);
							}
							catch (Exception e) {
								log.debug("Failed to close session", e);
							}
							finally {
								//TODO 
								//iter.remove();
							}
						}
					}
					
					this.sessions.Clear();
        
					if (this.socket != null) {
						this.socket.shutdownOutput();
						this.@out.Dispose();
						this.@in.Dispose();
						this.socket.Dispose();
						log.trace("Socket closed");
					}
					else {
						log.trace("Not yet initialized");
					}
				}
				catch (Exception e) {
					log.debug("Exception in disconnect", e);
				}
				finally {
					this.socket = null;
					this.digest = null;
					this.tconHostName = null;
					this.transportContext.getTransportPool().removeTransport(this);
				}
				return wasInUse;
			}
		}


		/// throws java.io.IOException
		protected internal override long makeKey(Request request) {
			//TODO 1 Credit
			var creditsNeeded=request.getCreditCost();
			//TODO set Credit Charge
			if (request is ServerMessageBlock2 serverMessageBlock2) {
				if (negotiated?.getSelectedDialect().atLeast(DialectVersion.SMB210) == true) {
					serverMessageBlock2.setCreditCharge(creditsNeeded);
				}
			}
			var m= this.mid.AddDeltaAndReturnPreviousValue(creditsNeeded);
			//long m = this.mid.IncrementValueAndReturn() - 1;
			if (!this.smb2) {
				m = (m % 32000);
			}
			((CommonServerMessageBlock) request).setMid(m);
			return m;
		}


		/// throws java.io.IOException
		protected internal override long? peekKey() {
			do {
				if ((readn(this.@in, this.sbuf, 0, 4)) < 4) {
					return null;
				}
			} while (this.sbuf[0] == unchecked((byte) 0x85)); // Dodge NetBIOS keep-alive
			/* read smb header */
			if ((readn(this.@in, this.sbuf, 4, SmbConstants.SMB1_HEADER_LENGTH)) < SmbConstants.SMB1_HEADER_LENGTH) {
				return null;
			}

			if (log.isTraceEnabled()) {
				log.trace("New data read: " + this);
				log.trace(Hexdump.toHexString(this.sbuf, 4, 32));
			}

			for (;;) {
				/*
				 * 01234567
				 * 00SSFSMB
				 * 0 - 0's
				 * S - size of payload
				 * FSMB - 0xFF SMB magic #
				 */

				if (this.sbuf[0] == (byte) 0x00 && this.sbuf[4] == unchecked((byte) 0xFE) && this.sbuf[5] == (byte) 'S' && this.sbuf[6] == (byte) 'M' && this.sbuf[7] == (byte) 'B') {
					this.smb2 = true;
					// also read the rest of the header
					int lenDiff = Smb2Constants.SMB2_HEADER_LENGTH - SmbConstants.SMB1_HEADER_LENGTH;
					if (readn(this.@in, this.sbuf, 4 + SmbConstants.SMB1_HEADER_LENGTH, lenDiff) < lenDiff) {
						return null;
					}
					return (long) Encdec.dec_uint64le(this.sbuf, 28);
				}

				if (this.sbuf[0] == (byte) 0x00 && this.sbuf[1] == (byte) 0x00 && (this.sbuf[4] == unchecked((byte) 0xFF)) && this.sbuf[5] == (byte) 'S' && this.sbuf[6] == (byte) 'M' && this.sbuf[7] == (byte) 'B') {
					break; // all good (SMB)
				}

				/* out of phase maybe? */
				/* inch forward 1 byte and try again */
				for (int i = 0; i < 35; i++) {
					log.warn("Possibly out of phase, trying to resync " + Hexdump.toHexString(this.sbuf, 0, 16));
					this.sbuf[i] = this.sbuf[i + 1];
				}
				int b;
				if ((b = this.@in.read()) == -1) {
					return null;
				}
				this.sbuf[35] = (byte) b;
			}

			/*
			 * Unless key returned is null or invalid Transport.loop() always
			 * calls doRecv() after and no one else but the transport thread
			 * should call doRecv(). Therefore it is ok to expect that the data
			 * in sbuf will be preserved for copying into BUF in doRecv().
			 */

			return (long) Encdec.dec_uint16le(this.sbuf, 34) & 0xFFFF;
		}


		/// throws java.io.IOException
		protected internal override void doSend(Request request) {

			CommonServerMessageBlock smb = (CommonServerMessageBlock) request;
			byte[] buffer = this.getContext().getBufferCache().getBuffer();
			try {
				// synchronize around encode and write so that the ordering for SMB1 signing can be maintained
				lock (this.outLock) {
					int n = smb.encode(buffer, 4);
					//TODO remove message length limit
					Encdec.enc_uint32be(n , buffer, 0); 
					//Encdec.enc_uint32be(n & 0xFFFF, buffer, 0); // 4 byte session message header
					if (log.isTraceEnabled()) {
						do {
							log.trace(smb.ToString());
						} while (smb is AndXServerMessageBlock && (smb = ((AndXServerMessageBlock) smb).getAndx()) != null);
						log.trace(Hexdump.toHexString(buffer, 4, n));

					}
					/*
					 * For some reason this can sometimes get broken up into another
					 * "NBSS Continuation Message" frame according to WireShark
					 */

					this.@out.write(buffer, 0, 4 + n);
					this.@out.flush();
				}
			}
			finally {
				this.getContext().getBufferCache().releaseBuffer(buffer);
			}
		}


		/// throws java.io.IOException
		public virtual T sendrecv<T>(CommonServerMessageBlockRequest request, T response, ISet<RequestParam> @params) where T : CommonServerMessageBlockResponse {
			//TODO is T out
			//jcifs.internal.Request<?>
			if (request is jcifs.@internal.Request<CommonServerMessageBlockResponse> commonServerMessageBlockRequest) {
				if (response == null) {
					//TODO cast
					response = (T)commonServerMessageBlockRequest.initResponse(getContext());
				}
				else if (isSMB2()) {
					throw new IOException("Should not provide response argument for SMB2");
				}
			}
			else {
				request.setResponse(response);
			}
			if (response == null) {
				throw new IOException("Invalid response");
			}

			CommonServerMessageBlockRequest curHead = request;

			int maxSize = getContext().getConfig().getMaximumBufferSize();

			while (curHead != null) {
				CommonServerMessageBlockRequest nextHead = null;
				int totalSize = 0;
				int n = 0;
				CommonServerMessageBlockRequest last = null;
				CommonServerMessageBlockRequest chain = curHead;
				while (chain != null) {
					//n++;
					int size = chain.size();
					int cost = chain.getCreditCost();
					n += cost;
					CommonServerMessageBlockRequest next = chain.getNext();
					if (log.isTraceEnabled()) {
						log.trace(($"{chain.GetType().FullName} costs {cost} avail {this.credits.Permits()} ({this.name})"));
					}
					//TODO credits.tryAcquire(cost)
					//TODO 1 Credits
					if ((next == null || chain.allowChain(next)) && totalSize + size < maxSize && this.credits.Permits() > n) {
						totalSize += size;
						last = chain;
						chain = next;
					}
					else if (last == null && totalSize + size > maxSize) {
						throw new SmbException(string.Format("Request size {0:D} exceeds allowable size {1:D}: {2}", size, maxSize, chain));
					}
					else if (last == null) {
						// don't have enough credits/space for the first request, block until available
						// for space there is nothing we can do, callers need to make sure that a single message fits

						try {
							long timeout = getResponseTimeout(chain);
							if (@params.Contains(RequestParam.NO_TIMEOUT)) {
								this.credits.Acquire(cost);
							}
							else {
								if (!this.credits.tryAcquire(cost, TimeSpan.FromMilliseconds(timeout))) {
									throw new SmbException("Failed to acquire credits in time");
								}
							}
							totalSize += size;
							// split off first request

							lock (chain) {
								CommonServerMessageBlockRequest snext = chain.split();
								nextHead = snext;
								if (log.isDebugEnabled() && snext != null) {
									log.debug("Insufficient credits, send only first " + chain + " next is " + snext);
								}
							}
							break;
						}
						catch (ThreadInterruptedException e) {
							var ie = new InterruptedIOException("Interrupted while acquiring credits",e);
							//ie.initCause(e);
							throw ie;
						}
					}
					else {
						// not enough credits available or too big, split
						if (log.isDebugEnabled()) {
							log.debug("Not enough credits, split at " + last);
						}
						lock (last) {
							nextHead = last.split();
						}
						break;
					}
				}

				//TODO 1 Credits
				int reqCredits = Math.Max(n, this.desiredCredits - this.credits.Permits() - n + 1);
				if (log.isTraceEnabled()) {
					log.trace("Request credits " + reqCredits);
				}
				request.setRequestCredits(reqCredits);

				CommonServerMessageBlockRequest thisReq = curHead;
				try {
					//TODO
					CommonServerMessageBlockResponse resp = ((CommonServerMessageBlock)thisReq).getResponse();
					if (log.isTraceEnabled()) {
						log.trace("sendrecv " + thisReq);
					}
					resp = base.sendrecv(curHead, resp, @params);

					if (!checkStatus(curHead, resp)) {
						if (log.isDebugEnabled()) {
							log.debug("Breaking on error " + resp);
						}
						break;
					}

					if (nextHead != null) {
						// prepare remaining
						// (e.g. set session/tree/fileid returned by the previous requests)
						resp.prepare(nextHead);
					}
					curHead = nextHead;
				}
				finally {
					CommonServerMessageBlockRequest curReq = thisReq;
					CommonServerMessageBlock commonServerMessageBlock = curReq;
					int grantedCredits = 0;
					// if
					while (curReq != null) {
						if (curReq.isResponseAsync()) {
							log.trace("Async");
							break;
						}

						CommonServerMessageBlockResponse resp = commonServerMessageBlock.getResponse();

						if (resp.isReceived()) {
							grantedCredits += resp.getGrantedCredits();
						}
						CommonServerMessageBlockRequest next = curReq.getNext();
						if (next == null) {
							break;
						}
						curReq = next;
					}
					if (!isDisconnected() && !curReq.isResponseAsync() && !commonServerMessageBlock.getResponse().isAsync() && !commonServerMessageBlock.getResponse().isError() && grantedCredits == 0) {
						if (this.credits.Permits() > 0 || n > 0) {
							log.debug("Server " + this + " returned zero credits for " + curReq);
						}
						else {
							log.warn("Server " + this + " took away all our credits");
						}
					}
					else if (!curReq.isResponseAsync()) {
						if (log.isTraceEnabled()) {
							log.trace("Adding credits " + grantedCredits);
						}
						this.credits.Release(grantedCredits);
					}
				}
			}

			if (!response.isReceived()) {
				throw new IOException("No response", response.getException());
			}
			return response;

		}


		//TODO 1 type
		protected  override bool handleIntermediate<T>(Request request, T response) {
			if (!this.smb2) {
				return false;
			}
		//TODO type  jcifs.internal.smb2.ServerMessageBlock2Request<?> req = (jcifs.internal.smb2.ServerMessageBlock2Request<?>) request;

			//TODO 1 type
			ServerMessageBlock2 req = request as ServerMessageBlock2;
			ServerMessageBlock2Response resp =  response as ServerMessageBlock2Response;//check
			if (null == resp || resp ==null) {
				throw new ArgumentException($"request={request.GetType().FullName},response={response.GetType().FullName} is not smb2");
			}
			lock (resp) {
				if (resp.isAsync() && !resp.isAsyncHandled() && resp.getStatus() == NtStatus.NT_STATUS_PENDING && resp.getAsyncId() != 0) {
					resp.setAsyncHandled(true);
					bool first = !req.isAsync();
					req.setAsyncId(resp.getAsyncId());
					long? exp = resp.getExpiration();
					if (exp != null) {
						resp.setExpiration(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + getResponseTimeout(request));
					}
					if (log.isDebugEnabled()) {
						log.debug("Have intermediate reply " + response);
					}

					if (first) {
						int credit = resp.getCredit();
						if (log.isDebugEnabled()) {
							log.debug("Credit from intermediate " + credit);
						}
						this.credits.Release(credit);
					}
					return true;
				}
			}
			return false;
		}


		/// throws java.io.IOException
		protected internal virtual void doSend0(Request request) {
			try {
				doSend(request);
			}
			catch (IOException ioe) {
				log.warn("send failed", ioe);
				try {
					disconnect(true);
				}
				catch (IOException ioe2) {
					//TODO 
					//ioe.addSuppressed(ioe2);
					log.error("disconnect failed", ioe2);
				}
				throw ioe;
			}
		}


		// must be synchronized with peekKey
		/// throws java.io.IOException
		protected internal override void doRecv(Response response) {
			CommonServerMessageBlock resp = (CommonServerMessageBlock) response;
			this.negotiated.setupResponse(response);
			try {
				if (this.smb2) {
					doRecvSMB2(resp);
				}
				else {
					doRecvSMB1(resp);
				}
			}
			catch (Exception e) {
				log.warn("Failure decoding message, disconnecting transport", e);
				response.exception(e);
				lock (response) {
					Monitor.PulseAll(response);
				}
				throw e;
			}

		}


		/// <param name="response"> </param>
		/// <exception cref="IOException"> </exception>
		/// <exception cref="SMBProtocolDecodingException"> </exception>
		/// throws IOException, jcifs.internal.SMBProtocolDecodingException
		private void doRecvSMB2(CommonServerMessageBlock response) {
			int size = (Encdec.dec_uint16be(this.sbuf, 2) & 0xFFFF) | (this.sbuf[1] & 0xFF) << 16;
			if (size < (Smb2Constants.SMB2_HEADER_LENGTH + 1)) {
				throw new IOException("Invalid payload size: " + size);
			}

			if (this.sbuf[0] != (byte) 0x00 || this.sbuf[4] != unchecked((byte) 0xFE) || this.sbuf[5] != (byte) 'S' || this.sbuf[6] != (byte) 'M' || this.sbuf[7] != (byte) 'B') {
				throw new IOException("Houston we have a synchronization problem");
			}

			int nextCommand = Encdec.dec_uint32le(this.sbuf, 4 + 20);
			int maximumBufferSize = getContext().getConfig().getMaximumBufferSize();
			int msgSize = nextCommand != 0 ? nextCommand : size;
			if (msgSize > maximumBufferSize) {
				throw new IOException(string.Format("Message size {0:D} exceeds maxiumum buffer size {1:D}", msgSize, maximumBufferSize));
			}

			ServerMessageBlock2Response cur = (ServerMessageBlock2Response) response;
			byte[] buffer = getContext().getBufferCache().getBuffer();
			try {
				int rl = nextCommand != 0 ? nextCommand : size;

				// read and decode first
				Array.Copy(this.sbuf, 4, buffer, 0, Smb2Constants.SMB2_HEADER_LENGTH);
				readn(this.@in, buffer, Smb2Constants.SMB2_HEADER_LENGTH, rl - Smb2Constants.SMB2_HEADER_LENGTH);

				cur.setReadSize(rl);
				int len = cur.decode(buffer, 0);

				if (len > rl) {
					throw new IOException(string.Format("WHAT? ( read {0:D} decoded {1:D} ): {2}", rl, len, cur));
				}
				else if (nextCommand != 0 && len > nextCommand) {
					throw new IOException("Overlapping commands");
				}
				size -= rl;

				while (size > 0 && nextCommand != 0) {
					cur = (ServerMessageBlock2Response) cur.getNextResponse();
					if (cur == null) {
						log.warn("Response not properly set up");
						this.@in.skip(size);
						break;
					}

					// read next header
					readn(this.@in, buffer, 0, Smb2Constants.SMB2_HEADER_LENGTH);
					nextCommand = Encdec.dec_uint32le(buffer, 20);

					if ((nextCommand != 0 && nextCommand > maximumBufferSize) || (nextCommand == 0 && size > maximumBufferSize)) {
						throw new IOException(string.Format("Message size {0:D} exceeds maxiumum buffer size {1:D}", nextCommand != 0 ? nextCommand : size, maximumBufferSize));
					}

					rl = nextCommand != 0 ? nextCommand : size;

					if (log.isDebugEnabled()) {
						log.debug(string.Format("Compound next command {0:D} read size {1:D} remain {2:D}", nextCommand, rl, size));
					}

					cur.setReadSize(rl);
					readn(this.@in, buffer, Smb2Constants.SMB2_HEADER_LENGTH, rl - Smb2Constants.SMB2_HEADER_LENGTH);

					len = cur.decode(buffer, 0, true);
					if (len > rl) {
						throw new IOException(string.Format("WHAT? ( read {0:D} decoded {1:D} ): {2}", rl, len, cur));
					}
					else if (nextCommand != 0 && len > nextCommand) {
						throw new IOException("Overlapping commands");
					}
					size -= rl;
				}
			}
			finally {
				getContext().getBufferCache().releaseBuffer(buffer);
			}
		}


		/// <param name="resp"> </param>
		/// <exception cref="IOException"> </exception>
		/// <exception cref="SMBProtocolDecodingException"> </exception>
		/// throws IOException, jcifs.internal.SMBProtocolDecodingException
		private void doRecvSMB1(CommonServerMessageBlock resp) {
			byte[] buffer = getContext().getBufferCache().getBuffer();
			try {
				Array.Copy(this.sbuf, 0, buffer, 0, 4 + SmbConstants.SMB1_HEADER_LENGTH);
				int size = (Encdec.dec_uint16be(buffer, 2) & 0xFFFF);
				if (size < (SmbConstants.SMB1_HEADER_LENGTH + 1) || (4 + size) > Math.Min(0xFFFF, getContext().getConfig().getMaximumBufferSize())) {
					throw new IOException("Invalid payload size: " + size);
				}
				int errorCode = Encdec.dec_uint32le(buffer, 9) & unchecked((int)0xFFFFFFFF);
				if (resp.getCommand() == ServerMessageBlock.SMB_COM_READ_ANDX && (errorCode == 0 || errorCode == NtStatus.NT_STATUS_BUFFER_OVERFLOW)) {
					// overflow indicator normal for pipe
					SmbComReadAndXResponse r = (SmbComReadAndXResponse) resp;
					int off = SmbConstants.SMB1_HEADER_LENGTH;
					/* WordCount thru dataOffset always 27 */
					readn(this.@in, buffer, 4 + off, 27);
					off += 27;
					resp.decode(buffer, 4);
					/* EMC can send pad w/o data */
					int pad = r.getDataOffset() - off;
					if (r.getByteCount() > 0 && pad > 0 && pad < 4) {
						readn(this.@in, buffer, 4 + off, pad);
					}

					if (r.getDataLength() > 0) {
						readn(this.@in, r.getData(), r.getOffset(), r.getDataLength()); // read direct
					}
				}
				else {
					readn(this.@in, buffer, 4 + SmbConstants.SMB1_HEADER_LENGTH, size - SmbConstants.SMB1_HEADER_LENGTH);
					resp.decode(buffer, 4);
				}
			}
			finally {
				getContext().getBufferCache().releaseBuffer(buffer);
			}
		}


		/// throws java.io.IOException
		protected internal override void doSkip(long? key) {
			lock (this.inLock) {
				int size = Encdec.dec_uint16be(this.sbuf, 2) & 0xFFFF;
				if (size < 33 || (4 + size) > this.getContext().getConfig().getReceiveBufferSize()) {
					//TODO 
					log.warn("Flusing stream input");
					this.@in.skip(this.@in.available());
				}
				else {
					Response notification = createNotification(key);
					if (notification != null) {
						log.debug("Parsing notification");
						doRecv(notification);
						handleNotification(notification);
						return;
					}
					log.warn("Skipping message " + key);
					if (this.isSMB2()) {
						this.@in.skip(size - Smb2Constants.SMB2_HEADER_LENGTH);
					}
					else {
						this.@in.skip(size - SmbConstants.SMB1_HEADER_LENGTH);
					}
				}
			}
		}


		/// <param name="notification"> </param>
		protected internal virtual void handleNotification(Response notification) {
			log.info("Received notification " + notification);
		}

		/// <param name="key">
		/// @return </param>
		/// <exception cref="SmbException"> </exception>
		/// throws SmbException
		protected internal virtual Response createNotification(long? key) {
			if (key == null) {
				// no valid header
				return null;
			}
			if (this.smb2) {
				if (key != -1) {
					return null;
				}
				int cmd = Encdec.dec_uint16le(this.sbuf, 4 + 12) & 0xFFFF;
				if (cmd == 0x12) {
					return new Smb2OplockBreakNotification(getContext().getConfig());
				}
			}
			else {
				if (key != 0xFFFF) {
					return null;
				}
				int cmd = this.sbuf[4 + 4];
				if (cmd == 0x24) {
					return new SmbComLockingAndX(getContext().getConfig());
				}
			}
			return null;
		}


		/// throws SmbException
		internal virtual bool checkStatus1(ServerMessageBlock req, ServerMessageBlock resp) {
			bool cont = false;
			if (resp.getErrorCode() == 0x30002) {
				// if using DOS error codes this indicates a DFS referral
				resp.setErrorCode(NtStatus.NT_STATUS_PATH_NOT_COVERED);
			}
			else {
				resp.setErrorCode(SmbException.getStatusByCode(resp.getErrorCode()));
			}
			switch (resp.getErrorCode()) {
			case NtStatus.NT_STATUS_OK:
				cont = true;
				break;
			case NtStatus.NT_STATUS_ACCESS_DENIED:
			case NtStatus.NT_STATUS_WRONG_PASSWORD:
			case NtStatus.NT_STATUS_LOGON_FAILURE:
			case NtStatus.NT_STATUS_ACCOUNT_RESTRICTION:
			case NtStatus.NT_STATUS_INVALID_LOGON_HOURS:
			case NtStatus.NT_STATUS_INVALID_WORKSTATION:
			case NtStatus.NT_STATUS_PASSWORD_EXPIRED:
			case NtStatus.NT_STATUS_ACCOUNT_DISABLED:
			case NtStatus.NT_STATUS_ACCOUNT_LOCKED_OUT:
			case NtStatus.NT_STATUS_TRUSTED_DOMAIN_FAILURE:
				throw new SmbAuthException(resp.getErrorCode());
			case unchecked((int)0xC00000BB): // NT_STATUS_NOT_SUPPORTED
				throw new SmbUnsupportedOperationException();
			case NtStatus.NT_STATUS_PATH_NOT_COVERED:
				// samba fails to report the proper status for some operations
			case unchecked((int)0xC00000A2):// NT_STATUS_MEDIA_WRITE_PROTECTED
				checkReferral(resp, req.getPath(), req);
				break;
			case NtStatus.NT_STATUS_BUFFER_OVERFLOW:
				break; // normal for DCERPC named pipes
			case NtStatus.NT_STATUS_MORE_PROCESSING_REQUIRED:
				break; // normal for NTLMSSP
			default:
				if (log.isDebugEnabled()) {
					log.debug("Error code: 0x" + Hexdump.toHexString(resp.getErrorCode(), 8) + " for " + req.GetType().Name);
				}
				throw new SmbException(resp.getErrorCode(), null);
			}
			if (resp.isVerifyFailed()) {
				throw new SmbException("Signature verification failed.");
			}
			return cont;
		}


		/// <param name="request"> </param>
		/// <param name="response"> </param>
		/// <exception cref="SmbException"> </exception>
		/// throws SmbException
		internal virtual bool checkStatus2(ServerMessageBlock2 req, jcifs.util.transport.Response resp) {
			bool cont = false;
			switch (resp.getErrorCode()) {
			case NtStatus.NT_STATUS_OK:
			case NtStatus.NT_STATUS_NO_MORE_FILES:
				cont = true;
				break;
			case NtStatus.NT_STATUS_PENDING:
				// must be the last
				cont = false;
				break;
			case NtStatus.NT_STATUS_ACCESS_DENIED:
			case NtStatus.NT_STATUS_WRONG_PASSWORD:
			case NtStatus.NT_STATUS_LOGON_FAILURE:
			case NtStatus.NT_STATUS_ACCOUNT_RESTRICTION:
			case NtStatus.NT_STATUS_INVALID_LOGON_HOURS:
			case NtStatus.NT_STATUS_INVALID_WORKSTATION:
			case NtStatus.NT_STATUS_PASSWORD_EXPIRED:
			case NtStatus.NT_STATUS_ACCOUNT_DISABLED:
			case NtStatus.NT_STATUS_ACCOUNT_LOCKED_OUT:
			case NtStatus.NT_STATUS_TRUSTED_DOMAIN_FAILURE:
				throw new SmbAuthException(resp.getErrorCode());
			case NtStatus.NT_STATUS_MORE_PROCESSING_REQUIRED:
				break; // normal for SPNEGO
			case 0x10B: // NT_STATUS_NOTIFY_CLEANUP:
			case NtStatus.NT_STATUS_NOTIFY_ENUM_DIR:
				break;
			case unchecked((int)0xC00000BB): // NT_STATUS_NOT_SUPPORTED
			case unchecked((int)0xC0000010):  // NT_STATUS_INVALID_DEVICE_REQUEST
				throw new SmbUnsupportedOperationException();
			case NtStatus.NT_STATUS_PATH_NOT_COVERED:
				if (!(req is RequestWithPath)) {
					throw new SmbException("Invalid request for a DFS NT_STATUS_PATH_NOT_COVERED response " + req.GetType().FullName);
				}
				string path = ((RequestWithPath) req).getFullUNCPath();
				checkReferral(resp, path, ((RequestWithPath) req));
				// checkReferral always throws and exception but put break here for clarity
				break;
			case NtStatus.NT_STATUS_BUFFER_OVERFLOW:
				if (resp is Smb2ReadResponse) {
					break;
				}
				if (resp is Smb2IoctlResponse) {
					int ctlCode = ((Smb2IoctlResponse) resp).getCtlCode();
					if (ctlCode == Smb2IoctlRequest.FSCTL_PIPE_TRANSCEIVE || ctlCode == Smb2IoctlRequest.FSCTL_PIPE_PEEK) {
						break;
					}
				}
				// fall through
				goto default;
			default:
				if (log.isDebugEnabled()) {
					log.debug("Error code: 0x" + Hexdump.toHexString(resp.getErrorCode(), 8) + " for " + req.GetType().Name);
				}
				throw new SmbException(resp.getErrorCode(), null);
			}
			if (resp.isVerifyFailed()) {
				throw new SMBSignatureValidationException("Signature verification failed.");
			}
			return cont;
		}


		/// <param name="resp"> </param>
		/// <param name="path"> </param>
		/// <param name="req"> </param>
		/// <exception cref="SmbException"> </exception>
		/// <exception cref="DfsReferral"> </exception>
		/// throws SmbException, DfsReferral
		private void checkReferral(Response resp, string path, RequestWithPath req) {
			DfsReferralData dr = null;
			if (!getContext().getConfig().isDfsDisabled()) {
				try {
					dr = getDfsReferrals(getContext(), path, req.getServer(), req.getDomain(), 1);
				}
				catch (CIFSException e) {
					throw new SmbException("Failed to get DFS referral", e);
				}
			}
			if (dr == null) {
				if (log.isDebugEnabled()) {
					log.debug("Error code: 0x" + Hexdump.toHexString(resp.getErrorCode(), 8));
				}
				throw new SmbException(resp.getErrorCode(), null);
			}

			if (req.getDomain()!=null && getContext().getConfig().isDfsConvertToFQDN() && dr is DfsReferralDataImpl) {
				((DfsReferralDataImpl) dr).fixupDomain(req.getDomain());
			}
			if (log.isDebugEnabled()) {
				log.debug("Got referral " + dr);
			}

			getContext().getDfs().cache(getContext(), path, dr);
			throw new DfsReferral(dr);
		}


		/// throws SmbException
		internal virtual T send<T>(CommonServerMessageBlockRequest request, T response) where T : CommonServerMessageBlockResponse {
			return send(request, response, new HashSet<RequestParam>(0));
		}


		/// throws SmbException
		internal virtual T send<T>(CommonServerMessageBlockRequest request, T response, ISet<RequestParam> @params) where T : CommonServerMessageBlockResponse {
			ensureConnected(); // must negotiate before we can test flags2, useUnicode, etc
			if (this.smb2 && !(request is ServerMessageBlock2)) {
				throw new SmbException("Not an SMB2 request " + request.GetType().FullName);
			}
			else if (!this.smb2 && !(request is ServerMessageBlock)) {
				throw new SmbException("Not an SMB1 request");
			}

			this.negotiated.setupRequest(request);

			if (response != null) {
				request.setResponse(response); // needed by sign
				response.setDigest(request.getDigest());
			}

			try {
				if (log.isTraceEnabled()) {
					log.trace("send  " + request);
				}
				if (request.isCancel()) {
                 					doSend0(request);
                 					return default(T);
                 				}
				else if (request is SmbComTransaction) {
					response = sendComTransaction(request, response, @params);
				}
				else {
					if (response != null) {
						response.setCommand(request.getCommand());
					}
					response = sendrecv(request, response, @params);
				}
			}
			catch (SmbException se) {
				throw se;
			}
			catch (IOException ioe) {
				throw new SmbException(ioe.Message, ioe);
			}

			if (log.isTraceEnabled()) {
				log.trace("Response is " + response);
			}

			checkStatus(request, response);
			return response;
		}


		/// <param name="request"> </param>
		/// <param name="response"> </param>
		/// <exception cref="SmbException"> </exception>
		/// throws SmbException
		private bool checkStatus<T>(CommonServerMessageBlockRequest request, T response) where T : CommonServerMessageBlockResponse {
			CommonServerMessageBlockRequest cur = request;
			//TODO is
			while (cur != null && cur is  jcifs.util.transport.Request utilRequest) {
				if (this.smb2) {
					if (!checkStatus2((ServerMessageBlock2) cur, utilRequest.getResponse())) {
						return false;
					}
				}
				else {
					if (!checkStatus1((ServerMessageBlock) cur, (ServerMessageBlock) utilRequest.getResponse())) {
						return false;
					}
				}
				cur = cur.getNext();
			}
			return true;
		}


		/// <param name="request"> </param>
		/// <param name="response"> </param>
		/// <param name="params"> </param>
		/// <exception cref="IOException"> </exception>
		/// <exception cref="SmbException"> </exception>
		/// <exception cref="TransportException"> </exception>
		/// <exception cref="EOFException"> </exception>
		/// throws IOException, SmbException, TransportException, java.io.EOFException
		private T sendComTransaction<T>(CommonServerMessageBlockRequest request, T response, ISet<RequestParam> @params) where T : CommonServerMessageBlock, jcifs.util.transport.Response {
			response.setCommand(request.getCommand());
			SmbComTransaction req = (SmbComTransaction) request;
			//TODO 
			SmbComTransactionResponse resp =response as SmbComTransactionResponse;
			if (null == resp)
			{
				throw new InvalidCastException();
			}

			resp.reset();

			long k;

			/*
			 * First request w/ interim response
			 */
			try {
				req.setBuffer(getContext().getBufferCache().getBuffer());
				req.nextElement();
				if (req.hasMoreElements()) {
					SmbComBlankResponse interim = new SmbComBlankResponse(getContext().getConfig());
					base.sendrecv(req, interim, @params);
					if (interim.getErrorCode() != 0) {
						checkStatus(req, interim);
					}
					k = req.nextElement().getMid();
				}
				else {
					k = makeKey(req);
				}

				try {
					resp.clearReceived();
					long timeout = getResponseTimeout(req);
					if (!@params.Contains(RequestParam.NO_TIMEOUT)) {
						resp.setExpiration(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + timeout);
					}
					else {
						resp.setExpiration(null);
					}

					byte[] txbuf = getContext().getBufferCache().getBuffer();
					resp.setBuffer(txbuf);

					this.responseMap[k] = resp;

					/*
					 * Send multiple fragments
					 */

					do {
						doSend0(req);
					} while (req.hasMoreElements() && req.nextElement() != null);

					/*
					 * Receive multiple fragments
					 */
					lock (resp) {
						while (!resp.isReceived() || resp.hasMoreElements()) {
							if (!@params.Contains(RequestParam.NO_TIMEOUT)) {
								Monitor.Wait(resp, TimeSpan.FromMilliseconds(timeout));
								timeout = resp.getExpiration().Value - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
								if (timeout <= 0) {
									throw new TransportException(this + " timedout waiting for response to " + req);
								}
							}
							else {
								Monitor.Wait(resp);
								if (log.isTraceEnabled()) {
									log.trace("Wait returned " + isDisconnected());
								}
								if (isDisconnected()) {
									throw new IOException("Transport closed while waiting for result");
								}
							}
						}
					}

					if (!resp.isReceived()) {
						throw new TransportException("Failed to read response");
					}

					if (resp.getErrorCode() != 0) {
						checkStatus(req, resp);
					}
					return response;
				}
				finally {
					this.responseMap.Remove(k);
					getContext().getBufferCache().releaseBuffer(resp.releaseBuffer());
				}
			}
			catch (ThreadInterruptedException ie) {
				throw new TransportException(ie);
			}
			finally {
				getContext().getBufferCache().releaseBuffer(req.releaseBuffer());
			}

		}


		public override string ToString() {
			return base.ToString() + "[" + this.address + ":" + this.port + ",state=" + this.state + ",signingEnforced=" + this.signingEnforced + ",usage=" + this.getUsageCount() + "]";
		}


		/* DFS */
		/// throws jcifs.CIFSException
		public virtual DfsReferralData getDfsReferrals(CIFSContext ctx, string path, string targetHost, string targetDomain, int rn) {
			if (log.isDebugEnabled()) {
				log.debug("Resolving DFS path " + path);
			}

			if (path.Length >= 2 && path[0] == '\\' && path[1] == '\\') {
				throw new SmbException("Path must not start with double slash: " + path);
			}

			//TODO
			using (SmbSessionImpl sess = (SmbSessionImpl)getSmbSession(ctx, targetHost, targetDomain))
			using (SmbTransportImpl transport = (SmbTransportImpl)sess.getTransport())
			using (	SmbTreeImpl ipc = (SmbTreeImpl)sess.getSmbTree("IPC$", null)) {

				DfsReferralRequestBuffer dfsReq = new DfsReferralRequestBuffer(path, 3);
				DfsReferralResponseBuffer dfsResp;
				if (isSMB2()) {
					Smb2IoctlRequest req = new Smb2IoctlRequest(ctx.getConfig(), Smb2IoctlRequest.FSCTL_DFS_GET_REFERRALS);
					req.setFlags(Smb2IoctlRequest.SMB2_O_IOCTL_IS_FSCTL);
					req.setInputData(dfsReq);
					dfsResp = ipc.send(req).getOutputData<DfsReferralResponseBuffer>(typeof(DfsReferralResponseBuffer));
				}
				else {
					Trans2GetDfsReferralResponse resp = new Trans2GetDfsReferralResponse(ctx.getConfig());
					ipc.send(new Trans2GetDfsReferral(ctx.getConfig(), path), resp);
					dfsResp = resp.getDfsResponse();
				}

				if (dfsResp.getNumReferrals() == 0) {
					return null;
				}
				else if (rn == 0 || dfsResp.getNumReferrals() < rn) {
					rn = dfsResp.getNumReferrals();
				}

				DfsReferralDataImpl cur = null;
				long expiration = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + (ctx.getConfig().getDfsTtl() * 1000);
				Referral[] refs = dfsResp.getReferrals();
				for (int di = 0; di < rn; di++) {
					DfsReferralDataImpl dr = DfsReferralDataImpl.fromReferral(refs[di], path, expiration, dfsResp.getPathConsumed());
					dr.setDomain(targetDomain);

					if ((dfsResp.getTflags() & 0x2) == 0 && (dr.getFlags() & 0x2) == 0) {
						log.debug("Non-root referral is not final " + dfsResp);
						dr.intermediate();
					}

					if (cur == null) {
						cur = dr;
					}
					else {
						cur.append(dr);
						cur = dr;
					}
				}

				if (log.isDebugEnabled()) {
					log.debug("Got referral " + cur);
				}
				return cur;
			}
		}


		internal virtual byte[] getPreauthIntegrityHash() {
			return this.preauthIntegrityHash;
		}


		/// throws jcifs.CIFSException
		private void updatePreauthHash(byte[] input) {
			lock (this.preauthIntegrityHash) {
				this.preauthIntegrityHash = calculatePreauthHash(input, 0, input.Length, this.preauthIntegrityHash);
			}
		}


		/// throws jcifs.CIFSException
		internal virtual byte[] calculatePreauthHash(byte[] input, int off, int len, byte[] oldHash) {
			if (!this.smb2 || this.negotiated == null) {
				throw new SmbUnsupportedOperationException();
			}

			Smb2NegotiateResponse resp = (Smb2NegotiateResponse) this.negotiated;
			if (!resp.getSelectedDialect().atLeast(DialectVersion.SMB311)) {
				throw new SmbUnsupportedOperationException();
			}

			MessageDigest dgst;
			switch (resp.getSelectedPreauthHash()) {
			case 1:
				dgst = Crypto.getSHA512();
				break;
			default:
				throw new SmbUnsupportedOperationException();
			}

			if (oldHash != null) {
				dgst.update(oldHash);
			}
			dgst.update(input, off, len);
			return dgst.digest();
		}


		/// throws jcifs.CIFSException
		internal virtual Cipher createEncryptionCipher(byte[] key) {
			if (!this.smb2 || this.negotiated == null) {
				throw new SmbUnsupportedOperationException();
			}

			Smb2NegotiateResponse resp = (Smb2NegotiateResponse) this.negotiated;
			int cipherId = -1;

			if (resp.getSelectedDialect().atLeast(DialectVersion.SMB311)) {
				cipherId = resp.getSelectedCipher();
			}
			else if (resp.getSelectedDialect().atLeast(DialectVersion.SMB300)) {
				cipherId = EncryptionNegotiateContext.CIPHER_AES128_CCM;
			}
			else {
				throw new SmbUnsupportedOperationException();
			}

			switch (cipherId) {
			case EncryptionNegotiateContext.CIPHER_AES128_CCM:
			case EncryptionNegotiateContext.CIPHER_AES128_GCM:
			default:
				throw new SmbUnsupportedOperationException();
			}
		}


		public virtual int getRequestSecurityMode(Smb2NegotiateResponse first) {
			int securityMode = Smb2Constants.SMB2_NEGOTIATE_SIGNING_ENABLED;
			if (this.signingEnforced || (first != null && first.isSigningRequired())) {
				securityMode = Smb2Constants.SMB2_NEGOTIATE_SIGNING_REQUIRED | Smb2Constants.SMB2_NEGOTIATE_SIGNING_ENABLED;
			}

			return securityMode;
		}
	}

}