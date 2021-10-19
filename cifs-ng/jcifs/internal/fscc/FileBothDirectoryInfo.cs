using System;
using Configuration = jcifs.Configuration;
using Decodable = jcifs.Decodable;
using SmbConstants = jcifs.SmbConstants;
using SMBProtocolDecodingException = jcifs.@internal.SMBProtocolDecodingException;
using SMBUtil = jcifs.@internal.util.SMBUtil;
using FileEntry = jcifs.smb.FileEntry;
using Strings = jcifs.util.Strings;

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
	public class FileBothDirectoryInfo : FileEntry, Decodable {

		private int nextEntryOffset;
		private int fileIndex;
		private long creationTime;
		private long lastAccessTime;
		private long lastWriteTime;
		private long changeTime;
		private long endOfFile;
		private long allocationSize;
		private int extFileAttributes;
		private int eaSize;
		private string shortName;
		private string filename;
		private readonly Configuration config;
		private readonly bool unicode;


		/// <param name="config"> </param>
		/// <param name="unicode">
		///  </param>
		public FileBothDirectoryInfo(Configuration config, bool unicode) {
			this.config = config;
			this.unicode = unicode;
		}


		public virtual string getName() {
			return this.filename;
		}


		public virtual int getType() {
			return SmbConstants.TYPE_FILESYSTEM;
		}


		/// <returns> the fileIndex </returns>
		public virtual int getFileIndex() {
			return this.fileIndex;
		}


		/// <returns> the filename </returns>
		public virtual string getFilename() {
			return this.filename;
		}


		public virtual int getAttributes() {
			return this.extFileAttributes;
		}


		public virtual long createTime() {
			return this.creationTime;
		}


		public virtual long lastModified() {
			return this.lastWriteTime;
		}


		public virtual long lastAccess() {
			return this.lastAccessTime;
		}


		public virtual long length() {
			return this.endOfFile;
		}


		/// <returns> the nextEntryOffset </returns>
		public virtual int getNextEntryOffset() {
			return this.nextEntryOffset;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Decodable#decode(byte[], int, int) </seealso>
		/// throws jcifs.internal.SMBProtocolDecodingException
		public virtual int decode(byte[] buffer, int bufferIndex, int len) {
			int start = bufferIndex;
			this.nextEntryOffset = SMBUtil.readInt4(buffer, bufferIndex);
			bufferIndex += 4;
			this.fileIndex = SMBUtil.readInt4(buffer, bufferIndex);
			bufferIndex += 4;
			this.creationTime = SMBUtil.readTime(buffer, bufferIndex);
			bufferIndex += 8;
			this.lastAccessTime = SMBUtil.readTime(buffer, bufferIndex);
			bufferIndex += 8;
			this.lastWriteTime = SMBUtil.readTime(buffer, bufferIndex);
			bufferIndex += 8;
			this.changeTime = SMBUtil.readTime(buffer, bufferIndex);
			bufferIndex += 8;
			this.endOfFile = SMBUtil.readInt8(buffer, bufferIndex);
			bufferIndex += 8;
			this.allocationSize = SMBUtil.readInt8(buffer, bufferIndex);
			bufferIndex += 8;
			this.extFileAttributes = SMBUtil.readInt4(buffer, bufferIndex);
			bufferIndex += 4;
			int fileNameLength = SMBUtil.readInt4(buffer, bufferIndex);
			bufferIndex += 4;
			this.eaSize = SMBUtil.readInt4(buffer, bufferIndex);
			bufferIndex += 4;

			int shortNameLength = buffer[bufferIndex] & 0xFF;
			bufferIndex += 2;

			this.shortName = Strings.fromUNIBytes(buffer, bufferIndex, shortNameLength);
			bufferIndex += 24;

			string str;
			if (this.unicode) {
				if (fileNameLength > 0 && buffer[bufferIndex + fileNameLength - 1] == (byte)'\0' && buffer[bufferIndex + fileNameLength - 2] == (byte)'\0') {
					fileNameLength -= 2;
				}
				str = Strings.fromUNIBytes(buffer, bufferIndex, fileNameLength);
			}
			else {
				if (fileNameLength > 0 && buffer[bufferIndex + fileNameLength - 1] == (byte)'\0') {
					fileNameLength -= 1;
				}
				str = Strings.fromOEMBytes(buffer, bufferIndex, fileNameLength, this.config);
			}
			this.filename = str;
			bufferIndex += fileNameLength;

			return start - bufferIndex;
		}


		public override string ToString() {
			return "SmbFindFileBothDirectoryInfo[" + "nextEntryOffset=" + this.nextEntryOffset + ",fileIndex=" + this.fileIndex + ",creationTime=" + DateTimeOffset.FromUnixTimeMilliseconds(this.creationTime) + ",lastAccessTime=" + DateTimeOffset.FromUnixTimeMilliseconds(this.lastAccessTime) + ",lastWriteTime=" + DateTimeOffset.FromUnixTimeMilliseconds(this.lastWriteTime) + ",changeTime=" + DateTimeOffset.FromUnixTimeMilliseconds(this.changeTime) + ",endOfFile=" + this.endOfFile + ",allocationSize=" + this.allocationSize + ",extFileAttributes=" + this.extFileAttributes + ",eaSize=" + this.eaSize + ",shortName=" + this.shortName + ",filename=" + this.filename + "]";
		}

	}
}