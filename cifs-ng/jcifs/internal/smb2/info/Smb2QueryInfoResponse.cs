using jcifs;

using System;
using jcifs.@internal.fscc;
using CIFSException = jcifs.CIFSException;
using Configuration = jcifs.Configuration;
using Decodable = jcifs.Decodable;
using SMBProtocolDecodingException = jcifs.@internal.SMBProtocolDecodingException;
using SecurityDescriptor = jcifs.@internal.dtyp.SecurityDescriptor;
using FileFsFullSizeInformation = jcifs.@internal.fscc.FileFsFullSizeInformation;
using FileFsSizeInformation = jcifs.@internal.fscc.FileFsSizeInformation;
using FileInformation = jcifs.@internal.fscc.FileInformation;
using FileInternalInfo = jcifs.@internal.fscc.FileInternalInfo;
using FileSystemInformation = jcifs.@internal.fscc.FileSystemInformation;
using ServerMessageBlock2Response = jcifs.@internal.smb2.ServerMessageBlock2Response;
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
namespace jcifs.@internal.smb2.info {



	/// <summary>
	/// @author mbechler
	/// 
	/// </summary>
	public class Smb2QueryInfoResponse : ServerMessageBlock2Response {

		private byte expectInfoType;
		private byte expectInfoClass;
		private Decodable info;


		/// <param name="config"> </param>
		/// <param name="expectInfoType"> </param>
		/// <param name="expectInfoClass"> </param>
		public Smb2QueryInfoResponse(Configuration config, byte expectInfoType, byte expectInfoClass) : base(config) {
			this.expectInfoType = expectInfoType;
			this.expectInfoClass = expectInfoClass;
		}


		/// <returns> the information </returns>
		public virtual Decodable getInfo() {
			return this.info;
		}


		/// <param name="clazz"> </param>
		/// <returns> the information </returns>
		/// <exception cref="CIFSException"> </exception>
		/// throws jcifs.CIFSException
		public virtual T getInfo<T>(Type clazz) where T : Decodable {
			if (!clazz.IsAssignableFrom(this.info.GetType())) {
				throw new CIFSException("Incompatible file information class");
			}
			return (T) getInfo();
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
			int bufferLength = SMBUtil.readInt4(buffer, bufferIndex);
			bufferIndex += 4;
			Decodable i = createInformation(this.expectInfoType, this.expectInfoClass);
			if (i != null) {
				i.decode(buffer, bufferOffset, bufferLength);
			}
			bufferIndex = Math.Max(bufferIndex, bufferOffset + bufferLength);
			this.info = i;
			return bufferIndex - start;
		}


		/// throws jcifs.internal.SMBProtocolDecodingException
		private static Decodable createInformation(byte infoType, byte infoClass) {

			switch (infoType) {
			case Smb2Constants.SMB2_0_INFO_FILE:
				return createFileInformation(infoClass);
			case Smb2Constants.SMB2_0_INFO_FILESYSTEM:
				return createFilesystemInformation(infoClass);
			case Smb2Constants.SMB2_0_INFO_QUOTA:
				return createQuotaInformation(infoClass);
			case Smb2Constants.SMB2_0_INFO_SECURITY:
				return createSecurityInformation(infoClass);
			default:
				throw new SMBProtocolDecodingException("Unknwon information type " + infoType);
			}
		}


		/// <param name="infoClass">
		/// @return </param>
		/// <exception cref="SMBProtocolDecodingException"> </exception>
		/// throws jcifs.internal.SMBProtocolDecodingException
		private static Decodable createFilesystemInformation(byte infoClass) {
			switch (infoClass) {
			case FileSystemInformationConstants.FS_FULL_SIZE_INFO:
				return new FileFsFullSizeInformation();
			case FileSystemInformationConstants.FS_SIZE_INFO:
				return new FileFsSizeInformation();
			default:
				throw new SMBProtocolDecodingException("Unknown filesystem info class " + infoClass);
			}
		}


		/// <param name="infoClass">
		/// @return </param>
		/// <exception cref="SMBProtocolDecodingException"> </exception>
		/// throws jcifs.internal.SMBProtocolDecodingException
		private static Decodable createSecurityInformation(byte infoClass) {
			return new SecurityDescriptor();
		}


		/// <param name="infoClass">
		/// @return </param>
		/// <exception cref="SMBProtocolDecodingException"> </exception>
		/// throws jcifs.internal.SMBProtocolDecodingException
		private static Decodable createQuotaInformation(byte infoClass) {
			switch (infoClass) {
			default:
				throw new SMBProtocolDecodingException("Unknown quota info class " + infoClass);
			}
		}


		/// <param name="infoClass">
		/// @return </param>
		/// <exception cref="SMBProtocolDecodingException"> </exception>
		/// throws jcifs.internal.SMBProtocolDecodingException
		private static Decodable createFileInformation(byte infoClass) {
			switch (infoClass) {
			case FileInformationConstants.FILE_INTERNAL_INFO:
				return new FileInternalInfo();
			default:
				throw new SMBProtocolDecodingException("Unknown file info class " + infoClass);
			}
		}

	}

}