using System.Diagnostics;
using System.Threading;
using System.Linq;
using cifs_ng.lib.ext;
using cifs_ng.lib.threading;
using Logger = org.slf4j.Logger;
using LoggerFactory = org.slf4j.LoggerFactory;
using CIFSException = jcifs.CIFSException;
using Configuration = jcifs.Configuration;
using SmbFileHandle = jcifs.SmbFileHandle;
using SmbComBlankResponse = jcifs.@internal.smb1.com.SmbComBlankResponse;
using SmbComClose = jcifs.@internal.smb1.com.SmbComClose;
using Smb2CloseRequest = jcifs.@internal.smb2.create.Smb2CloseRequest;
using Hexdump = jcifs.util.Hexdump;

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
	internal class SmbFileHandleImpl : SmbFileHandle {

		private static readonly Logger log = LoggerFactory.getLogger(typeof(SmbFileHandleImpl));

		private readonly Configuration cfg;
		private readonly int fid;
		private readonly byte[] fileId;
		private bool open = true;
		private readonly long tree_num; // for checking whether the tree changed
		private SmbTreeHandleImpl tree;

		private readonly AtomicLong usageCount = new AtomicLong(1);
		private readonly int flags;
		private readonly int access;
		private readonly int attrs;
		private readonly int options;
		private readonly string unc;

		private readonly StackFrame[] creationBacktrace;

		private long initialSize;


		/// <param name="cfg"> </param>
		/// <param name="fid"> </param>
		/// <param name="tree"> </param>
		/// <param name="unc"> </param>
		/// <param name="options"> </param>
		/// <param name="attrs"> </param>
		/// <param name="access"> </param>
		/// <param name="flags"> </param>
		/// <param name="initialSize"> </param>
		public SmbFileHandleImpl(Configuration cfg, byte[] fid, SmbTreeHandleImpl tree, string unc, int flags, int access, int attrs, int options, long initialSize) {
			this.cfg = cfg;
			this.fileId = fid;
			this.initialSize = initialSize;
			this.fid = 0;
			this.unc = unc;
			this.flags = flags;
			this.access = access;
			this.attrs = attrs;
			this.options = options;
			this.tree = tree.acquire();
			this.tree_num = tree.getTreeId();

			if (cfg.isTraceResourceUsage()) {
				StackTrace st = new System.Diagnostics.StackTrace();
				this.creationBacktrace = st.GetFrames();
			}
			else {
				this.creationBacktrace = null;
			}
		}


		/// <param name="cfg"> </param>
		/// <param name="fid"> </param>
		/// <param name="tree"> </param>
		/// <param name="unc"> </param>
		/// <param name="options"> </param>
		/// <param name="attrs"> </param>
		/// <param name="access"> </param>
		/// <param name="flags"> </param>
		/// <param name="initialSize"> </param>
		public SmbFileHandleImpl(Configuration cfg, int fid, SmbTreeHandleImpl tree, string unc, int flags, int access, int attrs, int options, long initialSize) {
			this.cfg = cfg;
			this.fid = fid;
			this.initialSize = initialSize;
			this.fileId = null;
			this.unc = unc;
			this.flags = flags;
			this.access = access;
			this.attrs = attrs;
			this.options = options;
			this.tree = tree.acquire();
			this.tree_num = tree.getTreeId();

			if (cfg.isTraceResourceUsage()) {
				StackTrace st = new System.Diagnostics.StackTrace();
				this.creationBacktrace = st.GetFrames();
			}
			else {
				this.creationBacktrace = null;
			}
		}


		/// <returns> the fid </returns>
		/// <exception cref="SmbException"> </exception>
		/// throws SmbException
		public virtual int getFid() {
			if (!isValid()) {
				throw new SmbException("Descriptor is no longer valid");
			}
			return this.fid;
		}


		/// throws SmbException
		public virtual byte[] getFileId() {
			if (!isValid()) {
				throw new SmbException("Descriptor is no longer valid");
			}
			return this.fileId;
		}


		/// <returns> the initialSize </returns>
		public virtual long getInitialSize() {
			return this.initialSize;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.SmbFileHandle#getTree() </seealso>
		public virtual SmbTreeHandle getTree() {
			return this.tree.acquire();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.SmbFileHandle#isValid() </seealso>
		public virtual bool isValid() {
			return this.open && this.tree_num == this.tree.getTreeId() && this.tree.isConnected();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.SmbFileHandle#close(long) </seealso>
		/// throws jcifs.CIFSException
		public virtual void close(long lastWriteTime) {
			lock (this) {
				closeInternal(lastWriteTime, true);
			}
		}


		/// <param name="lastWriteTime"> </param>
		/// <exception cref="SmbException"> </exception>
		/// throws jcifs.CIFSException
		internal virtual void closeInternal(long lastWriteTime, bool @explicit) {
			SmbTreeHandleImpl t = this.tree;
			try {
				if (t != null && isValid()) {
					if (log.isDebugEnabled()) {
						log.debug("Closing file handle " + this);
					}

					if (t.isSMB2()) {
						Smb2CloseRequest req = new Smb2CloseRequest(this.cfg, this.fileId);
						t.send(req, RequestParam.NO_RETRY);
					}
					else {
						t.send(new SmbComClose(this.cfg, this.fid, lastWriteTime), new SmbComBlankResponse(this.cfg), RequestParam.NO_RETRY);
					}
				}
			}
			finally {
				this.open = false;
				if (t != null) {
					// release tree usage
					t.release();
				}
				this.tree = null;
			}
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.SmbFileHandle#Dispose() </seealso>
		/// throws jcifs.CIFSException
		public virtual void Dispose() {
			release();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <exception cref="SmbException">
		/// </exception>
		/// <seealso cref= jcifs.SmbFileHandle#release() </seealso>
		/// throws jcifs.CIFSException
		public virtual void release() {
			lock (this) {
				long usage = this.usageCount.DecrementValueAndReturn();
				if (usage == 0) {
					closeInternal(0L, false);
				}
				else if (log.isTraceEnabled()) {
					log.trace(string.Format("Release {0} ({1:D})", this, usage));
				}
			}
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= java.lang.Object#finalize() </seealso>
		/// throws Throwable
		~SmbFileHandleImpl() {
			if (this.usageCount.Value != 0 && this.open) {
				log.warn("File handle was not properly closed: " + this);
				if (this.creationBacktrace != null) {
					log.warn($"creationBacktrace={this.creationBacktrace?.joinToString()}");
				}
			}
		}


		/// <returns> a file handle with increased usage count </returns>
		public virtual SmbFileHandleImpl acquire() {
			long usage = this.usageCount.IncrementValueAndReturn();
			if (log.isTraceEnabled()) {
				log.trace(string.Format("Acquire {0} ({1:D})", this, usage));
			}
			return this;
		}


		/// 
		public virtual void markClosed() {
			this.open = false;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= java.lang.Object#toString() </seealso>
		public override string ToString()
		{
			var x = this.fileId != null ? Hexdump.toHexString(this.fileId) : this.fid.ToString();
			return ($"FileHandle { this.unc} [fid={x},tree={this.tree_num},flags={this.flags},access={this.access},attrs={this.attrs},options={this.options}]");
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= java.lang.Object#hashCode() </seealso>
		public override int GetHashCode() {
			if (this.fileId != null) {
				return (int)(RuntimeHelp.hashCode(this.fileId) + 3 * this.tree_num);
			}
			return (int)(this.fid + 3 * this.tree_num);
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= java.lang.Object#equals(java.lang.Object) </seealso>
		public override bool Equals(object obj) {
			if (!(obj is SmbFileHandleImpl)) {
				return false;
			}
			SmbFileHandleImpl o = (SmbFileHandleImpl) obj;

			if (this.fileId != null) {
				return this.fileId.SequenceEqual(o.fileId) && this.tree_num == o.tree_num;
			}
			return this.fid == o.fid && this.tree_num == o.tree_num;
		}

	}

}