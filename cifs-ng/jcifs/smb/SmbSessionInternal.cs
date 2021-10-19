using CIFSException = jcifs.CIFSException;
using SmbSession = jcifs.SmbSession;
using SmbTransport = jcifs.SmbTransport;
using SmbTree = jcifs.SmbTree;

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
	/// @internal
	/// </summary>
	public interface SmbSessionInternal : SmbSession {

		/// <returns> whether the session is in use </returns>
		bool isInUse();


		/// <returns> the current session key </returns>
		/// <exception cref="CIFSException"> </exception>
		/// throws jcifs.CIFSException;
		byte[] getSessionKey();


		/// 
		/// <returns> the transport for this session </returns>
		SmbTransport getTransport();


		/// <summary>
		/// Connect to the logon share
		/// </summary>
		/// <exception cref="SmbException"> </exception>
		/// throws SmbException;
		void treeConnectLogon();


		/// <param name="share"> </param>
		/// <param name="service"> </param>
		/// <returns> tree instance </returns>
		SmbTree getSmbTree(string share, string service);


		/// <summary>
		/// Initiate reauthentication
		/// </summary>
		/// <exception cref="CIFSException"> </exception>
		/// throws jcifs.CIFSException;
		void reauthenticate();
	}

}