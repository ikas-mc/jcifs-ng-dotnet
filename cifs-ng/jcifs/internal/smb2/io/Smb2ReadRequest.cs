using System;
using CIFSContext = jcifs.CIFSContext;
using Configuration = jcifs.Configuration;
using RequestWithFileId = jcifs.@internal.smb2.RequestWithFileId;
using jcifs.@internal.smb2;
using Smb2Constants = jcifs.@internal.smb2.Smb2Constants;
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
	public class Smb2ReadRequest : ServerMessageBlock2Request<Smb2ReadResponse>, RequestWithFileId {

		/// 
		public static byte SMB2_READFLAG_READ_UNBUFFERED = 0x1;
		/// 
		public static int SMB2_CHANNEL_NONE = 0x0;
		/// 
		public static int SMB2_CHANNEL_RDMA_V1 = 0x1;
		/// 
		public static int SMB2_CHANNEL_RDMA_V1_INVALIDATE = 0x2;

		private byte[] fileId;
		private readonly byte[] outputBuffer;
		private readonly int outputBufferOffset;
		private byte padding;
		private byte readFlags;
		private int readLength;
		private long offset;
		private int minimumCount;
		private int channel;
		private int remainingBytes;


		/// <param name="config"> </param>
		/// <param name="fileId"> </param>
		/// <param name="outputBuffer"> </param>
		/// <param name="outputBufferOffset"> </param>
		public Smb2ReadRequest(Configuration config, byte[] fileId, byte[] outputBuffer, int outputBufferOffset) : base(config, SMB2_READ) {
			this.fileId = fileId;
			this.outputBuffer = outputBuffer;
			this.outputBufferOffset = outputBufferOffset;
		}


		protected  override Smb2ReadResponse createResponse(CIFSContext tc, ServerMessageBlock2Request<Smb2ReadResponse> req) {
			return new Smb2ReadResponse(tc.getConfig(), this.outputBuffer, this.outputBufferOffset);
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb2.RequestWithFileId#setFileId(byte[]) </seealso>
		public virtual void setFileId(byte[] fileId) {
			this.fileId = fileId;
		}


		/// <param name="padding">
		///            the padding to set </param>
		public virtual void setPadding(byte padding) {
			this.padding = padding;
		}


		/// <param name="readFlags">
		///            the readFlags to set </param>
		public virtual void setReadFlags(byte readFlags) {
			this.readFlags = readFlags;
		}


		/// <param name="readLength">
		///            the readLength to set </param>
		public virtual void setReadLength(int readLength) {
			this.readLength = readLength;
		}


		/// <param name="offset">
		///            the offset to set </param>
		public virtual void setOffset(long offset) {
			this.offset = offset;
		}


		/// <param name="minimumCount">
		///            the minimumCount to set </param>
		public virtual void setMinimumCount(int minimumCount) {
			this.minimumCount = minimumCount;
		}


		/// <param name="remainingBytes">
		///            the remainingBytes to set </param>
		public virtual void setRemainingBytes(int remainingBytes) {
			this.remainingBytes = remainingBytes;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.CommonServerMessageBlockRequest#size() </seealso>
		public override int size() {
			return size8(Smb2Constants.SMB2_HEADER_LENGTH + 49);
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb2.ServerMessageBlock2#writeBytesWireFormat(byte[], int) </seealso>
		protected  override int writeBytesWireFormat(byte[] dst, int dstIndex) {
			int start = dstIndex;
			SMBUtil.writeInt2(49, dst, dstIndex);
			dst[dstIndex + 2] = this.padding;
			dst[dstIndex + 3] = this.readFlags;
			dstIndex += 4;
			SMBUtil.writeInt4(this.readLength, dst, dstIndex);
			dstIndex += 4;
			SMBUtil.writeInt8(this.offset, dst, dstIndex);
			dstIndex += 8;
			Array.Copy(this.fileId, 0, dst, dstIndex, 16);
			dstIndex += 16;
			SMBUtil.writeInt4(this.minimumCount, dst, dstIndex);
			dstIndex += 4;
			SMBUtil.writeInt4(this.channel, dst, dstIndex);
			dstIndex += 4;
			SMBUtil.writeInt4(this.remainingBytes, dst, dstIndex);
			dstIndex += 4;

			// ReadChannelInfo
			SMBUtil.writeInt2(0, dst, dstIndex);
			SMBUtil.writeInt2(0, dst, dstIndex + 2);
			dstIndex += 4;

			// one byte in buffer must be zero
			dst[dstIndex] = 0;
			dstIndex += 1;

			return dstIndex - start;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb2.ServerMessageBlock2#readBytesWireFormat(byte[], int) </seealso>
		protected  override int readBytesWireFormat(byte[] buffer, int bufferIndex) {
			return 0;
		}

	}

}