using System;
using cifs_ng.lib.security;
using jcifs.lib;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Crypto = jcifs.util.Crypto;
using Logger = org.slf4j.Logger;
using LoggerFactory = org.slf4j.LoggerFactory;
using CommonServerMessageBlock = jcifs.@internal.CommonServerMessageBlock;
using SMBSigningDigest = jcifs.@internal.SMBSigningDigest;
using SMBUtil = jcifs.@internal.util.SMBUtil;

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
namespace jcifs.@internal.smb2 {






	/// <summary>
	/// @author mbechler
	/// 
	/// </summary>
	public class Smb2SigningDigest : SMBSigningDigest {

		private static readonly Logger log = LoggerFactory.getLogger(typeof(Smb2SigningDigest));

		/// 
		private const int SIGNATURE_OFFSET = 48;
		private const int SIGNATURE_LENGTH = 16;
		private readonly IMac digest;

		//out size
		private int size;

		/// <param name="sessionKey"> </param>
		/// <param name="dialect"> </param>
		/// <param name="preauthIntegrityHash"> </param>
		/// <exception cref="GeneralSecurityException">
		///  </exception>
		/// throws java.security.GeneralSecurityException
		public Smb2SigningDigest(byte[] sessionKey, int dialect, byte[] preauthIntegrityHash) {
			IMac m;
			byte[] signingKey;
			switch (dialect) {
			case Smb2Constants.SMB2_DIALECT_0202:
			case Smb2Constants.SMB2_DIALECT_0210:
				m = new HMac(new Sha256Digest());// Mac.getInstance("HmacSHA256");
				size = m.GetMacSize();
				signingKey = sessionKey;
				break;
			case Smb2Constants.SMB2_DIALECT_0300:
			case Smb2Constants.SMB2_DIALECT_0302:
				signingKey = Smb3KeyDerivation.deriveSigningKey(dialect, sessionKey, new byte[0]);
				m =new CMac(new AesEngine());// Mac.getInstance("AESCMAC", Crypto.getProvider());
				size = m.GetMacSize();
				break;
			case Smb2Constants.SMB2_DIALECT_0311:
				if (preauthIntegrityHash == null) {
					throw new System.ArgumentException("Missing preauthIntegrityHash for SMB 3.1");
				}
				signingKey = Smb3KeyDerivation.deriveSigningKey(dialect, sessionKey, preauthIntegrityHash);
				m =new CMac(new AesEngine());//m = Mac.getInstance("AESCMAC", Crypto.getProvider());
				size = m.GetMacSize();
				break;
			default:
				throw new System.ArgumentException("Unknown dialect");
			}

			//TODO
			var p= new KeyParameter(signingKey);
			m.Init(p);
			//m.Init(new SecretKeySpec(signingKey, "HMAC"));
			this.digest = m;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.SMBSigningDigest#sign(byte[], int, int, jcifs.internal.CommonServerMessageBlock,
		///      jcifs.internal.CommonServerMessageBlock) </seealso>
		public virtual void sign(byte[] data, int offset, int length, CommonServerMessageBlock request, CommonServerMessageBlock response) {
			lock (this) {
				this.digest.Reset();
        
				// zero out signature field
				int index = offset + SIGNATURE_OFFSET;
				for (int i = 0; i < SIGNATURE_LENGTH; i++) {
					data[index + i] = 0;
				}
        
				// set signed flag
				int oldFlags = SMBUtil.readInt4(data, offset + 16);
				int flags = oldFlags | ServerMessageBlock2.SMB2_FLAGS_SIGNED;
				SMBUtil.writeInt4(flags, data, offset + 16);
        
				this.digest.BlockUpdate(data, offset, length);
				
				//TODO 1 length
				byte[] sig = new byte[size];
				this.digest.DoFinal(sig,0);
				Array.Copy(sig, 0, data, offset + SIGNATURE_OFFSET, SIGNATURE_LENGTH);
			}
		}


		/// 
		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.SMBSigningDigest#verify(byte[], int, int, int, jcifs.internal.CommonServerMessageBlock) </seealso>
		public virtual bool verify(byte[] data, int offset, int length, int extraPad, CommonServerMessageBlock msg) {
			lock (this) {
				this.digest.Reset();
        
				int flags = SMBUtil.readInt4(data, offset + 16);
				if ((flags & ServerMessageBlock2.SMB2_FLAGS_SIGNED) == 0) {
					log.error("The server did not sign a message we expected to be signed");
					return true;
				}
        
				byte[] sig = new byte[SIGNATURE_LENGTH];
				Array.Copy(data, offset + SIGNATURE_OFFSET, sig, 0, SIGNATURE_LENGTH);
        
				int index = offset + SIGNATURE_OFFSET;
				for (int i = 0; i < SIGNATURE_LENGTH; i++) {
					data[index + i] = 0;
				}
        
				this.digest.BlockUpdate(data, offset, length);
        
				byte[] cmp = new byte[SIGNATURE_LENGTH];
				
				
				//TODO 1 Length
				byte[] r = new byte[size];
				this.digest.DoFinal(r,0);
				
				Array.Copy(r ,0, cmp, 0, SIGNATURE_LENGTH);
				if (!MessageDigest.isEqual(sig, cmp)) {
					return true;
				}
				return false;
			}
		}

	}

}