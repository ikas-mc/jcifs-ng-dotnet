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
using System;
namespace jcifs.util.transport {

	/// <summary>
	/// @author mbechler
	/// 
	/// </summary>
	public interface Message {

		/// <summary>
		/// Indicate that this message should retain it's raw payload
		/// </summary>
		void retainPayload();


		/// 
		/// <returns> whether to retain the message payload </returns>
		bool isRetainPayload();


		/// 
		/// <returns> the raw response message </returns>
		byte[] getRawPayload();


		/// <param name="rawPayload"> </param>
		void setRawPayload(byte[] rawPayload);
	}

}