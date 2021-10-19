/*
 *	jcifs-ng for dotnet
 *  ikas-mc@github 2021
 */

using Org.BouncyCastle.Crypto.Digests;
namespace cifs_ng.lib.security.bc.digests {
	public class BcMessageDigestSha256 : AbstractBcMessageDigest {
		public BcMessageDigestSha256() : base(new Sha256Digest()) {
		}
	}
}