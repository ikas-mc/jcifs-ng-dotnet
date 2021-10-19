using System;
using System.IO;
using cifs_ng.lib;
using cifs_ng.lib.threading;
using CIFSContext = jcifs.CIFSContext;
using CIFSException = jcifs.CIFSException;
using NdrBuffer = jcifs.dcerpc.ndr.NdrBuffer;
using NdrException = jcifs.dcerpc.ndr.NdrException;

/* jcifs msrpc client library in Java
 * Copyright (C) 2006  "Michael B. Allen" <jcifs at samba dot org>
 *                   "Eric Glass" <jcifs at samba dot org>
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

namespace jcifs.dcerpc {




	/// 
	/// 
	public abstract class DcerpcHandle :  AutoCloseable {
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields() {
			max_recv = this.max_xmit;
		}


		/*
		 * Bindings are in the form:
		 * proto:\\server[key1=val1,key2=val2]
		 * or
		 * proto:server[key1=val1,key2=val2]
		 * or
		 * proto:[key1=val1,key2=val2]
		 *
		 * If a key is absent it is assumed to be 'endpoint'. Thus the
		 * following are equivalent:
		 * proto:\\ts0.win.net[endpoint=\pipe\srvsvc]
		 * proto:ts0.win.net[\pipe\srvsvc]
		 *
		 * If the server is absent it is set to "127.0.0.1"
		 */
		/// throws DcerpcException
		protected internal static DcerpcBinding parseBinding(string str) {
			int state, mark, si;
			char[] arr = str.ToCharArray();
			string proto = null, key = null;
			DcerpcBinding binding = null;

			state = mark = si = 0;
			do {
				char ch = arr[si];

				switch (state) {
				case 0:
					if (ch == ':') {
						proto = str.Substring(mark, si - mark);
						mark = si + 1;
						state = 1;
					}
					break;
				case 1:
					if (ch == '\\') {
						mark = si + 1;
						break;
					}
					state = 2;
					goto case 2;
				case 2:
					if (ch == '[') {
						string server = str.Substring(mark, si - mark).Trim();
						if (server.Length == 0) {
							// this can also be a v6 address within brackets, look ahead required
							int nexts = str.IndexOf('[', si + 1);
							int nexte = str.IndexOf(']', si);
							if (nexts >= 0 && nexte >= 0 && nexte == nexts - 1) {
								server = str.Substring(si, (nexte + 1) - si);
								si = nexts;
							}
							else {
								server = "127.0.0.1";
							}
						}
						binding = new DcerpcBinding(proto, server);
						mark = si + 1;
						state = 5;
					}
					break;
				case 5:
					if (ch == '=') {
						key = str.Substring(mark, si - mark).Trim();
						mark = si + 1;
					}
					else if (ch == ',' || ch == ']') {
						string val = str.Substring(mark, si - mark).Trim();
						mark = si + 1;
						if ((key== null)) {
							key = "endpoint";
						}
						if (binding != null) {
							binding.setOption(key, val);
						}
						key = null;
					}
					break;
				default:
					si = arr.Length;
				break;
				}

				si++;
			} while (si < arr.Length);

			if (binding == null || (binding.getEndpoint()== null)) {
				throw new DcerpcException("Invalid binding URL: " + str);
			}

			return binding;
		}

		private static readonly AtomicInteger call_id = new AtomicInteger(1);

		private readonly DcerpcBinding binding;
		private int max_xmit = 4280;
		private int max_recv;
		private int state = 0;
		private DcerpcSecurityProvider securityProvider = null;
		private CIFSContext transportContext;


		/// <param name="tc">
		///  </param>
		public DcerpcHandle(CIFSContext tc) {
			if (!InstanceFieldsInitialized) {
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
			this.transportContext = tc;
			this.binding = null;
		}


		/// <param name="tc"> </param>
		/// <param name="binding"> </param>
		public DcerpcHandle(CIFSContext tc, DcerpcBinding binding) {
			if (!InstanceFieldsInitialized) {
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
			this.transportContext = tc;
			this.binding = binding;
		}


		/// <returns> the binding </returns>
		public virtual DcerpcBinding getBinding() {
			return this.binding;
		}


		/// <returns> the max_recv </returns>
		internal virtual int getMaxRecv() {
			return this.max_recv;
		}


		/// <returns> the max_xmit </returns>
		internal virtual int getMaxXmit() {
			return this.max_xmit;
		}


		/// <summary>
		/// Get a handle to a service
		/// </summary>
		/// <param name="url"> </param>
		/// <param name="tc">
		///            context to use </param>
		/// <returns> a DCERPC handle for the given url </returns>
		/// <exception cref="MalformedURLException"> </exception>
		/// <exception cref="DcerpcException"> </exception>
		/// throws MalformedURLException, DcerpcException
		public static DcerpcHandle getHandle(string url, CIFSContext tc) {
			return getHandle(url, tc, false);
		}


		/// <summary>
		/// Get a handle to a service
		/// </summary>
		/// <param name="url"> </param>
		/// <param name="tc"> </param>
		/// <param name="unshared">
		///            whether an exclusive connection should be used </param>
		/// <returns> a DCERPC handle for the given url </returns>
		/// <exception cref="MalformedURLException"> </exception>
		/// <exception cref="DcerpcException"> </exception>
		/// throws MalformedURLException, DcerpcException
		public static DcerpcHandle getHandle(string url, CIFSContext tc, bool unshared) {
			if (url.StartsWith("ncacn_np:", StringComparison.Ordinal)) {
				return new DcerpcPipeHandle(url, tc, unshared);
			}
			throw new DcerpcException("DCERPC transport not supported: " + url);
		}


		/// <summary>
		/// Bind the handle
		/// </summary>
		/// <exception cref="DcerpcException"> </exception>
		/// <exception cref="IOException"> </exception>
		/// throws DcerpcException, java.io.IOException
		public virtual void bind() {
			lock (this) {
				try {
					this.state = 1;
					DcerpcMessage bind = new DcerpcBind(this.binding, this);
					sendrecv(bind);
				}
				catch (IOException ioe) {
					this.state = 0;
					throw ioe;
				}
			}
		}


		/// 
		/// <param name="msg"> </param>
		/// <exception cref="DcerpcException"> </exception>
		/// <exception cref="IOException"> </exception>
		/// throws DcerpcException, java.io.IOException
		public virtual void sendrecv(DcerpcMessage msg) {
			if (this.state == 0) {
				bind();
			}
			byte[] inB = this.transportContext.getBufferCache().getBuffer();
			byte[] @out = this.transportContext.getBufferCache().getBuffer();
			try {
				NdrBuffer buf = encodeMessage(msg, @out);
				int off = sendFragments(msg, @out, buf);

				// last fragment gets written (possibly) using transact/call semantics
				int have = doSendReceiveFragment(@out, off, msg.length, inB);

				if (have != 0) {
					NdrBuffer hdrBuf = new NdrBuffer(inB, 0);
					setupReceivedFragment(hdrBuf);
					hdrBuf.setIndex(0);
					msg.decode_header(hdrBuf);
				}

				NdrBuffer msgBuf;
				if (have != 0 && !msg.isFlagSet(DcerpcConstants.DCERPC_LAST_FRAG)) {
					msgBuf = new NdrBuffer(receiveMoreFragments(msg, inB), 0);
				}
				else {
					msgBuf = new NdrBuffer(inB, 0);
				}
				msg.decode(msgBuf);
			}
			finally {
				this.transportContext.getBufferCache().releaseBuffer(inB);
				this.transportContext.getBufferCache().releaseBuffer(@out);
			}

			DcerpcException de;
			if ((de = msg.getResult()) != null) {
				throw de;
			}
		}


		/// <param name="msg"> </param>
		/// <param name="out"> </param>
		/// <param name="buf"> </param>
		/// <param name="off"> </param>
		/// <param name="tot">
		/// @return </param>
		/// <exception cref="IOException"> </exception>
		/// throws java.io.IOException
		private int sendFragments(DcerpcMessage msg, byte[] @out, NdrBuffer buf) {
			int off = 0;
			int tot = buf.getLength() - 24;
			while (off < tot) {
				int fragSize = tot - off;
				if ((24 + fragSize) > this.max_xmit) {
					// need fragementation
					msg.flags &= ~DcerpcConstants.DCERPC_LAST_FRAG;
					fragSize = this.max_xmit - 24;
				}
				else {
					msg.flags |= DcerpcConstants.DCERPC_LAST_FRAG;
					msg.alloc_hint = fragSize;
				}

				msg.length = 24 + fragSize;

				if (off > 0) {
					msg.flags &= ~DcerpcConstants.DCERPC_FIRST_FRAG;
				}

				if ((msg.flags & (DcerpcConstants.DCERPC_FIRST_FRAG | DcerpcConstants.DCERPC_LAST_FRAG)) != (DcerpcConstants.DCERPC_FIRST_FRAG | DcerpcConstants.DCERPC_LAST_FRAG)) {
					buf.start = off;
					buf.reset();
					msg.encode_header(buf);
					buf.enc_ndr_long(msg.alloc_hint);
					buf.enc_ndr_short(0); // context id
					buf.enc_ndr_short(msg.getOpnum());
				}

				if ((msg.flags & DcerpcConstants.DCERPC_LAST_FRAG) != DcerpcConstants.DCERPC_LAST_FRAG) {
					// all fragment but the last get written using read/write semantics
					doSendFragment(@out, off, msg.length);
					off += fragSize;
				}
				else {
					return off;
				}
			}
			throw new IOException();
		}


		/// <param name="msg"> </param>
		/// <param name="in"> </param>
		/// <param name="off"> </param>
		/// <param name="isDirect">
		/// @return </param>
		/// <exception cref="IOException"> </exception>
		/// <exception cref="DcerpcException"> </exception>
		/// <exception cref="NdrException"> </exception>
		/// throws IOException, DcerpcException, jcifs.dcerpc.ndr.NdrException
		private byte[] receiveMoreFragments(DcerpcMessage msg, byte[] @in) {
			int off = msg.ptype == 2 ? msg.length : 24;
			byte[] fragBytes = new byte[this.max_recv];
			NdrBuffer fragBuf = new NdrBuffer(fragBytes, 0);
			while (!msg.isFlagSet(DcerpcConstants.DCERPC_LAST_FRAG)) {
				doReceiveFragment(fragBytes);
				setupReceivedFragment(fragBuf);
				fragBuf.reset();
				msg.decode_header(fragBuf);
				int stub_frag_len = msg.length - 24;
				if ((off + stub_frag_len) > @in.Length) {
					// shouldn't happen if alloc_hint is correct or greater
					byte[] tmp = new byte[off + stub_frag_len];
					Array.Copy(@in, 0, tmp, 0, off);
					@in = tmp;
				}
				Array.Copy(fragBytes, 24, @in, off, stub_frag_len);
				off += stub_frag_len;
			}
			return @in;
		}


		/// <param name="fbuf"> </param>
		/// <exception cref="DcerpcException"> </exception>
		/// throws DcerpcException
		private void setupReceivedFragment(NdrBuffer fbuf) {
			fbuf.reset();
			fbuf.setIndex(8);
			fbuf.setLength(fbuf.dec_ndr_short());

			if (this.securityProvider != null) {
				this.securityProvider.unwrap(fbuf);
			}
		}


		/// <param name="msg"> </param>
		/// <param name="out">
		/// @return </param>
		/// <exception cref="NdrException"> </exception>
		/// <exception cref="DcerpcException"> </exception>
		/// throws NdrException, DcerpcException
		private NdrBuffer encodeMessage(DcerpcMessage msg, byte[] @out) {
			NdrBuffer buf = new NdrBuffer(@out, 0);

			msg.flags = DcerpcConstants.DCERPC_FIRST_FRAG | DcerpcConstants.DCERPC_LAST_FRAG;
			msg.call_id = call_id.IncrementValueAndReturn();

			msg.encode(buf);

			if (this.securityProvider != null) {
				buf.setIndex(0);
				this.securityProvider.wrap(buf);
			}
			return buf;
		}


		/// 
		/// <param name="securityProvider"> </param>
		public virtual void setDcerpcSecurityProvider(DcerpcSecurityProvider securityProvider) {
			this.securityProvider = securityProvider;
		}


		/// 
		/// <returns> the server connected to </returns>
		public abstract string getServer();


		/// <returns> the server resolved by DFS </returns>
		public abstract string getServerWithDfs();


		/// <returns> the transport context used </returns>
		public abstract CIFSContext getTransportContext();


		/// 
		/// <returns> session key of the underlying smb session </returns>
		/// <exception cref="CIFSException"> </exception>
		/// throws jcifs.CIFSException;
		public abstract byte[] getSessionKey();


		public override string ToString() {
			return this.binding.ToString();
		}


		/// throws java.io.IOException;
		protected internal abstract void doSendFragment(byte[] buf, int off, int length);


		/// throws java.io.IOException;
		protected internal abstract int doReceiveFragment(byte[] buf);


		/// throws java.io.IOException;
		protected internal abstract int doSendReceiveFragment(byte[] @out, int off, int length, byte[] inB);


		/// throws java.io.IOException
		public virtual void Dispose() {
			this.state = 0;
		}

	}

}