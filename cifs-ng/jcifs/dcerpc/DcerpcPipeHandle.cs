using System.IO;
using CIFSContext = jcifs.CIFSContext;
using CIFSException = jcifs.CIFSException;
using SmbPipeResource = jcifs.SmbPipeResource;
using SmbNamedPipe = jcifs.smb.SmbNamedPipe;
using SmbPipeHandleInternal = jcifs.smb.SmbPipeHandleInternal;
using Encdec = jcifs.util.Encdec;

/* jcifs msrpc client library in Java
 * Copyright (C) 2006  "Michael B. Allen" <jcifs at samba dot org>
 *                     "Eric Glass" <jcifs at samba dot org>
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
	public class DcerpcPipeHandle : DcerpcHandle {

		/* This 0x20000 bit is going to get chopped! */
		internal static readonly int pipeFlags = (0x2019F << 16) | SmbPipeResourceConstants.PIPE_TYPE_RDWR | SmbPipeResourceConstants.PIPE_TYPE_DCE_TRANSACT;

		private SmbNamedPipe pipe;
		private SmbPipeHandleInternal handle;


		/// <param name="url"> </param>
		/// <param name="tc"> </param>
		/// <param name="unshared"> </param>
		/// <exception cref="DcerpcException"> </exception>
		/// <exception cref="MalformedURLException"> </exception>
		/// throws DcerpcException, java.net.MalformedURLException
		public DcerpcPipeHandle(string url, CIFSContext tc, bool unshared) : base(tc, DcerpcHandle.parseBinding(url)) {
			this.pipe = new SmbNamedPipe(makePipeUrl(), pipeFlags, unshared, tc);
			this.handle = this.pipe.openPipe().unwrap<SmbPipeHandleInternal>(typeof(SmbPipeHandleInternal));
		}


		private string makePipeUrl() {
			DcerpcBinding binding = getBinding();
			string url = "smb://" + binding.getServer() + "/IPC$/" + binding.getEndpoint().Substring(6);

			string @params = "";
			string server = (string) binding.getOption("server");
			if ((server!=null)) {
				@params += "&server=" + server;
			}
			string address = (string) binding.getOption("address");
			if ((address!= null)) {
				@params += "&address=" + address;
			}
			if (@params.Length > 0) {
				url += "?" + @params.Substring(1);
			}

			return url;
		}


		public override CIFSContext getTransportContext() {
			return this.pipe.getContext();
		}


		public override string getServer() {
			return this.pipe.getLocator().getServer();
		}


		public override string getServerWithDfs() {
			return this.pipe.getLocator().getServerWithDfs();
		}


		/// throws jcifs.CIFSException
		public override byte[] getSessionKey() {
			return this.handle.getSessionKey();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.dcerpc.DcerpcHandle#doSendReceiveFragment(byte[], int, int, byte[]) </seealso>
		/// throws java.io.IOException
		protected internal override int doSendReceiveFragment(byte[] buf, int off, int length, byte[] inB) {
			if (this.handle.isStale()) {
				throw new IOException("DCERPC pipe is no longer open");
			}

			int have = this.handle.sendrecv(buf, off, length, inB, getMaxRecv());

			int fraglen = Encdec.dec_uint16le(inB, 8);
			if (fraglen > getMaxRecv()) {
				throw new IOException("Unexpected fragment length: " + fraglen);
			}

			while (have < fraglen) {
				int r = this.handle.recv(inB, have, fraglen - have);
				if (r == 0) {
					throw new IOException("Unexpected EOF");
				}
				have += r;
			}

			return have;
		}


		/// throws java.io.IOException
		protected internal override void doSendFragment(byte[] buf, int off, int length) {
			if (this.handle.isStale()) {
				throw new IOException("DCERPC pipe is no longer open");
			}
			this.handle.send(buf, off, length);
		}


		/// throws java.io.IOException
		protected internal override int doReceiveFragment(byte[] buf) {
			if (buf.Length < getMaxRecv()) {
				throw new System.ArgumentException("buffer too small");
			}

			int off = this.handle.recv(buf, 0, buf.Length);
			if (buf[0] != 5 || buf[1] != 0) {
				throw new IOException("Unexpected DCERPC PDU header");
			}

			int length = Encdec.dec_uint16le(buf, 8);
			if (length > getMaxRecv()) {
				throw new IOException("Unexpected fragment length: " + length);
			}

			while (off < length) {
				int r = this.handle.recv(buf, off, length - off);
				if (r == 0) {
					throw new IOException("Unexpected EOF");
				}
				off += r;
			}
			return off;
		}


		/// throws java.io.IOException
		public override void Dispose() {
			base.Dispose();
			try {
				this.handle.Dispose();
			}
			finally {
				this.pipe.Dispose();
			}
		}
	}

}