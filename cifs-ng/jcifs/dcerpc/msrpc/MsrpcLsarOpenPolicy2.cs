/* jcifs msrpc client library in Java
 * Copyright (C) 2006  "Michael B. Allen" <jcifs at samba dot org>
 *                     "Eric Glass" <jcifs at samba dot org>
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

	public class MsrpcLsarOpenPolicy2 : lsarpc.LsarOpenPolicy2 {

		public MsrpcLsarOpenPolicy2(string server, int access, LsaPolicyHandle policyHandle) : base(server, new lsarpc.LsarObjectAttributes(), access, policyHandle) {
			this.object_attributes.length = 24;
			lsarpc.LsarQosInfo qos = new lsarpc.LsarQosInfo();
			qos.length = 12;
			qos.impersonation_level = 2;
			qos.context_mode = 1;
			qos.effective_only = 0;
			this.object_attributes.security_quality_of_service = qos;
			this.ptype = 0;
			this.flags = DcerpcConstants.DCERPC_FIRST_FRAG | DcerpcConstants.DCERPC_LAST_FRAG;
		}
	}

}