using System;
using Configuration = jcifs.Configuration;
using CommonServerMessageBlockRequest = jcifs.@internal.CommonServerMessageBlockRequest;
using SMBProtocolDecodingException = jcifs.@internal.SMBProtocolDecodingException;
using ServerMessageBlock2Response = jcifs.@internal.smb2.ServerMessageBlock2Response;
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
namespace jcifs.@internal.smb2.session {



	/// <summary>
	/// @author mbechler
	/// 
	/// </summary>
	public class Smb2SessionSetupResponse : ServerMessageBlock2Response {

		/// 
		public const int SMB2_SESSION_FLAGS_IS_GUEST = 0x1;

		/// 
		public const int SMB2_SESSION_FLAGS_IS_NULL = 0x2;

		/// 
		public const int SMB2_SESSION_FLAG_ENCRYPT_DATA = 0x4;

		private int sessionFlags;
		private byte[] blob;


		/// <param name="config"> </param>
		public Smb2SessionSetupResponse(Configuration config) : base(config) {
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb2.ServerMessageBlock2Response#prepare(jcifs.internal.CommonServerMessageBlockRequest) </seealso>
		public override void prepare(CommonServerMessageBlockRequest next) {
			if (isReceived()) {
				next.setSessionId(getSessionId());
			}
			base.prepare(next);
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb2.ServerMessageBlock2#isErrorResponseStatus() </seealso>
		protected  override bool isErrorResponseStatus() {
			return getStatus() != NtStatus.NT_STATUS_MORE_PROCESSING_REQUIRED && base.isErrorResponseStatus();
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
		/// <exception cref="Smb2ProtocolDecodingException">
		/// </exception>
		/// <seealso cref= jcifs.internal.smb2.ServerMessageBlock2#readBytesWireFormat(byte[], int) </seealso>
		/// throws jcifs.internal.SMBProtocolDecodingException
		protected  override int readBytesWireFormat(byte[] buffer, int bufferIndex) {
			int start = bufferIndex;

			int structureSize = SMBUtil.readInt2(buffer, bufferIndex);
			if (structureSize != 9) {
				throw new SMBProtocolDecodingException("Structure size != 9");
			}

			this.sessionFlags = SMBUtil.readInt2(buffer, bufferIndex + 2);
			bufferIndex += 4;

			int securityBufferOffset = SMBUtil.readInt2(buffer, bufferIndex);
			int securityBufferLength = SMBUtil.readInt2(buffer, bufferIndex + 2);
			bufferIndex += 4;

			int pad = bufferIndex - (getHeaderStart() + securityBufferOffset);
			this.blob = new byte[securityBufferLength];
			Array.Copy(buffer, getHeaderStart() + securityBufferOffset, this.blob, 0, securityBufferLength);
			bufferIndex += pad;
			bufferIndex += securityBufferLength;

			return bufferIndex - start;
		}


		/// <returns> whether the session is either anonymous or a guest session </returns>
		public virtual bool isLoggedInAsGuest() {
			return (this.sessionFlags & (SMB2_SESSION_FLAGS_IS_GUEST | SMB2_SESSION_FLAGS_IS_NULL)) != 0;
		}


		/// <returns> the sessionFlags </returns>
		public virtual int getSessionFlags() {
			return this.sessionFlags;
		}


		/// <returns> security blob </returns>
		public virtual byte[] getBlob() {
			return this.blob;
		}

	}

}