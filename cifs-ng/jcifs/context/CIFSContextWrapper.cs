using System;
using cifs_ng.lib;
using BufferCache = jcifs.BufferCache;
using CIFSContext = jcifs.CIFSContext;
using CIFSException = jcifs.CIFSException;
using Configuration = jcifs.Configuration;
using Credentials = jcifs.Credentials;
using DfsResolver = jcifs.DfsResolver;
using NameServiceClient = jcifs.NameServiceClient;
using SidResolver = jcifs.SidResolver;
using SmbPipeResource = jcifs.SmbPipeResource;
using SmbResource = jcifs.SmbResource;
using SmbTransportPool = jcifs.SmbTransportPool;
using Handler = jcifs.smb.Handler;
using SmbFile = jcifs.smb.SmbFile;
using SmbNamedPipe = jcifs.smb.SmbNamedPipe;

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
	public class CIFSContextWrapper : CIFSContext {

		private readonly CIFSContext @delegate;
		private Handler wrappedHandler;


		/// <param name="delegate">
		///            context to delegate non-override methods to
		///  </param>
		public CIFSContextWrapper(CIFSContext @delegate) {
			this.@delegate = @delegate;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <exception cref="CIFSException">
		/// </exception>
		/// <seealso cref= jcifs.CIFSContext#get(java.lang.String) </seealso>
		/// throws jcifs.CIFSException
		public virtual SmbResource get(string url) {
			try {
				return new SmbFile(url, this);
			}
			catch (UriFormatException e) {
				throw new CIFSException("Invalid URL " + url, e);
			}
		}


		/// 
		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.CIFSContext#getPipe(java.lang.String, int) </seealso>
		/// throws jcifs.CIFSException
		public virtual SmbPipeResource getPipe(string url, int pipeType) {
			try {
				return new SmbNamedPipe(url, pipeType, this);
			}
			catch (UriFormatException e) {
				throw new CIFSException("Invalid URL " + url, e);
			}
		}


		protected internal virtual CIFSContext wrap(CIFSContext newContext) {
			return newContext;
		}


		public virtual Configuration getConfig() {
			return this.@delegate.getConfig();
		}


		public virtual DfsResolver getDfs() {
			return this.@delegate.getDfs();
		}


		public virtual Credentials getCredentials() {
			return this.@delegate.getCredentials();
		}


		public virtual URLStreamHandler getUrlHandler() {
			if (this.wrappedHandler == null) {
				this.wrappedHandler = new Handler(this);
			}
			return this.wrappedHandler;
		}


		public virtual SidResolver getSIDResolver() {
			return this.@delegate.getSIDResolver();
		}


		public virtual bool hasDefaultCredentials() {
			return this.@delegate.hasDefaultCredentials();
		}


		public virtual CIFSContext withCredentials(Credentials creds) {
			return wrap(this.@delegate.withCredentials(creds));
		}


		public virtual CIFSContext withDefaultCredentials() {
			return wrap(this.@delegate.withDefaultCredentials());
		}


		public virtual CIFSContext withAnonymousCredentials() {
			return wrap(this.@delegate.withAnonymousCredentials());
		}


		public virtual CIFSContext withGuestCrendentials() {
			return wrap(this.@delegate.withGuestCrendentials());
		}


		public virtual bool renewCredentials(string locationHint, Exception error) {
			return this.@delegate.renewCredentials(locationHint, error);
		}


		public virtual NameServiceClient getNameServiceClient() {
			return this.@delegate.getNameServiceClient();
		}


		public virtual BufferCache getBufferCache() {
			return this.@delegate.getBufferCache();
		}


		public virtual SmbTransportPool getTransportPool() {
			return this.@delegate.getTransportPool();
		}


		/// throws jcifs.CIFSException
		public virtual bool Dispose() {
			return this.@delegate.Dispose();
		}
	}

}