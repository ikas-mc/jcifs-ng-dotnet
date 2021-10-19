/*
 *	jcifs-ng for dotnet
 *  ikas-mc@github 2021
 */

using System.IO;
using System.Net.Sockets;
namespace cifs_ng.lib.socket {
	public class SocketStream : NetworkStream {

		public SocketStream(Socket socket, FileAccess access) : base(socket, access) {
		}

		public virtual int available() {
			return base.Socket.Available;
		}
	}
}