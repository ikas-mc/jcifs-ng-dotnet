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
namespace jcifs.@internal.smb2.@lock {



	/// <summary>
	/// @author mbechler
	/// 
	/// </summary>
	public class Smb2LockRequest : ServerMessageBlock2Request<Smb2LockResponse>, RequestWithFileId {

		private int lockSequenceNumber=0;
		private int lockSequenceIndex=0;
		private byte[] fileId;
		private readonly Smb2Lock[] locks;


		/// <param name="config"> </param>
		/// <param name="fileId"> </param>
		/// <param name="locks"> </param>
		public Smb2LockRequest(Configuration config, byte[] fileId, Smb2Lock[] locks) : base(config, SMB2_LOCK) {
			this.fileId = fileId;
			this.locks = locks;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb2.ServerMessageBlock2Request#createResponse(jcifs.CIFSContext,
		///      jcifs.internal.smb2.ServerMessageBlock2Request) </seealso>
		protected  override Smb2LockResponse createResponse(CIFSContext tc, ServerMessageBlock2Request<Smb2LockResponse> req) {
			return new Smb2LockResponse(tc.getConfig());
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb2.RequestWithFileId#setFileId(byte[]) </seealso>
		public virtual void setFileId(byte[] fileId) {
			this.fileId = fileId;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.CommonServerMessageBlockRequest#size() </seealso>
		public override int size() {
			int size = Smb2Constants.SMB2_HEADER_LENGTH + 24;
			foreach (Smb2Lock l in this.locks) {
				size += l.size();
			}
			return size8(size);
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb2.ServerMessageBlock2#writeBytesWireFormat(byte[], int) </seealso>
		protected  override int writeBytesWireFormat(byte[] dst, int dstIndex) {
			int start = dstIndex;
			SMBUtil.writeInt2(48, dst, dstIndex);
			SMBUtil.writeInt2(this.locks.Length, dst, dstIndex + 2);
			dstIndex += 4;
			SMBUtil.writeInt4(((this.lockSequenceNumber & 0xF) << 28) | (this.lockSequenceIndex & 0x0FFFFFFF), dst, dstIndex);
			dstIndex += 4;
			Array.Copy(this.fileId, 0, dst, dstIndex, 16);
			dstIndex += 16;

			foreach (Smb2Lock l in this.locks) {
				dstIndex += l.encode(dst, dstIndex);
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