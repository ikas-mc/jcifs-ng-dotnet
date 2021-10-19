using System;
using System.IO;
using System.Text;
using cifs_ng.lib.ext;
using Logger = org.slf4j.Logger;
using LoggerFactory = org.slf4j.LoggerFactory;
using CIFSContext = jcifs.CIFSContext;
using CIFSException = jcifs.CIFSException;
using SmbConstants = jcifs.SmbConstants;
using SmbFileHandle = jcifs.SmbFileHandle;
using SmbRandomAccess = jcifs.SmbRandomAccess;
using FileEndOfFileInformation = jcifs.@internal.fscc.FileEndOfFileInformation;
using SmbComReadAndX = jcifs.@internal.smb1.com.SmbComReadAndX;
using SmbComReadAndXResponse = jcifs.@internal.smb1.com.SmbComReadAndXResponse;
using SmbComWrite = jcifs.@internal.smb1.com.SmbComWrite;
using SmbComWriteAndX = jcifs.@internal.smb1.com.SmbComWriteAndX;
using SmbComWriteAndXResponse = jcifs.@internal.smb1.com.SmbComWriteAndXResponse;
using SmbComWriteResponse = jcifs.@internal.smb1.com.SmbComWriteResponse;
using Trans2SetFileInformation = jcifs.@internal.smb1.trans2.Trans2SetFileInformation;
using Trans2SetFileInformationResponse = jcifs.@internal.smb1.trans2.Trans2SetFileInformationResponse;
using Smb2SetInfoRequest = jcifs.@internal.smb2.info.Smb2SetInfoRequest;
using Smb2ReadRequest = jcifs.@internal.smb2.io.Smb2ReadRequest;
using Smb2ReadResponse = jcifs.@internal.smb2.io.Smb2ReadResponse;
using Smb2WriteRequest = jcifs.@internal.smb2.io.Smb2WriteRequest;
using Smb2WriteResponse = jcifs.@internal.smb2.io.Smb2WriteResponse;
using Encdec = jcifs.util.Encdec;

