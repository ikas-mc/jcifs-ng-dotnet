using System;
using SMBProtocolDecodingException = jcifs.@internal.SMBProtocolDecodingException;
using SMBUtil = jcifs.@internal.util.SMBUtil;
using Hexdump = jcifs.util.Hexdump;

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
namespace jcifs.@internal.fscc {



	/// 
	public class FileBasicInfo : BasicFileInformation {

		private long createTime;
		private long lastAccessTime;
		private long lastWriteTime;
		private long changeTime;
		private int attributes;


		/// 
		public FileBasicInfo() {
		}


		/// <param name="create"> </param>
		/// <param name="lastAccess"> </param>
		/// <param name="lastWrite"> </param>
		/// <param name="change"> </param>
		/// <param name="attributes"> </param>
		public FileBasicInfo(long create, long lastAccess, long lastWrite, long change, int attributes) {
			this.createTime = create;
			this.lastAccessTime = lastAccess;
			this.lastWriteTime = lastWrite;
			this.changeTime = change;
			this.attributes = attributes;

		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.fscc.FileInformation#getFileInformationLevel() </seealso>
		public virtual byte getFileInformationLevel() {
			return FileInformationConstants.FILE_BASIC_INFO;
		}


		public virtual int getAttributes() {
			return this.attributes;
		}


		public virtual long getCreateTime() {
			return this.createTime;
		}


		public virtual long getLastWriteTime() {
			return this.lastWriteTime;
		}


		public virtual long getLastAccessTime() {
			return this.lastAccessTime;
		}


		public virtual long getSize() {
			return 0L;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Decodable#decode(byte[], int, int) </seealso>
		/// throws jcifs.internal.SMBProtocolDecodingException
		public virtual int decode(byte[] buffer, int bufferIndex, int len) {
			int start = bufferIndex;
			this.createTime = SMBUtil.readTime(buffer, bufferIndex);
			bufferIndex += 8;
			this.lastAccessTime = SMBUtil.readTime(buffer, bufferIndex);
			bufferIndex += 8;
			this.lastWriteTime = SMBUtil.readTime(buffer, bufferIndex);
			bufferIndex += 8;
			this.changeTime = SMBUtil.readTime(buffer, bufferIndex);
			bufferIndex += 8;
			this.attributes = SMBUtil.readInt4(buffer, bufferIndex);
			bufferIndex += 4;
			return bufferIndex - start;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Encodable#size() </seealso>
		public virtual int size() {
			return 40;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Encodable#encode(byte[], int) </seealso>
		public virtual int encode(byte[] dst, int dstIndex) {
			int start = dstIndex;
			SMBUtil.writeTime(this.createTime, dst, dstIndex);
			dstIndex += 8;
			SMBUtil.writeTime(this.lastAccessTime, dst, dstIndex);
			dstIndex += 8;
			SMBUtil.writeTime(this.lastWriteTime, dst, dstIndex);
			dstIndex += 8;
			SMBUtil.writeTime(this.changeTime, dst, dstIndex);
			dstIndex += 8;
			SMBUtil.writeInt4(this.attributes, dst, dstIndex);
			dstIndex += 4;
			dstIndex += 4;
			return dstIndex - start;
		}


		public override string ToString() {
			return "SmbQueryFileBasicInfo[" + "createTime=" + DateTimeOffset.FromUnixTimeMilliseconds(this.createTime) + ",lastAccessTime=" + DateTimeOffset.FromUnixTimeMilliseconds(this.lastAccessTime) + ",lastWriteTime=" + DateTimeOffset.FromUnixTimeMilliseconds(this.lastWriteTime) + ",changeTime=" + DateTimeOffset.FromUnixTimeMilliseconds(this.changeTime) + ",attributes=0x" + Hexdump.toHexString(this.attributes, 4) + "]";
		}
	}
}