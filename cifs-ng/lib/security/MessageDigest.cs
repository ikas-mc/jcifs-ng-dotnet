/*
 *	jcifs-ng for dotnet
 *  ikas-mc@github 2021
 */

using System;
using cifs_ng.lib.security.bc;
using cifs_ng.lib.security.bc.digests;
namespace cifs_ng.lib.security {
	public abstract class MessageDigest {
		public void digest(byte[] buffer, int o, int len) {
			byte[] d = digest();
			Array.Copy(d, 0, buffer, o, len);
		}

		public byte[] digest(byte[] buffer) {
			update(buffer);
			return digest();
		}

		public abstract byte[] digest();
		public abstract int getDigestLength();

		public static MessageDigest getInstance(string algorithm) {
			switch (algorithm.ToLower()) {
			case "sha-1":
				return new BcMessageDigestSha1();
			case "md5":
				return new BcMessageDigestMd5();
			case "sha-512":
				return new BcMessageDigestSha512();
			case "md4":
				return new BcMessageDigestMd4();
			}
			throw new NotSupportedException($"The requested algorithm \"{algorithm}\" is not supported.");
		}

		public abstract void reset();
		public abstract void update(byte[] b);
		public abstract void update(byte b);
		public abstract void update(byte[] b, int offset, int len);

		public static bool isEqual(byte[] digesta, byte[] digestb) {
			if (digesta == digestb) return true;
			if (digesta == null || digestb == null) {
				return false;
			}

			int lenA = digesta.Length;
			int lenB = digestb.Length;

			if (lenB == 0) {
				return lenA == 0;
			}

			int result = 0;
			result |= lenA - lenB;

			for (int i = 0; i < lenA; i++) {
				//TODO 1 
				int indexB = i >= lenB ? 0 : i;
				result |= digesta[i] ^ digestb[indexB];
			}

			return result == 0;
		}
	}
}