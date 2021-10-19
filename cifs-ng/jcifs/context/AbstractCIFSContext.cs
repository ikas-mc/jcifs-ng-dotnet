using System;
using cifs_ng.lib;
using cifs_ng.lib.threading;
using Logger = org.slf4j.Logger;
using LoggerFactory = org.slf4j.LoggerFactory;
using CIFSContext = jcifs.CIFSContext;
using CIFSException = jcifs.CIFSException;
using Credentials = jcifs.Credentials;
using NtlmPasswordAuthenticator = jcifs.smb.NtlmPasswordAuthenticator;
using AuthenticationType = jcifs.smb.NtlmPasswordAuthenticator.AuthenticationType;

/*
 * © 2016 AgNO3 Gmbh & Co. KG
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
namespace jcifs.context {




	/// <summary>
	/// @author mbechler
	/// 
	/// </summary>
	public abstract class AbstractCIFSContext : Runnable, CIFSContext {
		public abstract URLStreamHandler getUrlHandler();
		public abstract SidResolver getSIDResolver();
		public abstract DfsResolver getDfs();
		public abstract SmbTransportPool getTransportPool();
		public abstract BufferCache getBufferCache();
		public abstract NameServiceClient getNameServiceClient();
		public abstract Configuration getConfig();
		public abstract SmbPipeResource getPipe(string url, int pipeType);
		public abstract SmbResource get(string url);

		private static readonly Logger log = LoggerFactory.getLogger(typeof(AbstractCIFSContext));
		private bool closed;


		/// 
		public AbstractCIFSContext() {
			//Runtime.getRuntime().addShutdownHook(this);
		}


		/// <param name="creds"> </param>
		/// <returns> a wrapped context with the given credentials </returns>
		public virtual CIFSContext withCredentials(Credentials creds) {
			return new CIFSContextCredentialWrapper(this, creds);
		}


		/// 
		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.CIFSContext#withAnonymousCredentials() </seealso>
		public virtual CIFSContext withAnonymousCredentials() {
			return withCredentials(new NtlmPasswordAuthenticator());
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.CIFSContext#withDefaultCredentials() </seealso>
		public virtual CIFSContext withDefaultCredentials() {
			return withCredentials(getDefaultCredentials());
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.CIFSContext#withGuestCrendentials() </seealso>
		public virtual CIFSContext withGuestCrendentials() {
			return withCredentials(new NtlmPasswordAuthenticator(null, null, null, NtlmPasswordAuthenticator.AuthenticationType.GUEST));
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.CIFSContext#getCredentials() </seealso>
		public virtual Credentials getCredentials() {
			return getDefaultCredentials();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.CIFSContext#hasDefaultCredentials() </seealso>
		public virtual bool hasDefaultCredentials() {
			return this.getDefaultCredentials() != null && !this.getDefaultCredentials().isAnonymous();
		}


		/// <summary>
		/// @return
		/// </summary>
		protected internal abstract Credentials getDefaultCredentials();


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.CIFSContext#renewCredentials(java.lang.String, java.lang.Throwable) </seealso>
		public virtual bool renewCredentials(string locationHint, Exception error) {
			return false;
		}


		/// <summary>
		/// {@inheritDoc}
		/// 
		/// </summary>
		/// <seealso cref= jcifs.CIFSContext#Dispose() </seealso>
		/// throws jcifs.CIFSException
		public virtual bool Dispose() {
			if (!this.closed) {
				//Runtime.getRuntime().removeShutdownHook(this);
			}
			return false;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= java.lang.Thread#run() </seealso>
		public  void run() {
			try {
				this.closed = true;
				Dispose();
			}
			catch (CIFSException e) {
				log.warn("Failed to close context on shutdown", e);
			}
		}
	}

}