using System;
using Logger = org.slf4j.Logger;
using LoggerFactory = org.slf4j.LoggerFactory;
using CIFSException = jcifs.CIFSException;
using jcifs;
using ResourceNameFilter = jcifs.ResourceNameFilter;
using SmbConstants = jcifs.SmbConstants;
using SmbResource = jcifs.SmbResource;
using SmbResourceLocator = jcifs.SmbResourceLocator;
using NetServerEnum2 = jcifs.@internal.smb1.net.NetServerEnum2;
using NetServerEnum2Response = jcifs.@internal.smb1.net.NetServerEnum2Response;
using SmbComTransaction = jcifs.@internal.smb1.trans.SmbComTransaction;

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

	/// 
	public class NetServerEnumIterator : CloseableIterator<FileEntry> {

		private static readonly Logger log = LoggerFactory.getLogger(typeof(NetServerEnumIterator));

		private readonly NetServerEnum2 request;
		private readonly NetServerEnum2Response response;
		private readonly SmbResource parent;
		private readonly SmbTreeHandleImpl treeHandle;
		private readonly ResourceNameFilter nameFilter;
		private readonly bool workgroup;
		private int ridx;
		private FileEntry nextField;


		/// <param name="parent"> </param>
		/// <param name="th"> </param>
		/// <param name="wildcard"> </param>
		/// <param name="searchAttributes"> </param>
		/// <param name="filter"> </param>
		/// <exception cref="CIFSException">
		///  </exception>
		/// throws jcifs.CIFSException
		public NetServerEnumIterator(SmbFile parent, SmbTreeHandleImpl th, string wildcard, int searchAttributes, ResourceNameFilter filter) {
			this.parent = parent;
			this.nameFilter = filter;
			SmbResourceLocator locator = parent.getLocator();
			this.workgroup = locator.getType() == SmbConstants.TYPE_WORKGROUP;
			if (string.IsNullOrEmpty(locator.getURL().Host)) {
				this.request = new NetServerEnum2(th.getConfig(), th.getOEMDomainName(), NetServerEnum2.SV_TYPE_DOMAIN_ENUM);
				this.response = new NetServerEnum2Response(th.getConfig());
			}
			else if (this.workgroup) {
				this.request = new NetServerEnum2(th.getConfig(), locator.getURL().Host, NetServerEnum2.SV_TYPE_ALL);
				this.response = new NetServerEnum2Response(th.getConfig());
			}
			else {
				throw new SmbException("The requested list operations is invalid: " + locator.getURL());
			}

			this.treeHandle = th.acquire();
			try {
				this.nextField = open();
			}
			catch (Exception e) {
				this.treeHandle.release();
				throw e;
			}

		}


		/// throws jcifs.CIFSException
		private FileEntry open() {
			this.treeHandle.send(this.request, this.response);
			checkStatus();
			FileEntry n = advance();
			if (n == null) {
				doClose();
			}
			return n;
		}


		/// <exception cref="SmbException"> </exception>
		/// throws SmbException
		private void checkStatus() {
			int status = this.response.getStatus();
			if (status == WinError.ERROR_SERVICE_NOT_INSTALLED) {
				throw new SmbUnsupportedOperationException();
			}
			if (status != WinError.ERROR_SUCCESS && status != WinError.ERROR_MORE_DATA) {
				throw new SmbException(status, true);
			}
		}


		/// throws jcifs.CIFSException
		private FileEntry advance() {
			int n = this.response.getStatus() == WinError.ERROR_MORE_DATA ? this.response.getNumEntries() - 1 : this.response.getNumEntries();
			while (this.ridx < n) {
				FileEntry itm = this.response.getResults()[this.ridx];
				this.ridx++;
				if (filter(itm)) {
					return itm;
				}
			}

			if (this.workgroup && this.response.getStatus() == WinError.ERROR_MORE_DATA) {
				this.request.reset(0, this.response.getLastName());
				this.response.reset();
				this.request.setSubCommand(SmbComTransaction.NET_SERVER_ENUM3);
				this.treeHandle.send(this.request, this.response);
				checkStatus();
				this.ridx = 0;
				return advance();
			}
			return null;
		}


		private bool filter(FileEntry fe) {
			string name = fe.getName();
			if (this.nameFilter == null) {
				return true;
			}
			try {
				if (!this.nameFilter.accept(this.parent, name)) {
					return false;
				}
				return true;
			}
			catch (CIFSException e) {
				log.error("Failed to apply name filter", e);
				return false;
			}
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= java.util.Iterator#hasNext() </seealso>
		public  bool hasNext() {
			return this.nextField != null;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= java.util.Iterator#next() </seealso>
		public  FileEntry next() {
			FileEntry n = this.nextField;
			try {
				FileEntry ne = advance();
				if (ne == null) {
					doClose();
					return n;
				}
				this.nextField = ne;
			}
			catch (CIFSException e) {
				log.warn("Enumeration failed", e);
				this.nextField = null;
			}
			return n;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.CloseableIterator#Dispose() </seealso>
		/// throws jcifs.CIFSException
		public virtual void Dispose() {
			if (this.nextField != null) {
				doClose();
			}
		}


		/// 
		private void doClose() {
			this.treeHandle.release();
			this.nextField = null;
		}


		public  void remove() {
			throw new System.NotSupportedException("remove");
		}
	}
}