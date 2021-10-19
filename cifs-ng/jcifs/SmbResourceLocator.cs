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
	/// Location information for a SMB resource
	/// 
	/// @author mbechler
	/// 
	/// </summary>
	public interface SmbResourceLocator {

		/// <summary>
		/// Returns the last component of the target URL. This will
		/// effectively be the name of the file or directory represented by this
		/// <code>SmbFile</code> or in the case of URLs that only specify a server
		/// or workgroup, the server or workgroup will be returned. The name of
		/// the root URL <code>smb://</code> is also <code>smb://</code>. If this
		/// <tt>SmbFile</tt> refers to a workgroup, server, share, or directory,
		/// the name will include a trailing slash '/' so that composing new
		/// <tt>SmbFile</tt>s will maintain the trailing slash requirement.
		/// </summary>
		/// <returns> The last component of the URL associated with this SMB
		///         resource or <code>smb://</code> if the resource is <code>smb://</code>
		///         itself. </returns>

		string getName();


		/// 
		/// <returns> dfs referral data </returns>
		DfsReferralData getDfsReferral();


		/// <summary>
		/// Everything but the last component of the URL representing this SMB
		/// resource is effectively it's parent. The root URL <code>smb://</code>
		/// does not have a parent. In this case <code>smb://</code> is returned.
		/// </summary>
		/// <returns> The parent directory of this SMB resource or
		///         <code>smb://</code> if the resource refers to the root of the URL
		///         hierarchy which incidentally is also <code>smb://</code>. </returns>
		string getParent();


		/// <summary>
		/// Returns the full uncanonicalized URL of this SMB resource. An
		/// <code>SmbFile</code> constructed with the result of this method will
		/// result in an <code>SmbFile</code> that is equal to the original.
		/// </summary>
		/// <returns> The uncanonicalized full URL of this SMB resource. </returns>

		string getPath();


		/// <summary>
		/// Returns the full URL of this SMB resource with '.' and '..' components
		/// factored out. An <code>SmbFile</code> constructed with the result of
		/// this method will result in an <code>SmbFile</code> that is equal to
		/// the original.
		/// </summary>
		/// <returns> The canonicalized URL of this SMB resource. </returns>
		string getCanonicalURL();


		/// <returns> The canonicalized UNC path of this SMB resource (relative to it's share) </returns>
		string getUNCPath();


		/// <returns> The canonicalized URL path (relative to the server/domain) </returns>
		string getURLPath();


		/// <summary>
		/// Retrieves the share associated with this SMB resource. In
		/// the case of <code>smb://</code>, <code>smb://workgroup/</code>,
		/// and <code>smb://server/</code> URLs which do not specify a share,
		/// <code>null</code> will be returned.
		/// </summary>
		/// <returns> The share component or <code>null</code> if there is no share </returns>
		string getShare();


		/// <summary>
		/// Retrieve the hostname of the server for this SMB resource. If the resources has been resolved by DFS this will
		/// return the target name.
		/// </summary>
		/// <returns> The server name </returns>
		string getServerWithDfs();


		/// <summary>
		/// Retrieve the hostname of the server for this SMB resource. If this
		/// <code>SmbFile</code> references a workgroup, the name of the workgroup
		/// is returned. If this <code>SmbFile</code> refers to the root of this
		/// SMB network hierarchy, <code>null</code> is returned.
		/// </summary>
		/// <returns> The server or workgroup name or <code>null</code> if this
		///         <code>SmbFile</code> refers to the root <code>smb://</code> resource. </returns>
		string getServer();


		/// <summary>
		/// If the path of this <code>SmbFile</code> falls within a DFS volume,
		/// this method will return the referral path to which it maps. Otherwise
		/// <code>null</code> is returned.
		/// </summary>
		/// <returns> URL to the DFS volume </returns>
		string getDfsPath();


		/// <returns> the transport port, if specified </returns>
		int getPort();


		/// <returns> the original URL </returns>
		URL getURL();


		/// <returns> resolved server address </returns>
		/// <exception cref="CIFSException"> </exception>
		/// throws CIFSException;
		Address getAddress();


		/// <returns> whether this is a IPC connection </returns>
		bool isIPC();


		/// <summary>
		/// Returns type of of object this <tt>SmbFile</tt> represents.
		/// </summary>
		/// <returns> <tt>TYPE_FILESYSTEM, TYPE_WORKGROUP, TYPE_SERVER,
		/// TYPE_NAMED_PIPE</tt>, or <tt>TYPE_SHARE</tt> in which case it may be either <tt>TYPE_SHARE</tt>,
		///         <tt>TYPE_PRINTER</tt> or <tt>TYPE_COMM</tt>. </returns>
		/// <exception cref="CIFSException"> </exception>
		/// throws CIFSException;
		int getType();


		/// <returns> whether this is a workgroup reference </returns>
		/// <exception cref="CIFSException"> </exception>
		/// throws CIFSException;
		bool isWorkgroup();


		/// 
		/// <returns> whether this is a root resource </returns>
		bool isRoot();

	}
}