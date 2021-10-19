using System;
using System.Text;
using cifs_ng.lib.ext;
using Configuration = jcifs.Configuration;
using TreeConnectResponse = jcifs.@internal.TreeConnectResponse;
using AndXServerMessageBlock = jcifs.@internal.smb1.AndXServerMessageBlock;
using ServerMessageBlock = jcifs.@internal.smb1.ServerMessageBlock;

/* jcifs smb client library in Java
 * Copyright (C) 2000  "Michael B. Allen" <jcifs at samba dot org>
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



	/// 
	/// <summary>
	/// @author mbechler
	/// 
	/// </summary>
	public class SmbComTreeConnectAndXResponse : AndXServerMessageBlock, TreeConnectResponse {

		private const int SMB_SUPPORT_SEARCH_BITS = 0x0001;
		private const int SMB_SHARE_IS_IN_DFS = 0x0002;

		private bool supportSearchBits, shareIsInDfs;
		private string service, nativeFileSystem = "";


		/// 
		/// <param name="config"> </param>
		/// <param name="andx"> </param>
		public SmbComTreeConnectAndXResponse(Configuration config, ServerMessageBlock andx) : base(config, andx) {
		}


		/// <returns> the service </returns>
		public string getService() {
			return this.service;
		}


		/// <returns> the nativeFileSystem </returns>
		public string getNativeFileSystem() {
			return this.nativeFileSystem;
		}


		/// <returns> the supportSearchBits </returns>
		public bool isSupportSearchBits() {
			return this.supportSearchBits;
		}


		/// <returns> the shareIsInDfs </returns>
		public bool isShareDfs() {
			return this.shareIsInDfs;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.TreeConnectResponse#isValidTid() </seealso>
		public virtual bool isValidTid() {
			return getTid() != 0xFFFF;
		}


		protected internal override int writeParameterWordsWireFormat(byte[] dst, int dstIndex) {
			return 0;
		}


		protected internal override int writeBytesWireFormat(byte[] dst, int dstIndex) {
			return 0;
		}


		protected internal override int readParameterWordsWireFormat(byte[] buffer, int bufferIndex) {
			this.supportSearchBits = (buffer[bufferIndex] & SMB_SUPPORT_SEARCH_BITS) == SMB_SUPPORT_SEARCH_BITS;
			this.shareIsInDfs = (buffer[bufferIndex] & SMB_SHARE_IS_IN_DFS) == SMB_SHARE_IS_IN_DFS;
			return 2;
		}


		protected internal override int readBytesWireFormat(byte[] buffer, int bufferIndex) {
			int start = bufferIndex;

			int len = readStringLength(buffer, bufferIndex, 32);
			try {
				//TODO 
				this.service = Encoding.ASCII.GetString(buffer, bufferIndex, len);//"ASCII"
			}
			catch (Exception) {
				return 0;
			}
			bufferIndex += len + 1;
			// win98 observed not returning nativeFileSystem
			/*
			 * Problems here with iSeries returning ASCII even though useUnicode = true
			 * Fortunately we don't really need nativeFileSystem for anything.
			 * if( byteCount > bufferIndex - start ) {
			 * nativeFileSystem = readString( buffer, bufferIndex );
			 * bufferIndex += stringWireLength( nativeFileSystem, bufferIndex );
			 * }
			 */

			return bufferIndex - start;
		}


		public override string ToString() {
			string result = "SmbComTreeConnectAndXResponse[" + base.ToString() + ",supportSearchBits=" + this.supportSearchBits + ",shareIsInDfs=" + this.shareIsInDfs + ",service=" + this.service + ",nativeFileSystem=" + this.nativeFileSystem + "]";
			return result;
		}
	}

}