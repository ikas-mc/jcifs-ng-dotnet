using System;
using System.Net;

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
	/// Interface for both netbios and internet addresses
	/// 
	/// @author mbechler
	/// 
	/// </summary>
	public interface Address {

		/// 
		/// <param name="type"> </param>
		/// <returns> instance for type, null if the type cannot be unwrapped </returns>
		T unwrap<T>(Type type);


		/// 
		/// <returns> the resolved host name, or the host address if it could not be resolved </returns>
		string getHostName();


		/// <summary>
		/// Return the IP address as text such as "192.168.1.15".
		/// </summary>
		/// <returns> the ip address </returns>
		string getHostAddress();


		/// 
		/// <returns> this address as an IPAddress </returns>
		/// <exception cref="UnknownHostException"> </exception>
		/// throws java.net.UnknownHostException;
		IPAddress toInetAddress();


		/// <summary>
		/// Guess called name to try for session establishment. These
		/// methods are used by the smb package.
		/// </summary>
		/// <param name="tc">
		/// </param>
		/// <returns> guessed name </returns>
		string firstCalledName();


		/// <summary>
		/// Guess next called name to try for session establishment. These
		/// methods are used by the smb package.
		/// </summary>
		/// <param name="tc">
		/// </param>
		/// <returns> guessed name </returns>
		string nextCalledName(CIFSContext tc);

	}

}