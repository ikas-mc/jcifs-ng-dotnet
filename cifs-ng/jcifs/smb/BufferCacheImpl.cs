using System;
using cifs_ng.lib.ext;
using BufferCache = jcifs.BufferCache;
using Configuration = jcifs.Configuration;

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
	/// Cache for reusable byte buffers
	/// 
	/// @internal
	/// </summary>
	public class BufferCacheImpl : BufferCache {

		private readonly object[] cache;
		private readonly int bufferSize;
		private int freeBuffers = 0;


		/// 
		/// <param name="cfg"> </param>
		public BufferCacheImpl(Configuration cfg) : this(cfg.getBufferCacheSize(), cfg.getMaximumBufferSize()) {
		}


		/// <param name="maxBuffers"> </param>
		/// <param name="maxSize">
		///  </param>
		public BufferCacheImpl(int maxBuffers, int maxSize) {
			this.cache = new object[maxBuffers];
			this.bufferSize = maxSize;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.BufferCache#getBuffer() </seealso>
		public virtual byte[] getBuffer() {
			lock (this.cache) {
				byte[] buf;

				if (this.freeBuffers > 0) {
					for (int i = 0; i < this.cache.Length; i++) {
						if (this.cache[i] != null) {
							buf = (byte[]) this.cache[i];
							this.cache[i] = null;
							this.freeBuffers--;
							return buf;
						}
					}
				}
				return new byte[this.bufferSize];
			}
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.BufferCache#releaseBuffer(byte[]) </seealso>
		public virtual void releaseBuffer(byte[] buf) {
			if (buf == null) {
				return;
			}
			// better safe than sorry: prevent leaks if there is some out of bound access
			Array.Clear(buf,0,buf.Length);
			lock (this.cache) {
				if (this.freeBuffers < this.cache.Length) {
					for (int i = 0; i < this.cache.Length; i++) {
						if (this.cache[i] == null) {
							this.cache[i] = buf;
							this.freeBuffers++;
							return;
						}
					}
				}
			}
		}
	}

}