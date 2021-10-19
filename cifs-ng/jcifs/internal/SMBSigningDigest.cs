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
	public interface SMBSigningDigest {

		/// <summary>
		/// Performs MAC signing of the SMB. This is done as follows.
		/// The signature field of the SMB is overwritten with the sequence number;
		/// The MD5 digest of the MAC signing key + the entire SMB is taken;
		/// The first 8 bytes of this are placed in the signature field.
		/// </summary>
		/// <param name="data">
		///            The data. </param>
		/// <param name="offset">
		///            The starting offset at which the SMB header begins. </param>
		/// <param name="length">
		///            The length of the SMB data starting at offset. </param>
		/// <param name="request">
		///            request message </param>
		/// <param name="response">
		///            response message </param>
		void sign(byte[] data, int offset, int length, CommonServerMessageBlock request, CommonServerMessageBlock response);


		/// <summary>
		/// Performs MAC signature verification. This calculates the signature
		/// of the SMB and compares it to the signature field on the SMB itself.
		/// </summary>
		/// <param name="data">
		///            The data. </param>
		/// <param name="offset">
		///            The starting offset at which the SMB header begins. </param>
		/// <param name="length"> </param>
		/// <param name="extraPad">
		///            extra padding to include in signature </param>
		/// <param name="msg">
		///            The message to verify </param>
		/// <returns> whether verification was unsuccessful </returns>
		bool verify(byte[] data, int offset, int length, int extraPad, CommonServerMessageBlock msg);

	}
}