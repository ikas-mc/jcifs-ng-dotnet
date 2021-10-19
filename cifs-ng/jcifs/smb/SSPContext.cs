
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

using Org.BouncyCastle.Asn1;

namespace jcifs.smb {




	/// <summary>
	/// @author mbechler
	/// 
	/// </summary>
	public interface SSPContext {

		/// <returns> the signing key for the session
		/// </returns>
		/// <exception cref="CIFSException"> </exception>
		/// throws jcifs.CIFSException;
		byte[] getSigningKey();


		/// <returns> whether the context is established </returns>
		bool isEstablished();


		/// <param name="token"> </param>
		/// <param name="off"> </param>
		/// <param name="len"> </param>
		/// <returns> result token </returns>
		/// <exception cref="SmbException"> </exception>
		/// <exception cref="CIFSException"> </exception>
		/// throws jcifs.CIFSException;
		byte[] initSecContext(byte[] token, int off, int len);


		/// <returns> the name of the remote endpoint </returns>
		string getNetbiosName();


		/// <exception cref="CIFSException"> </exception>
		/// throws jcifs.CIFSException;
		void dispose();


		/// <param name="mechanism"> </param>
		/// <returns> whether the specified mechanism is supported </returns>
		bool isSupported(DerObjectIdentifier mechanism);


		/// <param name="selectedMech"> </param>
		/// <returns> whether the specified mechanism is preferred </returns>
		bool isPreferredMech(DerObjectIdentifier selectedMech);


		/// <returns> context flags </returns>
		int getFlags();


		/// <returns> array of supported mechanism OIDs </returns>
		DerObjectIdentifier[] getSupportedMechs();


		/// 
		/// <returns> whether this mechanisms supports integrity </returns>
		bool supportsIntegrity();


		/// <param name="data"> </param>
		/// <returns> MIC </returns>
		/// <exception cref="CIFSException"> </exception>
		/// throws jcifs.CIFSException;
		byte[] calculateMIC(byte[] data);


		/// <param name="data"> </param>
		/// <param name="mic"> </param>
		/// <exception cref="CIFSException"> </exception>
		/// throws jcifs.CIFSException;
		void verifyMIC(byte[] data, byte[] mic);


		/// <returns> whether MIC can be used </returns>
		bool isMICAvailable();

	}

}