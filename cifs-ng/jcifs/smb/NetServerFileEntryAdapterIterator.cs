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



	internal class NetServerFileEntryAdapterIterator : FileEntryAdapterIterator {

		/// <param name="parent"> </param>
		/// <param name="delegate"> </param>
		/// <param name="filter"> </param>
		public NetServerFileEntryAdapterIterator(SmbResource parent, NetServerEnumIterator @delegate, ResourceFilter filter) : base(parent, @delegate, filter) {
		}


		/// <param name="fe">
		/// @return </param>
		/// <exception cref="MalformedURLException"> </exception>
		/// throws java.net.MalformedURLException
		protected internal override SmbResource adapt(FileEntry e) {
			return new SmbFile(getParent(), e.getName(), false, e.getType(), SmbConstants.ATTR_READONLY | SmbConstants.ATTR_DIRECTORY, 0L, 0L, 0L, 0L);
		}
	}
}