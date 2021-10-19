/*
 *	jcifs-ng for dotnet
 *  ikas-mc@github 2021
 */

using Org.BouncyCastle.Crypto.Digests;
namespace cifs_ng.lib.security.bc.digests {
	public class BcMessageDigestMd5 : AbstractBcMessageDigest {
		public BcMessageDigestMd5() : base(new MD5Digest()) {
		}
	}
}