/*
 *	jcifs-ng for dotnet
 *  ikas-mc@github 2021
 */

using System;
using System.IO;
namespace cifs_ng.lib.io {
	public class OutputStreamToStream : Stream {

		private OutputStream outputStream;
		public OutputStreamToStream(OutputStream outputStream) {
			this.outputStream = outputStream;
		}

		public override void Flush() {
			outputStream.flush();
		}

		public override int Read(byte[] buffer, int offset, int count) {
			throw new NotImplementedException();
		}

		public override long Seek(long offset, SeekOrigin origin) {
			throw new NotImplementedException();
		}

		public override void SetLength(long value) {
			throw new NotImplementedException();
		}

		public override void Write(byte[] buffer, int offset, int count) {
			outputStream.write(buffer, offset, count);
		}

		public override bool CanRead => false;

		public override bool CanSeek => false;

		public override bool CanWrite => true;

		public override long Length => throw new NotImplementedException();

		public override long Position { get; set; }

		protected override void Dispose(bool disposing) {
			var output = this.outputStream;
			this.outputStream = null;
			output?.Dispose();
		}
	}
}