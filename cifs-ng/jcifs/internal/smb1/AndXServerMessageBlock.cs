using System;
using Logger = org.slf4j.Logger;
using LoggerFactory = org.slf4j.LoggerFactory;
using Configuration = jcifs.Configuration;
using RuntimeCIFSException = jcifs.RuntimeCIFSException;
using SMBProtocolDecodingException = jcifs.@internal.SMBProtocolDecodingException;
using SmbComNTCreateAndXResponse = jcifs.@internal.smb1.com.SmbComNTCreateAndXResponse;
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

namespace jcifs.@internal.smb1 {




	/// 
	public abstract class AndXServerMessageBlock : ServerMessageBlock {

		private static readonly Logger log = LoggerFactory.getLogger(typeof(AndXServerMessageBlock));

		private const int ANDX_COMMAND_OFFSET = 1;
		private const int ANDX_RESERVED_OFFSET = 2;
		private const int ANDX_OFFSET_OFFSET = 3;

		private byte andxCommand = unchecked((byte) 0xFF);
		private int andxOffset = 0;

		private ServerMessageBlock andx;


		protected internal AndXServerMessageBlock(Configuration config, byte command, string name, ServerMessageBlock andx) : base(config, command, name) {
			this.andx = andx;
			if (andx != null) {
				this.andxCommand = (byte) andx.getCommand();
			}
		}


		protected internal AndXServerMessageBlock(Configuration config, byte command) : this(config, command, null) {
		}


		protected internal AndXServerMessageBlock(Configuration config, byte command, ServerMessageBlock andx) : base(config, command) {
			this.andx = andx;
			if (andx != null) {
				this.andxCommand = (byte) andx.getCommand();
			}
		}


		protected internal AndXServerMessageBlock(Configuration config) : this(config, null) {
		}


		protected internal AndXServerMessageBlock(Configuration config, ServerMessageBlock andx) : base(config) {
			this.andx = andx;
			if (andx != null) {
				this.andxCommand = (byte) andx.getCommand();
			}
		}


