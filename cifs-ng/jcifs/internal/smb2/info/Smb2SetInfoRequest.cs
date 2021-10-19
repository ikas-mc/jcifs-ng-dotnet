using jcifs;

using System;
using CIFSContext = jcifs.CIFSContext;
using Configuration = jcifs.Configuration;
using Encodable = jcifs.Encodable;
using FileInformation = jcifs.@internal.fscc.FileInformation;
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
	public class Smb2SetInfoRequest : ServerMessageBlock2Request<Smb2SetInfoResponse>, RequestWithFileId {

		private byte[] fileId;
		private byte infoType;
		private byte fileInfoClass;
		private int additionalInformation;
		private Encodable info;


		/// 
		/// <param name="config"> </param>
		public Smb2SetInfoRequest(Configuration config) : this(config, Smb2Constants.UNSPECIFIED_FILEID) {
		}


		/// <param name="config"> </param>
		/// <param name="fileId"> </param>
		public Smb2SetInfoRequest(Configuration config, byte[] fileId) : base(config, SMB2_SET_INFO) {
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
		public virtual void setInfoType(byte infoType) {
			this.infoType = infoType;
		}


		/// <param name="fileInfoClass">
		///            the fileInfoClass to set </param>
		public virtual void setFileInfoClass(byte fileInfoClass) {
			this.fileInfoClass = fileInfoClass;
		}


		/// <param name="additionalInformation">
		///            the additionalInformation to set </param>
		public virtual void setAdditionalInformation(int additionalInformation) {
			this.additionalInformation = additionalInformation;
		}


		/// 
		/// <param name="fi"> </param>
		public virtual void setFileInformation<T>(T fi) where T : FileInformation, Encodable {
			setInfoType(Smb2Constants.SMB2_0_INFO_FILE);
			setFileInfoClass(fi.getFileInformationLevel());
			setInfo(fi);
		}


		/// <param name="info">
		///            the info to set </param>
		public virtual void setInfo(Encodable info) {
			this.info = info;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb2.ServerMessageBlock2Request#createResponse(jcifs.CIFSContext,
		///      jcifs.internal.smb2.ServerMessageBlock2Request) </seealso>
		protected  override Smb2SetInfoResponse createResponse(CIFSContext tc, ServerMessageBlock2Request<Smb2SetInfoResponse> req) {
			return new Smb2SetInfoResponse(tc.getConfig());
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.CommonServerMessageBlockRequest#size() </seealso>
		public override int size() {
			return size8(Smb2Constants.SMB2_HEADER_LENGTH + 32 + this.info.size());
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb2.ServerMessageBlock2#writeBytesWireFormat(byte[], int) </seealso>
		protected  override int writeBytesWireFormat(byte[] dst, int dstIndex) {
			int start = dstIndex;
			SMBUtil.writeInt2(33, dst, dstIndex);
			dst[dstIndex + 2] = this.infoType;
			dst[dstIndex + 3] = this.fileInfoClass;
			dstIndex += 4;

			int bufferLengthOffset = dstIndex;
			dstIndex += 4;
			int bufferOffsetOffset = dstIndex;
			dstIndex += 4;

			SMBUtil.writeInt4(this.additionalInformation, dst, dstIndex);
			dstIndex += 4;

			Array.Copy(this.fileId, 0, dst, dstIndex, 16);
			dstIndex += 16;

			SMBUtil.writeInt2(dstIndex - getHeaderStart(), dst, bufferOffsetOffset);
			int len = this.info.encode(dst, dstIndex);
			SMBUtil.writeInt4(len, dst, bufferLengthOffset);
			dstIndex += len;
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