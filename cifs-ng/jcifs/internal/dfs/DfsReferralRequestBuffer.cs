using System;
using System.Text;
using cifs_ng.lib.ext;
using Encodable = jcifs.Encodable;
using SMBUtil = jcifs.@internal.util.SMBUtil;

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
namespace jcifs.@internal.dfs {



	/// <summary>
	/// @author mbechler
	/// 
	/// </summary>
	public class DfsReferralRequestBuffer : Encodable {

		private readonly int maxReferralLevel;
		private readonly string path;


		/// <param name="filename"> </param>
		/// <param name="maxReferralLevel"> </param>
		public DfsReferralRequestBuffer(string filename, int maxReferralLevel) {
			this.path = filename;
			this.maxReferralLevel = maxReferralLevel;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Encodable#size() </seealso>
		public virtual int size() {
			return 4 + 2 * this.path.Length;
		}


		public virtual int encode(byte[] dst, int dstIndex) {
			int start = dstIndex;
			SMBUtil.writeInt2(this.maxReferralLevel, dst, dstIndex);
			dstIndex += 2;
			byte[] pathBytes = this.path.getBytes(Encoding.Unicode);
			Array.Copy(pathBytes, 0, dst, dstIndex, pathBytes.Length);
			dstIndex += pathBytes.Length;
			SMBUtil.writeInt2(0, dst, dstIndex);
			dstIndex += 2; // null terminator
			return dstIndex - start;
		}
	}

}