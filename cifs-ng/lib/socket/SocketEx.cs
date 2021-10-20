/*
 *	jcifs-ng for dotnet
 *  ikas-mc@github 2021
 */

using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
namespace cifs_ng.lib.socket {
	public class SocketEx : Socket {

		private SocketEx(AddressFamily addressFamily,SocketType socketType, ProtocolType protocolType) : base(addressFamily, socketType, protocolType) {
		}

		public static SocketEx ofTcpSocket(AddressFamily family) {
			return new SocketEx(family,SocketType.Stream, ProtocolType.Tcp);
		}

		public static SocketEx ofUdpSocket(AddressFamily family) {
			return new SocketEx(family, SocketType.Dgram, ProtocolType.Udp);
		}

		public void Connect2(IPEndPoint endPoint, int timeOut) {
			using (var evt = new ManualResetEventSlim(false))
			using (var args = new SocketAsyncEventArgs {RemoteEndPoint = endPoint}) {
				args.Completed += delegate { evt.Set(); };

				ConnectAsync(args);

				if (!evt.Wait(timeOut)) {
					CancelConnectAsync(args);
					throw new SocketException((int) SocketError.TimedOut);
				}

				if (args.SocketError != SocketError.Success) {
					throw new SocketException((int) args.SocketError);
				}
			}
		}
		public void Connect(IPEndPoint endPoint, int timeOut) {
			this.Connect(endPoint);
		}

		public SocketInputStream GetInputStream() {
			return new SocketInputStream(new SocketStream(this, FileAccess.Read));
		}

		public SocketOutputStream GetOutputStream() {
			return new SocketOutputStream(new NetworkStream(this, FileAccess.Write));
		}

		public bool isClosed() {
			return _isClosed;
		}

		private volatile bool _isClosed;
		protected override void Dispose(bool disposing) {
			_isClosed = true;
		}

		public void shutdownOutput() {
		}
	}
}