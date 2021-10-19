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
namespace jcifs {


	/// <summary>
	/// This class represents a NetBIOS over TCP/IP address. Under normal
	/// conditions, users of jCIFS need not be concerned with this class as
	/// name resolution and session services are handled internally by the smb package.
	/// 
	/// <para>
	/// Applications can use the methods <code>getLocalHost</code>,
	/// <code>getByName</code>, and
	/// <code>getAllByAddress</code> to create a new NbtAddress instance. This
	/// class is symmetric with <seealso cref="java.net.IPAddress"/>.
	/// 
	/// </para>
	/// <para>
	/// <b>About NetBIOS:</b> The NetBIOS name
	/// service is a dynamic distributed service that allows hosts to resolve
	/// names by broadcasting a query, directing queries to a server such as
	/// Samba or WINS. NetBIOS is currently the primary networking layer for
	/// providing name service, datagram service, and session service to the
	/// Microsoft Windows platform. A NetBIOS name can be 15 characters long
	/// and hosts usually registers several names on the network. From a
	/// Windows command prompt you can see
	/// what names a host registers with the nbtstat command.
	/// </para>
	/// <para>
	/// <blockquote>
	/// 
	/// <pre>
	/// C:\&gt;nbtstat -a 192.168.1.15
	/// 
	///        NetBIOS Remote Machine Name Table
	/// 
	///    Name               Type         Status
	/// ---------------------------------------------
	/// JMORRIS2        &lt;00&gt;  UNIQUE      Registered
	/// BILLING-NY      &lt;00&gt;  GROUP       Registered
	/// JMORRIS2        &lt;03&gt;  UNIQUE      Registered
	/// JMORRIS2        &lt;20&gt;  UNIQUE      Registered
	/// BILLING-NY      &lt;1E&gt;  GROUP       Registered
	/// JMORRIS         &lt;03&gt;  UNIQUE      Registered
	/// 
	/// MAC Address = 00-B0-34-21-FA-3B
	/// </pre>
	/// 
	/// </blockquote>
	/// </para>
	/// <para>
	/// The hostname of this machine is <code>JMORRIS2</code>. It is
	/// a member of the group(a.k.a workgroup and domain) <code>BILLING-NY</code>. To
	/// obtain an <seealso cref="java.net.IPAddress"/> for a host one might do:
	/// 
	/// <pre>
	/// 
	/// IPAddress addr = NbtAddress.getByName("jmorris2").getInetAddress();
	/// </pre>
	/// </para>
	/// <para>
	/// From a UNIX platform with Samba installed you can perform similar
	/// diagnostics using the <code>nmblookup</code> utility.
	/// 
	/// @author Michael B. Allen
	/// </para>
	/// </summary>
	/// <seealso cref= java.net.IPAddress
	/// @since jcifs-0.1 </seealso>
	public interface NetbiosAddress : Address {

		/// <summary>
		/// Determines if the address is a group address. This is also
		/// known as a workgroup name or group name.
		/// </summary>
		/// <param name="tc">
		///            context to use </param>
		/// <returns> whether the given address is a group address
		/// </returns>
		/// <exception cref="UnknownHostException">
		///             if the host cannot be resolved to find out. </exception>
		/// throws java.net.UnknownHostException;
		bool isGroupAddress(CIFSContext tc);


		/// <summary>
		/// Checks the node type of this address.
		/// </summary>
		/// <param name="tc">
		///            context to use </param>
		/// <returns> <seealso cref="jcifs.netbios.NbtAddress.B_NODE"/>,
		///         <seealso cref="jcifs.netbios.NbtAddress.P_NODE"/>, <seealso cref="jcifs.netbios.NbtAddress.M_NODE"/>,
		///         <seealso cref="jcifs.netbios.NbtAddress.H_NODE"/>
		/// </returns>
		/// <exception cref="UnknownHostException">
		///             if the host cannot be resolved to find out. </exception>
		/// throws java.net.UnknownHostException;
		int getNodeType(CIFSContext tc);


		/// <summary>
		/// Determines if this address in the process of being deleted.
		/// </summary>
		/// <param name="tc">
		///            context to use </param>
		/// <returns> whether this address is in the process of being deleted
		/// </returns>
		/// <exception cref="UnknownHostException">
		///             if the host cannot be resolved to find out. </exception>
		/// throws java.net.UnknownHostException;
		bool isBeingDeleted(CIFSContext tc);


		/// <summary>
		/// Determines if this address in conflict with another address.
		/// </summary>
		/// <param name="tc">
		///            context to use </param>
		/// <returns> whether this address is in conflict with another address </returns>
		/// <exception cref="UnknownHostException">
		///             if the host cannot be resolved to find out. </exception>
		/// throws java.net.UnknownHostException;
		bool isInConflict(CIFSContext tc);


		/// <summary>
		/// Determines if this address is active.
		/// </summary>
		/// <param name="tc">
		///            context to use </param>
		/// <returns> whether this address is active
		/// </returns>
		/// <exception cref="UnknownHostException">
		///             if the host cannot be resolved to find out. </exception>
		/// throws java.net.UnknownHostException;
		bool isActive(CIFSContext tc);


		/// <summary>
		/// Determines if this address is set to be permanent.
		/// </summary>
		/// <param name="tc">
		///            context to use </param>
		/// <returns> whether this address is permanent
		/// </returns>
		/// <exception cref="UnknownHostException">
		///             if the host cannot be resolved to find out. </exception>
		/// throws java.net.UnknownHostException;
		bool isPermanent(CIFSContext tc);


		/// <summary>
		/// Retrieves the MAC address of the remote network interface. Samba returns all zeros.
		/// </summary>
		/// <param name="tc">
		///            context to use
		/// </param>
		/// <returns> the MAC address as an array of six bytes </returns>
		/// <exception cref="UnknownHostException">
		///             if the host cannot be resolved to
		///             determine the MAC address. </exception>
		/// throws java.net.UnknownHostException;
		byte[] getMacAddress(CIFSContext tc);


		/// <summary>
		/// Returned the hex code associated with this name(e.g. 0x20 is for the file service)
		/// </summary>
		/// <returns> the name type </returns>
		int getNameType();


		/// <returns> the name for this address </returns>
		NetbiosName getName();

	}

}