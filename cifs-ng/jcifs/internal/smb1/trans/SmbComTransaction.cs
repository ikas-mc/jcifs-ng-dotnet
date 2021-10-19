using System;
using System.Collections.Generic;
using cifs_ng.lib;
using Configuration = jcifs.Configuration;
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

namespace jcifs.@internal.smb1.trans {



	/// 
	public abstract class SmbComTransaction : ServerMessageBlock, Enumeration<SmbComTransaction> {

		// relative to headerStart
		private const int PRIMARY_SETUP_OFFSET = 61;
		private const int SECONDARY_PARAMETER_OFFSET = 51;

		internal const int DISCONNECT_TID = 0x01;
		internal const int ONE_WAY_TRANSACTION = 0x02;

		internal const int PADDING_SIZE = 4;

		private int tflags = 0x00;
		private int pad1 = 0;
		private int pad2 = 0;
		private bool hasMore = true;
		private bool isPrimary = true;
		private int bufParameterOffset;
		private int bufDataOffset;

		internal const int TRANSACTION_BUF_SIZE = 0xFFFF;

		/// 
		public const byte TRANS2_FIND_FIRST2 = (byte) 0x01;
		/// 
		public const byte TRANS2_FIND_NEXT2 = (byte) 0x02;
		/// 
		public const byte TRANS2_QUERY_FS_INFORMATION = (byte) 0x03;
		/// 
		public const byte TRANS2_QUERY_PATH_INFORMATION = (byte) 0x05;
		/// 
		public const byte TRANS2_GET_DFS_REFERRAL = (byte) 0x10;
		/// 
		public const byte TRANS2_QUERY_FILE_INFORMATION = (byte) 0x07;
		/// 
		public const byte TRANS2_SET_FILE_INFORMATION = (byte) 0x08;

		/// 
		public const byte NET_SHARE_ENUM = (byte) 0x00;
		/// 
		public const byte NET_SERVER_ENUM2 = (byte) 0x68;
		/// 
		public const byte NET_SERVER_ENUM3 = unchecked((byte) 0xD7);

		/// 
		public const byte TRANS_PEEK_NAMED_PIPE = (byte) 0x23;
		/// 
		public const byte TRANS_WAIT_NAMED_PIPE = (byte) 0x53;
		/// 
		public const byte TRANS_CALL_NAMED_PIPE = (byte) 0x54;
		/// 
		public const byte TRANS_TRANSACT_NAMED_PIPE = (byte) 0x26;

		protected internal int primarySetupOffset;
		protected internal int secondaryParameterOffset;
		protected internal int parameterCount;
		protected internal int parameterOffset;
		protected internal int parameterDisplacement;
		protected internal int dataCount;
		protected internal int dataOffset;
		protected internal int dataDisplacement;

		protected internal int totalParameterCount;
		protected internal int totalDataCount;
		protected internal int maxParameterCount;
		protected internal int maxDataCount;
		protected internal byte maxSetupCount;
		protected internal int timeout = 0;
		protected internal int setupCount = 1;
		private byte subCommand;
		protected internal string name = "";
		protected internal int maxBufferSize; // set in SmbTransport.sendTransaction() before nextElement called

		private byte[] txn_buf;


		protected internal SmbComTransaction(Configuration config, byte command, byte subCommand) : base(config, command) {
			this.subCommand = subCommand;
			this.maxDataCount = config.getTransactionBufferSize() - 512;
			this.maxParameterCount = 1024;
			this.primarySetupOffset = PRIMARY_SETUP_OFFSET;
			this.secondaryParameterOffset = SECONDARY_PARAMETER_OFFSET;
		}


		/// <param name="maxBufferSize">
		///            the maxBufferSize to set </param>
		public void setMaxBufferSize(int maxBufferSize) {
			this.maxBufferSize = maxBufferSize;
		}


		/// <param name="maxDataCount">
		///            the maxDataCount to set </param>
		public void setMaxDataCount(int maxDataCount) {
			this.maxDataCount = maxDataCount;
		}


		/// <param name="buffer"> </param>
		public virtual void setBuffer(byte[] buffer) {
			this.txn_buf = buffer;
		}


		/// <returns> the txn_buf </returns>
		public virtual byte[] releaseBuffer() {
			byte[] buf = this.txn_buf;
			this.txn_buf = null;
			return buf;
		}


