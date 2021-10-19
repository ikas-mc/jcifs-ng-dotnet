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



	internal class NameQueryRequest : NameServicePacket {

		internal NameQueryRequest(Configuration config, Name name) : base(config) {
			this.questionName = name;
			this.questionType = NB;
		}


		internal override int writeBodyWireFormat(byte[] dst, int dstIndex) {
			return writeQuestionSectionWireFormat(dst, dstIndex);
		}


		internal override int readBodyWireFormat(byte[] src, int srcIndex) {
			return readQuestionSectionWireFormat(src, srcIndex);
		}


		internal override int writeRDataWireFormat(byte[] dst, int dstIndex) {
			return 0;
		}


		internal override int readRDataWireFormat(byte[] src, int srcIndex) {
			return 0;
		}


		public override string ToString() {
			return "NameQueryRequest[" + base.ToString() + "]";
		}
	}

}