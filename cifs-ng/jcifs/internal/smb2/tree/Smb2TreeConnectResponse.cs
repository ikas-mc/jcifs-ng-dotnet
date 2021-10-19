using Configuration = jcifs.Configuration;
using CommonServerMessageBlockRequest = jcifs.@internal.CommonServerMessageBlockRequest;
using SMBProtocolDecodingException = jcifs.@internal.SMBProtocolDecodingException;
using TreeConnectResponse = jcifs.@internal.TreeConnectResponse;
using ServerMessageBlock2 = jcifs.@internal.smb2.ServerMessageBlock2;
using ServerMessageBlock2Response = jcifs.@internal.smb2.ServerMessageBlock2Response;
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
	public class Smb2TreeConnectResponse : ServerMessageBlock2Response, TreeConnectResponse {

		/// 
		public const byte SMB2_SHARE_TYPE_DISK = 0x1;
		/// 
		public const byte SMB2_SHARE_TYPE_PIPE = 0x2;
		/// 
		public const byte SMB2_SHARE_TYPE_PRINT = 0x3;

		/// 
		public const int SMB2_SHAREFLAG_MANUAL_CACHING = 0x0;
		/// 
		public const int SMB2_SHAREFLAG_AUTO_CACHING = 0x10;
		/// 
		public const int SMB2_SHAREFLAG_VDO_CACHING = 0x20;

		/// 
		public const int SMB2_SHAREFLAG_DFS = 0x1;
		/// 
		public const int SMB2_SHAREFLAG_DFS_ROOT = 0x2;

		/// 
		public const int SMB2_SHAREFLAG_RESTRICT_EXCLUSIVE_OPENS = 0x100;
		/// 
		public const int SMB2_SHAREFLAG_FORCE_SHARED_DELETE = 0x200;
		/// 
		public const int SMB2_SHAREFLAG_ALLOW_NAMESPACE_CACHING = 0x400;
		/// 
		public const int SMB2_SHAREFLAG_ACCESS_BASED_DIRECTORY_ENUM = 0x800;
		/// 
		public const int SMB2_SHAREFLAG_FORCE_LEVEL2_OPLOCK = 0x1000;
		/// 
		public const int SMB2_SHAREFLAG_ENABLE_HASH_V1 = 0x2000;
		/// 
		public const int SMB2_SHAREFLAG_ENABLE_HASH_V2 = 0x4000;
		/// 
		public const int SMB2_SHAREFLAG_ENCRYPT_DATA = 0x8000;

		/// 
		public const int SMB2_SHARE_CAP_DFS = 0x8;

		/// 
		public const int SMB2_SHARE_CAP_CONTINUOUS_AVAILABILITY = 0x10;

		/// 
		public const int SMB2_SHARE_CAP_SCALEOUT = 0x20;

		/// 
		public const int SMB2_SHARE_CAP_CLUSTER = 0x40;

		/// 
		public const int SMB2_SHARE_CAP_ASYMMETRIC = 0x80;

		private byte shareType;
		private int shareFlags;
		private int capabilities;
		private int maximalAccess;


		/// <param name="config"> </param>
		public Smb2TreeConnectResponse(Configuration config) : base(config) {
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb2.ServerMessageBlock2Response#prepare(jcifs.internal.CommonServerMessageBlockRequest) </seealso>
		public override void prepare(CommonServerMessageBlockRequest next) {
			if (isReceived()) {
				((ServerMessageBlock2) next).setTreeId(getTreeId());
			}
			base.prepare(next);
		}


		/// <returns> the shareType </returns>
		public virtual byte getShareType() {
			return this.shareType;
		}


		/// <returns> the shareFlags </returns>
		public virtual int getShareFlags() {
			return this.shareFlags;
		}


		/// <returns> the capabilities </returns>
		public virtual int getCapabilities() {
			return this.capabilities;
		}


		/// <returns> the maximalAccess </returns>
		public virtual int getMaximalAccess() {
			return this.maximalAccess;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.TreeConnectResponse#getTid() </seealso>
		public int getTid() {
			return getTreeId();
		}


		public virtual bool isValidTid() {
			return getTreeId() != 0;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.TreeConnectResponse#getService() </seealso>
		public virtual string getService() {
			return null;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.TreeConnectResponse#isShareDfs() </seealso>
		public virtual bool isShareDfs() {
			return (this.shareFlags & (SMB2_SHAREFLAG_DFS | SMB2_SHAREFLAG_DFS_ROOT)) != 0 || (this.capabilities & SMB2_SHARE_CAP_DFS) == SMB2_SHARE_CAP_DFS;
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
			if (structureSize != 16) {
				throw new SMBProtocolDecodingException("Structure size is not 16");
			}

			this.shareType = buffer[bufferIndex + 2];
			bufferIndex += 4;
			this.shareFlags = SMBUtil.readInt4(buffer, bufferIndex);
			bufferIndex += 4;
			this.capabilities = SMBUtil.readInt4(buffer, bufferIndex);
			bufferIndex += 4;
			this.maximalAccess = SMBUtil.readInt4(buffer, bufferIndex);
			bufferIndex += 4;
			return bufferIndex - start;
		}

	}

}