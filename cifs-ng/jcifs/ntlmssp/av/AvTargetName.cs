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

using System.Text;
using cifs_ng.lib.ext;
using jcifs.util;

namespace jcifs.ntlmssp.av {



	/// <summary>
	/// @author mbechler
	/// 
	/// </summary>
	public class AvTargetName : AvPair
	{

		/// 
		private static readonly Encoding UTF16LE = Strings.UTF_16LE_ENCODING;


		/// <param name="raw"> </param>
		public AvTargetName(byte[] raw) : base(AvPair.MsvAvTargetName, raw) {
		}


		/// 
		/// <param name="targetName"> </param>
		public AvTargetName(string targetName) : this(encode(targetName)) {
		}


		/// 
		/// <returns> the target name </returns>
		public virtual string getTargetName() {
			return UTF16LE.GetString(getRaw());
		}


		/// <param name="targetName">
		/// @return </param>
		private static byte[] encode(string targetName) {
			return targetName.getBytes(UTF16LE);
		}

	}

}