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
	public class Smb2FlushRequest : ServerMessageBlock2Request<Smb2FlushResponse>, RequestWithFileId {

		private byte[] fileId;


		/// <param name="config"> </param>
		/// <param name="fileId"> </param>
		public Smb2FlushRequest(Configuration config, byte[] fileId) : base(config, SMB2_FLUSH) {
			this.fileId = fileId;
		}


		protected  override Smb2FlushResponse createResponse(CIFSContext tc, ServerMessageBlock2Request<Smb2FlushResponse> req) {
			return new Smb2FlushResponse(tc.getConfig());
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
			return size8(Smb2Constants.SMB2_HEADER_LENGTH + 24);
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb2.ServerMessageBlock2#writeBytesWireFormat(byte[], int) </seealso>
		protected  override int writeBytesWireFormat(byte[] dst, int dstIndex) {
			int start = dstIndex;
			SMBUtil.writeInt2(24, dst, dstIndex);
			dstIndex += 2;
			dstIndex += 2; // Reserved1
			dstIndex += 4; // Reserved2

			Array.Copy(this.fileId, 0, dst, dstIndex, 16);
			dstIndex += 16;

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