using System.IO;
using cifs_ng.lib;
using cifs_ng.lib.io;

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
	/// This class represents a resource on an SMB network. Mainly these
	/// resources are files and directories however an <code>SmbFile</code>
	/// may also refer to servers and workgroups.
	/// </summary>
	/// <seealso cref= jcifs.smb.SmbFile for the main implementation of this interface
	/// @author mbechler </seealso>
	public interface SmbResource : AutoCloseable {

		/// <summary>
		/// Gets the file locator for this file
		/// 
		/// The file locator provides details about
		/// </summary>
		/// <returns> the fileLocator </returns>
		SmbResourceLocator getLocator();


		/// <summary>
		/// The context this file was opened with
		/// </summary>
		/// <returns> the context associated with this file </returns>
		CIFSContext getContext();


		/// <summary>
		/// Returns the last component of the target URL. This will
		/// effectively be the name of the file or directory represented by this
		/// <code>SmbResource</code> or in the case of URLs that only specify a server
		/// or workgroup, the server or workgroup will be returned. The name of
		/// the root URL <code>smb://</code> is also <code>smb://</code>. If this
		/// <tt>SmbResource</tt> refers to a workgroup, server, share, or directory,
		/// the name will include a trailing slash '/' so that composing new
		/// <tt>SmbResource</tt>s will maintain the trailing slash requirement.
		/// </summary>
		/// <returns> The last component of the URL associated with this SMB
		///         resource or <code>smb://</code> if the resource is <code>smb://</code>
		///         itself. </returns>
		string getName();


		/// <summary>
		/// Returns type of of object this <tt>SmbResource</tt> represents.
		/// </summary>
		/// <returns> <tt>TYPE_FILESYSTEM, TYPE_WORKGROUP, TYPE_SERVER, TYPE_SHARE,
		/// TYPE_PRINTER, TYPE_NAMED_PIPE</tt>, or <tt>TYPE_COMM</tt>. </returns>
		/// <exception cref="CIFSException"> </exception>
		/// throws CIFSException;
		int getType();


		/// <summary>
		/// Tests to see if the SMB resource exists. If the resource refers
		/// only to a server, this method determines if the server exists on the
		/// network and is advertising SMB services. If this resource refers to
		/// a workgroup, this method determines if the workgroup name is valid on
		/// the local SMB network. If this <code>SmbResource</code> refers to the root
		/// <code>smb://</code> resource <code>true</code> is always returned. If
		/// this <code>SmbResource</code> is a traditional file or directory, it will
		/// be queried for on the specified server as expected.
		/// </summary>
		/// <returns> <code>true</code> if the resource exists or is alive or
		///         <code>false</code> otherwise </returns>
		/// <exception cref="CIFSException"> </exception>
		/// throws CIFSException;
		bool exists();


		/// <summary>
		/// Fetch a child resource
		/// </summary>
		/// <param name="name"> </param>
		/// <returns> the child resource </returns>
		/// <exception cref="CIFSException"> </exception>
		/// throws CIFSException;
		SmbResource resolve(string name);


		/// <summary>
		/// Get the file index
		/// </summary>
		/// <returns> server side file index, 0 if unavailable </returns>
		/// <exception cref="CIFSException"> </exception>
		/// throws CIFSException;
		long fileIndex();


		/// <summary>
		/// Return the attributes of this file. Attributes are represented as a
		/// bitset that must be masked with <tt>ATTR_*</tt> constants to determine
		/// if they are set or unset. The value returned is suitable for use with
		/// the <tt>setAttributes()</tt> method.
		/// </summary>
		/// <returns> the <tt>ATTR_*</tt> attributes associated with this file </returns>
		/// <exception cref="CIFSException"> </exception>
		/// throws CIFSException;
		int getAttributes();


		/// <summary>
		/// Tests to see if the file this SmbResource represents is marked as
		/// hidden. This method will also return true for shares with names that
		/// end with '$' such as <code>IPC$</code> or <code>C$</code>.
		/// </summary>
		/// <returns> <code>true</code> if the <code>SmbResource</code> is marked as being hidden </returns>
		/// <exception cref="CIFSException"> </exception>
		/// throws CIFSException;
		bool isHidden();


		/// <summary>
		/// Tests to see if the file this <code>SmbResource</code> represents is not a directory.
		/// </summary>
		/// <returns> <code>true</code> if this <code>SmbResource</code> is not a directory </returns>
		/// <exception cref="CIFSException"> </exception>
		/// throws CIFSException;
		bool isFile();


		/// <summary>
		/// Tests to see if the file this <code>SmbResource</code> represents is a directory.
		/// </summary>
		/// <returns> <code>true</code> if this <code>SmbResource</code> is a directory </returns>
		/// <exception cref="CIFSException"> </exception>
		/// throws CIFSException;
		bool isDirectory();


		/// <summary>
		/// Tests to see if the file this <code>SmbResource</code> represents
		/// exists and is not marked read-only. By default, resources are
		/// considered to be read-only and therefore for <code>smb://</code>,
		/// <code>smb://workgroup/</code>, and <code>smb://server/</code> resources
		/// will be read-only.
		/// </summary>
		/// <returns> <code>true</code> if the resource exists is not marked
		///         read-only </returns>
		/// <exception cref="CIFSException"> </exception>
		/// throws CIFSException;
		bool canWrite();


		/// <summary>
		/// Tests to see if the file this <code>SmbResource</code> represents can be
		/// read. Because any file, directory, or other resource can be read if it
		/// exists, this method simply calls the <code>exists</code> method.
		/// </summary>
		/// <returns> <code>true</code> if the file is read-only </returns>
		/// <exception cref="CIFSException"> </exception>
		/// throws CIFSException;
		bool canRead();


		/// <summary>
		/// Turn off the read-only attribute of this file. This is shorthand for
		/// <tt>setAttributes( getAttributes() &amp; ~ATTR_READONLY )</tt>.
		/// </summary>
		/// <exception cref="CIFSException"> </exception>
		/// throws CIFSException;
		void setReadWrite();


		/// <summary>
		/// Make this file read-only. This is shorthand for <tt>setAttributes(
		/// getAttributes() | ATTR_READ_ONLY )</tt>.
		/// </summary>
		/// <exception cref="CIFSException"> </exception>
		/// throws CIFSException;
		void setReadOnly();


		/// <summary>
		/// Set the attributes of this file. Attributes are composed into a
		/// bitset by bitwise ORing the <tt>ATTR_*</tt> constants. Setting the
		/// value returned by <tt>getAttributes</tt> will result in both files
		/// having the same attributes.
		/// </summary>
		/// <param name="attrs">
		///            attribute flags
		/// </param>
		/// <exception cref="CIFSException"> </exception>
		/// throws CIFSException;
		void setAttributes(int attrs);


		/// <summary>
		/// Set the create, last modified and last access time of the file. The time is specified
		/// as milliseconds from Jan 1, 1970 which is the same as that which is returned by the
		/// <tt>createTime()</tt>, <tt>lastModified()</tt>, <tt>lastAccess()</tt> methods.
		/// <br>
		/// This method does not apply to workgroups, servers, or shares.
		/// </summary>
		/// <seealso cref= #setCreateTime </seealso>
		/// <seealso cref= #setLastAccess </seealso>
		/// <seealso cref= #setLastModified
		/// </seealso>
		/// <param name="createTime">
		///            the create time as milliseconds since Jan 1, 1970 </param>
		/// <param name="lastModified">
		///            the last modified time as milliseconds since Jan 1, 1970 </param>
		/// <param name="lastAccess">
		///            the last access time as milliseconds since Jan 1, 1970 </param>
		/// <exception cref="CIFSException"> </exception>
		/// <exception cref="jcifs.smb.SmbUnsupportedOperationException">
		///             if CAP_NT_SMBS is unavailable </exception>
		/// throws CIFSException;
		void setFileTimes(long createTime, long lastModified, long lastAccess);


		/// <summary>
		/// Set the last access time of the file. The time is specified as milliseconds
		/// from Jan 1, 1970 which is the same as that which is returned by the
		/// <tt>lastAccess()</tt> method.
		/// <br>
		/// This method does not apply to workgroups, servers, or shares.
		/// </summary>
		/// <param name="time">
		///            the last access time as milliseconds since Jan 1, 1970 </param>
		/// <exception cref="CIFSException"> </exception>
		/// <exception cref="jcifs.smb.SmbUnsupportedOperationException">
		///             if CAP_NT_SMBS is unavailable </exception>
		/// throws CIFSException;
		void setLastAccess(long time);


		/// <summary>
		/// Set the last modified time of the file. The time is specified as milliseconds
		/// from Jan 1, 1970 which is the same as that which is returned by the
		/// <tt>lastModified()</tt> method.
		/// <br>
		/// This method does not apply to workgroups, servers, or shares.
		/// </summary>
		/// <param name="time">
		///            the last modified time as milliseconds since Jan 1, 1970 </param>
		/// <exception cref="CIFSException"> </exception>
		/// throws CIFSException;
		void setLastModified(long time);


		/// <summary>
		/// Set the create time of the file. The time is specified as milliseconds
		/// from Jan 1, 1970 which is the same as that which is returned by the
		/// <tt>createTime()</tt> method.
		/// <br>
		/// This method does not apply to workgroups, servers, or shares.
		/// </summary>
		/// <param name="time">
		///            the create time as milliseconds since Jan 1, 1970 </param>
		/// <exception cref="CIFSException"> </exception>
		/// <exception cref="jcifs.smb.SmbUnsupportedOperationException">
		///             if CAP_NT_SMBS is unavailable </exception>
		/// throws CIFSException;
		void setCreateTime(long time);


		/// <summary>
		/// Retrieve the last acces time of the file represented by this <code>SmbResource</code>
		/// </summary>
		/// <returns> The number of milliseconds since the 00:00:00 GMT, January 1,
		///         1970 as a <code>long</code> value </returns>
		/// <exception cref="CIFSException"> </exception>
		/// throws CIFSException;
		long lastAccess();


		/// <summary>
		/// Retrieve the last time the file represented by this
		/// <code>SmbResource</code> was modified. The value returned is suitable for
		/// constructing a <seealso cref="System.DateTime"/> object (i.e. seconds since Epoch
		/// 1970). Times should be the same as those reported using the properties
		/// dialog of the Windows Explorer program.
		/// </summary>
		/// <returns> The number of milliseconds since the 00:00:00 GMT, January 1,
		///         1970 as a <code>long</code> value </returns>
		/// <exception cref="CIFSException"> </exception>
		/// throws CIFSException;
		long lastModified();


		/// <summary>
		/// Retrieve the time this <code>SmbResource</code> was created. The value
		/// returned is suitable for constructing a <seealso cref="System.DateTime"/> object
		/// (i.e. seconds since Epoch 1970). Times should be the same as those
		/// reported using the properties dialog of the Windows Explorer program.
		/// 
		/// For Win95/98/Me this is actually the last write time. It is currently
		/// not possible to retrieve the create time from files on these systems.
		/// </summary>
		/// <returns> The number of milliseconds since the 00:00:00 GMT, January 1,
		///         1970 as a <code>long</code> value </returns>
		/// <exception cref="CIFSException"> </exception>
		/// throws CIFSException;
		long createTime();


		/// <summary>
		/// Create a new file but fail if it already exists. The check for
		/// existence of the file and it's creation are an atomic operation with
		/// respect to other filesystem activities.
		/// </summary>
		/// <exception cref="CIFSException"> </exception>
		/// throws CIFSException;
		void createNewFile();


		/// <summary>
		/// Creates a directory with the path specified by this <tt>SmbResource</tt>
		/// and any parent directories that do not exist. This method will fail
		/// when used with <code>smb://</code>, <code>smb://workgroup/</code>,
		/// <code>smb://server/</code>, or <code>smb://server/share/</code> URLs
		/// because workgroups, servers, and shares cannot be dynamically created
		/// (although in the future it may be possible to create shares).
		/// </summary>
		/// <exception cref="CIFSException"> </exception>
		/// throws CIFSException;
		void mkdirs();


		/// <summary>
		/// Creates a directory with the path specified by this
		/// <code>SmbResource</code>. For this method to be successful, the target
		/// must not already exist. This method will fail when
		/// used with <code>smb://</code>, <code>smb://workgroup/</code>,
		/// <code>smb://server/</code>, or <code>smb://server/share/</code> URLs
		/// because workgroups, servers, and shares cannot be dynamically created
		/// (although in the future it may be possible to create shares).
		/// </summary>
		/// <exception cref="CIFSException"> </exception>
		/// throws CIFSException;
		void mkdir();


		/// <summary>
		/// This method returns the free disk space in bytes of the drive this share
		/// represents or the drive on which the directory or file resides. Objects
		/// other than <tt>TYPE_SHARE</tt> or <tt>TYPE_FILESYSTEM</tt> will result
		/// in 0L being returned.
		/// </summary>
		/// <returns> the free disk space in bytes of the drive on which this file or
		///         directory resides </returns>
		/// <exception cref="CIFSException"> </exception>
		/// throws CIFSException;
		long getDiskFreeSpace();


		/// <summary>
		/// Returns the length of this <tt>SmbResource</tt> in bytes. If this object
		/// is a <tt>TYPE_SHARE</tt> the total capacity of the disk shared in
		/// bytes is returned. If this object is a directory or a type other than
		/// <tt>TYPE_SHARE</tt>, 0L is returned.
		/// </summary>
		/// <returns> The length of the file in bytes or 0 if this
		///         <code>SmbResource</code> is not a file. </returns>
		/// <exception cref="CIFSException"> </exception>
		/// throws CIFSException;
		long length();


		/// <summary>
		/// This method will delete the file or directory specified by this
		/// <code>SmbResource</code>. If the target is a directory, the contents of
		/// the directory will be deleted as well. If a file within the directory or
		/// it's sub-directories is marked read-only, the read-only status will
		/// be removed and the file will be deleted.
		/// 
		/// If the file has been opened before, it will be closed.
		/// </summary>
		/// <exception cref="CIFSException"> </exception>
		/// throws CIFSException;
		void delete();


		/// <summary>
		/// This method will copy the file or directory represented by this
		/// <tt>SmbResource</tt> and it's sub-contents to the location specified by the
		/// <tt>dest</tt> parameter. This file and the destination file do not
		/// need to be on the same host. This operation does not copy extended
		/// file attributes such as ACLs but it does copy regular attributes as
		/// well as create and last write times. This method is almost twice as
		/// efficient as manually copying as it employs an additional write
		/// thread to read and write data concurrently.
		/// <br>
		/// It is not possible (nor meaningful) to copy entire workgroups or
		/// servers.
		/// </summary>
		/// <param name="dest">
		///            the destination file or directory </param>
		/// <exception cref="CIFSException"> </exception>
		/// throws CIFSException;
		void copyTo(SmbResource dest);


		/// <summary>
		/// Changes the name of the file this <code>SmbResource</code> represents to the name
		/// designated by the <code>SmbResource</code> argument.
		/// <br>
		/// <i>Remember: <code>SmbResource</code>s are immutable and therefore
		/// the path associated with this <code>SmbResource</code> object will not
		/// change). To access the renamed file it is necessary to construct a
		/// new <tt>SmbResource</tt></i>.
		/// </summary>
		/// <param name="dest">
		///            An <code>SmbResource</code> that represents the new pathname </param>
		/// <exception cref="CIFSException"> </exception>
		/// <exception cref="NullPointerException">
		///             If the <code>dest</code> argument is <code>null</code> </exception>
		/// throws CIFSException;
		void renameTo(SmbResource dest);


		/// <summary>
		/// Changes the name of the file this <code>SmbResource</code> represents to the name
		/// designated by the <code>SmbResource</code> argument.
		/// <br>
		/// <i>Remember: <code>SmbResource</code>s are immutable and therefore
		/// the path associated with this <code>SmbResource</code> object will not
		/// change). To access the renamed file it is necessary to construct a
		/// new <tt>SmbResource</tt></i>.
		/// </summary>
		/// <param name="dest">
		///            An <code>SmbResource</code> that represents the new pathname </param>
		/// <param name="replace">
		///            Whether an existing destination file should be replaced (only supported with SMB2) </param>
		/// <exception cref="CIFSException"> </exception>
		/// <exception cref="NullPointerException">
		///             If the <code>dest</code> argument is <code>null</code> </exception>
		/// throws CIFSException;
		void renameTo(SmbResource dest, bool replace);


		/// <summary>
		/// Creates a directory watch
		/// 
		/// The server will notify the client when there are changes to the directories contents
		/// </summary>
		/// <param name="filter">
		///            see constants in <seealso cref="FileNotifyInformation"/> </param>
		/// <param name="recursive">
		///            whether to also watch subdirectories </param>
		/// <returns> watch context, needs to be closed when finished </returns>
		/// <exception cref="CIFSException"> </exception>
		/// throws CIFSException;
		SmbWatchHandle watch(int filter, bool recursive);


		/// <summary>
		/// Return the resolved owner group SID for this file or directory
		/// </summary>
		/// <returns> the owner group SID, <code>null</code> if not present </returns>
		/// <exception cref="IOException"> </exception>
		/// throws java.io.IOException;
		SID getOwnerGroup();


		/// <summary>
		/// Return the owner group SID for this file or directory
		/// </summary>
		/// <param name="resolve">
		///            whether to resolve the group name </param>
		/// <returns> the owner group SID, <code>null</code> if not present </returns>
		/// <exception cref="IOException"> </exception>
		/// throws java.io.IOException;
		SID getOwnerGroup(bool resolve);


		/// <summary>
		/// Return the resolved owner user SID for this file or directory
		/// </summary>
		/// <returns> the owner user SID, <code>null</code> if not present </returns>
		/// <exception cref="IOException"> </exception>
		/// throws java.io.IOException;
		SID getOwnerUser();


		/// <summary>
		/// Return the owner user SID for this file or directory
		/// </summary>
		/// <param name="resolve">
		///            whether to resolve the user name </param>
		/// <returns> the owner user SID, <code>null</code> if not present </returns>
		/// <exception cref="IOException"> </exception>
		/// throws java.io.IOException;
		SID getOwnerUser(bool resolve);


		/// <summary>
		/// Return an array of Access Control Entry (ACE) objects representing
		/// the security descriptor associated with this file or directory.
		/// <para>
		/// Initially, the SIDs within each ACE will not be resolved however when
		/// <tt>getType()</tt>, <tt>getDomainName()</tt>, <tt>getAccountName()</tt>,
		/// or <tt>toString()</tt> is called, the names will attempt to be
		/// resolved. If the names cannot be resolved (e.g. due to temporary
		/// network failure), the said methods will return default values (usually
		/// <tt>S-X-Y-Z</tt> strings of fragments of).
		/// </para>
		/// <para>
		/// Alternatively <tt>getSecurity(true)</tt> may be used to resolve all
		/// SIDs together and detect network failures.
		/// 
		/// </para>
		/// </summary>
		/// <returns> array of ACEs </returns>
		/// <exception cref="IOException"> </exception>
		/// throws java.io.IOException;
		ACE[] getSecurity();


		/// <summary>
		/// Return an array of Access Control Entry (ACE) objects representing
		/// the security descriptor associated with this file or directory.
		/// If no DACL is present, null is returned. If the DACL is empty, an array with 0 elements is returned.
		/// </summary>
		/// <param name="resolveSids">
		///            Attempt to resolve the SIDs within each ACE form
		///            their numeric representation to their corresponding account names. </param>
		/// <returns> array of ACEs </returns>
		/// <exception cref="IOException"> </exception>
		/// throws java.io.IOException;
		ACE[] getSecurity(bool resolveSids);


		/// <summary>
		/// Return an array of Access Control Entry (ACE) objects representing
		/// the share permissions on the share exporting this file or directory.
		/// If no DACL is present, null is returned. If the DACL is empty, an array with 0 elements is returned.
		/// <para>
		/// Note that this is different from calling <tt>getSecurity</tt> on a
		/// share. There are actually two different ACLs for shares - the ACL on
		/// the share and the ACL on the folder being shared.
		/// Go to <i>Computer Management</i>
		/// &gt; <i>System Tools</i> &gt; <i>Shared Folders</i> &gt; <i>Shares</i> and
		/// look at the <i>Properties</i> for a share. You will see two tabs - one
		/// for "Share Permissions" and another for "Security". These correspond to
		/// the ACLs returned by <tt>getShareSecurity</tt> and <tt>getSecurity</tt>
		/// respectively.
		/// 
		/// </para>
		/// </summary>
		/// <param name="resolveSids">
		///            Attempt to resolve the SIDs within each ACE form
		///            their numeric representation to their corresponding account names. </param>
		/// <returns> array of ACEs </returns>
		/// <exception cref="IOException"> </exception>
		/// throws java.io.IOException;
		ACE[] getShareSecurity(bool resolveSids);


		/// <summary>
		/// Opens the file for random access
		/// </summary>
		/// <param name="mode">
		///            access mode (r|rw) </param>
		/// <param name="sharing">
		///            flags indicating for which operations others may concurrently open the file </param>
		/// <returns> random access file, needs to be closed when finished </returns>
		/// <exception cref="CIFSException">
		///  </exception>
		/// throws CIFSException;
		SmbRandomAccess openRandomAccess(string mode, int sharing);


		/// <summary>
		/// Opens the file for random access
		/// </summary>
		/// <param name="mode">
		///            access mode (r|rw) </param>
		/// <returns> random access file, needs to be closed when finished </returns>
		/// <exception cref="CIFSException"> </exception>
		/// throws CIFSException;
		SmbRandomAccess openRandomAccess(string mode);


		/// <summary>
		/// Opens an output stream writing to the file (write only, exclusive write access)
		/// </summary>
		/// <param name="append">
		///            whether to append to or truncate the input </param>
		/// <param name="openFlags">
		///            flags for open operation </param>
		/// <param name="access">
		///            desired file access flags </param>
		/// <param name="sharing">
		///            flags indicating for which operations others may open the file </param>
		/// <returns> output stream, needs to be closed when finished </returns>
		/// <exception cref="CIFSException"> </exception>
		/// throws CIFSException;
		OutputStream openOutputStream(bool append, int openFlags, int access, int sharing);


		/// <summary>
		/// Opens an output stream writing to the file (write only, exclusive write access)
		/// </summary>
		/// <param name="append">
		///            whether to append to or truncate the input </param>
		/// <param name="sharing">
		///            flags indicating for which operations others may open the file (FILE_SHARING_*) </param>
		/// <returns> output stream, needs to be closed when finished </returns>
		/// <exception cref="CIFSException"> </exception>
		/// throws CIFSException;
		OutputStream openOutputStream(bool append, int sharing);


		/// <summary>
		/// Opens an output stream writing to the file (write only, read sharable)
		/// </summary>
		/// <param name="append">
		///            whether to append to or truncate the input </param>
		/// <returns> output stream, needs to be closed when finished </returns>
		/// <exception cref="CIFSException"> </exception>
		/// throws CIFSException;
		OutputStream openOutputStream(bool append);


		/// <summary>
		/// Opens an output stream writing to the file (truncating, write only, sharable)
		/// </summary>
		/// <returns> output stream, needs to be closed when finished </returns>
		/// <exception cref="CIFSException"> </exception>
		/// throws CIFSException;
		OutputStream openOutputStream();


		/// <summary>
		/// Opens an input stream reading the file (read only)
		/// </summary>
		/// <param name="flags">
		///            open flags </param>
		/// <param name="access">
		///            desired access flags </param>
		/// <param name="sharing">
		///            flags indicating for which operations others may open the file (FILE_SHARING_*) </param>
		/// <returns> input stream, needs to be closed when finished </returns>
		/// <exception cref="CIFSException"> </exception>
		/// throws CIFSException;
		InputStream openInputStream(int flags, int access, int sharing);


		/// <summary>
		/// Opens an input stream reading the file (read only)
		/// </summary>
		/// <param name="sharing">
		///            flags indicating for which operations others may open the file (FILE_SHARING_*)
		/// </param>
		/// <returns> input stream, needs to be closed when finished </returns>
		/// <exception cref="CIFSException"> </exception>
		/// throws CIFSException;
		InputStream openInputStream(int sharing);


		/// <summary>
		/// Opens an input stream reading the file (read only, sharable)
		/// </summary>
		/// <returns> input stream, needs to be closed when finished </returns>
		/// <exception cref="CIFSException"> </exception>
		/// throws CIFSException;
		InputStream openInputStream();


		// /// <summary>
		// /// Close/release the file
		// /// 
		// /// This releases all resources that this file holds. If not using strict mode this is currently a no-op.
		// /// </summary>
		// /// <seealso cref= java.lang.AutoCloseable#Dispose() </seealso>
		// void Dispose();


		/// <summary>
		/// Fetch all children
		/// </summary>
		/// <returns> an iterator over the child resources </returns>
		/// <exception cref="CIFSException"> </exception>
		/// throws CIFSException;
		CloseableIterator<SmbResource> children();


		/// <summary>
		/// Fetch children matching pattern, server-side filtering
		/// 
		/// <para>
		/// The wildcard expression may consist of two special meta
		/// characters in addition to the normal filename characters. The '*'
		/// character matches any number of characters in part of a name. If
		/// the expression begins with one or more '?'s then exactly that
		/// many characters will be matched whereas if it ends with '?'s
		/// it will match that many characters <i>or less</i>.
		/// </para>
		/// <para>
		/// Wildcard expressions will not filter workgroup names or server names.
		/// 
		/// </para>
		/// </summary>
		/// <param name="wildcard"> </param>
		/// <returns> an iterator over the child resources </returns>
		/// <exception cref="CIFSException"> </exception>
		/// throws CIFSException;
		CloseableIterator<SmbResource> children(string wildcard);


		/// <param name="filter">
		///            filter acting on file names </param>
		/// <returns> an iterator over the child resources </returns>
		/// <seealso cref= SmbResource#children(String) for a more efficient way to do this when a pattern on the filename is
		///      sufficient for filtering </seealso>
		/// <exception cref="CIFSException"> </exception>
		/// throws CIFSException;
		CloseableIterator<SmbResource> children(ResourceNameFilter filter);


		/// <param name="filter">
		///            filter acting on SmbResource instances </param>
		/// <returns> an iterator over the child resources </returns>
		/// <seealso cref= SmbResource#children(String) for a more efficient way to do this when a pattern on the filename is
		///      sufficient for filtering </seealso>
		/// <exception cref="CIFSException"> </exception>
		/// throws CIFSException;
		CloseableIterator<SmbResource> children(ResourceFilter filter);

	}
}