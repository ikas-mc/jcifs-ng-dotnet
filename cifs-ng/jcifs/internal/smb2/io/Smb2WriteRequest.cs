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
	public class Smb2WriteRequest : ServerMessageBlock2Request<Smb2WriteResponse>, RequestWithFileId {

		/// 
		public static readonly int OVERHEAD = Smb2Constants.SMB2_HEADER_LENGTH + 48;

		private byte[] data;
		private int dataOffset;
		private int dataLength;

		private byte[] fileId;
		private long offset;
		private int channel;
		private int remainingBytes;
		private int writeFlags;


		/// <param name="config"> </param>
		/// <param name="fileId"> </param>
		public Smb2WriteRequest(Configuration config, byte[] fileId) : base(config, SMB2_WRITE) {
			this.fileId = fileId;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb2.RequestWithFileId#setFileId(byte[]) </seealso>
		public virtual void setFileId(byte[] fileId) {
			this.fileId = fileId;
		}


		protected  override Smb2WriteResponse createResponse(CIFSContext tc, ServerMessageBlock2Request<Smb2WriteResponse> req) {
			return new Smb2WriteResponse(tc.getConfig());
		}


		/// <param name="data">
		///            the data to set </param>
		/// <param name="offset"> </param>
		/// <param name="length"> </param>
		public virtual void setData(byte[] data, int offset, int length) {
			this.data = data;
			this.dataOffset = offset;
			this.dataLength = length;
		}


		/// <param name="remainingBytes">
		///            the remainingBytes to set </param>
		public virtual void setRemainingBytes(int remainingBytes) {
			this.remainingBytes = remainingBytes;
		}


		/// <param name="writeFlags">
		///            the writeFlags to set </param>
		public virtual void setWriteFlags(int writeFlags) {
			this.writeFlags = writeFlags;
		}


		/// <param name="offset">
		///            the offset to set </param>
		public virtual void setOffset(long offset) {
			this.offset = offset;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.CommonServerMessageBlockRequest#size() </seealso>
		public override int size() {
			return size8(Smb2Constants.SMB2_HEADER_LENGTH + 48 + this.dataLength);
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb2.ServerMessageBlock2#writeBytesWireFormat(byte[], int) </seealso>
		protected  override int writeBytesWireFormat(byte[] dst, int dstIndex) {
			int start = dstIndex;
			SMBUtil.writeInt2(49, dst, dstIndex);
			int dataOffsetOffset = dstIndex + 2;
			dstIndex += 4;
			SMBUtil.writeInt4(this.dataLength, dst, dstIndex);
			dstIndex += 4;
			SMBUtil.writeInt8(this.offset, dst, dstIndex);
			dstIndex += 8;
			Array.Copy(this.fileId, 0, dst, dstIndex, 16);
			dstIndex += 16;
			SMBUtil.writeInt4(this.channel, dst, dstIndex);
			dstIndex += 4;
			SMBUtil.writeInt4(this.remainingBytes, dst, dstIndex);
			dstIndex += 4;

			SMBUtil.writeInt2(0, dst, dstIndex); // writeChannelInfoOffset
			SMBUtil.writeInt2(0, dst, dstIndex + 2); // writeChannelInfoLength
			dstIndex += 4;

			SMBUtil.writeInt4(this.writeFlags, dst, dstIndex);
			dstIndex += 4;

			SMBUtil.writeInt2(dstIndex - getHeaderStart(), dst, dataOffsetOffset);

			if (dstIndex + this.dataLength > dst.Length) {
				throw new System.ArgumentException(string.Format("Data exceeds buffer size ( remain buffer: {0:D} data length: {1:D})", dst.Length - dstIndex, this.dataLength));
			}

			Array.Copy(this.data, this.dataOffset, dst, dstIndex, this.dataLength);
			dstIndex += this.dataLength;
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