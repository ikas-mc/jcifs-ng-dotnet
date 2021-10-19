using Encodable = jcifs.Encodable;
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
	public class FileEndOfFileInformation : FileInformation, Encodable {

		private long endOfFile;


		/// 
		public FileEndOfFileInformation() {
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.fscc.FileInformation#getFileInformationLevel() </seealso>
		public virtual byte getFileInformationLevel() {
			return FileInformationConstants.FILE_ENDOFFILE_INFO;
		}


		/// 
		/// <param name="eofOfFile"> </param>
		public FileEndOfFileInformation(long eofOfFile) {
			this.endOfFile = eofOfFile;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Decodable#decode(byte[], int, int) </seealso>
		/// throws jcifs.internal.SMBProtocolDecodingException
		public virtual int decode(byte[] buffer, int bufferIndex, int len) {
			this.endOfFile = SMBUtil.readInt8(buffer, bufferIndex);
			return 8;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Encodable#size() </seealso>
		public virtual int size() {
			return 8;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Encodable#encode(byte[], int) </seealso>
		public virtual int encode(byte[] dst, int dstIndex) {
			SMBUtil.writeInt8(this.endOfFile, dst, dstIndex);
			return 8;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= java.lang.Object#toString() </seealso>
		public override string ToString() {
			return "EndOfFileInformation[endOfFile=" + this.endOfFile + "]";
		}

	}

}