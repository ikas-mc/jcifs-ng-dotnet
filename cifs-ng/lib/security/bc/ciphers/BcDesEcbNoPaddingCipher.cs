/*
 *	jcifs-ng for dotnet
 *  ikas-mc@github 2021
 */

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
namespace cifs_ng.lib.security.bc.ciphers {
	public class BcDesEcbNoPaddingCipher : Cipher {

		private readonly BufferedBlockCipher cipher;

		public BcDesEcbNoPaddingCipher() {
			//CipherUtilities
			cipher = new BufferedBlockCipher(new DesEngine());
		}
		public override void init(byte[] key) {
			cipher.Init(true, new DesParameters(key));
		}
		public override void update(byte[] src, int soff, int slen, byte[] dst, int doff) {
			cipher.ProcessBytes(src, soff, slen, dst, doff);
		}
		public override void update(byte[] src, int soff, int slen, byte[] dst) {
			cipher.ProcessBytes(src, soff, slen, dst, 0);
		}
		public override byte[] doFinal(byte[] src) {
			return cipher.DoFinal(src);
		}
		public override byte[] doFinal() {
			return cipher.DoFinal();
		}

		public override void reset() {
			cipher.Reset();
		}
	}
}