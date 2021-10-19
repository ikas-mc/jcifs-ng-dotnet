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
namespace jcifs.@internal.smb2.ioctl {



	/// <summary>
	/// @author mbechler
	/// 
	/// </summary>
	public class Smb2IoctlRequest : ServerMessageBlock2Request<Smb2IoctlResponse>, RequestWithFileId {

		/// 
		public const int FSCTL_DFS_GET_REFERRALS = 0x0060194;
		/// 
		public const int FSCTL_PIPE_PEEK = 0x0011400C;
		/// 
		public const int FSCTL_PIPE_WAIT = 0x00110018;
		/// 
		public const int FSCTL_PIPE_TRANSCEIVE = 0x0011C017;
		/// 
		public const int FSCTL_SRV_COPYCHUNK = 0x001440F2;
		/// 
		public const int FSCTL_SRV_ENUMERATE_SNAPSHOTS = 0x00144064;
		/// 
		public const int FSCTL_SRV_REQUEST_RESUME_KEY = 0x00140078;
		/// 
		public const int FSCTL_SRV_READ_HASH = 0x001441bb;
		/// 
		public const int FSCTL_SRV_COPYCHUNK_WRITE = 0x001480F2;
		/// 
		public const int FSCTL_LRM_REQUEST_RESILENCY = 0x001401D4;
		/// 
		public const int FSCTL_QUERY_NETWORK_INTERFACE_INFO = 0x001401FC;
		/// 
		public const int FSCTL_SET_REPARSE_POINT = 0x000900A4;
		/// 
		public const int FSCTL_DFS_GET_REFERRALS_EX = 0x000601B0;
		/// 
		public const int FSCTL_FILE_LEVEL_TRIM = 0x00098208;
		/// 
		public const int FSCTL_VALIDATE_NEGOTIATE_INFO = 0x000140204;

		/// 
		public const int SMB2_O_IOCTL_IS_FSCTL = 0x1;

		private byte[] fileId;
		private readonly int controlCode;
		private readonly byte[] outputBuffer;
		private int maxOutputResponse;
		private int maxInputResponse;
		private int flags;
		private Encodable inputData;
		private Encodable outputData;


		/// <param name="config"> </param>
		/// <param name="controlCode">
		///  </param>
		public Smb2IoctlRequest(Configuration config, int controlCode) : this(config, controlCode, Smb2Constants.UNSPECIFIED_FILEID) {
		}


		/// <param name="config"> </param>
		/// <param name="controlCode"> </param>
		/// <param name="fileId"> </param>
		public Smb2IoctlRequest(Configuration config, int controlCode, byte[] fileId) : base(config, SMB2_IOCTL) {
			this.controlCode = controlCode;
			this.fileId = fileId;
			this.maxOutputResponse = config.getTransactionBufferSize() & ~0x7;
			this.outputBuffer = null;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb2.RequestWithFileId#setFileId(byte[]) </seealso>
		public virtual void setFileId(byte[] fileId) {
			this.fileId = fileId;
		}


		/// 
		/// <param name="config"> </param>
		/// <param name="controlCode"> </param>
		/// <param name="fileId"> </param>
		/// <param name="outputBuffer"> </param>
		public Smb2IoctlRequest(Configuration config, int controlCode, byte[] fileId, byte[] outputBuffer) : base(config, SMB2_IOCTL) {
			this.controlCode = controlCode;
			this.fileId = fileId;
			this.outputBuffer = outputBuffer;
			this.maxOutputResponse = outputBuffer.Length;
		}


		protected  override Smb2IoctlResponse createResponse(CIFSContext tc, ServerMessageBlock2Request<Smb2IoctlResponse> req) {
			return new Smb2IoctlResponse(tc.getConfig(), this.outputBuffer, this.controlCode);
		}


		/// <param name="flags">
		///            the flags to set </param>
		public virtual void setFlags(int flags) {
			this.flags = flags;
		}


		/// <param name="maxInputResponse">
		///            the maxInputResponse to set </param>
		public virtual void setMaxInputResponse(int maxInputResponse) {
			this.maxInputResponse = maxInputResponse;
		}


		/// <param name="maxOutputResponse">
		///            the maxOutputResponse to set </param>
		public virtual void setMaxOutputResponse(int maxOutputResponse) {
			this.maxOutputResponse = maxOutputResponse;
		}


		/// <param name="inputData">
		///            the inputData to set </param>
		public virtual void setInputData(Encodable inputData) {
			this.inputData = inputData;
		}


		/// <param name="outputData">
		///            the outputData to set </param>
		public virtual void setOutputData(Encodable outputData) {
			this.outputData = outputData;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.CommonServerMessageBlockRequest#size() </seealso>
		public override int size() {
			int size = Smb2Constants.SMB2_HEADER_LENGTH + 56;
			int dataLength = 0;
			if (this.inputData != null) {
				dataLength += this.inputData.size();
			}
			if (this.outputData != null) {
				dataLength += this.outputData.size();
			}
			return size8(size + dataLength);
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb2.ServerMessageBlock2#writeBytesWireFormat(byte[], int) </seealso>
		protected  override int writeBytesWireFormat(byte[] dst, int dstIndex) {
			int start = dstIndex;
			SMBUtil.writeInt2(57, dst, dstIndex);
			dstIndex += 4;
			SMBUtil.writeInt4(this.controlCode, dst, dstIndex);
			dstIndex += 4;
			Array.Copy(this.fileId, 0, dst, dstIndex, 16);
			dstIndex += 16;

			int inputOffsetOffset = dstIndex;
			dstIndex += 4;
			int inputLengthOffset = dstIndex;
			dstIndex += 4;
			SMBUtil.writeInt4(this.maxInputResponse, dst, dstIndex);
			dstIndex += 4;

			int outputOffsetOffset = dstIndex;
			dstIndex += 4;
			int outputLengthOffset = dstIndex;
			dstIndex += 4;
			SMBUtil.writeInt4(this.maxOutputResponse, dst, dstIndex);
			dstIndex += 4;

			SMBUtil.writeInt4(this.flags, dst, dstIndex);
			dstIndex += 4;
			dstIndex += 4; // Reserved2

			if (this.inputData != null) {
				SMBUtil.writeInt4(dstIndex - getHeaderStart(), dst, inputOffsetOffset);
				int len = this.inputData.encode(dst, dstIndex);
				SMBUtil.writeInt4(len, dst, inputLengthOffset);
				dstIndex += len;
			}
			else {
				SMBUtil.writeInt4(0, dst, inputOffsetOffset);
				SMBUtil.writeInt4(0, dst, inputLengthOffset);
			}

			if (this.outputData != null) {
				SMBUtil.writeInt4(dstIndex - getHeaderStart(), dst, outputOffsetOffset);
				int len = this.outputData.encode(dst, dstIndex);
				SMBUtil.writeInt4(len, dst, outputLengthOffset);
				dstIndex += len;
			}
			else {
				SMBUtil.writeInt4(0, dst, outputOffsetOffset);
				SMBUtil.writeInt4(0, dst, outputLengthOffset);
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