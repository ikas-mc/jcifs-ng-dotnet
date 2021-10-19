using NdrBuffer = jcifs.dcerpc.ndr.NdrBuffer;
using NdrException = jcifs.dcerpc.ndr.NdrException;
using NdrObject = jcifs.dcerpc.ndr.NdrObject;

/* jcifs msrpc client library in Java
 * Copyright (C) 2006  "Michael B. Allen" <jcifs at samba dot org>
 *                     "Eric Glass" <jcifs at samba dot org>
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

namespace jcifs.dcerpc {



	/// 
	public abstract class DcerpcMessage : NdrObject {

		protected internal int ptype = -1;
		protected internal int flags = 0;
		protected internal int length = 0;
		protected internal int call_id = 0;
		protected internal int alloc_hint = 0;
		protected internal int result = 0;


		/// 
		/// <param name="flag"> </param>
		/// <returns> whether flag is set </returns>
		public virtual bool isFlagSet(int flag) {
			return (this.flags & flag) == flag;
		}


		/// <summary>
		/// Remove flag
		/// </summary>
		/// <param name="flag"> </param>
		public virtual void unsetFlag(int flag) {
			this.flags &= ~flag;
		}


		/// <summary>
		/// Set flag
		/// </summary>
		/// <param name="flag"> </param>
		public virtual void setFlag(int flag) {
			this.flags |= flag;
		}


		/// 
		/// <returns> result exception, if the call failed </returns>
		public virtual DcerpcException getResult() {
			if (this.result != 0) {
				return new DcerpcException(this.result);
			}
			return null;
		}


		internal virtual void encode_header(NdrBuffer buf) {
			buf.enc_ndr_small(5); // RPC version
			buf.enc_ndr_small(0); // minor version
			buf.enc_ndr_small(this.ptype);
			buf.enc_ndr_small(this.flags);
			buf.enc_ndr_long(0x00000010); // Little-endian / ASCII / IEEE
			buf.enc_ndr_short(this.length);
			buf.enc_ndr_short(0); // length of auth_value
			buf.enc_ndr_long(this.call_id);
		}


		/// throws jcifs.dcerpc.ndr.NdrException
		internal virtual void decode_header(NdrBuffer buf) {
			/* RPC major / minor version */
			if (buf.dec_ndr_small() != 5 || buf.dec_ndr_small() != 0) {
				throw new NdrException("DCERPC version not supported");
			}
			this.ptype = buf.dec_ndr_small();
			this.flags = buf.dec_ndr_small();
			if (buf.dec_ndr_long() != 0x00000010) {
				throw new NdrException("Data representation not supported");
			}
			this.length = buf.dec_ndr_short();
			if (buf.dec_ndr_short() != 0) {
				throw new NdrException("DCERPC authentication not supported");
			}
			this.call_id = buf.dec_ndr_long();
		}


		/// throws jcifs.dcerpc.ndr.NdrException
		public override void encode(NdrBuffer buf) {
			int start = buf.getIndex();
			int alloc_hint_index = 0;

			buf.advance(16); // momentarily skip header
			if (this.ptype == 0) {
				alloc_hint_index = buf.getIndex();
				buf.enc_ndr_long(0); // momentarily skip alloc hint
				buf.enc_ndr_short(0); // context id
				buf.enc_ndr_short(getOpnum());
			}

			encode_in(buf);
			this.length = buf.getIndex() - start;

			if (this.ptype == 0) {
				buf.setIndex(alloc_hint_index);
				this.alloc_hint = this.length - alloc_hint_index;
				buf.enc_ndr_long(this.alloc_hint);
			}

			buf.setIndex(start);
			encode_header(buf);
			buf.setIndex(start + this.length);
		}


		/// throws jcifs.dcerpc.ndr.NdrException
		public override void decode(NdrBuffer buf) {
			decode_header(buf);

			if (this.ptype != 12 && this.ptype != 2 && this.ptype != 3 && this.ptype != 13) {
				throw new NdrException("Unexpected ptype: " + this.ptype);
			}

			if (this.ptype == 2 || this.ptype == 3) {
				this.alloc_hint = buf.dec_ndr_long();
				buf.dec_ndr_short(); // context id
				buf.dec_ndr_short(); // cancel count
			}
			if (this.ptype == 3 || this.ptype == 13) {
				this.result = buf.dec_ndr_long();
			}
			else {
				decode_out(buf);
			}
		}


		/// 
		/// <returns> the operation number </returns>
		public abstract int getOpnum();


		/// 
		/// <param name="buf"> </param>
		/// <exception cref="NdrException"> </exception>
		/// throws jcifs.dcerpc.ndr.NdrException;
		public abstract void encode_in(NdrBuffer buf);


		/// 
		/// <param name="buf"> </param>
		/// <exception cref="NdrException"> </exception>
		/// throws jcifs.dcerpc.ndr.NdrException;
		public abstract void decode_out(NdrBuffer buf);
	}

}