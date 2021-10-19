using System;
using Encodable = jcifs.Encodable;

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
namespace jcifs.util {



	/// <summary>
	/// @author mbechler
	/// 
	/// </summary>
	public class ByteEncodable : Encodable {

		private byte[] bytes;
		private int off;
		private int len;


		/// <param name="b"> </param>
		/// <param name="off"> </param>
		/// <param name="len"> </param>
		public ByteEncodable(byte[] b, int off, int len) {
			this.bytes = b;
			this.off = off;
			this.len = len;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Encodable#size() </seealso>
		public virtual int size() {
			return this.len;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Encodable#encode(byte[], int) </seealso>
		public virtual int encode(byte[] dst, int dstIndex) {
			Array.Copy(this.bytes, this.off, dst, dstIndex, this.len);
			return this.len;
		}

	}

}