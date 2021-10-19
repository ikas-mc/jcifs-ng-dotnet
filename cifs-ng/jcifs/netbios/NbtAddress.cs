using jcifs;

using System;
using System.Net;
using cifs_ng.lib;
using cifs_ng.lib.ext;
using cifs_ng.lib.socket;
using Address = jcifs.Address;
using CIFSContext = jcifs.CIFSContext;
using NetbiosAddress = jcifs.NetbiosAddress;
using NetbiosName = jcifs.NetbiosName;

/* jcifs smb client library in Java
 * Copyright (C) 2000  "Michael B. Allen" <jcifs at samba dot org>
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

namespace jcifs.netbios {




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

	public sealed class NbtAddress : NetbiosAddress {

		/// <summary>
		/// This is a special name that means all hosts. If you wish to find all hosts
		/// on a network querying a workgroup group name is the preferred method.
		/// </summary>
		public const string ANY_HOSTS_NAME = "*\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000";

		/// <summary>
		/// This is a special name for querying the master browser that serves the
		/// list of hosts found in "Network Neighborhood".
		/// </summary>

		public const string MASTER_BROWSER_NAME = "\u0001\u0002__MSBROWSE__\u0002";

		/// <summary>
		/// A special generic name specified when connecting to a host for which
		/// a name is not known. Not all servers respond to this name.
		/// </summary>

		public const string SMBSERVER_NAME = "*SMBSERVER     ";

		/// <summary>
		/// A B node only broadcasts name queries. This is the default if a
		/// nameserver such as WINS or Samba is not specified.
		/// </summary>

		public const int B_NODE = 0;

		/// <summary>
		/// A Point-to-Point node, or P node, unicasts queries to a nameserver
		/// only. Natrually the <code>jcifs.netbios.nameserver</code> property must
		/// be set.
		/// </summary>

		public const int P_NODE = 1;

		/// <summary>
		/// Try Broadcast queries first, then try to resolve the name using the
		/// nameserver.
		/// </summary>

		public const int M_NODE = 2;

		/// <summary>
		/// A Hybrid node tries to resolve a name using the nameserver first. If
		/// that fails use the broadcast address. This is the default if a nameserver
		/// is provided. This is the behavior of Microsoft Windows machines.
		/// </summary>

		public const int H_NODE = 3;

		/// <summary>
		/// Unknown MAC Address
		/// </summary>
		public static readonly byte[] UNKNOWN_MAC_ADDRESS = new byte[] {(byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00};

		internal Name hostName;
		internal int address, nodeType;
		internal bool groupName, isBeingDeletedField, isInConflictField, isActiveField, isPermanentField, isDataFromNodeStatus;
		internal byte[] macAddress;
		internal string calledName;


		internal NbtAddress(Name hostName, int address, bool groupName, int nodeType) {
			this.hostName = hostName;
			this.address = address;
			this.groupName = groupName;
			this.nodeType = nodeType;
		}


		internal NbtAddress(Name hostName, int address, bool groupName, int nodeType, bool isBeingDeleted, bool isInConflict, bool isActive, bool isPermanent, byte[] macAddress) {

			/*
			 * The NodeStatusResponse.readNodeNameArray method may also set this
			 * information. These two places where node status data is populated should
			 * be consistent. Be carefull!
			 */
			this.hostName = hostName;
			this.address = address;
			this.groupName = groupName;
			this.nodeType = nodeType;
			this.isBeingDeletedField = isBeingDeleted;
			this.isInConflictField = isInConflict;
			this.isActiveField = isActive;
			this.isPermanentField = isPermanent;
			this.macAddress = macAddress;
			this.isDataFromNodeStatus = true;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Address#unwrap(java.lang.Class) </seealso>
		public T unwrap<T>(Type type) {
			if (this is T v) {
				return v;
			}
			return default(T);
		}


		/// <summary>
		/// Guess next called name to try for session establishment. These
		/// methods are used by the smb package.
		/// </summary>
		/// <returns> guessed name </returns>
		public string firstCalledName() {

			this.calledName = this.hostName.name;

			if (char.IsDigit(this.calledName[0])) {
				int i, len, dots;
				char[] data;

				i = dots = 0; // quick IP address validation
				len = this.calledName.Length;
				data = this.calledName.ToCharArray();
				while (i < len && char.IsDigit(data[i++])) {
					if (i == len && dots == 3) {
						// probably an IP address
						this.calledName = SMBSERVER_NAME;
						break;
					}
					if (i < len && data[i] == '.') {
						dots++;
						i++;
					}
				}
			}
			else {
				switch (this.hostName.hexCode) {
				case 0x1B:
				case 0x1C:
				case 0x1D:
					this.calledName = SMBSERVER_NAME;
				break;
				}
			}

			return this.calledName;
		}


		/// 
		/// <param name="tc">
		///            context to use </param>
		/// <returns> net name to try </returns>
		public string nextCalledName(CIFSContext tc) {

			if (object.Equals(this.calledName, this.hostName.name)) {
				this.calledName = SMBSERVER_NAME;
			}
			else if (SMBSERVER_NAME.Equals(this.calledName)) {
				NetbiosAddress[] addrs;

				try {
					addrs = tc.getNameServiceClient().getNodeStatus(this);
					if (this.getNameType() == 0x1D) {
						for (int i = 0; i < addrs.Length; i++) {
							if (addrs[i].getNameType() == 0x20) {
								return addrs[i].getHostName();
							}
						}
						return null;
					}
					else if (this.isDataFromNodeStatus) {
						/*
						 * 'this' has been updated and should now
						 * have a real NetBIOS name
						 */
						this.calledName = null;
						return getHostName();
					}
				}
				catch (UnknownHostException) {
					this.calledName = null;
				}
			}
			else {
				this.calledName = null;
			}

			return this.calledName;
		}


		/*
		 * There are three degrees of state that any NbtAddress can have.
		 * 
		 * 1) IP Address - If a dot-quad IP string is used with getByName (or used
		 * to create an NbtAddress internal to this netbios package), no query is
		 * sent on the wire and the only state this object has is it's IP address
		 * (but that's enough to connect to a host using *SMBSERVER for CallingName).
		 * 
		 * 2) IP Address, NetBIOS name, nodeType, groupName - If however a
		 * legal NetBIOS name string is used a name query request will retreive
		 * the IP, node type, and whether or not this NbtAddress represents a
		 * group name. This degree of state can be obtained with a Name Query
		 * Request or Node Status Request.
		 * 
		 * 3) All - The NbtAddress will be populated with all state such as mac
		 * address, isPermanent, isBeingDeleted, ...etc. This information can only
		 * be retrieved with the Node Status request.
		 * 
		 * The degree of state that an NbtAddress has is dependant on how it was
		 * created and what is required of it. The second degree of state is the
		 * most common. This is the state information that would be retrieved from
		 * WINS for example. Natrually it is not practical for every NbtAddress
		 * to be populated will all state requiring a Node Status on every host
		 * encountered. The below methods allow state to be populated when requested
		 * in a lazy fashon.
		 */

		/// throws java.net.UnknownHostException
		internal void checkData(CIFSContext tc) {
			if (this.hostName.isUnknown()) {
				tc.getNameServiceClient().getNbtAllByAddress(this);
			}
		}


		/// throws java.net.UnknownHostException
		internal void checkNodeStatusData(CIFSContext tc) {
			if (this.isDataFromNodeStatus == false) {
				tc.getNameServiceClient().getNbtAllByAddress(this);
			}
		}


		/// throws java.net.UnknownHostException
		public bool isGroupAddress(CIFSContext tc) {
			checkData(tc);
			return this.groupName;
		}


		/// throws java.net.UnknownHostException
		public int getNodeType(CIFSContext tc) {
			checkData(tc);
			return this.nodeType;
		}


		/// throws java.net.UnknownHostException
		public bool isBeingDeleted(CIFSContext tc) {
			checkNodeStatusData(tc);
			return this.isBeingDeletedField;
		}


		/// throws java.net.UnknownHostException
		public bool isInConflict(CIFSContext tc) {
			checkNodeStatusData(tc);
			return this.isInConflictField;
		}


		/// throws java.net.UnknownHostException
		public bool isActive(CIFSContext tc) {
			checkNodeStatusData(tc);
			return this.isActiveField;
		}


		/// throws java.net.UnknownHostException
		public bool isPermanent(CIFSContext tc) {
			checkNodeStatusData(tc);
			return this.isPermanentField;
		}


		/// throws java.net.UnknownHostException
		public byte[] getMacAddress(CIFSContext tc) {
			checkNodeStatusData(tc);
			return this.macAddress;
		}


		/// <summary>
		/// The hostname of this address. If the hostname is null the local machines
		/// IP address is returned.
		/// </summary>
		/// <returns> the text representation of the hostname associated with this address </returns>
		public string getHostName() {
			/*
			 * 2010 - We no longer try a Node Status to get the
			 * hostname because apparently some servers do not respond
			 * anymore. I think everyone post Windows 98 will accept
			 * an IP address as the tconHostName which is the principal
			 * use of this method.
			 */
			if (this.hostName.isUnknown()) {
				return getHostAddress();
			}
			return this.hostName.name;
		}


		public NetbiosName getName() {
			return this.hostName;
		}


		/// <summary>
		/// Returns the raw IP address of this NbtAddress. The result is in network
		/// byte order: the highest order byte of the address is in getAddress()[0].
		/// </summary>
		/// <returns> a four byte array </returns>
		public byte[] getAddress() {
			byte[] addr = new byte[4];
			addr[0] = unchecked((byte)(((int)((uint)this.address >> 24)) & 0xFF));
			addr[1] = unchecked((byte)(((int)((uint)this.address >> 16)) & 0xFF));
			addr[2] = unchecked((byte)(((int)((uint)this.address >> 8)) & 0xFF));
			addr[3] = unchecked((byte)(this.address & 0xFF));
			return addr;
		}


		/// <summary>
		/// To convert this address to an <code>IPAddress</code>.
		/// </summary>
		/// <returns> the <seealso cref="java.net.IPAddress"/> representation of this address. </returns>
		/// <exception cref="UnknownHostException"> </exception>

		/// throws java.net.UnknownHostException
		public IPAddress getInetAddress() {
			return IPAddress.Parse(getHostAddress());
		}


		/// throws java.net.UnknownHostException
		public IPAddress toInetAddress() {
			return getInetAddress();
		}


		/// <summary>
		/// Returns this IP adress as a <seealso cref="string"/> in the form "%d.%d.%d.%d".
		/// </summary>
		/// <returns> string representation of the IP address </returns>

		public string getHostAddress() {
			return (((int)((uint)this.address >> 24)) & 0xFF) + "." + (((int)((uint)this.address >> 16)) & 0xFF) + "." + (((int)((uint)this.address >> 8)) & 0xFF) + "." + (((int)((uint)this.address >> 0)) & 0xFF);
		}


		public int getNameType() {
			return this.hostName.hexCode;
		}


		/// <summary>
		/// Returns a hashcode for this IP address. The hashcode comes from the IP address
		/// and is not generated from the string representation. So because NetBIOS nodes
		/// can have many names, all names associated with an IP will have the same
		/// hashcode.
		/// </summary>

		public override int GetHashCode() {
			return this.address;
		}


		/// <summary>
		/// Determines if this address is equal two another. Only the IP Addresses
		/// are compared. Similar to the <seealso cref="hashCode"/> method, the comparison
		/// is based on the integer IP address and not the string representation.
		/// </summary>

		public override bool Equals(object obj) {
			return (obj != null) && (obj is NbtAddress) && (((NbtAddress) obj).address == this.address);
		}


		/// <summary>
		/// Returns the <seealso cref="string"/> representaion of this address.
		/// </summary>

		public override string ToString() {
			return this.hostName.ToString() + "/" + getHostAddress();
		}
	}

}