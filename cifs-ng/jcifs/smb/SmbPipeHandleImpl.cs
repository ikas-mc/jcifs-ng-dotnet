using jcifs;

using System;
using System.IO;
using cifs_ng.lib.io;
using Logger = org.slf4j.Logger;
using LoggerFactory = org.slf4j.LoggerFactory;
using CIFSException = jcifs.CIFSException;
using SmbConstants = jcifs.SmbConstants;
using SmbPipeHandle = jcifs.SmbPipeHandle;
using SmbPipeResource = jcifs.SmbPipeResource;
using TransCallNamedPipe = jcifs.@internal.smb1.trans.TransCallNamedPipe;
using TransCallNamedPipeResponse = jcifs.@internal.smb1.trans.TransCallNamedPipeResponse;
using TransTransactNamedPipe = jcifs.@internal.smb1.trans.TransTransactNamedPipe;
using TransTransactNamedPipeResponse = jcifs.@internal.smb1.trans.TransTransactNamedPipeResponse;
using TransWaitNamedPipe = jcifs.@internal.smb1.trans.TransWaitNamedPipe;
using TransWaitNamedPipeResponse = jcifs.@internal.smb1.trans.TransWaitNamedPipeResponse;
using Smb2IoctlRequest = jcifs.@internal.smb2.ioctl.Smb2IoctlRequest;
using Smb2IoctlResponse = jcifs.@internal.smb2.ioctl.Smb2IoctlResponse;
using ByteEncodable = jcifs.util.ByteEncodable;

