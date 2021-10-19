using CIFSException = jcifs.CIFSException;
using TransPeekNamedPipe = jcifs.@internal.smb1.trans.TransPeekNamedPipe;
using TransPeekNamedPipeResponse = jcifs.@internal.smb1.trans.TransPeekNamedPipeResponse;
using Smb2IoctlRequest = jcifs.@internal.smb2.ioctl.Smb2IoctlRequest;
using Smb2IoctlResponse = jcifs.@internal.smb2.ioctl.Smb2IoctlResponse;
using SrvPipePeekResponse = jcifs.@internal.smb2.ioctl.SrvPipePeekResponse;

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
	public class SmbPipeInputStream : SmbFileInputStream {

		private SmbPipeHandleImpl handle;


		/// <param name="handle"> </param>
		/// <param name="th"> </param>
		/// <exception cref="SmbException"> </exception>
		/// throws jcifs.CIFSException
		internal SmbPipeInputStream(SmbPipeHandleImpl handle, SmbTreeHandleImpl th) : base((SmbFile)handle.getPipe(), th, null) {
			this.handle = handle;
		}


		/// throws jcifs.CIFSException
		 internal virtual SmbTreeHandleImpl ensureTreeConnected() {
			lock (this) {
				return (SmbTreeHandleImpl)this.handle.ensureTreeConnected();
			}
		}


		/// throws jcifs.CIFSException
		 internal override SmbFileHandleImpl ensureOpen() {
			lock (this) {
				return (SmbFileHandleImpl)this.handle.ensureOpen();
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
		public override int available() {
			try {
					using (SmbFileHandleImpl fd = (SmbFileHandleImpl)this.handle.ensureOpen())
					using (	SmbTreeHandleImpl th = (SmbTreeHandleImpl)fd.getTree()) {
					if (th.isSMB2()) {
						Smb2IoctlRequest req1 = new Smb2IoctlRequest(th.getConfig(), Smb2IoctlRequest.FSCTL_PIPE_PEEK, fd.getFileId());
						req1.setMaxOutputResponse(16);
						req1.setFlags(Smb2IoctlRequest.SMB2_O_IOCTL_IS_FSCTL);
						Smb2IoctlResponse resp1 = th.send(req1, RequestParam.NO_RETRY);
						return ((SrvPipePeekResponse)resp1.getOutputData()).getReadDataAvailable();
					}
					TransPeekNamedPipe req = new TransPeekNamedPipe(th.getConfig(), this.handle.getUncPath(), fd.getFid());
					TransPeekNamedPipeResponse resp = new TransPeekNamedPipeResponse(th.getConfig());
					th.send(req, resp, RequestParam.NO_RETRY);
					if (resp.getStatus() == TransPeekNamedPipeResponse.STATUS_DISCONNECTED || resp.getStatus() == TransPeekNamedPipeResponse.STATUS_SERVER_END_CLOSED) {
						fd.markClosed();
						return 0;
					}
					return resp.getAvailable();
					}
			}
			catch (SmbException se) {
				throw seToIoe(se);
			}
		}


		public new void Dispose() {
			// ignore, the shared file descriptor is closed by the pipe handle
		}
	}

}