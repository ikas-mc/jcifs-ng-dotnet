/*
 *	jcifs-ng for dotnet
 *  ikas-mc@github 2021
 */

using System;
using jcifs;
namespace cifs_ng.lib {
	public class URLStreamHandler {
		protected virtual int getDefaultPort() {
			return SmbConstants.DEFAULT_PORT;
		}

		public virtual URLConnection openConnection(URL u) {
			throw new NotImplementedException();
		}

		protected virtual void parseURL(URL u, string spec, int start, int limit) {
			throw new NotImplementedException();
		}

		protected void setURL(URL url, string smb, string uHost, int port, string uAuthority, object getUserInfo, string path, object getQuery, object o) {
			throw new NotImplementedException();
		}
	}
}