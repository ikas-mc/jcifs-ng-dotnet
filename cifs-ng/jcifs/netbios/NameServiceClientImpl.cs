using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using cifs_ng.lib;
using cifs_ng.lib.ext;
using cifs_ng.lib.socket;
using cifs_ng.lib.threading;
using jcifs.lib;
using Logger = org.slf4j.Logger;
using LoggerFactory = org.slf4j.LoggerFactory;
using CIFSContext = jcifs.CIFSContext;
using Configuration = jcifs.Configuration;
using NameServiceClient = jcifs.NameServiceClient;
using NetbiosAddress = jcifs.NetbiosAddress;
using ResolverType = jcifs.ResolverType;
using RuntimeCIFSException = jcifs.RuntimeCIFSException;
using SmbConstants = jcifs.SmbConstants;
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

namespace jcifs.netbios {
	/// 
	/// <summary>
	/// @author mbechler
	/// 
	/// </summary>
	public class NameServiceClientImpl : Runnable, NameServiceClient {

		private static Logger logger = LoggerFactory.getLogger(typeof(NameServiceClientImpl));

		private const int NAME_SERVICE_UDP_PORT = 137;

		internal static readonly byte[] UNKNOWN_MAC_ADDRESS = new byte[] {(byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00};

		private static readonly Logger log = LoggerFactory.getLogger(typeof(NameServiceClientImpl));

		private readonly object LOCK = new object();

		private int nbnsIndex = 0;

		private readonly IDictionary<Name, CacheEntry> addressCache = new Dictionary<Name, CacheEntry>();
		private readonly ISet<Name> inFlightLookups = new HashSet<Name>();

		private int lport;
		private int closeTimeout;
		private byte[] snd_buf, rcv_buf;
		private SocketEx socket;
		private IDictionary<int?, NameServicePacket> responseTable = new Dictionary<int?, NameServicePacket>();
		private Thread thread;
		private int nextNameTrnId = 0;
		private IList<ResolverType> resolveOrder = new List<ResolverType>();

		private IPAddress laddr, baddr;
		private CIFSContext transportContext;
		private NbtAddress localhostAddress;

		private Lmhosts lmhosts = new Lmhosts();
		private Name unknownName;
		private NbtAddress unknownAddress;


		private int SndBufSize;

		private int RcvBufSize;
		/// 
		/// <param name="tc"> </param>
		public NameServiceClientImpl(CIFSContext tc) : this(tc.getConfig().getNetbiosLocalPort(), tc.getConfig().getNetbiosLocalAddress(), tc) {
		}


		internal NameServiceClientImpl(int lport, IPAddress laddr, CIFSContext tc) {
			this.lport = lport;
			this.laddr = laddr;
			this.transportContext = tc;

			this.baddr = tc.getConfig().getBroadcastAddress();

			SndBufSize = tc.getConfig().getNetbiosSndBufSize();
			RcvBufSize = tc.getConfig().getNetbiosRcvBufSize();
			
			this.snd_buf = new byte[SndBufSize];
			this.rcv_buf = new byte[RcvBufSize];

			this.resolveOrder = tc.getConfig().getResolveOrder();

			initCache(tc);
		}

		internal sealed class CacheEntry {

			internal Name hostName;
			internal NbtAddress address;
			internal long expiration;


			internal CacheEntry(Name hostName, NbtAddress address, long expiration) {
				this.hostName = hostName;
				this.address = address;
				this.expiration = expiration;
			}
		}


		/// 
		private void initCache(CIFSContext tc) {
			this.unknownName = new Name(tc.getConfig(), "0.0.0.0", 0x00, null);
			this.unknownAddress = new NbtAddress(this.unknownName, 0, false, NbtAddress.B_NODE);
			this.addressCache[this.unknownName] = new CacheEntry(this.unknownName, this.unknownAddress, SmbConstants.FOREVER);

			/*
			 * Determine the IPAddress of the local interface
			 * if one was not specified.
			 */
			IPAddress localInetAddress = tc.getConfig().getNetbiosLocalAddress();
			if (localInetAddress == null) {
				try
				{
					//TODO 
					localInetAddress = tc.getConfig().getLocalAddr();
					if (null==localInetAddress)
					{
						//TODO 
						localInetAddress = Dns.GetHostAddresses(Dns.GetHostName()).First(x=>x.AddressFamily==AddressFamily.InterNetwork);
					}
					
				}
				catch (Exception) {
					/*
					 * Java cannot determine the localhost. This is basically a config
					 * issue on the host. There's not much we can do about it. Just
					 * to suppress NPEs that would result we can create a possibly bogus
					 * address. Pretty sure the below cannot actually thrown a UHE tho.
					 */
					try {
						localInetAddress = IPAddress.Parse("127.0.0.1");
					}
					catch (Exception ignored) {
						throw new RuntimeCIFSException(ignored);
					}
				}
			}

			/*
			 * If a local hostname was not provided a name like
			 * JCIFS34_172_A6 will be dynamically generated for the
			 * client. This is primarily (exclusively?) used as a
			 * CallingName during session establishment.
			 */
			string localHostname = tc.getConfig().getNetbiosHostname();
			if (localHostname == null || localHostname.Length == 0) {
				byte[] addr = localInetAddress.GetAddressBytes();
				localHostname = "JCIFS" + (addr[2] & 0xFF) + "_" + (addr[3] & 0xFF) + "_" + Hexdump.toHexString((int)(RuntimeHelp.nextDouble() * 0xFF), 2);
			}

			/*
			 * Create an NbtAddress for the local interface with
			 * the name deduced above possibly with scope applied and
			 * cache it forever.
			 */
			Name localName = new Name(tc.getConfig(), localHostname, 0x00, tc.getConfig().getNetbiosScope());
			this.localhostAddress = new NbtAddress(localName, localInetAddress.GetHashCode(), false, NbtAddress.B_NODE, false, false, true, false, UNKNOWN_MAC_ADDRESS);
			cacheAddress(localName, this.localhostAddress, SmbConstants.FOREVER);
		}


		/// throws java.net.UnknownHostException
		internal virtual NbtAddress doNameQuery(Name name, IPAddress svr) {
			NbtAddress addr;

			if (name.hexCode == 0x1d && svr == null) {
				svr = this.baddr; // bit of a hack but saves a lookup
			}
			name.srcHashCode = svr != null ? svr.GetHashCode() : 0;
			addr = getCachedAddress(name);

			if (addr == null) {
				/*
				 * This is almost exactly like IPAddress.java. See the
				 * comments there for a description of how the LOOKUP_TABLE prevents
				 * redundant queries from going out on the wire.
				 */
				if ((addr = (NbtAddress) checkLookupTable(name)) == null) {
					try {
						addr = getByName(name, svr);
					}
					catch (UnknownHostException) {
						addr = this.unknownAddress;
					}
					finally {
						cacheAddress(name, addr);
						updateLookupTable(name);
					}
				}
			}
			if (addr == this.unknownAddress) {
				throw new UnknownHostException(name.ToString());
			}
			return addr;
		}


		private object checkLookupTable(Name name) {
			object obj;

			lock (this.inFlightLookups) {
				if (this.inFlightLookups.Contains(name) == false) {
					this.inFlightLookups.Add(name);
					return null;
				}
				while (this.inFlightLookups.Contains(name)) {
					try {
						Monitor.Wait(this.inFlightLookups);
					}
					catch (ThreadInterruptedException e) {
						log.trace("Interrupted", e);
					}
				}
			}
			obj = getCachedAddress(name);
			if (obj == null) {
				lock (this.inFlightLookups) {
					this.inFlightLookups.Add(name);
				}
			}

			return obj;
		}


		private void updateLookupTable(Name name) {
			lock (this.inFlightLookups) {
				this.inFlightLookups.Remove(name);
				Monitor.PulseAll(this.inFlightLookups);
			}
		}


		internal virtual void cacheAddress(Name hostName, NbtAddress addr) {
			if (this.transportContext.getConfig().getNetbiosCachePolicy() == 0) {
				return;
			}
			long expiration = -1;
			if (this.transportContext.getConfig().getNetbiosCachePolicy() != SmbConstants.FOREVER) {
				expiration = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + this.transportContext.getConfig().getNetbiosCachePolicy() * 1000;
			}
			cacheAddress(hostName, addr, expiration);
		}


		internal virtual void cacheAddress(Name hostName, NbtAddress addr, long expiration) {
			if (this.transportContext.getConfig().getNetbiosCachePolicy() == 0) {
				return;
			}
			lock (this.addressCache) {
				CacheEntry entry = this.addressCache.get(hostName);
				if (entry == null) {
					entry = new CacheEntry(hostName, addr, expiration);
					this.addressCache[hostName] = entry;
				}
				else {
					entry.address = addr;
					entry.expiration = expiration;
				}
			}
		}


		internal virtual void cacheAddressArray(NbtAddress[] addrs) {
			if (this.transportContext.getConfig().getNetbiosCachePolicy() == 0) {
				return;
			}
			long expiration = -1;
			if (this.transportContext.getConfig().getNetbiosCachePolicy() != SmbConstants.FOREVER) {
				expiration = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + this.transportContext.getConfig().getNetbiosCachePolicy() * 1000;
			}
			lock (this.addressCache) {
				for (int i = 0; i < addrs.Length; i++) {
					CacheEntry entry = this.addressCache.get(addrs[i].hostName);
					if (entry == null) {
						entry = new CacheEntry(addrs[i].hostName, addrs[i], expiration);
						this.addressCache[addrs[i].hostName] = entry;
					}
					else {
						entry.address = addrs[i];
						entry.expiration = expiration;
					}
				}
			}
		}


		internal virtual NbtAddress getCachedAddress(Name hostName) {
			if (this.transportContext.getConfig().getNetbiosCachePolicy() == 0) {
				return null;
			}
			lock (this.addressCache) {
				CacheEntry entry = this.addressCache.get(hostName);
				if (entry != null && entry.expiration < DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() && entry.expiration >= 0) {
					entry = null;
				}
				return entry != null ? entry.address : null;
			}
		}


		internal virtual int getNextNameTrnId() {
			if ((++this.nextNameTrnId & 0xFFFF) == 0) {
				this.nextNameTrnId = 1;
			}
			return this.nextNameTrnId;
		}


		/// throws java.io.IOException
		internal virtual void ensureOpen(int timeout) {
			this.closeTimeout = 0;
			if (this.transportContext.getConfig().getNetbiosSoTimeout() != 0) {
				this.closeTimeout = Math.Max(this.transportContext.getConfig().getNetbiosSoTimeout(), timeout);
			}
			// If socket is still good, the new closeTimeout will
			// be ignored; see tryClose comment.
			if (this.socket == null) {
				this.socket = SocketEx.ofUdpSocket();
				socket.SetSocketOption(SocketOptionLevel.Socket,SocketOptionName.ReceiveTimeout,this.closeTimeout);
				socket.SetSocketOption(SocketOptionLevel.Socket,SocketOptionName.Broadcast,this.closeTimeout);
				socket.Bind(new IPEndPoint(this.laddr,this.lport));
				this.socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
				//socket.Connect(this.baddr, NAME_SERVICE_UDP_PORT);
				this.thread = new Thread(this.run);
				this.thread.Name = "JCIFS-NameServiceClient";
				this.thread.IsBackground=(true);
				this.thread.Start();
			}
		}


		internal virtual void tryClose() {
			lock (this.LOCK) {

				/*
				 * Yes, there is the potential to drop packets
				 * because we might close the socket during a
				 * request. However the chances are slim and the
				 * retry code should ensure the overall request
				 * is serviced. The alternative complicates things
				 * more than I think is worth it.
				 */

				if (this.socket != null) {
					this.socket.Dispose();
					this.socket = null;
				}
				this.thread = null;
				this.responseTable.Clear();
			}
		}


		public  void run() {
			try {
				while (this.thread == Thread.CurrentThread) {
					//TODO 
					//this.@in.setLength(this.transportContext.getConfig().getNetbiosRcvBufSize());
					//TODO 
					//this.socket.ReceiveTimeout = this.closeTimeout;
					//this.socket.setSoTimeout(this.closeTimeout);
					int len = socket.Receive(rcv_buf, 0, RcvBufSize, System.Net.Sockets.SocketFlags.None);
					
					log.trace("NetBIOS: new data read from socket");

					int nameTrnId = NameServicePacket.readNameTrnId(this.rcv_buf, 0);
					NameServicePacket response = this.responseTable.get(new int?(nameTrnId));
					if (response == null || response.received) {
						continue;
					}
					lock (response) {
						response.readWireFormat(this.rcv_buf, 0);
						response.received = true;

						if (log.isTraceEnabled()) {
							log.trace(response.ToString());
							log.trace(Hexdump.toHexString(this.rcv_buf, 0, len));
						}

						Monitor.Pulse(response);
					}
				}
			}
			catch (Exception ste) when(ste.IsSocketTimeoutException()) {
				log.trace("Socket timeout", ste);
			}
			catch (Exception ex) {
				log.warn("Uncaught exception in NameServiceClient", ex);
			}
			finally {
				tryClose();
			}
		}


		/// throws java.io.IOException
		internal virtual void send(NameServicePacket request, NameServicePacket response, int timeout) {
			int? nid = null;
			int max = this.transportContext.getConfig().getWinsServers().Length;

			if (max == 0) {
				max = 1; // No WINs, try only bcast addr
			}

			lock (response) {
				while (max-- > 0) {
					try {
						lock (this.LOCK) {
							request.nameTrnId = getNextNameTrnId();
							nid = new int?(request.nameTrnId);
						
							response.received = false;

							this.responseTable[nid] = response;
							ensureOpen(timeout + 1000);
							//TODO 1 connect 
							if (socket.Connected)
							{
								socket.Disconnect(true);
							}
							socket.Connect(request.addr, NAME_SERVICE_UDP_PORT);
							int requestLenght = request.writeWireFormat(snd_buf, 0);
							if (log.isTraceEnabled())
							{
								log.trace($"name server client send {Hexdump.toHexString(snd_buf,0,requestLenght)}");
							}
							socket.Send(snd_buf, 0, requestLenght, SocketFlags.None);

							if (log.isTraceEnabled()) {
								log.trace(request.ToString());
								log.trace(Hexdump.toHexString(this.snd_buf, 0, requestLenght));
							}
						}

						long start = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
						while (timeout > 0) {
							Monitor.Wait(response, TimeSpan.FromMilliseconds(timeout));

							/*
							 * JetDirect printer can respond to regular broadcast query
							 * with node status so we need to check to make sure that
							 * the record type matches the question type and if not,
							 * loop around and try again.
							 */
							if (response.received && request.questionType == response.recordType) {
								return;
							}

							response.received = false;
							timeout -= (int)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - start);
						}

					}
					catch (ThreadInterruptedException) {
						throw new InterruptedIOException();
					}
					finally {
						this.responseTable.Remove(nid);
					}

					lock (this.LOCK) {
						if (isWINS(request.addr) == false) {
							break;
						}
						/*
						 * Message was sent to WINS but
						 * failed to receive response.
						 * Try a different WINS server.
						 */
						if (request.addr == getWINSAddress()) {
							switchWINS();
						}
						request.addr = getWINSAddress();
					}
				}
			}
		}


