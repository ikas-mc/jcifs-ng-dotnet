using System;
using Configuration = jcifs.Configuration;
using Strings = jcifs.util.Strings;

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



	internal class NodeStatusResponse : NameServicePacket {

		private NbtAddress queryAddress;

		private int numberOfNames;
		private byte[] macAddress;
		private byte[] stats;

		internal NbtAddress[] addressArray;


		/*
		 * It is a little awkward but prudent to pass the quering address
		 * so that it may be included in the list of results. IOW we do
		 * not want to create a new NbtAddress object for this particular
		 * address from which the query is constructed, we want to populate
		 * the data of the existing address that should be one of several
		 * returned by the node status.
		 */

		internal NodeStatusResponse(Configuration cfg, NbtAddress queryAddress) : base(cfg) {
			this.queryAddress = queryAddress;
			this.recordName = new Name(cfg);
			this.macAddress = new byte[6];
		}


		internal override int writeBodyWireFormat(byte[] dst, int dstIndex) {
			return 0;
		}


		internal override int readBodyWireFormat(byte[] src, int srcIndex) {
			return readResourceRecordWireFormat(src, srcIndex);
		}


		internal override int writeRDataWireFormat(byte[] dst, int dstIndex) {
			return 0;
		}


		internal override int readRDataWireFormat(byte[] src, int srcIndex) {
			int start = srcIndex;
			this.numberOfNames = src[srcIndex] & 0xFF;
			int namesLength = this.numberOfNames * 18;
			int statsLength = this.rDataLength - namesLength - 1;
			this.numberOfNames = src[srcIndex++] & 0xFF;
			// gotta read the mac first so we can populate addressArray with it
			Array.Copy(src, srcIndex + namesLength, this.macAddress, 0, 6);
			srcIndex += readNodeNameArray(src, srcIndex);
			this.stats = new byte[statsLength];
			Array.Copy(src, srcIndex, this.stats, 0, statsLength);
			srcIndex += statsLength;
			return srcIndex - start;
		}


		private int readNodeNameArray(byte[] src, int srcIndex) {
			int start = srcIndex;

			this.addressArray = new NbtAddress[this.numberOfNames];

			string n;
			int hexCode;
			string scope = this.queryAddress.hostName.scope;
			bool groupName;
			int ownerNodeType;
			bool isBeingDeleted;
			bool isInConflict;
			bool isActive;
			bool isPermanent;
			int j;
			bool addrFound = false;

			for (int i = 0; i < this.numberOfNames; srcIndex += 18, i++) {
				for (j = srcIndex + 14; src[j] == 0x20; j--) {
					;
				}
				n = Strings.fromOEMBytes(src, srcIndex, j - srcIndex + 1, this.config);
				hexCode = src[srcIndex + 15] & 0xFF;
				groupName = ((src[srcIndex + 16] & 0x80) == 0x80) ? true : false;
				ownerNodeType = (src[srcIndex + 16] & 0x60) >> 5;
				isBeingDeleted = ((src[srcIndex + 16] & 0x10) == 0x10) ? true : false;
				isInConflict = ((src[srcIndex + 16] & 0x08) == 0x08) ? true : false;
				isActive = ((src[srcIndex + 16] & 0x04) == 0x04) ? true : false;
				isPermanent = ((src[srcIndex + 16] & 0x02) == 0x02) ? true : false;

				/*
				 * The NbtAddress object used to query this node will be in the list
				 * returned by the Node Status. A new NbtAddress object should not be
				 * created for it because the original is potentially being actively
				 * referenced by other objects. We must populate the existing object's
				 * data explicitly (and carefully).
				 */
				if (!addrFound && this.queryAddress.hostName.hexCode == hexCode && (this.queryAddress.hostName.isUnknown() || this.queryAddress.hostName.name.Equals(n))) {

					if (this.queryAddress.hostName.isUnknown()) {
						this.queryAddress.hostName = new Name(this.config, n, hexCode, scope);
					}
					this.queryAddress.groupName = groupName;
					this.queryAddress.nodeType = ownerNodeType;
					this.queryAddress.isBeingDeletedField = isBeingDeleted;
					this.queryAddress.isInConflictField = isInConflict;
					this.queryAddress.isActiveField = isActive;
					this.queryAddress.isPermanentField = isPermanent;
					this.queryAddress.macAddress = this.macAddress;
					this.queryAddress.isDataFromNodeStatus = true;
					addrFound = true;
					this.addressArray[i] = this.queryAddress;
				}
				else {
					this.addressArray[i] = new NbtAddress(new Name(this.config, n, hexCode, scope), this.queryAddress.address, groupName, ownerNodeType, isBeingDeleted, isInConflict, isActive, isPermanent, this.macAddress);
				}
			}
			return srcIndex - start;
		}


		public override string ToString() {
			return "NodeStatusResponse[" + base.ToString() + "]";
		}
	}

}