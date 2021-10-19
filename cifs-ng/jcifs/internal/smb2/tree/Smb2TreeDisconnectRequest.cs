using CIFSContext = jcifs.CIFSContext;
using Configuration = jcifs.Configuration;
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
namespace jcifs.@internal.smb2.tree {



	/// <summary>
	/// @author mbechler
	/// 
	/// </summary>
	public class Smb2TreeDisconnectRequest : ServerMessageBlock2Request<Smb2TreeDisconnectResponse> {

		/// <param name="config"> </param>
		public Smb2TreeDisconnectRequest(Configuration config) : base(config, SMB2_TREE_DISCONNECT) {
		}


		protected  override Smb2TreeDisconnectResponse createResponse(CIFSContext tc, ServerMessageBlock2Request<Smb2TreeDisconnectResponse> req) {
			return new Smb2TreeDisconnectResponse(tc.getConfig());
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.CommonServerMessageBlockRequest#size() </seealso>
		public override int size() {
			return size8(Smb2Constants.SMB2_HEADER_LENGTH + 4);
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb2.ServerMessageBlock2#writeBytesWireFormat(byte[], int) </seealso>
		protected  override int writeBytesWireFormat(byte[] dst, int dstIndex) {
			SMBUtil.writeInt2(4, dst, dstIndex);
			SMBUtil.writeInt2(0, dst, dstIndex + 2);
			return 4;
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