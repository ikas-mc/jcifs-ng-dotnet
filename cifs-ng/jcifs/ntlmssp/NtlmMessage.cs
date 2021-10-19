using System;
using jcifs.util;
using SmbConstants = jcifs.SmbConstants;

/* jcifs smb client library in Java
 * Copyright (C) 2002  "Michael B. Allen" <jcifs at samba dot org>
 *                   "Eric Glass" <jcifs at samba dot org>
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

namespace jcifs.ntlmssp {



	/// <summary>
	/// Abstract superclass for all NTLMSSP messages.
	/// </summary>
	public abstract class NtlmMessage{

		/// <summary>
		/// The NTLMSSP "preamble".
		/// </summary>
		protected internal static readonly byte[] NTLMSSP_SIGNATURE = new byte[] {(byte) 'N', (byte) 'T', (byte) 'L', (byte) 'M', (byte) 'S', (byte) 'S', (byte) 'P', (byte) 0};

		/// <summary>
		/// NTLM version
		/// </summary>
		protected internal static readonly byte[] NTLMSSP_VERSION = new byte[] {6, 1, 0, 0, 0, 0, 0, 15};

		protected internal const int NTLMSSP_TYPE1 = 0x1;
		protected internal const int NTLMSSP_TYPE2 = 0x2;
		protected internal const int NTLMSSP_TYPE3 = 0x3;

		private static readonly string OEM_ENCODING = SmbConstants.DEFAULT_OEM_ENCODING;
		protected internal static string UNI_ENCODING = Strings.UTF_16LE_ENCODING.EncodingName; //"UTF-16LE"; //TODO 1 config

		private int flags;


		/// <summary>
		/// Returns the flags currently in use for this message.
		/// </summary>
		/// <returns> An <code>int</code> containing the flags in use for this
		///         message. </returns>
		public virtual int getFlags() {
			return this.flags;
		}


		/// <summary>
		/// Sets the flags for this message.
		/// </summary>
		/// <param name="flags">
		///            The flags for this message. </param>
		public virtual void setFlags(int flags) {
			this.flags = flags;
		}


		/// <summary>
		/// Returns the status of the specified flag.
		/// </summary>
		/// <param name="flag">
		///            The flag to test (i.e., <code>NTLMSSP_NEGOTIATE_OEM</code>). </param>
		/// <returns> A <code>boolean</code> indicating whether the flag is set. </returns>
		public virtual bool getFlag(int flag) {
			return (getFlags() & flag) != 0;
		}


		/// <summary>
		/// Sets or clears the specified flag.
		/// </summary>
		/// <param name="flag">
		///            The flag to set/clear (i.e.,
		///            <code>NTLMSSP_NEGOTIATE_OEM</code>). </param>
		/// <param name="value">
		///            Indicates whether to set (<code>true</code>) or
		///            clear (<code>false</code>) the specified flag. </param>
		public virtual void setFlag(int flag, bool value)
		{
			//TODO 
			var x = unchecked((int) 0xffffffff);
			setFlags(value ? (getFlags() | flag) : (getFlags() & (x ^ flag)));
		}


		internal static int readULong(byte[] src, int index) {
			return (src[index] & 0xff) | ((src[index + 1] & 0xff) << 8) | ((src[index + 2] & 0xff) << 16) | ((src[index + 3] & 0xff) << 24);
		}


		internal static int readUShort(byte[] src, int index) {
			return (src[index] & 0xff) | ((src[index + 1] & 0xff) << 8);
		}


		internal static byte[] readSecurityBuffer(byte[] src, int index) {
			int length = readUShort(src, index);
			int offset = readULong(src, index + 4);
			byte[] buffer = new byte[length];
			Array.Copy(src, offset, buffer, 0, length);
			return buffer;
		}


		internal static void writeULong(byte[] dest, int offset, int @ulong) {
			dest[offset] = unchecked((byte)(@ulong & 0xff));
			dest[offset + 1] = unchecked((byte)(@ulong >> 8 & 0xff));
			dest[offset + 2] = unchecked((byte)(@ulong >> 16 & 0xff));
			dest[offset + 3] = unchecked((byte)(@ulong >> 24 & 0xff));
		}


		internal static void writeUShort(byte[] dest, int offset, int @ushort) {
			dest[offset] = unchecked((byte)(@ushort & 0xff));
			dest[offset + 1] = unchecked((byte)(@ushort >> 8 & 0xff));
		}


		internal static int writeSecurityBuffer(byte[] dest, int offset, byte[] src) {
			int length = (src != null) ? src.Length : 0;
			if (length == 0) {
				return offset + 4;
			}
			writeUShort(dest, offset, length);
			writeUShort(dest, offset + 2, length);
			return offset + 4;
		}


		internal static int writeSecurityBufferContent(byte[] dest, int pos, int off, byte[] src) {
			writeULong(dest, off, pos);
			if (src != null && src.Length > 0) {
				Array.Copy(src, 0, dest, pos, src.Length);
				return src.Length;
			}
			return 0;
		}


		internal static string getOEMEncoding() {
			return OEM_ENCODING;
		}


		/// <summary>
		/// Returns the raw byte representation of this message.
		/// </summary>
		/// <returns> A <code>byte[]</code> containing the raw message material. </returns>
		/// <exception cref="IOException"> </exception>
		/// throws java.io.IOException;
		public abstract byte[] toByteArray();

	}

}