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
	/// @author svella
	/// 
	/// </summary>
	public class SrvPipePeekResponse : Decodable {

		// see https://msdn.microsoft.com/en-us/library/dd414577.aspx

		private int namedPipeState;
		private int readDataAvailable;
		private int numberOfMessages;
		private int messageLength;
		private byte[] data;


		/// <returns> the chunkBytesWritten </returns>
		public virtual int getNamedPipeState() {
			return this.namedPipeState;
		}


		/// <returns> the chunksWritten </returns>
		public virtual int getReadDataAvailable() {
			return this.readDataAvailable;
		}


		/// <returns> the totalBytesWritten </returns>
		public virtual int getNumberOfMessages() {
			return this.numberOfMessages;
		}


		/// <returns> the totalBytesWritten </returns>
		public virtual int getMessageLength() {
			return this.messageLength;
		}


		/// <returns> the totalBytesWritten </returns>
		public virtual byte[] getData() {
			return this.data;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= Decodable#decode(byte[], int, int) </seealso>
		/// throws jcifs.internal.SMBProtocolDecodingException
		public virtual int decode(byte[] buffer, int bufferIndex, int len) {
			int start = bufferIndex;
			this.namedPipeState = SMBUtil.readInt4(buffer, bufferIndex);
			bufferIndex += 4;
			this.readDataAvailable = SMBUtil.readInt4(buffer, bufferIndex);
			bufferIndex += 4;
			this.numberOfMessages = SMBUtil.readInt4(buffer, bufferIndex);
			bufferIndex += 4;
			this.messageLength = SMBUtil.readInt4(buffer, bufferIndex);
			bufferIndex += 4;
			this.data = new byte[len - 16];
			if (this.data.Length > 0) {
				Array.Copy(buffer, bufferIndex, this.data, 0, this.data.Length);
			}
			return bufferIndex - start;
		}

	}

}