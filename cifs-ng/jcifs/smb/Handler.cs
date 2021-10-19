using System;
using cifs_ng.lib;
using cifs_ng.lib.ext;
using Logger = org.slf4j.Logger;
using LoggerFactory = org.slf4j.LoggerFactory;
using CIFSContext = jcifs.CIFSContext;
using SmbConstants = jcifs.SmbConstants;
using SingletonContext = jcifs.context.SingletonContext;

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

namespace jcifs.smb {





	/// <summary>
	/// URL handler for transparent smb:// URL handling
	/// 
	/// </summary>
	public class Handler : URLStreamHandler {

		private static readonly Logger log = LoggerFactory.getLogger(typeof(Handler));
		private CIFSContext transportContext;


		/// 
		public Handler() {
		}


		/// <param name="tc"> </param>
		public Handler(CIFSContext tc) {
			this.transportContext = tc;
		}


		protected  override int getDefaultPort() {
			return SmbConstants.DEFAULT_PORT;
		}


		/// throws java.io.IOException
		public override URLConnection openConnection(URL u) {
			if (log.isDebugEnabled()) {
				log.debug("Opening file " + u);
			}
			return new SmbFile(u, getTransportContext());
		}


		/// <summary>
		/// @return
		/// </summary>
		private CIFSContext getTransportContext() {
			if (this.transportContext == null) {
				this.transportContext = SingletonContext.getInstance();
			}
			return this.transportContext;
		}


		protected  override void parseURL(URL u, string spec, int start, int limit) {
			string host = u.Host;
			string path, @ref;
			int port;

			if (spec.Equals("smb://")) {
				spec = "smb:////";
				limit += 2;
			}
			else if (spec.StartsWith("smb://", StringComparison.Ordinal) == false && host != null && host.Length == 0) {
				spec = "//" + spec;
				limit += 2;
			}
			base.parseURL(u, spec, start, limit);
			path = u.getPath();
			@ref = u.getRef();
			if (@ref!=null) {
				path += '#' + @ref;
			}
			port = u.Port;
			if (port == -1) {
				port = getDefaultPort();
			}
			
			setURL(u, "smb", u.Host, port, u.Authority, u.UserInfo, path, u.Query, null);
		}

		
	}

}