		/// throws java.net.UnknownHostException
		internal virtual NbtAddress[] getAllByName(Name name, IPAddress addr) {
			int n;
			Configuration config = this.transportContext.getConfig();
			NameQueryRequest request = new NameQueryRequest(config, name);
			NameQueryResponse response = new NameQueryResponse(config);

			request.addr = addr != null ? addr : getWINSAddress();
			request.isBroadcast = request.addr == null;

			if (request.isBroadcast) {
				request.addr = this.baddr;
				n = config.getNetbiosRetryCount();
			}
			else {
				request.isBroadcast = false;
				n = 1;
			}

			do {
				try {
					send(request, response, config.getNetbiosRetryTimeout());
				}
				catch (InterruptedIOException ioe) {
					// second query thread to finish gets interrupted so this is expected
					if (log.isTraceEnabled()) {
						log.trace("Failed to send nameservice request for " + name.name, ioe);
					}
					throw new UnknownHostException(name.name);
				}
				catch (IOException ioe) {
					log.info("Failed to send nameservice request for " + name.name, ioe);
					throw new UnknownHostException(name.name);
				}

				if (response.received && response.resultCode == 0) {
					return response.addrEntry;
				}
			} while (--n > 0 && request.isBroadcast);

			throw new UnknownHostException(name.name);
		}


