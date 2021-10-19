using System;
using Configuration = jcifs.Configuration;
using SMBProtocolDecodingException = jcifs.@internal.SMBProtocolDecodingException;
using ServerMessageBlock2Response = jcifs.@internal.smb2.ServerMessageBlock2Response;
using Smb2Constants = jcifs.@internal.smb2.Smb2Constants;
using SMBUtil = jcifs.@internal.util.SMBUtil;
using NtStatus = jcifs.smb.NtStatus;

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
	public class Smb2ReadResponse : ServerMessageBlock2Response {

		/// 
		public static readonly int OVERHEAD = Smb2Constants.SMB2_HEADER_LENGTH + 16;

		private int dataRemaining;
		private int dataLength;
		private byte[] outputBuffer;
		private int outputBufferOffset;


		/// <param name="config"> </param>
		/// <param name="outputBufferOffset"> </param>
		/// <param name="outputBuffer"> </param>
		public Smb2ReadResponse(Configuration config, byte[] outputBuffer, int outputBufferOffset) : base(config) {
			this.outputBuffer = outputBuffer;
			this.outputBufferOffset = outputBufferOffset;
		}


		/// <returns> the dataLength </returns>
		public virtual int getDataLength() {
			return this.dataLength;
		}


		/// <returns> the dataRemaining </returns>
		public virtual int getDataRemaining() {
			return this.dataRemaining;
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
			if (structureSize == 9) {
				return this.readErrorResponse(buffer, bufferIndex);
			}
			else if (structureSize != 17) {
				throw new SMBProtocolDecodingException("Expected structureSize = 17");
			}

			short dataOffset = buffer[bufferIndex + 2];
			bufferIndex += 4;
			this.dataLength = SMBUtil.readInt4(buffer, bufferIndex);
			bufferIndex += 4;
			this.dataRemaining = SMBUtil.readInt4(buffer, bufferIndex);
			bufferIndex += 4;
			bufferIndex += 4; // Reserved2

			int dataStart = getHeaderStart() + dataOffset;

			if (this.dataLength + this.outputBufferOffset > this.outputBuffer.Length) {
				throw new SMBProtocolDecodingException("Buffer to small for read response");
			}
			Array.Copy(buffer, dataStart, this.outputBuffer, this.outputBufferOffset, this.dataLength);
			bufferIndex = Math.Max(bufferIndex, dataStart + this.dataLength);
			return bufferIndex - start;
		}

		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb2.ServerMessageBlock2#isErrorResponseStatus() </seealso>
		protected  override bool isErrorResponseStatus() {
			return getStatus() != NtStatus.NT_STATUS_BUFFER_OVERFLOW && base.isErrorResponseStatus();
		}


	}

}