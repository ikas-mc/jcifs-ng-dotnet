using DfsReferralData = jcifs.DfsReferralData;

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
	/// <summary>
	/// @author mbechler
	/// 
	/// @internal
	/// </summary>
	public class DfsReferral : SmbException {

		/// 
		private const long serialVersionUID = 1486630733410281686L;

		private readonly DfsReferralData data;


		/// <param name="dr"> </param>
		public DfsReferral(DfsReferralData data) {
			this.data = data;
		}


		public virtual DfsReferralData getData() {
			return this.data;
		}


		public override string ToString() {
			return this.data.ToString();
		}
	}

}