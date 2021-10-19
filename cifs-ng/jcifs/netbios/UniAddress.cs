using jcifs;

using System;
using System.Net;
using cifs_ng.lib.ext;
using Address = jcifs.Address;
using CIFSContext = jcifs.CIFSContext;

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
	/// <para>
	/// Under normal conditions it is not necessary to use
	/// this class to use jCIFS properly. Name resolusion is
	/// handled internally to the <code>jcifs.smb</code> package.
	/// </para>
	/// <para>
	/// This class is a wrapper for both <seealso cref="jcifs.netbios.NbtAddress"/>
	/// and <seealso cref="java.net.IPAddress"/>. The name resolution mechanisms
	/// used will systematically query all available configured resolution
	/// services including WINS, broadcasts, DNS, and LMHOSTS. See
	/// <a href="../../resolver.html">Setting Name Resolution Properties</a>
	/// and the <code>jcifs.resolveOrder</code> property. Changing
	/// jCIFS name resolution properties can greatly affect the behavior of
	/// the client and may be necessary for proper operation.
	/// </para>
	/// <para>
	/// This class should be used in favor of <tt>IPAddress</tt> to resolve
	/// hostnames on LANs and WANs that support a mixture of NetBIOS/WINS and
	/// DNS resolvable hosts.
	/// </para>
	/// </summary>

	public class UniAddress : Address {

		/// <summary>
		/// Check whether a hostname is actually an ip address
		/// </summary>
		/// <param name="hostname"> </param>
		/// <returns> whether this is an IP address </returns>
		public static bool isDotQuadIP(string hostname) {
			if (char.IsDigit(hostname[0])) {
				int i, len, dots;
				char[] data;

				i = dots = 0; // quick IP address validation
				len = hostname.Length;
				data = hostname.ToCharArray();
				while (i < len && char.IsDigit(data[i++])) {
					if (i == len && dots == 3) {
						// probably an IP address
						return true;
					}
					if (i < len && data[i] == '.') {
						dots++;
						i++;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Perform DNS SRV lookup on successively shorter suffixes of name
		/// and return successful suffix or throw an UnknownHostException.
		/// import javax.naming.*;
		/// import javax.naming.directory.*;
		/// public static String getDomainByName(String name) throws UnknownHostException {
		/// DirContext context;
		/// UnknownHostException uhe = null;
		/// 
		/// try {
		/// context = new InitialDirContext();
		/// for ( ;; ) {
		/// try {
		/// Attributes attributes = context.getAttributes(
		/// "dns:/_ldap._tcp.dc._msdcs." + name,
		/// new String[] { "SRV" }
		/// );
		/// return name;
		/// } catch (NameNotFoundException nnfe) {
		/// uhe = new UnknownHostException(nnfe.getMessage());
		/// }
		/// int dot = name.indexOf('.');
		/// if (dot == -1)
		/// break;
		/// name = name.substring(dot + 1);
		/// }
		/// } catch (NamingException ne) {
		/// if (log.level > 1)
		/// ne.printStackTrace(log);
		/// }
		/// 
		/// throw uhe != null ? uhe : new UnknownHostException("invalid name");
		/// }
		/// </summary>

		internal object addr;
		internal string calledName;


		/// <summary>
		/// Create a <tt>UniAddress</tt> by wrapping an <tt>IPAddress</tt> or
		/// <tt>NbtAddress</tt>.
		/// </summary>
		/// <param name="addr">
		///            wrapped address </param>
		public UniAddress(object addr) {
			if (addr == null) {
				throw new System.ArgumentException();
			}
			this.addr = addr;
		}


		/// <summary>
		/// Return the IP address of this address as a 32 bit integer.
		/// </summary>

		public override int GetHashCode() {
			return this.addr.GetHashCode();
		}


		/// <summary>
		/// Compare two addresses for equality. Two <tt>UniAddress</tt>s are equal
		/// if they are both <tt>UniAddress</tt>' and refer to the same IP address.
		/// </summary>
		public override bool Equals(object obj) {
			return obj is UniAddress && this.addr.Equals(((UniAddress) obj).addr);
		}


		/// <summary>
		/// Guess first called name to try for session establishment. This
		/// method is used exclusively by the <tt>jcifs.smb</tt> package.
		/// </summary>
		/// <returns> the guessed name </returns>
		public virtual string firstCalledName() {
			if (this.addr is NbtAddress) {
				return ((NbtAddress) this.addr).firstCalledName();
			}

			this.calledName = ((IPAddress) this.addr).ToString();
			if (isDotQuadIP(this.calledName)) {
				this.calledName = NbtAddress.SMBSERVER_NAME;
			}
			else {
				int i = this.calledName.IndexOf('.');
				if (i > 1 && i < 15) {
					this.calledName = this.calledName.Substring(0, i).ToUpper();
				}
				else if (this.calledName.Length > 15) {
					this.calledName = NbtAddress.SMBSERVER_NAME;
				}
				else {
					this.calledName = this.calledName.ToUpper();
				}
			}

			return this.calledName;
		}


		/// <summary>
		/// Guess next called name to try for session establishment. This
		/// method is used exclusively by the <tt>jcifs.smb</tt> package.
		/// </summary>
		/// <param name="tc">
		///            context to use
		/// </param>
		/// <returns> guessed alternate name </returns>
		public virtual string nextCalledName(CIFSContext tc) {
			if (this.addr is NbtAddress) {
				return ((NbtAddress) this.addr).nextCalledName(tc);
			}
			else if (!object.Equals(this.calledName, NbtAddress.SMBSERVER_NAME)) {
				this.calledName = NbtAddress.SMBSERVER_NAME;
				return this.calledName;
			}
			return null;
		}


		/// <summary>
		/// Return the underlying <tt>NbtAddress</tt> or <tt>IPAddress</tt>.
		/// </summary>
		/// <returns> wrapped address </returns>
		public virtual object getAddress() {
			return this.addr;
		}


		/// <summary>
		/// Return the hostname of this address such as "MYCOMPUTER".
		/// </summary>
		/// <returns> the hostname associated with the address </returns>
		public virtual string getHostName() {
			if (this.addr is NbtAddress) {
				return ((NbtAddress) this.addr).getHostName();
			}
			//TODO 
			return ((IPAddress) this.addr).ToString();
		}


		/// <summary>
		/// Return the IP address as text such as "192.168.1.15".
		/// </summary>
		/// <returns> the ip address </returns>
		public virtual string getHostAddress() {
			if (this.addr is NbtAddress) {
				return ((NbtAddress) this.addr).getHostAddress();
			}
			return ((IPAddress) this.addr).ToString();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <exception cref="UnknownHostException">
		/// </exception>
		/// <seealso cref= jcifs.Address#toInetAddress() </seealso>
		/// throws java.net.UnknownHostException
		public virtual IPAddress toInetAddress() {
			if (this.addr is Address) {
				return ((Address) this.addr).toInetAddress();
			}
			else if (this.addr is IPAddress) {
				return (IPAddress) this.addr;
			}
			return null;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Address#unwrap(java.lang.Class) </seealso>
		public virtual T unwrap<T>(Type type)  {
			if (this.addr is Address) {
				return (T)((Address) this.addr).unwrap<Address>(type);
			}
			else if (this is T v) {
				return  v;
			}
			return default;
		}


		/// <summary>
		/// Return the a text representation of this address such as
		/// <tt>MYCOMPUTER/192.168.1.15</tt>.
		/// </summary>
		public override string ToString() {
			return this.addr.ToString();
		}
	}

}