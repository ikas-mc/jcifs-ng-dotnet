using System;
using System.Threading;
using jcifs.util.transport;
using Logger = org.slf4j.Logger;
using LoggerFactory = org.slf4j.LoggerFactory;
using Configuration = jcifs.Configuration;
using RuntimeCIFSException = jcifs.RuntimeCIFSException;
using SmbConstants = jcifs.SmbConstants;
using CommonServerMessageBlock = jcifs.@internal.CommonServerMessageBlock;
using CommonServerMessageBlockRequest = jcifs.@internal.CommonServerMessageBlockRequest;
using CommonServerMessageBlockResponse = jcifs.@internal.CommonServerMessageBlockResponse;
using RequestWithPath = jcifs.@internal.RequestWithPath;
using SMBProtocolDecodingException = jcifs.@internal.SMBProtocolDecodingException;
using SMBSigningDigest = jcifs.@internal.SMBSigningDigest;
using SMBUtil = jcifs.@internal.util.SMBUtil;
using SmbException = jcifs.smb.SmbException;
using Hexdump = jcifs.util.Hexdump;
using Strings = jcifs.util.Strings;

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
	/// 
	public abstract class ServerMessageBlock : CommonServerMessageBlockRequest, CommonServerMessageBlockResponse, RequestWithPath {

		private static Logger log = LoggerFactory.getLogger(typeof(ServerMessageBlock));

		/*
		 * These are all the smbs supported by this library. This includes requests
		 * and well as their responses for each type however the actuall implementations
		 * of the readXxxWireFormat and writeXxxWireFormat methods may not be in
		 * place. For example at the time of this writing the readXxxWireFormat
		 * for requests and the writeXxxWireFormat for responses are not implemented
		 * and simply return 0. These would need to be completed for a server
		 * implementation.
		 */

		/// 
		public const byte SMB_COM_CREATE_DIRECTORY = (byte) 0x00;

		/// 
		public const byte SMB_COM_DELETE_DIRECTORY = (byte) 0x01;

		/// 
		public const byte SMB_COM_CLOSE = (byte) 0x04;

		/// 
		public const byte SMB_COM_DELETE = (byte) 0x06;

		/// 
		public const byte SMB_COM_RENAME = (byte) 0x07;

		/// 
		public const byte SMB_COM_QUERY_INFORMATION = (byte) 0x08;

		/// 
		public const byte SMB_COM_SET_INFORMATION = (byte) 0x09;

		/// 
		public const byte SMB_COM_WRITE = (byte) 0x0B;

		/// 
		public const byte SMB_COM_CHECK_DIRECTORY = (byte) 0x10;

		/// 
		public const byte SMB_COM_SEEK = (byte) 0x12;

		/// 
		public const byte SMB_COM_LOCKING_ANDX = (byte) 0x24;

		/// 
		public const byte SMB_COM_TRANSACTION = (byte) 0x25;

		/// 
		public const byte SMB_COM_TRANSACTION_SECONDARY = (byte) 0x26;

		/// 
		public const byte SMB_COM_MOVE = (byte) 0x2A;

		/// 
		public const byte SMB_COM_ECHO = (byte) 0x2B;

		/// 
		public const byte SMB_COM_OPEN_ANDX = (byte) 0x2D;

		/// 
		public const byte SMB_COM_READ_ANDX = (byte) 0x2E;

		/// 
		public const byte SMB_COM_WRITE_ANDX = (byte) 0x2F;

		/// 
		public const byte SMB_COM_TRANSACTION2 = (byte) 0x32;

		/// 
		public const byte SMB_COM_FIND_CLOSE2 = (byte) 0x34;

		/// 
		public const byte SMB_COM_TREE_DISCONNECT = (byte) 0x71;

		/// 
		public const byte SMB_COM_NEGOTIATE = (byte) 0x72;

		/// 
		public const byte SMB_COM_SESSION_SETUP_ANDX = (byte) 0x73;

		/// 
		public const byte SMB_COM_LOGOFF_ANDX = (byte) 0x74;

		/// 
		public const byte SMB_COM_TREE_CONNECT_ANDX = (byte) 0x75;

		/// 
		public const byte SMB_COM_NT_TRANSACT = unchecked((byte) 0xA0);

		/// 
		public const byte SMB_COM_NT_CANCEL = unchecked((byte) 0xA4);

		/// 
		public const byte SMB_COM_NT_TRANSACT_SECONDARY = unchecked((byte) 0xA1);

		/// 
		public const byte SMB_COM_NT_CREATE_ANDX = unchecked((byte) 0xA2);

		/*
		 * Some fields specify the offset from the beginning of the header. This
		 * field should be used for calculating that. This would likely be zero
		 * but an implemantation that encorporates the transport header(for
		 * efficiency) might use a different initial bufferIndex. For example,
		 * to eliminate copying data when writing NbtSession data one might
		 * manage that 4 byte header specifically and therefore the initial
		 * bufferIndex, and thus headerStart, would be 4).(NOTE: If one where
		 * looking for a way to improve perfomance this is precisly what you
		 * would want to do as the jcifs.netbios.SocketXxxputStream classes
		 * arraycopy all data read or written into a new buffer shifted over 4!)
		 */

		private byte command, flags;
		protected internal int headerStart, length, batchLevel, errorCode, flags2, pid, uid, mid, wordCount, byteCount;
		protected internal int tid = 0xFFFF;
		private bool useUnicode, forceUnicode, extendedSecurity;
		private volatile bool receivedField;
		private int signSeq;
		private bool verifyFailed;
		protected internal string path;
		protected internal SMB1SigningDigest digest = null;
		private ServerMessageBlock response;

		private Configuration config;

		private long? expiration;

		private Exception exceptionField;

		private bool isErrorField;

		private byte[] rawPayload;

		private bool retainPayloadField;

		private string fullPath;
		private string server;
		private string domain;

		private int? overrideTimeout;


		protected internal ServerMessageBlock(Configuration config) : this(config, (byte) 0) {
		}


		protected internal ServerMessageBlock(Configuration config, byte command) : this(config, command, null) {
		}


		protected internal ServerMessageBlock(Configuration config, byte command, string path) {
			this.config = config;
			this.command = command;
			this.path = path;
			this.flags = (byte)(SmbConstants.FLAGS_PATH_NAMES_CASELESS | SmbConstants.FLAGS_PATH_NAMES_CANONICALIZED);
			this.pid = config.getPid();
			this.batchLevel = 0;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.CommonServerMessageBlockRequest#size() </seealso>
		public virtual int size() {
			return 0;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.CommonServerMessageBlockResponse#isAsync() </seealso>
		public virtual bool isAsync() {
			return false;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.CommonServerMessageBlockRequest#isResponseAsync() </seealso>
		public virtual bool isResponseAsync() {
			return false;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.CommonServerMessageBlockRequest#getOverrideTimeout() </seealso>
		public int? getOverrideTimeout() {
			return this.overrideTimeout;
		}


		/// <param name="overrideTimeout">
		///            the overrideTimeout to set </param>
		public void setOverrideTimeout(int? overrideTimeout) {
			this.overrideTimeout = overrideTimeout;
		}


		/// 
		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.CommonServerMessageBlockRequest#getNext() </seealso>
		public virtual CommonServerMessageBlockRequest getNext() {
			return null;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.CommonServerMessageBlockRequest#allowChain(jcifs.internal.CommonServerMessageBlockRequest) </seealso>
		public virtual bool allowChain(CommonServerMessageBlockRequest next) {
			return false;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.CommonServerMessageBlockRequest#split() </seealso>
		public virtual CommonServerMessageBlockRequest split() {
			return null;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.CommonServerMessageBlockRequest#createCancel() </seealso>
		public virtual CommonServerMessageBlockRequest createCancel() {
			return null;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.CommonServerMessageBlockResponse#getNextResponse() </seealso>
		public virtual CommonServerMessageBlockResponse getNextResponse() {
			return null;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.CommonServerMessageBlockResponse#prepare(jcifs.internal.CommonServerMessageBlockRequest) </seealso>
		public virtual void prepare(CommonServerMessageBlockRequest next) {

		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.util.transport.Request#getCreditCost() </seealso>
		public virtual int getCreditCost() {
			return 1;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.util.transport.Response#getGrantedCredits() </seealso>
		public virtual int getGrantedCredits() {
			return 1;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.util.transport.Request#setRequestCredits(int) </seealso>
		public virtual void setRequestCredits(int credits) {

		}


		/// <returns> the command </returns>
		public int getCommand() {
			return this.command;
		}


		/// <param name="command">
		///            the command to set </param>
		public void setCommand(int command) {
			this.command = (byte) command;
		}


		/// <returns> the byteCount </returns>
		public int getByteCount() {
			return this.byteCount;
		}


		/// <returns> the length </returns>
		public int getLength() {
			return this.length;
		}


		/// <returns> the forceUnicode </returns>
		public virtual bool isForceUnicode() {
			return this.forceUnicode;
		}


		/// <returns> the flags </returns>
		public byte getFlags() {
			return this.flags;
		}


		/// <param name="flags">
		///            the flags to set </param>
		public void setFlags(byte flags) {
			this.flags = flags;
		}


		/// <returns> the flags2 </returns>
		public int getFlags2() {
			return this.flags2;
		}


		/// <param name="fl">
		///            the flags2 to set </param>
		public void setFlags2(int fl) {
			this.flags2 = fl;
		}


		/// <param name="fl"> </param>
		public void addFlags2(int fl) {
			this.flags2 |= fl;
		}


		/// 
		/// <param name="fl"> </param>
		public void remFlags2(int fl) {
			this.flags2 &= ~fl;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.RequestWithPath#setResolveInDfs(boolean) </seealso>
		public virtual void setResolveInDfs(bool resolve) {
			if (resolve) {
				addFlags2(SmbConstants.FLAGS2_RESOLVE_PATHS_IN_DFS);
			}
			else {
				remFlags2(SmbConstants.FLAGS2_RESOLVE_PATHS_IN_DFS);
			}
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.RequestWithPath#isResolveInDfs() </seealso>
		public virtual bool isResolveInDfs() {
			return (getFlags() & SmbConstants.FLAGS2_RESOLVE_PATHS_IN_DFS) == SmbConstants.FLAGS2_RESOLVE_PATHS_IN_DFS;
		}


		/// <returns> the errorCode </returns>
		public int getErrorCode() {
			return this.errorCode;
		}


		/// <param name="errorCode">
		///            the errorCode to set </param>
		public void setErrorCode(int errorCode) {
			this.errorCode = errorCode;
		}


		/// <returns> the path </returns>
		public string getPath() {
			return this.path;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.RequestWithPath#getFullUNCPath() </seealso>
		public virtual string getFullUNCPath() {
			return this.fullPath;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.RequestWithPath#getDomain() </seealso>
		public virtual string getDomain() {
			return this.domain;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.RequestWithPath#getServer() </seealso>
		public virtual string getServer() {
			return this.server;
		}


		/// 
		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.RequestWithPath#setFullUNCPath(java.lang.String, java.lang.String, java.lang.String) </seealso>
		public virtual void setFullUNCPath(string domain, string server, string fullPath) {
			this.domain = domain;
			this.server = server;
			this.fullPath = fullPath;
		}


		/// <param name="path">
		///            the path to set </param>
		public void setPath(string path) {
			this.path = path;
		}


		/// <returns>  SMB1SigningDigest </returns>
		public SMBSigningDigest getDigest() {
			return (SMB1SigningDigest)this.digest;
		}


		/// <param name="digest">
		///            the digest to set </param>
		public void setDigest(SMBSigningDigest digest) {
			this.digest = (SMB1SigningDigest) digest;
		}


		/// <returns> the extendedSecurity </returns>
		public virtual bool isExtendedSecurity() {
			return this.extendedSecurity;
		}


		public void setSessionId(long sessionId) {
			// ignore
		}


		/// <param name="extendedSecurity">
		///            the extendedSecurity to set </param>
		public virtual void setExtendedSecurity(bool extendedSecurity) {
			this.extendedSecurity = extendedSecurity;
		}


		/// <returns> the useUnicode </returns>
		public bool isUseUnicode() {
			return this.useUnicode;
		}


		/// <param name="useUnicode">
		///            the useUnicode to set </param>
		public void setUseUnicode(bool useUnicode) {
			this.useUnicode = useUnicode;
		}


		/// <returns> the received </returns>
		public bool isReceived() {
			return this.receivedField;
		}


		public void clearReceived() {
			this.receivedField = false;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.util.transport.Response#received() </seealso>
		public virtual void received() {
			this.receivedField = true;
			lock (this) {
				Monitor.PulseAll(this);
			}
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.util.transport.Response#exception(java.lang.Exception) </seealso>
		public virtual void exception(Exception e) {
			this.exceptionField = e;
			lock (this) {
				Monitor.PulseAll(this);
			}
		}

	

		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.util.transport.Response#error() </seealso>
		public virtual void error() {
			this.isErrorField = true;
			lock (this) {
				Monitor.PulseAll(this);
			}
		}

		

		/// <returns> the response </returns>
		public virtual CommonServerMessageBlockResponse getResponse() {
			return this.response;
		}
		
		//TODO 
		 jcifs.util.transport.Response jcifs.util.transport.Request.getResponse() {
			return this.response;
		}
		 
		 Response Response.getNextResponse()
		 {
			 return getNextResponse();
		 }


		 Request Request.getNext()
		 {
			 return getNext();
		 }

		/// 
		/// <returns> null </returns>
		public virtual CommonServerMessageBlock ignoreDisconnect() {
			return this;
		}


		/// <param name="response">
		///            the response to set </param>
		public void setResponse(CommonServerMessageBlockResponse response) {
			if (!(response is ServerMessageBlock)) {
				throw new System.ArgumentException();
			}
			this.response = (ServerMessageBlock) response;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.util.transport.Request#isCancel() </seealso>
		public virtual bool isCancel() {
			return false;
		}


		/// <returns> the mid </returns>
		public long getMid() {
			return this.mid;
		}


		/// <param name="mid">
		///            the mid to set </param>
		public void setMid(long mid) {
			this.mid = (int) mid;
		}


		/// <returns> the tid </returns>
		public int getTid() {
			return this.tid;
		}


		/// <param name="tid">
		///            the tid to set </param>
		public void setTid(int tid) {
			this.tid = tid;
		}


		/// <returns> the pid </returns>
		public int getPid() {
			return this.pid;
		}


		/// <param name="pid">
		///            the pid to set </param>
		public void setPid(int pid) {
			this.pid = pid;
		}


		/// <returns> the uid </returns>
		public int getUid() {
			return this.uid;
		}


		/// <param name="uid">
		///            the uid to set </param>
		public void setUid(int uid) {
			this.uid = uid;
		}


		/// <returns> the signSeq </returns>
		public virtual int getSignSeq() {
			return this.signSeq;
		}


		/// <param name="signSeq">
		///            the signSeq to set </param>
		public void setSignSeq(int signSeq) {
			this.signSeq = signSeq;
		}


		/// <returns> the verifyFailed </returns>
		public virtual bool isVerifyFailed() {
			return this.verifyFailed;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.util.transport.Response#getException() </seealso>
		public virtual Exception getException() {
			return this.exceptionField;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.util.transport.Response#isError() </seealso>
		public virtual bool isError() {
			return this.isErrorField;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.util.transport.Response#getRawPayload() </seealso>
		public virtual byte[] getRawPayload() {
			return this.rawPayload;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.util.transport.Response#setRawPayload(byte[]) </seealso>
		public virtual void setRawPayload(byte[] rawPayload) {
			this.rawPayload = rawPayload;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.util.transport.Response#isRetainPayload() </seealso>
		public virtual bool isRetainPayload() {
			return this.retainPayloadField;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.util.transport.Response#retainPayload() </seealso>
		public virtual void retainPayload() {
			this.retainPayloadField = true;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.util.transport.Response#getExpiration() </seealso>
		public virtual long? getExpiration() {
			return this.expiration;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.util.transport.Response#setExpiration(java.lang.Long) </seealso>
		public virtual void setExpiration(long? exp) {
			this.expiration = exp;
		}


		/// <returns> the config </returns>
		protected internal Configuration getConfig() {
			return this.config;
		}


		/// 
		public virtual void reset() {
			this.flags = (byte)(SmbConstants.FLAGS_PATH_NAMES_CASELESS | SmbConstants.FLAGS_PATH_NAMES_CANONICALIZED);
			this.flags2 = 0;
			this.errorCode = 0;
			this.receivedField = false;
			this.digest = null;
			this.uid = 0;
			this.tid = 0xFFFF;
		}


		/// 
		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.util.transport.Response#verifySignature(byte[], int, int) </seealso>
		public virtual bool verifySignature(byte[] buffer, int i, int size) {
			/*
			 * Verification fails (w/ W2K3 server at least) if status is not 0. This
			 * suggests MS doesn't compute the signature (correctly) for error responses
			 * (perhaps for DOS reasons).
			 */
			/*
			 * Looks like the failure case also is just reflecting back the signature we sent
			 */

			/// <summary>
			/// Maybe this is related:
			/// 
			/// If signing is not active, the SecuritySignature field of the SMB Header for all messages sent, except
			/// the SMB_COM_SESSION_SETUP_ANDX Response (section 2.2.4.53.2), MUST be set to
			/// 0x0000000000000000. For the SMB_COM_SESSION_SETUP_ANDX Response, the SecuritySignature
			/// field of the SMB Header SHOULD<226> be set to the SecuritySignature received in the
			/// SMB_COM_SESSION_SETUP_ANDX Request (section 2.2.4.53.1).
			/// </summary>
			if (this.digest != null && getErrorCode() == 0) {
				bool verify = this.digest.verify(buffer, i, size, 0, this);
				this.verifyFailed = verify;
				return !verify;
			}
			return true;
		}


		protected internal virtual int writeString(string str, byte[] dst, int dstIndex) {
			return writeString(str, dst, dstIndex, this.useUnicode);
		}


		protected internal virtual int writeString(string str, byte[] dst, int dstIndex, bool unicode) {
			int start = dstIndex;
			if (unicode) {
				// Unicode requires word alignment
				if (((dstIndex - this.headerStart) % 2) != 0) {
					dst[dstIndex++] = (byte) '\0';
				}
				Array.Copy(Strings.getUNIBytes(str), 0, dst, dstIndex, str.Length * 2);
				dstIndex += str.Length * 2;
				dst[dstIndex++] = (byte) '\0';
				dst[dstIndex++] = (byte) '\0';
			}
			else {
				byte[] b = Strings.getOEMBytes(str, this.getConfig());
				Array.Copy(b, 0, dst, dstIndex, b.Length);
				dstIndex += b.Length;
				dst[dstIndex++] = (byte) '\0';
			}
			return dstIndex - start;
		}


		/// 
		/// <param name="src"> </param>
		/// <param name="srcIndex"> </param>
		/// <returns> read string </returns>
		public virtual string readString(byte[] src, int srcIndex) {
			return readString(src, srcIndex, 255, this.useUnicode);
		}


		/// 
		/// <param name="src"> </param>
		/// <param name="srcIndex"> </param>
		/// <param name="maxLen"> </param>
		/// <param name="unicode"> </param>
		/// <returns> read string </returns>
		public virtual string readString(byte[] src, int srcIndex, int maxLen, bool unicode) {
			if (unicode) {
				// Unicode requires word alignment
				if (((srcIndex - this.headerStart) % 2) != 0) {
					srcIndex++;
				}
				return Strings.fromUNIBytes(src, srcIndex, Strings.findUNITermination(src, srcIndex, maxLen));
			}

			return Strings.fromOEMBytes(src, srcIndex, Strings.findTermination(src, srcIndex, maxLen), getConfig());
		}


		/// 
		/// <param name="src"> </param>
		/// <param name="srcIndex"> </param>
		/// <param name="srcEnd"> </param>
		/// <param name="maxLen"> </param>
		/// <param name="unicode"> </param>
		/// <returns> read string </returns>
		public virtual string readString(byte[] src, int srcIndex, int srcEnd, int maxLen, bool unicode) {
			if (unicode) {
				// Unicode requires word alignment
				if (((srcIndex - this.headerStart) % 2) != 0) {
					srcIndex++;
				}
				return Strings.fromUNIBytes(src, srcIndex, Strings.findUNITermination(src, srcIndex, maxLen));
			}

			return Strings.fromOEMBytes(src, srcIndex, Strings.findTermination(src, srcIndex, maxLen), getConfig());
		}


		/// 
		/// <param name="str"> </param>
		/// <param name="offset"> </param>
		/// <returns> string length </returns>
		public virtual int stringWireLength(string str, int offset) {
			int len = str.Length + 1;
			if (this.useUnicode) {
				len = str.Length * 2 + 2;
				len = (offset % 2) != 0 ? len + 1 : len;
			}
			return len;
		}


		protected internal virtual int readStringLength(byte[] src, int srcIndex, int max) {
			int len = 0;
			while (src[srcIndex + len] != (byte) 0x00) {
				if (len++ > max) {
					throw new RuntimeCIFSException("zero termination not found: " + this);
				}
			}
			return len;
		}


		public virtual int encode(byte[] dst, int dstIndex) {
			int start = this.headerStart = dstIndex;

			dstIndex += writeHeaderWireFormat(dst, dstIndex);
			this.wordCount = writeParameterWordsWireFormat(dst, dstIndex + 1);
			dst[dstIndex++] = unchecked((byte)((this.wordCount / 2) & 0xFF));
			dstIndex += this.wordCount;
			this.wordCount /= 2;
			this.byteCount = writeBytesWireFormat(dst, dstIndex + 2);
			dst[dstIndex++] = unchecked((byte)(this.byteCount & 0xFF));
			dst[dstIndex++] = unchecked((byte)((this.byteCount >> 8) & 0xFF));
			dstIndex += this.byteCount;

			this.length = dstIndex - start;

			if (this.digest != null) {
				this.digest.sign(dst, this.headerStart, this.length, this, this.response);
			}

			return this.length;
		}


		/// throws jcifs.internal.SMBProtocolDecodingException
		public virtual int decode(byte[] buffer, int bufferIndex) {
			int start = this.headerStart = bufferIndex;

			bufferIndex += readHeaderWireFormat(buffer, bufferIndex);

			this.wordCount = buffer[bufferIndex++];
			if (this.wordCount != 0) {
				int n;
				if ((n = readParameterWordsWireFormat(buffer, bufferIndex)) != this.wordCount * 2) {
					if (log.isTraceEnabled()) {
						log.trace("wordCount * 2=" + (this.wordCount * 2) + " but readParameterWordsWireFormat returned " + n);
					}
				}
				bufferIndex += this.wordCount * 2;
			}

			this.byteCount = SMBUtil.readInt2(buffer, bufferIndex);
			bufferIndex += 2;

			if (this.byteCount != 0) {
				int n;
				if ((n = readBytesWireFormat(buffer, bufferIndex)) != this.byteCount) {
					if (log.isTraceEnabled()) {
						log.trace("byteCount=" + this.byteCount + " but readBytesWireFormat returned " + n);
					}
				}
				// Don't think we can rely on n being correct here. Must use byteCount.
				// Last paragraph of section 3.13.3 eludes to this.

				bufferIndex += this.byteCount;
			}

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


		protected internal virtual int writeHeaderWireFormat(byte[] dst, int dstIndex) {
			Array.Copy(SMBUtil.SMB_HEADER, 0, dst, dstIndex, SMBUtil.SMB_HEADER.Length);
			dst[dstIndex + SmbConstants.CMD_OFFSET] = this.command;
			dst[dstIndex + SmbConstants.FLAGS_OFFSET] = this.flags;
			SMBUtil.writeInt2(this.flags2, dst, dstIndex + SmbConstants.FLAGS_OFFSET + 1);
			dstIndex += SmbConstants.TID_OFFSET;
			SMBUtil.writeInt2(this.tid, dst, dstIndex);
			SMBUtil.writeInt2(this.pid, dst, dstIndex + 2);
			SMBUtil.writeInt2(this.uid, dst, dstIndex + 4);
			SMBUtil.writeInt2(this.mid, dst, dstIndex + 6);
			return SmbConstants.SMB1_HEADER_LENGTH;
		}


		protected internal virtual int readHeaderWireFormat(byte[] buffer, int bufferIndex) {
			this.command = buffer[bufferIndex + SmbConstants.CMD_OFFSET];
			this.errorCode = SMBUtil.readInt4(buffer, bufferIndex + SmbConstants.ERROR_CODE_OFFSET);
			this.flags = buffer[bufferIndex + SmbConstants.FLAGS_OFFSET];
			this.flags2 = SMBUtil.readInt2(buffer, bufferIndex + SmbConstants.FLAGS_OFFSET + 1);
			this.tid = SMBUtil.readInt2(buffer, bufferIndex + SmbConstants.TID_OFFSET);
			this.pid = SMBUtil.readInt2(buffer, bufferIndex + SmbConstants.TID_OFFSET + 2);
			this.uid = SMBUtil.readInt2(buffer, bufferIndex + SmbConstants.TID_OFFSET + 4);
			this.mid = SMBUtil.readInt2(buffer, bufferIndex + SmbConstants.TID_OFFSET + 6);
			return SmbConstants.SMB1_HEADER_LENGTH;
		}


		protected internal virtual bool isResponse() {
			return (this.flags & SmbConstants.FLAGS_RESPONSE) == SmbConstants.FLAGS_RESPONSE;
		}


		/*
		 * For this packet deconstruction technique to work for
		 * other networking protocols the InputStream may need
		 * to be passed to the readXxxWireFormat methods. This is
		 * actually purer. However, in the case of smb we know the
		 * wordCount and byteCount. And since every subclass of
		 * ServerMessageBlock would have to perform the same read
		 * operation on the input stream, we might as will pull that
		 * common functionality into the superclass and read wordCount
		 * and byteCount worth of data.
		 * 
		 * We will still use the readXxxWireFormat return values to
		 * indicate how many bytes(note: readParameterWordsWireFormat
		 * returns bytes read and not the number of words(but the
		 * wordCount member DOES store the number of words)) we
		 * actually read. Incedentally this is important to the
		 * AndXServerMessageBlock class that needs to potentially
		 * read in another smb's parameter words and bytes based on
		 * information in it's andxCommand, andxOffset, ...etc.
		 */

		protected internal abstract int writeParameterWordsWireFormat(byte[] dst, int dstIndex);


		protected internal abstract int writeBytesWireFormat(byte[] dst, int dstIndex);


		protected internal abstract int readParameterWordsWireFormat(byte[] buffer, int bufferIndex);


		/// throws jcifs.internal.SMBProtocolDecodingException;
		protected internal abstract int readBytesWireFormat(byte[] buffer, int bufferIndex);


		public override int GetHashCode() {
			return this.mid;
		}


		public override bool Equals(object obj) {
			return obj is ServerMessageBlock && ((ServerMessageBlock) obj).mid == this.mid;
		}


		public override string ToString() {
			string c;
			switch (this.command) {
			case SMB_COM_NEGOTIATE:
				c = "SMB_COM_NEGOTIATE";
				break;
			case SMB_COM_SESSION_SETUP_ANDX:
				c = "SMB_COM_SESSION_SETUP_ANDX";
				break;
			case SMB_COM_TREE_CONNECT_ANDX:
				c = "SMB_COM_TREE_CONNECT_ANDX";
				break;
			case SMB_COM_QUERY_INFORMATION:
				c = "SMB_COM_QUERY_INFORMATION";
				break;
			case SMB_COM_CHECK_DIRECTORY:
				c = "SMB_COM_CHECK_DIRECTORY";
				break;
			case SMB_COM_TRANSACTION:
				c = "SMB_COM_TRANSACTION";
				break;
			case SMB_COM_TRANSACTION2:
				c = "SMB_COM_TRANSACTION2";
				break;
			case SMB_COM_TRANSACTION_SECONDARY:
				c = "SMB_COM_TRANSACTION_SECONDARY";
				break;
			case SMB_COM_FIND_CLOSE2:
				c = "SMB_COM_FIND_CLOSE2";
				break;
			case SMB_COM_TREE_DISCONNECT:
				c = "SMB_COM_TREE_DISCONNECT";
				break;
			case SMB_COM_LOGOFF_ANDX:
				c = "SMB_COM_LOGOFF_ANDX";
				break;
			case SMB_COM_ECHO:
				c = "SMB_COM_ECHO";
				break;
			case SMB_COM_MOVE:
				c = "SMB_COM_MOVE";
				break;
			case SMB_COM_RENAME:
				c = "SMB_COM_RENAME";
				break;
			case SMB_COM_DELETE:
				c = "SMB_COM_DELETE";
				break;
			case SMB_COM_DELETE_DIRECTORY:
				c = "SMB_COM_DELETE_DIRECTORY";
				break;
			case SMB_COM_NT_CREATE_ANDX:
				c = "SMB_COM_NT_CREATE_ANDX";
				break;
			case SMB_COM_OPEN_ANDX:
				c = "SMB_COM_OPEN_ANDX";
				break;
			case SMB_COM_READ_ANDX:
				c = "SMB_COM_READ_ANDX";
				break;
			case SMB_COM_CLOSE:
				c = "SMB_COM_CLOSE";
				break;
			case SMB_COM_WRITE_ANDX:
				c = "SMB_COM_WRITE_ANDX";
				break;
			case SMB_COM_CREATE_DIRECTORY:
				c = "SMB_COM_CREATE_DIRECTORY";
				break;
			case SMB_COM_NT_TRANSACT:
				c = "SMB_COM_NT_TRANSACT";
				break;
			case SMB_COM_NT_TRANSACT_SECONDARY:
				c = "SMB_COM_NT_TRANSACT_SECONDARY";
				break;
			case SMB_COM_LOCKING_ANDX:
				c = "SMB_COM_LOCKING_ANDX";
				break;
			default:
				c = "UNKNOWN";
			break;
			}
			string str = this.errorCode == 0 ? "0" : SmbException.getMessageByCode(this.errorCode);
			return "command=" + c + ",received=" + this.receivedField + ",errorCode=" + str + ",flags=0x" + Hexdump.toHexString(this.flags & 0xFF, 4) + ",flags2=0x" + Hexdump.toHexString(this.flags2, 4) + ",signSeq=" + this.signSeq + ",tid=" + this.tid + ",pid=" + this.pid + ",uid=" + this.uid + ",mid=" + this.mid + ",wordCount=" + this.wordCount + ",byteCount=" + this.byteCount;
		}

	}

}