/*
 *	jcifs-ng for dotnet
 *  ikas-mc@github 2021
 */

using System;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
namespace cifs_ng.lib.security.bc.ciphers {
	public class BcRc4Cipher : Cipher {

		private readonly IStreamCipher cipher;

		public BcRc4Cipher() {
			//CipherUtilities
			cipher = new RC4Engine();
		}
		public override void init(byte[] key) {
			cipher.Init(true, new KeyParameter(key));
		}
		public override void update(byte[] src, int soff, int slen, byte[] dst, int doff) {
			cipher.ProcessBytes(src, soff, slen, dst, doff);
		}
		public override void update(byte[] src, int soff, int slen, byte[] dst) {
			cipher.ProcessBytes(src, soff, slen, dst, 0);
		}
		public override byte[] doFinal(byte[] src) {
			try {
				var result = new byte[src.Length];
				cipher.ProcessBytes(src, 0, src.Length, result, 0);
				return result;
			}
			finally {
				cipher.Reset();
			}
		}
		public override byte[] doFinal() {
			throw new NotImplementedException();
		}
		public override void reset() {
			cipher.Reset();
		}
	}
}