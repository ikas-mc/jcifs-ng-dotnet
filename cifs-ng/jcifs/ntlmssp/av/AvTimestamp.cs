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
namespace jcifs.ntlmssp.av {



	/// <summary>
	/// @author mbechler
	/// 
	/// </summary>
	public class AvTimestamp : AvPair {

		/// <param name="raw"> </param>
		public AvTimestamp(byte[] raw) : base(AvPair.MsvAvTimestamp, raw) {
		}


		/// 
		/// <param name="ts"> </param>
		public AvTimestamp(long ts) : this(encode(ts)) {
		}


		/// <param name="ts">
		/// @return </param>
		private static byte[] encode(long ts) {
			byte[] data = new byte[8];
			SMBUtil.writeInt8(ts, data, 0);
			return data;
		}


		/// <returns> the timestamp </returns>
		public virtual long getTimestamp() {
			return SMBUtil.readInt8(getRaw(), 0);
		}

	}

}