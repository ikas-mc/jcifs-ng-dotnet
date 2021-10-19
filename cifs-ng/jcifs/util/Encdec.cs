using System;
using System.IO;
using SmbConstants = jcifs.SmbConstants;

/* encdec - encode and decode integers, times, and
 * internationalized strings to and from popular binary formats
 * http://www.ioplex.com/~miallen/encdec/
 * Copyright (c) 2003 Michael B. Allen <mballen@erols.com>
 *
 * The GNU Library General Public License
 * 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Library General Public
 * License as published by the Free Software Foundation; either
 * version 2 of the License, or (at your option) any later version.
 * 
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Library General Public License for more details.
 * 
 * You should have received a copy of the GNU Library General Public
 * License along with this library; if not, write to the Free
 * Software Foundation, Inc., 59 Temple Place - Suite 330, Boston,
 * MA 02111-1307, USA
 */

namespace jcifs.util {




	public sealed class Encdec {

		private const long SEC_BETWEEEN_1904_AND_1970 = 2082844800L;
		private const int TIME_1970_SEC_32BE = 1;
		private const int TIME_1970_SEC_32LE = 2;
		private const int TIME_1904_SEC_32BE = 3;
		private const int TIME_1904_SEC_32LE = 4;
		private const int TIME_1601_NANOS_64LE = 5;
		private const int TIME_1601_NANOS_64BE = 6;
		private const int TIME_1970_MILLIS_64BE = 7;
		private const int TIME_1970_MILLIS_64LE = 8;


		/// 
		private Encdec() {
		}


		/*
		 * Encode integers
		 */

		public static int enc_uint16be(short s, byte[] dst, int di) {
			dst[di++] = unchecked((byte)((s >> 8) & 0xFF));
			dst[di] = unchecked((byte)(s & 0xFF));
			return 2;
		}


		public static int enc_uint32be(int i, byte[] dst, int di) {
			dst[di++] = unchecked((byte)((i >> 24) & 0xFF));
			dst[di++] = unchecked((byte)((i >> 16) & 0xFF));
			dst[di++] = unchecked((byte)((i >> 8) & 0xFF));
			dst[di] = unchecked((byte)(i & 0xFF));
			return 4;
		}


		public static int enc_uint16le(short s, byte[] dst, int di) {
			dst[di++] = unchecked((byte)(s & 0xFF));
			dst[di] = unchecked((byte)((s >> 8) & 0xFF));
			return 2;
		}


		public static int enc_uint32le(int i, byte[] dst, int di) {
			dst[di++] = unchecked((byte)(i & 0xFF));
			dst[di++] = unchecked((byte)((i >> 8) & 0xFF));
			dst[di++] = unchecked((byte)((i >> 16) & 0xFF));
			dst[di] = unchecked((byte)((i >> 24) & 0xFF));
			return 4;
		}


		/*
		 * Decode integers
		 */

		public static short dec_uint16be(byte[] src, int si) {
			return (short)(((src[si] & 0xFF) << 8) | (src[si + 1] & 0xFF));
		}


		public static int dec_uint32be(byte[] src, int si) {
			return ((src[si] & 0xFF) << 24) | ((src[si + 1] & 0xFF) << 16) | ((src[si + 2] & 0xFF) << 8) | (src[si + 3] & 0xFF);
		}


		public static short dec_uint16le(byte[] src, int si) {
			return (short)((src[si] & 0xFF) | ((src[si + 1] & 0xFF) << 8));
		}


		public static int dec_uint32le(byte[] src, int si) {
			return (src[si] & 0xFF) | ((src[si + 1] & 0xFF) << 8) | ((src[si + 2] & 0xFF) << 16) | ((src[si + 3] & 0xFF) << 24);
		}


		/*
		 * Encode and decode 64 bit integers
		 */

		public static int enc_uint64be(long l, byte[] dst, int di) {
			enc_uint32be(unchecked((int)(l & 0xFFFFFFFFL)), dst, di + 4);
			enc_uint32be(unchecked((int)((l >> 32) & 0xFFFFFFFFL)), dst, di);
			return 8;
		}


