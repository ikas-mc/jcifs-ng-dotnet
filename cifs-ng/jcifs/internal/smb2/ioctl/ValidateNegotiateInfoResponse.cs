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
	public class ValidateNegotiateInfoResponse : Decodable {

		private int capabilities;
		private byte[] serverGuid = new byte[16];
		private int securityMode;
		private int dialect;


		/// <returns> the capabilities </returns>
		public virtual int getCapabilities() {
			return this.capabilities;
		}


		/// <returns> the serverGuid </returns>
		public virtual byte[] getServerGuid() {
			return this.serverGuid;
		}


		/// <returns> the securityMode </returns>
		public virtual int getSecurityMode() {
			return this.securityMode;
		}


		/// <returns> the dialect </returns>
		public virtual int getDialect() {
			return this.dialect;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Decodable#decode(byte[], int, int) </seealso>
		/// throws jcifs.internal.SMBProtocolDecodingException
		public virtual int decode(byte[] buffer, int bufferIndex, int len) {
			int start = bufferIndex;

			this.capabilities = SMBUtil.readInt4(buffer, bufferIndex);
			bufferIndex += 4;

			Array.Copy(buffer, bufferIndex, this.serverGuid, 0, 16);
			bufferIndex += 16;

			this.securityMode = SMBUtil.readInt2(buffer, bufferIndex);
			this.dialect = SMBUtil.readInt2(buffer, bufferIndex + 2);
			bufferIndex += 4;

			return bufferIndex - start;
		}

	}

}