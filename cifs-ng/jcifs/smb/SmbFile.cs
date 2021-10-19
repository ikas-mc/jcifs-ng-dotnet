using jcifs.@internal.fscc;

using System;
using System.IO;
using System.Threading;
using cifs_ng.lib;
using cifs_ng.lib.ext;
using cifs_ng.lib.io;
using cifs_ng.lib.socket;
using Logger = org.slf4j.Logger;
using LoggerFactory = org.slf4j.LoggerFactory;
using CIFSContext = jcifs.CIFSContext;
using CIFSException = jcifs.CIFSException;
using jcifs;
using jcifs.@internal.dtyp;
using Configuration = jcifs.Configuration;
using ResourceFilter = jcifs.ResourceFilter;
using ResourceNameFilter = jcifs.ResourceNameFilter;
using SmbConstants = jcifs.SmbConstants;
using SmbFileHandle = jcifs.SmbFileHandle;
using SmbResource = jcifs.SmbResource;
using SmbResourceLocator = jcifs.SmbResourceLocator;
using SmbTreeHandle = jcifs.SmbTreeHandle;
using SmbWatchHandle = jcifs.SmbWatchHandle;
using SingletonContext = jcifs.context.SingletonContext;
using DcerpcHandle = jcifs.dcerpc.DcerpcHandle;
using MsrpcShareGetInfo = jcifs.dcerpc.msrpc.MsrpcShareGetInfo;
using AllocInfo = jcifs.@internal.AllocInfo;
using SmbBasicFileInfo = jcifs.@internal.SmbBasicFileInfo;
using ACE = jcifs.@internal.dtyp.ACE;
using SecurityDescriptor = jcifs.@internal.dtyp.SecurityDescriptor;
using SecurityInfo = jcifs.@internal.dtyp.SecurityInfo;
using BasicFileInformation = jcifs.@internal.fscc.BasicFileInformation;
using FileBasicInfo = jcifs.@internal.fscc.FileBasicInfo;
using FileInformation = jcifs.@internal.fscc.FileInformation;
using FileInternalInfo = jcifs.@internal.fscc.FileInternalInfo;
using FileRenameInformation2 = jcifs.@internal.fscc.FileRenameInformation2;
using FileStandardInfo = jcifs.@internal.fscc.FileStandardInfo;
using FileSystemInformation = jcifs.@internal.fscc.FileSystemInformation;
using SmbComBlankResponse = jcifs.@internal.smb1.com.SmbComBlankResponse;
using SmbComCreateDirectory = jcifs.@internal.smb1.com.SmbComCreateDirectory;
using SmbComDelete = jcifs.@internal.smb1.com.SmbComDelete;
using SmbComDeleteDirectory = jcifs.@internal.smb1.com.SmbComDeleteDirectory;
using SmbComNTCreateAndX = jcifs.@internal.smb1.com.SmbComNTCreateAndX;
using SmbComNTCreateAndXResponse = jcifs.@internal.smb1.com.SmbComNTCreateAndXResponse;
using SmbComOpenAndX = jcifs.@internal.smb1.com.SmbComOpenAndX;
using SmbComOpenAndXResponse = jcifs.@internal.smb1.com.SmbComOpenAndXResponse;
using SmbComQueryInformation = jcifs.@internal.smb1.com.SmbComQueryInformation;
using SmbComQueryInformationResponse = jcifs.@internal.smb1.com.SmbComQueryInformationResponse;
using SmbComRename = jcifs.@internal.smb1.com.SmbComRename;
using SmbComSeek = jcifs.@internal.smb1.com.SmbComSeek;
using SmbComSeekResponse = jcifs.@internal.smb1.com.SmbComSeekResponse;
using SmbComSetInformation = jcifs.@internal.smb1.com.SmbComSetInformation;
using SmbComSetInformationResponse = jcifs.@internal.smb1.com.SmbComSetInformationResponse;
using NtTransQuerySecurityDesc = jcifs.@internal.smb1.trans.nt.NtTransQuerySecurityDesc;
using NtTransQuerySecurityDescResponse = jcifs.@internal.smb1.trans.nt.NtTransQuerySecurityDescResponse;
using Trans2QueryFSInformation = jcifs.@internal.smb1.trans2.Trans2QueryFSInformation;
using Trans2QueryFSInformationResponse = jcifs.@internal.smb1.trans2.Trans2QueryFSInformationResponse;
using Trans2QueryPathInformation = jcifs.@internal.smb1.trans2.Trans2QueryPathInformation;
using Trans2QueryPathInformationResponse = jcifs.@internal.smb1.trans2.Trans2QueryPathInformationResponse;
using Trans2SetFileInformation = jcifs.@internal.smb1.trans2.Trans2SetFileInformation;
using Trans2SetFileInformationResponse = jcifs.@internal.smb1.trans2.Trans2SetFileInformationResponse;
using jcifs.@internal.smb2;
using ServerMessageBlock2Response = jcifs.@internal.smb2.ServerMessageBlock2Response;
using Smb2Constants = jcifs.@internal.smb2.Smb2Constants;
using Smb2CloseRequest = jcifs.@internal.smb2.create.Smb2CloseRequest;
using Smb2CloseResponse = jcifs.@internal.smb2.create.Smb2CloseResponse;
using Smb2CreateRequest = jcifs.@internal.smb2.create.Smb2CreateRequest;
using Smb2CreateResponse = jcifs.@internal.smb2.create.Smb2CreateResponse;
using Smb2QueryInfoRequest = jcifs.@internal.smb2.info.Smb2QueryInfoRequest;
using Smb2QueryInfoResponse = jcifs.@internal.smb2.info.Smb2QueryInfoResponse;
using Smb2SetInfoRequest = jcifs.@internal.smb2.info.Smb2SetInfoRequest;

