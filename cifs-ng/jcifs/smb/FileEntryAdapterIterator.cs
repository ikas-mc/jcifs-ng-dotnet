using System;
using Logger = org.slf4j.Logger;
using LoggerFactory = org.slf4j.LoggerFactory;
using CIFSException = jcifs.CIFSException;
using jcifs;
using ResourceFilter = jcifs.ResourceFilter;
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




	internal abstract class FileEntryAdapterIterator : CloseableIterator<SmbResource> {

		private static readonly Logger log = LoggerFactory.getLogger(typeof(FileEntryAdapterIterator));

		private readonly CloseableIterator<FileEntry> @delegate;
		private readonly ResourceFilter filter;
		private readonly SmbResource parent;
		private SmbResource nextField;


		/// <param name="parent"> </param>
		/// <param name="delegate"> </param>
		/// <param name="filter">
		///  </param>
		public FileEntryAdapterIterator(SmbResource parent, CloseableIterator<FileEntry> @delegate, ResourceFilter filter) {
			this.parent = parent;
			this.@delegate = @delegate;
			this.filter = filter;
			this.nextField = advance();
		}


		/// <returns> the parent </returns>
		protected internal SmbResource getParent() {
			return this.parent;
		}


		/// <summary>
		/// @return
		/// 
		/// </summary>
		private SmbResource advance() {
			while (this.@delegate.hasNext()) {
				FileEntry fe = this.@delegate.next();
				if (this.filter == null) {
					try {
						return adapt(fe);
					}
					catch (UriFormatException e) {
						log.error("Failed to create child URL", e);
						continue;
					}
				}

				try {
						using (SmbResource r = adapt(fe)) {
						if (this.filter.accept(r)) {
							return r;
						}
						}
				}
				catch (UriFormatException e) {
					log.error("Failed to create child URL", e);
					continue;
				}
				catch (CIFSException e) {
					log.error("Filter failed", e);
					continue;
				}
			}
			return null;
		}


		/// throws java.net.MalformedURLException;
		protected internal abstract SmbResource adapt(FileEntry e);


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
		public  SmbResource next() {
			SmbResource n = this.nextField;
			this.nextField = advance();
			return n;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <exception cref="CIFSException">
		/// </exception>
		/// <seealso cref= java.lang.AutoCloseable#Dispose() </seealso>
		/// throws jcifs.CIFSException
		public virtual void Dispose() {
			this.@delegate.Dispose();
		}

		public  void remove() {
			this.@delegate.remove();
		}
	}
}