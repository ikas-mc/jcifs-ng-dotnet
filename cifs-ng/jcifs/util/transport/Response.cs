using System;

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
namespace jcifs.util.transport {

	/// 
	public interface Response : Message {

		/// 
		/// <returns> whether the response is received </returns>
		bool isReceived();


		/// <summary>
		/// Set received status
		/// </summary>
		void received();


		/// <summary>
		/// Unset received status
		/// </summary>
		void clearReceived();


		/// 
		/// <returns> number of credits granted by the server </returns>
		int getGrantedCredits();


		/// <returns> status code </returns>
		int getErrorCode();


		/// <param name="k"> </param>
		void setMid(long k);


		/// <returns> mid </returns>
		long getMid();


		/// 
		/// <param name="buffer"> </param>
		/// <param name="i"> </param>
		/// <param name="size"> </param>
		/// <returns> whether signature verification is successful </returns>
		bool verifySignature(byte[] buffer, int i, int size);


		/// <returns> whether signature verification failed </returns>
		bool isVerifyFailed();


		/// 
		/// <returns> whether the response is an error </returns>
		bool isError();


		/// <summary>
		/// Set error status
		/// </summary>
		void error();


		/// 
		/// <returns> the message timeout </returns>
		long? getExpiration();


		/// 
		/// <param name="exp">
		///            message timeout </param>
		void setExpiration(long? exp);


		/// 
		void reset();


		/// 
		/// <returns> an exception linked to an error </returns>
		Exception getException();


		/// <param name="e"> </param>
		void exception(Exception e);


		/// <returns> chained response </returns>
		Response getNextResponse();

	}

}