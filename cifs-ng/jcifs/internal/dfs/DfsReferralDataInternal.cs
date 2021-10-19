using System.Collections.Generic;
using DfsReferralData = jcifs.DfsReferralData;

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
namespace jcifs.@internal.dfs {



	/// <summary>
	/// @author mbechler
	/// 
	/// </summary>
	public interface DfsReferralDataInternal : DfsReferralData {

		/// <summary>
		/// Replaces the host with the given FQDN if it is currently unqualified
		/// </summary>
		/// <param name="fqdn"> </param>
		void fixupHost(string fqdn);


		/// <summary>
		/// Possibly appends the given domain name to the host name if it is currently unqualified
		/// </summary>
		/// <param name="domain"> </param>
		void fixupDomain(string domain);


		/// <summary>
		/// Reduces path consumed by the given value
		/// </summary>
		/// <param name="i"> </param>
		void stripPathConsumed(int i);


		DfsReferralDataInternal next();


		/// <param name="link"> </param>
		void setLink(string link);


		/// <returns> cache key </returns>
		string getKey();


		/// 
		/// <param name="key">
		///            cache key </param>
		void setKey(string key);


		/// <param name="map"> </param>
		void setCacheMap(IDictionary<string, DfsReferralDataInternal> map);


		/// <summary>
		/// Replaces the entry with key in the cache map with this referral
		/// </summary>
		void replaceCache();


		/// <summary>
		/// Not exactly sure what that is all about, certainly legacy stuff
		/// </summary>
		/// <returns> resolveHashes </returns>
		bool isResolveHashes();


		/// <returns> whether this refrral needs to be resolved further </returns>
		bool isIntermediate();


		/// <param name="next"> </param>
		/// <returns> new referral, combining a chain of referrals </returns>
		DfsReferralDataInternal combine(DfsReferralData next);


		/// <param name="dr"> </param>
		void append(DfsReferralDataInternal dr);
	}

}