using System.Collections.Generic;
using jcifs.@internal;
using jcifs.@internal.smb1.trans.nt;
using jcifs.@internal.smb2.notify;
using org.slf4j;

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
namespace jcifs.smb
{
    /// <summary>
    /// @author mbechler
    /// 
    /// </summary>
    internal class SmbWatchHandleImpl : SmbWatchHandle
    {
        private static readonly Logger log = LoggerFactory.getLogger(typeof(SmbWatchHandleImpl));

        private readonly SmbFileHandleImpl handle;
        private readonly int filter;
        private readonly bool recursive;


        /// <param name="fh"> </param>
        /// <param name="filter"> </param>
        /// <param name="recursive">
        ///  </param>
        public SmbWatchHandleImpl(SmbFileHandleImpl fh, int filter, bool recursive)
        {
            this.handle = fh;
            this.filter = filter;
            this.recursive = recursive;
        }


        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        /// <seealso cref= jcifs.SmbWatchHandle#watch() </seealso>
		/// throws jcifs.CIFSException
        public virtual IList<FileNotifyInformation> watch()
        {
            if (!this.handle.isValid())
            {
                throw new SmbException("Watch was broken by tree disconnect");
            }

            using (SmbTreeHandleImpl th = (SmbTreeHandleImpl)this.handle.getTree())
            {
                CommonServerMessageBlockRequest req;
                NotifyResponse resp = null;
                if (th.isSMB2())
                {
                    Smb2ChangeNotifyRequest r = new Smb2ChangeNotifyRequest(th.getConfig(), this.handle.getFileId());
                    r.setCompletionFilter(this.filter);
                    r.setNotifyFlags(this.recursive ? Smb2ChangeNotifyRequest.SMB2_WATCH_TREE : 0);
                    req = r;
                }
                else
                {
                    if (!th.hasCapability(SmbConstants.CAP_NT_SMBS))
                    {
                        throw new SmbUnsupportedOperationException("Not supported without CAP_NT_SMBS");
                    }

                    /*
                     * NtTrans Notify Change Request / Response
                     */
                    req = new NtTransNotifyChange(th.getConfig(), this.handle.getFid(), this.filter, this.recursive);
                    resp = new NtTransNotifyChangeResponse(th.getConfig());
                }

                if (log.isTraceEnabled())
                {
                    log.trace("Sending NtTransNotifyChange for " + this.handle);
                }

                try
                {
                    resp = th.send(req, resp, RequestParam.NO_TIMEOUT, RequestParam.NO_RETRY);
                }
                catch (SmbException e)
                {
                    if (e.getNtStatus() == unchecked((int) 0xC0000120))
                    {
                        // cancelled
                        log.debug("Request was cancelled", e);
                        return null;
                    }

                    throw e;
                }

                if (log.isTraceEnabled())
                {
                    log.trace("Returned from NtTransNotifyChange " + resp.getErrorCode());
                }

                if (!resp.isReceived())
                {
                    throw new CIFSException("Did not receive response");
                }

                if (resp.getErrorCode() == 0x10B)
                {
                    this.handle.markClosed();
                }

                if (resp.getErrorCode() == 0x10C)
                {
                    resp.getNotifyInformation().Clear();
                }

                return resp.getNotifyInformation();
            }
        }

        public IList<FileNotifyInformation> call()
        {
           return watch();
        }


        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        /// <seealso cref= jcifs.SmbWatchHandle#call() </seealso>
		/// throws jcifs.CIFSException

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        /// <seealso cref= jcifs.SmbWatchHandle#Dispose() </seealso>
		/// throws jcifs.CIFSException
        public virtual void Dispose()
        {
            if (this.handle.isValid())
            {
                this.handle.close(0L);
            }
        }
    }
}