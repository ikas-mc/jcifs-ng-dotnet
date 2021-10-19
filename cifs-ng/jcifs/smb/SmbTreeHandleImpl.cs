using System.Collections.Generic;
using cifs_ng.lib.threading;
using jcifs.@internal;
using jcifs.@internal.smb1.com;
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
    
    //TODO 1 public
    /// <summary>
    /// @author mbechler
    /// 
    /// </summary>
    public class SmbTreeHandleImpl : SmbTreeHandleInternal
    {
        private static readonly Logger log = LoggerFactory.getLogger(typeof(SmbTreeHandleImpl));

        private readonly SmbResourceLocatorImpl resourceLoc;
        private readonly SmbTreeConnection treeConnection;

        private readonly AtomicLong usageCount = new AtomicLong(1);


        /// <param name="resourceLoc"> </param>
        /// <param name="treeConnection"> </param>
        internal SmbTreeHandleImpl(SmbResourceLocatorImpl resourceLoc, SmbTreeConnection treeConnection)
        {
            this.resourceLoc = resourceLoc;
            this.treeConnection = treeConnection.acquire();
        }


        //TODO 
        public virtual SmbSession getSession()
        {
            return this.treeConnection.getSession();
        }


		/// throws jcifs.CIFSException
        public virtual void ensureDFSResolved()
        {
            this.treeConnection.ensureDFSResolved(this.resourceLoc);
        }


		/// throws SmbException
        public virtual bool hasCapability(int cap)
        {
            return this.treeConnection.hasCapability(cap);
        }


        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        /// <seealso cref= jcifs.SmbTreeHandle#isConnected() </seealso>
        public virtual bool isConnected()
        {
            return this.treeConnection.isConnected();
        }


        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        /// <seealso cref= jcifs.SmbTreeHandle#getConfig() </seealso>
        public virtual Configuration getConfig()
        {
            return this.treeConnection.getConfig();
        }


        /// <returns> the currently connected tree id </returns>
        public virtual long getTreeId()
        {
            return this.treeConnection.getTreeId();
        }


        /// 
        /// <param name="req"> </param>
        /// <param name="params"> </param>
        /// <returns> response </returns>
        /// <exception cref="CIFSException"> </exception>
		/// throws jcifs.CIFSException
        public virtual T send<T>(Request<T> req, params RequestParam[] @params) where T : CommonServerMessageBlockResponse
        {
            return send(req, default(T), @params);
        }


        /// <param name="request"> </param>
        /// <param name="response"> </param>
        /// <param name="params"> </param>
        /// <returns> response </returns>
        /// <exception cref="CIFSException"> </exception>
		/// throws jcifs.CIFSException
        public virtual T send<T>(CommonServerMessageBlockRequest request, T response, params RequestParam[] @params) where T : CommonServerMessageBlockResponse
        {
            return this.treeConnection.send(this.resourceLoc, request, response, @params);
        }


        /// 
        /// <param name="request"> </param>
        /// <param name="response"> </param>
        /// <param name="params"> </param>
        /// <returns> response </returns>
        /// <exception cref="CIFSException"> </exception>
		/// throws jcifs.CIFSException
        public virtual T send<T>(CommonServerMessageBlockRequest request, T response, ISet<RequestParam> @params) where T : CommonServerMessageBlockResponse
        {
            return this.treeConnection.send(this.resourceLoc, request, response, @params);
        }


        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        /// <seealso cref= jcifs.SmbTreeHandle#Dispose() </seealso>
        public virtual void Dispose()
        {
            lock (this)
            {
                release();
            }
        }


        /// <returns> tree handle with increased usage count </returns>
        public virtual SmbTreeHandleImpl acquire()
        {
            if (this.usageCount.IncrementValueAndReturn() == 1)
            {
                this.treeConnection.acquire();
            }

            return this;
        }


        public virtual void release()
        {
            long us = this.usageCount.DecrementValueAndReturn();
            if (us == 0)
            {
                this.treeConnection.release();
            }
            else if (us < 0)
            {
                throw new RuntimeCIFSException("Usage count dropped below zero");
            }
        }


        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        /// <seealso cref= java.lang.Object#finalize() </seealso>
		/// throws Throwable
        ~SmbTreeHandleImpl()
        {
            if (this.usageCount.Value != 0)
            {
                log.warn("Tree handle was not properly released " + this.resourceLoc.getURL());
            }
        }


        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        /// <seealso cref= jcifs.SmbTreeHandle#getRemoteHostName() </seealso>
        public virtual string getRemoteHostName()
        {
            using (SmbSessionImpl session = this.treeConnection.getSession())
            using (SmbTransportImpl transport = (SmbTransportImpl)session.getTransport())
            {
                return transport.getRemoteHostName();
            }
        }


        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        /// <exception cref="SmbException">
        /// </exception>
        /// <seealso cref= jcifs.SmbTreeHandle#getServerTimeZoneOffset() </seealso>
		/// throws SmbException
        public virtual long getServerTimeZoneOffset()
        {
            using (SmbSessionImpl session = this.treeConnection.getSession())
            using (SmbTransportImpl transport = (SmbTransportImpl)session.getTransport())
            {
                SmbNegotiationResponse nego = transport.getNegotiateResponse();
                if (nego is SmbComNegotiateResponse)
                {
                    return ((SmbComNegotiateResponse) nego).getServerData().serverTimeZone * 1000 * 60L;
                }

                return 0;
            }
        }


        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        /// <exception cref="SmbException">
        /// </exception>
        /// <seealso cref= jcifs.SmbTreeHandle#getOEMDomainName() </seealso>
		/// throws SmbException
        public virtual string getOEMDomainName()
        {
            using (SmbSessionImpl session = this.treeConnection.getSession())
            using (SmbTransportImpl transport = (SmbTransportImpl)session.getTransport())
            {
                SmbNegotiationResponse nego = transport.getNegotiateResponse();
                if (nego is SmbComNegotiateResponse)
                {
                    return ((SmbComNegotiateResponse) nego).getServerData().oemDomainName;
                }

                return null;
            }
        }


        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        /// <seealso cref= jcifs.SmbTreeHandle#getTreeType() </seealso>
        public virtual int getTreeType()
        {
            return this.treeConnection.getTreeType();
        }


        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        /// <seealso cref= jcifs.SmbTreeHandle#getConnectedShare() </seealso>
        public virtual string getConnectedShare()
        {
            return this.treeConnection.getConnectedShare();
        }


        /// 
        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        /// <seealso cref= jcifs.SmbTreeHandle#isSameTree(jcifs.SmbTreeHandle) </seealso>
        public virtual bool isSameTree(SmbTreeHandle th)
        {
            if (!(th is SmbTreeHandleImpl))
            {
                return false;
            }

            return this.treeConnection.isSame(((SmbTreeHandleImpl) th).treeConnection);
        }


		/// throws SmbException
        public virtual int getSendBufferSize()
        {
            using (SmbSessionImpl session = this.treeConnection.getSession())
            using (SmbTransportImpl transport = (SmbTransportImpl)session.getTransport())
            {
                return transport.getNegotiateResponse().getSendBufferSize();
            }
        }


		/// throws SmbException
        public virtual int getReceiveBufferSize()
        {
            using (SmbSessionImpl session = this.treeConnection.getSession())
            using (SmbTransportImpl transport = (SmbTransportImpl)session.getTransport())
            {
                return transport.getNegotiateResponse().getReceiveBufferSize();
            }
        }


		/// throws SmbException
        public virtual int getMaximumBufferSize()
        {
            using (SmbSessionImpl session = this.treeConnection.getSession())
            using (SmbTransportImpl transport =(SmbTransportImpl) session.getTransport())
            {
                return transport.getNegotiateResponse().getTransactionBufferSize();
            }
        }


		/// throws SmbException
        public virtual bool areSignaturesActive()
        {
            using (SmbSessionImpl session = this.treeConnection.getSession())
            using (SmbTransportImpl transport = (SmbTransportImpl)session.getTransport())
            {
                return transport.getNegotiateResponse().isSigningNegotiated();
            }
        }


        /// <returns> whether this tree handle uses SMB2 </returns>
        public virtual bool isSMB2()
        {
            try
            {
                using (SmbSessionImpl session = this.treeConnection.getSession())
                using (SmbTransportImpl transport = (SmbTransportImpl)session.getTransport())
                {
                    return transport.isSMB2();
                }
            }
            catch (SmbException e)
            {
                log.debug("Failed to connect for determining SMB2 support", e);
                return false;
            }
        }
    }
}