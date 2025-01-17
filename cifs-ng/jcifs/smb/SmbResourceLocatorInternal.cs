using CIFSException = jcifs.CIFSException;
using DfsReferralData = jcifs.DfsReferralData;
using SmbResourceLocator = jcifs.SmbResourceLocator;

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
namespace jcifs.smb {



	/// <summary>
	/// @author mbechler
	/// 
	/// </summary>
	public interface SmbResourceLocatorInternal : SmbResourceLocator {

		/// <returns> whether to enforce the use of signing on connection to this resource </returns>
		bool shouldForceSigning();


		/// <param name="other"> </param>
		/// <returns> whether the paths share a common root </returns>
		/// <exception cref="CIFSException"> </exception>
		/// throws jcifs.CIFSException;
		bool overlaps(SmbResourceLocator other);


		/// <summary>
		/// Internal: for testing only
		/// </summary>
		/// <param name="dr"> </param>
		/// <param name="reqPath"> </param>
		/// <returns> resolved unc path </returns>
		string handleDFSReferral(DfsReferralData dr, string reqPath);
	}

}