using System;
using System.Numerics;
using cifs_ng.lib;
using cifs_ng.lib.security;
using cifs_ng.lib.security.bc.ciphers;
using jcifs.lib;
//using BouncyCastleProvider = org.bouncycastle.jce.provider.BouncyCastleProvider;
using CIFSUnsupportedCryptoException = jcifs.CIFSUnsupportedCryptoException;

/*
 * © 2016 AgNO3 Gmbh & Co. KG
 * 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 * 
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */
namespace jcifs.util {






	/// <summary>
	/// @author mbechler
	/// 
	/// </summary>
	public sealed class Crypto {

		//private static Provider provider = null;


		/// 
		private Crypto() {
		}


		/// 
		/// <returns> MD4 digest </returns>
		public static MessageDigest getMD4()
		{
			try {
				return MessageDigest.getInstance("md4");
			}
			catch (NotSupportedException e) {
				throw new CIFSUnsupportedCryptoException(e);
			}
		}


		/// 
		/// <returns> MD5 digest </returns>
		public static MessageDigest getMD5() {
			try {
				return MessageDigest.getInstance("md5");
			}
			catch (NotSupportedException e) {
				throw new CIFSUnsupportedCryptoException(e);
			}
		}


		/// <returns> SHA512 digest </returns>
		public static MessageDigest getSHA512() {
			try {
				return MessageDigest.getInstance("sha-512");
			}
			catch (NotSupportedException e) {
				throw new CIFSUnsupportedCryptoException(e);
			}
		}


		/// 
		/// <param name="key"> </param>
		/// <returns> HMACT64 MAC </returns>
		public static MessageDigest getHMACT64(byte[] key) {
			return new HMACT64(key);
		}


		/// 
		/// <param name="key"> </param>
		/// <returns> RC4 cipher </returns>
		public static Cipher getArcfour(byte[] key) {
			var c=new BcRc4Cipher();
			c.init(key);
			return c;
		}


		/// <param name="key">
		///            7 or 8 byte DES key </param>
		/// <returns> DES cipher in encryption mode </returns>
		public static Cipher getDES(byte[] key) {
			if (key.Length == 7) {
				return getDES(des7to8(key));
			}

			var c=new BcDesEcbNoPaddingCipher();
			c.init(key);
			return c;
		}


		/// <param name="key">
		///            7-byte "raw" DES key </param>
		/// <returns> 8-byte DES key with parity </returns>
		internal static byte[] des7to8(byte[] key) {
			byte[] key8 = new byte[8];
			key8[0] = unchecked((byte)(key[0] & 0xFE));
			key8[1] = (byte)((key[0] << 7) | ((int)((uint)(key[1] & 0xFF) >> 1)));
			key8[2] = (byte)((key[1] << 6) | ((int)((uint)(key[2] & 0xFF) >> 2)));
			key8[3] = (byte)((key[2] << 5) | ((int)((uint)(key[3] & 0xFF) >> 3)));
			key8[4] = (byte)((key[3] << 4) | ((int)((uint)(key[4] & 0xFF) >> 4)));
			key8[5] = (byte)((key[4] << 3) | ((int)((uint)(key[5] & 0xFF) >> 5)));
			key8[6] = (byte)((key[5] << 2) | ((int)((uint)(key[6] & 0xFF) >> 6)));
			key8[7] = (byte)(key[6] << 1);
			for (int i = 0; i < key8.Length; i++) {
				key8[i] ^= (byte)(BitCount(key8[i] ^ 1) & 1);
			}
			
			
			return key8;
		}

		// public static int bitCount(int i) {
		// 	// HD, Figure 5-2
		// 	i = i - ((i >>> 1) & 0x55555555);
		// 	i = (i & 0x33333333) + ((i >>> 2) & 0x33333333);
		// 	i = (i + (i >>> 4)) & 0x0f0f0f0f;
		// 	i = i + (i >>> 8);
		// 	i = i + (i >>> 16);
		// 	return i & 0x3f;
		// }
		
		// public static int BitCount (int number)
		// {
		// 	number -= (number >> 1) & 0x55555555;
		// 	number = ((number >> 2) & 0x33333333) + (number & 0x33333333);
		// 	number = ((number >> 4) + number) & 0x0F0F0F0F;
		// 	number += number >> 8;
		// 	number += number >> 16;
		// 	return number & 0x0000003F;
		// }

		
		//TODO
		static int BitCount(int n)
		{
			n = (n & 0x55555555) + ((n >> 1) & 0x55555555);
			n = (n & 0x33333333) + ((n >> 2) & 0x33333333);
			n = (n & 0x0f0f0f0f) + ((n >> 4) & 0x0f0f0f0f);
			n = (n & 0x00ff00ff) + ((n >> 8) & 0x00ff00ff);
			return (n & 0x0000ffff) + ((n >> 16) & 0x0000ffff);
		}

	
		//TODO
		/*
		/// <summary>
		/// Default provider is BouncyCastleProvider.
		/// For registering custom provider </summary>
		/// <seealso cref= jcifs.util.Crypto#initProvider(Provider) </seealso>
		/// <returns> Provider </returns>
		public static Provider getProvider() {
			if (provider != null) {
				return provider;
			}
			provider = new BouncyCastleProvider();
			return provider;
		}

		/// <summary>
		/// Initialize Provider Instance with customProvider </summary>
		/// <param name="customProvider"> </param>
		/// <exception cref="Exception"> if Provider has already been initialized. </exception>
		/// throws jcifs.CIFSUnsupportedCryptoException
		public static void initProvider(Provider customProvider) {
			if (provider != null) {
				throw new CIFSUnsupportedCryptoException("Provider can't be re-initialized. Provider has already been initialized with " + provider.getInfo());
			}
			provider = customProvider;
		}*/
	}

}