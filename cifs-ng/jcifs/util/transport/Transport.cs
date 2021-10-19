using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using cifs_ng.lib;
using cifs_ng.lib.ext;
using cifs_ng.lib.socket;
using cifs_ng.lib.threading;
using jcifs.lib.io;
using Logger = org.slf4j.Logger;
using LoggerFactory = org.slf4j.LoggerFactory;
using RequestParam = jcifs.smb.RequestParam;

/*
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
namespace jcifs.util.transport {





	/// <summary>
	/// This class simplifies communication for protocols that support
	/// multiplexing requests. It encapsulates a stream and some protocol
	/// knowledge (provided by a concrete subclass) so that connecting,
	/// disconnecting, sending, and receiving can be syncronized
	/// properly. Apparatus is provided to send and receive requests
	/// concurrently.
	/// </summary>

	public abstract class Transport : Runnable, AutoCloseable {

		private static int id = 0;
		private static readonly Logger log = LoggerFactory.getLogger(typeof(Transport));


		/// <summary>
		/// Read bytes from the input stream into a buffer
		/// </summary>
		/// <param name="in"> </param>
		/// <param name="b"> </param>
		/// <param name="off"> </param>
		/// <param name="len"> </param>
		/// <returns> number of bytes read </returns>
		/// <exception cref="IOException"> </exception>
		/// throws java.io.IOException
		public static int readn(SocketInputStream @in, byte[] b, int off, int len) {
			int i = 0, n = -5;

			if (off + len > b.Length) {
				throw new IOException("Buffer too short, buf size " + b.Length + " read " + len);
			}

			while (i < len) {
				n = @in.read(b, off + i, len - i);
				if (n <= 0) {
					break;
				}
				i += n;
			}

			return i;
		}

		/*
		 * state values
		 * 0 - not connected
		 * 1 - connecting
		 * 2 - run connected
		 * 3 - connected
		 * 4 - error
		 * 5 - disconnecting
		 * 6 - disconnected/invalid
		 */
		protected internal volatile int state = 0;

		protected internal string name = "Transport" + id++;
		private volatile Thread thread;
		private volatile TransportException te;

		protected internal readonly object inLock = new object();
		protected internal readonly object outLock = new object();

		protected internal readonly IDictionary<long, Response> responseMap = new ConcurrentDictionary<long, Response>();//TODO
		private readonly AtomicLong usageCount = new AtomicLong(1);


		/// <returns> session increased usage count </returns>
		public virtual Transport acquire() {
			long usage = this.usageCount.IncrementValueAndReturn();
			if (log.isTraceEnabled()) {
				log.trace("Acquire transport " + usage + " " + this);
			}
			return this;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= java.lang.AutoCloseable#Dispose() </seealso>
		public  void Dispose() {
			release();
		}


		/// 
		public virtual void release() {
			long usage = this.usageCount.DecrementValueAndReturn();
			if (log.isTraceEnabled()) {
				log.trace("Release transport " + usage + " " + this);
			}

			if (usage == 0) {
				if (log.isTraceEnabled()) {
					log.trace("Transport usage dropped to zero " + this);
				}
			}
			else if (usage < 0) {
				throw new RuntimeCIFSException("Usage count dropped below zero");
			}
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= java.lang.Object#finalize() </seealso>
		/// throws Throwable
		~Transport() {
			if (!isDisconnected() && this.usageCount.Value != 0) {
				log.warn("Session was not properly released");
			}
		}


		/// <returns> the number of known usages </returns>
		protected internal virtual long getUsageCount() {
			return this.usageCount.Value;
		}


		/// throws java.io.IOException;
		protected internal abstract long makeKey(Request request);


		/// throws java.io.IOException;
		protected internal abstract long? peekKey();


		/// throws java.io.IOException;
		protected internal abstract void doSend(Request request);


		/// throws java.io.IOException;
		protected internal abstract void doRecv(Response response);


		/// throws java.io.IOException;
		protected internal abstract void doSkip(long? key);


		/// 
		/// <returns> whether the transport is disconnected </returns>
		public virtual bool isDisconnected() {
			return this.state == 4 || this.state == 5 || this.state == 6 || this.state == 0;
		}


		/// <returns> whether the transport is marked failed </returns>
		public virtual bool isFailed() {
			return this.state == 5 || this.state == 6;
		}


		/// <summary>
		/// Send a request message and recieve response
		/// </summary>
		/// <param name="request"> </param>
		/// <param name="response"> </param>
		/// <param name="params"> </param>
		/// <returns> the response </returns>
		/// <exception cref="IOException"> </exception>
		/// throws java.io.IOException
		public virtual T sendrecv<T>(Request request, T response, ISet<RequestParam> @params) where T : Response {
			if (isDisconnected() && this.state != 5) {
				throw new TransportException("Transport is disconnected " + this.name);
			}
			try {
				long timeout = !@params.Contains(RequestParam.NO_TIMEOUT) ? getResponseTimeout(request) : 0;

				long firstKey = doSend(request, response, @params, timeout);

				if (Thread.CurrentThread == this.thread) {
					// we are in the transport thread, ie. on idle disconnecting
					// this is synchronous operation
					// This does not handle compound requests
					lock (this.inLock) {
						long? peekKey = this.peekKey();
						if (peekKey == firstKey) {
							doRecv(response);
							response.received();
							return response;
						}
						doSkip(peekKey);
					}
				}

				return waitForResponses(request, response, timeout);
			}
			catch (IOException ioe) {
				log.warn("sendrecv failed", ioe);
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
			//TODO 
			catch (ThreadInterruptedException ie) {
				throw new TransportException(ie);
			}
			finally {
				Response curResp = response;
				Request curReq = request;
				while (curResp != null) {
					this.responseMap.Remove(curResp.getMid());
					Request next = curReq.getNext();
					if (next != null) {
						curReq = next;
						curResp = next.getResponse();
					}
					else {
						break;
					}
				}
			}
		}


		/// <param name="request"> </param>
		/// <param name="response"> </param>
		/// <param name="params"> </param>
		/// <param name="timeout">
		/// @return </param>
		/// <exception cref="IOException"> </exception>
		/// throws java.io.IOException
		protected internal virtual long doSend<T>(Request request, T response, ISet<RequestParam> @params, long timeout) where T : Response {
			long firstKey = prepareRequests(request, response, @params, timeout);
			doSend(request);
			return firstKey;
		}


		/// <param name="request"> </param>
		/// <param name="response"> </param>
		/// <param name="params"> </param>
		/// <param name="timeout"> </param>
		/// <param name="firstKey">
		/// @return </param>
		/// <exception cref="IOException"> </exception>
		/// throws java.io.IOException
		private long prepareRequests<T>(Request request, T response, ISet<RequestParam> @params, long timeout) where T : Response {
			Response curResp = response;
			Request curReq = request;
			long firstKey = 0;
			while (curResp != null) {
				curResp.reset();

				if (@params.Contains(RequestParam.RETAIN_PAYLOAD)) {
					curResp.retainPayload();
				}

				long k = makeKey(curReq);

				if (firstKey == 0) {
					firstKey = k;
				}

				if (timeout > 0) {
					curResp.setExpiration(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + timeout);
				}
				else {
					curResp.setExpiration(null);
				}

				curResp.setMid(k);
				this.responseMap[k] = curResp;

				Request next = curReq.getNext();
				if (next != null) {
					curReq = next;
					curResp = next.getResponse();
				}
				else {
					break;
				}
			}
			return firstKey;
		}


		/// <param name="request"> </param>
		/// <param name="response"> </param>
		/// <param name="timeout"> </param>
		/// <returns> first response </returns>
		/// <exception cref="InterruptedException"> </exception>
		/// <exception cref="TransportException"> </exception>
		/// throws InterruptedException, TransportException
		private T waitForResponses<T>(Request request, T response, long timeout) where T : Response {
			Response curResp = response;
			Request curReq = request;
			while (curResp != null) {
				lock (curResp) {
					if (!curResp.isReceived()) {
						if (timeout > 0) {
							Monitor.Wait(curResp, TimeSpan.FromMilliseconds(timeout));
							if (!curResp.isReceived() && handleIntermediate(curReq, curResp)) {
								continue;
							}

							if (curResp.isError()) {
								throw new TransportException(this.name + " error reading response to " + curReq, curResp.getException());
							}
							if (isDisconnected() && this.state != 5) {
								throw new TransportException($"Transport was disconnected while waiting for a response (transport: {this.name} state: {this.state}),");
							}
							timeout = curResp.getExpiration().GetValueOrDefault(0) - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
							if (timeout <= 0) {
								if (log.isDebugEnabled()) {
									log.debug("State is " + this.state);
								}
								throw new RequestTimeoutException(this.name + " timeout waiting for response to " + curReq);
							}
							continue;
						}

						Monitor.Wait(curResp);
						if (handleIntermediate(request, curResp)) {
							continue;
						}
						if (log.isDebugEnabled()) {
							log.debug("Wait returned state is " + this.state);
						}
						if (isDisconnected()) {
							//TODO 
							throw new ThreadInterruptedException("Transport was disconnected while waiting for a response");
						}
						continue;
					}
				}

				Request next = curReq.getNext();
				if (next != null) {
					curReq = next;
					curResp = next.getResponse();
				}
				else {
					break;
				}
			}
			return response;
		}


		/// <param name="request"> </param>
		/// <param name="response">
		/// @return </param>
		protected  virtual  bool handleIntermediate<T>(Request request, T response) where T : Response {
			return false;
		}


		/// <param name="request">
		/// @return </param>
		protected  abstract int getResponseTimeout(Request request);


		private void loop() {
			while (this.thread == Thread.CurrentThread) {
				try {
					lock (this.inLock) {
						long? key=null;
						try
						{
							key = peekKey();
						}
						catch (Exception e)when (e.IsSocketTimeoutException())
						{
								log.trace("Socket timeout during peekKey", e);
								if (getUsageCount() > 0)
								{
									if (log.isDebugEnabled())
									{
										log.debug("Transport still in use, no idle timeout " + this);
									}

									// notify, so that callers with timed-out requests can handle them
									foreach (Response item in this.responseMap.Values)
									{
										lock (item)
										{
											Monitor.PulseAll(item);
										}
									}

									continue;
								}

								if (log.isDebugEnabled())
								{
									log.debug($"Idle timeout on {this.name}");
								}

								throw;
						}

						if (key == null) {
							lock (this) {
								foreach (Response r in this.responseMap.Values) {
									r.error();
								}
							}
							throw new IOException("end of stream");
						}

						responseMap.TryGetValue(key.Value,out var response);
						if (response == null) {
							if (log.isDebugEnabled()) {
								log.debug("Unexpected message id, skipping message " + key);
							}
							doSkip(key);
						}
						else {
							doRecv(response);
							response.received();
						}
					}
				}
				catch (Exception ex) {
					string msg = ex.Message;
					//TODO
					bool timeout = ex.IsSocketTimeoutException() || msg!= null && msg.Equals("Read timed out");//TODO 1 timeout
					bool closed = msg!=null && msg.Equals("Socket closed");

					if (closed) {
						log.trace("Remote closed connection");
					}
					else if (timeout) {
						log.debug("socket timeout in non peek state", ex);
					}
					else {
						log.debug("recv failed", ex);
					}

					lock (this) {
						try {
							disconnect(!timeout, false);
						}
						//TODO
						catch (Exception ioe) {
							//TODO 
							//ex.addSuppressed(ioe);
							log.warn("Failed to disconnect", ioe);
						}
						log.debug("Disconnected");

						bool notified = false;
						
						/*IEnumerator<KeyValuePair<long, Response>> iterator = this.response_map.SetOfKeyValuePairs().GetEnumerator();
						while (iterator.MoveNext()) {
							Response resp = iterator.Current.Value;
							resp.exception(ex);
							//iterator.remove();
							notified = true;
						}*/

						//TODO 1 response_map
						var responses = this.responseMap.Values;
						this.responseMap.Clear();
						foreach (var resp in responses)
						{
							resp.exception(ex);
							notified = true;
						}
						
						if (notified) {
							log.debug("Notified clients");
						}
						else {
							log.debug("Exception without a request pending", ex);
						}
						return;
					}
				}
			}

		}


		/*
		 * Build a connection. Only one thread will ever call this method at
		 * any one time. If this method throws an exception or the connect timeout
		 * expires an encapsulating TransportException will be thrown from connect
		 * and the transport will be in error.
		 */

		/// throws Exception;
		protected internal abstract void doConnect();


		/*
		 * Tear down a connection. If the hard parameter is true, the diconnection
		 * procedure should not initiate or wait for any outstanding requests on
		 * this transport.
		 */

		/// throws java.io.IOException;
		protected internal abstract bool doDisconnect(bool hard, bool inUse);


		/// <summary>
		/// Connect the transport
		/// </summary>
		/// <param name="timeout"> </param>
		/// <returns> whether the transport was connected </returns>
		/// <exception cref="TransportException"> </exception>
		/// throws TransportException
		public virtual bool connect(long timeout) {
			lock (this) {
				int st = this.state;
				try {
					switch (st) {
					case 0:
						break;
					case 1:
						// already connecting
						Monitor.Wait(this.thread, TimeSpan.FromMilliseconds(timeout)); // wait for doConnect
						st = this.state;
						switch (st) {
						case 1: // doConnect never returned
							this.state = 6;
							cleanupThread(timeout);
							throw new ConnectionTimeoutException("Connection timeout");
						case 2:
							if (this.te != null) {
								this.state = 4; // error
								cleanupThread(timeout);
								throw this.te;
							}
							this.state = 3; // Success!
							return true;
						}
						break;
					case 3:
						return true; // already connected
					case 4:
						this.state = 6;
						throw new TransportException("Connection in error", this.te);
					case 5:
					case 6:
						log.debug("Trying to connect a disconnected transport");
						return false;
					default:
						TransportException tex = new TransportException("Invalid state: " + st);
						throw tex;
					}
        
					if (log.isDebugEnabled()) {
						log.debug("Connecting " + this.name);
					}
        
					this.state = 1;
					this.te = null;
        
					Thread t = new Thread(this.run);
					t.Name = this.name;
					t.IsBackground = true;
					this.thread = t;
        
					lock (this.thread) {
						t.Start();
						Monitor.Wait(t, TimeSpan.FromMilliseconds(timeout)); // wait for doConnect
        
						st = this.state;
						switch (st) {
						case 1: // doConnect never returned
							this.state = 6;
							throw new ConnectionTimeoutException("Connection timeout");
						case 2:
							if (this.te != null) {
								this.state = 4; // error
								throw this.te;
							}
							this.state = 3; // Success!
							return true;
						case 3:
							return true;
						default:
							return false;
						}
					}
				}
				catch (ConnectionTimeoutException e) {
					cleanupThread(timeout);
					// allow to retry the connection
					this.state = 0;
					throw;
				}
				catch (ThreadInterruptedException ie) {
					this.state = 6;
					cleanupThread(timeout);
					throw new TransportException(ie);
				}
				catch (TransportException e) {
					cleanupThread(timeout);
					throw;
				}
				finally {
					/*
					 * This guarantees that we leave in a valid state
					 */
					st = this.state;
					if (st != 0 && st != 3 && st != 4 && st != 5 && st != 6) {
						log.error("Invalid state: " + st);
						this.state = 6;
						cleanupThread(timeout);
					}
				}
			}
		}


		/// <param name="timeout"> </param>
		/// <exception cref="TransportException">
		///  </exception>
		/// throws TransportException
		private void cleanupThread(long timeout) {
			lock (this) {
				Thread t = this.thread;
				if (t != null && Thread.CurrentThread != t) {
					this.thread = null;
					try {
						log.debug("Interrupting transport thread");
						t.Interrupt();
						log.debug("Joining transport thread");
						t.Join(TimeSpan.FromMilliseconds(timeout));
						log.debug("Joined transport thread");
					}
					catch (ThreadInterruptedException e) {
						throw new TransportException("Failed to join transport thread", e);
					}
				}
				else if (t != null) {
					this.thread = null;
				}
			}
		}


		/// <summary>
		/// Disconnect the transport
		/// </summary>
		/// <param name="hard"> </param>
		/// <returns> whether conenction was in use </returns>
		/// <exception cref="IOException"> </exception>
		/// throws java.io.IOException
		public virtual bool disconnect(bool hard) {
			lock (this) {
				return disconnect(hard, true);
			}
		}


		/// <summary>
		/// Disconnect the transport
		/// </summary>
		/// <param name="hard"> </param>
		/// <param name="inUse">
		///            whether the caller is holding a usage reference on the transport </param>
		/// <returns> whether conenction was in use </returns>
		/// <exception cref="IOException"> </exception>
		/// throws java.io.IOException
		public virtual bool disconnect(bool hard, bool inUse) {
			lock (this) {
				IOException ioe = null;
        
				switch (this.state) {
				case 0: // not connected - just return
				case 5:
				case 6:
					return false;
				case 2:
					hard = true;
					goto case 3;
				case 3: // connected - go ahead and disconnect
					if (this.responseMap.Count != 0 && !hard && inUse) {
						break; // outstanding requests
					}
					try {
						this.state = 5;
						bool wasInUse = doDisconnect(hard, inUse);
						this.state = 6;
						return wasInUse;
					}
					catch (IOException ioe0) {
						this.state = 6;
						ioe = ioe0;
					}
					goto case 4;
				case 4: // failed to connect - reset the transport
					// thread is cleaned up by connect routine, joining it here causes a deadlock
					this.thread = null;
					this.state = 6;
					break;
				default:
					log.error("Invalid state: " + this.state);
					this.thread = null;
					this.state = 6;
					break;
				}
        
				if (ioe != null) {
					throw ioe;
				}
        
				return false;
			}
		}


		public  void run() {
			var currentThread = Thread.CurrentThread;
			Exception ex0 = null;

			try {
				/*
				 * We cannot synchronize (run_thread) here or the caller's
				 * thread.wait( timeout ) cannot reaquire the lock and
				 * return which would render the timeout effectively useless.
				 */
				if (this.state != 5 && this.state != 6) {
					doConnect();
				}
			}
			catch (Exception ex) {
				ex0 = ex; // Defer to below where we're locked
				return;
			}
			finally {
				lock (currentThread) {
					//TODO 
					if (currentThread != this.thread) {
						/*
						 * Thread no longer the one setup for this transport --
						 * doConnect returned too late, just ignore.
						 */
						if (ex0.IsSocketTimeoutException()) {
							log.debug("Timeout connecting", ex0);
						}
						else if (ex0 != null) {
							log.warn("Exception in transport thread", ex0); //$NON-NLS-1$
						}
					}
					else
					{
						if (ex0.IsSocketTimeoutException()) {
							this.te = new ConnectionTimeoutException(ex0);
						}
						else if (ex0 != null) {
							this.te = new TransportException(ex0);
						}
						this.state = 2; // run connected
						Monitor.Pulse(currentThread);
					}
				}
			}

			/*
			 * Proccess responses
			 */
			loop();
		}


		public override string ToString() {
			return this.name;
		}

	}

}