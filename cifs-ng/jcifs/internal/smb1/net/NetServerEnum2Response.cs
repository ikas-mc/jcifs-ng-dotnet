using Logger = org.slf4j.Logger;
using LoggerFactory = org.slf4j.LoggerFactory;
using Configuration = jcifs.Configuration;
using SmbConstants = jcifs.SmbConstants;
using SmbComTransactionResponse = jcifs.@internal.smb1.trans.SmbComTransactionResponse;
using SMBUtil = jcifs.@internal.util.SMBUtil;
using FileEntry = jcifs.smb.FileEntry;
using Hexdump = jcifs.util.Hexdump;

/* jcifs smb client library in Java
 * Copyright (C) 2000  "Michael B. Allen" <jcifs at samba dot org>
 *                             Gary Rambo <grambo aventail.com>
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

namespace jcifs.@internal.smb1.net {




	/// 
	/// <summary>
	/// @author mbechler
	/// 
	/// </summary>
	public class NetServerEnum2Response : SmbComTransactionResponse {

		private static readonly Logger log = LoggerFactory.getLogger(typeof(NetServerEnum2Response));

		internal class ServerInfo1 : FileEntry {
			private readonly NetServerEnum2Response outerInstance;

			public ServerInfo1(NetServerEnum2Response outerInstance) {
				this.outerInstance = outerInstance;
			}


			internal string name;
			internal int versionMajor;
			internal int versionMinor;
			internal int type;
			internal string commentOrMasterBrowser;


			public virtual string getName() {
				return this.name;
			}


			public virtual int getType() {
				return (this.type & 0x80000000) != 0 ? SmbConstants.TYPE_WORKGROUP : SmbConstants.TYPE_SERVER;
			}


			public virtual int getAttributes() {
				return SmbConstants.ATTR_READONLY | SmbConstants.ATTR_DIRECTORY;
			}


			/// <summary>
			/// {@inheritDoc}
			/// </summary>
			/// <seealso cref= jcifs.smb.FileEntry#getFileIndex() </seealso>
			public virtual int getFileIndex() {
				return 0;
			}


			public virtual long createTime() {
				return 0L;
			}


			public virtual long lastModified() {
				return 0L;
			}


			public virtual long lastAccess() {
				return 0L;
			}


			public virtual long length() {
				return 0L;
			}


			public override string ToString() {
				return "ServerInfo1[" + "name=" + this.name + ",versionMajor=" + this.versionMajor + ",versionMinor=" + this.versionMinor + ",type=0x" + Hexdump.toHexString(this.type, 8) + ",commentOrMasterBrowser=" + this.commentOrMasterBrowser + "]";
			}
		}

		private int converter, totalAvailableEntries;

		private string lastName;


		/// 
		/// <param name="config"> </param>
		public NetServerEnum2Response(Configuration config) : base(config) {
		}


		/// <returns> the lastName </returns>
		public string getLastName() {
			return this.lastName;
		}


		protected internal override int writeSetupWireFormat(byte[] dst, int dstIndex) {
			return 0;
		}


		protected internal override int writeParametersWireFormat(byte[] dst, int dstIndex) {
			return 0;
		}


		protected internal override int writeDataWireFormat(byte[] dst, int dstIndex) {
			return 0;
		}


		protected internal override int readSetupWireFormat(byte[] buffer, int bufferIndex, int len) {
			return 0;
		}


		protected internal override int readParametersWireFormat(byte[] buffer, int bufferIndex, int len) {
			int start = bufferIndex;
			setStatus(SMBUtil.readInt2(buffer, bufferIndex));
			bufferIndex += 2;
			this.converter = SMBUtil.readInt2(buffer, bufferIndex);
			bufferIndex += 2;
			setNumEntries(SMBUtil.readInt2(buffer, bufferIndex));
			bufferIndex += 2;
			this.totalAvailableEntries = SMBUtil.readInt2(buffer, bufferIndex);
			bufferIndex += 2;
			return bufferIndex - start;
		}


		protected internal override int readDataWireFormat(byte[] buffer, int bufferIndex, int len) {
			int start = bufferIndex;
			ServerInfo1 e = null;
			ServerInfo1[] results = new ServerInfo1[getNumEntries()];
			for (int i = 0; i < getNumEntries(); i++) {
				results[i] = e = new ServerInfo1(this);
				e.name = readString(buffer, bufferIndex, 16, false);
				bufferIndex += 16;
				e.versionMajor = buffer[bufferIndex++] & 0xFF;
				e.versionMinor = buffer[bufferIndex++] & 0xFF;
				e.type = SMBUtil.readInt4(buffer, bufferIndex);
				bufferIndex += 4;
				int off = SMBUtil.readInt4(buffer, bufferIndex);
				bufferIndex += 4;
				off = (off & 0xFFFF) - this.converter;
				off = start + off;
				e.commentOrMasterBrowser = readString(buffer, off, 48, false);

				if (log.isTraceEnabled()) {
					log.trace(e.ToString());
				}
			}
			setResults(results);
			this.lastName = e == null ? null : e.name;
			return bufferIndex - start;
		}


		public override string ToString() {
			return "NetServerEnum2Response[" + base.ToString() + ",status=" + this.getStatus() + ",converter=" + this.converter + ",entriesReturned=" + this.getNumEntries() + ",totalAvailableEntries=" + this.totalAvailableEntries + ",lastName=" + this.lastName + "]";
		}
	}

}