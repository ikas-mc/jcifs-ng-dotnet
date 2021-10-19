/*
 *	jcifs-ng for dotnet
 *  ikas-mc@github 2021
 */

using System;
namespace cifs_ng.lib.io {
	public interface InputStream : IDisposable {
		int available();

		long length();
		int read();

		int read(byte[] b);

		int read(byte[] b, int off, int len);

		long seek(long offset);

		long skip(long n);

		long position();
	}
}