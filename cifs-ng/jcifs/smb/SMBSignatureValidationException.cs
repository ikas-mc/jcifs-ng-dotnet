using jcifs.@internal.smb1.com;
using jcifs.smb;

using System;

/*
 * © 2016 AgNO3 Gmbh & Co. KG
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
namespace jcifs.smb {

	/// <summary>
	/// @author mbechler
	/// 
	/// </summary>
	public class SMBSignatureValidationException : SmbException {

		/// 
		public SMBSignatureValidationException() : base() {
		}

		/// <param name="msg"> </param>
		/// <param name="rootCause"> </param>
		public SMBSignatureValidationException(string msg, Exception rootCause) : base(msg, rootCause) {
		}


		/// <param name="msg"> </param>
		public SMBSignatureValidationException(string msg) : base(msg) {
		}

		/// 
		private const long serialVersionUID = 2283323396289696982L;

	}

}