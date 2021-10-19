using System;
using cifs_ng.lib;
using BufferCache = jcifs.BufferCache;
using CIFSException = jcifs.CIFSException;
using Configuration = jcifs.Configuration;
using Credentials = jcifs.Credentials;
using DfsResolver = jcifs.DfsResolver;
using NameServiceClient = jcifs.NameServiceClient;
using SidResolver = jcifs.SidResolver;
using SmbPipeResource = jcifs.SmbPipeResource;
using SmbResource = jcifs.SmbResource;
using SmbTransportPool = jcifs.SmbTransportPool;
using NameServiceClientImpl = jcifs.netbios.NameServiceClientImpl;
using BufferCacheImpl = jcifs.smb.BufferCacheImpl;
using CredentialsInternal = jcifs.smb.CredentialsInternal;
using DfsImpl = jcifs.smb.DfsImpl;
using Handler = jcifs.smb.Handler;
using NtlmPasswordAuthenticator = jcifs.smb.NtlmPasswordAuthenticator;
using SIDCacheImpl = jcifs.smb.SIDCacheImpl;
using SmbFile = jcifs.smb.SmbFile;
using SmbNamedPipe = jcifs.smb.SmbNamedPipe;
using SmbTransportPoolImpl = jcifs.smb.SmbTransportPoolImpl;

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
	public class BaseContext : AbstractCIFSContext {

		private readonly Configuration config;
		private readonly DfsResolver dfs;
		private readonly SidResolver sidResolver;
		private readonly Handler urlHandler;
		private readonly NameServiceClient nameServiceClient;
		private readonly BufferCache bufferCache;
		private readonly SmbTransportPool transportPool;
		private readonly CredentialsInternal defaultCredentials;


		/// <summary>
		/// Construct a context
		/// </summary>
		/// <param name="config">
		///            configuration for the context
		///  </param>
		public BaseContext(Configuration config) {
			this.config = config;
			this.dfs = new DfsImpl(this);
			this.sidResolver = new SIDCacheImpl(this);
			this.urlHandler = new Handler(this);
			this.nameServiceClient = new NameServiceClientImpl(this);
			this.bufferCache = new BufferCacheImpl(this.config);
			this.transportPool = new SmbTransportPoolImpl();
			this.defaultCredentials = new NtlmPasswordAuthenticator();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <exception cref="CIFSException">
		/// </exception>
		/// <seealso cref= jcifs.CIFSContext#get(java.lang.String) </seealso>
		/// throws jcifs.CIFSException
		public override SmbResource get(string url) {
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
		public override SmbPipeResource getPipe(string url, int pipeType) {
			try {
				return new SmbNamedPipe(url, pipeType, this);
			}
			catch (UriFormatException e) {
				throw new CIFSException("Invalid URL " + url, e);
			}
		}


		public override SmbTransportPool getTransportPool() {
			return this.transportPool;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.CIFSContext#getConfig() </seealso>
		public override Configuration getConfig() {
			return this.config;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.CIFSContext#getDfs() </seealso>
		public override DfsResolver getDfs() {
			return this.dfs;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.CIFSContext#getNameServiceClient() </seealso>
		public override NameServiceClient getNameServiceClient() {
			return this.nameServiceClient;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.CIFSContext#getBufferCache() </seealso>
		public override BufferCache getBufferCache() {
			return this.bufferCache;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.CIFSContext#getUrlHandler() </seealso>
		public override URLStreamHandler getUrlHandler() {
			return this.urlHandler;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.CIFSContext#getSIDResolver() </seealso>
		public override SidResolver getSIDResolver() {
			return this.sidResolver;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.context.AbstractCIFSContext#getDefaultCredentials() </seealso>
		protected internal override Credentials getDefaultCredentials() {
			return this.defaultCredentials;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.CIFSContext#Dispose() </seealso>
		/// throws jcifs.CIFSException
		public override bool Dispose() {
			bool inUse = base.Dispose();
			inUse |= this.transportPool.Dispose();
			return inUse;
		}

	}

}