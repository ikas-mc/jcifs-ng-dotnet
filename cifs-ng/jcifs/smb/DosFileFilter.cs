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
	public class DosFileFilter : SmbFileFilter {

		protected internal string wildcard;
		protected internal int attributes;


		/// <summary>
		/// This filter can be considerably more efficient than other file filters
		/// as the specified wildcard and attributes are passed to the server for
		/// filtering there (although attributes are largely ignored by servers
		/// they are filtered locally by the default accept method).
		/// </summary>
		/// <param name="wildcard"> </param>
		/// <param name="attributes"> </param>
		public DosFileFilter(string wildcard, int attributes) {
			this.wildcard = wildcard;
			this.attributes = attributes;
		}


		/// <summary>
		/// This returns true if the file's attributes contain any of the attributes
		/// specified for this filter. The wildcard has no influence on this
		/// method as the server should have performed that filtering already. The
		/// attributes are asserted here only because server file systems may not
		/// support filtering by all attributes (e.g. even though ATTR_DIRECTORY was
		/// specified the server may still return objects that are not directories).
		/// </summary>
		/// throws SmbException
		public virtual bool accept(SmbFile file) {
			return (file.getAttributes() & this.attributes) != 0;
		}
	}

}