using System;
using System.Text;
using cifs_ng.lib.ext;
using Org.BouncyCastle.Crypto.Macs;
using DerivationParameters = Org.BouncyCastle.Crypto.IDerivationParameters;
using SHA256Digest = Org.BouncyCastle.Crypto.Digests.Sha256Digest;
using KDFCounterBytesGenerator = Org.BouncyCastle.Crypto.Generators.KdfCounterBytesGenerator;
using KDFCounterParameters = Org.BouncyCastle.Crypto.Parameters.KdfCounterParameters;

/*
 * © 2017 AgNO3 Gmbh & Co. KG
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
namespace jcifs.@internal.smb2
{
    /// <summary>
    /// SMB3 SP800-108 Counter Mode Key Derivation
    /// 
    /// @author mbechler
    /// 
    /// </summary>
    internal sealed class Smb3KeyDerivation
    {
        private static readonly byte[] SIGNCONTEXT_300 = toCBytes("SmbSign");
        private static readonly byte[] SIGNLABEL_300 = toCBytes("SMB2AESCMAC");
        private static readonly byte[] SIGNLABEL_311 = toCBytes("SMBSigningKey");

        private static readonly byte[] APPCONTEXT_300 = toCBytes("SmbRpc");
        private static readonly byte[] APPLABEL_300 = toCBytes("SMB2APP");
        private static readonly byte[] APPLABEL_311 = toCBytes("SMBAppKey");

        private static readonly byte[] ENCCONTEXT_300 = toCBytes("ServerIn "); // there really is a space there
        private static readonly byte[] ENCLABEL_300 = toCBytes("SMB2AESCCM");
        private static readonly byte[] ENCLABEL_311 = toCBytes("SMB2C2SCipherKey");

        private static readonly byte[] DECCONTEXT_300 = toCBytes("ServerOut");
        private static readonly byte[] DECLABEL_300 = toCBytes("SMB2AESCCM");
        private static readonly byte[] DECLABEL_311 = toCBytes("SMB2S2CCipherKey");


        /// 
        private Smb3KeyDerivation()
        {
        }


        /// 
        /// <param name="dialect"> </param>
        /// <param name="sessionKey"> </param>
        /// <param name="preauthIntegrity"> </param>
        /// <returns> derived signing key </returns>
        public static byte[] deriveSigningKey(int dialect, byte[] sessionKey, byte[] preauthIntegrity)
        {
            return derive(sessionKey, dialect == Smb2Constants.SMB2_DIALECT_0311 ? SIGNLABEL_311 : SIGNLABEL_300, dialect == Smb2Constants.SMB2_DIALECT_0311 ? preauthIntegrity : SIGNCONTEXT_300);
        }


        /// 
        /// <param name="dialect"> </param>
        /// <param name="sessionKey"> </param>
        /// <param name="preauthIntegrity"> </param>
        /// <returns> derived application key </returns>
        public static byte[] dervieApplicationKey(int dialect, byte[] sessionKey, byte[] preauthIntegrity)
        {
            return derive(sessionKey, dialect == Smb2Constants.SMB2_DIALECT_0311 ? APPLABEL_311 : APPLABEL_300, dialect == Smb2Constants.SMB2_DIALECT_0311 ? preauthIntegrity : APPCONTEXT_300);
        }


        /// 
        /// <param name="dialect"> </param>
        /// <param name="sessionKey"> </param>
        /// <param name="preauthIntegrity"> </param>
        /// <returns> derived encryption key </returns>
        public static byte[] deriveEncryptionKey(int dialect, byte[] sessionKey, byte[] preauthIntegrity)
        {
            return derive(sessionKey, dialect == Smb2Constants.SMB2_DIALECT_0311 ? ENCLABEL_311 : ENCLABEL_300, dialect == Smb2Constants.SMB2_DIALECT_0311 ? preauthIntegrity : ENCCONTEXT_300);
        }


        /// 
        /// <param name="dialect"> </param>
        /// <param name="sessionKey"> </param>
        /// <param name="preauthIntegrity"> </param>
        /// <returns> derived decryption key </returns>
        public static byte[] deriveDecryptionKey(int dialect, byte[] sessionKey, byte[] preauthIntegrity)
        {
            return derive(sessionKey, dialect == Smb2Constants.SMB2_DIALECT_0311 ? DECLABEL_311 : DECLABEL_300, dialect == Smb2Constants.SMB2_DIALECT_0311 ? preauthIntegrity : DECCONTEXT_300);
        }


        /// <param name="sessionKey"> </param>
        /// <param name="label"> </param>
        /// <param name="context"> </param>
        private static byte[] derive(byte[] sessionKey, byte[] label, byte[] context)
        {
            KDFCounterBytesGenerator gen = new KDFCounterBytesGenerator(new HMac(new SHA256Digest()));

            int r = 32;
            byte[] suffix = new byte[label.Length + context.Length + 5];
            // per bouncycastle
            // <li>1: K(i) := PRF( KI, [i]_2 || Label || 0x00 || Context || [L]_2 ) with the counter at the very beginning
            // of the fixedInputData (The default implementation has this format)</li>
            // with the parameters
            // <li>1. KDFCounterParameters(ki, null, "Label || 0x00 || Context || [L]_2]", 8);

            // all fixed inputs go into the suffix:
            // + label
            Array.Copy(label, 0, suffix, 0, label.Length);
            // + 1 byte 0x00
            // + context
            Array.Copy(context, 0, suffix, label.Length + 1, context.Length);
            // + 4 byte (== r bits) big endian encoding of L
            suffix[suffix.Length - 1] = unchecked((byte) 128);

            DerivationParameters param = new KDFCounterParameters(sessionKey, null, suffix, r);
            gen.Init(param);

            byte[] derived = new byte[16];
            gen.GenerateBytes(derived, 0, 16);
            return derived;
        }


        /// <param name="string"> </param>
        /// <returns> null terminated ASCII bytes </returns>
        private static byte[] toCBytes(string @string)
        {
            byte[] data = new byte[@string.Length + 1];
            Array.Copy(@string.getBytes(Encoding.ASCII), 0, data, 0, @string.Length);
            return data;
        }
    }
}