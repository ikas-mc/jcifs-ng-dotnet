using Logger = org.slf4j.Logger;
using LoggerFactory = org.slf4j.LoggerFactory;
using CIFSException = jcifs.CIFSException;
using ResourceNameFilter = jcifs.ResourceNameFilter;
using SmbConstants = jcifs.SmbConstants;
using SmbResource = jcifs.SmbResource;
using Smb2CloseRequest = jcifs.@internal.smb2.create.Smb2CloseRequest;
using Smb2CreateRequest = jcifs.@internal.smb2.create.Smb2CreateRequest;
using Smb2CreateResponse = jcifs.@internal.smb2.create.Smb2CreateResponse;
using Smb2QueryDirectoryRequest = jcifs.@internal.smb2.info.Smb2QueryDirectoryRequest;
using Smb2QueryDirectoryResponse = jcifs.@internal.smb2.info.Smb2QueryDirectoryResponse;

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
	public class DirFileEntryEnumIterator2 : DirFileEntryEnumIteratorBase {

		private static readonly Logger log = LoggerFactory.getLogger(typeof(DirFileEntryEnumIterator2));

		private byte[] fileId;
		private Smb2QueryDirectoryResponse response;


		//TODO 1 public
		/// <param name="th"> </param>
		/// <param name="parent"> </param>
		/// <param name="wildcard"> </param>
		/// <param name="filter"> </param>
		/// <param name="searchAttributes"> </param>
		/// <exception cref="CIFSException"> </exception>
		/// throws jcifs.CIFSException
		public DirFileEntryEnumIterator2(SmbTreeHandleImpl th, SmbResource parent, string wildcard, ResourceNameFilter filter, int searchAttributes) : base(th, parent, wildcard, filter, searchAttributes) {
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.smb.DirFileEntryEnumIteratorBase#getResults() </seealso>
		protected internal override FileEntry[] getResults() {
			FileEntry[] results = this.response.getResults();
			if (results == null) {
				return new FileEntry[0];
			}
			return results;
		}


		/// <param name="th"> </param>
		/// <param name="parent"> </param>
		/// <param name="wildcard">
		/// @return </param>
		/// <exception cref="CIFSException"> </exception>
		/// throws jcifs.CIFSException
		protected internal override FileEntry open() {
			SmbTreeHandleImpl th = getTreeHandle();
			string uncPath = getParent().getLocator().getUNCPath();
			Smb2CreateRequest create = new Smb2CreateRequest(th.getConfig(), uncPath);
			create.setCreateOptions(Smb2CreateRequest.FILE_DIRECTORY_FILE);
			create.setDesiredAccess(SmbConstants.FILE_READ_DATA | SmbConstants.FILE_READ_ATTRIBUTES);
			Smb2QueryDirectoryRequest query = new Smb2QueryDirectoryRequest(th.getConfig());
			query.setFileName(getWildcard());
			create.chain(query);
			Smb2CreateResponse createResp;
			try {
				createResp = th.send(create);
			}
			catch (SmbException e) {
				Smb2CreateResponse cr = (Smb2CreateResponse)create.getResponse();
				if (cr != null && cr.isReceived() && cr.getStatus() == NtStatus.NT_STATUS_OK) {
					try {
						th.send(new Smb2CloseRequest(th.getConfig(), cr.getFileId()));
					}
					catch (SmbException e2) {
						if (log.isDebugEnabled()) {
							log.error(e2.Message,e2);
						}
						//TODO 
						//e.addSuppressed(e2);
					}
				}

				Smb2QueryDirectoryResponse qr = (Smb2QueryDirectoryResponse)query.getResponse();

				if (qr != null && qr.isReceived() && qr.getStatus() == NtStatus.NT_STATUS_NO_SUCH_FILE) {
					// this simply indicates an empty listing
					doClose();
					return null;
				}

				throw e;
			}
			this.fileId = createResp.getFileId();
			this.response = (Smb2QueryDirectoryResponse)query.getResponse();
			FileEntry n = advance(false);
			if (n == null) {
				doClose();
			}
			return n;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.smb.DirFileEntryEnumIteratorBase#fetchMore() </seealso>
		/// throws jcifs.CIFSException
		protected internal override bool fetchMore() {
			FileEntry[] results = this.response.getResults();
			SmbTreeHandleImpl th = getTreeHandle();
			Smb2QueryDirectoryRequest query = new Smb2QueryDirectoryRequest(th.getConfig(), this.fileId);
			query.setFileName(this.getWildcard());
			query.setFileIndex(results[results.Length - 1].getFileIndex());
			query.setQueryFlags(Smb2QueryDirectoryRequest.SMB2_INDEX_SPECIFIED);
			try {
				Smb2QueryDirectoryResponse r = th.send(query);
				if (r.getStatus() == NtStatus.NT_STATUS_NO_MORE_FILES) {
					return false;
				}
				this.response = r;
			}
			catch (SmbException e) {
				if (e.getNtStatus() == NtStatus.NT_STATUS_NO_MORE_FILES) {
					log.debug("End of listing", e);
					return false;
				}
				throw e;
			}
			return true;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.smb.DirFileEntryEnumIteratorBase#isDone() </seealso>
		protected internal override bool isDone() {
			return false;
		}


		/// <exception cref="CIFSException"> </exception>
		/// throws jcifs.CIFSException
		protected internal override void doCloseInternal() {
			try {
				SmbTreeHandleImpl th = getTreeHandle();
				if (this.fileId != null && th.isConnected()) {
					th.send(new Smb2CloseRequest(th.getConfig(), this.fileId));
				}
			}
			finally {
				this.fileId = null;
			}
		}

	}

}