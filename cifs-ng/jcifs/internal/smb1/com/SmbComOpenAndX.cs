using System;
using Configuration = jcifs.Configuration;
using SmbConstants = jcifs.SmbConstants;
using AndXServerMessageBlock = jcifs.@internal.smb1.AndXServerMessageBlock;
using ServerMessageBlock = jcifs.@internal.smb1.ServerMessageBlock;
using SMBUtil = jcifs.@internal.util.SMBUtil;
using Hexdump = jcifs.util.Hexdump;

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
	public class SmbComOpenAndX : AndXServerMessageBlock {

		// flags (not the same as flags constructor argument)
		internal const int FLAGS_RETURN_ADDITIONAL_INFO = 0x01;
		internal const int FLAGS_REQUEST_OPLOCK = 0x02;
		internal const int FLAGS_REQUEST_BATCH_OPLOCK = 0x04;

		// Access Mode Encoding for desiredAccess
		internal const int SHARING_COMPATIBILITY = 0x00;
		internal const int SHARING_DENY_READ_WRITE_EXECUTE = 0x10;
		internal const int SHARING_DENY_WRITE = 0x20;
		internal const int SHARING_DENY_READ_EXECUTE = 0x30;
		internal const int SHARING_DENY_NONE = 0x40;

		internal const int DO_NOT_CACHE = 0x1000; // bit 12
		internal const int WRITE_THROUGH = 0x4000; // bit 14

		internal const int OPEN_FN_CREATE = 0x10;
		internal const int OPEN_FN_FAIL_IF_EXISTS = 0x00;
		internal const int OPEN_FN_OPEN = 0x01;
		internal const int OPEN_FN_TRUNC = 0x02;

		internal int tflags, desiredAccess, searchAttributes, fileAttributes, creationTime, openFunction, allocationSize;


		// flags is NOT the same as flags member

		/// 
		/// <param name="config"> </param>
		/// <param name="fileName"> </param>
		/// <param name="access"> </param>
		/// <param name="shareAccess"> </param>
		/// <param name="flags"> </param>
		/// <param name="fileAttributes"> </param>
		/// <param name="andx"> </param>
		public SmbComOpenAndX(Configuration config, string fileName, int access, int shareAccess, int flags, int fileAttributes, ServerMessageBlock andx) : base(config, SMB_COM_OPEN_ANDX, fileName, andx) {
			this.fileAttributes = fileAttributes;

			this.desiredAccess = access & 0x3;
			if (this.desiredAccess == 0x3) {
				this.desiredAccess = 0x2; // Mmm, I thought 0x03 was RDWR
			}

			// map shareAccess as far as we can
			if ((shareAccess & SmbConstants.FILE_SHARE_READ) != 0 && (shareAccess & SmbConstants.FILE_SHARE_WRITE) != 0) {
				this.desiredAccess |= SHARING_DENY_NONE;
			}
			else if (shareAccess == SmbConstants.FILE_NO_SHARE) {
				this.desiredAccess |= SHARING_DENY_READ_WRITE_EXECUTE;
			}
			else if ((shareAccess & SmbConstants.FILE_SHARE_WRITE) == 0) {
				this.desiredAccess |= SHARING_DENY_WRITE;
			}
			else if ((shareAccess & SmbConstants.FILE_SHARE_READ) == 0) {
				this.desiredAccess |= SHARING_DENY_READ_EXECUTE;
			}
			else {
				// neither SHARE_READ nor SHARE_WRITE are set
				this.desiredAccess |= SHARING_DENY_READ_WRITE_EXECUTE;
			}

			this.desiredAccess &= ~0x1; // Win98 doesn't like GENERIC_READ ?! -- get Access Denied.

			// searchAttributes
			this.searchAttributes = SmbConstants.ATTR_DIRECTORY | SmbConstants.ATTR_HIDDEN | SmbConstants.ATTR_SYSTEM;

			// openFunction
			if ((flags & SmbConstants.O_TRUNC) == SmbConstants.O_TRUNC) {
				// truncate the file
				if ((flags & SmbConstants.O_CREAT) == SmbConstants.O_CREAT) {
					// create it if necessary
					this.openFunction = OPEN_FN_TRUNC | OPEN_FN_CREATE;
				}
				else {
					this.openFunction = OPEN_FN_TRUNC;
				}
			}
			else {
				// don't truncate the file
				if ((flags & SmbConstants.O_CREAT) == SmbConstants.O_CREAT) {
					// create it if necessary
					if ((flags & SmbConstants.O_EXCL) == SmbConstants.O_EXCL) {
						// fail if already exists
						this.openFunction = OPEN_FN_CREATE | OPEN_FN_FAIL_IF_EXISTS;
					}
					else {
						this.openFunction = OPEN_FN_CREATE | OPEN_FN_OPEN;
					}
				}
				else {
					this.openFunction = OPEN_FN_OPEN;
				}
			}
		}


		protected internal override int getBatchLimit(Configuration cfg, byte cmd) {
			return cmd == SMB_COM_READ_ANDX ? cfg.getBatchLimit("OpenAndX.ReadAndX") : 0;
		}


		protected internal override int writeParameterWordsWireFormat(byte[] dst, int dstIndex) {
			int start = dstIndex;

			SMBUtil.writeInt2(this.tflags, dst, dstIndex);
			dstIndex += 2;
			SMBUtil.writeInt2(this.desiredAccess, dst, dstIndex);
			dstIndex += 2;
			SMBUtil.writeInt2(this.searchAttributes, dst, dstIndex);
			dstIndex += 2;
			SMBUtil.writeInt2(this.fileAttributes, dst, dstIndex);
			dstIndex += 2;
			this.creationTime = 0;
			SMBUtil.writeInt4(this.creationTime, dst, dstIndex);
			dstIndex += 4;
			SMBUtil.writeInt2(this.openFunction, dst, dstIndex);
			dstIndex += 2;
			SMBUtil.writeInt4(this.allocationSize, dst, dstIndex);
			dstIndex += 4;
			for (int i = 0; i < 8; i++) {
				dst[dstIndex++] = 0x00;
			}

			return dstIndex - start;
		}


		protected internal override int writeBytesWireFormat(byte[] dst, int dstIndex) {
			int start = dstIndex;

			if (this.isUseUnicode()) {
				dst[dstIndex++] = (byte) '\0';
			}
			dstIndex += writeString(this.path, dst, dstIndex);

			return dstIndex - start;
		}


		protected internal override int readParameterWordsWireFormat(byte[] buffer, int bufferIndex) {
			return 0;
		}


		protected internal override int readBytesWireFormat(byte[] buffer, int bufferIndex) {
			return 0;
		}


		public override string ToString() {
			return "SmbComOpenAndX[" + base.ToString() + ",flags=0x" + Hexdump.toHexString(this.tflags, 2) + ",desiredAccess=0x" + Hexdump.toHexString(this.desiredAccess, 4) + ",searchAttributes=0x" + Hexdump.toHexString(this.searchAttributes, 4) + ",fileAttributes=0x" + Hexdump.toHexString(this.fileAttributes, 4) + ",creationTime=" + DateTimeOffset.FromUnixTimeMilliseconds(this.creationTime) + ",openFunction=0x" + Hexdump.toHexString(this.openFunction, 2) + ",allocationSize=" + this.allocationSize + ",fileName=" + this.path + "]";
		}
	}

}