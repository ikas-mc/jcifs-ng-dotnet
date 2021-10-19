/*
 *	jcifs-ng for dotnet
 *  ikas-mc@github 2021
 */

using System;
namespace cifs_ng.lib {
	public class URL : Uri {

		public URL(string uriString) : base(uriString) {
		}

		public URL(string uriString, UriKind uriKind) : base(uriString, uriKind) {
		}

		public URL(Uri baseUri, string relativeUri) : base(baseUri, relativeUri) {
		}

		public URL(Uri baseUri, Uri relativeUri) : base(baseUri, relativeUri) {
		}

		public string getPath() {
			return this.AbsolutePath;
		}

		//TODO 1 url ref
		public string getRef() {
			return this.Fragment;
		}
	}
}