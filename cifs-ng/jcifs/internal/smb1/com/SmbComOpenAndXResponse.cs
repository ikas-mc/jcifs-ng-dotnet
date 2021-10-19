using Configuration = jcifs.Configuration;
using SmbBasicFileInfo = jcifs.@internal.SmbBasicFileInfo;
using AndXServerMessageBlock = jcifs.@internal.smb1.AndXServerMessageBlock;
using SMBUtil = jcifs.@internal.util.SMBUtil;

/* jcifs smb client library in Java
 * Copyright (C) 2000  "Michael B. Allen" <jcifs at samba dot org>
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

namespace jcifs.@internal.smb1.com {



	/// 
	public class SmbComOpenAndXResponse : AndXServerMessageBlock, SmbBasicFileInfo {

		private int fid, fileAttributes, fileDataSize, grantedAccess, fileType, deviceState, action, serverFid;
		private long lastWriteTime;


		/// 
		/// <param name="config"> </param>
		public SmbComOpenAndXResponse(Configuration config) : base(config) {
		}


		/// <param name="config"> </param>
		/// <param name="andxResp"> </param>
		public SmbComOpenAndXResponse(Configuration config, SmbComSeekResponse andxResp) : base(config, andxResp) {
		}


		/// <returns> the fid </returns>
		public int getFid() {
			return this.fid;
		}


		/// <returns> the dataSize </returns>
		public int getDataSize() {
			return this.fileDataSize;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.SmbBasicFileInfo#getSize() </seealso>
		public virtual long getSize() {
			return getDataSize();
		}


		/// <returns> the grantedAccess </returns>
		public int getGrantedAccess() {
			return this.grantedAccess;
		}


		/// <returns> the fileAttributes </returns>
		public int getFileAttributes() {
			return this.fileAttributes;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.SmbBasicFileInfo#getAttributes() </seealso>
		public virtual int getAttributes() {
			return getFileAttributes();
		}


		/// <returns> the fileType </returns>
		public int getFileType() {
			return this.fileType;
		}


		/// <returns> the deviceState </returns>
		public int getDeviceState() {
			return this.deviceState;
		}


		/// <returns> the action </returns>
		public int getAction() {
			return this.action;
		}


		/// <returns> the serverFid </returns>
		public int getServerFid() {
			return this.serverFid;
		}


		/// <returns> the lastWriteTime </returns>
		public long getLastWriteTime() {
			return this.lastWriteTime;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.SmbBasicFileInfo#getCreateTime() </seealso>
		public virtual long getCreateTime() {
			return 0;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.SmbBasicFileInfo#getLastAccessTime() </seealso>
		public virtual long getLastAccessTime() {
			return 0;
		}


		protected internal override int writeParameterWordsWireFormat(byte[] dst, int dstIndex) {
			return 0;
		}


		protected internal override int writeBytesWireFormat(byte[] dst, int dstIndex) {
			return 0;
		}


		protected internal override int readParameterWordsWireFormat(byte[] buffer, int bufferIndex) {
			int start = bufferIndex;

			this.fid = SMBUtil.readInt2(buffer, bufferIndex);
			bufferIndex += 2;
			this.fileAttributes = SMBUtil.readInt2(buffer, bufferIndex);
			bufferIndex += 2;
			this.lastWriteTime = SMBUtil.readUTime(buffer, bufferIndex);
			bufferIndex += 4;
			this.fileDataSize = SMBUtil.readInt4(buffer, bufferIndex);
			bufferIndex += 4;
			this.grantedAccess = SMBUtil.readInt2(buffer, bufferIndex);
			bufferIndex += 2;
			this.fileType = SMBUtil.readInt2(buffer, bufferIndex);
			bufferIndex += 2;
			this.deviceState = SMBUtil.readInt2(buffer, bufferIndex);
			bufferIndex += 2;
			this.action = SMBUtil.readInt2(buffer, bufferIndex);
			bufferIndex += 2;
			this.serverFid = SMBUtil.readInt4(buffer, bufferIndex);
			bufferIndex += 6;

			return bufferIndex - start;
		}


		protected internal override int readBytesWireFormat(byte[] buffer, int bufferIndex) {
			return 0;
		}


		public override string ToString() {
			return "SmbComOpenAndXResponse[" + base.ToString() + ",fid=" + this.fid + ",fileAttributes=" + this.fileAttributes + ",lastWriteTime=" + this.lastWriteTime + ",dataSize=" + this.fileDataSize + ",grantedAccess=" + this.grantedAccess + ",fileType=" + this.fileType + ",deviceState=" + this.deviceState + ",action=" + this.action + ",serverFid=" + this.serverFid + "]";
		}
	}

}