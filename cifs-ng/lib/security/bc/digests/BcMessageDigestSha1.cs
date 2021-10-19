/*
 *	jcifs-ng for dotnet
 *  ikas-mc@github 2021
 */

using Org.BouncyCastle.Crypto.Digests;
namespace cifs_ng.lib.security.bc.digests {
	public class BcMessageDigestSha1 : AbstractBcMessageDigest {
		public BcMessageDigestSha1() : base(new Sha1Digest()) {
		}
	}
}