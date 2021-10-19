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

using System.Net;

namespace jcifs {



	/// 
	/// <summary>
	/// This is an internal API for resolving names
	/// 
	/// @author mbechler
	/// @internal
	/// </summary>
	public interface NameServiceClient {

		/// <returns> local host address </returns>
		NetbiosAddress getLocalHost();


		/// <returns> the local host name </returns>
		NetbiosName getLocalName();


		/// <returns> the unknown name </returns>
		NetbiosName getUnknownName();


		/// <summary>
		/// Retrieve all addresses of a host by it's address. NetBIOS hosts can
		/// have many names for a given IP address. The name and IP address make the
		/// NetBIOS address. This provides a way to retrieve the other names for a
		/// host with the same IP address.
		/// </summary>
		/// <param name="addr">
		///            the address to query </param>
		/// <returns> resolved addresses </returns>
		/// <exception cref="UnknownHostException">
		///             if address cannot be resolved </exception>
		/// throws java.net.UnknownHostException;
		NetbiosAddress[] getNbtAllByAddress(NetbiosAddress addr);


		/// <summary>
		/// Retrieve all addresses of a host by it's address. NetBIOS hosts can
		/// have many names for a given IP address. The name and IP address make
		/// the NetBIOS address. This provides a way to retrieve the other names
		/// for a host with the same IP address. See <seealso cref="getByName"/>
		/// for a description of <code>type</code>
		/// and <code>scope</code>.
		/// </summary>
		/// <param name="host">
		///            hostname to lookup all addresses for </param>
		/// <param name="type">
		///            the hexcode of the name </param>
		/// <param name="scope">
		///            the scope of the name </param>
		/// <returns> resolved addresses </returns>
		/// <exception cref="java.net.UnknownHostException">
		///             if there is an error resolving the name </exception>
		/// throws java.net.UnknownHostException;
		NetbiosAddress[] getNbtAllByAddress(string host, int type, string scope);


		/// <summary>
		/// Retrieve all addresses of a host by it's address. NetBIOS hosts can
		/// have many names for a given IP address. The name and IP address make the
		/// NetBIOS address. This provides a way to retrieve the other names for a
		/// host with the same IP address.
		/// </summary>
		/// <param name="host">
		///            hostname to lookup all addresses for </param>
		/// <returns> resolved addresses </returns>
		/// <exception cref="java.net.UnknownHostException">
		///             if there is an error resolving the name </exception>
		/// throws java.net.UnknownHostException;
		NetbiosAddress[] getNbtAllByAddress(string host);


		/// <summary>
		/// Retrieve all addresses of a host by it's name.
		/// </summary>
		/// <param name="host">
		///            hostname to lookup all addresses for </param>
		/// <param name="type">
		///            the hexcode of the name </param>
		/// <param name="scope">
		///            the scope of the name </param>
		/// <param name="svr">
		///            server to query
		/// </param>
		/// <returns> the resolved addresses </returns>
		/// <exception cref="UnknownHostException"> </exception>
		/// throws java.net.UnknownHostException;
		NetbiosAddress[] getNbtAllByName(string host, int type, string scope, IPAddress svr);


		/// <summary>
		/// Determines the address of a host given it's host name. NetBIOS
		/// names also have a <code>type</code>. Types(aka Hex Codes)
		/// are used to distinguish the various services on a host. <a
		/// href="../../../nbtcodes.html">Here</a> is
		/// a fairly complete list of NetBIOS hex codes. Scope is not used but is
		/// still functional in other NetBIOS products and so for completeness it has been
		/// implemented. A <code>scope</code> of <code>null</code> or <code>""</code>
		/// signifies no scope.
		/// 
		/// The additional <code>svr</code> parameter specifies the address to
		/// query. This might be the address of a specific host, a name server,
		/// or a broadcast address.
		/// </summary>
		/// <param name="host">
		///            the name to resolve </param>
		/// <param name="type">
		///            the hex code of the name </param>
		/// <param name="scope">
		///            the scope of the name </param>
		/// <param name="svr">
		///            server to query </param>
		/// <returns> the resolved address </returns>
		/// <exception cref="java.net.UnknownHostException">
		///             if there is an error resolving the name </exception>
		/// throws java.net.UnknownHostException;
		NetbiosAddress getNbtByName(string host, int type, string scope, IPAddress svr);


