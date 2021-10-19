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
	public sealed class SmbNegotiation {

		private readonly SmbNegotiationRequest request;
		private readonly SmbNegotiationResponse response;
		private readonly byte[] negoReqBuffer;
		private readonly byte[] negoRespBuffer;


		/// <param name="request"> </param>
		/// <param name="response"> </param>
		/// <param name="negoRespBuffer"> </param>
		/// <param name="negoReqBuffer">
		///  </param>
		public SmbNegotiation(SmbNegotiationRequest request, SmbNegotiationResponse response, byte[] negoReqBuffer, byte[] negoRespBuffer) {
			this.request = request;
			this.response = response;
			this.negoReqBuffer = negoReqBuffer;
			this.negoRespBuffer = negoRespBuffer;
		}


		/// <returns> the request </returns>
		public SmbNegotiationRequest getRequest() {
			return this.request;
		}


		/// <returns> the response </returns>
		public SmbNegotiationResponse getResponse() {
			return this.response;
		}


		/// <returns> the negoReqBuffer </returns>
		public byte[] getRequestRaw() {
			return this.negoReqBuffer;
		}


		/// <returns> the negoRespBuffer </returns>
		public byte[] getResponseRaw() {
			return this.negoRespBuffer;
		}
	}

}