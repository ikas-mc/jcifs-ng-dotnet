using Configuration = jcifs.Configuration;
using DfsReferralResponseBuffer = jcifs.@internal.dfs.DfsReferralResponseBuffer;
using SmbComTransaction = jcifs.@internal.smb1.trans.SmbComTransaction;
using SmbComTransactionResponse = jcifs.@internal.smb1.trans.SmbComTransactionResponse;

/* jcifs smb client library in Java
 * Copyright (C) 2003  "Michael B. Allen" <jcifs at samba dot org>
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

namespace jcifs.@internal.smb1.trans2 {



	/// 
	public class Trans2GetDfsReferralResponse : SmbComTransactionResponse {

		/// 
		public const int FLAGS_NAME_LIST_REFERRAL = 0x0002;
		/// 
		public const int FLAGS_TARGET_SET_BOUNDARY = 0x0004;
		/// 
		public const int TYPE_ROOT_TARGETS = 0x0;
		/// 
		public const int TYPE_NON_ROOT_TARGETS = 0x1;

		private readonly DfsReferralResponseBuffer dfsResponse = new DfsReferralResponseBuffer();


		/// 
		/// <param name="config"> </param>
		public Trans2GetDfsReferralResponse(Configuration config) : base(config) {
			this.setSubCommand(SmbComTransaction.TRANS2_GET_DFS_REFERRAL);
		}


		/// <returns> the buffer </returns>
		public virtual DfsReferralResponseBuffer getDfsResponse() {
			return this.dfsResponse;
		}


		public override bool isForceUnicode() {
			return true;
		}


		protected internal override int writeSetupWireFormat(byte[] dst, int dstIndex) {
			return 0;
		}


		protected internal override int writeParametersWireFormat(byte[] dst, int dstIndex) {
			return 0;
		}


		protected internal override int writeDataWireFormat(byte[] dst, int dstIndex) {
			return 0;
		}


		protected internal override int readSetupWireFormat(byte[] buffer, int bufferIndex, int len) {
			return 0;
		}


		protected internal override int readParametersWireFormat(byte[] buffer, int bufferIndex, int len) {
			return 0;
		}


		protected internal override int readDataWireFormat(byte[] buffer, int bufferIndex, int len) {
			int start = bufferIndex;
			bufferIndex += this.dfsResponse.decode(buffer, bufferIndex, len);
			return bufferIndex - start;
		}


		public override string ToString() {
			return "Trans2GetDfsReferralResponse[" + base.ToString() + ",buffer=" + this.dfsResponse + "]";
		}
	}

}