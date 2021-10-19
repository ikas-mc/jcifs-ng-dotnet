using System;
using Configuration = jcifs.Configuration;
using SmbBasicFileInfo = jcifs.@internal.SmbBasicFileInfo;
using AndXServerMessageBlock = jcifs.@internal.smb1.AndXServerMessageBlock;
using SMBUtil = jcifs.@internal.util.SMBUtil;
using Hexdump = jcifs.util.Hexdump;

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
	/// 
	public class SmbComNTCreateAndXResponse : AndXServerMessageBlock, SmbBasicFileInfo {

		internal const int EXCLUSIVE_OPLOCK_GRANTED = 1;
		internal const int BATCH_OPLOCK_GRANTED = 2;
		internal const int LEVEL_II_OPLOCK_GRANTED = 3;

		private byte oplockLevel;
		private int fid, createAction, extFileAttributes, fileType, deviceState;
		private long creationTime, lastAccessTime, lastWriteTime, changeTime, allocationSize, endOfFile;
		private bool directory;
		private bool isExtendedField;


		/// 
		/// <param name="config"> </param>
		public SmbComNTCreateAndXResponse(Configuration config) : base(config) {
		}


		/// <returns> the fileType </returns>
		public int getFileType() {
			return this.fileType;
		}


		/// <returns> the isExtended </returns>
		public bool isExtended() {
			return this.isExtendedField;
		}


		/// <param name="isExtended">
		///            the isExtended to set </param>
		public void setExtended(bool isExtended) {
			this.isExtendedField = isExtended;
		}


		/// <returns> the oplockLevel </returns>
		public byte getOplockLevel() {
			return this.oplockLevel;
		}


		/// <returns> the fid </returns>
		public int getFid() {
			return this.fid;
		}


		/// <returns> the createAction </returns>
		public int getCreateAction() {
			return this.createAction;
		}


		/// <returns> the extFileAttributes </returns>
		public int getExtFileAttributes() {
			return this.extFileAttributes;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.SmbBasicFileInfo#getAttributes() </seealso>
		public virtual int getAttributes() {
			return getExtFileAttributes();
		}


		/// <returns> the deviceState </returns>
		public int getDeviceState() {
			return this.deviceState;
		}


		/// <returns> the creationTime </returns>
		public long getCreationTime() {
			return this.creationTime;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.SmbBasicFileInfo#getCreateTime() </seealso>
		public virtual long getCreateTime() {
			return getCreationTime();
		}


		/// <returns> the lastAccessTime </returns>
		public long getLastAccessTime() {
			return this.lastAccessTime;
		}


		/// <returns> the lastWriteTime </returns>
		public long getLastWriteTime() {
			return this.lastWriteTime;
		}


		/// <returns> the allocationSize </returns>
		public long getAllocationSize() {
			return this.allocationSize;
		}


		/// <returns> the endOfFile </returns>
		public long getEndOfFile() {
			return this.endOfFile;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.SmbBasicFileInfo#getSize() </seealso>
		public virtual long getSize() {
			return getEndOfFile();
		}


		protected internal override int writeParameterWordsWireFormat(byte[] dst, int dstIndex) {
			return 0;
		}


		protected internal override int writeBytesWireFormat(byte[] dst, int dstIndex) {
			return 0;
		}


		protected internal override int readParameterWordsWireFormat(byte[] buffer, int bufferIndex) {
			int start = bufferIndex;

			this.oplockLevel = buffer[bufferIndex++];
			this.fid = SMBUtil.readInt2(buffer, bufferIndex);
			bufferIndex += 2;
			this.createAction = SMBUtil.readInt4(buffer, bufferIndex);
			bufferIndex += 4;
			this.creationTime = SMBUtil.readTime(buffer, bufferIndex);
			bufferIndex += 8;
			this.lastAccessTime = SMBUtil.readTime(buffer, bufferIndex);
			bufferIndex += 8;
			this.lastWriteTime = SMBUtil.readTime(buffer, bufferIndex);
			bufferIndex += 8;
			this.changeTime = SMBUtil.readTime(buffer, bufferIndex);
			bufferIndex += 8;
			this.extFileAttributes = SMBUtil.readInt4(buffer, bufferIndex);
			bufferIndex += 4;
			this.allocationSize = SMBUtil.readInt8(buffer, bufferIndex);
			bufferIndex += 8;
			this.endOfFile = SMBUtil.readInt8(buffer, bufferIndex);
			bufferIndex += 8;
			this.fileType = SMBUtil.readInt2(buffer, bufferIndex);
			bufferIndex += 2;
			this.deviceState = SMBUtil.readInt2(buffer, bufferIndex);
			bufferIndex += 2;
			this.directory = (buffer[bufferIndex++] & 0xFF) > 0;
			return bufferIndex - start;
		}


		protected internal override int readBytesWireFormat(byte[] buffer, int bufferIndex) {
			return 0;
		}


		public override string ToString() {
			return "SmbComNTCreateAndXResponse[" + base.ToString() + ",oplockLevel=" + this.oplockLevel + ",fid=" + this.fid + ",createAction=0x" + Hexdump.toHexString(this.createAction, 4) + ",creationTime=" + DateTimeOffset.FromUnixTimeMilliseconds(this.creationTime) + ",lastAccessTime=" + DateTimeOffset.FromUnixTimeMilliseconds(this.lastAccessTime) + ",lastWriteTime=" + DateTimeOffset.FromUnixTimeMilliseconds(this.lastWriteTime) + ",changeTime=" + DateTimeOffset.FromUnixTimeMilliseconds(this.changeTime) + ",extFileAttributes=0x" + Hexdump.toHexString(this.extFileAttributes, 4) + ",allocationSize=" + this.allocationSize + ",endOfFile=" + this.endOfFile + ",fileType=" + this.fileType + ",deviceState=" + this.deviceState + ",directory=" + this.directory + "]";
		}
	}

}