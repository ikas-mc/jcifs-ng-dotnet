using CIFSException = jcifs.CIFSException;
using SmbSession = jcifs.SmbSession;
using SmbTreeHandle = jcifs.SmbTreeHandle;

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
	public interface SmbTreeHandleInternal : SmbTreeHandle {

		/// 
		void release();


		/// 
		/// <exception cref="SmbException"> </exception>
		/// <exception cref="CIFSException"> </exception>
		/// throws jcifs.CIFSException;
		void ensureDFSResolved();


		/// <param name="cap"> </param>
		/// <returns> whether the capabiltiy is present </returns>
		/// <exception cref="CIFSException"> </exception>
		/// throws jcifs.CIFSException;
		bool hasCapability(int cap);


		/// <returns> the send buffer size of the underlying connection </returns>
		/// <exception cref="CIFSException"> </exception>
		/// throws jcifs.CIFSException;
		int getSendBufferSize();


		/// <returns> the receive buffer size of the underlying connection </returns>
		/// <exception cref="CIFSException"> </exception>
		/// throws jcifs.CIFSException;
		int getReceiveBufferSize();


		/// <returns> the maximum buffer size reported by the server </returns>
		/// <exception cref="CIFSException"> </exception>
		/// throws jcifs.CIFSException;
		int getMaximumBufferSize();


		/// <returns> whether the session uses SMB signing </returns>
		/// <exception cref="CIFSException"> </exception>
		/// <exception cref="SmbException"> </exception>
		/// throws jcifs.CIFSException;
		bool areSignaturesActive();


		/// <summary>
		/// Internal/testing use only
		/// </summary>
		/// <returns> attached session </returns>
		SmbSession getSession();
	}

}