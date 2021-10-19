using System.Collections.Generic;
using Configuration = jcifs.Configuration;
using FileNotifyInformation = jcifs.FileNotifyInformation;
using NotifyResponse = jcifs.@internal.NotifyResponse;
using SMBProtocolDecodingException = jcifs.@internal.SMBProtocolDecodingException;
using FileNotifyInformationImpl = jcifs.@internal.smb1.trans.nt.FileNotifyInformationImpl;
using ServerMessageBlock2Response = jcifs.@internal.smb2.ServerMessageBlock2Response;
using SMBUtil = jcifs.@internal.util.SMBUtil;
using NtStatus = jcifs.smb.NtStatus;

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
	public class Smb2ChangeNotifyResponse : ServerMessageBlock2Response, NotifyResponse {

		private IList<FileNotifyInformation> notifyInformation = new List<FileNotifyInformation>();


		/// <param name="config"> </param>
		public Smb2ChangeNotifyResponse(Configuration config) : base(config) {
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb2.ServerMessageBlock2#writeBytesWireFormat(byte[], int) </seealso>
		protected  override int writeBytesWireFormat(byte[] dst, int dstIndex) {
			return 0;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <exception cref="SMBProtocolDecodingException">
		/// </exception>
		/// <seealso cref= jcifs.internal.smb2.ServerMessageBlock2#readBytesWireFormat(byte[], int) </seealso>
		/// throws jcifs.internal.SMBProtocolDecodingException
		protected  override int readBytesWireFormat(byte[] buffer, int bufferIndex) {
			int start = bufferIndex;

			int structureSize = SMBUtil.readInt2(buffer, bufferIndex);
			if (structureSize != 9) {
				throw new SMBProtocolDecodingException("Expected structureSize = 9");
			}

			int bufferOffset = SMBUtil.readInt2(buffer, bufferIndex + 2) + getHeaderStart();
			bufferIndex += 4;
			int len = SMBUtil.readInt4(buffer, bufferIndex);
			bufferIndex += 4;

			int elemStart = bufferOffset;
			FileNotifyInformationImpl i = new FileNotifyInformationImpl();
			bufferIndex += i.decode(buffer, bufferOffset, len);
			this.notifyInformation.Add(i);

			while (i.getNextEntryOffset() > 0 && bufferIndex < bufferOffset + len) {
				bufferIndex = elemStart + i.getNextEntryOffset();
				elemStart = bufferIndex;

				i = new FileNotifyInformationImpl();
				bufferIndex += i.decode(buffer, bufferIndex, len);
				this.notifyInformation.Add(i);
			}

			return bufferIndex - start;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.NotifyResponse#getNotifyInformation() </seealso>
		public virtual IList<FileNotifyInformation> getNotifyInformation() {
			return this.notifyInformation;
		}

		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb2.ServerMessageBlock2#isErrorResponseStatus() </seealso>
		protected  override bool isErrorResponseStatus() {
			return getStatus() != NtStatus.NT_STATUS_NOTIFY_ENUM_DIR && base.isErrorResponseStatus();
		}


	}

}