/* jcifs smb client library in Java
 * Copyright (C) 2003  "Michael B. Allen" <jcifs at samba dot org>
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
	/// 
	/// 
	public class SmbRandomAccessFile : SmbRandomAccess {

		private static readonly Logger log = LoggerFactory.getLogger(typeof(SmbRandomAccessFile));
		private const int WRITE_OPTIONS = 0x0842;

		private SmbFile file;
		private long fp;
		private int openFlags, access = 0, readSize, writeSize, options = 0;
		private byte[] tmp = new byte[8];
		private SmbComWriteAndXResponse write_andx_resp = null;

		private bool largeReadX;
		private readonly bool unsharedFile;

		private SmbFileHandleImpl handle;

		private int sharing;


		/// <summary>
		/// Instantiate a random access file from URL
		/// </summary>
		/// <param name="url"> </param>
		/// <param name="mode"> </param>
		/// <param name="sharing"> </param>
		/// <param name="tc"> </param>
		/// <exception cref="SmbException"> </exception>
		/// <exception cref="MalformedURLException"> </exception>
		/// throws SmbException, java.net.MalformedURLException
		public SmbRandomAccessFile(string url, string mode, int sharing, CIFSContext tc) : this(new SmbFile(url, tc), mode, sharing, true) {
		}


		/// <summary>
		/// Instantiate a random access file from a <seealso cref="SmbFile"/>
		/// </summary>
		/// <param name="file"> </param>
		/// <param name="mode"> </param>
		/// <exception cref="SmbException"> </exception>
		/// throws SmbException
		public SmbRandomAccessFile(SmbFile file, string mode) : this(file, mode, SmbConstants.DEFAULT_SHARING, false) {
		}


		/// <summary>
		/// Instantiate a random access file from a <seealso cref="SmbFile"/>
		/// </summary>
		/// <param name="file"> </param>
		/// <param name="mode"> </param>
		/// <exception cref="SmbException"> </exception>
		/// throws SmbException
		internal SmbRandomAccessFile(SmbFile file, string mode, int sharing, bool unsharedFile) {
			this.file = file;
			this.sharing = sharing;
			this.unsharedFile = unsharedFile;

			try {
					using (SmbTreeHandleInternal th = this.file.ensureTreeConnected()) {
					if (mode.Equals("r")) {
						this.openFlags = SmbConstants.O_CREAT | SmbConstants.O_RDONLY;
						this.access = SmbConstants.FILE_READ_DATA;
					}
					else if (mode.Equals("rw")) {
						this.openFlags = SmbConstants.O_CREAT | SmbConstants.O_RDWR | SmbConstants.O_APPEND;
						this.write_andx_resp = new SmbComWriteAndXResponse(th.getConfig());
						this.options = WRITE_OPTIONS;
						this.access = SmbConstants.FILE_READ_DATA | SmbConstants.FILE_WRITE_DATA;
					}
					else {
						throw new System.ArgumentException("Invalid mode");
					}
        
					using (SmbFileHandle h = ensureOpen()) {
					}
					this.readSize = th.getReceiveBufferSize() - 70;
					this.writeSize = th.getSendBufferSize() - 70;
        
					if (th.hasCapability(SmbConstants.CAP_LARGE_READX)) {
						this.largeReadX = true;
						this.readSize = Math.Min(th.getConfig().getReceiveBufferSize() - 70, th.areSignaturesActive() ? 0xFFFF - 70 : 0xFFFFFF - 70);
					}
        
					// there seems to be a bug with some servers that causes corruption if using signatures + CAP_LARGE_WRITE
					if (th.hasCapability(SmbConstants.CAP_LARGE_WRITEX) && !th.areSignaturesActive()) {
						this.writeSize = Math.Min(th.getConfig().getSendBufferSize() - 70, 0xFFFF - 70);
					}
        
					this.fp = 0L;
					}
			}
			catch (CIFSException e) {
				throw SmbException.wrap(e);
			}
		}


		/// <summary>
		/// @return </summary>
		/// <exception cref="SmbException"> </exception>
		/// throws jcifs.CIFSException
		internal virtual SmbFileHandleImpl ensureOpen() {
			lock (this) {
				// ensure file is open
				if (this.handle == null || !this.handle.isValid()) {
					// one extra acquire to keep this open till the stream is released
					this.handle = this.file.openUnshared(this.openFlags, this.access, this.sharing, SmbConstants.ATTR_NORMAL, this.options).acquire();
					return this.handle;
				}
				return this.handle.acquire();
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


		/// throws SmbException
		public virtual void Dispose() {
			lock (this) {
				try {
					if (this.handle != null) {
						try {
							this.handle.Dispose();
						}
						catch (CIFSException e) {
							throw SmbException.wrap(e);
						}
						this.handle = null;
					}
				}
				finally {
					this.file.clearAttributeCache();
					if (this.unsharedFile) {
						this.file.Dispose();
					}
				}
			}
		}


		/// throws SmbException
		public virtual int read() {
			if (read(this.tmp, 0, 1) == -1) {
				return -1;
			}
			return this.tmp[0] & 0xFF;
		}


		/// throws SmbException
		public virtual int read(byte[] b) {
			return read(b, 0, b.Length);
		}


		/// throws SmbException
		public virtual int read(byte[] b, int off, int len) {
			if (len <= 0) {
				return 0;
			}
			long start = this.fp;

			try {
					using (SmbFileHandleImpl fh = ensureOpen())
					using (	SmbTreeHandleImpl th = (SmbTreeHandleImpl)fh.getTree()) {
					int r, n;
					SmbComReadAndXResponse response = new SmbComReadAndXResponse(th.getConfig(), b, off);
					do {
						r = len > this.readSize ? this.readSize : len;
        
						if (th.isSMB2()) {
							Smb2ReadRequest request = new Smb2ReadRequest(th.getConfig(), fh.getFileId(), b, off);
							request.setOffset(this.fp);
							request.setReadLength(r);
							request.setRemainingBytes(len - off);
							try {
								Smb2ReadResponse resp = th.send(request, RequestParam.NO_RETRY);
								n = resp.getDataLength();
							}
							catch (SmbException e) {
								if (e.getNtStatus() == unchecked((int)0xC0000011)) {
									log.debug("Reached end of file", e);
									n = -1;
								}
								else {
									throw e;
								}
							}
						}
						else {
							SmbComReadAndX request = new SmbComReadAndX(th.getConfig(), fh.getFid(), this.fp, r, null);
							if (this.largeReadX) {
								request.setMaxCount(r & 0xFFFF);
								request.setOpenTimeout((r >> 16) & 0xFFFF);
							}
        
							try {
								th.send(request, response, RequestParam.NO_RETRY);
								n = response.getDataLength();
							}
							catch (CIFSException e) {
								throw SmbException.wrap(e);
							}
						}
						if (n <= 0) {
							return (int)((this.fp - start) > 0L ? this.fp - start : -1);
						}
						this.fp += n;
						len -= n;
						off += n;
						response.adjustOffset(n);
					} while (len > 0 && n == r);
        
					return (int)(this.fp - start);
					}
			}
			catch (CIFSException e) {
				throw SmbException.wrap(e);
			}
		}


		/// throws SmbException
		public  void readFully(byte[] b) {
			readFully(b, 0, b.Length);
		}


		/// throws SmbException
		public  void readFully(byte[] b, int off, int len) {
			int n = 0, count;

			do {
				count = this.read(b, off + n, len - n);
				if (count < 0) {
					throw new SmbEndOfFileException();
				}
				n += count;
			} while (n < len);
		}


		/// throws SmbException
		public  int skipBytes(int n) {
			if (n > 0) {
				this.fp += n;
				return n;
			}
			return 0;
		}


		/// throws SmbException
		public  void write(int b) {
			this.tmp[0] = (byte) b;
			write(this.tmp, 0, 1);
		}


		/// throws SmbException
		public  void write(byte[] b) {
			write(b, 0, b.Length);
		}


		/// throws SmbException
		public  void write(byte[] b, int off, int len) {
			if (len <= 0) {
				return;
			}

			// ensure file is open
			try {
					using (SmbFileHandleImpl fh = ensureOpen())
					using (SmbTreeHandleImpl th = (SmbTreeHandleImpl)fh.getTree()) {
					int w;
					do {
						w = len > this.writeSize ? this.writeSize : len;
						long cnt;
        
						if (th.isSMB2()) {
							Smb2WriteRequest request = new Smb2WriteRequest(th.getConfig(), fh.getFileId());
							request.setOffset(this.fp);
							request.setRemainingBytes(len - w - off);
							request.setData(b, off, w);
							Smb2WriteResponse resp = th.send(request, RequestParam.NO_RETRY);
							cnt = resp.getCount();
						}
						else {
							SmbComWriteAndX request = new SmbComWriteAndX(th.getConfig(), fh.getFid(), this.fp, len - w - off, b, off, w, null);
							th.send(request, this.write_andx_resp, RequestParam.NO_RETRY);
							cnt = this.write_andx_resp.getCount();
						}
        
						this.fp += cnt;
						len -= (int)cnt;
						off += (int)cnt;
					} while (len > 0);
					}
			}
			catch (CIFSException e) {
				throw SmbException.wrap(e);
			}
		}


		public virtual long getFilePointer() {
			return this.fp;
		}


		public virtual void seek(long pos) {
			this.fp = pos;
		}


		/// throws SmbException
		public virtual long length() {
			return this.file.length();
		}


		/// throws SmbException
		public virtual void setLength(long newLength) {
			try {
					using (SmbFileHandleImpl fh = ensureOpen())
					using (	SmbTreeHandleImpl th =(SmbTreeHandleImpl) fh.getTree()) {
					if (th.isSMB2()) {
						Smb2SetInfoRequest req = new Smb2SetInfoRequest(th.getConfig(), fh.getFileId());
						req.setFileInformation(new FileEndOfFileInformation(newLength));
						th.send(req, RequestParam.NO_RETRY);
					}
					else if (th.hasCapability(SmbConstants.CAP_NT_SMBS)) {
						th.send(new Trans2SetFileInformation(th.getConfig(), fh.getFid(), new FileEndOfFileInformation(newLength)), new Trans2SetFileInformationResponse(th.getConfig()), RequestParam.NO_RETRY);
					}
					else {
						// this is the original, COM_WRITE allows truncation but no 64 bit offsets
						SmbComWriteResponse rsp = new SmbComWriteResponse(th.getConfig());
						th.send(new SmbComWrite(th.getConfig(), fh.getFid(), unchecked((int)(newLength & 0xFFFFFFFFL)), 0, this.tmp, 0, 0), rsp, RequestParam.NO_RETRY);
					}
					}
			}
			catch (CIFSException e) {
				throw SmbException.wrap(e);
			}
		}


		/// throws SmbException
		public  bool readBoolean() {
			if ((read(this.tmp, 0, 1)) < 0) {
				throw new SmbEndOfFileException();
			}
			return this.tmp[0] != (byte) 0x00;
		}


		/// throws SmbException
		public  byte readByte() {
			if ((read(this.tmp, 0, 1)) < 0) {
				throw new SmbEndOfFileException();
			}
			return this.tmp[0];
		}


		/// throws SmbException
		public  int readUnsignedByte() {
			if ((read(this.tmp, 0, 1)) < 0) {
				throw new SmbEndOfFileException();
			}
			return this.tmp[0] & 0xFF;
		}


		/// throws SmbException
		public  short readShort() {
			if ((read(this.tmp, 0, 2)) < 0) {
				throw new SmbEndOfFileException();
			}
			return Encdec.dec_uint16be(this.tmp, 0);
		}


		/// throws SmbException
		public  int readUnsignedShort() {
			if ((read(this.tmp, 0, 2)) < 0) {
				throw new SmbEndOfFileException();
			}
			return Encdec.dec_uint16be(this.tmp, 0) & 0xFFFF;
		}


		/// throws SmbException
		public  char readChar() {
			if ((read(this.tmp, 0, 2)) < 0) {
				throw new SmbEndOfFileException();
			}
			return (char) Encdec.dec_uint16be(this.tmp, 0);
		}


		/// throws SmbException
		public  int readInt() {
			if ((read(this.tmp, 0, 4)) < 0) {
				throw new SmbEndOfFileException();
			}
			return Encdec.dec_uint32be(this.tmp, 0);
		}


		/// throws SmbException
		public  long readLong() {
			if ((read(this.tmp, 0, 8)) < 0) {
				throw new SmbEndOfFileException();
			}
			return Encdec.dec_uint64be(this.tmp, 0);
		}


		/// throws SmbException
		public  float readFloat() {
			if ((read(this.tmp, 0, 4)) < 0) {
				throw new SmbEndOfFileException();
			}
			return Encdec.dec_floatbe(this.tmp, 0);
		}


		/// throws SmbException
		public  double readDouble() {
			if ((read(this.tmp, 0, 8)) < 0) {
				throw new SmbEndOfFileException();
			}
			return Encdec.dec_doublebe(this.tmp, 0);
		}


		/// throws SmbException
		public  string readLine() {
			StringBuilder input = new StringBuilder();
			int c = -1;
			bool eol = false;

			while (!eol) {
				switch (c = read()) {
				case -1:
				case '\n':
					eol = true;
					break;
				case '\r':
					eol = true;
					long cur = this.fp;
					if (read() != '\n') {
						this.fp = cur;
					}
					break;
				default:
					input.Append((char) c);
					break;
				}
			}

			if ((c == -1) && (input.Length == 0)) {
				return null;
			}

			return input.ToString();
		}


		/// throws SmbException
		public  string readUTF() {
			int size = readUnsignedShort();
			byte[] b = new byte[size];
			read(b, 0, size);
			try {
				return Encdec.dec_utf8(b, 0, size);
			}
			catch (IOException ioe) {
				throw new SmbException("", ioe);
			}
		}


		/// throws SmbException
		public  void writeBoolean(bool v) {
			this.tmp[0] = (byte)(v ? 1 : 0);
			write(this.tmp, 0, 1);
		}


		/// throws SmbException
		public  void writeByte(int v) {
			this.tmp[0] = (byte) v;
			write(this.tmp, 0, 1);
		}


		/// throws SmbException
		public  void writeShort(int v) {
			Encdec.enc_uint16be((short) v, this.tmp, 0);
			write(this.tmp, 0, 2);
		}


		/// throws SmbException
		public  void writeChar(int v) {
			Encdec.enc_uint16be((short) v, this.tmp, 0);
			write(this.tmp, 0, 2);
		}


		/// throws SmbException
		public  void writeInt(int v) {
			Encdec.enc_uint32be(v, this.tmp, 0);
			write(this.tmp, 0, 4);
		}


		/// throws SmbException
		public  void writeLong(long v) {
			Encdec.enc_uint64be(v, this.tmp, 0);
			write(this.tmp, 0, 8);
		}


		/// throws SmbException
		public  void writeFloat(float v) {
			Encdec.enc_floatbe(v, this.tmp, 0);
			write(this.tmp, 0, 4);
		}


		/// throws SmbException
		public  void writeDouble(double v) {
			Encdec.enc_doublebe(v, this.tmp, 0);
			write(this.tmp, 0, 8);
		}


		/// throws SmbException
		public  void writeBytes(string s) {
			byte[] b = s.getBytes();
			write(b, 0, b.Length);
		}


		/// throws SmbException
		public  void writeChars(string s) {
			int clen = s.Length;
			int blen = 2 * clen;
			byte[] b = new byte[blen];
			char[] c = new char[clen];
			s.CopyTo(0, c, 0, clen - 0);
			for (int i = 0, j = 0; i < clen; i++) {
				b[j++] = (byte)((int)((uint)c[i] >> 8));
				b[j++] = (byte)((int)((uint)c[i] >> 0));
			}
			write(b, 0, blen);
		}


		/// throws SmbException
		public  void writeUTF(string str) {
			int len = str.Length;
			int ch, size = 0;
			byte[] dst;

			for (int i = 0; i < len; i++) {
				ch = str[i];
				size += ch > 0x07F ? (ch > 0x7FF ? 3 : 2) : 1;
			}
			dst = new byte[size];
			writeShort(size);
			Encdec.enc_utf8(str, dst, 0, size);
			write(dst, 0, size);
		}

	}

}