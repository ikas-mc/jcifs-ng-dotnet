using System;
using System.Collections.Generic;
using Logger = org.slf4j.Logger;
using LoggerFactory = org.slf4j.LoggerFactory;
using CIFSException = jcifs.CIFSException;
using jcifs;
using ResourceFilter = jcifs.ResourceFilter;
using SmbConstants = jcifs.SmbConstants;
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





	internal class ShareEnumIterator : CloseableIterator<SmbResource> {

		private static readonly Logger log = LoggerFactory.getLogger(typeof(ShareEnumIterator));

		private readonly IEnumerator<FileEntry> @delegate;
		private readonly ResourceFilter filter;
		private readonly SmbResource parent;
		private SmbResource nextField;


		/// <param name="parent"> </param>
		/// <param name="delegate"> </param>
		/// <param name="filter">
		///  </param>
		public ShareEnumIterator(SmbResource parent, IEnumerator<FileEntry> @delegate, ResourceFilter filter) {
			this.parent = parent;
			this.@delegate = @delegate;
			this.filter = filter;
			this.nextField = advance();
		}


		/// <returns> next element </returns>
		private SmbResource advance() {
			while (this.@delegate.MoveNext()) {
				FileEntry n = this.@delegate.Current;
				if (this.filter == null) {
					try {
						return adapt(n);
					}
					//TODO 
					catch (UriFormatException e) {
						log.error("Failed to create child URL", e);
						continue;
					}
				}
				try {
						using (SmbResource nr = adapt(n)) {
						if (!this.filter.accept(nr)) {
							continue;
						}
						return nr;
						}
				}
				catch (CIFSException e) {
					log.error("Failed to apply filter", e);
					continue;
				}
				//TODO 
				catch (UriFormatException e) {
					log.error("Failed to create child URL", e);
					continue;
				}
			}
			return null;
		}


		/// throws java.net.MalformedURLException
		private SmbResource adapt(FileEntry e) {
			return new SmbFile(this.parent, e.getName(), false, e.getType(), SmbConstants.ATTR_READONLY | SmbConstants.ATTR_DIRECTORY, 0L, 0L, 0L, 0L);
		}


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
		/// <seealso cref= jcifs.CloseableIterator#Dispose() </seealso>
		public virtual void Dispose() {
			// nothing to clean up
			this.nextField = null;
		}

		public  void remove() {
			throw new System.NotSupportedException("remove");
		}
	}
}