		/// <returns> the subCommand </returns>
		public byte getSubCommand() {
			return this.subCommand;
		}


		/// <param name="subCommand">
		///            the subCommand to set </param>
		public void setSubCommand(byte subCommand) {
			this.subCommand = subCommand;
		}


		public override void reset() {
			base.reset();
			this.isPrimary = this.hasMore = true;
		}


		protected internal virtual void reset(int key, string lastName) {
			reset();
		}


		public  bool hasMoreElements() {
			return this.hasMore;
		}


		public  SmbComTransaction nextElement() {
			if (this.isPrimary) {
				this.isPrimary = false;

				// primarySetupOffset
				// SMB_COM_TRANSACTION: 61 = 32 SMB header + 1 (word count) + 28 (fixed words)
				// SMB_COM_NT_TRANSACTION: 69 = 32 SMB header + 1 (word count) + 38 (fixed words)
				this.parameterOffset = this.primarySetupOffset;

				// 2* setupCount
				this.parameterOffset += this.setupCount * 2;
				this.parameterOffset += 2; // ByteCount

				if (this.getCommand() == SMB_COM_TRANSACTION && isResponse() == false) {
					this.parameterOffset += stringWireLength(this.name, this.parameterOffset);
				}

				this.pad1 = pad(this.parameterOffset);
				this.parameterOffset += this.pad1;

				this.totalParameterCount = writeParametersWireFormat(this.txn_buf, this.bufParameterOffset);
				this.bufDataOffset = this.totalParameterCount; // data comes right after data

				int available = this.maxBufferSize - this.parameterOffset;
				this.parameterCount = Math.Min(this.totalParameterCount, available);
				available -= this.parameterCount;

				this.dataOffset = this.parameterOffset + this.parameterCount;
				this.pad2 = this.pad(this.dataOffset);
				this.dataOffset += this.pad2;

				this.totalDataCount = writeDataWireFormat(this.txn_buf, this.bufDataOffset);

				this.dataCount = Math.Min(this.totalDataCount, available);
			}
			else {
				if (this.getCommand() != SMB_COM_NT_TRANSACT) {
					this.setCommand(SMB_COM_TRANSACTION_SECONDARY);
				}
				else {
					this.setCommand(SMB_COM_NT_TRANSACT_SECONDARY);
				}
				// totalParameterCount and totalDataCount are set ok from primary

				this.parameterOffset = SECONDARY_PARAMETER_OFFSET;
				if ((this.totalParameterCount - this.parameterDisplacement) > 0) {
					this.pad1 = this.pad(this.parameterOffset);
					this.parameterOffset += this.pad1;
				}

				// caclulate parameterDisplacement before calculating new parameterCount
				this.parameterDisplacement += this.parameterCount;

				int available = this.maxBufferSize - this.parameterOffset - this.pad1;
				this.parameterCount = Math.Min(this.totalParameterCount - this.parameterDisplacement, available);
				available -= this.parameterCount;

				this.dataOffset = this.parameterOffset + this.parameterCount;
				this.pad2 = this.pad(this.dataOffset);
				this.dataOffset += this.pad2;

				this.dataDisplacement += this.dataCount;

				available -= this.pad2;
				this.dataCount = Math.Min(this.totalDataCount - this.dataDisplacement, available);
			}
			if ((this.parameterDisplacement + this.parameterCount) >= this.totalParameterCount && (this.dataDisplacement + this.dataCount) >= this.totalDataCount) {
				this.hasMore = false;
			}
			return this;
		}


		/// <summary>
		/// @return
		/// </summary>
		protected internal virtual int pad(int offset) {
			int p = offset % getPadding();
			if (p == 0) {
				return 0;
			}
			return getPadding() - p;
		}


		/// 
		/// <returns> padding size </returns>
		public virtual int getPadding() {
			return PADDING_SIZE;
		}


