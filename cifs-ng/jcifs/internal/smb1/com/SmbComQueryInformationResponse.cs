using System;
using Configuration = jcifs.Configuration;
using SmbBasicFileInfo = jcifs.@internal.SmbBasicFileInfo;
using ServerMessageBlock = jcifs.@internal.smb1.ServerMessageBlock;
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
	public class SmbComQueryInformationResponse : ServerMessageBlock, SmbBasicFileInfo {

		private int fileAttributes = 0x0000;
		private long lastWriteTime = 0L;
		private long serverTimeZoneOffset;
		private int fileSize = 0;


		/// 
		/// <param name="config"> </param>
		/// <param name="serverTimeZoneOffset"> </param>
		public SmbComQueryInformationResponse(Configuration config, long serverTimeZoneOffset) : base(config, SMB_COM_QUERY_INFORMATION) {
			this.serverTimeZoneOffset = serverTimeZoneOffset;
		}


		public virtual int getAttributes() {
			return this.fileAttributes;
		}


		public virtual long getCreateTime() {
			return convertTime(this.lastWriteTime);
		}


		/// <param name="time">
		/// @return </param>
		private long convertTime(long time) {
			return time + this.serverTimeZoneOffset;
		}


		public virtual long getLastWriteTime() {
			return convertTime(this.lastWriteTime);
		}


		public virtual long getLastAccessTime() {
			// Fake access time
			return convertTime(this.lastWriteTime);
		}


		public virtual long getSize() {
			return this.fileSize;
		}


		protected internal override int writeParameterWordsWireFormat(byte[] dst, int dstIndex) {
			return 0;
		}


		protected internal override int writeBytesWireFormat(byte[] dst, int dstIndex) {
			return 0;
		}


		protected internal override int readParameterWordsWireFormat(byte[] buffer, int bufferIndex) {
			if (this.wordCount == 0) {
				return 0;
			}
			this.fileAttributes = SMBUtil.readInt2(buffer, bufferIndex);
			bufferIndex += 2;
			this.lastWriteTime = SMBUtil.readUTime(buffer, bufferIndex);
			bufferIndex += 4;
			this.fileSize = SMBUtil.readInt4(buffer, bufferIndex);
			return 20;
		}


		protected internal override int readBytesWireFormat(byte[] buffer, int bufferIndex) {
			return 0;
		}


		public override string ToString() {
			return "SmbComQueryInformationResponse[" + base.ToString() + ",fileAttributes=0x" + Hexdump.toHexString(this.fileAttributes, 4) + ",lastWriteTime=" + DateTimeOffset.FromUnixTimeMilliseconds(this.lastWriteTime) + ",fileSize=" + this.fileSize + "]";
		}
	}

}