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
	/// This is an internal API.
	/// 
	/// @author mbechler
	/// @internal
	/// </summary>
	public interface DfsResolver {

		/// <param name="domain"> </param>
		/// <param name="tf"> </param>
		/// <returns> whether the given domain is trusted </returns>
		/// <exception cref="CIFSException"> </exception>
		/// <exception cref="jcifs.smb.SmbAuthException"> </exception>
		/// throws CIFSException;
		bool isTrustedDomain(CIFSContext tf, string domain);


		/// <summary>
		/// Get a connection to the domain controller for a given domain
		/// </summary>
		/// <param name="domain"> </param>
		/// <param name="tf"> </param>
		/// <returns> connection </returns>
		/// <exception cref="CIFSException"> </exception>
		/// <exception cref="jcifs.smb.SmbAuthException"> </exception>
		/// throws CIFSException;
		SmbTransport getDc(CIFSContext tf, string domain);


		/// <summary>
		/// Resolve the location of a DFS path
		/// </summary>
		/// <param name="domain"> </param>
		/// <param name="root"> </param>
		/// <param name="path"> </param>
		/// <param name="tf"> </param>
		/// <returns> the final referral for the given DFS path </returns>
		/// <exception cref="CIFSException"> </exception>
		/// <exception cref="jcifs.smb.SmbAuthException"> </exception>
		/// throws CIFSException;
		DfsReferralData resolve(CIFSContext tf, string domain, string root, string path);


		/// <summary>
		/// Add a referral to the cache
		/// </summary>
		/// <param name="path"> </param>
		/// <param name="dr"> </param>
		/// <param name="tc"> </param>
		void cache(CIFSContext tc, string path, DfsReferralData dr);

	}
}