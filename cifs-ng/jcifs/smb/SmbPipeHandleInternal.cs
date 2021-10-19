using CIFSException = jcifs.CIFSException;
using SmbFileHandle = jcifs.SmbFileHandle;
using SmbPipeHandle = jcifs.SmbPipeHandle;

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
	/// @internal
	/// </summary>
	public interface SmbPipeHandleInternal : SmbPipeHandle {

		/// <returns> the pipe type </returns>
		int getPipeType();


		/// <returns> session key of the underlying smb session </returns>
		/// <exception cref="CIFSException"> </exception>
		/// throws jcifs.CIFSException;
		byte[] getSessionKey();


		/// 
		/// <returns> this pipe's input stream </returns>
		/// <exception cref="SmbException"> </exception>
		/// throws jcifs.CIFSException;
		SmbPipeInputStream getInput();


		/// 
		/// <returns> this pipe's output stream </returns>
		/// <exception cref="SmbException">
		/// @throws </exception>
		/// throws jcifs.CIFSException;
		SmbPipeOutputStream getOutput();


		/// <returns> tree connection </returns>
		/// <exception cref="SmbException"> </exception>
		/// <exception cref="CIFSException"> </exception>
		/// throws jcifs.CIFSException;
		SmbTreeHandleInternal ensureTreeConnected();


		/// <returns> file handle </returns>
		/// <exception cref="CIFSException"> </exception>
		/// throws jcifs.CIFSException;
		SmbFileHandle ensureOpen();


		/// <param name="buf"> </param>
		/// <param name="off"> </param>
		/// <param name="length"> </param>
		/// <param name="direct"> </param>
		/// <returns> received bytes </returns>
		/// <exception cref="CIFSException"> </exception>
		/// <exception cref="IOException"> </exception>
		/// throws java.io.IOException;
		int recv(byte[] buf, int off, int length);


		/// <param name="buf"> </param>
		/// <param name="off"> </param>
		/// <param name="length"> </param>
		/// <param name="direct"> </param>
		/// <exception cref="IOException"> </exception>
		/// throws java.io.IOException;
		void send(byte[] buf, int off, int length);


		/// <param name="buf"> </param>
		/// <param name="off"> </param>
		/// <param name="length"> </param>
		/// <param name="inB"> </param>
		/// <param name="maxRecvCnt"> </param>
		/// <returns> len </returns>
		/// <exception cref="IOException"> </exception>
		/// throws java.io.IOException;
		int sendrecv(byte[] buf, int off, int length, byte[] inB, int maxRecvCnt);
	}

}