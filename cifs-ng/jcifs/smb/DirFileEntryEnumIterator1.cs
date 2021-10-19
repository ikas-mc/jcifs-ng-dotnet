using Logger = org.slf4j.Logger;
using LoggerFactory = org.slf4j.LoggerFactory;
using CIFSException = jcifs.CIFSException;
using ResourceNameFilter = jcifs.ResourceNameFilter;
using SmbResource = jcifs.SmbResource;
using SmbResourceLocator = jcifs.SmbResourceLocator;
using SmbComBlankResponse = jcifs.@internal.smb1.com.SmbComBlankResponse;
using SmbComFindClose2 = jcifs.@internal.smb1.com.SmbComFindClose2;
using SmbComTransaction = jcifs.@internal.smb1.trans.SmbComTransaction;
using Trans2FindFirst2 = jcifs.@internal.smb1.trans2.Trans2FindFirst2;
using Trans2FindFirst2Response = jcifs.@internal.smb1.trans2.Trans2FindFirst2Response;
using Trans2FindNext2 = jcifs.@internal.smb1.trans2.Trans2FindNext2;

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




	internal class DirFileEntryEnumIterator1 : DirFileEntryEnumIteratorBase {

		private static readonly Logger log = LoggerFactory.getLogger(typeof(DirFileEntryEnumIterator1));

		private Trans2FindNext2 nextRequest;
		private Trans2FindFirst2Response response;


		/// throws jcifs.CIFSException
		public DirFileEntryEnumIterator1(SmbTreeHandleImpl th, SmbResource parent, string wildcard, ResourceNameFilter filter, int searchAttributes) : base(th, parent, wildcard, filter, searchAttributes) {
		}


		/// throws jcifs.CIFSException
		protected internal override FileEntry open() {
			SmbResourceLocator loc = this.getParent().getLocator();
			string unc = loc.getUNCPath();
			string p = loc.getURL().getPath();
			if (p.LastIndexOf('/') != (p.Length - 1)) {
				throw new SmbException(loc.getURL() + " directory must end with '/'");
			}
			if (unc.LastIndexOf('\\') != (unc.Length - 1)) {
				throw new SmbException(unc + " UNC must end with '\\'");
			}

			SmbTreeHandleImpl th = getTreeHandle();
			this.response = new Trans2FindFirst2Response(th.getConfig());

			try {
				th.send(new Trans2FindFirst2(th.getConfig(), unc, this.getWildcard(), this.getSearchAttributes(), th.getConfig().getListCount(), th.getConfig().getListSize()), this.response);

				this.nextRequest = new Trans2FindNext2(th.getConfig(), this.response.getSid(), this.response.getResumeKey(), this.response.getLastName(), th.getConfig().getListCount(), th.getConfig().getListSize());
			}
			catch (SmbException e) {
				if (this.response != null && this.response.isReceived() && e.getNtStatus() == NtStatus.NT_STATUS_NO_SUCH_FILE) {
					doClose();
					return null;
				}
				throw e;
			}

			this.response.setSubCommand(SmbComTransaction.TRANS2_FIND_NEXT2);
			FileEntry n = advance(false);
			if (n == null) {
				doClose();
			}
			return n;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.smb.DirFileEntryEnumIteratorBase#getResults() </seealso>
		protected internal override FileEntry[] getResults() {
			return this.response.getResults();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <exception cref="CIFSException">
		/// </exception>
		/// <seealso cref= jcifs.smb.DirFileEntryEnumIteratorBase#fetchMore() </seealso>
		/// throws jcifs.CIFSException
		protected internal override bool fetchMore() {
			this.nextRequest.reset(this.response.getResumeKey(), this.response.getLastName());
			this.response.reset();
			try {
				getTreeHandle().send(this.nextRequest, this.response);
				return this.response.getStatus() != NtStatus.NT_STATUS_NO_MORE_FILES;
			}
			catch (SmbException e) {
				if (e.getNtStatus() == NtStatus.NT_STATUS_NO_MORE_FILES) {
					log.debug("No more entries", e);
					return false;
				}
				throw e;
			}
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.smb.DirFileEntryEnumIteratorBase#isDone() </seealso>
		protected internal override bool isDone() {
			return this.response.isEndOfSearch();
		}


		/// <exception cref="CIFSException"> </exception>
		/// throws jcifs.CIFSException
		protected internal override void doCloseInternal() {
			try {
				SmbTreeHandleImpl th = getTreeHandle();
				if (this.response != null) {
					th.send(new SmbComFindClose2(th.getConfig(), this.response.getSid()), new SmbComBlankResponse(th.getConfig()));
				}
			}
			catch (SmbException se) {
				log.debug("SmbComFindClose2 failed", se);
			}

		}

	}
}