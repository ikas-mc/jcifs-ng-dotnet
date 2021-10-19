using System;
using System.Net;

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
	/// This is an internal API for managing pools of SMB connections
	/// 
	/// @author mbechler
	/// @internal
	/// </summary>
	public interface SmbTransportPool {

		/// <param name="tf"> </param>
		/// <param name="name"> </param>
		/// <param name="port"> </param>
		/// <param name="exclusive"> </param>
		/// <param name="forceSigning"> </param>
		/// <returns> a connected transport </returns>
		/// <exception cref="UnknownHostException"> </exception>
		/// <exception cref="IOException"> </exception>
		/// throws UnknownHostException, java.io.IOException;
		SmbTransport getSmbTransport(CIFSContext tf, string name, int port, bool exclusive, bool forceSigning);


		/// <summary>
		/// Get transport connection
		/// </summary>
		/// <param name="tc">
		///            context to use </param>
		/// <param name="address"> </param>
		/// <param name="port"> </param>
		/// <param name="exclusive">
		///            whether to acquire an unshared connection </param>
		/// <returns> a transport connection to the target </returns>
		SmbTransport getSmbTransport(CIFSContext tc, Address address, int port, bool exclusive);


		/// <summary>
		/// Get transport connection
		/// </summary>
		/// <param name="tc">
		///            context to use </param>
		/// <param name="address"> </param>
		/// <param name="port"> </param>
		/// <param name="exclusive">
		///            whether to acquire an unshared connection </param>
		/// <param name="forceSigning">
		///            whether to enforce SMB signing on this connection </param>
		/// <returns> a transport connection to the target </returns>
		SmbTransport getSmbTransport(CIFSContext tc, Address address, int port, bool exclusive, bool forceSigning);


		/// <summary>
		/// Get transport connection, with local binding
		/// </summary>
		/// <param name="tc">
		///            context to use </param>
		/// <param name="address"> </param>
		/// <param name="port"> </param>
		/// <param name="localAddr"> </param>
		/// <param name="localPort"> </param>
		/// <param name="hostName"> </param>
		/// <param name="exclusive">
		///            whether to acquire an unshared connection </param>
		/// <returns> a transport connection to the target </returns>
		SmbTransport getSmbTransport(CIFSContext tc, Address address, int port, IPAddress localAddr, int localPort, string hostName, bool exclusive);


		/// <param name="tc">
		///            context to use </param>
		/// <param name="address"> </param>
		/// <param name="port"> </param>
		/// <param name="localAddr"> </param>
		/// <param name="localPort"> </param>
		/// <param name="hostName"> </param>
		/// <param name="exclusive">
		///            whether to acquire an unshared connection </param>
		/// <param name="forceSigning">
		///            whether to enforce SMB signing on this connection </param>
		/// <returns> a transport connection to the target </returns>
		SmbTransport getSmbTransport(CIFSContext tc, Address address, int port, IPAddress localAddr, int localPort, string hostName, bool exclusive, bool forceSigning);


		/// 
		/// <param name="trans"> </param>
		void removeTransport(SmbTransport trans);


		/// <summary>
		/// Closes the pool and all connections in it
		/// </summary>
		/// <returns> whether any transport was still in use
		/// </returns>
		/// <exception cref="CIFSException">
		///  </exception>
		/// throws CIFSException;
		bool Dispose();


		/// <summary>
		/// Authenticate arbitrary credentials represented by the
		/// <tt>NtlmPasswordAuthentication</tt> object against the domain controller
		/// specified by the <tt>UniAddress</tt> parameter. If the credentials are
		/// not accepted, an <tt>SmbAuthException</tt> will be thrown. If an error
		/// occurs an <tt>SmbException</tt> will be thrown. If the credentials are
		/// valid, the method will return without throwing an exception. See the
		/// last <a href="../../../faq.html">FAQ</a> question.
		/// <para>
		/// See also the <tt>jcifs.smb.client.logonShare</tt> property.
		/// 
		/// </para>
		/// </summary>
		/// <param name="dc"> </param>
		/// <param name="tc"> </param>
		/// <exception cref="CIFSException"> </exception>
		/// @deprecated functionality is broken and will be removed at some point,
		///             use actual Active Directory authentication instead 
		/// throws CIFSException;
		[Obsolete("functionality is broken and will be removed at some point,")]
		void logon(CIFSContext tc, Address dc);


		/// <summary>
		/// Authenticate arbitrary credentials represented by the
		/// <tt>NtlmPasswordAuthentication</tt> object against the domain controller
		/// specified by the <tt>UniAddress</tt> parameter. If the credentials are
		/// not accepted, an <tt>SmbAuthException</tt> will be thrown. If an error
		/// occurs an <tt>SmbException</tt> will be thrown. If the credentials are
		/// valid, the method will return without throwing an exception. See the
		/// last <a href="../../../faq.html">FAQ</a> question.
		/// <para>
		/// See also the <tt>jcifs.smb.client.logonShare</tt> property.
		/// 
		/// </para>
		/// </summary>
		/// <param name="dc"> </param>
		/// <param name="port"> </param>
		/// <param name="tc"> </param>
		/// <exception cref="CIFSException"> </exception>
		/// @deprecated functionality is broken and will be removed at some point,
		///             use actual Active Directory authentication instead 
		/// throws CIFSException;
		[Obsolete("functionality is broken and will be removed at some point,")]
		void logon(CIFSContext tc, Address dc, int port);


		/// <summary>
		/// Get NTLM challenge from a server
		/// </summary>
		/// <param name="dc"> </param>
		/// <param name="tc"> </param>
		/// <returns> NTLM challenge </returns>
		/// <exception cref="CIFSException"> </exception>
		/// @deprecated functionality is broken and will be removed at some point,
		///             use actual Active Directory authentication instead 
		/// throws CIFSException;
		[Obsolete("functionality is broken and will be removed at some point,")]
		byte[] getChallenge(CIFSContext tc, Address dc);


		/// <summary>
		/// Get NTLM challenge from a server
		/// </summary>
		/// <param name="dc"> </param>
		/// <param name="port"> </param>
		/// <param name="tc"> </param>
		/// <returns> NTLM challenge </returns>
		/// <exception cref="CIFSException"> </exception>
		/// @deprecated functionality is broken and will be removed at some point,
		///             use actual Active Directory authentication instead 
		/// throws CIFSException;
		[Obsolete("functionality is broken and will be removed at some point,")]
		byte[] getChallenge(CIFSContext tc, Address dc, int port);

	}
}