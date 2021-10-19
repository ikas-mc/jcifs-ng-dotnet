using CIFSContext = jcifs.CIFSContext;
using CIFSException = jcifs.CIFSException;
using DfsReferralData = jcifs.DfsReferralData;
using SmbSession = jcifs.SmbSession;
using SmbTransport = jcifs.SmbTransport;

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
	public interface SmbTransportInternal : SmbTransport {

		/// <param name="cap"> </param>
		/// <returns> whether the transport has the given capability </returns>
		/// <exception cref="SmbException"> </exception>
		/// throws SmbException;
		bool hasCapability(int cap);


		/// <returns> whether the transport has been disconnected </returns>
		bool isDisconnected();


		/// <param name="hard"> </param>
		/// <param name="inuse"> </param>
		/// <returns> whether the connection was in use </returns>
		/// <exception cref="IOException"> </exception>
		/// throws java.io.IOException;
		bool disconnect(bool hard, bool inuse);


		/// <returns> whether the transport was connected </returns>
		/// <exception cref="SmbException"> </exception>
		/// <exception cref="IOException">
		///  </exception>
		/// throws java.io.IOException;
		bool ensureConnected();


		/// <param name="ctx"> </param>
		/// <param name="name"> </param>
		/// <param name="targetHost"> </param>
		/// <param name="targetDomain"> </param>
		/// <param name="rn"> </param>
		/// <returns> dfs referral </returns>
		/// <exception cref="SmbException"> </exception>
		/// <exception cref="CIFSException"> </exception>
		/// throws jcifs.CIFSException;
		DfsReferralData getDfsReferrals(CIFSContext ctx, string name, string targetHost, string targetDomain, int rn);


		/// <returns> whether signatures are supported but not required </returns>
		/// <exception cref="SmbException"> </exception>
		/// throws SmbException;
		bool isSigningOptional();


		/// <returns> whether signatures are enforced from either side </returns>
		/// <exception cref="SmbException"> </exception>
		/// throws SmbException;
		bool isSigningEnforced();


		/// <returns> the encryption key used by the server </returns>
		byte[] getServerEncryptionKey();


		/// <param name="ctx"> </param>
		/// <returns> session </returns>
		SmbSession getSmbSession(CIFSContext ctx);


		/// <param name="tf"> </param>
		/// <param name="targetHost"> </param>
		/// <param name="targetDomain"> </param>
		/// <returns> session </returns>
		SmbSession getSmbSession(CIFSContext tf, string targetHost, string targetDomain);


		/// <returns> whether this is a SMB2 connection </returns>
		/// <exception cref="SmbException"> </exception>
		/// throws SmbException;
		bool isSMB2();


		/// <returns> number of inflight requests </returns>
		int getInflightRequests();
	}

}