using System;
using System.Text;
using cifs_ng.lib.ext;
using CIFSContext = jcifs.CIFSContext;
using Configuration = jcifs.Configuration;
using RequestWithFileId = jcifs.@internal.smb2.RequestWithFileId;
using jcifs.@internal.smb2;
using jcifs.util;
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
	public class Smb2QueryDirectoryRequest : ServerMessageBlock2Request<Smb2QueryDirectoryResponse>, RequestWithFileId {

		/// 
		public const byte FILE_DIRECTORY_INFO = 0x1;

		/// 
		public const byte FILE_FULL_DIRECTORY_INFO = 0x2;

		/// 
		public const byte FILE_BOTH_DIRECTORY_INFO = 0x03;

		/// 
		public const byte FILE_NAMES_INFO = 0x0C;

		/// 
		public const byte FILE_ID_BOTH_DIRECTORY_INFO = 0x24;

		/// 
		public const byte FILE_ID_FULL_DIRECTORY_INFO = 0x26;

		/// 
		public const byte SMB2_RESTART_SCANS = 0x1;

		/// 
		public const byte SMB2_RETURN_SINGLE_ENTRY = 0x2;

		/// 
		public const byte SMB2_INDEX_SPECIFIED = 0x4;

		/// 
		public const byte SMB2_REOPEN = 0x10;

		private byte fileInformationClass = FILE_BOTH_DIRECTORY_INFO;
		private byte queryFlags;
		private int fileIndex;
		private byte[] fileId;
		private int outputBufferLength;
		private string fileName;

		/// 
		/// <param name="config"> </param>
		public Smb2QueryDirectoryRequest(Configuration config) : this(config, Smb2Constants.UNSPECIFIED_FILEID) {
		}


		/// <param name="config"> </param>
		/// <param name="fileId"> </param>
		public Smb2QueryDirectoryRequest(Configuration config, byte[] fileId) : base(config, SMB2_QUERY_DIRECTORY) {
			//TODO
			this.outputBufferLength = (Math.Min(config.getMaximumBufferSize(),CreditUtil.SINGLE_CREDIT_SIZE) - Smb2QueryDirectoryResponse.OVERHEAD) & ~0x7;
			this.fileId = fileId;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb2.RequestWithFileId#setFileId(byte[]) </seealso>
		public virtual void setFileId(byte[] fileId) {
			this.fileId = fileId;
		}


		/// <param name="fileInformationClass">
		///            the fileInformationClass to set </param>
		public virtual void setFileInformationClass(byte fileInformationClass) {
			this.fileInformationClass = fileInformationClass;
		}


		/// <param name="queryFlags">
		///            the queryFlags to set </param>
		public virtual void setQueryFlags(byte queryFlags) {
			this.queryFlags = queryFlags;
		}


		/// <param name="fileIndex">
		///            the fileIndex to set </param>
		public virtual void setFileIndex(int fileIndex) {
			this.fileIndex = fileIndex;
		}


		/// <param name="fileName">
		///            the fileName to set </param>
		public virtual void setFileName(string fileName) {
			this.fileName = fileName;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb2.ServerMessageBlock2Request#createResponse(jcifs.CIFSContext,
		///      jcifs.internal.smb2.ServerMessageBlock2Request) </seealso>
		protected  override Smb2QueryDirectoryResponse createResponse(CIFSContext tc, ServerMessageBlock2Request<Smb2QueryDirectoryResponse> req) {
			return new Smb2QueryDirectoryResponse(tc.getConfig(), this.fileInformationClass);
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.CommonServerMessageBlockRequest#size() </seealso>
		public override int size() {
			return size8(Smb2Constants.SMB2_HEADER_LENGTH + 32 + (this.fileName!=null ? 2 * this.fileName.Length : 0));
		}

		public override int getCreditCost() {
			//TODO 
			return 1;
		}

		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb2.ServerMessageBlock2#writeBytesWireFormat(byte[], int) </seealso>
		protected  override int writeBytesWireFormat(byte[] dst, int dstIndex) {
			int start = dstIndex;
			SMBUtil.writeInt2(33, dst, dstIndex);
			dst[dstIndex + 2] = this.fileInformationClass;
			dst[dstIndex + 3] = this.queryFlags;
			dstIndex += 4;
			SMBUtil.writeInt4(this.fileIndex, dst, dstIndex);
			dstIndex += 4;
			Array.Copy(this.fileId, 0, dst, dstIndex, 16);
			dstIndex += 16;

			int fnOffsetOffset = dstIndex;
			int fnLengthOffset = dstIndex + 2;
			dstIndex += 4;

			SMBUtil.writeInt4(this.outputBufferLength, dst, dstIndex);
			dstIndex += 4;

			if (this.fileName==null) {
				SMBUtil.writeInt2(0, dst, fnOffsetOffset);
				SMBUtil.writeInt2(0, dst, fnLengthOffset);
			}
			else {
				byte[] fnBytes = this.fileName.getBytes(Encoding.Unicode);
				SMBUtil.writeInt2(dstIndex - getHeaderStart(), dst, fnOffsetOffset);
				SMBUtil.writeInt2(fnBytes.Length, dst, fnLengthOffset);
				Array.Copy(fnBytes, 0, dst, dstIndex, fnBytes.Length);
				dstIndex += fnBytes.Length;
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