		/// throws java.net.UnknownHostException
		internal virtual NbtAddress getByName(Name name, IPAddress addr) {
			NameQueryRequest request = new NameQueryRequest(this.transportContext.getConfig(), name);
			NameQueryResponse response = new NameQueryResponse(this.transportContext.getConfig());

			if (addr != null) {
			/*
			                       * UniAddress calls always use this
			                       * because it specifies addr
			                       */
				request.addr = addr; // if addr ends with 255 flag it bcast
				request.isBroadcast = (addr.GetAddressBytes()[3] == unchecked((byte) 0xFF));

				int n = this.transportContext.getConfig().getNetbiosRetryCount();
				do {
					try {
						send(request, response, this.transportContext.getConfig().getNetbiosRetryTimeout());
					}
					catch (SocketException ioe) {
						if (log.isTraceEnabled()) {
							log.trace("Timeout waiting for response " + name.name, ioe);
						}
						throw new UnknownHostException(name.name);
					}
					catch (IOException ioe) {
						log.info("Failed to send nameservice request for " + name.name, ioe);
						throw new UnknownHostException(name.name);
					}

					if (response.received && response.resultCode == 0) {
						int last = response.addrEntry.Length - 1;
						response.addrEntry[last].hostName.srcHashCode = addr.GetHashCode();
						return response.addrEntry[last];
					}
				} while (--n > 0 && request.isBroadcast);

				throw new UnknownHostException(name.name);
			}

			/*
			 * If a target address to query was not specified explicitly
			 * with the addr parameter we fall into this resolveOrder routine.
			 */

			foreach (ResolverType resolverType in this.resolveOrder) {
				try {
					switch (resolverType) {
					case ResolverType.RESOLVER_LMHOSTS:
						NbtAddress ans = this.lmhosts.getByName(name, this.transportContext);
						if (ans != null) {
							ans.hostName.srcHashCode = 0; // just has to be different
														  // from other methods
							return ans;
						}
						break;
					case ResolverType.RESOLVER_WINS:
					case ResolverType.RESOLVER_BCAST:
						if (resolverType == ResolverType.RESOLVER_WINS && !object.Equals(name.name, NbtAddress.MASTER_BROWSER_NAME) && name.hexCode != 0x1d) {
							request.addr = getWINSAddress();
							request.isBroadcast = false;
						}
						else {
							request.addr = this.baddr;
							request.isBroadcast = true;
						}

						int n = this.transportContext.getConfig().getNetbiosRetryCount();
						while (n-- > 0) {
							try {
								send(request, response, this.transportContext.getConfig().getNetbiosRetryTimeout());
							}
							catch (IOException ioe) {
								log.info("Failed to send nameservice request for " + name.name, ioe);
								throw new UnknownHostException(name.name);
							}
							if (response.received && response.resultCode == 0) {

								/*
								 * Before we return, in anticipation of this address being cached we must
								 * augment the addresses name's hashCode to distinguish those resolved by
								 * Lmhosts, WINS, or BCAST. Otherwise a failed query from say WINS would
								 * get pulled out of the cache for a BCAST on the same name.
								 */
								response.addrEntry[0].hostName.srcHashCode = request.addr.GetHashCode();
								return response.addrEntry[0];
							}
							else if (resolverType == ResolverType.RESOLVER_WINS) {
								/*
								 * If WINS reports negative, no point in retry
								 */
								break;
							}
						}
						break;
					default:
						break;
					}
				}
				catch (IOException ioe) {
					log.debug("Failed to lookup name", ioe);
				}
			}
			throw new UnknownHostException(name.name);
		}


