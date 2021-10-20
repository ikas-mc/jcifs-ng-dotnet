using System;
using System.IO;
using System.Threading;
using cifs_ng.lib;
using cifs_ng.lib.io;
using cifs_ng.lib.socket;
using Logger = org.slf4j.Logger;
using LoggerFactory = org.slf4j.LoggerFactory;
using CIFSContext = jcifs.CIFSContext;
using CIFSException = jcifs.CIFSException;
using SmbConstants = jcifs.SmbConstants;
using SmbFileHandle = jcifs.SmbFileHandle;
using SmbComReadAndX = jcifs.@internal.smb1.com.SmbComReadAndX;
using SmbComReadAndXResponse = jcifs.@internal.smb1.com.SmbComReadAndXResponse;
using Smb2ReadRequest = jcifs.@internal.smb2.io.Smb2ReadRequest;
using Smb2ReadResponse = jcifs.@internal.smb2.io.Smb2ReadResponse;
using TransportException = jcifs.util.transport.TransportException;

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

	/// <summary>
	/// This InputStream can read bytes from a file on an SMB file server. Offsets are 64 bits.
	/// </summary>
	public class SmbFileInputStream : InputStream  {

		private static readonly Logger log = LoggerFactory.getLogger(typeof(SmbFileInputStream));

		private SmbFileHandleImpl handle;
		private long fp;
		private int readSize, readSizeFile, openFlags, access, sharing;
		private byte[] tmp = new byte[1];

		internal SmbFile file;

		private bool largeReadX;

		private readonly bool unsharedFile;

		private bool smb2;


		/// <param name="url"> </param>
		/// <param name="tc">
		///            context to use </param>
		/// <exception cref="SmbException"> </exception>
		/// <exception cref="MalformedURLException"> </exception>
		/// throws SmbException, java.net.MalformedURLException
		public SmbFileInputStream(string url, CIFSContext tc) : this(new SmbFile(url, tc), 0, SmbConstants.O_RDONLY, SmbConstants.DEFAULT_SHARING, true) {
		}


		/// <summary>
		/// Creates an <seealso cref="System.IO.Stream_Input"/> for reading bytes from a file on
		/// an SMB server represented by the <seealso cref="jcifs.smb.SmbFile"/> parameter. See
		/// <seealso cref="jcifs.smb.SmbFile"/> for a detailed description and examples of
		/// the smb URL syntax.
		/// </summary>
		/// <param name="file">
		///            An <code>SmbFile</code> specifying the file to read from </param>
		/// <exception cref="SmbException"> </exception>
		/// throws SmbException
		public SmbFileInputStream(SmbFile file) : this(file, 0, SmbConstants.O_RDONLY, SmbConstants.DEFAULT_SHARING, false) {
		}


		/// throws SmbException
		internal SmbFileInputStream(SmbFile file, int openFlags, int access, int sharing, bool unshared) {
			this.file = file;
			this.unsharedFile = unshared;
			this.openFlags = openFlags;
			this.access = access;
			this.sharing = sharing;

			try {
					using (SmbTreeHandleInternal th = file.ensureTreeConnected()) {
					this.smb2 = th.isSMB2();
					if (file.getType() != SmbConstants.TYPE_NAMED_PIPE) {
						using (SmbFileHandle h = ensureOpen()) {
						}
						this.openFlags &= ~(SmbConstants.O_CREAT | SmbConstants.O_TRUNC);
					}
        
					init(th);
					}
			}
			catch (CIFSException e) {
				throw SmbException.wrap(e);
			}
		}


		/// <exception cref="SmbException">
		///  </exception>
		/// throws SmbException
		internal SmbFileInputStream(SmbFile file, SmbTreeHandleImpl th, SmbFileHandleImpl fh) {
			this.file = file;
			this.handle = fh;
			this.unsharedFile = false;
			this.smb2 = th.isSMB2();
			try {
				init(th);
			}
			catch (CIFSException e) {
				throw SmbException.wrap(e);
			}
		}


		/// <param name="f"> </param>
		/// <param name="th"> </param>
		/// <exception cref="SmbException"> </exception>
		/// throws jcifs.CIFSException
		private void init(SmbTreeHandleInternal th) {
			if (this.smb2) {
				this.readSize = th.getReceiveBufferSize();
				this.readSizeFile = th.getReceiveBufferSize();
				return;
			}

			this.readSize = Math.Min(th.getReceiveBufferSize() - 70, th.getMaximumBufferSize() - 70);

			if (th.hasCapability(SmbConstants.CAP_LARGE_READX)) {
				this.largeReadX = true;
				//TODO max size 0xFFFF
				//TODO 
				this.readSizeFile = Math.Min(th.getConfig().getReceiveBufferSize() - 70,  0xFFFF - 70 );
				//this.readSizeFile = Math.Min(th.getConfig().getReceiveBufferSize() - 70, th.areSignaturesActive() ? 0xFFFF - 70 : 0xFFFFFF - 70);
				log.debug("Enabling LARGE_READX with " + this.readSizeFile);
			}
			else {
				log.debug("LARGE_READX disabled");
				this.readSizeFile = this.readSize;
			}

			if (log.isDebugEnabled()) {
				log.debug("Negotiated file read size is " + this.readSizeFile);
			}
		}


		/// <summary>
		/// Ensures that the file descriptor is openend
		/// </summary>
		/// <exception cref="CIFSException"> </exception>
		/// throws jcifs.CIFSException
		public virtual void open() {
			using (SmbFileHandleImpl fh = ensureOpen()) {
			}
		}


		/// <param name="file"> </param>
		/// <param name="openFlags">
		/// @return </param>
		/// <exception cref="SmbException"> </exception>
		/// throws jcifs.CIFSException
		internal virtual SmbFileHandleImpl ensureOpen() {
			lock (this) {
				if (this.handle == null || !this.handle.isValid()) {
					// one extra acquire to keep this open till the stream is released
					if (this.file is SmbNamedPipe) {
						this.handle = this.file.openUnshared(SmbConstants.O_EXCL, ((SmbNamedPipe) this.file).getPipeType() & 0xFF0000, this.sharing, SmbConstants.ATTR_NORMAL, 0);
					}
					else {
						this.handle = this.file.openUnshared(this.openFlags, this.access, this.sharing, SmbConstants.ATTR_NORMAL, 0).acquire();
					}
					return this.handle;
				}
				return this.handle.acquire();
			}
		}


		protected internal static IOException seToIoe(SmbException se) {
			IOException ioe = se;
			Exception root = se.InnerException;
			if (root is TransportException) {
				ioe = (TransportException) root;
				root = ((TransportException) ioe).InnerException;
			}
			if (root is ThreadInterruptedException) {
				ioe = new InterruptedIOException(root.Message,root);
			}
			return ioe;
		}


		/// <summary>
		/// Closes this input stream and releases any system resources associated with the stream.
		/// </summary>
		/// <exception cref="IOException">
		///             if a network error occurs </exception>

		/// throws java.io.IOException
		
		public  void Dispose(){
			try {
				SmbFileHandleImpl h = this.handle;
				if (h != null) {
					h.Dispose();
				}
			}
			catch (SmbException se) {
				throw seToIoe(se);
			}
			finally {
				this.tmp = null;
				this.handle = null;
				if (this.unsharedFile) {
					this.file.Dispose();
				}
			}
		}

		/// <summary>
		/// Reads a byte of data from this input stream.
		/// </summary>
		/// <exception cref="IOException">
		///             if a network error occurs </exception>

		/// throws java.io.IOException
		public  int read() {
			// need oplocks to cache otherwise use BufferedInputStream
			if (read(this.tmp, 0, 1) == -1) {
				return -1;
			}
			return this.tmp[0] & 0xFF;
		}
		
		public  long seek(long offset) {
			this.fp = offset;
			return this.fp;
		}
		
		public  long length() {
			return this.file.length();
		}
		

		public  void write(byte[] buffer, int offset, int count)
		{
			throw new NotImplementedException();
		}


		public long position() {
			return this.fp;
		}

		/// <summary>
		/// Reads up to b.length bytes of data from this input stream into an array of bytes.
		/// </summary>
		/// <exception cref="IOException">
		///             if a network error occurs </exception>

		/// throws java.io.IOException
		public  int read(byte[] b) {
			return read(b, 0, b.Length);
		}


		/// <summary>
		/// Reads up to len bytes of data from this input stream into an array of bytes.
		/// </summary>
		/// <exception cref="IOException">
		///             if a network error occurs </exception>

		/// throws java.io.IOException
		public  int read(byte[] b, int off, int len) {
			return readDirect(b, off, len);
		}


		/// <summary>
		/// Reads up to len bytes of data from this input stream into an array of bytes.
		/// </summary>
		/// <param name="b"> </param>
		/// <param name="off"> </param>
		/// <param name="len"> </param>
		/// <returns> number of bytes read
		/// </returns>
		/// <exception cref="IOException">
		///             if a network error occurs </exception>
		/// throws java.io.IOException
		public virtual int readDirect(byte[] b, int off, int len) {
			if (len <= 0) {
				return 0;
			}
			long start = this.fp;

			if (this.tmp == null) {
				throw new IOException("Bad file descriptor");
			}
			// ensure file is open
			using (SmbFileHandleImpl fd = ensureOpen())
			using (	SmbTreeHandleImpl th =(SmbTreeHandleImpl) fd.getTree()) {

				/*
				 * Read AndX Request / Response
				 */

				if (log.isTraceEnabled()) {
					log.trace("read: fid=" + fd + ",off=" + off + ",len=" + len);
				}

				SmbComReadAndXResponse response = new SmbComReadAndXResponse(th.getConfig(), b, off);

				int type = this.file.getType();
				int r, n;
				int blockSize = (type == SmbConstants.TYPE_FILESYSTEM) ? this.readSizeFile : this.readSize;
				do {
					r = len > blockSize ? blockSize : len;

					if (log.isTraceEnabled()) {
						log.trace("read: len=" + len + ",r=" + r + ",fp=" + this.fp + ",b.length=" + b.Length);
					}

					try {

						if (th.isSMB2()) {
							Smb2ReadRequest request1 = new Smb2ReadRequest(th.getConfig(), fd.getFileId(), b, off);
							request1.setOffset(type == SmbConstants.TYPE_NAMED_PIPE ? 0 : this.fp);
							request1.setReadLength(r);
							request1.setRemainingBytes(len - r);

							try {
								Smb2ReadResponse resp = th.send(request1, RequestParam.NO_RETRY);
								n = resp.getDataLength();
							}
							catch (SmbException e) {
								// unchecked((int)0xC0000011)
								if (e.getNtStatus() == NtStatus.NT_STATUS_END_OF_FILE) {
									log.debug("Reached end of file", e);
									n = -1;
								}
								else {
									throw e;
								}
							}
							if (n <= 0) {
								return (int)((this.fp - start) > 0L ? this.fp - start : -1);
							}
							this.fp += n;
							off += n;
							len -= n;
							continue;
						}

						SmbComReadAndX request = new SmbComReadAndX(th.getConfig(), fd.getFid(), this.fp, r, null);
						if (type == SmbConstants.TYPE_NAMED_PIPE) {
							request.setMinCount(1024);
							request.setMaxCount(1024);
							request.setRemaining(1024);
						}
						else if (this.largeReadX) {
							request.setMaxCount(r & 0xFFFF);
							request.setMinCount(r & 0xFFFF);
							request.setOpenTimeout((r >> 16) & 0xFFFF);
						}
						th.send(request, response, RequestParam.NO_RETRY);
						n = response.getDataLength();
					}
					catch (SmbException se) {
						if (type == SmbConstants.TYPE_NAMED_PIPE && se.getNtStatus() == NtStatus.NT_STATUS_PIPE_BROKEN) {
							return -1;
						}
						throw seToIoe(se);
					}
					if (n <= 0) {
						return (int)((this.fp - start) > 0L ? this.fp - start : -1);
					}
					this.fp += n;
					len -= n;
					response.adjustOffset(n);
				} while (len > blockSize && n == r);
				// this used to be len > 0, but this is BS:
				// - InputStream.read gives no such guarantee
				// - otherwise the caller would need to figure out the block size, or otherwise might end up with very small
				// reads
				return (int)(this.fp - start);
			}
		}


		/// <summary>
		/// This stream class is unbuffered. Therefore this method will always
		/// return 0 for streams connected to regular files. However, a
		/// stream created from a Named Pipe this method will query the server using a
		/// "peek named pipe" operation and return the number of available bytes
		/// on the server.
		/// </summary>
		/// throws java.io.IOException
		public virtual  int available() {
			//TODO 
			return 0;
		}


		/// <summary>
		/// Skip n bytes of data on this stream. This operation will not result
		/// in any IO with the server. Unlink <tt>InputStream</tt> value less than
		/// the one provided will not be returned if it exceeds the end of the file
		/// (if this is a problem let us know).
		/// </summary>
		/// throws java.io.IOException
		public virtual long skip(long n) {
			if (n > 0) {
				this.fp += n;
				return n;
			}
			return 0;
		}

	}

}