using System;
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
namespace jcifs.@internal.smb2.ioctl {



	/// <summary>
	/// @author mbechler
	/// 
	/// </summary>
	public class SrvCopychunkCopy : Encodable {

		private readonly byte[] sourceKey;
		private readonly SrvCopychunk[] chunks;


		/// <param name="sourceKey"> </param>
		/// <param name="chunks">
		///  </param>
		public SrvCopychunkCopy(byte[] sourceKey, params SrvCopychunk[] chunks) {
			this.sourceKey = sourceKey;
			this.chunks = chunks;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Encodable#encode(byte[], int) </seealso>
		public virtual int encode(byte[] dst, int dstIndex) {
			int start = dstIndex;

			Array.Copy(this.sourceKey, 0, dst, dstIndex, 24);
			dstIndex += 24;

			SMBUtil.writeInt4(this.chunks.Length, dst, dstIndex);
			dstIndex += 4;

			dstIndex += 4; // Reserved

			foreach (SrvCopychunk chk in this.chunks) {
				dstIndex += chk.encode(dst, dstIndex);
			}
			return dstIndex - start;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Encodable#size() </seealso>
		public virtual int size() {
			return 32 + this.chunks.Length * 24;
		}

	}

}