		/// <summary>
		/// Determines the address of a host given it's host name. NetBIOS
		/// names also have a <code>type</code>. Types(aka Hex Codes)
		/// are used to distinguish the various services on a host. <a
		/// href="../../../nbtcodes.html">Here</a> is
		/// a fairly complete list of NetBIOS hex codes. Scope is not used but is
		/// still functional in other NetBIOS products and so for completeness it has been
		/// implemented. A <code>scope</code> of <code>null</code> or <code>""</code>
		/// signifies no scope.
		/// </summary>
		/// <param name="host">
		///            the name to resolve </param>
		/// <param name="type">
		///            the hex code of the name </param>
		/// <param name="scope">
		///            the scope of the name </param>
		/// <returns> the resolved address </returns>
		/// <exception cref="java.net.UnknownHostException">
		///             if there is an error resolving the name </exception>
		/// throws java.net.UnknownHostException;
		NetbiosAddress getNbtByName(string host, int type, string scope);


		/// <summary>
		/// Determines the address of a host given it's host name. The name can be a NetBIOS name like
		/// "freto" or an IP address like "192.168.1.15". It cannot be a DNS name;
		/// the analygous <seealso cref="jcifs.netbios.UniAddress"/> or <seealso cref="java.net.IPAddress"/>
		/// <code>getByName</code> methods can be used for that.
		/// </summary>
		/// <param name="host">
		///            hostname to resolve </param>
		/// <returns> the resolved address </returns>
		/// <exception cref="java.net.UnknownHostException">
		///             if there is an error resolving the name </exception>
		/// throws java.net.UnknownHostException;
		NetbiosAddress getNbtByName(string host);


		/// <param name="nbtAddress"> </param>
		/// <returns> the node status responses </returns>
		/// <exception cref="UnknownHostException"> </exception>
		/// throws java.net.UnknownHostException;
		NetbiosAddress[] getNodeStatus(NetbiosAddress nbtAddress);


		/// <summary>
		/// Lookup addresses for the given <tt>hostname</tt>.
		/// </summary>
		/// <param name="hostname"> </param>
		/// <param name="possibleNTDomainOrWorkgroup"> </param>
		/// <returns> found addresses </returns>
		/// <exception cref="UnknownHostException"> </exception>
		/// throws java.net.UnknownHostException;
		Address[] getAllByName(string hostname, bool possibleNTDomainOrWorkgroup);


		/// <summary>
		/// Lookup <tt>hostname</tt> and return it's <tt>UniAddress</tt>. If the
		/// <tt>possibleNTDomainOrWorkgroup</tt> parameter is <tt>true</tt> an
		/// additional name query will be performed to locate a master browser.
		/// </summary>
		/// <param name="hostname"> </param>
		/// <param name="possibleNTDomainOrWorkgroup">
		/// </param>
		/// <returns> the first resolved address </returns>
		/// <exception cref="UnknownHostException"> </exception>
		/// throws java.net.UnknownHostException;
		Address getByName(string hostname, bool possibleNTDomainOrWorkgroup);


		/// <summary>
		/// Determines the address of a host given it's host name. The name can be a
		/// machine name like "jcifs.samba.org", or an IP address like "192.168.1.15".
		/// </summary>
		/// <param name="hostname">
		///            NetBIOS or DNS hostname to resolve </param>
		/// <returns> the found address </returns>
		/// <exception cref="java.net.UnknownHostException">
		///             if there is an error resolving the name </exception>
		/// throws java.net.UnknownHostException;
		Address getByName(string hostname);

	}
}