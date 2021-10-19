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



	/// 
	public abstract class SessionServicePacket {

		// session service packet types
		internal const int SESSION_MESSAGE = 0x00;
		internal const int SESSION_REQUEST = 0x81;

		/// 
		public const int POSITIVE_SESSION_RESPONSE = 0x82;

		/// 
		public const int NEGATIVE_SESSION_RESPONSE = 0x83;
		internal const int SESSION_RETARGET_RESPONSE = 0x84;
		internal const int SESSION_KEEP_ALIVE = 0x85;

		internal const int MAX_MESSAGE_SIZE = 0x0001FFFF;
		internal const int HEADER_LENGTH = 4;


		internal static void writeInt2(int val, byte[] dst, int dstIndex) {
			dst[dstIndex++] = unchecked((byte)((val >> 8) & 0xFF));
			dst[dstIndex] = unchecked((byte)(val & 0xFF));
		}


		internal static void writeInt4(int val, byte[] dst, int dstIndex) {
			dst[dstIndex++] = unchecked((byte)((val >> 24) & 0xFF));
			dst[dstIndex++] = unchecked((byte)((val >> 16) & 0xFF));
			dst[dstIndex++] = unchecked((byte)((val >> 8) & 0xFF));
			dst[dstIndex] = unchecked((byte)(val & 0xFF));
		}


		internal static int readInt2(byte[] src, int srcIndex) {
			return ((src[srcIndex] & 0xFF) << 8) + (src[srcIndex + 1] & 0xFF);
		}


		internal static int readInt4(byte[] src, int srcIndex) {
			return ((src[srcIndex] & 0xFF) << 24) + ((src[srcIndex + 1] & 0xFF) << 16) + ((src[srcIndex + 2] & 0xFF) << 8) + (src[srcIndex + 3] & 0xFF);
		}


		internal static int readLength(byte[] src, int srcIndex) {
			srcIndex++;
			return ((src[srcIndex++] & 0x01) << 16) + ((src[srcIndex++] & 0xFF) << 8) + (src[srcIndex++] & 0xFF);
		}


		/// throws java.io.IOException
		internal static int readn(Stream @in, byte[] b, int off, int len) {
			int i = 0, n;

			while (i < len) {
				n = @in.Read(b, off + i, len - i);
				if (n <= 0) {
					break;
				}
				i += n;
			}

			return i;
		}


		/// throws java.io.IOException
		internal static int readPacketType(Stream @in, byte[] buffer, int bufferIndex) {
			int n;
			if ((n = readn(@in, buffer, bufferIndex, HEADER_LENGTH)) != HEADER_LENGTH) {
				if (n == -1) {
					return -1;
				}
				throw new IOException("unexpected EOF reading netbios session header");
			}
			int t = buffer[bufferIndex] & 0xFF;
			return t;
		}

		internal int type, length;


		/// <param name="dst"> </param>
		/// <param name="dstIndex"> </param>
		/// <returns> written bytes </returns>
		public virtual int writeWireFormat(byte[] dst, int dstIndex) {
			this.length = writeTrailerWireFormat(dst, dstIndex + HEADER_LENGTH);
			writeHeaderWireFormat(dst, dstIndex);
			return HEADER_LENGTH + this.length;
		}


		/// throws java.io.IOException
		internal virtual int readWireFormat(Stream @in, byte[] buffer, int bufferIndex) {
			readHeaderWireFormat(@in, buffer, bufferIndex);
			return HEADER_LENGTH + readTrailerWireFormat(@in, buffer, bufferIndex);
		}


		internal virtual int writeHeaderWireFormat(byte[] dst, int dstIndex) {
			dst[dstIndex++] = (byte) this.type;
			if (this.length > 0x0000FFFF) {
				dst[dstIndex] = (byte) 0x01;
			}
			dstIndex++;
			writeInt2(this.length, dst, dstIndex);
			return HEADER_LENGTH;
		}


		internal virtual int readHeaderWireFormat(Stream @in, byte[] buffer, int bufferIndex) {
			this.type = buffer[bufferIndex++] & 0xFF;
			this.length = ((buffer[bufferIndex] & 0x01) << 16) + readInt2(buffer, bufferIndex + 1);
			return HEADER_LENGTH;
		}


		internal abstract int writeTrailerWireFormat(byte[] dst, int dstIndex);


		/// throws java.io.IOException;
		internal abstract int readTrailerWireFormat(Stream @in, byte[] buffer, int bufferIndex);
	}

}