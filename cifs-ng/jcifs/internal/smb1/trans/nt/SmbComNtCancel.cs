using Configuration = jcifs.Configuration;
using ServerMessageBlock = jcifs.@internal.smb1.ServerMessageBlock;

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
namespace jcifs.@internal.smb1.trans.nt {



	/// <summary>
	/// @author mbechler
	/// 
	/// </summary>
	public class SmbComNtCancel : ServerMessageBlock {

		/// <param name="config"> </param>
		protected internal SmbComNtCancel(Configuration config, int mid) : base(config, SMB_COM_NT_CANCEL) {
			setMid(mid);
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb1.ServerMessageBlock#isCancel() </seealso>
		public override bool isCancel() {
			return true;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb1.ServerMessageBlock#writeParameterWordsWireFormat(byte[], int) </seealso>
		protected internal override int writeParameterWordsWireFormat(byte[] dst, int dstIndex) {
			return 0;
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
		protected internal override int readBytesWireFormat(byte[] buffer, int bufferIndex) {
			return 0;
		}

	}

}