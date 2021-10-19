using Decodable = jcifs.Decodable;
using FileNotifyInformation = jcifs.FileNotifyInformation;
using SMBProtocolDecodingException = jcifs.@internal.SMBProtocolDecodingException;
using SMBUtil = jcifs.@internal.util.SMBUtil;
using Hexdump = jcifs.util.Hexdump;
using Strings = jcifs.util.Strings;

/*
 * © 2016 AgNO3 Gmbh & Co. KG
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

namespace jcifs.@internal.smb1.trans.nt {



	/// <summary>
	/// File notification information
	/// 
	/// 
	/// @author mbechler
	/// 
	/// </summary>
	public class FileNotifyInformationImpl : FileNotifyInformation, Decodable {

		internal int nextEntryOffset;
		internal int action;
		internal int fileNameLength;
		internal string fileName;


		/// 
		public FileNotifyInformationImpl() {
		}


		public virtual int getAction() {
			return this.action;
		}


		public virtual string getFileName() {
			return this.fileName;
		}


		/// <returns> the nextEntryOffset </returns>
		public virtual int getNextEntryOffset() {
			return this.nextEntryOffset;
		}


		/// 
		/// <param name="buffer"> </param>
		/// <param name="bufferIndex"> </param>
		/// <param name="len"> </param>
		/// <exception cref="IOException"> </exception>
		/// throws java.io.IOException
		public FileNotifyInformationImpl(byte[] buffer, int bufferIndex, int len) {
			decode(buffer, bufferIndex, len);
		}


		/// throws jcifs.internal.SMBProtocolDecodingException
		public virtual int decode(byte[] buffer, int bufferIndex, int len) {
			if (len == 0) {
				// nothing to do
				return 0;
			}
			int start = bufferIndex;

			this.nextEntryOffset = SMBUtil.readInt4(buffer, bufferIndex);
			bufferIndex += 4;

			if ((this.nextEntryOffset % 4) != 0) {
				throw new SMBProtocolDecodingException("Non aligned nextEntryOffset");
			}

			this.action = SMBUtil.readInt4(buffer, bufferIndex);
			bufferIndex += 4;

			this.fileNameLength = SMBUtil.readInt4(buffer, bufferIndex);
			bufferIndex += 4;

			this.fileName = Strings.fromUNIBytes(buffer, bufferIndex, this.fileNameLength);
			bufferIndex += this.fileNameLength;
			return bufferIndex - start;
		}


		public override string ToString() {
			string ret = "FileNotifyInformation[nextEntry=" + this.nextEntryOffset + ",action=0x" + Hexdump.toHexString(this.action, 4) + ",file=" + this.fileName + "]";
			return ret;
		}
	}

}