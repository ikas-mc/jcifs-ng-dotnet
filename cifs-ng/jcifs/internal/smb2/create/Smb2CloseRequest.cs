using System;
using System.Diagnostics;
using Logger = org.slf4j.Logger;
using LoggerFactory = org.slf4j.LoggerFactory;
using CIFSContext = jcifs.CIFSContext;
using Configuration = jcifs.Configuration;
using RequestWithFileId = jcifs.@internal.smb2.RequestWithFileId;
using jcifs.@internal.smb2;
using Smb2Constants = jcifs.@internal.smb2.Smb2Constants;
using SMBUtil = jcifs.@internal.util.SMBUtil;
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
namespace jcifs.@internal.smb2.create {




	/// <summary>
	/// @author mbechler
	/// 
	/// </summary>
	public class Smb2CloseRequest : ServerMessageBlock2Request<Smb2CloseResponse>, RequestWithFileId {

		private static readonly Logger log = LoggerFactory.getLogger(typeof(Smb2CloseRequest));

		private byte[] fileId;
		private readonly string fileName;
		private int closeFlags;


		/// <param name="config"> </param>
		/// <param name="fileId"> </param>
		/// <param name="fileName"> </param>
		public Smb2CloseRequest(Configuration config, byte[] fileId, string fileName) : base(config, SMB2_CLOSE) {
			this.fileId = fileId;
			this.fileName = fileName;
		}


		/// 
		/// <param name="config"> </param>
		/// <param name="fileId"> </param>
		public Smb2CloseRequest(Configuration config, byte[] fileId) : this(config, fileId, "") {
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb2.RequestWithFileId#setFileId(byte[]) </seealso>
		public virtual void setFileId(byte[] fileId) {
			this.fileId = fileId;
		}


		/// <param name="config"> </param>
		/// <param name="fileName"> </param>
		public Smb2CloseRequest(Configuration config, string fileName) : this(config, Smb2Constants.UNSPECIFIED_FILEID, fileName) {
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb2.ServerMessageBlock2#createResponse(jcifs.Configuration,
		///      jcifs.internal.smb2.ServerMessageBlock2) </seealso>
		protected  override Smb2CloseResponse createResponse(CIFSContext tc, ServerMessageBlock2Request<Smb2CloseResponse> req) {
			return new Smb2CloseResponse(tc.getConfig(), this.fileId, this.fileName);
		}


		/// <param name="flags">
		///            the flags to set </param>
		public virtual void setCloseFlags(int flags) {
			this.closeFlags = flags;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.CommonServerMessageBlockRequest#size() </seealso>
		public override int size() {
			return size8(Smb2Constants.SMB2_HEADER_LENGTH + 24);
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb2.ServerMessageBlock2#writeBytesWireFormat(byte[], int) </seealso>
		protected  override int writeBytesWireFormat(byte[] dst, int dstIndex) {
			int start = dstIndex;
			SMBUtil.writeInt2(24, dst, dstIndex);
			SMBUtil.writeInt2(this.closeFlags, dst, dstIndex + 2);
			dstIndex += 4;
			dstIndex += 4; // Reserved
			Array.Copy(this.fileId, 0, dst, dstIndex, 16);
			dstIndex += 16;

			if (log.isDebugEnabled()) {
				log.debug(string.Format("Closing {0} ({1})", Hexdump.toHexString(this.fileId), this.fileName));
			}
			return dstIndex - start;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb2.ServerMessageBlock2#readBytesWireFormat(byte[], int) </seealso>
		protected  override int readBytesWireFormat(byte[] buffer, int bufferIndex) {
			return 0;
		}
	}

}