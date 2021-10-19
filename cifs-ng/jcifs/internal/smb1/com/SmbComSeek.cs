using Configuration = jcifs.Configuration;
using SMBProtocolDecodingException = jcifs.@internal.SMBProtocolDecodingException;
using ServerMessageBlock = jcifs.@internal.smb1.ServerMessageBlock;
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
	public class SmbComSeek : ServerMessageBlock {

		/// <param name="config"> </param>
		/// <param name="fid"> </param>
		public SmbComSeek(Configuration config, int fid) : base(config, SMB_COM_SEEK) {
			this.fid = fid;
		}

		private int fid;
		private int mode;
		private long offset;


		/// <param name="fid">
		///            the fid to set </param>
		public virtual void setFid(int fid) {
			this.fid = fid;
		}


		/// <param name="mode">
		///            the mode to set </param>
		public void setMode(int mode) {
			this.mode = mode;
		}


		/// <param name="offset">
		///            the offset to set </param>
		public void setOffset(long offset) {
			this.offset = offset;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb1.ServerMessageBlock#writeParameterWordsWireFormat(byte[], int) </seealso>
		protected internal override int writeParameterWordsWireFormat(byte[] dst, int dstIndex) {
			int start = dstIndex;
			SMBUtil.writeInt2(this.fid, dst, dstIndex);
			dstIndex += 2;
			SMBUtil.writeInt2(this.mode, dst, dstIndex);
			dstIndex += 2;
			SMBUtil.writeInt4(this.offset, dst, dstIndex);
			dstIndex += 4;
			return dstIndex - start;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb1.ServerMessageBlock#writeBytesWireFormat(byte[], int) </seealso>
		protected internal override int writeBytesWireFormat(byte[] dst, int dstIndex) {
			return 0;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb1.ServerMessageBlock#readParameterWordsWireFormat(byte[], int) </seealso>
		protected internal override int readParameterWordsWireFormat(byte[] buffer, int bufferIndex) {
			return 0;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb1.ServerMessageBlock#readBytesWireFormat(byte[], int) </seealso>
		/// throws jcifs.internal.SMBProtocolDecodingException
		protected internal override int readBytesWireFormat(byte[] buffer, int bufferIndex) {
			return 0;
		}

	}

}