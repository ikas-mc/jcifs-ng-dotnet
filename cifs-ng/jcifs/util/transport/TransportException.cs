using System;
using CIFSException = jcifs.CIFSException;

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



	/// 
	public class TransportException : CIFSException {

		/// 
		private const long serialVersionUID = 3743631204022885618L;


		/// 
		public TransportException() {
		}


		/// 
		/// <param name="msg"> </param>
		public TransportException(string msg) : base(msg) {
		}


		/// 
		/// <param name="rootCause"> </param>
		public TransportException(Exception rootCause) : base(rootCause) {
		}


		/// 
		/// <param name="msg"> </param>
		/// <param name="rootCause"> </param>
		public TransportException(string msg, Exception rootCause) : base(msg, rootCause) {
		}


		/// 
		/// <returns> root cause </returns>
		[Obsolete]
		public virtual Exception getRootCause() {
			//TODO
			return InnerException;
		}
	}

}