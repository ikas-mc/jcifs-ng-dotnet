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
	public class EncryptionNegotiateContext : NegotiateContextRequest, NegotiateContextResponse {

		/// <summary>
		/// Context type
		/// </summary>
		public const int NEGO_CTX_ENC_TYPE = 0x2;

		/// <summary>
		/// AES 128 CCM
		/// </summary>
		public const int CIPHER_AES128_CCM = 0x1;

		/// <summary>
		/// AES 128 GCM
		/// </summary>
		public const int CIPHER_AES128_GCM = 0x2;

		private int[] ciphers;


		/// 
		/// <param name="config"> </param>
		/// <param name="ciphers"> </param>
		public EncryptionNegotiateContext(Configuration config, int[] ciphers) {
			this.ciphers = ciphers;
		}


		/// 
		public EncryptionNegotiateContext() {
		}


		/// <returns> the ciphers </returns>
		public virtual int[] getCiphers() {
			return this.ciphers;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb2.nego.NegotiateContextRequest#getContextType() </seealso>
		public virtual int getContextType() {
			return NEGO_CTX_ENC_TYPE;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Encodable#encode(byte[], int) </seealso>
		public virtual int encode(byte[] dst, int dstIndex) {
			int start = dstIndex;
			SMBUtil.writeInt2(this.ciphers != null ? this.ciphers.Length : 0, dst, dstIndex);
			dstIndex += 2;

			if (this.ciphers != null) {
				foreach (int cipher in this.ciphers) {
					SMBUtil.writeInt2(cipher, dst, dstIndex);
					dstIndex += 2;
				}
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
			int nciphers = SMBUtil.readInt2(buffer, bufferIndex);
			bufferIndex += 2;

			this.ciphers = new int[nciphers];
			for (int i = 0; i < nciphers; i++) {
				this.ciphers[i] = SMBUtil.readInt2(buffer, bufferIndex);
				bufferIndex += 2;
			}

			return bufferIndex - start;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Encodable#size() </seealso>
		public virtual int size() {
			return 4 + (this.ciphers != null ? 2 * this.ciphers.Length : 0);
		}

	}

}