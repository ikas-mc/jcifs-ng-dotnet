using System;
using System.Collections.Generic;
using Logger = org.slf4j.Logger;
using LoggerFactory = org.slf4j.LoggerFactory;
using Configuration = jcifs.Configuration;
using CommonServerMessageBlockRequest = jcifs.@internal.CommonServerMessageBlockRequest;
using SMBProtocolDecodingException = jcifs.@internal.SMBProtocolDecodingException;
using SmbBasicFileInfo = jcifs.@internal.SmbBasicFileInfo;
using RequestWithFileId = jcifs.@internal.smb2.RequestWithFileId;
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
	public class Smb2CreateResponse : ServerMessageBlock2Response, SmbBasicFileInfo {

		private static readonly Logger log = LoggerFactory.getLogger(typeof(Smb2CreateResponse));

		private byte oplockLevel;
		private byte openFlags;
		private int createAction;
		private long creationTime;
		private long lastAccessTime;
		private long lastWriteTime;
		private long changeTime;
		private long allocationSize;
		private long endOfFile;
		private int fileAttributes;
		private byte[] fileId = new byte[16];
		private CreateContextResponse[] createContexts;
		private readonly string fileName;


		/// <param name="config"> </param>
		/// <param name="name"> </param>
		public Smb2CreateResponse(Configuration config, string name) : base(config) {
			this.fileName = name;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb2.ServerMessageBlock2Response#prepare(jcifs.internal.CommonServerMessageBlockRequest) </seealso>
		public override void prepare(CommonServerMessageBlockRequest next) {
			if (isReceived() && (next is RequestWithFileId)) {
				((RequestWithFileId) next).setFileId(this.fileId);
			}
			base.prepare(next);
		}


		/// <returns> the oplockLevel </returns>
		public byte getOplockLevel() {
			return this.oplockLevel;
		}


		/// <returns> the flags </returns>
		public byte getOpenFlags() {
			return this.openFlags;
		}


		/// <returns> the createAction </returns>
		public int getCreateAction() {
			return this.createAction;
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


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.SmbBasicFileInfo#getSize() </seealso>
		public long getSize() {
			return getEndOfFile();
		}


		/// <returns> the fileAttributes </returns>
		public int getFileAttributes() {
			return this.fileAttributes;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.SmbBasicFileInfo#getAttributes() </seealso>
		public int getAttributes() {
			return getFileAttributes();
		}


		/// <returns> the fileId </returns>
		public byte[] getFileId() {
			return this.fileId;
		}


		/// <returns> the fileName </returns>
		public string getFileName() {
			return this.fileName;
		}


		/// <returns> the createContexts </returns>
		public virtual CreateContextResponse[] getCreateContexts() {
			return this.createContexts;
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

			if (structureSize != 89) {
				throw new SMBProtocolDecodingException("Structure size is not 89");
			}

			this.oplockLevel = buffer[bufferIndex + 2];
			this.openFlags = buffer[bufferIndex + 3];
			bufferIndex += 4;

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

			this.allocationSize = SMBUtil.readInt8(buffer, bufferIndex);
			bufferIndex += 8;
			this.endOfFile = SMBUtil.readInt8(buffer, bufferIndex);
			bufferIndex += 8;

			this.fileAttributes = SMBUtil.readInt4(buffer, bufferIndex);
			bufferIndex += 4;
			bufferIndex += 4; // Reserved2

			Array.Copy(buffer, bufferIndex, this.fileId, 0, 16);
			bufferIndex += 16;

			int createContextOffset = SMBUtil.readInt4(buffer, bufferIndex);
			bufferIndex += 4;
			int createContextLength = SMBUtil.readInt4(buffer, bufferIndex);
			bufferIndex += 4;

			if (createContextOffset > 0 && createContextLength > 0) {
				IList<CreateContextResponse> contexts = new List<CreateContextResponse>();
				int createContextStart = getHeaderStart() + createContextOffset;
				int next = 0;
				do {
					int cci = createContextStart;
					next = SMBUtil.readInt4(buffer, cci);
					cci += 4;

					int nameOffset = SMBUtil.readInt2(buffer, cci);
					int nameLength = SMBUtil.readInt2(buffer, cci + 2);
					cci += 4;

					int dataOffset = SMBUtil.readInt2(buffer, cci + 2);
					cci += 4;
					int dataLength = SMBUtil.readInt4(buffer, cci);
					cci += 4;

					byte[] nameBytes = new byte[nameLength];
					Array.Copy(buffer, createContextStart + nameOffset, nameBytes, 0, nameBytes.Length);
					cci = Math.Max(cci, createContextStart + nameOffset + nameLength);

					CreateContextResponse cc = createContext(nameBytes);
					if (cc != null) {
						cc.decode(buffer, createContextStart + dataOffset, dataLength);
						contexts.Add(cc);
					}

					cci = Math.Max(cci, createContextStart + dataOffset + dataLength);

					if (next > 0) {
						createContextStart += next;
					}
					bufferIndex = Math.Max(bufferIndex, cci);
				} while (next > 0);
				this.createContexts = ((List<CreateContextResponse>)contexts).ToArray();
			}

			if (log.isDebugEnabled()) {
				log.debug("Opened " + this.fileName + ": " + Hexdump.toHexString(this.fileId));
			}

			return bufferIndex - start;
		}


		/// <param name="nameBytes">
		/// @return </param>
		private static CreateContextResponse createContext(byte[] nameBytes) {
			return null;
		}

	}

}