/*
 * © 2017 AgNO3 Gmbh & Co. KG
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
	/// 
	/// </summary>
	internal class SmbPipeHandleImpl : SmbPipeHandleInternal {

		private static readonly Logger log = LoggerFactory.getLogger(typeof(SmbPipeHandleImpl));

		private readonly SmbNamedPipe pipe;
		private readonly bool transact;
		private readonly bool call;

		private readonly int openFlags;
		private readonly int access;
		private volatile bool open = true;

		private SmbFileHandleImpl handle;
		private SmbPipeOutputStream output;
		private SmbPipeInputStream input;

		private readonly string uncPath;

		private SmbTreeHandleImpl treeHandle;

		private int sharing = SmbConstants.DEFAULT_SHARING;

		/// <param name="pipe"> </param>
		public SmbPipeHandleImpl(SmbNamedPipe pipe) {
			this.pipe = pipe;
			this.transact = (pipe.getPipeType() & SmbPipeResourceConstants.PIPE_TYPE_TRANSACT) == SmbPipeResourceConstants.PIPE_TYPE_TRANSACT;
			this.call = (pipe.getPipeType() & SmbPipeResourceConstants.PIPE_TYPE_CALL) == SmbPipeResourceConstants.PIPE_TYPE_CALL;
			this.openFlags = (pipe.getPipeType() & unchecked((int)0xFFFF00FF)) | SmbConstants.O_EXCL;
			this.access = (pipe.getPipeType() & 7) | 0x20000;
			this.uncPath = this.pipe.getUncPath();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.SmbPipeHandle#unwrap(java.lang.Class) </seealso>
		public virtual T unwrap<T>(Type type)  { //where T : SmbPipeHandle //TODO 
			if (this is T v) {
				return  v;
			}
			throw new System.InvalidCastException();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.SmbPipeHandle#getPipe() </seealso>
		public virtual SmbPipeResource getPipe() {
			return this.pipe;
		}

		InputStream SmbPipeHandle.getInput()
		{
			return getInput();
		}

		OutputStream SmbPipeHandle.getOutput()
		{
			return getOutput();
		}


		/// throws jcifs.CIFSException
		public virtual SmbTreeHandleInternal  ensureTreeConnected() {
			if (this.treeHandle == null) {
				// extra acquire to keep the tree alive
				this.treeHandle = this.pipe.ensureTreeConnected();
			}
			return this.treeHandle.acquire();
		}


		public virtual string getUncPath() {
			return this.uncPath;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.SmbPipeHandle#isOpen() </seealso>
		public virtual bool isOpen() {
			return this.open && this.handle != null && this.handle.isValid();
		}


		/// 
		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.smb.SmbPipeHandleInternal#getSessionKey() </seealso>
		/// throws jcifs.CIFSException
		public virtual byte[] getSessionKey() {
			using (SmbTreeHandleImpl th =(SmbTreeHandleImpl) ensureTreeConnected())
			using (	SmbSessionImpl sess =(SmbSessionImpl) th.getSession()) {
				return sess.getSessionKey();
			}
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.SmbPipeHandle#isStale() </seealso>
		public virtual bool isStale() {
			return !this.open || (this.handle != null && !this.handle.isValid());
		}


		/// throws jcifs.CIFSException
		public virtual SmbFileHandle ensureOpen() {
			lock (this) {
				if (!this.open) {
					throw new SmbException("Pipe handle already closed");
				}
        
				if (!isOpen()) {
					using (SmbTreeHandleImpl th = (SmbTreeHandleImpl)ensureTreeConnected()) {
        
						if (th.isSMB2()) {
							this.handle = this.pipe.openUnshared(this.uncPath, 0, this.access, this.sharing, SmbConstants.ATTR_NORMAL, 0);
							return this.handle.acquire();
						}
        
						// TODO: wait for pipe, still not sure when this needs to be called exactly
						if (this.uncPath.StartsWith("\\pipe\\", StringComparison.Ordinal)) {
							th.send(new TransWaitNamedPipe(th.getConfig(), this.uncPath), new TransWaitNamedPipeResponse(th.getConfig()));
						}
        
						if (th.hasCapability(SmbConstants.CAP_NT_SMBS) || this.uncPath.StartsWith("\\pipe\\", StringComparison.Ordinal)) {
							this.handle = this.pipe.openUnshared(this.openFlags, this.access, this.sharing, SmbConstants.ATTR_NORMAL, 0);
						}
						else {
							// at least on samba, SmbComOpenAndX fails without the pipe prefix
							this.handle = this.pipe.openUnshared("\\pipe" + getUncPath(), this.openFlags, this.access, this.sharing, SmbConstants.ATTR_NORMAL, 0);
						}
						// one extra acquire to keep this open till the stream is released
						return this.handle.acquire();
					}
        
				}
				log.trace("Pipe already open");
				return this.handle.acquire();
			}
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.SmbPipeHandle#getInput() </seealso>
		/// throws jcifs.CIFSException
		public virtual SmbPipeInputStream getInput() {

			if (!this.open) {
				throw new SmbException("Already closed");
			}

			if (this.input != null) {
				return this.input;
			}

			using (SmbTreeHandleImpl th = (SmbTreeHandleImpl)ensureTreeConnected()) {
				this.input = new SmbPipeInputStream(this, th);
			}
			return this.input;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.SmbPipeHandle#getOutput() </seealso>
		/// throws jcifs.CIFSException
		public virtual SmbPipeOutputStream getOutput() {
			if (!this.open) {
				throw new SmbException("Already closed");
			}

			if (this.output != null) {
				return this.output;
			}

			using (SmbTreeHandleImpl th = (SmbTreeHandleImpl)ensureTreeConnected()) {
				this.output = new SmbPipeOutputStream(this, th);
			}
			return this.output;
		}


		/// 
		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.smb.SmbPipeHandleInternal#sendrecv(byte[], int, int, byte[], int) </seealso>
		/// throws java.io.IOException
		public virtual int sendrecv(byte[] buf, int off, int length, byte[] inB, int maxRecvSize) {
			using (SmbFileHandleImpl fh = (SmbFileHandleImpl)ensureOpen())
			using (	SmbTreeHandleImpl th = (SmbTreeHandleImpl)fh.getTree()) {

				if (th.isSMB2()) {
					Smb2IoctlRequest req = new Smb2IoctlRequest(th.getConfig(), Smb2IoctlRequest.FSCTL_PIPE_TRANSCEIVE, fh.getFileId(), inB);
					req.setFlags(Smb2IoctlRequest.SMB2_O_IOCTL_IS_FSCTL);
					req.setInputData(new ByteEncodable(buf, off, length));
					req.setMaxOutputResponse(maxRecvSize);
					Smb2IoctlResponse resp = th.send(req, RequestParam.NO_RETRY);
					return resp.getOutputLength();
				}
				else if (this.transact) {
					TransTransactNamedPipe req = new TransTransactNamedPipe(th.getConfig(), fh.getFid(), buf, off, length);
					TransTransactNamedPipeResponse resp = new TransTransactNamedPipeResponse(th.getConfig(), inB);
					if ((getPipeType() & SmbPipeResourceConstants.PIPE_TYPE_DCE_TRANSACT) == SmbPipeResourceConstants.PIPE_TYPE_DCE_TRANSACT) {
						req.setMaxDataCount(1024);
					}
					th.send(req, resp, RequestParam.NO_RETRY);
					return resp.getResponseLength();
				}
				else if (this.call) {
					th.send(new TransWaitNamedPipe(th.getConfig(), this.uncPath), new TransWaitNamedPipeResponse(th.getConfig()));
					TransCallNamedPipeResponse resp = new TransCallNamedPipeResponse(th.getConfig(), inB);
					th.send(new TransCallNamedPipe(th.getConfig(), this.uncPath, buf, off, length), resp);
					return resp.getResponseLength();
				}
				else {
					SmbPipeOutputStream @out = getOutput();
					SmbPipeInputStream @in = getInput();
					@out.write(buf, off, length);
					return @in.read(inB, 0, inB.Length);
				}
			}
		}


		/// throws java.io.IOException
		public virtual int recv(byte[] buf, int off, int len) {
			return getInput().readDirect(buf, off, len);

		}


		/// throws java.io.IOException
		public virtual void send(byte[] buf, int off, int length) {
			getOutput().writeDirect(buf, off, length, 1);
		}


		/// 
		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.smb.SmbPipeHandleInternal#getPipeType() </seealso>
		public virtual int getPipeType() {
			return this.pipe.getPipeType();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.SmbPipeHandle#Dispose() </seealso>
		/// throws jcifs.CIFSException
		public virtual void Dispose() {
			lock (this) {
				bool wasOpen = isOpen();
				this.open = false;
				if (this.input != null) {
					this.input.Dispose();
					this.input = null;
				}
        
				if (this.output != null) {
					this.output.Dispose();
					this.output = null;
				}
        
				try {
					if (wasOpen) {
						this.handle.Dispose();
					}
					else if (this.handle != null) {
						this.handle.release();
					}
					this.handle = null;
				}
				finally {
					if (this.treeHandle != null) {
						this.treeHandle.release();
					}
				}
			}
		}

	}

}