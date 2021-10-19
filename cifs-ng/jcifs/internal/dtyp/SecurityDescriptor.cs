using SMBUtil = jcifs.@internal.util.SMBUtil;

/* jcifs smb client library in Java
 * Copyright (C) 2005  "Michael B. Allen" <jcifs at samba dot org>
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

namespace jcifs.@internal.dtyp {



	/// <summary>
	/// Internal use only
	/// 
	/// @internal
	/// </summary>
	public class SecurityDescriptor : SecurityInfo {

		/// <summary>
		/// Descriptor type
		/// </summary>
		private int type;

		/// <summary>
		/// ACEs
		/// </summary>
		private ACE[] aces;
		private jcifs.smb.SID ownerUserSid, ownerGroupSid;


		/// 
		public SecurityDescriptor() {
		}


		/// <param name="buffer"> </param>
		/// <param name="bufferIndex"> </param>
		/// <param name="len"> </param>
		/// <exception cref="IOException"> </exception>
		/// throws java.io.IOException
		public SecurityDescriptor(byte[] buffer, int bufferIndex, int len) {
			this.decode(buffer, bufferIndex, len);
		}


		/// <returns> the type </returns>
		public int getType() {
			return this.type;
		}


		/// <returns> the aces </returns>
		public ACE[] getAces() {
			return this.aces;
		}


		/// <returns> the ownerGroupSid </returns>
		public SID getOwnerGroupSid() {
			return this.ownerGroupSid;
		}


		/// <returns> the ownerUserSid </returns>
		public SID getOwnerUserSid() {
			return this.ownerUserSid;
		}


		/// 
		/// <param name="buffer"> </param>
		/// <param name="bufferIndex"> </param>
		/// <param name="len"> </param>
		/// <returns> decoded data length </returns>
		/// <exception cref="SMBProtocolDecodingException"> </exception>
		/// throws jcifs.internal.SMBProtocolDecodingException
		public virtual int decode(byte[] buffer, int bufferIndex, int len) {
			int start = bufferIndex;

			bufferIndex++; // revision
			bufferIndex++;
			this.type = SMBUtil.readInt2(buffer, bufferIndex);
			bufferIndex += 2;
			int ownerUOffset = SMBUtil.readInt4(buffer, bufferIndex); // offset to owner sid
			bufferIndex += 4;
			int ownerGOffset = SMBUtil.readInt4(buffer, bufferIndex); // offset to group sid
			bufferIndex += 4;
			SMBUtil.readInt4(buffer, bufferIndex); // offset to sacl
			bufferIndex += 4;
			int daclOffset = SMBUtil.readInt4(buffer, bufferIndex);

			if (ownerUOffset > 0) {
				bufferIndex = start + ownerUOffset;
				this.ownerUserSid = new jcifs.smb.SID(buffer, bufferIndex);
				bufferIndex += 8 + 4 * this.ownerUserSid.sub_authority_count;
			}

			if (ownerGOffset > 0) {
				bufferIndex = start + ownerGOffset;
				this.ownerGroupSid = new jcifs.smb.SID(buffer, bufferIndex);
				bufferIndex += 8 + 4 * this.ownerGroupSid.sub_authority_count;
			}

			bufferIndex = start + daclOffset;

			if (daclOffset > 0) {
				bufferIndex++; // revision
				bufferIndex++;
				SMBUtil.readInt2(buffer, bufferIndex);
				bufferIndex += 2;
				int numAces = SMBUtil.readInt4(buffer, bufferIndex);
				bufferIndex += 4;

				if (numAces > 4096) {
					throw new SMBProtocolDecodingException("Invalid SecurityDescriptor");
				}

				this.aces = new ACE[numAces];
				for (int i = 0; i < numAces; i++) {
					this.aces[i] = new ACE();
					bufferIndex += this.aces[i].decode(buffer, bufferIndex, len - bufferIndex);
				}
			}
			else {
				this.aces = null;
			}

			return bufferIndex - start;
		}


		public override string ToString() {
			string ret = "SecurityDescriptor:\n";
			if (this.aces != null) {
				for (int ai = 0; ai < this.aces.Length; ai++) {
					ret += this.aces[ai].ToString() + "\n";
				}
			}
			else {
				ret += "NULL";
			}
			return ret;
		}
	}

}