using System;
using Configuration = jcifs.Configuration;
using AndXServerMessageBlock = jcifs.@internal.smb1.AndXServerMessageBlock;
using ServerMessageBlock = jcifs.@internal.smb1.ServerMessageBlock;
using SMBUtil = jcifs.@internal.util.SMBUtil;

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
	public class SmbComSessionSetupAndXResponse : AndXServerMessageBlock {

		private string nativeOs = "";
		private string nativeLanMan = "";
		private string primaryDomain = "";

		private bool isLoggedInAsGuestField;
		private byte[] blob = null;


		/// 
		/// <param name="config"> </param>
		/// <param name="andx"> </param>
		public SmbComSessionSetupAndXResponse(Configuration config, ServerMessageBlock andx) : base(config, andx) {
		}


		/// <returns> the nativeLanMan </returns>
		public string getNativeLanMan() {
			return this.nativeLanMan;
		}


		/// <returns> the nativeOs </returns>
		public string getNativeOs() {
			return this.nativeOs;
		}


		/// <returns> the primaryDomain </returns>
		public string getPrimaryDomain() {
			return this.primaryDomain;
		}


		/// <returns> the isLoggedInAsGuest </returns>
		public bool isLoggedInAsGuest() {
			return this.isLoggedInAsGuestField;
		}


		/// <returns> the blob </returns>
		public byte[] getBlob() {
			return this.blob;
		}


		protected internal override int writeParameterWordsWireFormat(byte[] dst, int dstIndex) {
			return 0;
		}


		protected internal override int writeBytesWireFormat(byte[] dst, int dstIndex) {
			return 0;
		}


		protected internal override int readParameterWordsWireFormat(byte[] buffer, int bufferIndex) {
			int start = bufferIndex;
			this.isLoggedInAsGuestField = (buffer[bufferIndex] & 0x01) == 0x01 ? true : false;
			bufferIndex += 2;
			if (this.isExtendedSecurity()) {
				int blobLength = SMBUtil.readInt2(buffer, bufferIndex);
				bufferIndex += 2;
				this.blob = new byte[blobLength];
			}
			return bufferIndex - start;
		}


		protected internal override int readBytesWireFormat(byte[] buffer, int bufferIndex) {
			int start = bufferIndex;

			if (this.isExtendedSecurity()) {
				Array.Copy(buffer, bufferIndex, this.blob, 0, this.blob.Length);
				bufferIndex += this.blob.Length;
			}

			this.nativeOs = readString(buffer, bufferIndex);
			bufferIndex += stringWireLength(this.nativeOs, bufferIndex);
			this.nativeLanMan = readString(buffer, bufferIndex, start + this.byteCount, 255, this.isUseUnicode());
			bufferIndex += stringWireLength(this.nativeLanMan, bufferIndex);
			if (!this.isExtendedSecurity()) {
				this.primaryDomain = readString(buffer, bufferIndex, start + this.byteCount, 255, this.isUseUnicode());
				bufferIndex += stringWireLength(this.primaryDomain, bufferIndex);
			}

			return bufferIndex - start;
		}


		public override string ToString() {
			string result = "SmbComSessionSetupAndXResponse[" + base.ToString() + ",isLoggedInAsGuest=" + this.isLoggedInAsGuestField + ",nativeOs=" + this.nativeOs + ",nativeLanMan=" + this.nativeLanMan + ",primaryDomain=" + this.primaryDomain + "]";
			return result;
		}
	}

}