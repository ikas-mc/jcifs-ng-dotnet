using System;
using cifs_ng.lib.security;
using CIFSContext = jcifs.CIFSContext;
using CIFSException = jcifs.CIFSException;
using Credentials = jcifs.Credentials;

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
namespace jcifs.smb {



	/// <summary>
	/// @author mbechler
	/// 
	/// </summary>
	public interface CredentialsInternal : ICloneable, Credentials {

		/// <param name="tc"> </param>
		/// <param name="targetDomain"> </param>
		/// <param name="host"> </param>
		/// <param name="initialToken"> </param>
		/// <param name="doSigning"> </param>
		/// <returns> a new context </returns>
		/// <exception cref="SmbException"> </exception>
		/// throws SmbException;
		SSPContext createContext(CIFSContext tc, string targetDomain, string host, byte[] initialToken, bool doSigning);


		/// <returns> subject associated with the credentials </returns>
		Subject getSubject();


		/// <exception cref="CIFSException">
		///  </exception>
		/// throws jcifs.CIFSException;
		void refresh();
	}

}