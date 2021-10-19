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
	public class AvPair {

		/// <summary>
		/// EOL type
		/// </summary>
		public const int MsvAvEOL = 0x0;

		/// <summary>
		/// Flags type
		/// </summary>
		public const int MsvAvFlags = 0x6;

		/// <summary>
		/// Timestamp type
		/// </summary>
		public const int MsvAvTimestamp = 0x7;

		/// <summary>
		/// Single host type
		/// </summary>
		public const int MsvAvSingleHost = 0x08;

		/// <summary>
		/// Target name type
		/// </summary>
		public const int MsvAvTargetName = 0x09;

		/// <summary>
		/// Channel bindings type
		/// </summary>
		public const int MsvAvChannelBindings = 0x0A;

		private readonly int type;
		private readonly byte[] raw;


		/// <param name="type"> </param>
		/// <param name="raw"> </param>
		public AvPair(int type, byte[] raw) {
			this.type = type;
			this.raw = raw;
		}


		/// <returns> the type </returns>
		public int getType() {
			return this.type;
		}


		/// <returns> the raw </returns>
		public byte[] getRaw() {
			return this.raw;
		}

	}

}