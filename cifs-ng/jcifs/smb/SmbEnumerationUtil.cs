using jcifs.smb;

using System.Collections.Generic;
using System.IO;
using cifs_ng.lib;
using cifs_ng.lib.ext;
using cifs_ng.lib.socket;
using Logger = org.slf4j.Logger;
using LoggerFactory = org.slf4j.LoggerFactory;
using Address = jcifs.Address;
using CIFSContext = jcifs.CIFSContext;
using CIFSException = jcifs.CIFSException;
using jcifs;
using ResourceFilter = jcifs.ResourceFilter;
using ResourceNameFilter = jcifs.ResourceNameFilter;
using SmbConstants = jcifs.SmbConstants;
using SmbResource = jcifs.SmbResource;
using SmbResourceLocator = jcifs.SmbResourceLocator;
using DcerpcException = jcifs.dcerpc.DcerpcException;
using DcerpcHandle = jcifs.dcerpc.DcerpcHandle;
using MsrpcDfsRootEnum = jcifs.dcerpc.msrpc.MsrpcDfsRootEnum;
using MsrpcShareEnum = jcifs.dcerpc.msrpc.MsrpcShareEnum;
using NetShareEnum = jcifs.@internal.smb1.net.NetShareEnum;
using NetShareEnumResponse = jcifs.@internal.smb1.net.NetShareEnumResponse;
using SmbComTransaction = jcifs.@internal.smb1.trans.SmbComTransaction;
using SmbComTransactionResponse = jcifs.@internal.smb1.trans.SmbComTransactionResponse;

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
	/// 
	/// </summary>
	internal sealed class SmbEnumerationUtil {

		private static readonly Logger log = LoggerFactory.getLogger(typeof(SmbEnumerationUtil));


		/// 
		private SmbEnumerationUtil() {
		}


		/// throws MalformedURLException, jcifs.dcerpc.DcerpcException
		private static DcerpcHandle getHandle(CIFSContext ctx, SmbResourceLocator loc, Address address, string ep) {
			return DcerpcHandle.getHandle(string.Format("ncacn_np:{0}[endpoint={1},address={2}]", loc.getServer(), ep, address.getHostAddress()), ctx);
		}


		/// throws java.io.IOException
		internal static FileEntry[] doDfsRootEnum(CIFSContext ctx, SmbResourceLocator loc, Address address) {
			using (DcerpcHandle handle = getHandle(ctx, loc, address, "\\PIPE\\netdfs")) {
				MsrpcDfsRootEnum rpc = new MsrpcDfsRootEnum(loc.getServer());
				handle.sendrecv(rpc);
				if (rpc.retval != 0) {
					throw new SmbException(rpc.retval, true);
				}
				return rpc.getEntries();
			}
		}


		/// throws java.io.IOException
		internal static FileEntry[] doMsrpcShareEnum(CIFSContext ctx, SmbResourceLocator loc, Address address) {
			using (DcerpcHandle handle = getHandle(ctx, loc, address, "\\PIPE\\srvsvc")) {
				MsrpcShareEnum rpc = new MsrpcShareEnum(loc.getServer());
				handle.sendrecv(rpc);
				if (rpc.retval != 0) {
					throw new SmbException(rpc.retval, true);
				}
				return rpc.getEntries();
			}
		}


		/// throws jcifs.CIFSException
		internal static FileEntry[] doNetShareEnum(SmbTreeHandleImpl th) {
			SmbComTransaction req = new NetShareEnum(th.getConfig());
			SmbComTransactionResponse resp = new NetShareEnumResponse(th.getConfig());
			th.send(req, resp);
			if (resp.getStatus() != WinError.ERROR_SUCCESS) {
				throw new SmbException(resp.getStatus(), true);
			}

			return resp.getResults();
		}


		/// throws jcifs.CIFSException
		internal static CloseableIterator<SmbResource> doShareEnum(SmbFile parent, string wildcard, int searchAttributes, ResourceNameFilter fnf, ResourceFilter ff) {
			// clone the locator so that the address index is not modified
			SmbResourceLocatorImpl locator = (SmbResourceLocatorImpl)parent.fileLocator.Clone();
			CIFSContext tc = parent.getContext();
			URL u = locator.getURL();

			FileEntry[] entries;

			if (u.getPath().LastIndexOf('/') != (u.getPath().Length - 1)) {
				throw new SmbException(u.ToString() + " directory must end with '/'");
			}

			if (locator.getType() != SmbConstants.TYPE_SERVER) {
				throw new SmbException("The requested list operations is invalid: " + u.ToString());
			}

			ISet<FileEntry> set = new HashSet<FileEntry>();

			if (tc.getDfs().isTrustedDomain(tc, locator.getServer())) {
				/*
				 * The server name is actually the name of a trusted
				 * domain. Add DFS roots to the list.
				 */
				try {
					entries = doDfsRootEnum(tc, locator, locator.getAddress());
					for (int ei = 0; ei < entries.Length; ei++) {
						FileEntry e = entries[ei];
						if (!set.Contains(e) && (fnf == null || fnf.accept(parent, e.getName()))) {
							set.Add(e);
						}
					}
				}
				catch (IOException ioe) {
					log.debug("DS enumeration failed", ioe);
				}
			}

			SmbTreeConnection treeConn = SmbTreeConnection.create(tc);
			try {
					using (SmbTreeHandleImpl th = treeConn.connectHost(locator, locator.getServerWithDfs()))
					using (	SmbSessionImpl session = (SmbSessionImpl)th.getSession())
					using (	SmbTransportImpl transport = (SmbTransportImpl)session.getTransport()) {
					try {
						entries = doMsrpcShareEnum(tc, locator, transport.getRemoteAddress());
					}
					catch (IOException ioe) {
						if (th.isSMB2()) {
							throw ioe;
						}
						log.debug("doMsrpcShareEnum failed", ioe);
						entries = doNetShareEnum(th);
					}
					for (int ei = 0; ei < entries.Length; ei++) {
						FileEntry e = entries[ei];
						if (!set.Contains(e) && (fnf == null || fnf.accept(parent, e.getName()))) {
							set.Add(e);
						}
					}
        
					}
			}
			catch (SmbException e) {
				throw e;
			}
			catch (IOException ioe) {
				log.debug("doNetShareEnum failed", ioe);
				throw new SmbException(u.ToString(), ioe);
			}
			return new ShareEnumIterator(parent, set.GetEnumerator(), ff);
		}


		/// throws jcifs.CIFSException
		internal static CloseableIterator<SmbResource> doEnum(SmbFile parent, string wildcard, int searchAttributes, ResourceNameFilter fnf, ResourceFilter ff) {
			DosFileFilter dff = unwrapDOSFilter(ff);
			if (dff != null) {
				if (dff.wildcard!=null) {
					wildcard = dff.wildcard;
				}
				searchAttributes = dff.attributes;
			}
			SmbResourceLocator locator = parent.getLocator();
			if (string.IsNullOrEmpty( locator.getURL().Host)) {
				// smb:// -> enumerate servers through browsing
				Address addr;
				try {
					addr = locator.getAddress();
				}
				catch (CIFSException e) {
					if (e.InnerException is UnknownHostException) {
						log.debug("Failed to find master browser", e);
						throw new SmbUnsupportedOperationException();
					}
					throw e;
				}
				using (SmbFile browser = (SmbFile) parent.resolve(addr.getHostAddress())) {
					using (SmbTreeHandleImpl th = browser.ensureTreeConnected()) {
						if (th.isSMB2()) {
							throw new SmbUnsupportedOperationException();
						}
						return new NetServerFileEntryAdapterIterator(parent, new NetServerEnumIterator(parent, th, wildcard, searchAttributes, fnf), ff);
					}
				}
			}
			else if (locator.getType() == SmbConstants.TYPE_WORKGROUP) {
				using (SmbTreeHandleImpl th = parent.ensureTreeConnected()) {
					if (th.isSMB2()) {
						throw new SmbUnsupportedOperationException();
					}
					return new NetServerFileEntryAdapterIterator(parent, new NetServerEnumIterator(parent, th, wildcard, searchAttributes, fnf), ff);
				}
			}
			else if (locator.isRoot()) {
				return doShareEnum(parent, wildcard, searchAttributes, fnf, ff);
			}

			using (SmbTreeHandleImpl th = parent.ensureTreeConnected()) {
				if (th.isSMB2()) {
					return new DirFileEntryAdapterIterator(parent, new DirFileEntryEnumIterator2(th, parent, wildcard, fnf, searchAttributes), ff);
				}
				return new DirFileEntryAdapterIterator(parent, new DirFileEntryEnumIterator1(th, parent, wildcard, fnf, searchAttributes), ff);
			}
		}


		private static DosFileFilter unwrapDOSFilter(ResourceFilter ff) {
			if (ff is ResourceFilterWrapper) {
				SmbFileFilter sff = ((ResourceFilterWrapper) ff).getFileFilter();
				if (sff is DosFileFilter) {
					return (DosFileFilter) sff;
				}
			}
			return null;
		}


		/// throws SmbException
		internal static string[] list(SmbFile root, string wildcard, int searchAttributes, in SmbFilenameFilter fnf, in SmbFileFilter ff) {
			try
			{
				using (CloseableIterator<SmbResource> it = doEnum(root, wildcard, searchAttributes, fnf == null ? null : new ResourceNameFilterAnonymousInnerClass(fnf), ff == null ? null : new ResourceFilterAnonymousInnerClass(ff)))
				{

					IList<string> list = new List<string>();
					while (it.hasNext())
					{
						using (SmbResource n = it.next())
						{
							list.Add(n.getName());
						}
					}

					return ((List<string>) list).ToArray();
				}
			}
			catch (CIFSException e)
			{
				throw SmbException.wrap(e);
			}
		}

		private class ResourceNameFilterAnonymousInnerClass : ResourceNameFilter {
			private SmbFilenameFilter fnf;

			public ResourceNameFilterAnonymousInnerClass(SmbFilenameFilter fnf) {
				this.fnf = fnf;
			}


		/// throws jcifs.CIFSException
			public bool accept(SmbResource parent, string name) {
				if (!(parent is SmbFile)) {
					return false;
				}
				return fnf.accept((SmbFile) parent, name);
			}
		}

		private class ResourceFilterAnonymousInnerClass : ResourceFilter {
			private SmbFileFilter ff;

			public ResourceFilterAnonymousInnerClass(SmbFileFilter ff) {
				this.ff = ff;
			}


		/// throws jcifs.CIFSException
			public bool accept(SmbResource resource) {
				if (!(resource is SmbFile)) {
					return false;
				}
				return ff.accept((SmbFile) resource);
			}
		}


		/// throws SmbException
		internal static SmbFile[] listFiles(SmbFile root, string wildcard, int searchAttributes, in SmbFilenameFilter fnf, in SmbFileFilter ff) {
			try {
					using (CloseableIterator<SmbResource> it = doEnum(root, wildcard, searchAttributes, fnf == null ? null : new ResourceNameFilterWrapper(fnf), ff == null ? null : new ResourceFilterWrapper(ff))) {
        
					IList<SmbFile> list = new List<SmbFile>();
					while (it.hasNext()) {
						using (SmbResource n = it.next()) {
							if (n is SmbFile) {
								list.Add((SmbFile) n);
							}
						}
					}
					return ((List<SmbFile>)list).ToArray();
					}
			}
			catch (CIFSException e) {
				throw SmbException.wrap(e);
			}
		}

		/// <summary>
		/// @author mbechler
		/// 
		/// </summary>
		private sealed class ResourceFilterWrapper : ResourceFilter {

			/// 
			internal readonly SmbFileFilter ff;


			/// <param name="ff"> </param>
			internal ResourceFilterWrapper(SmbFileFilter ff) {
				this.ff = ff;
			}


			internal SmbFileFilter getFileFilter() {
				return this.ff;
			}


		/// throws jcifs.CIFSException
			public bool accept(SmbResource resource) {
				if (!(resource is SmbFile)) {
					return false;
				}
				return this.ff.accept((SmbFile) resource);
			}
		}

		/// <summary>
		/// @author mbechler
		/// 
		/// </summary>
		private sealed class ResourceNameFilterWrapper : ResourceNameFilter {

			/// 
			internal readonly SmbFilenameFilter fnf;


			/// <param name="fnf"> </param>
			internal ResourceNameFilterWrapper(SmbFilenameFilter fnf) {
				this.fnf = fnf;
			}


		/// throws jcifs.CIFSException
			public bool accept(SmbResource parent, string name) {
				if (!(parent is SmbFile)) {
					return false;
				}
				return this.fnf.accept((SmbFile) parent, name);
			}
		}

	}

}