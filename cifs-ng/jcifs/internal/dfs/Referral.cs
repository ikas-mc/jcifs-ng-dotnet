using System.Collections.Generic;
using Decodable = jcifs.Decodable;
using RuntimeCIFSException = jcifs.RuntimeCIFSException;
using Trans2GetDfsReferralResponse = jcifs.@internal.smb1.trans2.Trans2GetDfsReferralResponse;
using SMBUtil = jcifs.@internal.util.SMBUtil;
using Strings = jcifs.util.Strings;

/*
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




	/// 
	public class Referral : Decodable {

		internal int version;
		internal int size;
		internal int serverType;
		internal int rflags;
		internal int proximity;
		internal string altPath;

		internal int ttl;
		internal string rpath = null;
		internal string node = null;
		internal string specialName = null;

		internal string[] expandedNames = new string[0];


		/// <returns> the version </returns>
		public int getVersion() {
			return this.version;
		}


		/// <returns> the size </returns>
		public int getSize() {
			return this.size;
		}


		/// <returns> the serverType </returns>
		public int getServerType() {
			return this.serverType;
		}


		/// <returns> the rflags </returns>
		public int getRFlags() {
			return this.rflags;
		}


		/// <returns> the proximity </returns>
		public int getProximity() {
			return this.proximity;
		}


		/// <returns> the altPath </returns>
		public string getAltPath() {
			return this.altPath;
		}


		/// <returns> the ttl </returns>
		public int getTtl() {
			return this.ttl;
		}


		/// <returns> the rpath </returns>
		public string getRpath() {
			return this.rpath;
		}


		/// <returns> the node </returns>
		public string getNode() {
			return this.node;
		}


		/// <returns> the specialName </returns>
		public string getSpecialName() {
			return this.specialName;
		}


		/// <returns> the expandedNames </returns>
		public string[] getExpandedNames() {
			return this.expandedNames;
		}


		public virtual int decode(byte[] buffer, int bufferIndex, int len) {
			int start = bufferIndex;

			this.version = SMBUtil.readInt2(buffer, bufferIndex);
			if (this.version != 3 && this.version != 1) {
				throw new RuntimeCIFSException("Version " + this.version + " referral not supported. Please report this to jcifs at samba dot org.");
			}
			bufferIndex += 2;
			this.size = SMBUtil.readInt2(buffer, bufferIndex);
			bufferIndex += 2;
			this.serverType = SMBUtil.readInt2(buffer, bufferIndex);
			bufferIndex += 2;
			this.rflags = SMBUtil.readInt2(buffer, bufferIndex);
			bufferIndex += 2;
			if (this.version == 3) {
				this.proximity = SMBUtil.readInt2(buffer, bufferIndex);
				bufferIndex += 2;
				this.ttl = SMBUtil.readInt2(buffer, bufferIndex);
				bufferIndex += 2;

				if ((this.rflags & Trans2GetDfsReferralResponse.FLAGS_NAME_LIST_REFERRAL) == 0) {
					int pathOffset = SMBUtil.readInt2(buffer, bufferIndex);
					bufferIndex += 2;
					int altPathOffset = SMBUtil.readInt2(buffer, bufferIndex);
					bufferIndex += 2;
					int nodeOffset = SMBUtil.readInt2(buffer, bufferIndex);
					bufferIndex += 2;

					if (pathOffset > 0) {
						this.rpath = readString(buffer, start + pathOffset, len);
					}
					if (nodeOffset > 0) {
						this.node = readString(buffer, start + nodeOffset, len);
					}
					if (altPathOffset > 0) {
						this.altPath = readString(buffer, start + altPathOffset, len);
					}
				}
				else {
					int specialNameOffset = SMBUtil.readInt2(buffer, bufferIndex);
					bufferIndex += 2;
					int numExpanded = SMBUtil.readInt2(buffer, bufferIndex);
					bufferIndex += 2;
					int expandedNameOffset = SMBUtil.readInt2(buffer, bufferIndex);
					bufferIndex += 2;

					if (specialNameOffset > 0) {
						this.specialName = readString(buffer, start + specialNameOffset, len);
					}

					if (expandedNameOffset > 0) {
						IList<string> names = new List<string>();
						for (int i = 0; i < numExpanded; i++) {
							string en = readString(buffer, start + expandedNameOffset, len);
							names.Add(en);
							expandedNameOffset += en.Length * 2 + 2;
						}
						this.expandedNames = ((List<string>)names).ToArray();
					}

				}
			}
			else if (this.version == 1) {
				this.node = readString(buffer, bufferIndex, len);
			}

			return this.size;
		}


		private static string readString(byte[] buffer, int bufferIndex, int len) {
			// this is not absolutely correct, but we assume that the header is aligned
			if ((bufferIndex % 2) != 0) {
				bufferIndex++;
			}
			return Strings.fromUNIBytes(buffer, bufferIndex, Strings.findUNITermination(buffer, bufferIndex, len));
		}


		public override string ToString() {
			return "Referral[" + "version=" + this.version + ",size=" + this.size + ",serverType=" + this.serverType + ",flags=" + this.rflags + ",proximity=" + this.proximity + ",ttl=" + this.ttl + ",path=" + this.rpath + ",altPath=" + this.altPath + ",node=" + this.node + "]";
		}
	}
}