using System;
using jcifs.util.transport;
using org.slf4j;
using Configuration = jcifs.Configuration;
using CommonServerMessageBlock = jcifs.@internal.CommonServerMessageBlock;
using CommonServerMessageBlockResponse = jcifs.@internal.CommonServerMessageBlockResponse;
using SMBProtocolDecodingException = jcifs.@internal.SMBProtocolDecodingException;
using SMBSigningDigest = jcifs.@internal.SMBSigningDigest;
using SMBUtil = jcifs.@internal.util.SMBUtil;
using SmbException = jcifs.smb.SmbException;
using Hexdump = jcifs.util.Hexdump;

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
namespace jcifs.@internal.smb2 {



	/// 
	/// <summary>
	/// @author mbechler
	/// 
	/// </summary>
	public abstract class ServerMessageBlock2 : CommonServerMessageBlock
	{

		private static Logger _logger = LoggerFactory.getLogger(typeof(ServerMessageBlock2));
		/*
		 * These are all the smbs supported by this library. This includes requests
		 * and well as their responses for each type however the actual implementations
		 * of the readXxxWireFormat and writeXxxWireFormat methods may not be in
		 * place. For example at the time of this writing the readXxxWireFormat
		 * for requests and the writeXxxWireFormat for responses are not implemented
		 * and simply return 0. These would need to be completed for a server
		 * implementation.
		 */

		protected  const short SMB2_NEGOTIATE = 0x00;
		protected  const short SMB2_SESSION_SETUP = 0x01;
		protected  const short SMB2_LOGOFF = 0x02;
		protected  const short SMB2_TREE_CONNECT = 0x0003;
		protected  const short SMB2_TREE_DISCONNECT = 0x0004;
		protected  const short SMB2_CREATE = 0x0005;
		protected  const short SMB2_CLOSE = 0x0006;
		protected  const short SMB2_FLUSH = 0x0007;
		protected  const short SMB2_READ = 0x0008;
		protected  const short SMB2_WRITE = 0x0009;
		protected  const short SMB2_LOCK = 0x000A;
		protected  const short SMB2_IOCTL = 0x000B;
		protected  const short SMB2_CANCEL = 0x000C;
		protected  const short SMB2_ECHO = 0x000D;
		protected  const short SMB2_QUERY_DIRECTORY = 0x000E;
		protected  const short SMB2_CHANGE_NOTIFY = 0x000F;
		protected  const short SMB2_QUERY_INFO = 0x0010;
		protected  const short SMB2_SET_INFO = 0x0011;
		protected  const short SMB2_OPLOCK_BREAK = 0x0012;

		/// 
		public const int SMB2_FLAGS_SERVER_TO_REDIR = 0x00000001;
		/// 
		public const int SMB2_FLAGS_ASYNC_COMMAND = 0x00000002;
		/// 
		public const int SMB2_FLAGS_RELATED_OPERATIONS = 0x00000004;
		/// 
		public const int SMB2_FLAGS_SIGNED = 0x00000008;
		/// 
		public const int SMB2_FLAGS_PRIORITY_MASK = 0x00000070;
		/// 
		public const int SMB2_FLAGS_DFS_OPERATIONS = 0x10000000;
		/// 
		public const int SMB2_FLAGS_REPLAY_OPERATION = 0x20000000;

		private int command;
		private int flags;
		private int length, headerStart, wordCount, byteCount;

		private byte[] signature = new byte[16];
		private Smb2SigningDigest digest = null;

		private Configuration config;

		private int creditCharge;
		private int status;
		private int credit;
		private int nextCommand;
		private int readSize;
		private bool async;
		private int treeId;
		private long mid, asyncId, sessionId;
		private byte errorContextCount;
		private byte[] errorData;

		private bool retainPayloadField;
		private byte[] rawPayload;

		private ServerMessageBlock2 next;

		protected  ServerMessageBlock2(Configuration config) {
			this.config = config;
		}


		protected  ServerMessageBlock2(Configuration config, int command) {
			this.config = config;
			this.command = command;
		}


