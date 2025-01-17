using System;
using Configuration = jcifs.Configuration;
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
	public class AvSingleHost : AvPair {

		/// <param name="raw"> </param>
		public AvSingleHost(byte[] raw) : base(AvPair.MsvAvSingleHost, raw) {
		}


		/// 
		/// <param name="cfg"> </param>
		public AvSingleHost(Configuration cfg) : this(new byte[8], cfg.getMachineId()) {
		}


		/// 
		/// <param name="customData"> </param>
		/// <param name="machineId"> </param>
		public AvSingleHost(byte[] customData, byte[] machineId) : this(encode(customData, machineId)) {
		}


		private static byte[] encode(byte[] customData, byte[] machineId) {
			int size = 8 + 8 + 32;
			byte[] enc = new byte[size];
			SMBUtil.writeInt4(size, enc, 0);
			SMBUtil.writeInt4(0, enc, 4);
			Array.Copy(customData, 0, enc, 8, 8);
			Array.Copy(machineId, 0, enc, 16, 32);
			return enc;
		}

	}

}