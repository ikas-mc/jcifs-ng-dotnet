/*
 *	jcifs-ng for dotnet
 *  ikas-mc@github 2021
 */

using Org.BouncyCastle.Crypto.Digests;
namespace cifs_ng.lib.security.bc.digests {
	public class BcMessageDigestSha512 : MessageDigest {
		private readonly Sha512Digest sha512Digest;
		public BcMessageDigestSha512() {
			sha512Digest = new Sha512Digest();
		}
		public override byte[] digest() {
			var result = new byte[sha512Digest.GetDigestSize()];
			this.sha512Digest.DoFinal(result, 0);
			return result;
		}
		public override int getDigestLength() {
			return sha512Digest.GetDigestSize();
		}
		public override void reset() {
			sha512Digest.Reset();
		}
		public override void update(byte[] b) {
			sha512Digest.BlockUpdate(b, 0, b.Length);
		}
		public override void update(byte b) {
			sha512Digest.Update(b);
		}
		public override void update(byte[] b, int offset, int len) {
			sha512Digest.BlockUpdate(b, offset, len);
		}
	}
}