		/// throws java.net.UnknownHostException
		public virtual NetbiosAddress[] getNodeStatus(NetbiosAddress addr) {
			NodeStatusResponse response = new NodeStatusResponse(this.transportContext.getConfig(), addr.unwrap<NbtAddress>(typeof(NbtAddress)));
			NodeStatusRequest request = new NodeStatusRequest(this.transportContext.getConfig(), new Name(this.transportContext.getConfig(), NbtAddress.ANY_HOSTS_NAME, 0x00, null));
			request.addr = addr.toInetAddress();

			int n = this.transportContext.getConfig().getNetbiosRetryCount();
			while (n-- > 0) {
				try {
					send(request, response, this.transportContext.getConfig().getNetbiosRetryTimeout());
				}
				catch (IOException ioe) {
					log.info("Failed to send node status request for " + addr, ioe);
					throw new UnknownHostException(addr.ToString());
				}
				if (response.received && response.resultCode == 0) {

					/*
					 * For name queries resolved by different sources (e.g. WINS,
					 * BCAST, Node Status) we need to augment the hashcode generated
					 * for the addresses hostname or failed lookups for one type will
					 * be cached and cause other types to fail even though they may
					 * not be the authority for the name. For example, if a WINS lookup
					 * for FOO fails and caches unknownAddress for FOO, a subsequent
					 * lookup for FOO using BCAST should not fail because of that
					 * name cached from WINS.
					 *
					 * So, here we apply the source addresses hashCode to each name to
					 * make them specific to who resolved the name.
					 */

					int srcHashCode = request.addr.GetHashCode();
					for (int i = 0; i < response.addressArray.Length; i++) {
						response.addressArray[i].hostName.srcHashCode = srcHashCode;
					}
					return response.addressArray;
				}
			}
			throw new UnknownHostException(addr.getHostName());
		}


