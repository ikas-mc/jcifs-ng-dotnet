using System;
using System.Text;
using cifs_ng.lib.ext;
using CIFSContext = jcifs.CIFSContext;
using Configuration = jcifs.Configuration;
using ServerMessageBlock2 = jcifs.@internal.smb2.ServerMessageBlock2;
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
	public class Smb2TreeConnectRequest : ServerMessageBlock2Request<Smb2TreeConnectResponse> {

		private int treeFlags;
		private string path;


		/// <param name="config"> </param>
		/// <param name="path"> </param>
		public Smb2TreeConnectRequest(Configuration config, string path) : base(config, SMB2_TREE_CONNECT) {
			this.path = path;
		}


		protected  override Smb2TreeConnectResponse createResponse(CIFSContext tc, ServerMessageBlock2Request<Smb2TreeConnectResponse> req) {
			return new Smb2TreeConnectResponse(tc.getConfig());
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb2.ServerMessageBlock2#chain(jcifs.internal.smb2.ServerMessageBlock2) </seealso>
		public override bool chain(ServerMessageBlock2 n) {
			n.setTreeId(Smb2Constants.UNSPECIFIED_TREEID);
			return base.chain(n);
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.CommonServerMessageBlockRequest#size() </seealso>
		public override int size() {
			return size8(Smb2Constants.SMB2_HEADER_LENGTH + 8 + this.path.Length * 2);
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb2.ServerMessageBlock2#writeBytesWireFormat(byte[], int) </seealso>
		protected  override int writeBytesWireFormat(byte[] dst, int dstIndex) {
			int start = dstIndex;
			SMBUtil.writeInt2(9, dst, dstIndex);
			SMBUtil.writeInt2(this.treeFlags, dst, dstIndex + 2);
			dstIndex += 4;

			byte[] data = this.path.getBytes(Encoding.Unicode);
			int offsetOffset = dstIndex;
			SMBUtil.writeInt2(data.Length, dst, dstIndex + 2);
			dstIndex += 4;
			SMBUtil.writeInt2(dstIndex - getHeaderStart(), dst, offsetOffset);

			Array.Copy(data, 0, dst, dstIndex, data.Length);
			dstIndex += data.Length;
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