using SmbConstants = jcifs.SmbConstants;

/*
 * © 2016 AgNO3 Gmbh & Co. KG
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
namespace jcifs.@internal.util {



	/// <summary>
	/// @author mbechler
	/// 
	/// </summary>
	public class SMBUtil {

		public static void writeInt2(long val, byte[] dst, int dstIndex) {
			dst[dstIndex] = (byte)(val);
			dst[++dstIndex] = (byte)(val >> 8);
		}


		public static void writeInt4(long val, byte[] dst, int dstIndex) {
			dst[dstIndex] = (byte)(val);
			dst[++dstIndex] = (byte)(val >>= 8);
			dst[++dstIndex] = (byte)(val >>= 8);
			dst[++dstIndex] = (byte)(val >> 8);
		}


		public static int readInt2(byte[] src, int srcIndex) {
			return (src[srcIndex] & 0xFF) + ((src[srcIndex + 1] & 0xFF) << 8);
		}


		public static int readInt4(byte[] src, int srcIndex) {
			return (src[srcIndex] & 0xFF) + ((src[srcIndex + 1] & 0xFF) << 8) + ((src[srcIndex + 2] & 0xFF) << 16) + ((src[srcIndex + 3] & 0xFF) << 24);
		}


		public static long readInt8(byte[] src, int srcIndex) {
			return (readInt4(src, srcIndex) & 0xFFFFFFFFL) + ((long)(readInt4(src, srcIndex + 4)) << 32);
		}


		public static void writeInt8(long val, byte[] dst, int dstIndex) {
			dst[dstIndex] = (byte)(val);
			dst[++dstIndex] = (byte)(val >>= 8);
			dst[++dstIndex] = (byte)(val >>= 8);
			dst[++dstIndex] = (byte)(val >>= 8);
			dst[++dstIndex] = (byte)(val >>= 8);
			dst[++dstIndex] = (byte)(val >>= 8);
			dst[++dstIndex] = (byte)(val >>= 8);
			dst[++dstIndex] = (byte)(val >> 8);
		}


		public static long readTime(byte[] src, int srcIndex) {
			int low = readInt4(src, srcIndex);
			int hi = readInt4(src, srcIndex + 4);
			long t = ((long) hi << 32) | (low & 0xFFFFFFFFL);
			t = (t / 10000L - SmbConstants.MILLISECONDS_BETWEEN_1970_AND_1601);
			return t;
		}


		public static void writeTime(long t, byte[] dst, int dstIndex) {
			if (t != 0L) {
				t = (t + SmbConstants.MILLISECONDS_BETWEEN_1970_AND_1601) * 10000L;
			}
			writeInt8(t, dst, dstIndex);
		}


		public static long readUTime(byte[] buffer, int bufferIndex) {
			return (readInt4(buffer, bufferIndex) & 0xFFFFFFFFL) * 1000L;
		}


		public static void writeUTime(long t, byte[] dst, int dstIndex) {
			writeInt4(t / 1000, dst, dstIndex);
		}

		public static readonly byte[] SMB_HEADER = new byte[] {unchecked((byte) 0xFF), (byte) 'S', (byte) 'M', (byte) 'B', (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00};

		public static readonly byte[] SMB2_HEADER = new byte[] {unchecked((byte) 0xFE), (byte) 'S', (byte) 'M', (byte) 'B', (byte) 64, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00};

	}

}