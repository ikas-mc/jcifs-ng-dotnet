using Configuration = jcifs.Configuration;
using SMBProtocolDecodingException = jcifs.@internal.SMBProtocolDecodingException;
using AndXServerMessageBlock = jcifs.@internal.smb1.AndXServerMessageBlock;
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
	public class SmbComLockingAndX : AndXServerMessageBlock {

		private int fid;
		private byte typeOfLock;
		private byte newOpLockLevel;
		private long timeout;
		private LockingAndXRange[] locks;
		private LockingAndXRange[] unlocks;
		private bool largeFile;


		/// <param name="config"> </param>
		public SmbComLockingAndX(Configuration config) : base(config) {
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb1.ServerMessageBlock#writeParameterWordsWireFormat(byte[], int) </seealso>
		protected internal override int writeParameterWordsWireFormat(byte[] dst, int dstIndex) {
			int start = dstIndex;

			SMBUtil.writeInt2(this.fid, dst, dstIndex);
			dstIndex += 2;

			dst[dstIndex] = this.typeOfLock;
			dst[dstIndex + 1] = this.newOpLockLevel;
			dstIndex += 2;

			SMBUtil.writeInt4(this.timeout, dst, dstIndex);
			dstIndex += 4;

			SMBUtil.writeInt2(this.unlocks != null ? this.unlocks.Length : 0, dst, dstIndex);
			dstIndex += 2;

			SMBUtil.writeInt2(this.locks != null ? this.locks.Length : 0, dst, dstIndex);
			dstIndex += 2;
			return start - dstIndex;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb1.ServerMessageBlock#readParameterWordsWireFormat(byte[], int) </seealso>
		protected internal override int readParameterWordsWireFormat(byte[] buffer, int bufferIndex) {
			int start = bufferIndex;

			this.fid = SMBUtil.readInt2(buffer, bufferIndex);
			bufferIndex += 2;

			this.typeOfLock = buffer[bufferIndex];

			if ((this.typeOfLock & 0x10) == 0x10) {
				this.largeFile = true;
			}

			this.newOpLockLevel = buffer[bufferIndex + 1];
			bufferIndex += 2;

			this.timeout = SMBUtil.readInt4(buffer, bufferIndex);
			bufferIndex += 4;

			int nunlocks = SMBUtil.readInt2(buffer, bufferIndex);
			this.unlocks = new LockingAndXRange[nunlocks];
			bufferIndex += 2;

			int nlocks = SMBUtil.readInt2(buffer, bufferIndex);
			this.locks = new LockingAndXRange[nlocks];
			bufferIndex += 2;
			return start - bufferIndex;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb1.ServerMessageBlock#writeBytesWireFormat(byte[], int) </seealso>
		protected internal override int writeBytesWireFormat(byte[] dst, int dstIndex) {
			int start = dstIndex;
			if (this.unlocks != null) {
				foreach (LockingAndXRange lockingAndXRange in this.unlocks) {
					dstIndex += lockingAndXRange.encode(dst, dstIndex);
				}
			}
			if (this.locks != null) {
				foreach (LockingAndXRange lockingAndXRange in this.locks) {
					dstIndex += lockingAndXRange.encode(dst, dstIndex);
				}
			}
			return start - dstIndex;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb1.ServerMessageBlock#readBytesWireFormat(byte[], int) </seealso>
		/// throws jcifs.internal.SMBProtocolDecodingException
		protected internal override int readBytesWireFormat(byte[] buffer, int bufferIndex) {
			int start = bufferIndex;
			for (int i = 0; i < this.unlocks.Length; i++) {
				this.unlocks[i] = createLockRange();
				bufferIndex += this.unlocks[i].decode(buffer, bufferIndex, buffer.Length);
			}

			for (int i = 0; i < this.locks.Length; i++) {
				this.locks[i] = createLockRange();
				bufferIndex += this.locks[i].decode(buffer, bufferIndex, buffer.Length);
			}

			return start - bufferIndex;
		}


		/// <summary>
		/// @return
		/// </summary>
		private LockingAndXRange createLockRange() {
			return new LockingAndXRange(this.largeFile);
		}


		public override string ToString() {
			return "SmbComLockingAndX[" + base.ToString() + ",fid=" + this.fid + ",typeOfLock=" + this.typeOfLock + ",newOplockLevel=" + this.newOpLockLevel + "]";
		}

	}

}