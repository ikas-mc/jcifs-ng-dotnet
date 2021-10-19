using cifs_ng.lib;
using DcerpcHandle = jcifs.dcerpc.DcerpcHandle;
using rpc = jcifs.dcerpc.rpc;
using SmbException = jcifs.smb.SmbException;

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



	public class LsaPolicyHandle : rpc.policy_handle, AutoCloseable {

		private readonly DcerpcHandle handle;
		private bool opened;


		/// throws java.io.IOException
		public LsaPolicyHandle(DcerpcHandle handle, string server, int access) {
			this.handle = handle;
			if ((server== null)) {
				server = "\\\\";
			}
			MsrpcLsarOpenPolicy2 rpc = new MsrpcLsarOpenPolicy2(server, access, this);
			handle.sendrecv(rpc);
			if (rpc.retval != 0) {
				throw new SmbException(rpc.retval, false);
			}
			this.opened = true;
		}


		/// throws java.io.IOException
		public  void Dispose() {
			lock (this) {
				if (this.opened) {
					this.opened = false;
					MsrpcLsarClose rpc = new MsrpcLsarClose(this);
					this.handle.sendrecv(rpc);
					if (rpc.retval != 0) {
						throw new SmbException(rpc.retval, false);
					}
				}
			}
		}
	}

}