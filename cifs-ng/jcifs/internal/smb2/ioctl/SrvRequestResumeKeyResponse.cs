using System;
using Decodable = jcifs.Decodable;
using SMBProtocolDecodingException = jcifs.@internal.SMBProtocolDecodingException;
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
	public class SrvRequestResumeKeyResponse : Decodable {

		private byte[] resumeKey;


		/// <returns> the resumeKey </returns>
		public virtual byte[] getResumeKey() {
			return this.resumeKey;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Decodable#decode(byte[], int, int) </seealso>
		/// throws jcifs.internal.SMBProtocolDecodingException
		public virtual int decode(byte[] buffer, int bufferIndex, int len) {
			int start = bufferIndex;
			if (len < 24) {
				throw new SMBProtocolDecodingException("Invalid resume key");
			}

			this.resumeKey = new byte[24];
			Array.Copy(buffer, bufferIndex, this.resumeKey, 0, 24);
			bufferIndex += 24;

			SMBUtil.readInt4(buffer, bufferIndex); // contextLength - reserved
			bufferIndex += 4;

			return bufferIndex - start;
		}

	}

}