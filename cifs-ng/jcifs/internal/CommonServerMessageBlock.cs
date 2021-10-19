using Message = jcifs.util.transport.Message;

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
	public interface CommonServerMessageBlock : Message {

		/// <summary>
		/// Decode message data from the given byte array
		/// </summary>
		/// <param name="buffer"> </param>
		/// <param name="bufferIndex"> </param>
		/// <returns> message length </returns>
		/// <exception cref="SMBProtocolDecodingException"> </exception>
		/// throws SMBProtocolDecodingException;
		int decode(byte[] buffer, int bufferIndex);


		/// <param name="dst"> </param>
		/// <param name="dstIndex"> </param>
		/// <returns> message length </returns>
		int encode(byte[] dst, int dstIndex);


		/// <param name="digest"> </param>
		void setDigest(SMBSigningDigest digest);


		/// <returns> the signing digest </returns>
		SMBSigningDigest getDigest();


		/// <returns> the associated response </returns>
		CommonServerMessageBlockResponse getResponse();


		/// 
		/// <param name="msg"> </param>
		void setResponse(CommonServerMessageBlockResponse msg);


		/// 
		/// <returns> the message id </returns>
		long getMid();


		/// <param name="mid"> </param>
		void setMid(long mid);


		/// <returns> the command </returns>
		int getCommand();


		/// <param name="command"> </param>
		void setCommand(int command);


		/// <param name="uid"> </param>
		void setUid(int uid);


		/// <param name="extendedSecurity"> </param>
		void setExtendedSecurity(bool extendedSecurity);


		/// <param name="sessionId"> </param>
		void setSessionId(long sessionId);


		/// 
		void reset();

	}

}