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

	public static class SmbPipeResourceConstants{
		/// <summary>
		/// The pipe should be opened read-only.
		/// </summary>

		public static int PIPE_TYPE_RDONLY = SmbConstants.O_RDONLY;

		/// <summary>
		/// The pipe should be opened only for writing.
		/// </summary>

		public static int PIPE_TYPE_WRONLY = SmbConstants.O_WRONLY;

		/// <summary>
		/// The pipe should be opened for both reading and writing.
		/// </summary>

		public static int PIPE_TYPE_RDWR = SmbConstants.O_RDWR;

		/// <summary>
		/// Pipe operations should behave like the <code>CallNamedPipe</code> Win32 Named Pipe function.
		/// </summary>

		public static int PIPE_TYPE_CALL = 0x0100;

		/// <summary>
		/// Pipe operations should behave like the <code>TransactNamedPipe</code> Win32 Named Pipe function.
		/// </summary>

		public static int PIPE_TYPE_TRANSACT = 0x0200;

		/// <summary>
		/// Pipe is used for DCE
		/// </summary>
		public static int PIPE_TYPE_DCE_TRANSACT = 0x0200 | 0x0400;

		/// <summary>
		/// Pipe should use it's own exclusive transport connection
		/// </summary>
		public static int PIPE_TYPE_UNSHARED = 0x800;
	}

	/// <summary>
	/// SMB resource respresenting a named pipe
	/// 
	/// @author mbechler
	/// 
	/// </summary>
	public interface SmbPipeResource : SmbResource {

		/// <returns> the type of the pipe </returns>
		int getPipeType();


		/// <summary>
		/// Create a pipe handle
		/// </summary>
		/// <returns> pipe handle, needs to be closed when finished </returns>
		SmbPipeHandle openPipe();

	}

}