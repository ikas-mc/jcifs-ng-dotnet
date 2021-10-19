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
namespace jcifs.@internal.fscc {



	/// <summary>
	/// @author mbechler
	/// 
	/// </summary>
	public class FsctlPipeWaitRequest : Encodable {

		private readonly byte[] nameBytes;
		private readonly long timeout;
		private readonly bool timeoutSpecified;


		/// <param name="name">
		///  </param>
		public FsctlPipeWaitRequest(string name) {
			this.nameBytes = name.getBytes(Encoding.Unicode);
			this.timeoutSpecified = false;
			this.timeout = 0;
		}


		/// <param name="name"> </param>
		/// <param name="timeout">
		///  </param>
		public FsctlPipeWaitRequest(string name, long timeout) {
			this.nameBytes = name.getBytes(Encoding.Unicode);
			this.timeoutSpecified = true;
			this.timeout = timeout;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Encodable#encode(byte[], int) </seealso>
		public virtual int encode(byte[] dst, int dstIndex) {
			int start = dstIndex;
			SMBUtil.writeInt8(this.timeout, dst, dstIndex);
			dstIndex += 8;
			SMBUtil.writeInt4(this.nameBytes.Length, dst, dstIndex);
			dstIndex += 4;

			dst[dstIndex] = (byte)(this.timeoutSpecified ? 0x1 : 0x0);
			dstIndex++;
			dstIndex++; // Padding

			Array.Copy(this.nameBytes, 0, dst, dstIndex, this.nameBytes.Length);
			dstIndex += this.nameBytes.Length;

			return dstIndex - start;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Encodable#size() </seealso>
		public virtual int size() {
			return 14 + this.nameBytes.Length;
		}

	}

}