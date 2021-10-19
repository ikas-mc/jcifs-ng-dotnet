using System.Collections.Generic;
using cifs_ng.lib;
using cifs_ng.lib.threading;

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
	/// Handle for receiving change notifications from an SMB server
	/// 
	/// 
	/// @author mbechler
	/// 
	/// </summary>
	public interface SmbWatchHandle : AutoCloseable, Callable<IList<FileNotifyInformation>> {

		/// <summary>
		/// Get the next set of changes
		/// 
		/// Will block until the server returns a set of changes that match the given filter. The file will be automatically
		/// opened if it is not and should be closed with <seealso cref="Dispose()"/> when no longer
		/// needed.
		/// 
		/// Closing the context should cancel a pending notify request, but that does not seem to work reliable in all
		/// implementations.
		/// 
		/// Changes in between these calls (as long as the file is open) are buffered by the server, so iteratively calling
		/// this method should provide all changes (size of that buffer can be adjusted through
		/// <seealso cref="jcifs.Configuration.getNotifyBufferSize()"/>).
		/// If the server cannot fulfill the request because the changes did not fit the buffer
		/// it will return an empty list of changes.
		/// </summary>
		/// <returns> changes since the last invocation </returns>
		/// <exception cref="CIFSException"> </exception>
		/// throws CIFSException;
		IList<FileNotifyInformation> watch();


		// /// <summary>
		// /// {@inheritDoc}
		// /// </summary>
		// /// <seealso cref= java.lang.AutoCloseable#Dispose() </seealso>
		// /// throws CIFSException;
		// void Dispose();

	}
}