		/// <returns> the andx </returns>
		public ServerMessageBlock getAndx() {
			return this.andx;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb1.ServerMessageBlock#getNext() </seealso>
		public override CommonServerMessageBlockRequest getNext() {
			return this.andx;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.CommonServerMessageBlockResponse#getNextResponse() </seealso>
		public override CommonServerMessageBlockResponse getNextResponse() {
			return this.andx;
		}


		protected internal virtual int getBatchLimit(Configuration cfg, byte cmd) {
			/*
			 * the default limit is 0 batched messages before this
			 * one, meaning this message cannot be batched.
			 */
			return 0;
		}


		/*
		 * We overload this method from ServerMessageBlock because
		 * we want writeAndXWireFormat to write the parameterWords
		 * and bytes. This is so we can write batched smbs because
		 * all but the first smb of the chaain do not have a header
		 * and therefore we do not want to writeHeaderWireFormat. We
		 * just recursivly call writeAndXWireFormat.
		 */

		public override int encode(byte[] dst, int dstIndex) {
			int start = this.headerStart = dstIndex;

			dstIndex += writeHeaderWireFormat(dst, dstIndex);
			dstIndex += writeAndXWireFormat(dst, dstIndex);
			this.length = dstIndex - start;

			if (this.digest != null) {
				this.digest.sign(dst, this.headerStart, this.length, this, this.getResponse());
			}

			return this.length;
		}


		/*
		 * We overload this because we want readAndXWireFormat to
		 * read the parameter words and bytes. This is so when
		 * commands are batched together we can recursivly call
		 * readAndXWireFormat without reading the non-existent header.
		 */

		/// throws jcifs.internal.SMBProtocolDecodingException
		public override int decode(byte[] buffer, int bufferIndex) {
			int start = this.headerStart = bufferIndex;

			bufferIndex += readHeaderWireFormat(buffer, bufferIndex);
			bufferIndex += readAndXWireFormat(buffer, bufferIndex);

			int len = bufferIndex - start;
			this.length = len;

			if (isRetainPayload()) {
				byte[] payload = new byte[len];
				Array.Copy(buffer, 4, payload, 0, len);
				setRawPayload(payload);
			}

			if (!verifySignature(buffer, 4, len)) {
				throw new SMBProtocolDecodingException("Signature verification failed for " + this.GetType().FullName);
			}
			return len;
		}


		protected internal virtual int writeAndXWireFormat(byte[] dst, int dstIndex) {
			int start = dstIndex;

			this.wordCount = writeParameterWordsWireFormat(dst, start + ANDX_OFFSET_OFFSET + 2);
			this.wordCount += 4; // for command, reserved, and offset
			dstIndex += this.wordCount + 1;
			this.wordCount /= 2;
			dst[start] = unchecked((byte)(this.wordCount & 0xFF));

			this.byteCount = writeBytesWireFormat(dst, dstIndex + 2);
			dst[dstIndex++] = unchecked((byte)(this.byteCount & 0xFF));
			dst[dstIndex++] = unchecked((byte)((this.byteCount >> 8) & 0xFF));
			dstIndex += this.byteCount;

			/*
			 * Normally, without intervention everything would batch
			 * with everything else. If the below clause evaluates true
			 * the andx command will not be written and therefore the
			 * response will not read a batched command and therefore
			 * the 'received' member of the response object will not
			 * be set to true indicating the send and sendTransaction
			 * methods that the next part should be sent. This is a
			 * very indirect and simple batching control mechanism.
			 */

			if (this.andx == null || getConfig().isUseBatching() || this.batchLevel >= getBatchLimit(getConfig(), (byte) this.andx.getCommand())) {
				this.andxCommand = unchecked((byte) 0xFF);
				this.andx = null;

				dst[start + ANDX_COMMAND_OFFSET] = unchecked((byte) 0xFF);
				dst[start + ANDX_RESERVED_OFFSET] = (byte) 0x00;
				// dst[start + ANDX_OFFSET_OFFSET] = (byte)0x00;
				// dst[start + ANDX_OFFSET_OFFSET + 1] = (byte)0x00;
				dst[start + ANDX_OFFSET_OFFSET] = unchecked((byte) 0xde);
				dst[start + ANDX_OFFSET_OFFSET + 1] = unchecked((byte) 0xde);

				// andx not used; return
				return dstIndex - start;
			}

			/*
			 * The message provided to batch has a batchLimit that is
			 * higher than the current batchLevel so we will now encode
			 * that chained message. Before doing so we must increment
			 * the batchLevel of the andx message in case it itself is an
			 * andx message and needs to perform the same check as above.
			 */

			this.andx.batchLevel = this.batchLevel + 1;

			dst[start + ANDX_COMMAND_OFFSET] = this.andxCommand;
			dst[start + ANDX_RESERVED_OFFSET] = (byte) 0x00;
			this.andxOffset = dstIndex - this.headerStart;
			SMBUtil.writeInt2(this.andxOffset, dst, start + ANDX_OFFSET_OFFSET);

			this.andx.setUseUnicode(this.isUseUnicode());
			if (this.andx is AndXServerMessageBlock) {

				/*
				 * A word about communicating header info to andx smbs
				 *
				 * This is where we recursively invoke the provided andx smb
				 * object to write it's parameter words and bytes to our outgoing
				 * array. Incedentally when these andx smbs are created they are not
				 * necessarily populated with header data because they're not writing
				 * the header, only their body. But for whatever reason one might wish
				 * to populate fields if the writeXxx operation needs this header data
				 * for whatever reason. I copy over the uid here so it appears correct
				 * in logging output. Logging of andx segments of messages inadvertantly
				 * print header information because of the way toString always makes a
				 * super.toString() call(see toString() at the end of all smbs classes).
				 */

				this.andx.uid = this.uid;
				dstIndex += ((AndXServerMessageBlock) this.andx).writeAndXWireFormat(dst, dstIndex);
			}
			else {
				// the andx smb is not of type andx so lets just write it here and
				// were done.
				int andxStart = dstIndex;
				this.andx.wordCount = this.andx.writeParameterWordsWireFormat(dst, dstIndex);
				dstIndex += this.andx.wordCount + 1;
				this.andx.wordCount /= 2;
				dst[andxStart] = unchecked((byte)(this.andx.wordCount & 0xFF));

				this.andx.byteCount = this.andx.writeBytesWireFormat(dst, dstIndex + 2);
				dst[dstIndex++] = unchecked((byte)(this.andx.byteCount & 0xFF));
				dst[dstIndex++] = unchecked((byte)((this.andx.byteCount >> 8) & 0xFF));
				dstIndex += this.andx.byteCount;
			}

			return dstIndex - start;
		}


		/// throws jcifs.internal.SMBProtocolDecodingException
		protected internal virtual int readAndXWireFormat(byte[] buffer, int bufferIndex) {
			int start = bufferIndex;

			this.wordCount = buffer[bufferIndex++];

			if (this.wordCount != 0) {
				/*
				 * these fields are common to all andx commands
				 * so let's populate them here
				 */

				this.andxCommand = buffer[bufferIndex];
				this.andxOffset = SMBUtil.readInt2(buffer, bufferIndex + 2);

				if (this.andxOffset == 0) {
					this.andxCommand = unchecked((byte) 0xFF);
				}

				/*
				 * no point in calling readParameterWordsWireFormat if there are no more
				 * parameter words. besides, win98 doesn't return "OptionalSupport" field
				 */

				if (this.wordCount > 2) {
					readParameterWordsWireFormat(buffer, bufferIndex + 4);

					/*
					 * The SMB_COM_NT_CREATE_ANDX response wordCount is wrong. There's an
					 * extra 16 bytes for some "Offline Files (CSC or Client Side Caching)"
					 * junk. We need to bump up the wordCount here so that this method returns
					 * the correct number of bytes for signing purposes. Otherwise we get a
					 * signing verification failure.
					 */
					if (this.getCommand() == SMB_COM_NT_CREATE_ANDX && ((SmbComNTCreateAndXResponse) this).isExtended() && ((SmbComNTCreateAndXResponse) this).getFileType() != 1) {
						this.wordCount += 8;
					}
				}

				bufferIndex = start + 1 + (this.wordCount * 2);
			}

			this.byteCount = SMBUtil.readInt2(buffer, bufferIndex);
			bufferIndex += 2;

			if (this.byteCount != 0) {
				// TODO: is this really correct?
				int n = readBytesWireFormat(buffer, bufferIndex);
				if (n != this.byteCount && log.isTraceEnabled()) {
					log.trace("Short read, have " + n + ", want " + this.byteCount);
				}
				bufferIndex += this.byteCount;
			}

			/*
			 * if there is an andx and it itself is an andx then just recur by
			 * calling this method for it. otherwise just read it's parameter words
			 * and bytes as usual. Note how we can't just call andx.readWireFormat
			 * because there's no header.
			 */

			if (this.errorCode != 0 || this.andxCommand == unchecked((byte) 0xFF)) {
				this.andxCommand = unchecked((byte) 0xFF);
				this.andx = null;
			}
			else if (this.andx == null) {
				this.andxCommand = unchecked((byte) 0xFF);
				throw new RuntimeCIFSException("no andx command supplied with response");
			}
			else {

				/*
				 * Set bufferIndex according to andxOffset
				 */

				bufferIndex = this.headerStart + this.andxOffset;

				this.andx.headerStart = this.headerStart;
				this.andx.setCommand(this.andxCommand);
				this.andx.setErrorCode(getErrorCode());
				this.andx.setFlags(getFlags());
				this.andx.setFlags2(getFlags2());
				this.andx.setTid(getTid());
				this.andx.setPid(getPid());
				this.andx.setUid(getUid());
				this.andx.setMid(getMid());
				this.andx.setUseUnicode(this.isUseUnicode());

				if (this.andx is AndXServerMessageBlock) {
					bufferIndex += ((AndXServerMessageBlock) this.andx).readAndXWireFormat(buffer, bufferIndex);
				}
				else {

					/*
					 * Just a plain smb. Read it as normal.
					 */

					buffer[bufferIndex++] = unchecked((byte)(this.andx.wordCount & 0xFF));

					if (this.andx.wordCount != 0) {
						/*
						 * no point in calling readParameterWordsWireFormat if there are no more
						 * parameter words. besides, win98 doesn't return "OptionalSupport" field
						 */

						if (this.andx.wordCount > 2) {
							bufferIndex += this.andx.readParameterWordsWireFormat(buffer, bufferIndex);
						}
					}

					this.andx.byteCount = SMBUtil.readInt2(buffer, bufferIndex);
					bufferIndex += 2;

					if (this.andx.byteCount != 0) {
						this.andx.readBytesWireFormat(buffer, bufferIndex);
						bufferIndex += this.andx.byteCount;
					}
				}
				this.andx.received();
			}

			return bufferIndex - start;
		}


		public override string ToString() {
			return base.ToString() + ",andxCommand=0x" + Hexdump.toHexString(this.andxCommand, 2) + ",andxOffset=" + this.andxOffset;
		}
	}

}