		protected internal override int writeParameterWordsWireFormat(byte[] dst, int dstIndex) {
			int start = dstIndex;

			SMBUtil.writeInt2(this.totalParameterCount, dst, dstIndex);
			dstIndex += 2;
			SMBUtil.writeInt2(this.totalDataCount, dst, dstIndex);
			dstIndex += 2;
			if (this.getCommand() != SMB_COM_TRANSACTION_SECONDARY) {
				SMBUtil.writeInt2(this.maxParameterCount, dst, dstIndex);
				dstIndex += 2;
				SMBUtil.writeInt2(this.maxDataCount, dst, dstIndex);
				dstIndex += 2;
				dst[dstIndex++] = this.maxSetupCount;
				dst[dstIndex++] = (byte) 0x00; // Reserved1
				SMBUtil.writeInt2(this.tflags, dst, dstIndex);
				dstIndex += 2;
				SMBUtil.writeInt4(this.timeout, dst, dstIndex);
				dstIndex += 4;
				dst[dstIndex++] = (byte) 0x00; // Reserved2
				dst[dstIndex++] = (byte) 0x00;
			}
			SMBUtil.writeInt2(this.parameterCount, dst, dstIndex);
			dstIndex += 2;
			// writeInt2(( parameterCount == 0 ? 0 : parameterOffset ), dst, dstIndex );
			SMBUtil.writeInt2(this.parameterOffset, dst, dstIndex);
			dstIndex += 2;
			if (this.getCommand() == SMB_COM_TRANSACTION_SECONDARY) {
				SMBUtil.writeInt2(this.parameterDisplacement, dst, dstIndex);
				dstIndex += 2;
			}
			SMBUtil.writeInt2(this.dataCount, dst, dstIndex);
			dstIndex += 2;
			SMBUtil.writeInt2((this.dataCount == 0 ? 0 : this.dataOffset), dst, dstIndex);
			dstIndex += 2;
			if (this.getCommand() == SMB_COM_TRANSACTION_SECONDARY) {
				SMBUtil.writeInt2(this.dataDisplacement, dst, dstIndex);
				dstIndex += 2;
			}
			else {
				dst[dstIndex++] = (byte) this.setupCount;
				dst[dstIndex++] = (byte) 0x00; // Reserved3
				dstIndex += writeSetupWireFormat(dst, dstIndex);
			}

			return dstIndex - start;
		}


		protected internal override int writeBytesWireFormat(byte[] dst, int dstIndex) {
			int start = dstIndex;

			if (this.getCommand() == SMB_COM_TRANSACTION && isResponse() == false) {
				dstIndex += writeString(this.name, dst, dstIndex);
			}

			int end = dstIndex + this.pad1;

			if (this.parameterCount > 0) {
				Array.Copy(this.txn_buf, this.bufParameterOffset, dst, this.headerStart + this.parameterOffset, this.parameterCount);
				end = Math.Max(end, this.headerStart + this.parameterOffset + this.parameterCount + this.pad2);
			}

			if (this.dataCount > 0) {
				Array.Copy(this.txn_buf, this.bufDataOffset, dst, this.headerStart + this.dataOffset, this.dataCount);
				this.bufDataOffset += this.dataCount;
				end = Math.Max(end, this.headerStart + this.dataOffset + this.dataCount);
			}

			return end - start;
		}


		protected internal override int readParameterWordsWireFormat(byte[] buffer, int bufferIndex) {
			return 0;
		}


		protected internal override int readBytesWireFormat(byte[] buffer, int bufferIndex) {
			return 0;
		}


		protected internal abstract int writeSetupWireFormat(byte[] dst, int dstIndex);


		protected internal abstract int writeParametersWireFormat(byte[] dst, int dstIndex);


		protected internal abstract int writeDataWireFormat(byte[] dst, int dstIndex);


		protected internal abstract int readSetupWireFormat(byte[] buffer, int bufferIndex, int len);


		protected internal abstract int readParametersWireFormat(byte[] buffer, int bufferIndex, int len);


		protected internal abstract int readDataWireFormat(byte[] buffer, int bufferIndex, int len);


		public override string ToString() {
			return base.ToString() + ",totalParameterCount=" + this.totalParameterCount + ",totalDataCount=" + this.totalDataCount + ",maxParameterCount=" + this.maxParameterCount + ",maxDataCount=" + this.maxDataCount + ",maxSetupCount=" + (int) this.maxSetupCount + ",flags=0x" + Hexdump.toHexString(this.tflags, 2) + ",timeout=" + this.timeout + ",parameterCount=" + this.parameterCount + ",parameterOffset=" + this.parameterOffset + ",parameterDisplacement=" + this.parameterDisplacement + ",dataCount=" + this.dataCount + ",dataOffset=" + this.dataOffset + ",dataDisplacement=" + this.dataDisplacement + ",setupCount=" + this.setupCount + ",pad=" + this.pad1 + ",pad1=" + this.pad2;
		}

	}

}