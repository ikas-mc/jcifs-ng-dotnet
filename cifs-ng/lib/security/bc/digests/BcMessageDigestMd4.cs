/*
 *	jcifs-ng for dotnet
 *  ikas-mc@github 2021
 */

using Org.BouncyCastle.Crypto.Digests;
namespace cifs_ng.lib.security.bc.digests {
	public class BcMessageDigestMd4: AbstractBcMessageDigest {
		public BcMessageDigestMd4() : base(new MD4Digest()) {
		}
	}
}