using jcifs;

using sid_t = jcifs.dcerpc.rpc.sid_t;
using SID = jcifs.smb.SID;

/*
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



	internal class LsarSidArrayX : lsarpc.LsarSidArray {

		internal LsarSidArrayX(jcifs.SID[] sids) {
			this.num_sids = sids.Length;
			this.sids = new lsarpc.LsarSidPtr[sids.Length];
			for (int si = 0; si < sids.Length; si++) {
				this.sids[si] = new lsarpc.LsarSidPtr();
				this.sids[si].sid = sids[si].unwrap<sid_t>(typeof(sid_t));
			}
		}


		internal LsarSidArrayX(jcifs.smb.SID[] sids) {
			this.num_sids = sids.Length;
			this.sids = new lsarpc.LsarSidPtr[sids.Length];
			for (int si = 0; si < sids.Length; si++) {
				this.sids[si] = new lsarpc.LsarSidPtr();
				this.sids[si].sid = sids[si];
			}
		}
	}

}