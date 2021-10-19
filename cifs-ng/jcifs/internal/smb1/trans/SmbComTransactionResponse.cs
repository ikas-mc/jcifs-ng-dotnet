using System;
using System.Collections.Generic;
using cifs_ng.lib;
using Configuration = jcifs.Configuration;
using SMBProtocolDecodingException = jcifs.@internal.SMBProtocolDecodingException;
using ServerMessageBlock = jcifs.@internal.smb1.ServerMessageBlock;
using SMBUtil = jcifs.@internal.util.SMBUtil;
using FileEntry = jcifs.smb.FileEntry;

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
	public abstract class SmbComTransactionResponse : ServerMessageBlock, Enumeration<SmbComTransactionResponse> {

		// relative to headerStart
		internal const int SETUP_OFFSET = 61;

		internal const int DISCONNECT_TID = 0x01;
		internal const int ONE_WAY_TRANSACTION = 0x02;

		private int pad;
		private int pad1;
		private bool parametersDone, dataDone;

		protected internal int totalParameterCount;
		protected internal int totalDataCount;
		protected internal int parameterCount;
		protected internal int parameterOffset;
		protected internal int parameterDisplacement;
		protected internal int dataOffset;
		protected internal int dataDisplacement;
		protected internal int setupCount;
		protected internal int bufParameterStart;
		protected internal int bufDataStart;

		internal int dataCount;
		internal byte subCommand;
		internal volatile bool hasMore = true;
		internal volatile bool isPrimary = true;
		internal byte[] txn_buf;

		/* for doNetEnum and doFindFirstNext */
		private int status;
		private int numEntries;
		private FileEntry[] results;


		protected internal SmbComTransactionResponse(Configuration config) : base(config) {
		}


		protected internal SmbComTransactionResponse(Configuration config, byte command, byte subcommand) : base(config, command) {
			this.subCommand = subcommand;
		}


		/// <returns> the dataCount </returns>
		protected internal int getDataCount() {
			return this.dataCount;
		}


		/// <param name="dataCount">
		///            the dataCount to set </param>
		public void setDataCount(int dataCount) {
			this.dataCount = dataCount;
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


		/// <returns> the status </returns>
		public int getStatus() {
			return this.status;
		}


		/// <param name="status">
		///            the status to set </param>
		protected internal void setStatus(int status) {
			this.status = status;
		}


		/// <returns> the numEntries </returns>
		public int getNumEntries() {
			return this.numEntries;
		}


		/// <param name="numEntries">
		///            the numEntries to set </param>
		protected internal void setNumEntries(int numEntries) {
			this.numEntries = numEntries;
		}


		/// <returns> the results </returns>
		public FileEntry[] getResults() {
			return this.results;
		}


		/// <param name="results">
		///            the results to set </param>
		protected internal void setResults(FileEntry[] results) {
			this.results = results;
		}


		public override void reset() {
			base.reset();
			this.bufDataStart = 0;
			this.isPrimary = this.hasMore = true;
			this.parametersDone = this.dataDone = false;
		}


		public  bool hasMoreElements() {
			return this.errorCode == 0 && this.hasMore;
		}


		public  SmbComTransactionResponse nextElement() {
			if (this.isPrimary) {
				this.isPrimary = false;
			}
			return this;
		}


		protected internal override int writeParameterWordsWireFormat(byte[] dst, int dstIndex) {
			return 0;
		}


		protected internal override int writeBytesWireFormat(byte[] dst, int dstIndex) {
			return 0;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb1.ServerMessageBlock#decode(byte[], int) </seealso>
		/// throws jcifs.internal.SMBProtocolDecodingException
		public override int decode(byte[] buffer, int bufferIndex) {
			int len = base.decode(buffer, bufferIndex);
			if (this.byteCount == 0) {
				// otherwise hasMore may not be correctly set
				readBytesWireFormat(buffer, len + bufferIndex);
			}
			nextElement();
			return len;
		}


		protected internal override int readParameterWordsWireFormat(byte[] buffer, int bufferIndex) {
			int start = bufferIndex;

			this.totalParameterCount = SMBUtil.readInt2(buffer, bufferIndex);
			if (this.bufDataStart == 0) {
				this.bufDataStart = this.totalParameterCount;
			}
			bufferIndex += 2;
			this.totalDataCount = SMBUtil.readInt2(buffer, bufferIndex);
			bufferIndex += 4; // Reserved
			this.parameterCount = SMBUtil.readInt2(buffer, bufferIndex);
			bufferIndex += 2;
			this.parameterOffset = SMBUtil.readInt2(buffer, bufferIndex);
			bufferIndex += 2;
			this.parameterDisplacement = SMBUtil.readInt2(buffer, bufferIndex);
			bufferIndex += 2;
			this.dataCount = SMBUtil.readInt2(buffer, bufferIndex);
			bufferIndex += 2;
			this.dataOffset = SMBUtil.readInt2(buffer, bufferIndex);
			bufferIndex += 2;
			this.dataDisplacement = SMBUtil.readInt2(buffer, bufferIndex);
			bufferIndex += 2;
			this.setupCount = buffer[bufferIndex] & 0xFF;
			bufferIndex += 2;

			return bufferIndex - start;
		}


		/// throws jcifs.internal.SMBProtocolDecodingException
		protected internal override int readBytesWireFormat(byte[] buffer, int bufferIndex) {
			this.pad = this.pad1 = 0;
			if (this.parameterCount > 0) {
				bufferIndex += this.pad = this.parameterOffset - (bufferIndex - this.headerStart);
				Array.Copy(buffer, bufferIndex, this.txn_buf, this.bufParameterStart + this.parameterDisplacement, this.parameterCount);
				bufferIndex += this.parameterCount;
			}
			if (this.dataCount > 0) {
				bufferIndex += this.pad1 = this.dataOffset - (bufferIndex - this.headerStart);
				Array.Copy(buffer, bufferIndex, this.txn_buf, this.bufDataStart + this.dataDisplacement, this.dataCount);
				bufferIndex += this.dataCount;
			}

			/*
			 * Check to see if the entire transaction has been
			 * read. If so call the read methods.
			 */

			if (!this.parametersDone && (this.parameterDisplacement + this.parameterCount) == this.totalParameterCount) {
				this.parametersDone = true;
			}

			if (!this.dataDone && (this.dataDisplacement + this.dataCount) == this.totalDataCount) {
				this.dataDone = true;
			}

			if (this.parametersDone && this.dataDone) {
				readParametersWireFormat(this.txn_buf, this.bufParameterStart, this.totalParameterCount);
				readDataWireFormat(this.txn_buf, this.bufDataStart, this.totalDataCount);
				this.hasMore = false;
			}

			return this.pad + this.parameterCount + this.pad1 + this.dataCount;
		}


		protected internal abstract int writeSetupWireFormat(byte[] dst, int dstIndex);


		protected internal abstract int writeParametersWireFormat(byte[] dst, int dstIndex);


		protected internal abstract int writeDataWireFormat(byte[] dst, int dstIndex);


		protected internal abstract int readSetupWireFormat(byte[] buffer, int bufferIndex, int len);


		/// throws jcifs.internal.SMBProtocolDecodingException;
		protected internal abstract int readParametersWireFormat(byte[] buffer, int bufferIndex, int len);


		/// throws jcifs.internal.SMBProtocolDecodingException;
		protected internal abstract int readDataWireFormat(byte[] buffer, int bufferIndex, int len);


		public override string ToString() {
			return base.ToString() + ",totalParameterCount=" + this.totalParameterCount + ",totalDataCount=" + this.totalDataCount + ",parameterCount=" + this.parameterCount + ",parameterOffset=" + this.parameterOffset + ",parameterDisplacement=" + this.parameterDisplacement + ",dataCount=" + this.dataCount + ",dataOffset=" + this.dataOffset + ",dataDisplacement=" + this.dataDisplacement + ",setupCount=" + this.setupCount + ",pad=" + this.pad + ",pad1=" + this.pad1;
		}
	}

}