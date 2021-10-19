/*
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

	/// 
	/// 
	/// 
	public interface FileEntry {

		/// 
		/// <returns> the file name </returns>
		string getName();


		/// 
		/// <returns> the file type </returns>
		int getType();


		/// 
		/// <returns> the file attributes </returns>
		int getAttributes();


		/// 
		/// <returns> the creation time </returns>
		long createTime();


		/// 
		/// <returns> the last modified time </returns>
		long lastModified();


		/// 
		/// <returns> the last access time </returns>
		long lastAccess();


		/// 
		/// <returns> the file size </returns>
		long length();


		/// <returns> the file index inside the parent </returns>
		int getFileIndex();
	}

}