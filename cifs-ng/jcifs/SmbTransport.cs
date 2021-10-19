using System;
using cifs_ng.lib;

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
namespace jcifs {

	/// <summary>
	/// Opaque reference to a SMB transport
	/// 
	/// @author mbechler
	/// @internal
	/// </summary>
	public interface SmbTransport : AutoCloseable {

		/// <returns> the context this transport is attached to </returns>
		CIFSContext getContext();


		/// 
		/// <param name="type"> </param>
		/// <returns> transport instance with the given type </returns>
		T unwrap<T>(Type type) where T : SmbTransport ;


		/*/// 
		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= java.lang.AutoCloseable#Dispose() </seealso>
		void Dispose();*/


		/// <returns> the connected address </returns>
		Address getRemoteAddress();


		/// <returns> the connected host name </returns>
		string getRemoteHostName();

	}
}