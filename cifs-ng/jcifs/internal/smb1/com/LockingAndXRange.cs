using Decodable = jcifs.Decodable;
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
namespace jcifs.@internal.smb1.com {



	/// <summary>
	/// @author mbechler
	/// 
	/// </summary>
	public class LockingAndXRange : Encodable, Decodable {

		private readonly bool largeFile;
		private int pid;
		private long byteOffset;
		private long lengthInBytes;


		/// <param name="largeFile">
		///  </param>
		public LockingAndXRange(bool largeFile) {
			this.largeFile = largeFile;
		}


		/// 
		/// <returns> pid </returns>
		public virtual int getPid() {
			return this.pid;
		}


		/// 
		/// <returns> start byte offset </returns>
		public virtual long getByteOffset() {
			return this.byteOffset;
		}


		/// 
		/// <returns> byte length </returns>
		public virtual long getLengthInBytes() {
			return this.lengthInBytes;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Decodable#decode(byte[], int, int) </seealso>
		/// throws jcifs.internal.SMBProtocolDecodingException
		public virtual int decode(byte[] buffer, int bufferIndex, int len) {
			if (this.largeFile) {
				this.pid = SMBUtil.readInt2(buffer, bufferIndex);
				int boHigh = SMBUtil.readInt4(buffer, bufferIndex + 4);
				int boLow = SMBUtil.readInt4(buffer, bufferIndex + 8);

				this.byteOffset = (boHigh << 32) | boLow;

				int lHigh = SMBUtil.readInt4(buffer, bufferIndex + 12);
				int lLow = SMBUtil.readInt4(buffer, bufferIndex + 16);

				this.lengthInBytes = (lHigh << 32) | lLow;
				return 20;
			}
			this.pid = SMBUtil.readInt2(buffer, bufferIndex);
			this.byteOffset = SMBUtil.readInt4(buffer, bufferIndex + 2);
			this.lengthInBytes = SMBUtil.readInt4(buffer, bufferIndex + 6);
			return 10;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Encodable#encode(byte[], int) </seealso>
		public virtual int encode(byte[] dst, int dstIndex) {
			if (this.largeFile) {
				SMBUtil.writeInt2(this.pid, dst, dstIndex);
				SMBUtil.writeInt4(this.byteOffset >> 32, dst, dstIndex + 4);
				SMBUtil.writeInt4(this.byteOffset & 0xFFFFFFFF, dst, dstIndex + 8);
				SMBUtil.writeInt4(this.lengthInBytes >> 32, dst, dstIndex + 12);
				SMBUtil.writeInt4(this.lengthInBytes & 0xFFFFFFFF, dst, dstIndex + 16);
				return 20;
			}
			SMBUtil.writeInt2(this.pid, dst, dstIndex);
			SMBUtil.writeInt4(this.byteOffset, dst, dstIndex + 2);
			SMBUtil.writeInt4(this.lengthInBytes, dst, dstIndex + 6);
			return 10;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Encodable#size() </seealso>
		public virtual int size() {
			return this.largeFile ? 20 : 10;
		}

	}

}