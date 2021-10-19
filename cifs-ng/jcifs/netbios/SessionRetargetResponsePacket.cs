using System.IO;

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

namespace jcifs.netbios {



	internal class SessionRetargetResponsePacket : SessionServicePacket {

		internal SessionRetargetResponsePacket() {
			this.type = SESSION_RETARGET_RESPONSE;
			this.length = 6;
		}


		internal override int writeTrailerWireFormat(byte[] dst, int dstIndex) {
			return 0;
		}


		/// throws java.io.IOException
		internal override int readTrailerWireFormat(Stream @in, byte[] buffer, int bufferIndex) {
			if (@in.Read(buffer, bufferIndex, this.length) != this.length) {
				throw new IOException("unexpected EOF reading netbios retarget session response");
			}
			int addr = readInt4(buffer, bufferIndex);
			bufferIndex += 4;
			new NbtAddress(null, addr, false, NbtAddress.B_NODE);
			readInt2(buffer, bufferIndex);
			return this.length;
		}
	}

}