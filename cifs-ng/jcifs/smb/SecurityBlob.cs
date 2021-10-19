using System;

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
namespace jcifs.smb {

	/// <summary>
	/// This class represents the Secrity_Blob in SMB Block and is set to support
	/// kerberos authentication.
	/// 
	/// @author Shun
	/// 
	/// </summary>
	internal class SecurityBlob : ICloneable{

		private byte[] b = new byte[0];


		internal SecurityBlob() {
		}


		internal SecurityBlob(byte[] b) {
			set(b);
		}


		internal virtual void set(byte[] b) {
			this.b = b == null ? new byte[0] : b;
		}


		internal virtual byte[] get() {
			return this.b;
		}


		internal virtual int length() {
			if (this.b == null) {
				return 0;
			}
			return this.b.Length;
		}


		/*
		 * (non-Javadoc)
		 * 
		 * @see java.lang.Object#clone()
		 */
		/// throws CloneNotSupportedException
		public   object Clone() {
			return new SecurityBlob((byte[])this.b.Clone());
		}


		/*
		 * (non-Javadoc)
		 * 
		 * @see java.lang.Object#equals(java.lang.Object)
		 */
		public override bool Equals(object arg0) {
			try {
				SecurityBlob t = (SecurityBlob) arg0;
				for (int i = 0; i < this.b.Length; i++) {
					if (this.b[i] != t.b[i]) {
						return false;
					}
				}
				return true;
			}
			catch (Exception) {
				return false;
			}
		}


		/*
		 * (non-Javadoc)
		 * 
		 * @see java.lang.Object#hashCode()
		 */
		public override int GetHashCode() {
			return this.b.GetHashCode();
		}


		/*
		 * (non-Javadoc)
		 * 
		 * @see java.lang.Object#toString()
		 */
		public override string ToString() {
			string ret = "";
			for (int i = 0; i < this.b.Length; i++) {
				int n = this.b[i] & 0xff;
				if (n <= 0x0f) {
					ret += "0";
				}
				ret += n.ToString("x");
			}
			return ret;
		}
	}

}