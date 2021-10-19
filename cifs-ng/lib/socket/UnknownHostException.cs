/*
 *	jcifs-ng for dotnet
 *  ikas-mc@github 2021
 */

using System;
namespace cifs_ng.lib.socket {
	public class UnknownHostException : Exception {
		public UnknownHostException() {
		}

		public UnknownHostException(string message) : base(message) {
		}

		public UnknownHostException(string message, Exception innerException) : base(message, innerException) {
		}
	}
}