		public static int enc_uint64le(long l, byte[] dst, int di) {
			enc_uint32le(unchecked((int)(l & 0xFFFFFFFFL)), dst, di);
			enc_uint32le(unchecked((int)((l >> 32) & 0xFFFFFFFFL)), dst, di + 4);
			return 8;
		}


		public static long dec_uint64be(byte[] src, int si) {
			long l;
			l = dec_uint32be(src, si) & 0xFFFFFFFFL;
			l <<= 32;
			l |= dec_uint32be(src, si + 4) & 0xFFFFFFFFL;
			return l;
		}


		public static long dec_uint64le(byte[] src, int si) {
			long l;
			l = dec_uint32le(src, si + 4) & 0xFFFFFFFFL;
			l <<= 32;
			l |= dec_uint32le(src, si) & 0xFFFFFFFFL;
			return l;
		}


		/*
		 * Encode floats
		 */

		public static int enc_floatle(float f, byte[] dst, int di) {
			return enc_uint32le((int)f, dst, di);
		}


		public static int enc_floatbe(float f, byte[] dst, int di) {
			return enc_uint32be((int)f, dst, di);
		}


		/*
		 * Decode floating point numbers
		 */

		public static float dec_floatle(byte[] src, int si) {
			return (float)(dec_uint32le(src, si));
		}


		public static float dec_floatbe(byte[] src, int si) {
			return (float)(dec_uint32be(src, si));
		}


		/*
		 * Encode and decode doubles
		 */

		public static int enc_doublele(double d, byte[] dst, int di) {
			return enc_uint64le(System.BitConverter.DoubleToInt64Bits(d), dst, di);
		}


		public static int enc_doublebe(double d, byte[] dst, int di) {
			return enc_uint64be(System.BitConverter.DoubleToInt64Bits(d), dst, di);
		}


		public static double dec_doublele(byte[] src, int si) {
			return (double)(dec_uint64le(src, si));
		}


		public static double dec_doublebe(byte[] src, int si) {
			return (double)(dec_uint64be(src, si));
		}


		/*
		 * Encode times
		 */

		public static int enc_time(DateTimeOffset date, byte[] dst, int di, int enc) {
			long t;

			switch (enc) {
			case TIME_1970_SEC_32BE:
				return enc_uint32be((int)(date.ToUnixTimeSeconds()), dst, di);
			case TIME_1970_SEC_32LE:
				return enc_uint32le((int)(date.ToUnixTimeSeconds()), dst, di);
			case TIME_1904_SEC_32BE:
				return enc_uint32be(unchecked((int)((date.ToUnixTimeSeconds() + SEC_BETWEEEN_1904_AND_1970) & 0xFFFFFFFF)), dst, di);
			case TIME_1904_SEC_32LE:
				return enc_uint32le(unchecked((int)((date.ToUnixTimeSeconds() + SEC_BETWEEEN_1904_AND_1970) & 0xFFFFFFFF)), dst, di);
			case TIME_1601_NANOS_64BE:
				t = (date.ToUnixTimeMilliseconds() + SmbConstants.MILLISECONDS_BETWEEN_1970_AND_1601) * 10000L;
				return enc_uint64be(t, dst, di);
			case TIME_1601_NANOS_64LE:
				t = (date.ToUnixTimeMilliseconds() + SmbConstants.MILLISECONDS_BETWEEN_1970_AND_1601) * 10000L;
				return enc_uint64le(t, dst, di);
			case TIME_1970_MILLIS_64BE:
				return enc_uint64be(date.ToUnixTimeMilliseconds(), dst, di);
			case TIME_1970_MILLIS_64LE:
				return enc_uint64le(date.ToUnixTimeMilliseconds(), dst, di);
			default:
				throw new System.ArgumentException("Unsupported time encoding");
			}
		}


		/*
		 * Decode times
		 */

