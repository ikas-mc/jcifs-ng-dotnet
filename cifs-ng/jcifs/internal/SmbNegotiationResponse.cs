using CIFSContext = jcifs.CIFSContext;
using DialectVersion = jcifs.DialectVersion;
using Response = jcifs.util.transport.Response;

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
namespace jcifs.@internal {



	/// <summary>
	/// @author mbechler
	/// 
	/// </summary>
	public interface SmbNegotiationResponse : CommonServerMessageBlock, Response {

		/// 
		/// <param name="cifsContext"> </param>
		/// <param name="singingEnforced"> </param>
		/// <param name="request"> </param>
		/// <returns> whether the protocol negotiation was successful </returns>
		bool isValid(CIFSContext cifsContext, SmbNegotiationRequest request);


		/// 
		/// <returns> selected dialect </returns>
		DialectVersion getSelectedDialect();


		/// 
		/// <returns> whether the server has singing enabled </returns>
		bool isSigningEnabled();


		/// 
		/// <returns> whether the server requires signing </returns>
		bool isSigningRequired();


		/// <returns> whether the server supports DFS </returns>
		bool isDFSSupported();


		/// <param name="request"> </param>
		void setupRequest(CommonServerMessageBlock request);


		/// <param name="resp"> </param>
		void setupResponse(Response resp);


		/// <returns> whether signing has been negotiated </returns>
		bool isSigningNegotiated();


		/// <param name="cap"> </param>
		/// <returns> whether capability is negotiated </returns>
		bool haveCapabilitiy(int cap);


		/// <returns> the send buffer size </returns>
		int getSendBufferSize();


		/// <returns> the receive buffer size </returns>
		int getReceiveBufferSize();


		/// 
		/// <returns> the transaction buffer size </returns>
		int getTransactionBufferSize();


		/// 
		/// <returns> number of initial credits the server grants </returns>
		int getInitialCredits();


		/// <param name="tc"> </param>
		/// <param name="forceSigning"> </param>
		/// <returns> whether a connection can be reused for this config </returns>
		bool canReuse(CIFSContext tc, bool forceSigning);

	}

}