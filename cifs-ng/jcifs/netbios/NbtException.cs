using CIFSException = jcifs.CIFSException;

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

namespace jcifs.netbios {



	public class NbtException : CIFSException {

		/// 
		private const long serialVersionUID = 492638554095148960L;
		// error classes
		public const int SUCCESS = 0;
		public const int ERR_NAM_SRVC = 0x01;
		public const int ERR_SSN_SRVC = 0x02;

		// name service error codes
		public const int FMT_ERR = 0x1;
		public const int SRV_ERR = 0x2;
		public const int IMP_ERR = 0x4;
		public const int RFS_ERR = 0x5;
		public const int ACT_ERR = 0x6;
		public const int CFT_ERR = 0x7;

		// session service error codes
		public const int CONNECTION_REFUSED = -1;
		public const int NOT_LISTENING_CALLED = 0x80;
		public const int NOT_LISTENING_CALLING = 0x81;
		public const int CALLED_NOT_PRESENT = 0x82;
		public const int NO_RESOURCES = 0x83;
		public const int UNSPECIFIED = 0x8F;

		public int errorClass;
		public int errorCode;


		public static string getErrorString(int errorClass, int errorCode) {
			string result = "";
			switch (errorClass) {
			case SUCCESS:
				result += "SUCCESS";
				break;
			case ERR_NAM_SRVC:
				result += "ERR_NAM_SRVC/";
				switch (errorCode) {
				case FMT_ERR:
					result += "FMT_ERR: Format Error";
					goto default;
				default:
					result += "Unknown error code: " + errorCode;
				break;
				}
				break;
			case ERR_SSN_SRVC:
				result += "ERR_SSN_SRVC/";
				switch (errorCode) {
				case CONNECTION_REFUSED:
					result += "Connection refused";
					break;
				case NOT_LISTENING_CALLED:
					result += "Not listening on called name";
					break;
				case NOT_LISTENING_CALLING:
					result += "Not listening for calling name";
					break;
				case CALLED_NOT_PRESENT:
					result += "Called name not present";
					break;
				case NO_RESOURCES:
					result += "Called name present, but insufficient resources";
					break;
				case UNSPECIFIED:
					result += "Unspecified error";
					break;
				default:
					result += "Unknown error code: " + errorCode;
				break;
				}
				break;
			default:
				result += "unknown error class: " + errorClass;
			break;
			}
			return result;
		}


		public NbtException(int errorClass, int errorCode) : base(getErrorString(errorClass, errorCode)) {
			this.errorClass = errorClass;
			this.errorCode = errorCode;
		}


		public override string ToString() {
			return "errorClass=" + this.errorClass + ",errorCode=" + this.errorCode + ",errorString=" + getErrorString(this.errorClass, this.errorCode);
		}
	}

}