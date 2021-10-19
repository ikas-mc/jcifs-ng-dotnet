using System.IO;
using Configuration = jcifs.Configuration;
using NetbiosName = jcifs.NetbiosName;

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




	/// 
	public class SessionRequestPacket : SessionServicePacket {

		private Name calledName, callingName;


		internal SessionRequestPacket(Configuration config) {
			this.calledName = new Name(config);
			this.callingName = new Name(config);
		}


		/// 
		/// <param name="config"> </param>
		/// <param name="calledName"> </param>
		/// <param name="callingName"> </param>
		public SessionRequestPacket(Configuration config, NetbiosName calledName, NetbiosName callingName) {
			this.type = SESSION_REQUEST;
			this.calledName = new Name(config, calledName);
			this.callingName = new Name(config, callingName);
		}


		internal override int writeTrailerWireFormat(byte[] dst, int dstIndex) {
			int start = dstIndex;
			dstIndex += this.calledName.writeWireFormat(dst, dstIndex);
			dstIndex += this.callingName.writeWireFormat(dst, dstIndex);
			return dstIndex - start;
		}


		/// throws java.io.IOException
		internal override int readTrailerWireFormat(Stream @in, byte[] buffer, int bufferIndex) {
			int start = bufferIndex;
			if (@in.Read(buffer, bufferIndex, this.length) != this.length) {
				throw new IOException("invalid session request wire format");
			}
			bufferIndex += this.calledName.readWireFormat(buffer, bufferIndex);
			bufferIndex += this.callingName.readWireFormat(buffer, bufferIndex);
			return bufferIndex - start;
		}
	}

}