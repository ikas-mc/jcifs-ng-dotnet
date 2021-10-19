using System;
using CIFSContext = jcifs.CIFSContext;
using Configuration = jcifs.Configuration;
using Encodable = jcifs.Encodable;
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
namespace jcifs.@internal.smb2.info {



	/// <summary>
	/// @author mbechler
	/// 
	/// </summary>
	public class Smb2QueryInfoRequest : ServerMessageBlock2Request<Smb2QueryInfoResponse>, RequestWithFileId {

		private byte infoType;
		private byte fileInfoClass;
		private int outputBufferLength;
		private int additionalInformation;
		private int queryFlags;
		private byte[] fileId;
		private Encodable inputBuffer;


		/// <param name="config"> </param>
		public Smb2QueryInfoRequest(Configuration config) : this(config, Smb2Constants.UNSPECIFIED_FILEID) {
		}


		/// <param name="config"> </param>
		/// <param name="fileId"> </param>
		public Smb2QueryInfoRequest(Configuration config, byte[] fileId) : base(config, SMB2_QUERY_INFO) {
			this.outputBufferLength = config.getListSize();
			this.fileId = fileId;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb2.RequestWithFileId#setFileId(byte[]) </seealso>
		public virtual void setFileId(byte[] fileId) {
			this.fileId = fileId;
		}


		/// <param name="infoType">
		///            the infoType to set </param>
		public void setInfoType(byte infoType) {
			this.infoType = infoType;
		}


		/// <param name="fileInfoClass">
		///            the fileInfoClass to set </param>
		public void setFileInfoClass(byte fileInfoClass) {
			setInfoType(Smb2Constants.SMB2_0_INFO_FILE);
			this.fileInfoClass = fileInfoClass;
		}


		/// <param name="fileInfoClass">
		///            the fileInfoClass to set </param>
		public void setFilesystemInfoClass(byte fileInfoClass) {
			setInfoType(Smb2Constants.SMB2_0_INFO_FILESYSTEM);
			this.fileInfoClass = fileInfoClass;
		}


		/// <param name="additionalInformation">
		///            the additionalInformation to set </param>
		public void setAdditionalInformation(int additionalInformation) {
			this.additionalInformation = additionalInformation;
		}


		/// <param name="queryFlags">
		///            the queryFlags to set </param>
		public void setQueryFlags(int queryFlags) {
			this.queryFlags = queryFlags;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb2.ServerMessageBlock2Request#createResponse(jcifs.CIFSContext,
		///      jcifs.internal.smb2.ServerMessageBlock2Request) </seealso>
		protected  override Smb2QueryInfoResponse createResponse(CIFSContext tc, ServerMessageBlock2Request<Smb2QueryInfoResponse> req) {
			return new Smb2QueryInfoResponse(tc.getConfig(), this.infoType, this.fileInfoClass);
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.CommonServerMessageBlockRequest#size() </seealso>
		public override int size() {
			int size = Smb2Constants.SMB2_HEADER_LENGTH + 40;
			if (this.inputBuffer != null) {
				size += this.inputBuffer.size();
			}
			return size8(size);
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb2.ServerMessageBlock2#writeBytesWireFormat(byte[], int) </seealso>
		protected  override int writeBytesWireFormat(byte[] dst, int dstIndex) {
			int start = dstIndex;

			SMBUtil.writeInt2(41, dst, dstIndex);
			dst[dstIndex + 2] = this.infoType;
			dst[dstIndex + 3] = this.fileInfoClass;
			dstIndex += 4;

			SMBUtil.writeInt4(this.outputBufferLength, dst, dstIndex);
			dstIndex += 4;
			int inBufferOffsetOffset = dstIndex;
			dstIndex += 4;
			int inBufferLengthOffset = dstIndex;
			dstIndex += 4;
			SMBUtil.writeInt4(this.additionalInformation, dst, dstIndex);
			dstIndex += 4;
			SMBUtil.writeInt4(this.queryFlags, dst, dstIndex);
			dstIndex += 4;
			Array.Copy(this.fileId, 0, dst, dstIndex, 16);
			dstIndex += 16;

			if (this.inputBuffer == null) {
				SMBUtil.writeInt2(0, dst, inBufferOffsetOffset);
				SMBUtil.writeInt4(0, dst, inBufferLengthOffset);
			}
			else {
				SMBUtil.writeInt2(dstIndex - getHeaderStart(), dst, inBufferOffsetOffset);
				int len = this.inputBuffer.encode(dst, dstIndex);
				SMBUtil.writeInt4(len, dst, inBufferLengthOffset);
				dstIndex += len;
			}
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