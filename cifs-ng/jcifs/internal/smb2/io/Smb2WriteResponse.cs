using Configuration = jcifs.Configuration;
using SMBProtocolDecodingException = jcifs.@internal.SMBProtocolDecodingException;
using ServerMessageBlock2Response = jcifs.@internal.smb2.ServerMessageBlock2Response;
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
namespace jcifs.@internal.smb2.io {



	/// <summary>
	/// @author mbechler
	/// 
	/// </summary>
	public class Smb2WriteResponse : ServerMessageBlock2Response {

		private int count;
		private int remaining;


		/// <param name="config"> </param>
		public Smb2WriteResponse(Configuration config) : base(config) {
		}


		/// <returns> the count </returns>
		public int getCount() {
			return this.count;
		}


		/// <returns> the remaining </returns>
		public int getRemaining() {
			return this.remaining;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb2.ServerMessageBlock2#writeBytesWireFormat(byte[], int) </seealso>
		protected  override int writeBytesWireFormat(byte[] dst, int dstIndex) {
			return 0;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb2.ServerMessageBlock2#readBytesWireFormat(byte[], int) </seealso>
		/// throws jcifs.internal.SMBProtocolDecodingException
		protected  override int readBytesWireFormat(byte[] buffer, int bufferIndex) {
			int start = bufferIndex;
			int structureSize = SMBUtil.readInt2(buffer, bufferIndex);
			if (structureSize != 17) {
				throw new SMBProtocolDecodingException("Expected structureSize = 17");
			}
			bufferIndex += 4;

			this.count = SMBUtil.readInt4(buffer, bufferIndex);
			bufferIndex += 4;
			this.remaining = SMBUtil.readInt4(buffer, bufferIndex);
			bufferIndex += 4;
			bufferIndex += 4; // WriteChannelInfoOffset/WriteChannelInfoLength
			return bufferIndex - start;
		}

	}

}