		public static DateTimeOffset dec_time(byte[] src, int si, int enc) {
			long t;

			switch (enc) {
			case TIME_1970_SEC_32BE:
				return DateTimeOffset.FromUnixTimeSeconds(dec_uint32be(src, si));
			case TIME_1970_SEC_32LE:
				return DateTimeOffset.FromUnixTimeSeconds(dec_uint32le(src, si));
			case TIME_1904_SEC_32BE:
				return DateTimeOffset.FromUnixTimeSeconds(((dec_uint32be(src, si) & 0xFFFFFFFFL) - SEC_BETWEEEN_1904_AND_1970));
			case TIME_1904_SEC_32LE:
				return DateTimeOffset.FromUnixTimeSeconds(((dec_uint32le(src, si) & 0xFFFFFFFFL) - SEC_BETWEEEN_1904_AND_1970));
			case TIME_1601_NANOS_64BE:
				t = dec_uint64be(src, si);
				return DateTimeOffset.FromUnixTimeMilliseconds(t / 10000L - SmbConstants.MILLISECONDS_BETWEEN_1970_AND_1601);
			case TIME_1601_NANOS_64LE:
				t = dec_uint64le(src, si);
				return DateTimeOffset.FromUnixTimeMilliseconds(t / 10000L - SmbConstants.MILLISECONDS_BETWEEN_1970_AND_1601);
			case TIME_1970_MILLIS_64BE:
				return DateTimeOffset.FromUnixTimeMilliseconds(dec_uint64be(src, si));
			case TIME_1970_MILLIS_64LE:
				return DateTimeOffset.FromUnixTimeMilliseconds(dec_uint64le(src, si));
			default:
				throw new System.ArgumentException("Unsupported time encoding");
			}
		}


		public static int enc_utf8(string str, byte[] dst, int di, int dlim) {
			int start = di, ch;
			int strlen = str.Length;

			for (int i = 0; di < dlim && i < strlen; i++) {
				ch = str[i];
				if ((ch >= 0x0001) && (ch <= 0x007F)) {
					dst[di++] = (byte) ch;
				}
				else if (ch > 0x07FF) {
					if ((dlim - di) < 3) {
						break;
					}
					dst[di++] = unchecked((byte)(0xE0 | ((ch >> 12) & 0x0F)));
					dst[di++] = unchecked((byte)(0x80 | ((ch >> 6) & 0x3F)));
					dst[di++] = unchecked((byte)(0x80 | ((ch >> 0) & 0x3F)));
				}
				else {
					if ((dlim - di) < 2) {
						break;
					}
					dst[di++] = unchecked((byte)(0xC0 | ((ch >> 6) & 0x1F)));
					dst[di++] = unchecked((byte)(0x80 | ((ch >> 0) & 0x3F)));
				}
			}

			return di - start;
		}


		/// throws java.io.IOException
		public static string dec_utf8(byte[] src, int si, int slim) {
			char[] uni = new char[slim - si];
			int ui, ch;

			for (ui = 0; si < slim && (ch = src[si++] & 0xFF) != 0; ui++) {
				if (ch < 0x80) {
					uni[ui] = (char) ch;
				}
				else if ((ch & 0xE0) == 0xC0) {
					if ((slim - si) < 2) {
						break;
					}
					uni[ui] = (char)((ch & 0x1F) << 6);
					ch = src[si++] & 0xFF;
					uni[ui] |= (char) (ch & 0x3F);
					if ((ch & 0xC0) != 0x80 || uni[ui] < (char)0x80) {
						throw new IOException("Invalid UTF-8 sequence");
					}
				}
				else if ((ch & 0xF0) == 0xE0) {
					if ((slim - si) < 3) {
						break;
					}
					uni[ui] = (char)((ch & 0x0F) << 12);
					ch = src[si++] & 0xFF;
					if ((ch & 0xC0) != 0x80) {
						throw new IOException("Invalid UTF-8 sequence");
					}
					uni[ui] |=(char) ((ch & 0x3F) << 6);
					ch = src[si++] & 0xFF;
					uni[ui] |= (char) (ch & 0x3F);
					if ((ch & 0xC0) != 0x80 || uni[ui] < (char)0x800) {
						throw new IOException("Invalid UTF-8 sequence");
					}
				}
				else {
					throw new IOException("Unsupported UTF-8 sequence");
				}
			}

			return new string(uni, 0, ui);
		}


		public static string dec_ucs2le(byte[] src, int si, int slim, char[] buf) {
			int bi;

			for (bi = 0; (si + 1) < slim; bi++, si += 2) {
				buf[bi] = (char) dec_uint16le(src, si);
				if (buf[bi] == '\0') {
					break;
				}
			}

			return new string(buf, 0, bi);
		}
	}

}