		/// throws java.net.UnknownHostException
		public virtual NetbiosAddress getNbtByName(string host) {
			return getNbtByName(host, 0x00, null);
		}


		/// throws java.net.UnknownHostException
		public virtual NetbiosAddress getNbtByName(string host, int type, string scope) {
			return getNbtByName(host, type, scope, null);
		}


		/// throws java.net.UnknownHostException
		public virtual NetbiosAddress getNbtByName(string host, int type, string scope, IPAddress svr) {

			if (host == null || host.Length == 0) {
				return getLocalHost();
			}

			Name name = new Name(this.transportContext.getConfig(), host, type, scope);
			if (!char.IsDigit(host[0])) {
				return doNameQuery(name, svr);
			}

			int IP = 0x00;
			int hitDots = 0;
			char[] data = host.ToCharArray();

			for (int i = 0; i < data.Length; i++) {
				char c = data[i];
				if (c < (char)48 || c > (char)57) {
					return doNameQuery(name, svr);
				}
				int b = 0x00;
				while (c != '.') {
					if (c < (char)48 || c > (char)57) {
						return doNameQuery(name, svr);
					}
					b = b * 10 + c - '0';

					if (++i >= data.Length) {
						break;
					}

					c = data[i];
				}
				if (b > 0xFF) {
					return doNameQuery(name, svr);
				}
				IP = (IP << 8) + b;
				hitDots++;
			}
			if (hitDots != 4 || host.EndsWith(".", StringComparison.Ordinal)) {
				return doNameQuery(name, svr);
			}
			return new NbtAddress((Name)getUnknownName(), IP, false, NbtAddress.B_NODE);
		}


