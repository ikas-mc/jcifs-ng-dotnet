using System;
using CIFSContext = jcifs.CIFSContext;
using Configuration = jcifs.Configuration;
using jcifs.@internal.smb2;
using Smb2Constants = jcifs.@internal.smb2.Smb2Constants;
using SMBUtil = jcifs.@internal.util.SMBUtil;

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
namespace jcifs.@internal.smb2.notify {



	/// <summary>
	/// @author mbechler
	/// 
	/// </summary>
	public class Smb2ChangeNotifyRequest : ServerMessageBlock2Request<Smb2ChangeNotifyResponse> {

		/// 
		public const int SMB2_WATCH_TREE = 0x1;

		/// 
		public const int FILE_NOTIFY_CHANGE_FILE_NAME = 0x1;
		/// 
		public const int FILE_NOTIFY_CHANGE_DIR_NAME = 0x2;
		/// 
		public const int FILE_NOTIFY_CHANGE_ATTRIBUTES = 0x4;
		/// 
		public const int FILE_NOTIFY_CHANGE_SIZE = 0x8;
		/// 
		public const int FILE_NOTIFY_CHANGE_LAST_WRITE = 0x10;
		/// 
		public const int FILE_NOTIFY_CHANGE_LAST_ACCESS = 0x20;
		/// 
		public const int FILE_NOTIFY_CHANGE_CREATION = 0x40;
		/// 
		public const int FILE_NOTIFY_CHANGE_EA = 0x80;
		/// 
		public const int FILE_NOTIFY_CHANGE_SECURITY = 0x100;
		/// 
		public const int FILE_NOTIFY_CHANGE_STREAM_NAME = 0x200;
		/// 
		public const int FILE_NOTIFY_CHANGE_STREAM_SIZE = 0x400;
		/// 
		public const int FILE_NOTIFY_CHANGE_STREAM_WRITE = 0x800;

		private readonly byte[] fileId;
		private int outputBufferLength;
		private int notifyFlags;
		private int completionFilter;


		/// <param name="config"> </param>
		/// <param name="fileId"> </param>
		public Smb2ChangeNotifyRequest(Configuration config, byte[] fileId) : base(config, SMB2_CHANGE_NOTIFY) {
			this.outputBufferLength = config.getNotifyBufferSize();
			this.fileId = fileId;
		}


		/// <param name="notifyFlags">
		///            the notifyFlags to set </param>
		public virtual void setNotifyFlags(int notifyFlags) {
			this.notifyFlags = notifyFlags;
		}


		/// <param name="completionFilter">
		///            the completionFilter to set </param>
		public virtual void setCompletionFilter(int completionFilter) {
			this.completionFilter = completionFilter;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb2.ServerMessageBlock2Request#createResponse(jcifs.CIFSContext,
		///      jcifs.internal.smb2.ServerMessageBlock2Request) </seealso>
		protected  override Smb2ChangeNotifyResponse createResponse(CIFSContext tc, ServerMessageBlock2Request<Smb2ChangeNotifyResponse> req) {
			return new Smb2ChangeNotifyResponse(tc.getConfig());
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.CommonServerMessageBlockRequest#size() </seealso>
		public override int size() {
			return size8(Smb2Constants.SMB2_HEADER_LENGTH + 32);
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb2.ServerMessageBlock2#writeBytesWireFormat(byte[], int) </seealso>
		protected  override int writeBytesWireFormat(byte[] dst, int dstIndex) {
			int start = dstIndex;
			SMBUtil.writeInt2(32, dst, dstIndex);
			SMBUtil.writeInt2(this.notifyFlags, dst, dstIndex + 2);
			dstIndex += 4;

			SMBUtil.writeInt4(this.outputBufferLength, dst, dstIndex);
			dstIndex += 4;

			Array.Copy(this.fileId, 0, dst, dstIndex, 16);
			dstIndex += 16;

			SMBUtil.writeInt4(this.completionFilter, dst, dstIndex);
			dstIndex += 4;
			dstIndex += 4; // Reserved
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