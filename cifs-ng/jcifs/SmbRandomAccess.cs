using cifs_ng.lib;
using SmbException = jcifs.smb.SmbException;

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
namespace jcifs {




	/// <summary>
	/// File access that exposes random access semantics
	/// 
	/// @author mbechler
	/// 
	/// </summary>
	public interface SmbRandomAccess :  AutoCloseable {

		// /// <summary>
		// /// Close the file
		// /// </summary>
		// /// <exception cref="SmbException"> </exception>
		// /// throws jcifs.smb.SmbException;
		// void Dispose();


		/// <summary>
		/// Read a single byte from the current position
		/// </summary>
		/// <returns> read byte, -1 if EOF </returns>
		/// <exception cref="SmbException"> </exception>
		/// throws jcifs.smb.SmbException;
		int read();


		/// <summary>
		/// Read into buffer from current position
		/// </summary>
		/// <param name="b">
		///            buffer </param>
		/// <returns> number of bytes read </returns>
		/// <exception cref="SmbException"> </exception>
		/// throws jcifs.smb.SmbException;
		int read(byte[] b);


		/// <summary>
		/// Read into buffer from current position
		/// </summary>
		/// <param name="b">
		///            buffer </param>
		/// <param name="off">
		///            offset into buffer </param>
		/// <param name="len">
		///            read up to <tt>len</tt> bytes </param>
		/// <returns> number of bytes read </returns>
		/// <exception cref="SmbException"> </exception>
		/// throws jcifs.smb.SmbException;
		int read(byte[] b, int off, int len);


		/// <summary>
		/// Current position in file
		/// </summary>
		/// <returns> current position </returns>
		long getFilePointer();


		/// <summary>
		/// Seek to new position
		/// </summary>
		/// <param name="pos"> </param>
		void seek(long pos);


		/// <summary>
		/// Get the current file length
		/// </summary>
		/// <returns> file length </returns>
		/// <exception cref="SmbException"> </exception>
		/// throws jcifs.smb.SmbException;
		long length();


		/// <summary>
		/// Expand/truncate file length
		/// </summary>
		/// <param name="newLength">
		///            new file length </param>
		/// <exception cref="SmbException"> </exception>
		/// throws jcifs.smb.SmbException;
		void setLength(long newLength);

	}

}