		/// throws java.net.UnknownHostException
		public virtual NetbiosAddress[] getNbtAllByName(string host, int type, string scope, IPAddress svr) {
			return getAllByName(new Name(this.transportContext.getConfig(), host, type, scope), svr);
		}


		/// throws java.net.UnknownHostException
		public virtual NetbiosAddress[] getNbtAllByAddress(string host) {
			return getNbtAllByAddress(getNbtByName(host, 0x00, null));
		}


		/// throws java.net.UnknownHostException
		public virtual NetbiosAddress[] getNbtAllByAddress(string host, int type, string scope) {
			return getNbtAllByAddress(getNbtByName(host, type, scope));
		}


		/// throws java.net.UnknownHostException
		public virtual NetbiosAddress[] getNbtAllByAddress(NetbiosAddress addr) {
			try {
				NbtAddress[] addrs = (NbtAddress[])getNodeStatus(addr);
				cacheAddressArray(addrs);
				return addrs;
			}
			catch (UnknownHostException) {
				throw new UnknownHostException("no name with type 0x" + Hexdump.toHexString(addr.getNameType(), 2) + (((addr.getName().getScope()==null) || (addr.getName().getScope().Length == 0)) ? " with no scope" : " with scope " + addr.getName().getScope()) + " for host " + addr.getHostAddress());
			}
		}


		/// 
		/// <param name="tc"> </param>
		/// <returns> address of active WINS server </returns>
		protected internal virtual IPAddress getWINSAddress() {
			return this.transportContext.getConfig().getWinsServers().Length == 0 ? null : this.transportContext.getConfig().getWinsServers()[this.nbnsIndex];
		}


		/// 
		/// <param name="svr"> </param>
		/// <returns> whether the given address is a WINS server </returns>
		protected internal virtual bool isWINS(IPAddress svr) {
			for (int i = 0; svr != null && i < this.transportContext.getConfig().getWinsServers().Length; i++) {
				if (svr.GetHashCode() == this.transportContext.getConfig().getWinsServers()[i].GetHashCode()) {
					return true;
				}
			}
			return false;
		}


