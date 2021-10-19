using System;
using System.IO;
using cifs_ng.lib.io;
using jcifs.@internal.fscc;
using jcifs.@internal.smb1.com;
using jcifs.@internal.smb2.info;
using jcifs.@internal.smb2.io;
using org.slf4j;

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
	/// This <code>OutputStream</code> can write bytes to a file on an SMB file server.
	/// </summary>
	public class SmbFileOutputStream : OutputStream {

		private static readonly Logger log = LoggerFactory.getLogger(typeof(SmbFileOutputStream));

		private SmbFile file;
		private bool append, useNTSmbs;
		private int openFlags, access, writeSize, writeSizeFile;
		private long fp;
		private byte[] tmp = new byte[1];
		private SmbComWriteAndX reqx;
		private SmbComWriteAndXResponse rspx;
		private SmbComWrite req;
		private SmbComWriteResponse rsp;

		private SmbFileHandleImpl handle;

		private int sharing;

		private readonly bool smb2;


		/// <summary>
		/// Creates an <seealso cref="System.IO.Stream_Output"/> for writing bytes to a file on
		/// an SMB server represented by the <seealso cref="jcifs.smb.SmbFile"/> parameter. See
		/// <seealso cref="jcifs.smb.SmbFile"/> for a detailed description and examples of
		/// the smb URL syntax.
		/// </summary>
		/// <param name="file">
		///            An <code>SmbFile</code> specifying the file to write to </param>
		/// <exception cref="SmbException"> </exception>
		/// throws SmbException
		public SmbFileOutputStream(SmbFile file) : this(file, false) {
		}


		/// <summary>
		/// Creates an <seealso cref="System.IO.Stream_Output"/> for writing bytes to a file
		/// on an SMB server addressed by the <code>SmbFile</code> parameter. See
		/// <seealso cref="jcifs.smb.SmbFile"/> for a detailed description and examples of
		/// the smb URL syntax. If the second argument is <code>true</code>, then
		/// bytes will be written to the end of the file rather than the beginning.
		/// </summary>
		/// <param name="file">
		///            An <code>SmbFile</code> representing the file to write to </param>
		/// <param name="append">
		///            Append to the end of file </param>
		/// <exception cref="SmbException"> </exception>
		/// throws SmbException
		public SmbFileOutputStream(SmbFile file, bool append) : this(file, append, append ? SmbConstants.O_CREAT | SmbConstants.O_WRONLY | SmbConstants.O_APPEND : SmbConstants.O_CREAT | SmbConstants.O_WRONLY | SmbConstants.O_TRUNC, 0, SmbConstants.DEFAULT_SHARING) {
		}


		/// throws SmbException
		internal SmbFileOutputStream(SmbFile file, bool append, int openFlags, int access, int sharing) {
			this.file = file;
			this.append = append;
			this.openFlags = openFlags;
			this.sharing = sharing;
			this.access = access | SmbConstants.FILE_WRITE_DATA;

			try {
				using (SmbTreeHandleImpl th = file.ensureTreeConnected()) {
					this.smb2 = th.isSMB2();
					using (SmbFileHandleImpl fh = ensureOpen()) {
						if (append) {
							this.fp = fh.getInitialSize();
						}
						init(th);
						if (!append && this.smb2) {
							// no open option for truncating, need to truncate the file
							Smb2SetInfoRequest treq = new Smb2SetInfoRequest(th.getConfig(), fh.getFileId());
							treq.setFileInformation(new FileEndOfFileInformation(0));
							th.send(treq, RequestParam.NO_RETRY);
						}
					}
				}
			}
			catch (CIFSException e) {
				throw SmbException.wrap(e);
			}
		}


		/// throws jcifs.CIFSException
		internal SmbFileOutputStream(SmbFile file, SmbTreeHandleImpl th, SmbFileHandleImpl handle, int openFlags, int access, int sharing) {
			this.file = file;
			this.handle = handle;
			this.openFlags = openFlags;
			this.access = access;
			this.sharing = sharing;
			this.append = false;
			this.smb2 = th.isSMB2();
			init(th);
		}


		/// <param name="th"> </param>
		/// <exception cref="SmbException"> </exception>
		/// throws jcifs.CIFSException
		protected internal void init(SmbTreeHandleImpl th) {
			int sendBufferSize = th.getSendBufferSize();
			if (this.smb2) {
				this.writeSize = sendBufferSize;
				this.writeSizeFile = sendBufferSize;
				return;
			}

			this.openFlags &= ~(SmbConstants.O_CREAT | SmbConstants.O_TRUNC); // in case we close and reopen
			this.writeSize = sendBufferSize - 70;

			this.useNTSmbs = th.hasCapability(SmbConstants.CAP_NT_SMBS);
			if (!this.useNTSmbs) {
				log.debug("No support for NT SMBs");
			}

			// there seems to be a bug with some servers that causes corruption if using signatures +
			// CAP_LARGE_WRITE
			if (th.hasCapability(SmbConstants.CAP_LARGE_WRITEX) && !th.areSignaturesActive()) {
				this.writeSizeFile = Math.Min(th.getConfig().getSendBufferSize() - 70, 0xFFFF - 70);
			}
			else {
				log.debug("No support or SMB signing is enabled, not enabling large writes");
				this.writeSizeFile = this.writeSize;
			}

			if (log.isDebugEnabled()) {
				log.debug("Negotiated file write size is " + this.writeSizeFile);
			}

			if (this.useNTSmbs) {
				this.reqx = new SmbComWriteAndX(th.getConfig());
				this.rspx = new SmbComWriteAndXResponse(th.getConfig());
			}
			else {
				this.req = new SmbComWrite(th.getConfig());
				this.rsp = new SmbComWriteResponse(th.getConfig());
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


		/// <summary>
		/// Closes this output stream and releases any system resources associated
		/// with it.
		/// </summary>
		/// <exception cref="IOException">
		///             if a network error occurs </exception>
		/// throws java.io.IOException
		public void Dispose() {
			try {
				if (this.handle.isValid()) {
					this.handle.Dispose();
				}
			}
			finally {
				this.file.clearAttributeCache();
				this.tmp = null;
			}
		}

		public void flush() {

		}

		/// <summary>
		/// Writes the specified byte to this file output stream.
		/// </summary>
		/// <exception cref="IOException">
		///             if a network error occurs </exception>
		/// throws java.io.IOException
		public void write(byte b) {
			this.tmp[0] = (byte) b;
			write(this.tmp, 0, 1);
		}

		/// <summary>
		/// Writes b.length bytes from the specified byte array to this
		/// file output stream.
		/// </summary>
		/// <exception cref="IOException">
		///             if a network error occurs </exception>
		/// throws java.io.IOException
		public virtual void write(byte[] b) {
			write(b, 0, b.Length);
		}


		/// <returns> whether the stream is open </returns>
		public virtual bool isOpen() {
			return this.handle != null && this.handle.isValid();
		}


		/// throws jcifs.CIFSException
		internal virtual SmbFileHandleImpl ensureOpen() {
			lock (this) {
				if (!isOpen()) {
					// one extra acquire to keep this open till the stream is released
					this.handle = this.file.openUnshared(this.openFlags, this.access, this.sharing, SmbConstants.ATTR_NORMAL, 0).acquire();
					if (this.append) {
						this.fp = this.handle.getInitialSize();
						if (log.isDebugEnabled()) {
							log.debug("File pointer is at " + this.fp);
						}
					}
					return this.handle;
				}

				log.trace("File already open");
				return this.handle.acquire();
			}
		}


		/// throws jcifs.CIFSException
		internal virtual SmbTreeHandleImpl ensureTreeConnected() {
			return this.file.ensureTreeConnected();
		}

		/// <summary>
		/// Writes len bytes from the specified byte array starting at
		/// offset off to this file output stream.
		/// </summary>
		/// <param name="b">
		///            The array </param>
		/// <exception cref="IOException">
		///             if a network error occurs </exception>
		/// throws java.io.IOException
		public void write(byte[] b, int off, int len) {
			writeDirect(b, off, len, 0);
		}


		/// <summary>
		/// Just bypasses TransWaitNamedPipe - used by DCERPC bind.
		/// </summary>
		/// <param name="b"> </param>
		/// <param name="off"> </param>
		/// <param name="len"> </param>
		/// <param name="flags"> </param>
		/// <exception cref="IOException"> </exception>
		/// throws java.io.IOException
		public virtual void writeDirect(byte[] b, int off, int len, int flags) {
			if (len <= 0) {
				return;
			}

			if (this.tmp == null) {
				throw new IOException("Bad file descriptor");
			}

			using (SmbFileHandleImpl fh = ensureOpen())
			using (SmbTreeHandleImpl th = (SmbTreeHandleImpl) fh.getTree()) {
				if (log.isDebugEnabled()) {
					log.debug("write: fid=" + fh + ",off=" + off + ",len=" + len);
				}

				int w;
				do {
					int blockSize = (this.file.getType() == SmbConstants.TYPE_FILESYSTEM) ? this.writeSizeFile : this.writeSize;
					w = len > blockSize ? blockSize : len;

					if (this.smb2) {
						Smb2WriteRequest wr = new Smb2WriteRequest(th.getConfig(), fh.getFileId());
						wr.setOffset(this.fp);
						wr.setData(b, off, w);

						Smb2WriteResponse resp = th.send(wr, RequestParam.NO_RETRY);
						long cnt = resp.getCount();
						this.fp += cnt;
						len -= (int) cnt;
						off += (int) cnt;
					}
					else if (this.useNTSmbs) {
						this.reqx.setParam(fh.getFid(), this.fp, len - w, b, off, w);
						if ((flags & 1) != 0) {
							this.reqx.setParam(fh.getFid(), this.fp, len, b, off, w);
							this.reqx.setWriteMode(0x8);
						}
						else {
							this.reqx.setWriteMode(0);
						}

						th.send(this.reqx, this.rspx, RequestParam.NO_RETRY);
						long cnt = this.rspx.getCount();
						this.fp += cnt;
						len -= (int) cnt;
						off += (int) cnt;
					}
					else {
						if (log.isTraceEnabled()) {
							log.trace(string.Format("Wrote at {0:D} remain {1:D} off {2:D} len {3:D}", this.fp, len - w, off, w));
						}
						this.req.setParam(fh.getFid(), this.fp, len - w, b, off, w);
						th.send(this.req, this.rsp);
						long cnt = this.rsp.getCount();
						this.fp += cnt;
						len -= (int) cnt;
						off += (int) cnt;
						if (log.isTraceEnabled()) {
							log.trace(string.Format("Wrote at {0:D} remain {1:D} off {2:D} len {3:D}", this.fp, len - w, off, w));
						}
					}

				} while (len > 0);
			}
		}

	}

}