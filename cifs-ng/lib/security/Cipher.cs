/*
 *	jcifs-ng for dotnet
 *  ikas-mc@github 2021
 */

namespace cifs_ng.lib.security {
	public abstract class Cipher {
		public abstract void init(byte[] key);
		public abstract void update(byte[] src, int soff, int slen, byte[] dst, int doff);

		public abstract void update(byte[] src, int soff, int slen, byte[] dst);

		public abstract byte[] doFinal(byte[] src);

		public abstract byte[] doFinal();

		public abstract void reset();

	}
}