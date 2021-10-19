using System.Collections.Generic;
using Configuration = jcifs.Configuration;
using SMBProtocolDecodingException = jcifs.@internal.SMBProtocolDecodingException;
using FileBothDirectoryInfo = jcifs.@internal.fscc.FileBothDirectoryInfo;
using ServerMessageBlock2Response = jcifs.@internal.smb2.ServerMessageBlock2Response;
using Smb2Constants = jcifs.@internal.smb2.Smb2Constants;
using SMBUtil = jcifs.@internal.util.SMBUtil;
using FileEntry = jcifs.smb.FileEntry;

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
namespace jcifs.@internal.smb2.info {




	/// <summary>
	/// @author mbechler
	/// 
	/// </summary>
	public class Smb2QueryDirectoryResponse : ServerMessageBlock2Response {

		/// 
		public static readonly int OVERHEAD = Smb2Constants.SMB2_HEADER_LENGTH + 8;

		private readonly byte expectInfoClass;
		private FileEntry[] results;


		/// <param name="config"> </param>
		/// <param name="expectInfoClass"> </param>
		public Smb2QueryDirectoryResponse(Configuration config, byte expectInfoClass) : base(config) {
			this.expectInfoClass = expectInfoClass;
		}


		/// <returns> the fileInformation </returns>
		public virtual FileEntry[] getResults() {
			return this.results;
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

			if (structureSize != 9) {
				throw new SMBProtocolDecodingException("Expected structureSize = 9");
			}

			int bufferOffset = SMBUtil.readInt2(buffer, bufferIndex + 2) + getHeaderStart();
			bufferIndex += 4;
			int bufferLength = SMBUtil.readInt4(buffer, bufferIndex);
			bufferIndex += 4;

			// bufferIndex = bufferOffset;

			IList<FileEntry> infos = new List<FileEntry>();
			do {
				FileBothDirectoryInfo cur = createFileInfo();
				if (cur == null) {
					break;
				}
				cur.decode(buffer, bufferIndex, bufferLength);
				infos.Add(cur);
				int nextEntryOffset = cur.getNextEntryOffset();
				if (nextEntryOffset > 0) {
					bufferIndex += nextEntryOffset;
				}
				else {
					break;
				}
			} while (bufferIndex < bufferOffset + bufferLength);
			this.results = ((List<FileEntry>)infos).ToArray();
			return bufferIndex - start;
		}


		private FileBothDirectoryInfo createFileInfo() {
			if (this.expectInfoClass == Smb2QueryDirectoryRequest.FILE_BOTH_DIRECTORY_INFO) {
				return new FileBothDirectoryInfo(getConfig(), true);
			}
			return null;
		}

	}

}