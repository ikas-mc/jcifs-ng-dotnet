using System;
using CIFSException = jcifs.CIFSException;
using WinError = jcifs.smb.WinError;
using Hexdump = jcifs.util.Hexdump;

/* jcifs msrpc client library in Java
 * Copyright (C) 2006  "Michael B. Allen" <jcifs at samba dot org>
 *                     "Eric Glass" <jcifs at samba dot org>
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

namespace jcifs.dcerpc {



	/// 
	public class DcerpcException : CIFSException {

		/// 
		private const long serialVersionUID = -6113895861333916945L;


		internal static string getMessageByDcerpcError(int errcode) {
			int min = 0;
			int max = DcerpcError.DCERPC_FAULT_CODES.Length;

			while (max >= min) {
				int mid = (min + max) / 2;

				if (errcode > DcerpcError.DCERPC_FAULT_CODES[mid]) {
					min = mid + 1;
				}
				else if (errcode < DcerpcError.DCERPC_FAULT_CODES[mid]) {
					max = mid - 1;
				}
				else {
					return DcerpcError.DCERPC_FAULT_MESSAGES[mid];
				}
			}

			return "0x" + Hexdump.toHexString(errcode, 8);
		}

		private int error;


		internal DcerpcException(int error) : base(getMessageByDcerpcError(error)) {
			this.error = error;
		}


		/// <param name="msg"> </param>
		public DcerpcException(string msg) : base(msg) {
		}


		/// <param name="msg"> </param>
		/// <param name="rootCause"> </param>
		public DcerpcException(string msg, Exception rootCause) : base(msg, rootCause) {
		}


		/// 
		/// <returns> the error code </returns>
		public virtual int getErrorCode() {
			return this.error;
		}


		/// 
		/// <returns> the root cause </returns>
		/// @deprecated use <seealso cref="getCause()"/> 
		[Obsolete("use <seealso cref=\"getCause()\"/>")]
		public virtual Exception getRootCause() {
			return InnerException;
		}

	}

}