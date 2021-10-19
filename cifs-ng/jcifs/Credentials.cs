using System;

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
	/// Interface for opaque credential data
	/// 
	/// @author mbechler
	/// 
	/// </summary>
	public interface Credentials {

		/// 
		/// <param name="type"> </param>
		/// <returns> instance for type, null if the type cannot be unwrapped </returns>
		T unwrap<T>(Type type);


		/// <returns> the domain the user account is in </returns>
		string getUserDomain();


		/// <returns> whether these are anonymous credentials </returns>
		bool isAnonymous();


		/// <returns> whether these are guest credentials </returns>
		bool isGuest();

	}
}