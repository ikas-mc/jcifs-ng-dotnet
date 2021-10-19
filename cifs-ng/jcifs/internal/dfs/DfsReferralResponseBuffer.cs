using cifs_ng.lib.ext;
using Decodable = jcifs.Decodable;
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
namespace jcifs.@internal.dfs {



	/// <summary>
	/// @author mbechler
	/// 
	/// </summary>
	public class DfsReferralResponseBuffer : Decodable {

		private int pathConsumed;
		private int numReferrals;
		private int tflags;
		private Referral[] referrals;


		/// <returns> the pathConsumed </returns>
		public int getPathConsumed() {
			return this.pathConsumed;
		}


		/// <returns> the numReferrals </returns>
		public int getNumReferrals() {
			return this.numReferrals;
		}


		/// <returns> the tflags </returns>
		public int getTflags() {
			return this.tflags;
		}


		/// <returns> the referrals </returns>
		public Referral[] getReferrals() {
			return this.referrals;
		}


		public virtual int decode(byte[] buffer, int bufferIndex, int len) {
			int start = bufferIndex;

			this.pathConsumed = SMBUtil.readInt2(buffer, bufferIndex) / 2;
			bufferIndex += 2;
			this.numReferrals = SMBUtil.readInt2(buffer, bufferIndex);
			bufferIndex += 2;
			this.tflags = SMBUtil.readInt2(buffer, bufferIndex);
			bufferIndex += 4;

			this.referrals = new Referral[this.numReferrals];
			for (int ri = 0; ri < this.numReferrals; ri++) {
				this.referrals[ri] = new Referral();
				bufferIndex += this.referrals[ri].decode(buffer, bufferIndex, len);
			}

			return bufferIndex - start;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= java.lang.Object#toString() </seealso>
		public override string ToString() {
			return $"pathConsumed={this.pathConsumed},numReferrals={this.numReferrals},flags={this.tflags},referrals={this.referrals?.joinToString()}";
		}
	}

}