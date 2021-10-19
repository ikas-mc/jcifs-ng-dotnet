using jcifs;

using System;
using Configuration = jcifs.Configuration;
using Decodable = jcifs.Decodable;
using SMBProtocolDecodingException = jcifs.@internal.SMBProtocolDecodingException;
using DfsReferralResponseBuffer = jcifs.@internal.dfs.DfsReferralResponseBuffer;
using ServerMessageBlock2Response = jcifs.@internal.smb2.ServerMessageBlock2Response;
using SMBUtil = jcifs.@internal.util.SMBUtil;
using NtStatus = jcifs.smb.NtStatus;
using SmbException = jcifs.smb.SmbException;

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
	public class Smb2IoctlResponse : ServerMessageBlock2Response {

		private readonly byte[] outputBuffer;
		private int ctlCode;
		private byte[] fileId;
		private int ioctlFlags;
		private Decodable outputData;
		private Decodable inputData;
		private int outputLength;


		/// <param name="config"> </param>
		public Smb2IoctlResponse(Configuration config) : base(config) {
			this.outputBuffer = null;
		}


		/// <param name="config"> </param>
		/// <param name="outputBuffer"> </param>
		public Smb2IoctlResponse(Configuration config, byte[] outputBuffer) : base(config) {
			this.outputBuffer = outputBuffer;
		}


		/// <param name="config"> </param>
		/// <param name="outputBuffer"> </param>
		/// <param name="ctlCode"> </param>
		public Smb2IoctlResponse(Configuration config, byte[] outputBuffer, int ctlCode) : base(config) {
			this.outputBuffer = outputBuffer;
			this.ctlCode = ctlCode;
		}


		/// <returns> the ctlCode </returns>
		public virtual int getCtlCode() {
			return this.ctlCode;
		}


		/// <returns> the ioctlFlags </returns>
		public virtual int getIoctlFlags() {
			return this.ioctlFlags;
		}


		/// <returns> the fileId </returns>
		public virtual byte[] getFileId() {
			return this.fileId;
		}


		/// <returns> the outputData </returns>
		public virtual Decodable getOutputData() {
			return this.outputData;
		}


		/// <returns> the outputLength </returns>
		public virtual int getOutputLength() {
			return this.outputLength;
		}


		/// <returns> the inputData </returns>
		public virtual Decodable getInputData() {
			return this.inputData;
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
		/// <seealso cref= jcifs.internal.smb2.ServerMessageBlock2#isErrorResponseStatus() </seealso>
		protected  override bool isErrorResponseStatus() {
			int status = getStatus();
			return status != NtStatus.NT_STATUS_INVALID_PARAMETER && !(status == NtStatus.NT_STATUS_INVALID_PARAMETER && (this.ctlCode == Smb2IoctlRequest.FSCTL_SRV_COPYCHUNK || this.ctlCode == Smb2IoctlRequest.FSCTL_SRV_COPYCHUNK_WRITE)) && !(status == NtStatus.NT_STATUS_BUFFER_OVERFLOW && (this.ctlCode == Smb2IoctlRequest.FSCTL_PIPE_TRANSCEIVE || this.ctlCode == Smb2IoctlRequest.FSCTL_PIPE_PEEK || this.ctlCode == Smb2IoctlRequest.FSCTL_DFS_GET_REFERRALS)) && base.isErrorResponseStatus();
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
				return base.readErrorResponse(buffer, bufferIndex);
			}
			else if (structureSize != 49) {
				throw new SMBProtocolDecodingException("Expected structureSize = 49");
			}
			bufferIndex += 4;
			this.ctlCode = SMBUtil.readInt4(buffer, bufferIndex);
			bufferIndex += 4;

			this.fileId = new byte[16];
			Array.Copy(buffer, bufferIndex, this.fileId, 0, 16);
			bufferIndex += 16;

			int inputOffset = SMBUtil.readInt4(buffer, bufferIndex) + getHeaderStart();
			bufferIndex += 4;

			int inputCount = SMBUtil.readInt4(buffer, bufferIndex);
			bufferIndex += 4;

			int outputOffset = SMBUtil.readInt4(buffer, bufferIndex) + getHeaderStart();
			bufferIndex += 4;

			int outputCount = SMBUtil.readInt4(buffer, bufferIndex);
			bufferIndex += 4;

			this.ioctlFlags = SMBUtil.readInt4(buffer, bufferIndex);
			bufferIndex += 4;
			bufferIndex += 4; // Reserved2

			this.inputData = createInputDecodable();
			this.outputData = this.outputBuffer == null ? createOutputDecodable() : null;

			if (this.inputData != null) {
				this.inputData.decode(buffer, inputOffset, inputCount);
			}
			bufferIndex = Math.Max(inputOffset + inputCount, bufferIndex);

			if (this.outputBuffer != null) {
				if (outputCount > this.outputBuffer.Length) {
					throw new SMBProtocolDecodingException("Output length exceeds buffer size");
				}
				Array.Copy(buffer, outputOffset, this.outputBuffer, 0, outputCount);
			}
			else if (this.outputData != null) {
				this.outputData.decode(buffer, outputOffset, outputCount);
			}
			this.outputLength = outputCount;
			bufferIndex = Math.Max(outputOffset + outputCount, bufferIndex);
			return bufferIndex - start;
		}


		/// <summary>
		/// @return
		/// </summary>
		protected  virtual Decodable createOutputDecodable() {
			switch (this.ctlCode) {
			case Smb2IoctlRequest.FSCTL_DFS_GET_REFERRALS:
				return new DfsReferralResponseBuffer();
			case Smb2IoctlRequest.FSCTL_SRV_REQUEST_RESUME_KEY:
				return new SrvRequestResumeKeyResponse();
			case Smb2IoctlRequest.FSCTL_SRV_COPYCHUNK:
			case Smb2IoctlRequest.FSCTL_SRV_COPYCHUNK_WRITE:
				return new SrvCopyChunkCopyResponse();
			case Smb2IoctlRequest.FSCTL_VALIDATE_NEGOTIATE_INFO:
				return new ValidateNegotiateInfoResponse();
			case Smb2IoctlRequest.FSCTL_PIPE_PEEK:
				return new SrvPipePeekResponse();
			}
			return null;
		}


		/// <summary>
		/// @return
		/// </summary>
		protected  virtual Decodable createInputDecodable() {
			return null;
		}


		/// <param name="responseType"> </param>
		/// <returns> decoded data </returns>
		/// <exception cref="SmbException"> </exception>
		/// throws jcifs.smb.SmbException
		public virtual T getOutputData<T>(Type responseType) where T : Decodable {

			Decodable @out = getOutputData();

			if (@out == null) {
				throw new SmbException("Failed to decode output data");
			}

			if (!responseType.IsAssignableFrom(@out.GetType())) {
				throw new SmbException("Incompatible response data " + @out.GetType());
			}
			return (T) @out;
		}

	}

}