/*
 *	jcifs-ng for dotnet
 *  ikas-mc@github 2021
 */

using System;
using System.IO;
namespace cifs_ng.lib.socket {
	public class SocketOutputStream : IDisposable {
		protected Stream _stream;

		public SocketOutputStream(Stream stream) {
			_stream = stream;
		}

		public virtual void Dispose() {
			_stream?.Dispose();
			_stream = null;
		}

		public virtual void flush() {
			_stream.Flush();
		}


		public virtual void write(int b) {
			_stream.WriteByte((byte) b);
		}

		public virtual void write(byte[] b) {
			_stream.Write(b, 0, b.Length);
		}

		public virtual void write(byte[] b, int offset, int len) {
			_stream.Write(b, offset, len);
		}
	}
}