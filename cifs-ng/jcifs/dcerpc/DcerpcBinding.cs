using System;
using System.Collections.Generic;
using cifs_ng.lib.ext;
using lsarpc = jcifs.dcerpc.msrpc.lsarpc;
using netdfs = jcifs.dcerpc.msrpc.netdfs;
using samr = jcifs.dcerpc.msrpc.samr;
using srvsvc = jcifs.dcerpc.msrpc.srvsvc;

/* jcifs msrpc client library in Java
 * Copyright (C) 2006  "Michael B. Allen" <jcifs at samba dot org>
 *                     "Eric Glass" <jcifs at samba dot org>
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

namespace jcifs.dcerpc {




	/// 
	public class DcerpcBinding {

		private static readonly IDictionary<string, string> INTERFACES = new Dictionary<string, string>();


		static DcerpcBinding() {
			INTERFACES["srvsvc"] = srvsvc.getSyntax();
			INTERFACES["lsarpc"] = lsarpc.getSyntax();
			INTERFACES["samr"] = samr.getSyntax();
			INTERFACES["netdfs"] = netdfs.getSyntax();
			INTERFACES["netlogon"] = "12345678-1234-abcd-ef00-01234567cffb:1.0";
			INTERFACES["wkssvc"] = "6BFFD098-A112-3610-9833-46C3F87E345A:1.0";
			INTERFACES["samr"] = "12345778-1234-ABCD-EF00-0123456789AC:1.0";
		}


		/// <summary>
		/// Add an interface to the registry
		/// </summary>
		/// <param name="name"> </param>
		/// <param name="syntax"> </param>
		public static void addInterface(string name, string syntax) {
			INTERFACES[name] = syntax;
		}

		private string proto;
		private IDictionary<string, object> options = null;
		private string server;
		private string endpoint = null;
		private UUID uuid = null;
		private int major;
		private int minor;


		internal DcerpcBinding(string proto, string server) {
			this.proto = proto;
			this.server = server;
		}


		/// <returns> the proto </returns>
		public virtual string getProto() {
			return this.proto;
		}


		/// <returns> the options </returns>
		public virtual IDictionary<string, object> getOptions() {
			return this.options;
		}


		/// <returns> the server </returns>
		public virtual string getServer() {
			return this.server;
		}


		/// <returns> the endpoint </returns>
		public virtual string getEndpoint() {
			return this.endpoint;
		}


		/// <returns> the uuid </returns>
		internal virtual UUID getUuid() {
			return this.uuid;
		}


		/// <returns> the major </returns>
		internal virtual int getMajor() {
			return this.major;
		}


		/// <returns> the minor </returns>
		internal virtual int getMinor() {
			return this.minor;
		}


		/// throws DcerpcException
		internal virtual void setOption(string key, object val) {
			if (key.Equals("endpoint")) {
				this.endpoint = val.ToString();
				string lep = this.endpoint.ToLower();
				if (lep.StartsWith("\\pipe\\", StringComparison.Ordinal)) {
					string iface = INTERFACES.get(lep.Substring(6));
					if (iface!= null) {
						int c, p;
						c = iface.IndexOf(':');
						p = iface.IndexOf('.', c + 1);
						this.uuid = new UUID(iface.Substring(0, c));
						this.major = int.Parse(iface.Substring(c + 1, p - (c + 1)));
						this.minor = int.Parse(iface.Substring(p + 1));
						return;
					}
				}
				throw new DcerpcException("Bad endpoint: " + this.endpoint);
			}
			if (this.options == null) {
				this.options = new Dictionary<string, object>();
			}
			this.options[key] = val;
		}


		internal virtual object getOption(string key) {
			if (key.Equals("endpoint")) {
				return this.endpoint;
			}
			if (this.options != null) {
				return this.options.get(key);
			}
			return null;
		}


		public override string ToString() {
			string ret = this.proto + ":" + this.server + "[" + this.endpoint;
			if (this.options != null) {
				foreach (var entry in this.options) {
					ret += "," + entry.Key + "=" + entry.Value;
				}
			}
			ret += "]";
			return ret;
		}
	}

}