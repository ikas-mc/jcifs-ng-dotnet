/*
 *	jcifs-ng for dotnet
 *  ikas-mc@github 2021
 */

using System.IO;
namespace cifs_ng.lib {
	public abstract class URLConnection {
		protected URL url;

		public URLConnection(URL url) {
			this.url = url;
		}

		public URL getURL() {
			return this.url;
		}

		public abstract void connect();

		public abstract Stream openStreamForRead();

		public abstract Stream openStreamForWrite();

	}
}