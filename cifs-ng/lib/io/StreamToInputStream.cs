/*
 *	jcifs-ng for dotnet
 *  ikas-mc@github 2021
 */

using System;
using System.IO;
namespace jcifs.lib.io {
	public class StreamToInputStream : IDisposable {
		private static int MAX_SKIP_BUFFER_SIZE = 2048;

		private Stream _stream;

		public StreamToInputStream(Stream stream) {
			_stream = stream;
		}


		public virtual int read() {
			return _stream.ReadByte();
		}


		public virtual int read(byte[] b) {
			return _stream.Read(b, 0, b.Length);
		}

		public virtual int read(byte[] b, int off, int len) {
			return _stream.Read(b, off, len);
		}

		public virtual long skip(long n) {
			if (n <= 0) {
				return 0;
			}

			long remaining = n;
			int nr;

			var size = (int) Math.Min(MAX_SKIP_BUFFER_SIZE, remaining);
			var skipBuffer = new byte[size];

			while (remaining > 0) {
				nr = read(skipBuffer, 0, (int) Math.Min(size, remaining));
				if (nr < 0) {
					break;
				}
				remaining -= nr;
			}

			return n - remaining;
		}


		public virtual int available() {
			return 0;
		}

		public virtual void mark(int readlimit) {
			lock (this) {}
		}


		public virtual void reset() {
			lock (this) {}
		}

		public virtual bool markSupported() {
			return false;
		}


		public virtual void Dispose() {
			_stream?.Dispose();
			_stream = null;
		}
	}
}