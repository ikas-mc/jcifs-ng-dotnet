using Configuration = jcifs.Configuration;

/* jcifs smb client library in Java
 * Copyright (C) 2000  "Michael B. Allen" <jcifs at samba dot org>
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

namespace jcifs.netbios {



	internal class NodeStatusRequest : NameServicePacket {

		internal NodeStatusRequest(Configuration cfg, Name name) : base(cfg) {
			this.questionName = name;
			this.questionType = NBSTAT;
			this.isRecurDesired = false;
			this.isBroadcast = false;
		}


		internal override int writeBodyWireFormat(byte[] dst, int dstIndex) {
			int tmp = this.questionName.hexCode;
			this.questionName.hexCode = 0x00; // type has to be 0x00 for node status
			int result = writeQuestionSectionWireFormat(dst, dstIndex);
			this.questionName.hexCode = tmp;
			return result;
		}


		internal override int readBodyWireFormat(byte[] src, int srcIndex) {
			return 0;
		}


		internal override int writeRDataWireFormat(byte[] dst, int dstIndex) {
			return 0;
		}


		internal override int readRDataWireFormat(byte[] src, int srcIndex) {
			return 0;
		}


		public override string ToString() {
			return "NodeStatusRequest[" + base.ToString() + "]";
		}
	}

}