		protected internal virtual IPAddress switchWINS() {
			this.nbnsIndex = (this.nbnsIndex + 1) < this.transportContext.getConfig().getWinsServers().Length ? this.nbnsIndex + 1 : 0;
			return this.transportContext.getConfig().getWinsServers().Length == 0 ? null : this.transportContext.getConfig().getWinsServers()[this.nbnsIndex];
		}

		internal class Sem {

			internal Sem(int count) {
				this.count = count;
			}

			internal int count;
		}

		internal class QueryThread : Runnable {

			internal Sem sem;
			internal string host, scope;
			internal int type;
			internal NetbiosAddress[] ans = null;
			internal IPAddress svr;
			internal UnknownHostException uhe;
			internal CIFSContext tc;
			private string name;


			internal QueryThread(Sem sem, string host, int type, string scope, IPAddress svr, CIFSContext tc)  {
				this.sem = sem;
				this.host = host;
				this.type = type;
				this.scope = scope;
				this.svr = svr;
				this.tc = tc;
				this.name = "JCIFS-QueryThread: " + host;
			}


			public  void run() {
				try {
					this.ans = this.tc.getNameServiceClient().getNbtAllByName(this.host, this.type, this.scope, this.svr);
				}
				catch (UnknownHostException ex) {
					this.uhe = ex;
				}
				catch (Exception ex) {
					this.uhe = new UnknownHostException(ex.Message);
				}
				finally {
					lock (this.sem) {
						this.sem.count--;
						Monitor.Pulse(this.sem);
					}
				}
			}

			internal Thread _thread;
			public void start()
			{
				_thread = new Thread(this.run);
				_thread.Name = name;
				_thread.IsBackground = true;
			}


			/// <returns> the ans </returns>
			public virtual NetbiosAddress[] getAnswer() {
				return this.ans;
			}


			/// <returns> the uhe </returns>
			public virtual UnknownHostException getException() {
				return this.uhe;
			}

		}


		/// throws java.net.UnknownHostException
		internal virtual NetbiosAddress[] lookupServerOrWorkgroup(string name, IPAddress svr) {
			Sem sem = new Sem(2);
			int type = isWINS(svr) ? 0x1b : 0x1d;

			QueryThread q1x = new QueryThread(sem, name, type, null, svr, this.transportContext);
			QueryThread q20 = new QueryThread(sem, name, 0x20, null, svr, this.transportContext);
			try {
				lock (sem) {
					q1x.start();
					q20.start();

					while (sem.count > 0 && q1x.getAnswer() == null && q20.getAnswer() == null) {
						Monitor.Wait(sem);
					}
				}
			}
			catch (ThreadInterruptedException) {
				throw new UnknownHostException(name);
			}
			waitForQueryThreads(q1x, q20);
			if (q1x.getAnswer() != null) {
				return q1x.getAnswer();
			}
			else if (q20.getAnswer() != null) {
				return q20.getAnswer();
			}
			else {
				throw q1x.getException();
			}
		}


		private static void waitForQueryThreads(QueryThread q1x, QueryThread q20) {
			interruptThreadSafely(q1x);
			joinThread(q1x._thread);
			interruptThreadSafely(q20);
			joinThread(q20._thread);
		}


		private static void interruptThreadSafely(QueryThread thread) {
			try {
				thread._thread.Interrupt();
			}
			catch (ThreadInterruptedException e) {
				if (log.isDebugEnabled()) {
					logger.error(e.Message,e);
				}
			}
		}


		private static void joinThread(Thread thread) {
			try {
				thread.Join();
			}
			catch (ThreadInterruptedException e) {
				if (log.isDebugEnabled()) {
					logger.error(e.Message,e);
				}
			}
		}


		private static bool isAllDigits(string hostname) {
			for (int i = 0; i < hostname.Length; i++) {
				if (char.IsDigit(hostname[i]) == false) {
					return false;
				}
			}
			return true;
		}


		/// throws java.net.UnknownHostException
		public virtual Address getByName(string hostname) {
			return getByName(hostname, false);
		}


		/// throws java.net.UnknownHostException
		public virtual Address getByName(string hostname, bool possibleNTDomainOrWorkgroup) {
			return getAllByName(hostname, possibleNTDomainOrWorkgroup)[0];
		}


