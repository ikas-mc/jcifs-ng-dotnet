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
	/// Information returned in DFS referrals
	/// 
	/// @author mbechler
	/// @internal
	/// </summary>
	public interface DfsReferralData {

		/// 
		/// <param name="type"> </param>
		/// <returns> the referral adapted to type </returns>
		/// <exception cref="ClassCastException">
		///             if the type is not valid for this object </exception>
		T unwrap<T>(Type type);


		/// <returns> the server this referral points to </returns>
		string getServer();


		/// 
		/// <returns> the domain this referral is for </returns>
		string getDomain();


		/// <returns> the share this referral points to </returns>
		string getShare();


		/// <returns> the number of characters from the unc path that were consumed by this referral </returns>
		int getPathConsumed();


		/// <returns> the replacement path for this referal </returns>
		string getPath();


		/// <returns> the expiration time of this entry </returns>
		long getExpiration();


		/// 
		/// <returns> pointer to next referral, points to self if there is no further referral </returns>
		DfsReferralData next();


		/// <returns> the link </returns>
		string getLink();

	}

}