/* jcifs msrpc client library in Java
 * Copyright (C) 2007  "Michael B. Allen" <jcifs at samba dot org>
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

namespace jcifs.dcerpc.msrpc {

	public class MsrpcSamrOpenAlias : samr.SamrOpenAlias {

		public MsrpcSamrOpenAlias(SamrDomainHandle handle, int access, int rid, SamrAliasHandle aliasHandle) : base(handle, access, rid, aliasHandle) {
			this.ptype = 0;
			this.flags = DcerpcConstants.DCERPC_FIRST_FRAG | DcerpcConstants.DCERPC_LAST_FRAG;
		}
	}

}