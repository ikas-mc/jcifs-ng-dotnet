/*
 * © 2017 AgNO3 Gmbh & Co. KG
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

using System;
using System.Text;
using cifs_ng.lib.ext;
using Logger = org.slf4j.Logger;
using LoggerFactory = org.slf4j.LoggerFactory;
using CIFSContext = jcifs.CIFSContext;
using Configuration = jcifs.Configuration;
using RequestWithPath = jcifs.@internal.RequestWithPath;
using jcifs.@internal.smb2;
using jcifs.util;
using Smb2Constants = jcifs.@internal.smb2.Smb2Constants;
using SMBUtil = jcifs.@internal.util.SMBUtil;
using Hexdump = jcifs.util.Hexdump;


namespace jcifs.@internal.smb2.create {
	#pragma warning disable CS0649

	/// <summary>
	/// @author mbechler
	/// 
	/// </summary>
	public class Smb2CreateRequest : ServerMessageBlock2Request<Smb2CreateResponse>, RequestWithPath {

		private static readonly Logger log = LoggerFactory.getLogger(typeof(Smb2CreateRequest));

		/// 
		public const byte SMB2_OPLOCK_LEVEL_NONE = 0x0;
		/// 
		public const byte SMB2_OPLOCK_LEVEL_II = 0x1;
		/// 
		public const byte SMB2_OPLOCK_LEVEL_EXCLUSIVE = 0x8;
		/// 
		public const byte SMB2_OPLOCK_LEVEL_BATCH = 0x9;
		/// 
		public static readonly byte SMB2_OPLOCK_LEVEL_LEASE = unchecked((byte) 0xFF);

		/// 
		public const int SMB2_IMPERSONATION_LEVEL_ANONYMOUS = 0x0;

		/// 
		public const int SMB2_IMPERSONATION_LEVEL_IDENTIFICATION = 0x1;

		/// 
		public const int SMB2_IMPERSONATION_LEVEL_IMPERSONATION = 0x2;

		/// 
		public const int SMB2_IMPERSONATION_LEVEL_DELEGATE = 0x3;

		/// 
		public const int FILE_SHARE_READ = 0x1;

		/// 
		public const int FILE_SHARE_WRITE = 0x2;

		/// 
		public const int FILE_SHARE_DELETE = 0x4;

		/// 
		public const int FILE_SUPERSEDE = 0x0;
		/// 
		public const int FILE_OPEN = 0x1;
		/// 
		public const int FILE_CREATE = 0x2;
		/// 
		public const int FILE_OPEN_IF = 0x3;
		/// 
		public const int FILE_OVERWRITE = 0x4;
		/// 
		public const int FILE_OVERWRITE_IF = 0x5;

		/// 
		public const int FILE_DIRECTORY_FILE = 0x1;
		/// 
		public const int FILE_WRITE_THROUGH = 0x2;
		/// 
		public const int FILE_SEQUENTIAL_ONLY = 0x4;
		/// 
		public const int FILE_NO_IMTERMEDIATE_BUFFERING = 0x8;
		/// 
		public const int FILE_SYNCHRONOUS_IO_ALERT = 0x10;
		/// 
		public const int FILE_SYNCHRONOUS_IO_NONALERT = 0x20;
		/// 
		public const int FILE_NON_DIRECTORY_FILE = 0x40;
		/// 
		public const int FILE_COMPLETE_IF_OPLOCKED = 0x100;
		/// 
		public const int FILE_NO_EA_KNOWLEDGE = 0x200;
		/// 
		public const int FILE_OPEN_REMOTE_INSTANCE = 0x400;
		/// 
		public const int FILE_RANDOM_ACCESS = 0x800;
		/// 
		public const int FILE_DELETE_ON_CLOSE = 0x1000;
		/// 
		public const int FILE_OPEN_BY_FILE_ID = 0x2000;
		/// 
		public const int FILE_OPEN_FOR_BACKUP_INTENT = 0x4000;
		/// 
		public const int FILE_NO_COMPRESSION = 0x8000;
		/// 
		public const int FILE_OPEN_REQUIRING_OPLOCK = 0x10000;
		/// 
		public const int FILE_DISALLOW_EXCLUSIVE = 0x20000;
		/// 
		public const int FILE_RESERVE_OPFILTER = 0x100000;
		/// 
		public const int FILE_OPEN_REPARSE_POINT = 0x200000;
		/// 
		public const int FILE_NOP_RECALL = 0x400000;
		/// 
		public const int FILE_OPEN_FOR_FREE_SPACE_QUERY = 0x800000;

		private byte securityFlags;
		private byte requestedOplockLevel = SMB2_OPLOCK_LEVEL_NONE;
		private int impersonationLevel = SMB2_IMPERSONATION_LEVEL_IMPERSONATION;
		private long smbCreateFlags;
		private int desiredAccess = 0x00120089; // 0x80000000 | 0x1;
		private int fileAttributes;
		private int shareAccess = FILE_SHARE_READ | FILE_SHARE_WRITE;
		private int createDisposition = FILE_OPEN;
		private int createOptions = 0;

		private string name;
		private CreateContextRequest[] createContexts;
		private string fullName;

		private string domain;

		private string server;

		private bool resolveDfs;


		/// <param name="config"> </param>
		/// <param name="name">
		///            uncPath to open, strips a leading \ </param>
		public Smb2CreateRequest(Configuration config, string name) : base(config, SMB2_CREATE) {
			setPath(name);
		}


		protected  override Smb2CreateResponse createResponse(CIFSContext tc, ServerMessageBlock2Request<Smb2CreateResponse> req) {
			return new Smb2CreateResponse(tc.getConfig(), this.name);
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.RequestWithPath#getPath() </seealso>
		public virtual string getPath() {
			return '\\' + this.name;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.RequestWithPath#getFullUNCPath() </seealso>
		public virtual string getFullUNCPath() {
			return this.fullName;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.RequestWithPath#getServer() </seealso>
		public virtual string getServer() {
			return this.server;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.RequestWithPath#getDomain() </seealso>
		public virtual string getDomain() {
			return this.domain;
		}


		/// <param name="fullName">
		///            the fullName to set </param>
		public virtual void setFullUNCPath(string domain, string server, string fullName) {
			this.domain = domain;
			this.server = server;
			this.fullName = fullName;
		}


		/// <summary>
		/// {@inheritDoc}
		/// 
		/// Strips a leading \
		/// </summary>
		/// <seealso cref= jcifs.internal.RequestWithPath#setPath(java.lang.String) </seealso>
		public virtual void setPath(string path) {
			if (path.Length > 0 && path[0] == '\\') {
				path = path.Substring(1);
			}
			// win8.1 returns ACCESS_DENIED if the trailing backslash is included
			if (path.Length > 1 && path[path.Length - 1] == '\\') {
				path = path.Substring(0, path.Length - 1);
			}
			this.name = path;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.RequestWithPath#setResolveInDfs(boolean) </seealso>
		public virtual void setResolveInDfs(bool resolve) {
			addFlags(SMB2_FLAGS_DFS_OPERATIONS);
			this.resolveDfs = resolve;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.RequestWithPath#isResolveInDfs() </seealso>
		public virtual bool isResolveInDfs() {
			return this.resolveDfs;
		}


		/// <param name="securityFlags">
		///            the securityFlags to set </param>
		public virtual void setSecurityFlags(byte securityFlags) {
			this.securityFlags = securityFlags;
		}


		/// <param name="requestedOplockLevel">
		///            the requestedOplockLevel to set </param>
		public virtual void setRequestedOplockLevel(byte requestedOplockLevel) {
			this.requestedOplockLevel = requestedOplockLevel;
		}


		/// <param name="impersonationLevel">
		///            the impersonationLevel to set </param>
		public virtual void setImpersonationLevel(int impersonationLevel) {
			this.impersonationLevel = impersonationLevel;
		}


		/// <param name="smbCreateFlags">
		///            the smbCreateFlags to set </param>
		public virtual void setSmbCreateFlags(long smbCreateFlags) {
			this.smbCreateFlags = smbCreateFlags;
		}


		/// <param name="desiredAccess">
		///            the desiredAccess to set </param>
		public virtual void setDesiredAccess(int desiredAccess) {
			this.desiredAccess = desiredAccess;
		}


		/// <param name="fileAttributes">
		///            the fileAttributes to set </param>
		public virtual void setFileAttributes(int fileAttributes) {
			this.fileAttributes = fileAttributes;
		}


		/// <param name="shareAccess">
		///            the shareAccess to set </param>
		public virtual void setShareAccess(int shareAccess) {
			this.shareAccess = shareAccess;
		}


		/// <param name="createDisposition">
		///            the createDisposition to set </param>
		public virtual void setCreateDisposition(int createDisposition) {
			this.createDisposition = createDisposition;
		}


		/// <param name="createOptions">
		///            the createOptions to set </param>
		public virtual void setCreateOptions(int createOptions) {
			this.createOptions = createOptions;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.CommonServerMessageBlockRequest#size() </seealso>
		public override int size() {
			int size = Smb2Constants.SMB2_HEADER_LENGTH + 56;
			int nameLen = 2 * this.name.Length;
			if (nameLen == 0) {
				nameLen++;
			}

			size += size8(nameLen);
			if (this.createContexts != null) {
				foreach (CreateContextRequest ccr in this.createContexts) {
					size += size8(ccr.size());
				}
			}
			return size8(size);
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb2.ServerMessageBlock2#writeBytesWireFormat(byte[], int) </seealso>
		protected  override int writeBytesWireFormat(byte[] dst, int dstIndex) {
			int start = dstIndex;

			if (log.isDebugEnabled()) {
				log.debug("Opening " + this.name);
				log.debug("Flags are " + Hexdump.toHexString(getFlags(), 4));
			}

			SMBUtil.writeInt2(57, dst, dstIndex);
			dst[dstIndex + 2] = this.securityFlags;
			dst[dstIndex + 3] = this.requestedOplockLevel;
			dstIndex += 4;

			SMBUtil.writeInt4(this.impersonationLevel, dst, dstIndex);
			dstIndex += 4;
			SMBUtil.writeInt8(this.smbCreateFlags, dst, dstIndex);
			dstIndex += 8;
			dstIndex += 8; // Reserved

			SMBUtil.writeInt4(this.desiredAccess, dst, dstIndex);
			dstIndex += 4;
			SMBUtil.writeInt4(this.fileAttributes, dst, dstIndex);
			dstIndex += 4;
			SMBUtil.writeInt4(this.shareAccess, dst, dstIndex);
			dstIndex += 4;
			SMBUtil.writeInt4(this.createDisposition, dst, dstIndex);
			dstIndex += 4;
			SMBUtil.writeInt4(this.createOptions, dst, dstIndex);
			dstIndex += 4;

			int nameOffsetOffset = dstIndex;
			byte[] nameBytes = this.name.getBytes(Strings.UTF_16LE_ENCODING);
			SMBUtil.writeInt2(nameBytes.Length, dst, dstIndex + 2);
			dstIndex += 4;

			int createContextOffsetOffset = dstIndex;
			dstIndex += 4; // createContextOffset
			int createContextLengthOffset = dstIndex;
			dstIndex += 4; // createContextLength

			SMBUtil.writeInt2(dstIndex - getHeaderStart(), dst, nameOffsetOffset);

			Array.Copy(nameBytes, 0, dst, dstIndex, nameBytes.Length);
			if (nameBytes.Length == 0) {
				// buffer must contain at least one byte
				dstIndex++;
			}
			else {
				dstIndex += nameBytes.Length;
			}

			dstIndex += pad8(dstIndex);

			if (this.createContexts == null || this.createContexts.Length == 0) {
				SMBUtil.writeInt4(0, dst, createContextOffsetOffset);
			}
			else {
				SMBUtil.writeInt4(dstIndex - getHeaderStart(), dst, createContextOffsetOffset);
			}
			int totalCreateContextLength = 0;
			if (this.createContexts != null) {
				int lastStart = -1;
				foreach (CreateContextRequest createContext in this.createContexts) {
					int structStart = dstIndex;

					SMBUtil.writeInt4(0, dst, structStart); // Next
					if (lastStart > 0) {
						// set next pointer of previous CREATE_CONTEXT
						SMBUtil.writeInt4(structStart - dstIndex, dst, lastStart);
					}

					dstIndex += 4;
					byte[] cnBytes = createContext.getName();
					int cnOffsetOffset = dstIndex;
					SMBUtil.writeInt2(cnBytes.Length, dst, dstIndex + 2);
					dstIndex += 4;

					int dataOffsetOffset = dstIndex + 2;
					dstIndex += 4;
					int dataLengthOffset = dstIndex;
					dstIndex += 4;

					SMBUtil.writeInt2(dstIndex - structStart, dst, cnOffsetOffset);
					Array.Copy(cnBytes, 0, dst, dstIndex, cnBytes.Length);
					dstIndex += cnBytes.Length;
					dstIndex += pad8(dstIndex);

					SMBUtil.writeInt2(dstIndex - structStart, dst, dataOffsetOffset);
					int len = createContext.encode(dst, dstIndex);
					SMBUtil.writeInt4(len, dst, dataLengthOffset);
					dstIndex += len;

					int pad = pad8(dstIndex);
					totalCreateContextLength += len + pad;
					dstIndex += pad;
					lastStart = structStart;
				}
			}
			SMBUtil.writeInt4(totalCreateContextLength, dst, createContextLengthOffset);
			return dstIndex - start;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb2.ServerMessageBlock2#readBytesWireFormat(byte[], int) </seealso>
		protected  override int readBytesWireFormat(byte[] buffer, int bufferIndex) {
			return 0;
		}


		public override string ToString() {
			return "[" + base.ToString() + ",name=" + this.name + ",resolveDfs=" + this.resolveDfs + "]";
		}
	}

}