using System;
using Configuration = jcifs.Configuration;
using SMBProtocolDecodingException = jcifs.@internal.SMBProtocolDecodingException;
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
namespace jcifs.@internal.smb2.nego {



	/// <summary>
	/// @author mbechler
	/// 
	/// </summary>
	public class PreauthIntegrityNegotiateContext : NegotiateContextRequest, NegotiateContextResponse {

		/// <summary>
		/// Context type
		/// </summary>
		public const int NEGO_CTX_PREAUTH_TYPE = 0x1;

		/// <summary>
		/// SHA-512
		/// </summary>
		public const int HASH_ALGO_SHA512 = 0x1;

		private int[] hashAlgos;
		private byte[] salt;


		/// 
		/// <param name="config"> </param>
		/// <param name="hashAlgos"> </param>
		/// <param name="salt"> </param>
		public PreauthIntegrityNegotiateContext(Configuration config, int[] hashAlgos, byte[] salt) {
			this.hashAlgos = hashAlgos;
			this.salt = salt;
		}


		/// 
		public PreauthIntegrityNegotiateContext() {
		}


		/// <returns> the salt </returns>
		public virtual byte[] getSalt() {
			return this.salt;
		}


		/// <returns> the hashAlgos </returns>
		public virtual int[] getHashAlgos() {
			return this.hashAlgos;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb2.nego.NegotiateContextRequest#getContextType() </seealso>
		public virtual int getContextType() {
			return NEGO_CTX_PREAUTH_TYPE;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Encodable#encode(byte[], int) </seealso>
		public virtual int encode(byte[] dst, int dstIndex) {
			int start = dstIndex;

			SMBUtil.writeInt2(this.hashAlgos != null ? this.hashAlgos.Length : 0, dst, dstIndex);
			SMBUtil.writeInt2(this.salt != null ? this.salt.Length : 0, dst, dstIndex + 2);
			dstIndex += 4;

			if (this.hashAlgos != null) {
				foreach (int hashAlgo in this.hashAlgos) {
					SMBUtil.writeInt2(hashAlgo, dst, dstIndex);
					dstIndex += 2;
				}
			}

			if (this.salt != null) {
				Array.Copy(this.salt, 0, dst, dstIndex, this.salt.Length);
				dstIndex += this.salt.Length;
			}

			return dstIndex - start;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Decodable#decode(byte[], int, int) </seealso>
		/// throws jcifs.internal.SMBProtocolDecodingException
		public virtual int decode(byte[] buffer, int bufferIndex, int len) {
			int start = bufferIndex;
			int nalgos = SMBUtil.readInt2(buffer, bufferIndex);
			int nsalt = SMBUtil.readInt2(buffer, bufferIndex + 2);
			bufferIndex += 4;

			this.hashAlgos = new int[nalgos];
			for (int i = 0; i < nalgos; i++) {
				this.hashAlgos[i] = SMBUtil.readInt2(buffer, bufferIndex);
				bufferIndex += 2;
			}

			this.salt = new byte[nsalt];
			Array.Copy(buffer, bufferIndex, this.salt, 0, nsalt);
			bufferIndex += nsalt;

			return bufferIndex - start;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Encodable#size() </seealso>
		public virtual int size() {
			return 4 + (this.hashAlgos != null ? 2 * this.hashAlgos.Length : 0) + (this.salt != null ? this.salt.Length : 0);
		}

	}

}