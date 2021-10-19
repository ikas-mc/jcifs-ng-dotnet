using Request = jcifs.util.transport.Request;

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
	public interface CommonServerMessageBlockRequest : CommonServerMessageBlock, Request {

		/// <returns> request was handled asynchronously </returns>
		bool isResponseAsync();


		/// 
		/// <returns> next chained message </returns>
		CommonServerMessageBlockRequest  getNext();


		/// 
		/// <returns> the following message </returns>
		CommonServerMessageBlockRequest split();


		/// <returns> the size of this message </returns>
		int size();


		/// <returns> create cancel request </returns>
		CommonServerMessageBlockRequest createCancel();


		/// <param name="next"> </param>
		/// <returns> whether to allow chaining </returns>
		bool allowChain(CommonServerMessageBlockRequest next);


		/// <param name="t"> </param>
		void setTid(int t);


		/// 
		/// <returns> custom response timeout for this request </returns>
		int? getOverrideTimeout();

	}

}