		/// <returns> the config </returns>
		protected  virtual Configuration getConfig() {
			return this.config;
		}


		public virtual void reset() {
			this.flags = 0;
			this.digest = null;
			this.sessionId = 0;
			this.treeId = 0;
		}


		/// <returns> the command </returns>
		public virtual int getCommand() {
			return this.command;
		}


		/// <returns> offset to next compound command </returns>
		public virtual int getNextCommandOffset() {
			return this.nextCommand;
		}


		/// <param name="readSize">
		///            the readSize to set </param>
		public virtual void setReadSize(int readSize) {
			this.readSize = readSize;
		}


		/// <returns> the async </returns>
		public virtual bool isAsync() {
			return this.async;
		}


		/// <param name="command">
		///            the command to set </param>
		public virtual void setCommand(int command) {
			this.command = command;
		}


		/// <returns> the treeId </returns>
		public virtual int getTreeId() {
			return this.treeId;
		}


		/// <param name="treeId">
		///            the treeId to set </param>
		public virtual void setTreeId(int treeId) {
			this.treeId = treeId;
			if (this.next != null) {
				this.next.setTreeId(treeId);
			}
		}


		/// <returns> the asyncId </returns>
		public virtual long getAsyncId() {
			return this.asyncId;
		}


		/// <param name="asyncId">
		///            the asyncId to set </param>
		public virtual void setAsyncId(long asyncId) {
			this.asyncId = asyncId;
		}


		/// <returns> the credit </returns>
		public virtual int getCredit() {
			return this.credit;
		}


		/// <param name="credit">
		///            the credit to set </param>
		public  virtual void setCredit(int credit) {
			this.credit = credit;
		}


		/// <returns> the creditCharge </returns>
		public virtual int getCreditCharge() {
			return this.creditCharge;
		}


		public virtual void retainPayload() {
			this.retainPayloadField = true;
		}


		public virtual bool isRetainPayload() {
			return this.retainPayloadField;
		}


		public virtual byte[] getRawPayload() {
			return this.rawPayload;
		}


