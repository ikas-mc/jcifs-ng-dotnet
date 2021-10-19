/*
 *	jcifs-ng for dotnet
 *  ikas-mc@github 2021
 */

using Org.BouncyCastle.Crypto.Digests;
namespace cifs_ng.lib.security.bc.digests {
	public abstract class AbstractBcMessageDigest : MessageDigest {
		private readonly GeneralDigest generalDigest;

		protected AbstractBcMessageDigest(GeneralDigest generalDigest) {
			this.generalDigest = generalDigest;
		}

		public override byte[] digest() {
			var result = new byte[generalDigest.GetDigestSize()];
			this.generalDigest.DoFinal(result, 0);
			return result;
		}
		public override int getDigestLength() {
			return generalDigest.GetDigestSize();
		}
		public override void reset() {
			generalDigest.Reset();
		}
		public override void update(byte[] b) {
			generalDigest.BlockUpdate(b, 0, b.Length);
		}
		public override void update(byte b) {
			generalDigest.Update(b);
		}
		public override void update(byte[] b, int offset, int len) {
			generalDigest.BlockUpdate(b, offset, len);
		}
	}
}