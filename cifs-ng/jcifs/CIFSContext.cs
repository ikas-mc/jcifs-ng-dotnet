using System;
using cifs_ng.lib;

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
namespace jcifs {


	/// <summary>
	/// Encapsulation of client context
	/// 
	/// 
	/// A context holds the client configuration, shared services as well as the active credentials.
	/// 
	/// Usually you will want to create one context per client configuration and then
	/// multiple sub-contexts using different credentials (if necessary).
	/// 
	/// <seealso cref="withDefaultCredentials()"/>, <seealso cref="withAnonymousCredentials()"/>, <seealso cref="withCredentials(Credentials)"/>
	/// allow to create such sub-contexts.
	/// 
	/// 
	/// Implementors of this interface should extend <seealso cref="jcifs.context.BaseContext"/> or
	/// <seealso cref="jcifs.context.CIFSContextWrapper"/> to get forward compatibility.
	/// 
	/// @author mbechler
	/// 
	/// </summary>
	public interface CIFSContext {

		/// <summary>
		/// Get a resource
		/// </summary>
		/// <param name="url"> </param>
		/// <returns> the SMB resource at the specified location </returns>
		/// <exception cref="CIFSException"> </exception>
		/// throws CIFSException;
		SmbResource get(string url);


		/// <summary>
		/// Get a pipe resource
		/// </summary>
		/// <param name="url"> </param>
		/// <param name="pipeType">
		///            the type of the pipe </param>
		/// <returns> the SMB pipe resource at the specified location </returns>
		/// <exception cref="CIFSException"> </exception>
		/// throws CIFSException;
		SmbPipeResource getPipe(string url, int pipeType);


		/// 
		/// <returns> whether any connection was still in use </returns>
		/// <exception cref="CIFSException"> </exception>
		/// throws CIFSException;
		bool Dispose();


		/// 
		/// <returns> the active configuration </returns>
		Configuration getConfig();


		/// <returns> the name server client </returns>
		NameServiceClient getNameServiceClient();


		/// <returns> the buffer cache </returns>
		BufferCache getBufferCache();


		/// <returns> the transport pool </returns>
		SmbTransportPool getTransportPool();


		/// <returns> the DFS instance for this context </returns>
		DfsResolver getDfs();


		/// <returns> the SID resolver for this context </returns>
		SidResolver getSIDResolver();


		/// 
		/// <returns> the used credentials </returns>
		Credentials getCredentials();


		/// <returns> an URL handler using this context </returns>
		URLStreamHandler getUrlHandler();


		/// <returns> whether default credentials are available </returns>
		bool hasDefaultCredentials();


		/// <returns> a child context using the configured default credentials </returns>
		CIFSContext withDefaultCredentials();


		/// <returns> a child context using anonymous credentials </returns>
		CIFSContext withAnonymousCredentials();


		/// 
		/// <returns> a child context using guest credentials </returns>
		CIFSContext withGuestCrendentials();


		/// 
		/// <param name="creds"> </param>
		/// <returns> a child context using using the given credentials </returns>
		CIFSContext withCredentials(Credentials creds);


		/// <param name="locationHint"> </param>
		/// <param name="error"> </param>
		/// <returns> whether new credentials are obtained </returns>
		bool renewCredentials(string locationHint, Exception error);

	}

}