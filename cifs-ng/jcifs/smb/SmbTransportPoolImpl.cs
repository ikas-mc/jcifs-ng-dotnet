using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using cifs_ng.lib;
using cifs_ng.lib.ext;
using cifs_ng.lib.socket;
using Logger = org.slf4j.Logger;
using LoggerFactory = org.slf4j.LoggerFactory;
using Address = jcifs.Address;
using CIFSContext = jcifs.CIFSContext;
using CIFSException = jcifs.CIFSException;
using SmbConstants = jcifs.SmbConstants;
using SmbTransport = jcifs.SmbTransport;
using SmbTransportPool = jcifs.SmbTransportPool;
using TransportException = jcifs.util.transport.TransportException;

/*
 * © 2016 AgNO3 Gmbh & Co. KG
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
	/// @author mbechler
	/// @internal
	/// </summary>
	public class SmbTransportPoolImpl : SmbTransportPool {

		private static readonly Logger log = LoggerFactory.getLogger(typeof(SmbTransportPoolImpl));

		private readonly IList<SmbTransportImpl> connections = new List<SmbTransportImpl>();//TODO link
		private readonly IList<SmbTransportImpl> nonPooledConnections = new List<SmbTransportImpl>();//TODO link
		private readonly  ConcurrentQueue<SmbTransportImpl> toRemove = new ConcurrentQueue<SmbTransportImpl>();
		internal readonly IDictionary<string, int> failCounts = new ConcurrentDictionary<string, int>();


		public virtual SmbTransport getSmbTransport(CIFSContext tc, Address address, int port, bool nonPooled) {
			return getSmbTransport(tc, address, port, tc.getConfig().getLocalAddr(), tc.getConfig().getLocalPort(), null, nonPooled);
		}


		public virtual SmbTransport getSmbTransport(CIFSContext tc, Address address, int port, bool nonPooled, bool forceSigning) {
			return getSmbTransport(tc, address, port, tc.getConfig().getLocalAddr(), tc.getConfig().getLocalPort(), null, nonPooled, forceSigning);
		}


		public virtual SmbTransport getSmbTransport(CIFSContext tc, Address address, int port, IPAddress localAddr, int localPort, string hostName, bool nonPooled) {
			return getSmbTransport(tc, address, port, localAddr, localPort, hostName, nonPooled, false);
		}


		public virtual SmbTransport getSmbTransport(CIFSContext tc, Address address, int port, IPAddress localAddr, int localPort, string hostName, bool nonPooled, bool forceSigning) {
			if (port <= 0) {
				port = SmbConstants.DEFAULT_PORT;
			}
			lock (this.connections) {
				cleanup();
				if (log.isTraceEnabled()) {
					log.trace("Exclusive " + nonPooled + " enforced signing " + forceSigning);
				}
				if (!nonPooled && tc.getConfig().getSessionLimit() != 1) {
					SmbTransportImpl existing = findConnection(tc, address, port, localAddr, localPort, hostName, forceSigning, false);
					if (existing != null) {
						return existing;
					}
				}
				SmbTransportImpl conn = new SmbTransportImpl(tc, address, port, localAddr, localPort, forceSigning);
				if (log.isDebugEnabled()) {
					log.debug("New transport connection " + conn);
				}
				if (nonPooled) {
					this.nonPooledConnections.Add(conn);
				}
				else {
					this.connections.Insert(0, conn);
				}
				return conn;
			}
		}


		/// <param name="tc"> </param>
		/// <param name="address"> </param>
		/// <param name="port"> </param>
		/// <param name="localAddr"> </param>
		/// <param name="localPort"> </param>
		/// <param name="hostName"> </param>
		/// <param name="forceSigning">
		/// @return </param>
		private SmbTransportImpl findConnection(CIFSContext tc, Address address, int port, IPAddress localAddr, int localPort, string hostName, bool forceSigning, bool connectedOnly) {
			foreach (SmbTransportImpl conn in this.connections) {
				if (conn.matches(address, port, localAddr, localPort, hostName) && (tc.getConfig().getSessionLimit() == 0 || conn.getNumSessions() < tc.getConfig().getSessionLimit())) {
					try {
						if (conn.isFailed() || (connectedOnly && conn.isDisconnected())) {
							continue;
						}

						if (forceSigning && !conn.isSigningEnforced()) {
							// if signing is enforced and was not on the connection, skip
							if (log.isTraceEnabled()) {
								log.debug("Cannot reuse, signing enforced but connection does not have it enabled " + conn);
							}
							continue;
						}

						if (!forceSigning && !tc.getConfig().isSigningEnforced() && conn.isSigningEnforced() && !conn.getNegotiateResponse().isSigningRequired()) {
							// if signing is not enforced, dont use connections that have signing enforced
							// for purposes that dont require it.
							if (log.isTraceEnabled()) {
								log.debug("Cannot reuse, signing enforced on connection " + conn);
							}
							continue;
						}

						if (!conn.getNegotiateResponse().canReuse(tc, forceSigning)) {
							if (log.isTraceEnabled()) {
								log.trace("Cannot reuse, different config " + conn);
							}
							continue;
						}
					}
					catch (CIFSException e) {
						log.debug("Error while checking for reuse", e);
						continue;
					}

					if (log.isTraceEnabled()) {
						log.trace("Reusing transport connection " + conn);
					}
					return (SmbTransportImpl)conn.acquire();
				}
			}

			return null;
		}


		/// throws java.io.IOException
		public virtual SmbTransport getSmbTransport(CIFSContext tf, string name, int port, bool exclusive, bool forceSigning) {

			Address[] addrs = tf.getNameServiceClient().getAllByName(name, true);

			if (addrs == null || addrs.Length == 0) {
				throw new UnknownHostException(name);
			}

			Array.Sort(addrs, new ComparatorAnonymousInnerClass(this));

			lock (this.connections) {
				foreach (Address addr in addrs) {
					SmbTransportImpl found = findConnection(tf, addr, port, tf.getConfig().getLocalAddr(), tf.getConfig().getLocalPort(), name, forceSigning, true);
					if (found != null) {
						return found;
					}
				}
			}

			IOException ex = null;
			foreach (Address addr in addrs) {
				if (log.isDebugEnabled()) {
					log.debug("Trying address "+addr);
				}

				try
				{
					using (SmbTransportImpl trans = getSmbTransport(tf, addr, port, exclusive, forceSigning).unwrap<SmbTransportImpl>(typeof(SmbTransportImpl)))
					{
						try
						{
							trans.ensureConnected();
						}
						catch (IOException e)
						{
							removeTransport(trans);
							throw e;
						}

						return (SmbTransportImpl) trans.acquire();
					}
				}
				catch (IOException e)
				{
					string hostAddress = addr.getHostAddress();
					int? failCount = this.failCounts.get(hostAddress);
					if (failCount == null)
					{
						this.failCounts[hostAddress] = 1;
					}
					else
					{
						this.failCounts[hostAddress] = failCount.Value + 1;
					}

					ex = e;
				}
				catch (Exception e)
				{
					Debug.WriteLine(e.StackTrace);
					throw;
				}
			}

			if (ex != null) {
				throw ex;
			}
			throw new TransportException("All connection attempts failed");
		}

		private class ComparatorAnonymousInnerClass : IComparer<Address> {
			private readonly SmbTransportPoolImpl outerInstance;

			public ComparatorAnonymousInnerClass(SmbTransportPoolImpl outerInstance) {
				this.outerInstance = outerInstance;
			}


			public int Compare(Address o1, Address o2) {
				int? fail1 = outerInstance.failCounts.get(o1.getHostAddress());
				int? fail2 = outerInstance.failCounts.get(o2.getHostAddress());
				if (fail1 == null) {
					fail1 = 0;
				}
				if (fail2 == null) {
					fail2 = 0;
				}
				return (fail1 < fail2) ? -1 : ((fail1 == fail2) ? 0 : 1);
			}

		}


		/// 
		/// <param name="trans"> </param>
		/// <returns> whether (non-exclusive) connection is in the pool </returns>
		public virtual bool contains(SmbTransport trans) {
			lock (this.connections) {
				cleanup();
				if (! (trans is SmbTransportImpl impl))
				{
					return false;
				}
				return this.connections.Contains(impl);
			}
		}


		public virtual void removeTransport(SmbTransport trans) {
			if (log.isDebugEnabled()) {
				log.debug("Scheduling transport connection for removal " + trans + " (" + RuntimeHelp.identityHashCode(trans) + ")");
			}
			this.toRemove.Enqueue((SmbTransportImpl) trans);
		}


		private void cleanup() {
			lock (this.connections)
			{
				SmbTransportImpl trans = null;
				this.toRemove.TryDequeue(out  trans);
				while (trans != null) {
					if (log.isDebugEnabled()) {
						log.debug("Removing transport connection " + trans + " (" + RuntimeHelp.identityHashCode(trans) + ")");
					}
					this.connections.Remove(trans);
					this.nonPooledConnections.Remove(trans);
					this.toRemove.TryDequeue(out  trans);
				}
			}
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.SmbTransportPool#Dispose() </seealso>
		/// throws jcifs.CIFSException
		public virtual bool Dispose() {
			bool inUse = false;

			List<SmbTransportImpl> toClose;
			lock (this.connections) {
				cleanup();
				log.debug("Closing pool");
				toClose = new List<SmbTransportImpl>(this.connections);
				toClose.AddRange(this.nonPooledConnections);
				this.connections.Clear();
				this.nonPooledConnections.Clear();
			}
			foreach (SmbTransportImpl conn in toClose) {
				try {
					inUse |= conn.disconnect(false, false);
				}
				catch (IOException e) {
					log.warn("Failed to close connection", e);
				}
			}
			lock (this.connections) {
				cleanup();
			}
			return inUse;
		}


		/// throws SmbException
		public virtual byte[] getChallenge(CIFSContext tf, Address dc) {
			return getChallenge(tf, dc, 0);
		}


		/// throws SmbException
		public virtual byte[] getChallenge(CIFSContext tf, Address dc, int port) {
			try {
					using (SmbTransportInternal trans = tf.getTransportPool().getSmbTransport(tf, dc, port, false, !tf.getCredentials().isAnonymous() && tf.getConfig().isIpcSigningEnforced()).unwrap<SmbTransportInternal>(typeof(SmbTransportInternal))) {
					trans.ensureConnected();
					return trans.getServerEncryptionKey();
					}
			}
			catch (SmbException e) {
				throw e;
			}
			catch (IOException e) {
				throw new SmbException("Connection failed", e);
			}
		}


		/// throws SmbException
		[Obsolete]
		public virtual void logon(CIFSContext tf, Address dc) {
			logon(tf, dc, 0);
		}


		/// throws SmbException
		[Obsolete]
		public virtual void logon(CIFSContext tf, Address dc, int port) {
			using (SmbTransportInternal smbTransport = tf.getTransportPool().getSmbTransport(tf, dc, port, false, tf.getConfig().isIpcSigningEnforced()).unwrap<SmbTransportInternal>(typeof(SmbTransportInternal)))
			using (	SmbSessionInternal smbSession = smbTransport.getSmbSession(tf, dc.getHostName(), null).unwrap<SmbSessionInternal>(typeof(SmbSessionInternal)))
			using (	SmbTreeInternal tree = smbSession.getSmbTree(tf.getConfig().getLogonShare(), null).unwrap<SmbTreeInternal>(typeof(SmbTreeInternal))) {
				tree.connectLogon(tf);
			}
		}

	}

}