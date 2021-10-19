using System;
using System.IO;
using System.Threading;
using cifs_ng.lib.threading;
using Logger = org.slf4j.Logger;
using LoggerFactory = org.slf4j.LoggerFactory;
using CIFSException = jcifs.CIFSException;
using jcifs;
using SmbConstants = jcifs.SmbConstants;
using SmbResource = jcifs.SmbResource;
using FileBasicInfo = jcifs.@internal.fscc.FileBasicInfo;
using SmbComSetInformation = jcifs.@internal.smb1.com.SmbComSetInformation;
using SmbComSetInformationResponse = jcifs.@internal.smb1.com.SmbComSetInformationResponse;
using Trans2SetFileInformation = jcifs.@internal.smb1.trans2.Trans2SetFileInformation;
using Trans2SetFileInformationResponse = jcifs.@internal.smb1.trans2.Trans2SetFileInformationResponse;
using Smb2SetInfoRequest = jcifs.@internal.smb2.info.Smb2SetInfoRequest;
using Smb2IoctlRequest = jcifs.@internal.smb2.ioctl.Smb2IoctlRequest;
using Smb2IoctlResponse = jcifs.@internal.smb2.ioctl.Smb2IoctlResponse;
using SrvCopyChunkCopyResponse = jcifs.@internal.smb2.ioctl.SrvCopyChunkCopyResponse;
using SrvCopychunk = jcifs.@internal.smb2.ioctl.SrvCopychunk;
using SrvCopychunkCopy = jcifs.@internal.smb2.ioctl.SrvCopychunkCopy;
using SrvRequestResumeKeyResponse = jcifs.@internal.smb2.ioctl.SrvRequestResumeKeyResponse;

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
	internal sealed class SmbCopyUtil {

		private static readonly Logger log = LoggerFactory.getLogger(typeof(SmbCopyUtil));


		/// 
		private SmbCopyUtil() {
		}


		/// <param name="dest">
		/// @return </param>
		/// <exception cref="SmbException"> </exception>
		/// <exception cref="SmbAuthException"> </exception>
		/// throws jcifs.CIFSException
		internal static SmbFileHandleImpl openCopyTargetFile(SmbFile dest, int attrs, bool alsoRead) {
			try {
				return dest.openUnshared(SmbConstants.O_CREAT | SmbConstants.O_WRONLY | SmbConstants.O_TRUNC, SmbConstants.FILE_WRITE_DATA | SmbConstants.FILE_WRITE_ATTRIBUTES | (alsoRead ? SmbConstants.FILE_READ_DATA : 0), SmbConstants.FILE_NO_SHARE, attrs, 0);
			}
			catch (SmbAuthException sae) {
				log.trace("copyTo0", sae);
				int dattrs = dest.getAttributes();
				if ((dattrs & SmbConstants.ATTR_READONLY) != 0) {
					/*
					 * Remove READONLY and try again
					 */
					dest.setPathInformation(dattrs & ~SmbConstants.ATTR_READONLY, 0L, 0L, 0L);
					return dest.openUnshared(SmbConstants.O_CREAT | SmbConstants.O_WRONLY | SmbConstants.O_TRUNC, SmbConstants.FILE_WRITE_DATA | SmbConstants.FILE_WRITE_ATTRIBUTES | (alsoRead ? SmbConstants.FILE_READ_DATA : 0), SmbConstants.FILE_NO_SHARE, attrs, 0);
				}
				throw sae;
			}
		}


		/// <param name="dest"> </param>
		/// <param name="b"> </param>
		/// <param name="bsize"> </param>
		/// <param name="w"> </param>
		/// <param name="dh"> </param>
		/// <param name="sh"> </param>
		/// <param name="req"> </param>
		/// <param name="resp"> </param>
		/// <exception cref="SmbException"> </exception>
		/// throws SmbException
		internal static void copyFile(SmbFile src, SmbFile dest, byte[][] b, int bsize, WriterThread w, SmbTreeHandleImpl sh, SmbTreeHandleImpl dh) {

			if (sh.isSMB2() && dh.isSMB2() && sh.isSameTree(dh)) {
				try {
					serverSideCopy(src, dest, sh, dh, false);
					return;
				}
				catch (SmbUnsupportedOperationException e) {
					log.debug("Server side copy not supported, falling back to normal copying", e);
				}
				catch (CIFSException e) {
					log.warn("Server side copy failed", e);
					throw SmbException.wrap(e);
				}
			}

			try {
					using (SmbFileHandleImpl sfd = src.openUnshared(0, SmbConstants.O_RDONLY, SmbConstants.FILE_SHARE_READ, SmbConstants.ATTR_NORMAL, 0))
					using (SmbFileInputStream fis = new SmbFileInputStream(src, sh, sfd)) {
					int attrs = src.getAttributes();
        
					using (SmbFileHandleImpl dfd = openCopyTargetFile(dest, attrs, false))
					using (	SmbFileOutputStream fos = new SmbFileOutputStream(dest, dh, dfd, SmbConstants.O_CREAT | SmbConstants.O_WRONLY | SmbConstants.O_TRUNC, SmbConstants.FILE_WRITE_DATA | SmbConstants.FILE_WRITE_ATTRIBUTES, SmbConstants.FILE_NO_SHARE)) {
						long mtime = src.lastModified();
						long ctime = src.createTime();
						long atime = src.lastAccess();
						int i = 0;
						long off = 0L;
						while (true) {
							int read = fis.read(b[i], 0, b[i].Length);
							lock (w) {
								w.checkException();
								while (!w.isReady()) {
									try {
										Monitor.Wait(w);
									}
									catch (ThreadInterruptedException ie) {
										throw new SmbException(dest.getURL().ToString(), ie);
									}
								}
								w.checkException();
        
								if (read <= 0) {
									break;
								}
        
								w.write(b[i], read, fos);
							}
        
							i = i == 1 ? 0 : 1;
							off += read;
						}
        
						if (log.isDebugEnabled()) {
							log.debug(string.Format("Copied a total of {0:D} bytes", off));
						}
        
						if (dh.isSMB2()) {
							Smb2SetInfoRequest req = new Smb2SetInfoRequest(dh.getConfig(), dfd.getFileId());
							req.setFileInformation(new FileBasicInfo(ctime, atime, mtime, 0L, attrs));
							dh.send(req);
						}
						else if (dh.hasCapability(SmbConstants.CAP_NT_SMBS)) {
							// use the open file descriptor
							dh.send(new Trans2SetFileInformation(dh.getConfig(), dfd.getFid(), attrs, ctime, mtime, atime), new Trans2SetFileInformationResponse(dh.getConfig()));
						}
						else {
							dh.send(new SmbComSetInformation(dh.getConfig(), dest.getUncPath(), attrs, mtime), new SmbComSetInformationResponse(dh.getConfig()));
						}
					}
					}
			}
			catch (IOException se) {
				if (!src.getContext().getConfig().isIgnoreCopyToException()) {
					throw new SmbException("Failed to copy file from [" + src.ToString() + "] to [" + dest.ToString() + "]", se);
				}
				log.warn("Copy failed", se);
			}
		}


		/// <param name="src"> </param>
		/// <param name="dest"> </param>
		/// <param name="sh"> </param>
		/// <param name="dh"> </param>
		/// <exception cref="SmbException"> </exception>
		/// throws jcifs.CIFSException
		private static void serverSideCopy(SmbFile src, SmbFile dest, SmbTreeHandleImpl sh, SmbTreeHandleImpl dh, bool write) {
			log.debug("Trying server side copy");
			SmbFileHandleImpl dfd = null;
			try {
				long size;
				byte[] resumeKey;

				// despite there being a resume key, we still need an open file descriptor?
				using (SmbFileHandleImpl sfd = src.openUnshared(0, SmbConstants.O_RDONLY, SmbConstants.FILE_SHARE_READ, SmbConstants.ATTR_NORMAL, 0)) {
					if (sfd.getInitialSize() == 0) {
						using (SmbFileHandleImpl edfd = openCopyTargetFile(dest, src.getAttributes(), !write)) {
							return;
						}
					}

					Smb2IoctlRequest resumeReq = new Smb2IoctlRequest(sh.getConfig(), Smb2IoctlRequest.FSCTL_SRV_REQUEST_RESUME_KEY, sfd.getFileId());
					resumeReq.setFlags(Smb2IoctlRequest.SMB2_O_IOCTL_IS_FSCTL);
					Smb2IoctlResponse resumeResp = sh.send(resumeReq);
					SrvRequestResumeKeyResponse rkresp = resumeResp.getOutputData<SrvRequestResumeKeyResponse>(typeof(SrvRequestResumeKeyResponse));
					size = sfd.getInitialSize();
					resumeKey = rkresp.getResumeKey();

					// start with some reasonably safe defaults, the server will till us if it does not like it
					// can we resume this if we loose the file descriptor?

					int maxChunks = 256;
					int maxChunkSize = 1024 * 1024;
					int byteLimit = 16 * 1024 * 1024;
					bool retry = false;
					do {
						long ooff = 0;
						while (ooff < size) {
							long wsize = size - ooff;
							if (wsize > byteLimit) {
								wsize = byteLimit;
							}

							int chunks = (int)(wsize / maxChunkSize);
							int lastChunkSize;
							if (chunks + 1 > maxChunks) {
								chunks = maxChunks;
								lastChunkSize = maxChunkSize;
							}
							else {
								lastChunkSize = (int)(wsize % maxChunkSize);
								if (lastChunkSize != 0) {
									chunks++;
								}
								else {
									lastChunkSize = maxChunkSize;
								}
							}

							SrvCopychunk[] chunkInfo = new SrvCopychunk[chunks];
							long ioff = 0;
							for (int i = 0; i < chunks; i++) {
								long absoff = ooff + ioff;
								int csize = i == chunks - 1 ? lastChunkSize : maxChunkSize;
								chunkInfo[i] = new SrvCopychunk(absoff, absoff, csize);
								ioff += maxChunkSize;
							}

							if (dfd == null || !dfd.isValid()) {
								// don't reopen the file for every round if it's not necessary, keep the lock
								dfd = openCopyTargetFile(dest, src.getAttributes(), !write);
							}

							// FSCTL_SRV_COPYCHUNK_WRITE allows to open the file for writing only, FSCTL_SRV_COPYCHUNK also
							// needs read access
							Smb2IoctlRequest copy = new Smb2IoctlRequest(sh.getConfig(), write ? Smb2IoctlRequest.FSCTL_SRV_COPYCHUNK_WRITE : Smb2IoctlRequest.FSCTL_SRV_COPYCHUNK, dfd.getFileId());
							copy.setFlags(Smb2IoctlRequest.SMB2_O_IOCTL_IS_FSCTL);
							copy.setInputData(new SrvCopychunkCopy(resumeKey, chunkInfo));

							try {
								SrvCopyChunkCopyResponse r = dh.send(copy, RequestParam.NO_RETRY).getOutputData<SrvCopyChunkCopyResponse>(typeof(SrvCopyChunkCopyResponse));
								if (log.isDebugEnabled()) {
									log.debug(string.Format("Wrote {0:D} bytes ({1:D} chunks, last partial write {2:D})", r.getTotalBytesWritten(), r.getChunksWritten(), r.getChunkBytesWritten()));
								}
								ooff += r.getTotalBytesWritten();
							}
							catch (SmbException e) {
								Smb2IoctlResponse response = (Smb2IoctlResponse)copy.getResponse();
								if (!retry && response.isReceived() && !response.isError() && response.getStatus() == NtStatus.NT_STATUS_INVALID_PARAMETER) {
									retry = true;
									SrvCopyChunkCopyResponse outputData = response.getOutputData<SrvCopyChunkCopyResponse>(typeof(SrvCopyChunkCopyResponse));
									maxChunks = outputData.getChunksWritten();
									maxChunkSize = outputData.getChunkBytesWritten();
									byteLimit = outputData.getTotalBytesWritten();
									continue;
								}
								throw e;
							}
						}
						break;
					} while (retry);
				}
			}
			catch (SmbUnsupportedOperationException e) {
				throw e;
			}
			catch (IOException se) {
				throw new CIFSException("Server side copy failed", se);
			}
			finally {
				if (dfd != null) {
					dfd.Dispose();
				}
			}
		}


		/// <param name="dest"> </param>
		/// <param name="b"> </param>
		/// <param name="bsize"> </param>
		/// <param name="w"> </param>
		/// <param name="dh"> </param>
		/// <param name="sh"> </param>
		/// <param name="req"> </param>
		/// <param name="resp"> </param>
		/// <exception cref="SmbException"> </exception>
		/// throws jcifs.CIFSException
		internal static void copyDir(SmbFile src, SmbFile dest, byte[][] b, int bsize, WriterThread w, SmbTreeHandleImpl sh, SmbTreeHandleImpl dh) {
			string path = dest.getLocator().getUNCPath();
			if (path.Length > 1) {
				try {
					dest.mkdir();
					if (dh.hasCapability(SmbConstants.CAP_NT_SMBS)) {
						dest.setPathInformation(src.getAttributes(), src.createTime(), src.lastModified(), src.lastAccess());
					}
					else {
						dest.setPathInformation(src.getAttributes(), 0L, src.lastModified(), 0L);
					}
				}
				catch (SmbUnsupportedOperationException e) {
					if (src.getContext().getConfig().isIgnoreCopyToException()) {
						log.warn("Failed to set file attributes on " + path, e);
					}
					else {
						throw e;
					}
				}
				catch (SmbException se) {
					log.trace("copyTo0", se);
					if (se.getNtStatus() != NtStatus.NT_STATUS_ACCESS_DENIED && se.getNtStatus() != NtStatus.NT_STATUS_OBJECT_NAME_COLLISION) {
						throw se;
					}
				}
			}

			try {
					using (CloseableIterator<SmbResource> it = SmbEnumerationUtil.doEnum(src, "*", SmbConstants.ATTR_DIRECTORY | SmbConstants.ATTR_HIDDEN | SmbConstants.ATTR_SYSTEM, null, null)) {
					while (it.hasNext()) {
						using (SmbResource r = it.next()) {
							using (SmbFile ndest = new SmbFile(dest, r.getLocator().getName(), true, r.getLocator().getType(), r.getAttributes(), r.createTime(), r.lastModified(), r.lastAccess(), r.length())) {
        
								if (r is SmbFile) {
									((SmbFile) r).copyRecursive(ndest, b, bsize, w, sh, dh);
								}
        
							}
						}
					}
					}
			}
			catch (UriFormatException mue) {
				throw new SmbException(src.getURL().ToString(), mue);
			}
		}

	}


	internal class WriterThread : Runnable {

		private byte[] b;
		private int n;
		private bool ready;
		private SmbFileOutputStream @out;

		private SmbException e = null;


		private Thread _thread;
		internal WriterThread() 
		{
			_thread = new Thread(this.run);
			_thread.Name = "JCIFS-WriterThread";
			this.ready = false;
		}

		public void Start()
		{
			_thread.Start();
		}
		
		public void Join()
		{
			_thread.Join();
		}
		
		public void Interrupt()
		{
			_thread.Interrupt();
		}

		/// <returns> the ready </returns>
		internal virtual bool isReady() {
			return this.ready;
		}


		/// <exception cref="SmbException">
		///  </exception>
		/// throws SmbException
		public virtual void checkException() {
			if (this.e != null) {
				throw this.e;
			}
		}


		internal virtual void write(byte[] buffer, int len, SmbFileOutputStream d) {
			lock (this) {
				this.b = buffer;
				this.n = len;
				this.@out = d;
				this.ready = false;
				Monitor.Pulse(this);
			}
		}


		public  void run() {
			lock (this) {
				try {
					for (;;) {
						Monitor.Pulse(this);
						this.ready = true;
						while (this.ready) {
							Monitor.Wait(this);
						}
						if (this.n == -1) {
							return;
						}

						this.@out.write(this.b, 0, this.n);
					}
				}
				catch (SmbException ex) {
					this.e = ex;
				}
				catch (Exception x) {
					this.e = new SmbException("WriterThread", x);
				}
				Monitor.Pulse(this);
			}
		}

		public void setDaemon(bool b1)
		{
			_thread.IsBackground = b1;
		}
	}

}