using System;
using Logger = org.slf4j.Logger;
using LoggerFactory = org.slf4j.LoggerFactory;
using CIFSException = jcifs.CIFSException;
using jcifs;
using ResourceNameFilter = jcifs.ResourceNameFilter;
using SmbResource = jcifs.SmbResource;

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
	public abstract class DirFileEntryEnumIteratorBase : CloseableIterator<FileEntry> {

		private static readonly Logger log = LoggerFactory.getLogger(typeof(DirFileEntryEnumIteratorBase));

		private readonly SmbTreeHandleImpl treeHandle;
		private readonly ResourceNameFilter nameFilter;
		private readonly SmbResource parent;
		private readonly string wildcard;
		private readonly int searchAttributes;
		private FileEntry nextField;
		private int ridx;

		private bool closed = false;


		/// <param name="th"> </param>
		/// <param name="parent"> </param>
		/// <param name="wildcard"> </param>
		/// <param name="filter"> </param>
		/// <param name="searchAttributes"> </param>
		/// <exception cref="CIFSException">
		///  </exception>
		/// throws jcifs.CIFSException
		public DirFileEntryEnumIteratorBase(SmbTreeHandleImpl th, SmbResource parent, string wildcard, ResourceNameFilter filter, int searchAttributes) {
			this.parent = parent;
			this.wildcard = wildcard;
			this.nameFilter = filter;
			this.searchAttributes = searchAttributes;

			this.treeHandle = th.acquire();
			try {
				this.nextField = open();
				if (this.nextField == null) {
					doClose();
				}
			}
			catch (Exception e) {
				doClose();
				throw e;
			}

		}


		/// <returns> the treeHandle </returns>
		public SmbTreeHandleImpl getTreeHandle() {
			return this.treeHandle;
		}


		/// <returns> the searchAttributes </returns>
		public int getSearchAttributes() {
			return this.searchAttributes;
		}


		/// <returns> the wildcard </returns>
		public string getWildcard() {
			return this.wildcard;
		}


		/// <returns> the parent </returns>
		public SmbResource getParent() {
			return this.parent;
		}


		private bool filter(FileEntry fe) {
			string name = fe.getName();
			if (name.Length < 3) {
				int h = name.GetHashCode();
				if (h == SmbFile.HASH_DOT || h == SmbFile.HASH_DOT_DOT) {
					if (name.Equals(".") || name.Equals("..")) {
						return false;
					}
				}
			}
			if (this.nameFilter == null) {
				return true;
			}
			try {
				if (!this.nameFilter.accept(this.parent, name)) {
					return false;
				}
				return true;
			}
			catch (CIFSException e) {
				log.error("Failed to apply name filter", e);
				return false;
			}
		}


		/// throws jcifs.CIFSException
		protected internal FileEntry advance(bool last) {
			FileEntry[] results = getResults();
			while (this.ridx < results.Length) {
				FileEntry itm = results[this.ridx];
				this.ridx++;
				if (filter(itm)) {
					return itm;
				}
			}

			if (!last && !isDone()) {
				if (!fetchMore()) {
					doClose();
					return null;
				}
				this.ridx = 0;
				return advance(true);
			}
			return null;
		}


		/// throws jcifs.CIFSException;
		protected internal abstract FileEntry open();


		protected internal abstract bool isDone();


		/// throws jcifs.CIFSException;
		protected internal abstract bool fetchMore();


		protected internal abstract FileEntry[] getResults();


		/// 
		/// throws jcifs.CIFSException
		protected internal virtual void doClose() {
			lock (this) {
				// otherwise already closed
				if (!this.closed) {
					this.closed = true;
					try {
						doCloseInternal();
					}
					finally {
						this.nextField = null;
						this.treeHandle.release();
					}
				}
			}
		}


		/// 
		/// throws jcifs.CIFSException;
		protected internal abstract void doCloseInternal();


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= java.util.Iterator#hasNext() </seealso>
		public  bool hasNext() {
			return this.nextField != null;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= java.util.Iterator#next() </seealso>
		public  FileEntry next() {
			FileEntry n = this.nextField;
			try {
				FileEntry ne = advance(false);
				if (ne == null) {
					doClose();
					return n;
				}
				this.nextField = ne;
			}
			catch (CIFSException e) {
				log.warn("Enumeration failed", e);
				this.nextField = null;
				try {
					doClose();
				}
				catch (CIFSException) {
					log.debug("Failed to close enum", e);
				}
			}
			return n;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= java.lang.AutoCloseable#Dispose() </seealso>
		/// throws jcifs.CIFSException
		public virtual void Dispose() {
			if (this.nextField != null) {
				doClose();
			}
		}

		public  void remove() {
			throw new System.NotSupportedException("remove");
		}
	}

}