		/// throws java.net.UnknownHostException
		public virtual Address[] getAllByName(string hostname, bool possibleNTDomainOrWorkgroup) {
			if (hostname == null || hostname.Length == 0) {
				throw new UnknownHostException();
			}

			if (UniAddress.isDotQuadIP(hostname)) {
				return new UniAddress[] {new UniAddress(getNbtByName(hostname))};
			}

			if (log.isTraceEnabled()) {
				log.trace("Resolver order is " + this.transportContext.getConfig().getResolveOrder());
			}

			foreach (ResolverType resolver in this.transportContext.getConfig().getResolveOrder()) {
				NetbiosAddress[] addr = null;
				try {
					switch (resolver) {
					case ResolverType.RESOLVER_LMHOSTS:
						NbtAddress lmaddr;
						if ((lmaddr = getLmhosts().getByName(hostname, this.transportContext)) == null) {
							continue;
						}
						addr = new NetbiosAddress[] {lmaddr};
						break;
					case ResolverType.RESOLVER_WINS:
						if (hostname.Equals(NbtAddress.MASTER_BROWSER_NAME) || hostname.Length > 15) {
							// invalid netbios name
							continue;
						}
						if (possibleNTDomainOrWorkgroup) {
							addr = lookupServerOrWorkgroup(hostname, getWINSAddress());
						}
						else {
							addr = getNbtAllByName(hostname, 0x20, null, getWINSAddress());
						}
						break;
					case ResolverType.RESOLVER_BCAST:
						if (hostname.Length > 15) {
							// invalid netbios name
							continue;
						}
						if (possibleNTDomainOrWorkgroup) {
							addr = lookupServerOrWorkgroup(hostname, this.transportContext.getConfig().getBroadcastAddress());
						}
						else {
							addr = getNbtAllByName(hostname, 0x20, null, this.transportContext.getConfig().getBroadcastAddress());
						}
						break;
					case ResolverType.RESOLVER_DNS:
						if (isAllDigits(hostname)) {
							throw new UnknownHostException(hostname);
						}

						//TODO
						IPAddress[] ipAddresses;
						try
						{
							ipAddresses = Dns.GetHostAddresses(hostname);
						}
						catch (Exception e)
						{
							throw new UnknownHostException(hostname,e);
						}
					
						UniAddress[] addrs = wrapInetAddresses(ipAddresses);
						if (log.isDebugEnabled()) {
							log.debug($"Resolved '{hostname}' to {addrs?.joinToString()} using DNS");
						}
						return addrs; // Success
					default:
						throw new UnknownHostException(hostname);
					}

					if (addr != null) {
						if (log.isDebugEnabled()) {
							log.debug($"Resolved '{hostname}' to addrs {addr?.joinToString()} via {resolver}");
						}
						return wrapNetbiosAddresses(addr);
					}
				}
				catch (IOException ioe) {
					// Failure
					log.trace($"Resolving {hostname} via {resolver} failed:");
					log.trace("Exception is", ioe);
				}
			}
			throw new UnknownHostException(hostname);
		}


		private static UniAddress[] wrapInetAddresses(IPAddress[] iaddrs) {
			UniAddress[] addrs = new UniAddress[iaddrs.Length];
			for (int ii = 0; ii < iaddrs.Length; ii++) {
				addrs[ii] = new UniAddress(iaddrs[ii]);
			}
			return addrs;
		}


		private static UniAddress[] wrapNetbiosAddresses(NetbiosAddress[] addr) {
			UniAddress[] addrs = new UniAddress[addr.Length];
			for (int i = 0; i < addr.Length; i++) {
				addrs[i] = new UniAddress(addr[i]);
			}
			return addrs;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.NameServiceClient#getLocalHost() </seealso>
		public virtual NetbiosAddress getLocalHost() {
			return this.localhostAddress;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.NameServiceClient#getLocalName() </seealso>
		public virtual NetbiosName getLocalName() {
			if (this.localhostAddress != null) {
				return this.localhostAddress.hostName;
			}
			return null;
		}


		/// 
		/// <returns> lmhosts file used </returns>
		public virtual Lmhosts getLmhosts() {
			return this.lmhosts;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.NameServiceClient#getUnknownName() </seealso>
		public virtual NetbiosName getUnknownName() {
			return this.unknownName;
		}
	}

}