/* jcifs smb client library in Java
 * Copyright (C) 2000  "Michael B. Allen" <jcifs at samba dot org>
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
	/// This class represents a resource on an SMB network. Mainly these
	/// resources are files and directories however an <code>SmbFile</code>
	/// may also refer to servers and workgroups. If the resource is a file or
	/// directory the methods of <code>SmbFile</code> follow the behavior of
	/// the well known <seealso cref="java.io.File"/> class. One fundamental difference
	/// is the usage of a URL scheme [1] to specify the target file or
	/// directory. SmbFile URLs have the following syntax:
	/// 
	/// <blockquote>
	/// 
	/// <pre>
	///     smb://[[[domain;]username[:password]@]server[:port]/[[share/[dir/]file]]][?param=value[param2=value2[...]]]
	/// </pre>
	/// 
	/// </blockquote>
	/// 
	/// This example:
	/// 
	/// <blockquote>
	/// 
	/// <pre>
	///     smb://storage15/public/foo.txt
	/// </pre>
	/// 
	/// </blockquote>
	/// 
	/// would reference the file <code>foo.txt</code> in the share
	/// <code>public</code> on the server <code>storage15</code>. In addition
	/// to referencing files and directories, jCIFS can also address servers,
	/// and workgroups.
	/// <para>
	/// <font color="#800000"><i>Important: all SMB URLs that represent
	/// workgroups, servers, shares, or directories require a trailing slash '/'.
	/// </i></font>
	/// </para>
	/// <para>
	/// When using the <tt>java.net.URL</tt> class with
	/// 'smb://' URLs it is necessary to first call the static
	/// <tt>jcifs.Config.registerSmbURLHandler();</tt> method. This is required
	/// to register the SMB protocol handler.
	/// </para>
	/// <para>
	/// The userinfo component of the SMB URL (<tt>domain;user:pass</tt>) must
	/// be URL encoded if it contains reserved characters. According to RFC 2396
	/// these characters are non US-ASCII characters and most meta characters
	/// however jCIFS will work correctly with anything but '@' which is used
	/// to delimit the userinfo component from the server and '%' which is the
	/// URL escape character itself.
	/// </para>
	/// <para>
	/// The server
	/// component may a traditional NetBIOS name, a DNS name, or IP
	/// address. These name resolution mechanisms and their resolution order
	/// can be changed (See <a href="../../../resolver.html">Setting Name
	/// Resolution Properties</a>). The servername and path components are
	/// not case sensitive but the domain, username, and password components
	/// are. It is also likely that properties must be specified for jcifs
	/// to function (See <a href="../../overview-summary.html#scp">Setting
	/// JCIFS Properties</a>). Here are some examples of SMB URLs with brief
	/// descriptions of what they do:
	/// 
	/// </para>
	/// <para>
	/// [1] This URL scheme is based largely on the <i>SMB
	/// Filesharing URL Scheme</i> IETF draft.
	/// 
	/// </para>
	/// <para>
	/// <table border="1" cellpadding="3" cellspacing="0" width="100%" summary="URL examples">
	/// <tr bgcolor="#ccccff">
	/// <td colspan="2"><b>SMB URL Examples</b></td>
	/// <tr>
	/// <td width="20%"><b>URL</b></td>
	/// <td><b>Description</b></td>
	/// </tr>
	/// 
	/// <tr>
	/// <td width="20%"><code>smb://users-nyc;miallen:mypass@angus/tmp/</code></td>
	/// <td>
	/// This URL references a share called <code>tmp</code> on the server
	/// <code>angus</code> as user <code>miallen</code> who's password is
	/// <code>mypass</code>.
	/// </td>
	/// </tr>
	/// 
	/// <tr>
	/// <td width="20%">
	/// <code>smb://Administrator:P%40ss@msmith1/c/WINDOWS/Desktop/foo.txt</code></td>
	/// <td>
	/// A relatively sophisticated example that references a file
	/// <code>msmith1</code>'s desktop as user <code>Administrator</code>. Notice the '@' is URL encoded with the '%40'
	/// hexcode escape.
	/// </td>
	/// </tr>
	/// 
	/// <tr>
	/// <td width="20%"><code>smb://angus/</code></td>
	/// <td>
	/// This references only a server. The behavior of some methods is different
	/// in this context(e.g. you cannot <code>delete</code> a server) however
	/// as you might expect the <code>list</code> method will list the available
	/// shares on this server.
	/// </td>
	/// </tr>
	/// 
	/// <tr>
	/// <td width="20%"><code>smb://angus.foo.net/d/jcifs/pipes.doc</code></td>
	/// <td>
	/// The server name may also be a DNS name as it is in this example. See
	/// <a href="../../../resolver.html">Setting Name Resolution Properties</a>
	/// for details.
	/// </td>
	/// </tr>
	/// 
	/// <tr>
	/// <td width="20%"><code>smb://192.168.1.15/ADMIN$/</code></td>
	/// <td>
	/// The server name may also be an IP address. See <a
	/// href="../../../resolver.html">Setting Name Resolution Properties</a>
	/// for details.
	/// </td>
	/// </tr>
	/// 
	/// <tr>
	/// <td width="20%">
	/// <code>smb://domain;username:password@server/share/path/to/file.txt</code></td>
	/// <td>
	/// A prototypical example that uses all the fields.
	/// </td>
	/// </tr>
	/// 
	/// <tr>
	/// <td width="20%">
	/// <code>smb://server/share/path/to/dir &lt;-- ILLEGAL </code></td>
	/// <td>
	/// URLs that represent servers, shares, or directories require a trailing slash '/'.
	/// </td>
	/// </tr>
	/// 
	/// </table>
	/// 
	/// </para>
	/// <para>
	/// A second constructor argument may be specified to augment the URL
	/// for better programmatic control when processing many files under
	/// a common base. This is slightly different from the corresponding
	/// <code>java.io.File</code> usage; a '/' at the beginning of the second
	/// parameter will still use the server component of the first parameter. The
	/// examples below illustrate the resulting URLs when this second constructor
	/// argument is used.
	/// 
	/// </para>
	/// <para>
	/// <table border="1" cellpadding="3" cellspacing="0" width="100%" summary="Usage examples">
	/// <tr bgcolor="#ccccff">
	/// <td colspan="3">
	/// <b>Examples Of SMB URLs When Augmented With A Second Constructor Parameter</b></td>
	/// <tr>
	/// <td width="20%">
	/// <b>First Parameter</b></td>
	/// <td><b>Second Parameter</b></td>
	/// <td><b>Result</b></td>
	/// </tr>
	/// 
	/// <tr>
	/// <td width="20%"><code>
	///  smb://host/share/a/b/
	/// </code></td>
	/// <td width="20%"><code>
	///  c/d/
	/// </code></td>
	/// <td><code>
	///  smb://host/share/a/b/c/d/
	/// </code></td>
	/// </tr>
	/// 
	/// <tr>
	/// <td width="20%"><code>
	///  smb://host/share/foo/bar/
	/// </code></td>
	/// <td width="20%"><code>
	///  /share2/zig/zag
	/// </code></td>
	/// <td><code>
	///  smb://host/share2/zig/zag
	/// </code></td>
	/// </tr>
	/// 
	/// <tr>
	/// <td width="20%"><code>
	///  smb://host/share/foo/bar/
	/// </code></td>
	/// <td width="20%"><code>
	///  ../zip/
	/// </code></td>
	/// <td><code>
	///  smb://host/share/foo/zip/
	/// </code></td>
	/// </tr>
	/// 
	/// <tr>
	/// <td width="20%"><code>
	///  smb://host/share/zig/zag
	/// </code></td>
	/// <td width="20%"><code>
	///  smb://foo/bar/
	/// </code></td>
	/// <td><code>
	///  smb://foo/bar/
	/// </code></td>
	/// </tr>
	/// 
	/// <tr>
	/// <td width="20%"><code>
	///  smb://host/share/foo/
	/// </code></td>
	/// <td width="20%"><code>
	///  ../.././.././../foo/
	/// </code></td>
	/// <td><code>
	///  smb://host/foo/
	/// </code></td>
	/// </tr>
	/// 
	/// <tr>
	/// <td width="20%"><code>
	///  smb://host/share/zig/zag
	/// </code></td>
	/// <td width="20%"><code>
	///  /
	/// </code></td>
	/// <td><code>
	///  smb://host/
	/// </code></td>
	/// </tr>
	/// 
	/// <tr>
	/// <td width="20%"><code>
	///  smb://server/
	/// </code></td>
	/// <td width="20%"><code>
	///  ../
	/// </code></td>
	/// <td><code>
	///  smb://server/
	/// </code></td>
	/// </tr>
	/// 
	/// </table>
	/// 
	/// </para>
	/// <para>
	/// Instances of the <code>SmbFile</code> class are immutable; that is,
	/// once created, the abstract pathname represented by an SmbFile object
	/// will never change.
	/// 
	/// </para>
	/// </summary>
	/// <seealso cref= java.io.File </seealso>

	public class SmbFile : URLConnection, SmbResource{

		protected internal const int ATTR_GET_MASK = 0x7FFF;
		protected internal const int ATTR_SET_MASK = 0x30A7;
		protected internal const int DEFAULT_ATTR_EXPIRATION_PERIOD = 5000;

		protected internal static readonly int HASH_DOT = ".".GetHashCode();
		protected internal static readonly int HASH_DOT_DOT = "..".GetHashCode();

		private static Logger log = LoggerFactory.getLogger(typeof(SmbFile));

		private long createTimeValue;
		private long lastModifiedValue;
		private long lastAccessValue;
		private int attributesValue;
		private long attrExpiration;
		private long size;
		private long sizeExpiration;
		private bool isExists;

		private CIFSContext transportContext;
		private SmbTreeConnection treeConnection;
		 internal readonly SmbResourceLocatorImpl fileLocator;
		private SmbTreeHandleImpl treeHandle;


		/// <summary>
		/// Constructs an SmbFile representing a resource on an SMB network such as
		/// a file or directory. See the description and examples of smb URLs above.
		/// </summary>
		/// <param name="url">
		///            A URL string </param>
		/// <exception cref="MalformedURLException">
		///             If the <code>parent</code> and <code>child</code> parameters
		///             do not follow the prescribed syntax </exception>
		/// throws java.net.MalformedURLException
		[Obsolete]
		public SmbFile(string url) : this(new URL( url)) {
		}


		/// <summary>
		/// Constructs an SmbFile representing a resource on an SMB network such
		/// as a file or directory from a <tt>URL</tt> object.
		/// </summary>
		/// <param name="url">
		///            The URL of the target resource </param>
		/// <exception cref="MalformedURLException"> </exception>
		/// throws java.net.MalformedURLException
		[Obsolete]
		public SmbFile(URL url) : this(url, SingletonContext.getInstance().withCredentials(new NtlmPasswordAuthentication(SingletonContext.getInstance(), url.UserInfo))) {
		}


		/// <summary>
		/// Constructs an SmbFile representing a resource on an SMB network such
		/// as a file or directory. The second parameter is a relative path from
		/// the <code>parent SmbFile</code>. See the description above for examples
		/// of using the second <code>name</code> parameter.
		/// </summary>
		/// <param name="context">
		///            A base <code>SmbFile</code> </param>
		/// <param name="name">
		///            A path string relative to the <code>parent</code> parameter </param>
		/// <exception cref="MalformedURLException">
		///             If the <code>parent</code> and <code>child</code> parameters
		///             do not follow the prescribed syntax </exception>
		/// <exception cref="UnknownHostException">
		///             If the server or workgroup of the <tt>context</tt> file cannot be determined </exception>
		/// throws MalformedURLException, java.net.UnknownHostException
		public SmbFile(SmbResource context, string name) : this(isWorkgroup(context) ? new URL("smb://" + checkName(name)) : new URL(context.getLocator().getURL(), encodeRelativePath(checkName(name))), context.getContext()) {
			setContext(context, name);
		}


		/// <summary>
		/// Construct from string URL
		/// </summary>
		/// <param name="url"> </param>
		/// <param name="tc">
		///            context to use </param>
		/// <exception cref="MalformedURLException"> </exception>
		/// throws java.net.MalformedURLException
		public SmbFile(string url, CIFSContext tc) : this(new URL(url), tc) {
		}


		/// <summary>
		/// Construct from URL
		/// </summary>
		/// <param name="url"> </param>
		/// <param name="tc">
		///            context to use </param>
		/// <exception cref="MalformedURLException"> </exception>
		/// throws java.net.MalformedURLException
		public SmbFile(URL url, CIFSContext tc) : base(url) {
			if (!string.IsNullOrEmpty(url.getPath()) && url.getPath()[0] != '/') {
				throw new UriFormatException("Invalid SMB URL: " + url);
			}
			this.transportContext = tc;
			this.fileLocator = new SmbResourceLocatorImpl(tc, url);
			this.treeConnection = SmbTreeConnection.create(tc);
		}


		/// throws java.net.MalformedURLException
		internal SmbFile(SmbResource context, string name, bool loadedAttributes, int type, int attributesParam, long createTimeParam, long lastModifiedParam, long lastAccessParam, long size) : this(isWorkgroup(context) ? new URL(null, "smb://" + checkName(name) + "/") : new URL(context.getLocator().getURL(), encodeRelativePath(checkName(name)) + ((attributesParam & SmbConstants.ATTR_DIRECTORY) > 0 ? "/" : "")), context.getContext()) {

			if (!isWorkgroup(context)) {
				setContext(context, name + ((attributesParam & SmbConstants.ATTR_DIRECTORY) > 0 ? "/" : ""));
			}

			/*
			 * why? am I going around in circles?
			 * this.type = type == TYPE_WORKGROUP ? 0 : type;
			 */
			this.fileLocator.updateType(type);
			this.attributesValue = attributesParam;
			this.createTimeValue = createTimeParam;
			this.lastModifiedValue = lastModifiedParam;
			this.lastAccessValue = lastAccessParam;
			this.size = size;
			this.isExists = true;

			if (loadedAttributes) {
				this.attrExpiration = this.sizeExpiration = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + getContext().getConfig().getAttributeCacheTimeout();
			}
		}


		private static string encodeRelativePath(string name) {
			return name;
		}


		/// <summary>
		/// @return
		/// </summary>
		private static bool isWorkgroup(SmbResource r) {
			try {
				return r.getLocator().isWorkgroup();
			}
			catch (CIFSException e) {
				log.debug("Failed to check for workgroup", e);
				return false;
			}
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= java.net.URLConnection#connect() </seealso>
		/// throws java.io.IOException
		public override void connect() {
			using (SmbTreeHandle th = ensureTreeConnected()) {
			}
		}


		/// 
		/// <returns> a tree handle </returns>
		/// <exception cref="CIFSException"> </exception>
		/// throws jcifs.CIFSException
		public virtual SmbTreeHandle getTreeHandle() {
			return ensureTreeConnected();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= java.lang.AutoCloseable#Dispose() </seealso>
		public virtual void Dispose() {
			lock (this) {
				SmbTreeHandleImpl th = this.treeHandle;
				if (th != null) {
					this.treeHandle = null;
					if (this.transportContext.getConfig().isStrictResourceLifecycle()) {
						th.Dispose();
					}
				}
			}
		}


		/// <summary>
		/// @return </summary>
		/// <exception cref="CIFSException">
		///  </exception>
		/// throws jcifs.CIFSException
		internal virtual SmbTreeHandleImpl ensureTreeConnected() {
			lock (this) {
				if (this.treeHandle == null || !this.treeHandle.isConnected()) {
					if (this.treeHandle != null && this.transportContext.getConfig().isStrictResourceLifecycle()) {
						this.treeHandle.release();
					}
					this.treeHandle = this.treeConnection.connectWrapException(this.fileLocator);
					this.treeHandle.ensureDFSResolved();
					if (this.transportContext.getConfig().isStrictResourceLifecycle()) {
						// one extra share to keep the tree alive
						return this.treeHandle.acquire();
					}
					return this.treeHandle;
				}
				return this.treeHandle.acquire();
			}
		}


		/// <param name="context"> </param>
		/// <param name="name"> </param>
		private void setContext(SmbResource context, string name) {
			this.fileLocator.resolveInContext(context.getLocator(), name);
			if (context.getLocator().getShare()!=null && (context is SmbFile)) {
				this.treeConnection = SmbTreeConnection.create(((SmbFile) context).treeConnection);
			}
			else {
				this.treeConnection = SmbTreeConnection.create(context.getContext());
			}
		}


		/// throws java.net.MalformedURLException
		private static string checkName(string name) {
			if (string.IsNullOrEmpty(name)) {
				throw new UriFormatException("Name must not be empty");
			}
			return name;
		}


		/// <param name="nonPooled">
		///            whether this file will use an exclusive connection </param>
		protected internal virtual void setNonPooled(bool nonPooled) {
			this.treeConnection.setNonPooled(nonPooled);
		}


		/// <returns> the transportContext </returns>
		[Obsolete]
		public virtual CIFSContext getTransportContext() {
			return this.getContext();
		}


		public virtual CIFSContext getContext() {
			return this.transportContext;
		}


		public virtual SmbResourceLocator getLocator() {
			return this.fileLocator;
		}


		/// throws jcifs.CIFSException
		public virtual SmbResource resolve(string name) {
			try {
				if (string.IsNullOrEmpty(name)) {
					throw new SmbException("Name must not be empty");
				}
				return new SmbFile(this, name);
			}
			catch (Exception e) when (e is UriFormatException || e is UnknownHostException) {
				// this should not actually happen
				throw new SmbException("Failed to resolve child element", e);
			}
		}


		/// throws jcifs.CIFSException
		internal virtual SmbFileHandleImpl openUnshared(int flags, int access, int sharing, int attrs, int options) {
			return openUnshared(getUncPath(), flags, access, sharing, attrs, options);
		}


		/// throws jcifs.CIFSException
		internal virtual SmbFileHandleImpl openUnshared(string uncPath, int flags, int access, int sharing, int attrs, int options) {
			SmbFileHandleImpl fh = null;
			using (SmbTreeHandleImpl h = ensureTreeConnected()) {

				if (log.isDebugEnabled()) {
					log.debug(string.Format("openUnshared: {0} flags: {1:x} access: {2:x} attrs: {3:x} options: {4:x}", uncPath, flags, access, attrs, options));
				}

				Configuration config = h.getConfig();
				SmbBasicFileInfo info;
				bool haveSize = true, haveAttributes = true;
				long fileSize = 0;
				if (h.isSMB2()) {
					Smb2CreateRequest req = new Smb2CreateRequest(config, uncPath);
					req.setDesiredAccess(access);

					if ((flags & SmbConstants.O_TRUNC) == SmbConstants.O_TRUNC && (flags & SmbConstants.O_CREAT) == SmbConstants.O_CREAT) {
						req.setCreateDisposition(Smb2CreateRequest.FILE_OVERWRITE_IF);
					}
					else if ((flags & SmbConstants.O_TRUNC) == SmbConstants.O_TRUNC) {
						req.setCreateDisposition(Smb2CreateRequest.FILE_OVERWRITE);
					}
					else if ((flags & SmbConstants.O_EXCL) == SmbConstants.O_EXCL) {
						req.setCreateDisposition(Smb2CreateRequest.FILE_CREATE);
					}
					else if ((flags & SmbConstants.O_CREAT) == SmbConstants.O_CREAT) {
						req.setCreateDisposition(Smb2CreateRequest.FILE_OPEN_IF);
					}
					else {
						req.setCreateDisposition(Smb2CreateRequest.FILE_OPEN);
					}

					req.setShareAccess(sharing);
					req.setFileAttributes(attrs);
					Smb2CreateResponse resp = h.send(req);
					info = resp;
					fileSize = resp.getEndOfFile();
					fh = new SmbFileHandleImpl(config, resp.getFileId(), h, uncPath, flags, access, 0, 0, resp.getEndOfFile());
				}
				else if (h.hasCapability(SmbConstants.CAP_NT_SMBS)) {
					SmbComNTCreateAndXResponse resp = new SmbComNTCreateAndXResponse(config);
					SmbComNTCreateAndX req = new SmbComNTCreateAndX(config, uncPath, flags, access, sharing, attrs, options, null);
					customizeCreate(req, resp);

					h.send(req, resp);
					info = resp;
					fileSize = resp.getEndOfFile();
					this.fileLocator.updateType(resp.getFileType());
					fh = new SmbFileHandleImpl(config, resp.getFid(), h, uncPath, flags, access, attrs, options, resp.getEndOfFile());
				}
				else {
					SmbComOpenAndXResponse response = new SmbComOpenAndXResponse(config);
					h.send(new SmbComOpenAndX(config, uncPath, access, sharing, flags, attrs, null), response);
					this.fileLocator.updateType(response.getFileType());
					info = response;
					fileSize = response.getDataSize();

					// this is so damn unreliable, needs another race-prone query if required
					haveAttributes = false;

					// This seems to be the only way to obtain a reliable (with respect to locking) file size here
					// It is more critical than other attributes because append mode depends on it.
					// We do only really care if we open for writing and not shared for writing
					// otherwise there are no guarantees anyway, but this stuff is legacy anyways.
					SmbComSeek seekReq = new SmbComSeek(config, 0);
					seekReq.setMode(0x2); // from EOF
					SmbComSeekResponse seekResp = new SmbComSeekResponse(config);
					seekReq.setFid(response.getFid());
					try {
						h.send(seekReq, seekResp);
						if (log.isDebugEnabled() && seekResp.getOffset() != fileSize) {
							log.debug(string.Format("Open returned wrong size {0:D} != {1:D}", fileSize, seekResp.getOffset()));
						}
						fileSize = seekResp.getOffset();
					}
					catch (Exception e) {
						log.debug("Seek failed", e);
						haveSize = false;
					}
					fh = new SmbFileHandleImpl(config, response.getFid(), h, uncPath, flags, access, 0, 0, fileSize);
				}

				long attrTimeout = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + config.getAttributeCacheTimeout();

				if (haveSize) {
					this.size = fileSize;
					this.sizeExpiration = attrTimeout;

				}
				if (haveAttributes) {
					this.createTimeValue = info.getCreateTime();
					this.lastModifiedValue = info.getLastWriteTime();
					this.lastAccessValue = info.getLastAccessTime();
					this.attributesValue = info.getAttributes() & ATTR_GET_MASK;
					this.attrExpiration = attrTimeout;
				}

				this.isExists = true;
				return fh;
			}
		}


		/// <returns> this file's unc path below the share </returns>
		public virtual string getUncPath() {
			return this.fileLocator.getUNCPath();
		}


		/// <param name="request"> </param>
		/// <param name="response"> </param>
		protected internal virtual void customizeCreate(SmbComNTCreateAndX request, SmbComNTCreateAndXResponse response) {
		}


		/// throws jcifs.CIFSException
		internal virtual SmbBasicFileInfo queryPath(SmbTreeHandleImpl th, string path, int infoLevel) {
			if (log.isDebugEnabled()) {
				log.debug("queryPath: " + path);
			}

			/*
			 * We really should do the referral before this in case
			 * the redirected target has different capabilities. But
			 * the way we have been doing that is to call exists() which
			 * calls this method so another technique will be necessary
			 * to support DFS referral _to_ Win95/98/ME.
			 */

			if (th.isSMB2()) {
				// just open and close. withOpen will store the attributes
				//TODO 
				return (SmbBasicFileInfo) withOpen<Smb2CloseResponse>(th, Smb2CreateRequest.FILE_OPEN, SmbConstants.FILE_READ_ATTRIBUTES, SmbConstants.FILE_SHARE_READ | SmbConstants.FILE_SHARE_WRITE, null);
			}
			else if (th.hasCapability(SmbConstants.CAP_NT_SMBS)) {
				/*
				 * Trans2 Query Path Information Request / Response
				 */
				Trans2QueryPathInformationResponse response1 = new Trans2QueryPathInformationResponse(th.getConfig(), infoLevel);
				response1 = th.send(new Trans2QueryPathInformation(th.getConfig(), path, infoLevel), response1);

				if (log.isDebugEnabled()) {
					log.debug("Path information " + response1);
				}
				BasicFileInformation info = response1.getInfo<BasicFileInformation>(typeof(BasicFileInformation));
				this.isExists = true;
				if (info is FileBasicInfo) {
					this.attributesValue = info.getAttributes() & ATTR_GET_MASK;
					this.createTimeValue = info.getCreateTime();
					this.lastModifiedValue = info.getLastWriteTime();
					this.lastAccessValue = info.getLastAccessTime();
					this.attrExpiration = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + th.getConfig().getAttributeCacheTimeout();
				}
				else if (info is FileStandardInfo) {
					this.size = info.getSize();
					this.sizeExpiration = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + th.getConfig().getAttributeCacheTimeout();
				}
				return info;
			}

			/*
			 * Query Information Request / Response
			 */
			SmbComQueryInformationResponse response = new SmbComQueryInformationResponse(th.getConfig(), th.getServerTimeZoneOffset());
			response = th.send(new SmbComQueryInformation(th.getConfig(), path), response);
			if (log.isDebugEnabled()) {
				log.debug("Legacy path information " + response);
			}

			this.isExists = true;
			this.attributesValue = response.getAttributes() & ATTR_GET_MASK;
			this.lastModifiedValue = response.getLastWriteTime();
			this.attrExpiration = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + th.getConfig().getAttributeCacheTimeout();

			this.size = response.getSize();
			this.sizeExpiration = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + th.getConfig().getAttributeCacheTimeout();
			return response;
		}


		/// throws SmbException
		public virtual bool exists() {

			if (this.attrExpiration > DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()) {
				log.trace("Using cached attributes");
				return this.isExists;
			}

			this.attributesValue = SmbConstants.ATTR_READONLY | SmbConstants.ATTR_DIRECTORY;
			this.createTimeValue = 0L;
			this.lastModifiedValue = 0L;
			this.lastAccessValue = 0L;
			this.isExists = false;

			try {
				if (this.url.Host.Length == 0) {
				}
				else if (this.fileLocator.getShare()==null) {
					if (this.fileLocator.getType() == SmbConstants.TYPE_WORKGROUP) {
						getContext().getNameServiceClient().getByName(this.url.Host, true);
					}
					else {
						getContext().getNameServiceClient().getByName(this.url.Host).getHostName();
					}
				}
				else {
					// queryPath on a share root will fail, we only know whether this is one after we have resolved DFS
					// referrals.
					using (SmbTreeHandleImpl th = ensureTreeConnected()) {
						if (this.fileLocator.getType() == SmbConstants.TYPE_SHARE) {
							// treeConnect is good enough, but we need to do this after resolving DFS
							using (SmbTreeHandleImpl th2 = ensureTreeConnected()) {
							}
						}
						else {
							queryPath(th, this.fileLocator.getUNCPath(), FileInformationConstants.FILE_BASIC_INFO);
						}
					}
				}

				/*
				 * If any of the above fail, isExists will not be set true
				 */

				this.isExists = true;

			}
			catch (UnknownHostException uhe) {
				log.debug("Unknown host", uhe);
			}
			catch (SmbException se) {
				log.trace("exists:", se);
				switch (se.getNtStatus()) {
				case NtStatus.NT_STATUS_NO_SUCH_FILE:
				case NtStatus.NT_STATUS_OBJECT_NAME_INVALID:
				case NtStatus.NT_STATUS_OBJECT_NAME_NOT_FOUND:
				case NtStatus.NT_STATUS_OBJECT_PATH_NOT_FOUND:
					break;
				default:
					throw se;
				}
			}
			catch (CIFSException e) {
				throw SmbException.wrap(e);
			}

			this.attrExpiration = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + getContext().getConfig().getAttributeCacheTimeout();
			return this.isExists;
		}


		//TODO name
		/// throws SmbException
		public virtual int getType() {
			try {
				int t = this.fileLocator.getType();
				if (t == SmbConstants.TYPE_SHARE) {
					using (SmbTreeHandle th = ensureTreeConnected()) {
						this.fileLocator.updateType(th.getTreeType());
					}
				}
				return t;
			}
			catch (CIFSException e) {
				throw SmbException.wrap(e);
			}
		}


		public virtual string getName() {
			return this.fileLocator.getName();
		}


		/// <summary>
		/// Everything but the last component of the URL representing this SMB
		/// resource is effectively it's parent. The root URL <code>smb://</code>
		/// does not have a parent. In this case <code>smb://</code> is returned.
		/// </summary>
		/// <returns> The parent directory of this SMB resource or
		///         <code>smb://</code> if the resource refers to the root of the URL
		///         hierarchy which incidentally is also <code>smb://</code>. </returns>
		public virtual string getParent() {
			return this.fileLocator.getParent();
		}


		/// <summary>
		/// Returns the full uncanonicalized URL of this SMB resource. An
		/// <code>SmbFile</code> constructed with the result of this method will
		/// result in an <code>SmbFile</code> that is equal to the original.
		/// </summary>
		/// <returns> The uncanonicalized full URL of this SMB resource. </returns>

		public virtual string getPath() {
			return this.fileLocator.getPath();
		}


		/// <summary>
		/// Returns the Windows UNC style path with backslashes instead of forward slashes.
		/// </summary>
		/// <returns> The UNC path. </returns>
		public virtual string getCanonicalUncPath() {
			return this.fileLocator.getCanonicalURL();
		}


		/// <summary>
		/// If the path of this <code>SmbFile</code> falls within a DFS volume,
		/// this method will return the referral path to which it maps. Otherwise
		/// <code>null</code> is returned.
		/// </summary>
		/// <returns> URL to the DFS volume </returns>
		/// <exception cref="SmbException"> </exception>
		/// throws SmbException
		public virtual string getDfsPath() {
			try {
				string path = this.treeConnection.ensureDFSResolved(this.fileLocator).getDfsPath();
				if (path != null && isDirectory()) {
					path += '/';
				}
				return path;
			}
			catch (CIFSException e) {
				throw SmbException.wrap(e);
			}
		}


		/// <summary>
		/// Returns the full URL of this SMB resource with '.' and '..' components
		/// factored out. An <code>SmbFile</code> constructed with the result of
		/// this method will result in an <code>SmbFile</code> that is equal to
		/// the original.
		/// </summary>
		/// <returns> The canonicalized URL of this SMB resource. </returns>
		public virtual string getCanonicalPath() {
			return this.fileLocator.getCanonicalURL();
		}


		/// <summary>
		/// Retrieves the share associated with this SMB resource. In
		/// the case of <code>smb://</code>, <code>smb://workgroup/</code>,
		/// and <code>smb://server/</code> URLs which do not specify a share,
		/// <code>null</code> will be returned.
		/// </summary>
		/// <returns> The share component or <code>null</code> if there is no share </returns>
		public virtual string getShare() {
			return this.fileLocator.getShare();
		}


		/// <summary>
		/// Retrieve the hostname of the server for this SMB resource. If the resources has been resolved by DFS this will
		/// return the target name.
		/// </summary>
		/// <returns> The server name </returns>
		public virtual string getServerWithDfs() {
			return this.fileLocator.getServerWithDfs();
		}


		/// <summary>
		/// Retrieve the hostname of the server for this SMB resource. If this
		/// <code>SmbFile</code> references a workgroup, the name of the workgroup
		/// is returned. If this <code>SmbFile</code> refers to the root of this
		/// SMB network hierarchy, <code>null</code> is returned.
		/// </summary>
		/// <returns> The server or workgroup name or <code>null</code> if this
		///         <code>SmbFile</code> refers to the root <code>smb://</code> resource. </returns>
		public virtual string getServer() {
			return this.fileLocator.getServer();
		}


		/// throws jcifs.CIFSException
		public virtual SmbWatchHandle watch(int filter, bool recursive) {

			if (filter == 0) {
				throw new System.ArgumentException("filter must not be 0");
			}

			if (!isDirectory()) {
				throw new SmbException("Is not a directory");
			}

			using (SmbTreeHandleImpl th = ensureTreeConnected()) {
				if (!th.isSMB2() && !th.hasCapability(SmbConstants.CAP_NT_SMBS)) {
					throw new SmbUnsupportedOperationException("Not supported without CAP_NT_SMBS");
				}
				return new SmbWatchHandleImpl(openUnshared(SmbConstants.O_RDONLY, SmbConstants.READ_CONTROL | SmbConstants.GENERIC_READ, SmbConstants.DEFAULT_SHARING, 0, 1), filter, recursive);
			}
		}


		/// throws SmbException
		public virtual bool canRead() {
			if (getType() == SmbConstants.TYPE_NAMED_PIPE) {
				return true;
			}
			return exists(); // try opening and catch sharing violation?
		}


		/// throws SmbException
		public virtual bool canWrite() {
			if (getType() == SmbConstants.TYPE_NAMED_PIPE) {
				return true;
			}
			return exists() && (this.attributesValue & SmbConstants.ATTR_READONLY) == 0;
		}


		/// throws SmbException
		public virtual bool isDirectory() {
			if (this.fileLocator.isRootOrShare()) {
				return true;
			}
			if (!exists()) {
				return false;
			}
			return (this.attributesValue & SmbConstants.ATTR_DIRECTORY) == SmbConstants.ATTR_DIRECTORY;
		}


		/// throws SmbException
		public virtual bool isFile() {
			if (this.fileLocator.isRootOrShare()) {
				return false;
			}
			exists();
			return (this.attributesValue & SmbConstants.ATTR_DIRECTORY) == 0;
		}


		/// throws SmbException
		public virtual bool isHidden() {
			if (this.fileLocator.getShare()==null) {
				return false;
			}
			else if (this.fileLocator.isRootOrShare()) {
				if (this.fileLocator.getShare().EndsWith("$", StringComparison.Ordinal)) {
					return true;
				}
				return false;
			}
			exists();
			return (this.attributesValue & SmbConstants.ATTR_HIDDEN) == SmbConstants.ATTR_HIDDEN;
		}


		/// throws SmbException
		public virtual long createTime() {
			if (!this.fileLocator.isRootOrShare()) {
				exists();
				return this.createTimeValue;
			}
			return 0L;
		}


		/// throws SmbException
		public virtual long lastModified() {
			if (!this.fileLocator.isRootOrShare()) {
				exists();
				return this.lastModifiedValue;
			}
			return 0L;
		}


		/// throws SmbException
		public virtual long lastAccess() {
			if (!this.fileLocator.isRootOrShare()) {
				exists();
				return this.lastAccessValue;
			}
			return 0L;
		}


		/// <summary>
		/// List the contents of this SMB resource. The list returned by this
		/// method will be;
		/// 
		/// <ul>
		/// <li>files and directories contained within this resource if the
		/// resource is a normal disk file directory,
		/// <li>all available NetBIOS workgroups or domains if this resource is
		/// the top level URL <code>smb://</code>,
		/// <li>all servers registered as members of a NetBIOS workgroup if this
		/// resource refers to a workgroup in a <code>smb://workgroup/</code> URL,
		/// <li>all browseable shares of a server including printers, IPC
		/// services, or disk volumes if this resource is a server URL in the form
		/// <code>smb://server/</code>,
		/// <li>or <code>null</code> if the resource cannot be resolved.
		/// </ul>
		/// </summary>
		/// <returns> A <code>String[]</code> array of files and directories,
		///         workgroups, servers, or shares depending on the context of the
		///         resource URL </returns>
		/// <exception cref="SmbException"> </exception>
		/// throws SmbException
		public virtual string[] list() {
			return SmbEnumerationUtil.list(this, "*", SmbConstants.ATTR_DIRECTORY | SmbConstants.ATTR_HIDDEN | SmbConstants.ATTR_SYSTEM, null, null);
		}


		/// <summary>
		/// List the contents of this SMB resource. The list returned will be
		/// identical to the list returned by the parameterless <code>list()</code>
		/// method minus filenames filtered by the specified filter.
		/// </summary>
		/// <param name="filter">
		///            a filename filter to exclude filenames from the results </param>
		/// <returns> <code>String[]</code> array of matching files and directories,
		///         workgroups, servers, or shares depending on the context of the
		///         resource URL </returns>
		/// <exception cref="SmbException"> </exception>
		///             # <returns> An array of filenames </returns>
		/// throws SmbException
		public virtual string[] list(SmbFilenameFilter filter) {
			return SmbEnumerationUtil.list(this, "*", SmbConstants.ATTR_DIRECTORY | SmbConstants.ATTR_HIDDEN | SmbConstants.ATTR_SYSTEM, filter, null);
		}


		/// <summary>
		/// List the contents of this SMB resource as an array of
		/// <code>SmbResource</code> objects. This method is much more efficient than
		/// the regular <code>list</code> method when querying attributes of each
		/// file in the result set.
		/// <para>
		/// The list of <code>SmbResource</code>s returned by this method will be;
		/// 
		/// <ul>
		/// <li>files and directories contained within this resource if the
		/// resource is a normal disk file directory,
		/// <li>all available NetBIOS workgroups or domains if this resource is
		/// the top level URL <code>smb://</code>,
		/// <li>all servers registered as members of a NetBIOS workgroup if this
		/// resource refers to a workgroup in a <code>smb://workgroup/</code> URL,
		/// <li>all browseable shares of a server including printers, IPC
		/// services, or disk volumes if this resource is a server URL in the form
		/// <code>smb://server/</code>,
		/// <li>or <code>null</code> if the resource cannot be resolved.
		/// </ul>
		/// 
		/// If strict resource lifecycle is used, make sure you close the individual files after use.
		/// 
		/// </para>
		/// </summary>
		/// <returns> An array of <code>SmbResource</code> objects representing file
		///         and directories, workgroups, servers, or shares depending on the context
		///         of the resource URL </returns>
		/// <exception cref="SmbException"> </exception>
		/// throws SmbException
		public virtual SmbFile[] listFiles() {
			return SmbEnumerationUtil.listFiles(this, "*", SmbConstants.ATTR_DIRECTORY | SmbConstants.ATTR_HIDDEN | SmbConstants.ATTR_SYSTEM, null, null);
		}


		/// <summary>
		/// The CIFS protocol provides for DOS "wildcards" to be used as
		/// a performance enhancement. The client does not have to filter
		/// the names and the server does not have to return all directory
		/// entries.
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
		/// <blockquote>
		/// 
		/// <pre>
		/// winnt&gt; ls c?o*
		/// clock.avi                  -rw--      82944 Mon Oct 14 1996 1:38 AM
		/// Cookies                    drw--          0 Fri Nov 13 1998 9:42 PM
		/// 2 items in 5ms
		/// </pre>
		/// 
		/// </blockquote>
		/// 
		/// If strict resource lifecycle is used, make sure you close the individual files after use.
		/// 
		/// </para>
		/// </summary>
		/// <param name="wildcard">
		///            a wildcard expression </param>
		/// <exception cref="SmbException"> </exception>
		/// <returns> An array of <code>SmbResource</code> objects representing file
		///         and directories, workgroups, servers, or shares depending on the context
		///         of the resource URL </returns>
		/// throws SmbException
		public virtual SmbFile[] listFiles(string wildcard) {
			return SmbEnumerationUtil.listFiles(this, wildcard, SmbConstants.ATTR_DIRECTORY | SmbConstants.ATTR_HIDDEN | SmbConstants.ATTR_SYSTEM, null, null);
		}


		/// <summary>
		/// List the contents of this SMB resource. The list returned will be
		/// identical to the list returned by the parameterless <code>listFiles()</code>
		/// method minus files filtered by the specified filename filter.
		/// 
		/// If strict resource lifecycle is used, make sure you close the individual files after use.
		/// </summary>
		/// <param name="filter">
		///            a filter to exclude files from the results </param>
		/// <returns> An array of <tt>SmbResource</tt> objects </returns>
		/// <exception cref="SmbException"> </exception>
		/// throws SmbException
		public virtual SmbFile[] listFiles(SmbFilenameFilter filter) {
			return SmbEnumerationUtil.listFiles(this, "*", SmbConstants.ATTR_DIRECTORY | SmbConstants.ATTR_HIDDEN | SmbConstants.ATTR_SYSTEM, filter, null);
		}


		/// <summary>
		/// List the contents of this SMB resource. The list returned will be
		/// identical to the list returned by the parameterless <code>listFiles()</code>
		/// method minus filenames filtered by the specified filter.
		/// 
		/// If strict resource lifecycle is used, make sure you close the individual files after use.
		/// </summary>
		/// <param name="filter">
		///            a file filter to exclude files from the results </param>
		/// <returns> An array of <tt>SmbResource</tt> objects </returns>
		/// <exception cref="SmbException"> </exception>
		/// throws SmbException
		public virtual SmbFile[] listFiles(SmbFileFilter filter) {
			return SmbEnumerationUtil.listFiles(this, "*", SmbConstants.ATTR_DIRECTORY | SmbConstants.ATTR_HIDDEN | SmbConstants.ATTR_SYSTEM, null, filter);
		}


		/// throws jcifs.CIFSException
		public virtual CloseableIterator<SmbResource> children() {
			return SmbEnumerationUtil.doEnum(this, "*", SmbConstants.ATTR_DIRECTORY | SmbConstants.ATTR_HIDDEN | SmbConstants.ATTR_SYSTEM, null, null);
		}


		/// throws jcifs.CIFSException
		public virtual CloseableIterator<SmbResource> children(string wildcard) {
			return SmbEnumerationUtil.doEnum(this, wildcard, SmbConstants.ATTR_DIRECTORY | SmbConstants.ATTR_HIDDEN | SmbConstants.ATTR_SYSTEM, null, null);
		}


		/// throws jcifs.CIFSException
		public virtual CloseableIterator<SmbResource> children(ResourceNameFilter filter) {
			return SmbEnumerationUtil.doEnum(this, "*", SmbConstants.ATTR_DIRECTORY | SmbConstants.ATTR_HIDDEN | SmbConstants.ATTR_SYSTEM, filter, null);
		}


		/// throws jcifs.CIFSException
		public virtual CloseableIterator<SmbResource> children(ResourceFilter filter) {
			return SmbEnumerationUtil.doEnum(this, "*", SmbConstants.ATTR_DIRECTORY | SmbConstants.ATTR_HIDDEN | SmbConstants.ATTR_SYSTEM, null, filter);
		}


		/// throws SmbException
		public virtual void renameTo(SmbResource d) {
			renameTo(d, false);
		}


		/// throws SmbException
		public virtual void renameTo(SmbResource d, bool replace) {
			if (!(d is SmbFile)) {
				throw new SmbException("Invalid target resource");
			}
			SmbFile dest = (SmbFile) d;
			try {
					using (SmbTreeHandleImpl sh = ensureTreeConnected())
					using (	SmbTreeHandleImpl th = dest.ensureTreeConnected()) {
        
					// this still might be required for standalone DFS
					if (!exists()) {
						throw new SmbException(NtStatus.NT_STATUS_OBJECT_NAME_NOT_FOUND, null);
					}
					dest.exists();
        
					if (this.fileLocator.isRootOrShare() || dest.fileLocator.isRootOrShare()) {
						throw new SmbException("Invalid operation for workgroups, servers, or shares");
					}
        
					if (!sh.isSameTree(th)) {
						// trigger requests to resolve the actual target
						exists();
						dest.exists();
        
						if (!Equals(getServerWithDfs(), dest.getServerWithDfs()) || !Equals(getShare(), dest.getShare())) {
							throw new SmbException("Cannot rename between different trees");
						}
					}
        
					if (log.isDebugEnabled()) {
						log.debug("renameTo: " + getUncPath() + " -> " + dest.getUncPath());
					}
        
					dest.attrExpiration = dest.sizeExpiration = 0;
					/*
					 * Rename Request / Response
					 */
					if (sh.isSMB2()) {
						Smb2SetInfoRequest req = new Smb2SetInfoRequest(sh.getConfig());
						req.setFileInformation(new FileRenameInformation2(dest.getUncPath().Substring(1), replace));
						withOpen(sh, Smb2CreateRequest.FILE_OPEN, SmbConstants.FILE_WRITE_ATTRIBUTES | SmbConstants.DELETE, SmbConstants.FILE_SHARE_READ | SmbConstants.FILE_SHARE_WRITE, req);
					}
					else {
						if (replace) {
							// TRANS2_SET_FILE_INFORMATION does not seem to support the SMB1 RENAME_INFO
							throw new SmbUnsupportedOperationException("Replacing rename only supported with SMB2");
						}
						sh.send(new SmbComRename(sh.getConfig(), getUncPath(), dest.getUncPath()), new SmbComBlankResponse(sh.getConfig()));
					}
        
					this.attrExpiration = this.sizeExpiration = 0;
					}
			}
			catch (CIFSException e) {
				throw SmbException.wrap(e);
			}
		}


		/// throws jcifs.CIFSException
		internal virtual void copyRecursive(SmbFile dest, byte[][] b, int bsize, WriterThread w, SmbTreeHandleImpl sh, SmbTreeHandleImpl dh) {
			if (isDirectory()) {
				SmbCopyUtil.copyDir(this, dest, b, bsize, w, sh, dh);
			}
			else {
				SmbCopyUtil.copyFile(this, dest, b, bsize, w, sh, dh);
			}

			dest.clearAttributeCache();
		}


		/// 
		internal virtual void clearAttributeCache() {
			this.attrExpiration = 0;
			this.sizeExpiration = 0;
		}


		/// throws SmbException
		public virtual void copyTo(SmbResource d) {
			if (!(d is SmbFile)) {
				throw new SmbException("Invalid target resource");
			}
			SmbFile dest = (SmbFile) d;
			try {
					using (SmbTreeHandleImpl sh = ensureTreeConnected())
						using(SmbTreeHandleImpl dh = dest.ensureTreeConnected()) {
					if (!exists()) {
						throw new SmbException(NtStatus.NT_STATUS_OBJECT_NAME_NOT_FOUND, null);
					}
        
					/*
					 * Should be able to copy an entire share actually
					 */
					if (this.fileLocator.getShare()==null || dest.getLocator().getShare()==null) {
						throw new SmbException("Invalid operation for workgroups or servers");
					}
        
					/*
					 * It is invalid for the source path to be a child of the destination
					 * path or visa versa.
					 */
					if (this.fileLocator.overlaps(dest.getLocator())) {
						throw new SmbException("Source and destination paths overlap.");
					}
        
					WriterThread w = new WriterThread();
					w.setDaemon(true);
        
					try {
						w.Start();
						// use commonly acceptable buffer size
						int bsize = Math.Min(sh.getReceiveBufferSize() - 70, dh.getSendBufferSize() - 70);
						byte[][] b = {new byte[bsize], new byte[bsize]};
						copyRecursive(dest, b, bsize, w, sh, dh);
					}
					finally {
						w.write(null, -1, null);
						w.Interrupt();
						try {
							w.Join();
						}
						catch (ThreadInterruptedException e) {
							log.warn("Interrupted while joining copy thread", e);
						}
					}
					}
			}
			catch (CIFSException e) {
				throw SmbException.wrap(e);
			}
		}


		/// throws SmbException
		public virtual void delete() {
			try {
				delete(this.fileLocator.getUNCPath());
			}
			catch (CIFSException e) {
				throw SmbException.wrap(e);
			}
			Dispose();
		}


		/// throws jcifs.CIFSException
		internal virtual void delete(string fileName) {
			if (this.fileLocator.isRootOrShare()) {
				throw new SmbException("Invalid operation for workgroups, servers, or shares");
			}

			using (SmbTreeHandleImpl th = ensureTreeConnected()) {
				if (!exists()) {
					throw new SmbException(NtStatus.NT_STATUS_OBJECT_NAME_NOT_FOUND, null);
				}

				if ((this.attributesValue & SmbConstants.ATTR_READONLY) != 0) {
					setReadWrite();
				}

				/*
				 * Delete or Delete Directory Request / Response
				 */

				if (log.isDebugEnabled()) {
					log.debug("delete: " + fileName);
				}

				if ((this.attributesValue & SmbConstants.ATTR_DIRECTORY) != 0) {

					/*
					 * Recursively delete directory contents
					 */

					try {
							using (CloseableIterator<SmbResource> it = SmbEnumerationUtil.doEnum(this, "*", SmbConstants.ATTR_DIRECTORY | SmbConstants.ATTR_HIDDEN | SmbConstants.ATTR_SYSTEM, null, null)) {
							while (it.hasNext()) {
								using (SmbResource r = it.next()) {
									try {
										r.delete();
									}
									catch (CIFSException e) {
										throw SmbException.wrap(e);
									}
								}
							}
							}
					}
					catch (SmbException se) {
						/*
						 * Oracle FilesOnline version 9.0.4 doesn't send '.' and '..' so
						 * listFiles may generate undesirable "cannot find
						 * the file specified".
						 */
						log.debug("delete", se);
						if (se.getNtStatus() != NtStatus.NT_STATUS_NO_SUCH_FILE) {
							throw se;
						}
					}

					if (th.isSMB2()) {
						Smb2CreateRequest req = new Smb2CreateRequest(th.getConfig(), fileName);
						req.setDesiredAccess(0x10000); // delete
						req.setCreateOptions(Smb2CreateRequest.FILE_DELETE_ON_CLOSE | Smb2CreateRequest.FILE_DIRECTORY_FILE);
						req.setCreateDisposition(Smb2CreateRequest.FILE_OPEN);
						req.chain(new Smb2CloseRequest(th.getConfig(), fileName));
						th.send(req);
					}
					else {
						th.send(new SmbComDeleteDirectory(th.getConfig(), fileName), new SmbComBlankResponse(th.getConfig()));
					}
				}
				else {

					if (th.isSMB2()) {
						Smb2CreateRequest req = new Smb2CreateRequest(th.getConfig(), fileName.Substring(1));
						req.setDesiredAccess(0x10000); // delete
						req.setCreateOptions(Smb2CreateRequest.FILE_DELETE_ON_CLOSE);
						req.chain(new Smb2CloseRequest(th.getConfig(), fileName));
						th.send(req);
					}
					else {
						th.send(new SmbComDelete(th.getConfig(), fileName), new SmbComBlankResponse(th.getConfig()));
					}
				}
				this.attrExpiration = this.sizeExpiration = 0;
			}

		}


		/// throws SmbException
		public virtual long length() {
			if (this.sizeExpiration > DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()) {
				return this.size;
			}

			try {
					using (SmbTreeHandleImpl th = ensureTreeConnected()) {
					int t = getType();
					if (t == SmbConstants.TYPE_SHARE) {
						this.size = fetchAllocationInfo(th).getCapacity();
					}
					else if (!this.fileLocator.isRoot() && t != SmbConstants.TYPE_NAMED_PIPE) {
						queryPath(th, this.fileLocator.getUNCPath(), FileInformationConstants.FILE_STANDARD_INFO);
					}
					else {
						this.size = 0L;
					}
					this.sizeExpiration = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + getContext().getConfig().getAttributeCacheTimeout();
					return this.size;
					}
			}
			catch (CIFSException e) {
				throw SmbException.wrap(e);
			}
		}


		/// throws SmbException
		public virtual long getDiskFreeSpace() {
			try {
					using (SmbTreeHandleImpl th = ensureTreeConnected()) {
					int t = getType();
					if (t == SmbConstants.TYPE_SHARE || t == SmbConstants.TYPE_FILESYSTEM) {
						AllocInfo allocInfo = fetchAllocationInfo(th);
						this.size = allocInfo.getCapacity();
						this.sizeExpiration = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + getContext().getConfig().getAttributeCacheTimeout();
						return allocInfo.getFree();
					}
					return 0L;
					}
			}
			catch (CIFSException e) {
				throw SmbException.wrap(e);
			}
		}


		/// <summary>
		/// @return </summary>
		/// <exception cref="CIFSException"> </exception>
		/// <exception cref="SmbException"> </exception>
		/// throws CIFSException, SmbException
		private AllocInfo fetchAllocationInfo(SmbTreeHandleImpl th) {
			AllocInfo ai;
			try {
				ai = queryFSInformation<AllocInfo>(th, typeof(AllocInfo), FileSystemInformationConstants.FS_SIZE_INFO);
			}
			catch (SmbException ex) {
				log.debug("getDiskFreeSpace", ex);
				switch (ex.getNtStatus()) {
				case NtStatus.NT_STATUS_INVALID_INFO_CLASS:
				case NtStatus.NT_STATUS_UNSUCCESSFUL: // NetApp Filer
					if (!th.isSMB2()) {
						// SMB_FS_FULL_SIZE_INFORMATION not supported by the server.
						ai = queryFSInformation<AllocInfo>(th, typeof(AllocInfo), FileSystemInformationConstants.SMB_INFO_ALLOCATION);
						break;
					}
					throw ex;
				default:
					throw ex;
				}
			}
			return ai;
		}


		/// throws jcifs.CIFSException
		private T queryFSInformation<T>(SmbTreeHandleImpl th, Type clazz, byte level) where T : FileSystemInformation {
			if (th.isSMB2()) {
				Smb2QueryInfoRequest qreq = new Smb2QueryInfoRequest(th.getConfig());
				qreq.setFilesystemInfoClass(level);
				Smb2QueryInfoResponse resp = withOpen<Smb2QueryInfoResponse>(th, Smb2CreateRequest.FILE_OPEN, SmbConstants.FILE_READ_ATTRIBUTES, (SmbConstants.FILE_SHARE_READ | SmbConstants.FILE_SHARE_WRITE), qreq);
				return resp.getInfo<T>(clazz);
			}
			Trans2QueryFSInformationResponse response = new Trans2QueryFSInformationResponse(th.getConfig(), level);
			th.send(new Trans2QueryFSInformation(th.getConfig(), level), response);
			return response.getInfo<T>(clazz);
		}


		/// throws SmbException
		public virtual void mkdir() {
			string path = this.fileLocator.getUNCPath();

			if (path.Length == 1) {
				throw new SmbException("Invalid operation for workgroups, servers, or shares");
			}

			try {
					using (SmbTreeHandleImpl th = ensureTreeConnected()) {
					// should not normally be required, but samba without NTStatus does not properly resolve the path and fails
					// with
					// STATUS_UNSUCCESSFUL
					exists();
					// get the path again, this may have changed through DFS referrals
					path = this.fileLocator.getUNCPath();
        
					/*
					 * Create Directory Request / Response
					 */
        
					if (log.isDebugEnabled()) {
						log.debug("mkdir: " + path);
					}
        
					if (th.isSMB2()) {
						Smb2CreateRequest req = new Smb2CreateRequest(th.getConfig(), path);
						req.setCreateDisposition(Smb2CreateRequest.FILE_CREATE);
						req.setCreateOptions(Smb2CreateRequest.FILE_DIRECTORY_FILE);
						req.chain(new Smb2CloseRequest(th.getConfig(), path));
						th.send(req);
					}
					else {
						th.send(new SmbComCreateDirectory(th.getConfig(), path), new SmbComBlankResponse(th.getConfig()));
					}
					this.attrExpiration = this.sizeExpiration = 0;
					}
			}
			catch (CIFSException e) {
				throw SmbException.wrap(e);
			}
		}


		/// throws SmbException
		public virtual void mkdirs() {
			string p = this.fileLocator.getParent();
			try {
					using (SmbTreeHandle th = ensureTreeConnected())
						using(SmbFile parent = new SmbFile(p, getContext())) {
					try {
						if (!parent.exists()) {
							if (log.isDebugEnabled()) {
								log.debug("Parent does not exist " + p);
							}
							parent.mkdirs();
						}
					}
					catch (SmbException e) {
						if (log.isDebugEnabled()) {
							log.debug("Failed to ensure parent exists " + p, e);
						}
						throw e;
					}
					try {
						mkdir();
					}
					catch (SmbException e) {
						log.debug("mkdirs", e);
						// Ignore "Cannot create a file when that file already exists." errors for now as
						// they seem to be show up under some conditions most likely due to timing issues.
						if (e.getNtStatus() != NtStatus.NT_STATUS_OBJECT_NAME_COLLISION) {
							throw e;
						}
					}
					}
			}
			catch (CIFSException e) {
				throw SmbException.wrap(e);
			}
			catch (UriFormatException e) {
				throw new SmbException("Invalid URL in mkdirs", e);
			}
		}


		/// throws jcifs.CIFSException
	
		protected internal virtual T withOpen<T>(SmbTreeHandleImpl th, ServerMessageBlock2Request<T> first, params ServerMessageBlock2[] others) where T : ServerMessageBlock2Response {
			return withOpen<T>(th, Smb2CreateRequest.FILE_OPEN, 0x00120089, SmbConstants.FILE_SHARE_READ | SmbConstants.FILE_SHARE_WRITE, first, others);
		}


		/// throws jcifs.CIFSException

		protected internal virtual T withOpen<T>(SmbTreeHandleImpl th, int createDisposition, int desiredAccess, int shareAccess, ServerMessageBlock2Request<T> first, params ServerMessageBlock2[] others) where T : ServerMessageBlock2Response {
			return withOpen(th, createDisposition, 0, SmbConstants.ATTR_NORMAL, desiredAccess, shareAccess, first, others);
		}


		/// throws jcifs.CIFSException
		
		//TODO 1 type, ServerMessageBlock2 is not ServerMessageBlock2Request<T>,but who is???
		protected internal virtual T withOpen<T>(SmbTreeHandleImpl th, int createDisposition, int createOptions, int fileAttributes, int desiredAccess, int shareAccess, ServerMessageBlock2Request<T> first, params ServerMessageBlock2[] others) where T : ServerMessageBlock2Response {
			Smb2CreateRequest cr = new Smb2CreateRequest(th.getConfig(), getUncPath());
			try {
				cr.setCreateDisposition(createDisposition);
				cr.setCreateOptions(createOptions);
				cr.setFileAttributes(fileAttributes);
				cr.setDesiredAccess(desiredAccess);
				cr.setShareAccess(shareAccess);

				//TODO 
		//TODO type  jcifs.internal.smb2.ServerMessageBlock2Request<?> cur = cr;
				//TODO 1 type
				ServerMessageBlock2 cur = cr;

				if (first != null) {
					cr.chain(first);
					cur = first;

		//TODO type  for (jcifs.internal.smb2.ServerMessageBlock2Request<?> req : others)
					//TODO 1 type
					foreach (ServerMessageBlock2 req in others) {
						cur.chain(req);
						cur = req;
					}
				}

				Smb2CloseRequest closeReq = new Smb2CloseRequest(th.getConfig(), getUncPath());
				closeReq.setCloseFlags(Smb2CloseResponse.SMB2_CLOSE_FLAG_POSTQUERY_ATTIB);
				cur.chain(closeReq);

				Smb2CreateResponse createResp = th.send(cr);

				Smb2CloseResponse closeResp = (Smb2CloseResponse)closeReq.getResponse();
				SmbBasicFileInfo info;

				if ((closeResp.getCloseFlags() & Smb2CloseResponse.SMB2_CLOSE_FLAG_POSTQUERY_ATTIB) != 0) {
					info = closeResp;
				}
				else {
					info = createResp;
				}

				this.isExists = true;
				this.createTimeValue = info.getCreateTime();
				this.lastModifiedValue = info.getLastWriteTime();
				this.lastAccessValue = info.getLastAccessTime();
				this.attributesValue = info.getAttributes() & ATTR_GET_MASK;
				this.attrExpiration = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + th.getConfig().getAttributeCacheTimeout();

				this.size = info.getSize();
				this.sizeExpiration = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + th.getConfig().getAttributeCacheTimeout();
				return (T) createResp.getNextResponse();
			}
			catch (Exception e) when (e is CIFSException || e is Exception) {
				try {
					// make sure that the handle is closed when one of the requests fails
					Smb2CreateResponse createResp = (Smb2CreateResponse)cr.getResponse();
					if (createResp.isReceived() && createResp.getStatus() == NtStatus.NT_STATUS_OK) {
						th.send(new Smb2CloseRequest(th.getConfig(), createResp.getFileId()), RequestParam.NO_RETRY);
					}
				}
				catch (Exception e2) {
					log.debug("Failed to close after failure", e2);
					//e.addSuppressed(e2);
				}
				throw e;
			}
		}


		/// throws SmbException
		public virtual void createNewFile() {
			if (this.fileLocator.isRootOrShare()) {
				throw new SmbException("Invalid operation for workgroups, servers, or shares");
			}

			try {
					using (SmbTreeHandleImpl th = ensureTreeConnected()) {
        
					if (th.isSMB2()) {
						//TODO 1 return type
						withOpen<Smb2CloseResponse>(th, Smb2CreateRequest.FILE_OPEN_IF, SmbConstants.O_RDWR, 0, null);
					}
					else {
						using (SmbFileHandle fd = openUnshared(SmbConstants.O_RDWR | SmbConstants.O_CREAT | SmbConstants.O_EXCL, SmbConstants.O_RDWR, SmbConstants.FILE_NO_SHARE, SmbConstants.ATTR_NORMAL, 0)) {
							// close explicitly
							fd.close(0L);
						}
					}
					}
			}
			catch (CIFSException e) {
				throw SmbException.wrap(e);
			}
		}


		/// throws jcifs.CIFSException
		internal virtual void setPathInformation(int attrs, long ctime, long mtime, long atime) {
			using (SmbTreeHandleImpl th = ensureTreeConnected()) {
				if (!exists()) {
					throw new SmbException(NtStatus.NT_STATUS_OBJECT_NAME_NOT_FOUND, null);
				}

				int dir = this.attributesValue & SmbConstants.ATTR_DIRECTORY;

				if (th.isSMB2()) {

					Smb2SetInfoRequest req = new Smb2SetInfoRequest(th.getConfig());
					req.setFileInformation(new FileBasicInfo(ctime, atime, mtime, 0L, attrs | dir));
					withOpen(th, Smb2CreateRequest.FILE_OPEN, SmbConstants.FILE_WRITE_ATTRIBUTES, SmbConstants.FILE_SHARE_READ | SmbConstants.FILE_SHARE_WRITE, req);
				}
				else if (th.hasCapability(SmbConstants.CAP_NT_SMBS)) {

					using (SmbFileHandleImpl f = openUnshared(SmbConstants.O_RDONLY, SmbConstants.FILE_WRITE_ATTRIBUTES, SmbConstants.FILE_SHARE_READ | SmbConstants.FILE_SHARE_WRITE, dir, dir != 0 ? 0x0001 : 0x0040)) {
						th.send(new Trans2SetFileInformation(th.getConfig(), f.getFid(), attrs | dir, ctime, mtime, atime), new Trans2SetFileInformationResponse(th.getConfig()), RequestParam.NO_RETRY);
					}
				}
				else {
					if (ctime != 0 || atime != 0) {
						throw new SmbUnsupportedOperationException("Cannot set creation or access time without CAP_NT_SMBS");
					}
					th.send(new SmbComSetInformation(th.getConfig(), getUncPath(), attrs, mtime - th.getServerTimeZoneOffset()), new SmbComSetInformationResponse(th.getConfig()));
				}

				this.attrExpiration = 0;
			}
		}


		/// throws SmbException
		public virtual void setFileTimes(long createTime, long lastLastModified, long lastLastAccess) {
			if (this.fileLocator.isRootOrShare()) {
				throw new SmbException("Invalid operation for workgroups, servers, or shares");
			}

			try {
				setPathInformation(0, createTime, lastLastModified, lastLastAccess);
			}
			catch (CIFSException e) {
				throw SmbException.wrap(e);
			}
		}


		/// throws SmbException
		public virtual void setCreateTime(long time) {
			if (this.fileLocator.isRootOrShare()) {
				throw new SmbException("Invalid operation for workgroups, servers, or shares");
			}

			try {
				setPathInformation(0, time, 0L, 0L);
			}
			catch (CIFSException e) {
				throw SmbException.wrap(e);
			}
		}


		/// throws SmbException
		public virtual void setLastModified(long time) {
			if (this.fileLocator.isRootOrShare()) {
				throw new SmbException("Invalid operation for workgroups, servers, or shares");
			}

			try {
				setPathInformation(0, 0L, time, 0L);
			}
			catch (CIFSException e) {
				throw SmbException.wrap(e);
			}
		}


		/// throws SmbException
		public virtual void setLastAccess(long time) {
			if (this.fileLocator.isRootOrShare()) {
				throw new SmbException("Invalid operation for workgroups, servers, or shares");
			}

			try {
				setPathInformation(0, 0L, 0L, time);
			}
			catch (CIFSException e) {
				throw SmbException.wrap(e);
			}
		}


		/// throws SmbException
		public virtual int getAttributes() {
			if (this.fileLocator.isRootOrShare()) {
				return 0;
			}
			exists();
			return this.attributesValue & ATTR_GET_MASK;
		}


		/// throws SmbException
		public virtual void setAttributes(int attrs) {
			if (this.fileLocator.isRootOrShare()) {
				throw new SmbException("Invalid operation for workgroups, servers, or shares");
			}
			try {
				setPathInformation(attrs & ATTR_SET_MASK, 0L, 0L, 0L);
			}
			catch (SmbException e) {
				if (e.getNtStatus() != unchecked((int)0xC00000BB)) {
					throw e;
				}
				throw new SmbUnsupportedOperationException("Attribute not supported by server");
			}
			catch (CIFSException e) {
				throw SmbException.wrap(e);
			}
		}


		/// throws SmbException
		public virtual void setReadOnly() {
			setAttributes(getAttributes() | SmbConstants.ATTR_READONLY);
		}


		/// throws SmbException
		public virtual void setReadWrite() {
			setAttributes(getAttributes() & ~SmbConstants.ATTR_READONLY);
		}


		/// <summary>
		/// Returns a <seealso cref="java.net.URL"/> for this <code>SmbFile</code>. The
		/// <code>URL</code> may be used as any other <code>URL</code> might to
		/// access an SMB resource. Currently only retrieving data and information
		/// is supported (i.e. no <tt>doOutput</tt>).
		/// </summary>
		/// @deprecated Use getURL() instead 
		/// <returns> A new <code><seealso cref="java.net.URL"/></code> for this <code>SmbFile</code> </returns>
		[Obsolete("Use getURL() instead")]
		public virtual URL toURL() {
			return getURL();
		}


		/// <summary>
		/// Computes a hashCode for this file based on the URL string and IP
		/// address if the server. The hashing function uses the hashcode of the
		/// server address, the canonical representation of the URL, and does not
		/// compare authentication information. In essence, two
		/// <code>SmbFile</code> objects that refer to
		/// the same file should generate the same hashcode provided it is possible
		/// to make such a determination.
		/// </summary>
		/// <returns> A hashcode for this abstract file </returns>
		public override int GetHashCode() {
			return this.fileLocator.GetHashCode();
		}


		/// <summary>
		/// Tests to see if two <code>SmbFile</code> objects are equal. Two
		/// SmbFile objects are equal when they reference the same SMB
		/// resource. More specifically, two <code>SmbFile</code> objects are
		/// equals if their server IP addresses are equal and the canonicalized
		/// representation of their URLs, minus authentication parameters, are
		/// case insensitively and lexographically equal.
		/// <br>
		/// For example, assuming the server <code>angus</code> resolves to the
		/// <code>192.168.1.15</code> IP address, the below URLs would result in
		/// <code>SmbFile</code>s that are equal.
		/// 
		/// <para>
		/// <blockquote>
		/// 
		/// <pre>
		/// smb://192.168.1.15/share/DIR/foo.txt
		/// smb://angus/share/data/../dir/foo.txt
		/// </pre>
		/// 
		/// </blockquote>
		/// 
		/// </para>
		/// </summary>
		/// <param name="obj">
		///            Another <code>SmbFile</code> object to compare for equality </param>
		/// <returns> <code>true</code> if the two objects refer to the same SMB resource
		///         and <code>false</code> otherwise </returns>

		public override bool Equals(object obj) {
			if (obj is SmbFile) {
				SmbResource f = (SmbResource) obj;

				if (this == f) {
					return true;
				}

				return this.fileLocator.Equals(f.getLocator());
			}

			return false;
		}


		/// <summary>
		/// Returns the string representation of this SmbFile object. This will
		/// be the same as the URL used to construct this <code>SmbFile</code>.
		/// This method will return the same value
		/// as <code>getPath</code>.
		/// </summary>
		/// <returns> The original URL representation of this SMB resource </returns>

		public override string ToString() {
			return this.url.ToString();
		}


		/* URLConnection implementation */
		/// <summary>
		/// This URLConnection method just returns the result of <tt>length()</tt>.
		/// </summary>
		/// <returns> the length of this file or 0 if it refers to a directory </returns>
		[Obsolete]
		public  int getContentLength() {
			try {
				return unchecked((int)(length() & 0xFFFFFFFFL));
			}
			catch (SmbException se) {
				log.debug("getContentLength", se);
			}
			return 0;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= java.net.URLConnection#getContentLengthLong() </seealso>
		public  long getContentLengthLong() {
			try {
				return length();
			}
			catch (SmbException se) {
				log.debug("getContentLength", se);
			}
			return 0;
		}


		/// <summary>
		/// This URLConnection method just returns the result of <tt>lastModified</tt>.
		/// </summary>
		/// <returns> the last modified data as milliseconds since Jan 1, 1970 </returns>
		public  long getDate() {
			try {
				return lastModified();
			}
			catch (SmbException se) {
				log.debug("getDate", se);
			}
			return 0L;
		}


		/// <summary>
		/// This URLConnection method just returns the result of <tt>lastModified</tt>.
		/// </summary>
		/// <returns> the last modified data as milliseconds since Jan 1, 1970 </returns>
		public  long getLastModified() {
			try {
				return lastModified();
			}
			catch (SmbException se) {
				log.debug("getLastModified", se);
			}
			return 0L;
		}


		/// <summary>
		/// This URLConnection method just returns a new <tt>SmbFileInputStream</tt> created with this file.
		/// </summary>
		/// <exception cref="IOException">
		///             thrown by <tt>SmbFileInputStream</tt> constructor </exception>
		/// throws java.io.IOException
		public override Stream openStreamForRead() {
			return new InputStreamToStream(new SmbFileInputStream(this));
		}


		/// throws SmbException
		public virtual InputStream openInputStream() {
			return new SmbFileInputStream(this);
		}


		/// throws SmbException
		public virtual InputStream openInputStream(int sharing) {
			return openInputStream(0, SmbConstants.O_RDONLY, sharing);
		}


		/// throws SmbException
		public virtual InputStream openInputStream(int flags, int access, int sharing) {
			return new SmbFileInputStream(this, flags, access, sharing, false);
		}


		/// throws java.io.IOException
		public override Stream openStreamForWrite() {
			return new OutputStreamToStream(new SmbFileOutputStream(this));
		}


		/// throws SmbException
		public virtual OutputStream openOutputStream() {
			return new SmbFileOutputStream(this);
		}


		/// throws SmbException
		public virtual OutputStream openOutputStream(bool append) {
			return openOutputStream(append, SmbConstants.FILE_SHARE_READ);
		}


		/// throws SmbException
		public virtual OutputStream openOutputStream(bool append, int sharing) {
			return openOutputStream(append, append ? SmbConstants.O_CREAT | SmbConstants.O_WRONLY | SmbConstants.O_APPEND : SmbConstants.O_CREAT | SmbConstants.O_WRONLY | SmbConstants.O_TRUNC, 0, sharing);
		}


		/// throws SmbException
		public virtual OutputStream openOutputStream(bool append, int openFlags, int access, int sharing) {
			return new SmbFileOutputStream(this, append, openFlags, access, sharing);
		}


		/// throws SmbException
		public virtual SmbRandomAccess openRandomAccess(string mode) {
			return new SmbRandomAccessFile(this, mode);
		}


		/// throws SmbException
		public virtual SmbRandomAccess openRandomAccess(string mode, int sharing) {
			return new SmbRandomAccessFile(this, mode, sharing, false);
		}


		/// throws java.io.IOException
		private void processAces(ACE[] aces, bool resolveSids) {
			string server = this.fileLocator.getServerWithDfs();
			int ai;

			if (resolveSids) {
				SID[] sids = new SID[aces.Length];
				for (ai = 0; ai < aces.Length; ai++) {
					sids[ai] =(SID) aces[ai].getSID();
				}

				for (int off = 0; off < sids.Length; off += 64) {
					int len = sids.Length - off;
					if (len > 64) {
						len = 64;
					}

					getContext().getSIDResolver().resolveSids(getContext(), server, sids, off, len);
				}
			}
			else {
				for (ai = 0; ai < aces.Length; ai++) {
					((SID)aces[ai].getSID()).initContext(server, getContext());
				}
			}
		}


		/// throws SmbException
		public virtual long fileIndex() {

			try {
					using (SmbTreeHandleImpl th = ensureTreeConnected()) {
        
					if (th.isSMB2()) {
						Smb2QueryInfoRequest req = new Smb2QueryInfoRequest(th.getConfig());
						req.setFileInfoClass(FileInformationConstants.FILE_INTERNAL_INFO);
						Smb2QueryInfoResponse resp = withOpen(th, Smb2CreateRequest.FILE_OPEN, SmbConstants.FILE_READ_ATTRIBUTES, SmbConstants.FILE_SHARE_READ | SmbConstants.FILE_SHARE_WRITE, req);
						FileInternalInfo info = resp.getInfo<FileInternalInfo>(typeof(FileInternalInfo));
						return info.getIndexNumber();
					}
					}
			}
			catch (CIFSException e) {
				throw SmbException.wrap(e);
			}

			return 0;
		}


		/// throws jcifs.CIFSException
		internal virtual SecurityDescriptor querySecurity(SmbTreeHandleImpl th, int types) {
			if (th.isSMB2()) {
				Smb2QueryInfoRequest req = new Smb2QueryInfoRequest(th.getConfig());
				req.setInfoType(Smb2Constants.SMB2_0_INFO_SECURITY);
				req.setAdditionalInformation(types);
				Smb2QueryInfoResponse resp = withOpen(th, Smb2CreateRequest.FILE_OPEN, SmbConstants.FILE_READ_ATTRIBUTES | SmbConstants.READ_CONTROL, SmbConstants.FILE_SHARE_READ | SmbConstants.FILE_SHARE_WRITE, req);
				return resp.getInfo<SecurityDescriptor>(typeof(SecurityDescriptor));
			}

			if (!th.hasCapability(SmbConstants.CAP_NT_SMBS)) {
				throw new SmbUnsupportedOperationException("Not supported without CAP_NT_SMBS/SMB2");
			}
			NtTransQuerySecurityDescResponse response = new NtTransQuerySecurityDescResponse(getContext().getConfig());

			using (SmbFileHandleImpl f = openUnshared(SmbConstants.O_RDONLY, SmbConstants.READ_CONTROL, SmbConstants.DEFAULT_SHARING, 0, isDirectory() ? 1 : 0)) {
				/*
				 * NtTrans Query Security Desc Request / Response
				 */
				NtTransQuerySecurityDesc request = new NtTransQuerySecurityDesc(getContext().getConfig(), f.getFid(), types);
				response = th.send(request, response, RequestParam.NO_RETRY);
				return response.getSecurityDescriptor();
			}
		}


		/// throws java.io.IOException
		public virtual ACE[] getSecurity() {
			return getSecurity(false);
		}


		/// throws java.io.IOException
		public virtual ACE[] getSecurity(bool resolveSids) {
			using (SmbTreeHandleImpl th = ensureTreeConnected()) {
				SecurityDescriptor desc = querySecurity(th, SecurityInfoConstants.DACL_SECURITY_INFO);
				ACE[] aces = desc.getAces();
				if (aces != null) {
					processAces(aces, resolveSids);
				}

				return aces;
			}
		}


		/// throws java.io.IOException
		public virtual jcifs.SID getOwnerUser() {
			return getOwnerUser(true);
		}


		/// throws java.io.IOException
		public virtual jcifs.SID getOwnerUser(bool resolve) {
			using (SmbTreeHandleImpl th = ensureTreeConnected()) {
				SecurityDescriptor desc = querySecurity(th, SecurityInfoConstants.OWNER_SECURITY_INFO);
				SID ownerUser = (SID)desc.getOwnerUserSid();
				if (ownerUser == null) {
					return null;
				}

				string server = this.fileLocator.getServerWithDfs();
				if (resolve) {
					try {
						ownerUser.resolve(server, getContext());
					}
					catch (IOException e) {
						log.warn("Failed to resolve SID " + ownerUser.ToString(), e);
					}
				}
				else {
					ownerUser.initContext(server, getContext());
				}
				return ownerUser;
			}
		}


		/// throws java.io.IOException
		public virtual jcifs.SID getOwnerGroup() {
			return getOwnerGroup(true);
		}


		/// throws java.io.IOException
		public virtual jcifs.SID getOwnerGroup(bool resolve) {
			using (SmbTreeHandleImpl th = ensureTreeConnected()) {
				SecurityDescriptor desc = querySecurity(th, SecurityInfoConstants.GROUP_SECURITY_INFO);
				SID ownerGroup = (SID)desc.getOwnerGroupSid();
				if (ownerGroup == null) {
					return null;
				}

				string server = this.fileLocator.getServerWithDfs();
				if (resolve) {
					try {
						ownerGroup.resolve(server, getContext());
					}
					catch (IOException e) {
						log.warn("Failed to resolve SID " + ownerGroup.ToString(), e);
					}
				}
				else {
					ownerGroup.initContext(server, getContext());
				}
				return ownerGroup;
			}
		}


		/// throws java.io.IOException
		public virtual ACE[] getShareSecurity(bool resolveSids) {
			using (SmbTreeHandleInternal th = ensureTreeConnected()) {
				string server = this.fileLocator.getServerWithDfs();
				ACE[] aces;
				MsrpcShareGetInfo rpc = new MsrpcShareGetInfo(server, th.getConnectedShare());
				using (DcerpcHandle handle = DcerpcHandle.getHandle("ncacn_np:" + server + "[\\PIPE\\srvsvc]", getContext())) {
					handle.sendrecv(rpc);
					if (rpc.retval != 0) {
						throw new SmbException(rpc.retval, true);
					}
					aces = rpc.getSecurity();
					if (aces != null) {
						processAces(aces, resolveSids);
					}
				}
				return aces;
			}
		}

	}

}