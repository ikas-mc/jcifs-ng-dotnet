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
	public class ValidateNegotiateInfoRequest : Encodable {

		private int capabilities;
		private byte[] clientGuid;
		private int securityMode;
		private int[] dialects;


		/// <param name="capabilities"> </param>
		/// <param name="clientGuid"> </param>
		/// <param name="securityMode"> </param>
		/// <param name="dialects">
		///  </param>
		public ValidateNegotiateInfoRequest(int capabilities, byte[] clientGuid, int securityMode, int[] dialects) {
			this.capabilities = capabilities;
			this.clientGuid = clientGuid;
			this.securityMode = securityMode;
			this.dialects = dialects;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Encodable#encode(byte[], int) </seealso>
		public virtual int encode(byte[] dst, int dstIndex) {
			int start = dstIndex;

			SMBUtil.writeInt4(this.capabilities, dst, dstIndex);
			dstIndex += 4;

			Array.Copy(this.clientGuid, 0, dst, dstIndex, 16);
			dstIndex += 16;

			SMBUtil.writeInt2(this.securityMode, dst, dstIndex);
			dstIndex += 2;
			SMBUtil.writeInt2(this.dialects.Length, dst, dstIndex);
			dstIndex += 2;

			foreach (int dialect in this.dialects) {
				SMBUtil.writeInt2(dialect, dst, dstIndex);
				dstIndex += 2;
			}

			return dstIndex - start;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Encodable#size() </seealso>
		public virtual int size() {
			return 24 + 2 * this.dialects.Length;
		}

	}

}