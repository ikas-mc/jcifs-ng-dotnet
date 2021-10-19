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
using cifs_ng.lib;
namespace jcifs {

	/// <summary>
	/// Handle to a connected SMB tree
	/// 
	/// @author mbechler
	/// 
	/// </summary>
	public interface SmbTreeHandle : AutoCloseable {

		/// <returns> the active configuration </returns>
		Configuration getConfig();


		// /// <summary>
		// /// {@inheritDoc}
		// /// </summary>
		// /// <seealso cref= java.lang.AutoCloseable#Dispose() </seealso>
		// /// throws CIFSException;
		// void Dispose();


		/// <returns> the tree is connected </returns>
		bool isConnected();


		/// <returns> server timezone offset </returns>
		/// <exception cref="CIFSException"> </exception>
		/// throws CIFSException;
		long getServerTimeZoneOffset();


		/// <returns> server reported domain name </returns>
		/// <exception cref="CIFSException"> </exception>
		/// throws CIFSException;
		string getOEMDomainName();


		/// <returns> the share we are connected to </returns>
		string getConnectedShare();


		/// <param name="th"> </param>
		/// <returns> whether the handles refer to the same tree </returns>
		bool isSameTree(SmbTreeHandle th);


		/// <returns> whether this tree handle uses SMB2+ </returns>
		bool isSMB2();


		/// <returns> the remote host name </returns>
		string getRemoteHostName();


		/// <returns> the tree type </returns>
		int getTreeType();

	}
}