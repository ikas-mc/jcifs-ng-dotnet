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
using cifs_ng.lib;
namespace jcifs {

	/// <summary>
	/// Handle to an open file
	/// 
	/// @author mbechler
	/// 
	/// </summary>
	public interface SmbFileHandle : AutoCloseable {

		/// <returns> the tree </returns>
		SmbTreeHandle getTree();


		/// <returns> whether the file descriptor is valid </returns>
		bool isValid();


		/// <param name="lastWriteTime"> </param>
		/// <exception cref="CIFSException"> </exception>
		/// throws CIFSException;
		void close(long lastWriteTime);


		// /// <summary>
		// /// {@inheritDoc}
		// /// </summary>
		// /// <seealso cref= java.lang.AutoCloseable#Dispose() </seealso>
		// /// throws CIFSException;
		// void Dispose();


		/// <exception cref="CIFSException">
		///  </exception>
		/// throws CIFSException;
		void release();


		/// <returns> the file size when it was opened </returns>
		long getInitialSize();

	}
}