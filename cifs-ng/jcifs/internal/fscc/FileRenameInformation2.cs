using System;
using System.Text;
using cifs_ng.lib.ext;
using jcifs.util;
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



	/// <summary>
	/// @author mbechler
	/// 
	/// </summary>
	public class FileRenameInformation2 : FileInformation {

		private bool replaceIfExists;
		private string fileName;


		/// 
		public FileRenameInformation2() {
		}


		/// 
		/// <param name="name"> </param>
		/// <param name="replaceIfExists"> </param>
		public FileRenameInformation2(string name, bool replaceIfExists) {
			this.fileName = name;
			this.replaceIfExists = replaceIfExists;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Decodable#decode(byte[], int, int) </seealso>
		/// throws jcifs.internal.SMBProtocolDecodingException
		public virtual int decode(byte[] buffer, int bufferIndex, int len) {
			int start = bufferIndex;
			this.replaceIfExists = buffer[bufferIndex] != 0;
			bufferIndex += 8;
			bufferIndex += 8;

			int nameLen = SMBUtil.readInt4(buffer, bufferIndex);
			bufferIndex += 4;
			byte[] nameBytes = new byte[nameLen];
			Array.Copy(buffer, bufferIndex, nameBytes, 0, nameBytes.Length);
			bufferIndex += nameLen;
			this.fileName = Strings.UTF_16LE_ENCODING.GetString(nameBytes);//StandardCharsets.UTF_16LE
			return bufferIndex - start;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Encodable#encode(byte[], int) </seealso>
		public virtual int encode(byte[] dst, int dstIndex) {
			int start = dstIndex;
			dst[dstIndex] = (byte)(this.replaceIfExists ? 1 : 0);
			dstIndex += 8; // 7 Reserved
			dstIndex += 8; // RootDirectory = 0

			byte[] nameBytes = this.fileName.getBytes(Encoding.Unicode);

			SMBUtil.writeInt4(nameBytes.Length, dst, dstIndex);
			dstIndex += 4;

			Array.Copy(nameBytes, 0, dst, dstIndex, nameBytes.Length);
			dstIndex += nameBytes.Length;

			return dstIndex - start;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Encodable#size() </seealso>
		public virtual int size() {
			return 20 + 2 * this.fileName.Length;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.fscc.FileInformation#getFileInformationLevel() </seealso>
		public virtual byte getFileInformationLevel() {
			return FileInformationConstants.FILE_RENAME_INFO;
		}

	}

}