		public virtual void setRawPayload(byte[] rawPayload) {
			this.rawPayload = rawPayload;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.CommonServerMessageBlock#getDigest() </seealso>
		public virtual SMBSigningDigest getDigest() {
			return this.digest;
		}


		/// 
		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.CommonServerMessageBlock#setDigest(jcifs.internal.SMBSigningDigest) </seealso>
		public virtual void setDigest(SMBSigningDigest digest) {
			this.digest = (Smb2SigningDigest) digest;
			if (this.next != null) {
				this.next.setDigest(digest);
			}
		}


		/// <returns> the status </returns>
		public virtual int getStatus() {
			return this.status;
		}


		/// <returns> the sessionId </returns>
		public  virtual long getSessionId() {
			return this.sessionId;
		}


		/// <param name="sessionId">
		///            the sessionId to set </param>
		public virtual void setSessionId(long sessionId) {
			this.sessionId = sessionId;
			if (this.next != null) {
				this.next.setSessionId(sessionId);
			}
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.CommonServerMessageBlock#setExtendedSecurity(boolean) </seealso>
		public virtual void setExtendedSecurity(bool extendedSecurity) {
			// ignore
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.CommonServerMessageBlock#setUid(int) </seealso>
		public virtual void setUid(int uid) {
			// ignore
		}


		/// <returns> the flags </returns>
		public virtual int getFlags() {
			return this.flags;
		}


		/// 
		/// <param name="flag"> </param>
		public virtual void addFlags(int flag) {
			this.flags |= flag;
		}


		/// 
		/// <param name="flag"> </param>
		public virtual void clearFlags(int flag) {
			this.flags &= ~flag;
		}


		/// <returns> the mid </returns>
		public virtual long getMid() {
			return this.mid;
		}


		/// <param name="mid">
		///            the mid to set </param>
		public virtual void setMid(long mid) {
			this.mid = mid;
		}


		/// <param name="n"> </param>
		/// <returns> whether chaining was successful </returns>
		public virtual bool chain(ServerMessageBlock2 n) {
			if (this.next != null) {
				return this.next.chain(n);
			}

			n.addFlags(SMB2_FLAGS_RELATED_OPERATIONS);
			this.next = n;
			return true;
		}


		protected  virtual ServerMessageBlock2 getNext() {
			return this.next;
		}


		protected internal virtual void setNext(ServerMessageBlock2 n) {
			this.next = n;
		}


		/// <returns> the response </returns>
		public virtual CommonServerMessageBlockResponse getResponse() {
			return null;
		}


		/// 
		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.CommonServerMessageBlock#setResponse(jcifs.internal.CommonServerMessageBlockResponse) </seealso>
		public virtual void setResponse(CommonServerMessageBlockResponse msg) {

		}


		/// <returns> the errorData </returns>
		public virtual byte[] getErrorData() {
			return this.errorData;
		}


		/// <returns> the errorContextCount </returns>
		public virtual byte getErrorContextCount() {
			return this.errorContextCount;
		}


		/// <returns> the headerStart </returns>
		public virtual int getHeaderStart() {
			return this.headerStart;
		}


		/// <returns> the length </returns>
		public virtual int getLength() {
			return this.length;
		}


		public virtual int encode(byte[] dst, int dstIndex) {
			int start = this.headerStart = dstIndex;
			dstIndex += writeHeaderWireFormat(dst, dstIndex);

			this.byteCount = writeBytesWireFormat(dst, dstIndex);
			dstIndex += this.byteCount;
			dstIndex += pad8(dstIndex);

			this.length = dstIndex - start;

			int len = this.length;

			if (this.next != null) {
				int nextStart = dstIndex;
				dstIndex += this.next.encode(dst, dstIndex);
				int off = nextStart - start;
				SMBUtil.writeInt4(off, dst, start + 20);
				len += dstIndex - nextStart;
			}

			if (this.digest != null) {
				this.digest.sign(dst, this.headerStart, this.length, this, getResponse());
			}

			if (isRetainPayload()) {
				this.rawPayload = new byte[len];
				Array.Copy(dst, start, this.rawPayload, 0, len);
			}

			return len;
		}


		protected  static int size8(int size) {
			return size8(size, 0);
		}


		protected  static int size8(int size, int align) {

			int rem = size % 8 - align;
			if (rem == 0) {
				return size;
			}
			if (rem < 0) {
				rem = 8 + rem;
			}
			return size + 8 - rem;
		}


		/// <param name="dstIndex">
		/// @return </param>
		protected  int pad8(int dstIndex) {
			int fromHdr = dstIndex - this.headerStart;
			int rem = fromHdr % 8;
			if (rem == 0) {
				return 0;
			}
			return 8 - rem;
		}


		/// throws jcifs.internal.SMBProtocolDecodingException
		public virtual int decode(byte[] buffer, int bufferIndex) {
			return decode(buffer, bufferIndex, false);
		}


		/// <param name="buffer"> </param>
		/// <param name="bufferIndex"> </param>
		/// <param name="compound"> </param>
		/// <returns> decoded length </returns>
		/// <exception cref="SMBProtocolDecodingException"> </exception>
		/// throws jcifs.internal.SMBProtocolDecodingException
		public virtual int decode(byte[] buffer, int bufferIndex, bool compound) {
			int start = this.headerStart = bufferIndex;
			bufferIndex += readHeaderWireFormat(buffer, bufferIndex);
			if (isErrorResponseStatus()) {
				bufferIndex += readErrorResponse(buffer, bufferIndex);
			}
			else {
				bufferIndex += readBytesWireFormat(buffer, bufferIndex);
			}

			this.length = bufferIndex - start;
			int len = this.length;

			if (this.nextCommand != 0) {
				// padding becomes part of signature if this is _PART_ of a compound chain
				len += pad8(bufferIndex);
			}
			else if (compound && this.nextCommand == 0 && this.readSize > 0) {
				// TODO: only apply this for actual compound chains, or is this correct for single responses, too?
				// 3.2.5.1.9 Handling Compounded Responses
				// The final response in the compounded response chain will have NextCommand equal to 0,
				// and it MUST be processed as an individual message of a size equal to the number of bytes
				// remaining in this receive.
				int rem = this.readSize - this.length;
				len += rem;
			}

			haveResponse(buffer, start, len);

			if (this.nextCommand != 0 && this.next != null) {
				if (this.nextCommand % 8 != 0) {
					throw new SMBProtocolDecodingException("Chained command is not aligned");
				}
			}
			return len;
		}


		protected  virtual bool isErrorResponseStatus() {
			return getStatus() != 0;
		}


		/// <param name="buffer"> </param>
		/// <param name="start"> </param>
		/// <param name="len"> </param>
		/// <exception cref="SMBProtocolDecodingException"> </exception>
		/// throws jcifs.internal.SMBProtocolDecodingException
		protected  virtual void haveResponse(byte[] buffer, int start, int len) {
		}


		/// <param name="buffer"> </param>
		/// <param name="bufferIndex">
		/// @return </param>
		/// <exception cref="Smb2ProtocolDecodingException"> </exception>
		/// throws jcifs.internal.SMBProtocolDecodingException
		protected  virtual int readErrorResponse(byte[] buffer, int bufferIndex) {
			int start = bufferIndex;
			int structureSize = SMBUtil.readInt2(buffer, bufferIndex);
			if (structureSize != 9) {
				throw new SMBProtocolDecodingException("Error structureSize should be 9");
			}
			this.errorContextCount = buffer[bufferIndex + 2];
			bufferIndex += 4;

			int bc = SMBUtil.readInt4(buffer, bufferIndex);
			bufferIndex += 4;

			if (bc > 0) {
				this.errorData = new byte[bc];
				Array.Copy(buffer, bufferIndex, this.errorData, 0, bc);
				bufferIndex += bc;
			}
			return bufferIndex - start;
		}

		protected  virtual int writeHeaderWireFormat(byte[] dst, int dstIndex) {
			Array.Copy(SMBUtil.SMB2_HEADER, 0, dst, dstIndex, SMBUtil.SMB2_HEADER.Length);

			SMBUtil.writeInt2(this.creditCharge, dst, dstIndex + 6);
			SMBUtil.writeInt2(this.command, dst, dstIndex + 12);
			SMBUtil.writeInt2(this.credit, dst, dstIndex + 14);
			SMBUtil.writeInt4(this.flags, dst, dstIndex + 16);
			SMBUtil.writeInt4(this.nextCommand, dst, dstIndex + 20);
			SMBUtil.writeInt8(this.mid, dst, dstIndex + 24);

			if (_logger.isDebugEnabled())
			{
				_logger.debug($"Smb2 Request Message={this.GetType()},Mid={this.mid}");
			}

			if (this.async) {
				SMBUtil.writeInt8(this.asyncId, dst, dstIndex + 32);
				SMBUtil.writeInt8(this.sessionId, dst, dstIndex + 40);
			}
			else {
				// 4 reserved
				SMBUtil.writeInt4(this.treeId, dst, dstIndex + 36);
				SMBUtil.writeInt8(this.sessionId, dst, dstIndex + 40);
				// + signature
			}

			return Smb2Constants.SMB2_HEADER_LENGTH;
		}


		protected  virtual int readHeaderWireFormat(byte[] buffer, int bufferIndex) {
			// these are common between SYNC/ASYNC
			SMBUtil.readInt4(buffer, bufferIndex);
			bufferIndex += 4;
			SMBUtil.readInt2(buffer, bufferIndex);
			this.creditCharge = SMBUtil.readInt2(buffer, bufferIndex + 2);
			bufferIndex += 4;
			this.status = SMBUtil.readInt4(buffer, bufferIndex);
			bufferIndex += 4;
			this.command = SMBUtil.readInt2(buffer, bufferIndex);
			this.credit = SMBUtil.readInt2(buffer, bufferIndex + 2);
			bufferIndex += 4;

			this.flags = SMBUtil.readInt4(buffer, bufferIndex);
			bufferIndex += 4;
			this.nextCommand = SMBUtil.readInt4(buffer, bufferIndex);
			bufferIndex += 4;
			this.mid = SMBUtil.readInt8(buffer, bufferIndex);
			bufferIndex += 8;

			if ((this.flags & SMB2_FLAGS_ASYNC_COMMAND) == SMB2_FLAGS_ASYNC_COMMAND) {
				// async
				this.async = true;
				this.asyncId = SMBUtil.readInt8(buffer, bufferIndex);
				bufferIndex += 8;
				this.sessionId = SMBUtil.readInt8(buffer, bufferIndex);
				bufferIndex += 8;
				Array.Copy(buffer, bufferIndex, this.signature, 0, 16);
				bufferIndex += 16;
			}
			else {
				// sync
				this.async = false;
				bufferIndex += 4; // reserved
				this.treeId = SMBUtil.readInt4(buffer, bufferIndex);
				bufferIndex += 4;
				this.sessionId = SMBUtil.readInt8(buffer, bufferIndex);
				bufferIndex += 8;
				Array.Copy(buffer, bufferIndex, this.signature, 0, 16);
				bufferIndex += 16;
			}

			return Smb2Constants.SMB2_HEADER_LENGTH;
		}


		internal virtual bool isResponse() {
			return (this.flags & SMB2_FLAGS_SERVER_TO_REDIR) == SMB2_FLAGS_SERVER_TO_REDIR;
		}


		protected  abstract int writeBytesWireFormat(byte[] dst, int dstIndex);


		/// throws jcifs.internal.SMBProtocolDecodingException;
		protected  abstract int readBytesWireFormat(byte[] buffer, int bufferIndex);


		public override int GetHashCode() {
			return (int) this.mid;
		}


		public override bool Equals(object obj) {
			return obj is ServerMessageBlock2 && ((ServerMessageBlock2) obj).mid == this.mid;
		}


		public override string ToString() {
			string c;
			switch (this.command) {

			case SMB2_NEGOTIATE:
				c = "SMB2_NEGOTIATE";
				break;
			case SMB2_SESSION_SETUP:
				c = "SMB2_SESSION_SETUP";
				break;
			case SMB2_LOGOFF:
				c = "SMB2_LOGOFF";
				break;
			case SMB2_TREE_CONNECT:
				c = "SMB2_TREE_CONNECT";
				break;
			case SMB2_TREE_DISCONNECT:
				c = "SMB2_TREE_DISCONNECT";
				break;
			case SMB2_CREATE:
				c = "SMB2_CREATE";
				break;
			case SMB2_CLOSE:
				c = "SMB2_CLOSE";
				break;
			case SMB2_FLUSH:
				c = "SMB2_FLUSH";
				break;
			case SMB2_READ:
				c = "SMB2_READ";
				break;
			case SMB2_WRITE:
				c = "SMB2_WRITE";
				break;
			case SMB2_LOCK:
				c = "SMB2_LOCK";
				break;
			case SMB2_IOCTL:
				c = "SMB2_IOCTL";
				break;
			case SMB2_CANCEL:
				c = "SMB2_CANCEL";
				break;
			case SMB2_ECHO:
				c = "SMB2_ECHO";
				break;
			case SMB2_QUERY_DIRECTORY:
				c = "SMB2_QUERY_DIRECTORY";
				break;
			case SMB2_CHANGE_NOTIFY:
				c = "SMB2_CHANGE_NOTIFY";
				break;
			case SMB2_QUERY_INFO:
				c = "SMB2_QUERY_INFO";
				break;
			case SMB2_SET_INFO:
				c = "SMB2_SET_INFO";
				break;
			case SMB2_OPLOCK_BREAK:
				c = "SMB2_OPLOCK_BREAK";
				break;
			default:
				c = "UNKNOWN";
			break;
			}
			string str = this.status == 0 ? "0" : SmbException.getMessageByCode(this.status);
			return "command=" + c + ",status=" + str + ",flags=0x" + Hexdump.toHexString(this.flags, 4) + ",mid=" + this.mid + ",wordCount=" + this.wordCount + ",byteCount=" + this.byteCount;
		}

	}

}