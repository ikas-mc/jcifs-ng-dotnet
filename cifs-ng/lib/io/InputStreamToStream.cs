/*
 *	jcifs-ng for dotnet
 *  ikas-mc@github 2021
 */

using System;
using System.IO;
namespace cifs_ng.lib.io {
	public class InputStreamToStream : Stream {

		private InputStream inputStream;
		public InputStreamToStream(InputStream inputStream) {
			this.inputStream = inputStream;
		}
		private int fixReadLength(int length) {
			return length == -1 ? 0 : length;
		}

		public override void Flush() {
			throw new NotImplementedException();
		}
		public override int ReadByte() {
			return fixReadLength(inputStream.read());
		}
		public override int Read(byte[] buffer, int offset, int count) {
			return fixReadLength(inputStream.read(buffer, offset, count));
		}
		public override long Seek(long offset, SeekOrigin origin) {
			switch (origin) {
			case SeekOrigin.Begin:
				inputStream.seek(offset);
				break;
			case SeekOrigin.Current:
				inputStream.seek(offset + inputStream.position());
				break;
			case SeekOrigin.End:
				inputStream.seek(inputStream.length());
				break;
			}
			return inputStream.position();
		}
		public override void SetLength(long value) {
			throw new NotImplementedException();
		}
		public override void Write(byte[] buffer, int offset, int count) {
			throw new NotImplementedException();
		}
		public override bool CanRead => true;
		public override bool CanSeek => true;
		public override bool CanWrite => false;
		public override long Length => inputStream.length();

		public override long Position
		{
			get => inputStream.position();
			set => inputStream.seek(value);
		}

		protected override void Dispose(bool disposing) {
			var input = this.inputStream;
			this.inputStream = null;
			input?.Dispose();
		}
	}
}