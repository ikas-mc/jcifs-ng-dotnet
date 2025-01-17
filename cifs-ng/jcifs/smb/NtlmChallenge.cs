using System;
using UniAddress = jcifs.netbios.UniAddress;
using Hexdump = jcifs.util.Hexdump;

/* jcifs smb client library in Java
 * Copyright (C) 2004  "Michael B. Allen" <jcifs at samba dot org>
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

namespace jcifs.smb {



	/// 
	/// <summary>
	/// @internal
	/// </summary>
	[Serializable]
	public sealed class NtlmChallenge {

		/// 
		private const long serialVersionUID = 2484853610174848092L;

		/// <summary>
		/// Challenge
		/// </summary>
		public byte[] challenge;

		/// <summary>
		/// Server address
		/// </summary>
		public UniAddress dc;


		/// <param name="challenge"> </param>
		/// <param name="dc"> </param>
		public NtlmChallenge(byte[] challenge, UniAddress dc) {
			this.challenge = challenge;
			this.dc = dc;
		}


		public override string ToString() {
			return "NtlmChallenge[challenge=0x" + Hexdump.toHexString(this.challenge, 0, this.challenge.Length * 2) + ",dc=" + this.dc.ToString() + "]";
		}
	}

}