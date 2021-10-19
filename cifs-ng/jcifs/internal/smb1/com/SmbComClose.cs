using Logger = org.slf4j.Logger;
using LoggerFactory = org.slf4j.LoggerFactory;
using CIFSContext = jcifs.CIFSContext;
using Configuration = jcifs.Configuration;
using jcifs.@internal;
using SMB1SigningDigest = jcifs.@internal.smb1.SMB1SigningDigest;
using ServerMessageBlock = jcifs.@internal.smb1.ServerMessageBlock;
using SMBUtil = jcifs.@internal.util.SMBUtil;

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
	/// 
	public class SmbComClose : ServerMessageBlock, Request<SmbComBlankResponse> {

		private static readonly Logger log = LoggerFactory.getLogger(typeof(SmbComClose));

		private int fid;
		private long lastWriteTime;


		/// 
		/// <param name="config"> </param>
		/// <param name="fid"> </param>
		/// <param name="lastWriteTime"> </param>
		public SmbComClose(Configuration config, int fid, long lastWriteTime) : base(config, SMB_COM_CLOSE) {
			this.fid = fid;
			this.lastWriteTime = lastWriteTime;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb1.ServerMessageBlock#getResponse() </seealso>
		public sealed override CommonServerMessageBlockResponse getResponse() {
			return (SmbComBlankResponse) base.getResponse();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.Request#initResponse(jcifs.CIFSContext) </seealso>
		public virtual SmbComBlankResponse initResponse(CIFSContext tc) {
			SmbComBlankResponse resp = new SmbComBlankResponse(tc.getConfig());
			setResponse(resp);
			return resp;
		}

		SmbComBlankResponse Request<SmbComBlankResponse>.getResponse()
		{
			return (SmbComBlankResponse)getResponse();
		}


		protected internal override int writeParameterWordsWireFormat(byte[] dst, int dstIndex) {
			SMBUtil.writeInt2(this.fid, dst, dstIndex);
			dstIndex += 2;
			if (this.digest != null) {
				SMB1SigningDigest.writeUTime(getConfig(), this.lastWriteTime, dst, dstIndex);
			}
			else {
				log.trace("SmbComClose without a digest");
			}
			return 6;
		}


		protected internal override int writeBytesWireFormat(byte[] dst, int dstIndex) {
			return 0;
		}


		protected internal override int readParameterWordsWireFormat(byte[] buffer, int bufferIndex) {
			return 0;
		}


		protected internal override int readBytesWireFormat(byte[] buffer, int bufferIndex) {
			return 0;
		}


		public override string ToString() {
			return "SmbComClose[" + base.ToString() + ",fid=" + this.fid + ",lastWriteTime=" + this.lastWriteTime + "]";
		}
	}

}