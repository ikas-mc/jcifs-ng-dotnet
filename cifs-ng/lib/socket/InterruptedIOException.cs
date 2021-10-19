/*
 *	jcifs-ng for dotnet
 *  ikas-mc@github 2021
 */

using System;
using System.IO;
namespace cifs_ng.lib.socket {
	public class InterruptedIOException : IOException {
		public InterruptedIOException() {
		}

		public InterruptedIOException(string message) : base(message) {
		}

		public InterruptedIOException(string message, Exception innerException) : base(message, innerException) {
		}
	}
}