/*
 *	jcifs-ng for dotnet
 *  ikas-mc@github 2021
 */

using System;
namespace cifs_ng.lib.io {
	public interface OutputStream : IDisposable {
		void flush();
		void write(byte b);
		void write(byte[] b);
		void write(byte[] b, int off, int len);
	}
}