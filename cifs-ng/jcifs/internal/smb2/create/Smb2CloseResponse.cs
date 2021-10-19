using Logger = org.slf4j.Logger;
using LoggerFactory = org.slf4j.LoggerFactory;
using Configuration = jcifs.Configuration;
using SMBProtocolDecodingException = jcifs.@internal.SMBProtocolDecodingException;
using SmbBasicFileInfo = jcifs.@internal.SmbBasicFileInfo;
using ServerMessageBlock2Response = jcifs.@internal.smb2.ServerMessageBlock2Response;
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
namespace jcifs.@internal.smb2.create {




	/// <summary>
	/// @author mbechler
	/// 
	/// </summary>
	public class Smb2CloseResponse : ServerMessageBlock2Response, SmbBasicFileInfo {

		private static readonly Logger log = LoggerFactory.getLogger(typeof(Smb2CloseResponse));

		/// 
		public const int SMB2_CLOSE_FLAG_POSTQUERY_ATTIB = 0x1;

		private readonly byte[] fileId;
		private readonly string fileName;
		private int closeFlags;
		private long creationTime;
		private long lastAccessTime;
		private long lastWriteTime;
		private long changeTime;
		private long allocationSize;
		private long endOfFile;
		private int fileAttributes;


		/// <param name="config"> </param>
		/// <param name="fileId"> </param>
		/// <param name="fileName"> </param>
		public Smb2CloseResponse(Configuration config, byte[] fileId, string fileName) : base(config) {
			this.fileId = fileId;
			this.fileName = fileName;
		}


		/// <returns> the closeFlags </returns>
		public int getCloseFlags() {
			return this.closeFlags;
		}


		/// <returns> the creationTime </returns>
		public long getCreationTime() {
			return this.creationTime;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.SmbBasicFileInfo#getCreateTime() </seealso>
		public long getCreateTime() {
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


		/// <returns> the changeTime </returns>
		public long getChangeTime() {
			return this.changeTime;
		}


		/// <returns> the allocationSize </returns>
		public long getAllocationSize() {
			return this.allocationSize;
		}


		/// <returns> the endOfFile </returns>
		public long getEndOfFile() {
			return this.endOfFile;
		}


		/// <returns> the fileId </returns>
		public virtual byte[] getFileId() {
			return this.fileId;
		}


		/// <returns> the fileName </returns>
		public virtual string getFileName() {
			return this.fileName;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.SmbBasicFileInfo#getSize() </seealso>
		public virtual long getSize() {
			return getEndOfFile();
		}


		/// <returns> the fileAttributes </returns>
		public virtual int getFileAttributes() {
			return this.fileAttributes;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.SmbBasicFileInfo#getAttributes() </seealso>
		public virtual int getAttributes() {
			return getFileAttributes();
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
		/// <seealso cref= jcifs.internal.smb2.ServerMessageBlock2#readBytesWireFormat(byte[], int) </seealso>
		/// throws jcifs.internal.SMBProtocolDecodingException
		protected  override int readBytesWireFormat(byte[] buffer, int bufferIndex) {
			int start = bufferIndex;
			int structureSize = SMBUtil.readInt2(buffer, bufferIndex);
			if (structureSize != 60) {
				throw new SMBProtocolDecodingException("Expected structureSize = 60");
			}
			this.closeFlags = SMBUtil.readInt2(buffer, bufferIndex + 2);
			bufferIndex += 4;
			bufferIndex += 4; // Reserved
			this.creationTime = SMBUtil.readTime(buffer, bufferIndex);
			bufferIndex += 8;
			this.lastAccessTime = SMBUtil.readTime(buffer, bufferIndex);
			bufferIndex += 8;
			this.lastWriteTime = SMBUtil.readTime(buffer, bufferIndex);
			bufferIndex += 8;
			this.changeTime = SMBUtil.readTime(buffer, bufferIndex);
			bufferIndex += 8;
			this.allocationSize = SMBUtil.readInt8(buffer, bufferIndex);
			bufferIndex += 8;
			this.endOfFile = SMBUtil.readInt8(buffer, bufferIndex);
			bufferIndex += 8;
			this.fileAttributes = SMBUtil.readInt4(buffer, bufferIndex);
			bufferIndex += 4;

			if (log.isDebugEnabled()) {
				log.debug(string.Format("Closed {0} ({1})", Hexdump.toHexString(this.fileId), this.fileName));
			}

			return bufferIndex - start;
		}

	}

}