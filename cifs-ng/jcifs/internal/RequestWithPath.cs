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
	public interface RequestWithPath : CommonServerMessageBlock {

		/// <returns> the path to the resource (below share) </returns>
		string getPath();


		/// 
		/// <returns> the server name </returns>
		string getServer();


		/// 
		/// <returns> the domain name </returns>
		string getDomain();


		/// 
		/// <returns> the full UNC path </returns>
		string getFullUNCPath();


		/// <param name="path"> </param>
		void setPath(string path);


		/// 
		/// <param name="domain"> </param>
		/// <param name="server"> </param>
		/// <param name="fullPath"> </param>
		void setFullUNCPath(string domain, string server, string fullPath);


		/// <param name="resolve">
		///  </param>
		void setResolveInDfs(bool resolve);


		/// 
		/// <returns> whether to resolve the request path in DFS </returns>
		bool isResolveInDfs();

	}

}