using System;
using System.IO;

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
namespace jcifs {


	/// <summary>
	/// Base for all checked exceptions used by this library
	/// 
	/// @author mbechler
	/// 
	/// </summary>
	public class CIFSException : IOException {

		/// 
		private const long serialVersionUID = 7806460518865806784L;


		/// 
		public CIFSException() : base() {
		}


		/// 
		/// <param name="message"> </param>
		/// <param name="cause"> </param>
		public CIFSException(string message, Exception cause) : base(message, cause) {
		}


		/// 
		/// <param name="message"> </param>
		public CIFSException(string message) : base(message) {
		}


		/// 
		/// <param name="cause"> </param>
		public CIFSException(Exception cause) : base(cause.Message,cause) {
		}

	}

}