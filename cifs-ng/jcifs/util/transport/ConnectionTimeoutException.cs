using System;

/*
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
namespace jcifs.util.transport {

	/// <summary>
	/// @author mbechler
	/// 
	/// </summary>
	public class ConnectionTimeoutException : TransportException {

		/// 
		private const long serialVersionUID = 7327198103204592731L;


		/// 
		public ConnectionTimeoutException() {
		}


		/// <param name="msg"> </param>
		public ConnectionTimeoutException(string msg) : base(msg) {
		}


		/// <param name="rootCause"> </param>
		public ConnectionTimeoutException(Exception rootCause) : base(rootCause) {
		}


		/// <param name="msg"> </param>
		/// <param name="rootCause"> </param>
		public ConnectionTimeoutException(string msg, Exception rootCause) : base(msg, rootCause) {
		}

	}

}