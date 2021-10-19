/*
 *	jcifs-ng for dotnet
 *  ikas-mc@github 2021
 */

using System;
using System.IO;
using System.Net.Sockets;
namespace cifs_ng.lib.socket {
	public static class SocketExceptionEx {
		public static bool IsSocketTimeoutException(this Exception e) {
			switch (e) {
			case SocketException socketException1:
				return socketException1.SocketErrorCode == SocketError.TimedOut;
			case IOException ioException when ioException.InnerException is SocketException socketException2:
				return socketException2.SocketErrorCode == SocketError.TimedOut;
			default:
				return false;
			}
		}
	}
}