using SMBProtocolDecodingException = jcifs.@internal.SMBProtocolDecodingException;
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
namespace jcifs.@internal.fscc {



	/// 
	public class FileStandardInfo : BasicFileInformation {

		private long allocationSize;
		private long endOfFile;
		private int numberOfLinks;
		private bool deletePending;
		private bool directory;


		public virtual byte getFileInformationLevel() {
			return FileInformationConstants.FILE_STANDARD_INFO;
		}


		public virtual int getAttributes() {
			return 0;
		}


		public virtual long getCreateTime() {
			return 0L;
		}


		public virtual long getLastWriteTime() {
			return 0L;
		}


		public virtual long getLastAccessTime() {
			return 0L;
		}


		public virtual long getSize() {
			return this.endOfFile;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Decodable#decode(byte[], int, int) </seealso>
		/// throws jcifs.internal.SMBProtocolDecodingException
		public virtual int decode(byte[] buffer, int bufferIndex, int len) {
			int start = bufferIndex;
			this.allocationSize = SMBUtil.readInt8(buffer, bufferIndex);
			bufferIndex += 8;
			this.endOfFile = SMBUtil.readInt8(buffer, bufferIndex);
			bufferIndex += 8;
			this.numberOfLinks = SMBUtil.readInt4(buffer, bufferIndex);
			bufferIndex += 4;
			this.deletePending = (buffer[bufferIndex++] & 0xFF) > 0;
			this.directory = (buffer[bufferIndex++] & 0xFF) > 0;
			return bufferIndex - start;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Encodable#size() </seealso>
		public virtual int size() {
			return 22;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Encodable#encode(byte[], int) </seealso>
		public virtual int encode(byte[] dst, int dstIndex) {
			int start = dstIndex;
			SMBUtil.writeInt8(this.allocationSize, dst, dstIndex);
			dstIndex += 8;
			SMBUtil.writeInt8(this.endOfFile, dst, dstIndex);
			dstIndex += 8;
			SMBUtil.writeInt4(this.numberOfLinks, dst, dstIndex);
			dstIndex += 4;
			dst[dstIndex++] = (byte)(this.deletePending ? 1 : 0);
			dst[dstIndex++] = (byte)(this.directory ? 1 : 0);
			return dstIndex - start;
		}


		public override string ToString() {
			return "SmbQueryInfoStandard[" + "allocationSize=" + this.allocationSize + ",endOfFile=" + this.endOfFile + ",numberOfLinks=" + this.numberOfLinks + ",deletePending=" + this.deletePending + ",directory=" + this.directory + "]";
		}
	}
}