using System;
using System.Collections.Generic;
using cifs_ng.lib.ext;
using Encdec = jcifs.util.Encdec;
using Strings = jcifs.util.Strings;

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

namespace jcifs.dcerpc.ndr {




	public class NdrBuffer {

		private int referent;
		private IDictionary<object, Entry> referents;

		private class Entry {

			public Entry(int referent, object obj) {
				this.referent = referent;
				this.obj = obj;
			}

			internal readonly int referent;

			internal readonly object obj;
		}

		public byte[] buf;
		public int start;
		public int index;
		public int length;
		public NdrBuffer deferred;


		public NdrBuffer(byte[] buf, int start) {
			this.buf = buf;
			this.start = this.index = start;
			this.length = 0;
			this.deferred = this;
		}


		public virtual NdrBuffer derive(int idx) {
			NdrBuffer nb = new NdrBuffer(this.buf, this.start);
			nb.index = idx;
			nb.deferred = this.deferred;
			return nb;
		}


		public virtual void reset() {
			this.index = this.start;
			this.length = 0;
			this.deferred = this;
		}


		public virtual int getIndex() {
			return this.index;
		}


		public virtual void setIndex(int index) {
			this.index = index;
		}


		public virtual int getCapacity() {
			return this.buf.Length - this.start;
		}


		public virtual int getTailSpace() {
			return this.buf.Length - this.index;
		}


		public virtual byte[] getBuffer() {
			return this.buf;
		}


		public virtual int align(int boundary, byte value) {
			int n = align(boundary);
			int i = n;
			while (i > 0) {
				this.buf[this.index - i] = value;
				i--;
			}
			return n;
		}


		public virtual void writeOctetArray(byte[] b, int i, int l) {
			Array.Copy(b, i, this.buf, this.index, l);
			advance(l);
		}


		public virtual void readOctetArray(byte[] b, int i, int l) {
			Array.Copy(this.buf, this.index, b, i, l);
			advance(l);
		}


		public virtual int getLength() {
			return this.deferred.length;
		}


		public virtual void setLength(int length) {
			this.deferred.length = length;
		}


		public virtual void advance(int n) {
			this.index += n;
			if ((this.index - this.start) > this.deferred.length) {
				this.deferred.length = this.index - this.start;
			}
		}


		public virtual int align(int boundary) {
			int m = boundary - 1;
			int i = this.index - this.start;
			int n = ((i + m) & ~m) - i;
			advance(n);
			return n;
		}


		public virtual void enc_ndr_small(int s) {
			this.buf[this.index] = unchecked((byte)(s & 0xFF));
			advance(1);
		}


		public virtual int dec_ndr_small() {
			int val = this.buf[this.index] & 0xFF;
			advance(1);
			return val;
		}


		public virtual void enc_ndr_short(int s) {
			align(2);
			Encdec.enc_uint16le((short) s, this.buf, this.index);
			advance(2);
		}


		public virtual int dec_ndr_short() {
			align(2);
			int val = Encdec.dec_uint16le(this.buf, this.index);
			advance(2);
			return val;
		}


		public virtual void enc_ndr_long(int l) {
			align(4);
			Encdec.enc_uint32le(l, this.buf, this.index);
			advance(4);
		}


		public virtual int dec_ndr_long() {
			align(4);
			int val = Encdec.dec_uint32le(this.buf, this.index);
			advance(4);
			return val;
		}


		public virtual void enc_ndr_hyper(long h) {
			align(8);
			Encdec.enc_uint64le(h, this.buf, this.index);
			advance(8);
		}


		public virtual long dec_ndr_hyper() {
			align(8);
			long val = Encdec.dec_uint64le(this.buf, this.index);
			advance(8);
			return val;
		}


		/* float */
		/* double */
		public virtual void enc_ndr_string(string s) {
			align(4);
			int i = this.index;
			int len = s.Length;
			Encdec.enc_uint32le(len + 1, this.buf, i);
			i += 4;
			Encdec.enc_uint32le(0, this.buf, i);
			i += 4;
			Encdec.enc_uint32le(len + 1, this.buf, i);
			i += 4;
			Array.Copy(Strings.getUNIBytes(s), 0, this.buf, i, len * 2);
			i += len * 2;
			this.buf[i++] = (byte) '\0';
			this.buf[i++] = (byte) '\0';
			advance(i - this.index);
		}


		/// throws NdrException
		public virtual string dec_ndr_string() {
			align(4);
			int i = this.index;
			string val = null;
			int len = Encdec.dec_uint32le(this.buf, i);
			i += 12;
			if (len != 0) {
				len--;
				int size = len * 2;
				if (size < 0 || size > 0xFFFF) {
					throw new NdrException(NdrException.INVALID_CONFORMANCE);
				}
				val = Strings.fromUNIBytes(this.buf, i, size);
				i += size + 2;
			}
			advance(i - this.index);
			return val;
		}


		private int getDceReferent(object obj) {
			Entry e;

			if (this.referents == null) {
				this.referents = new Dictionary<object, Entry>();
				this.referent = 1;
			}

			if ((e = this.referents.get(obj)) == null) {
				e = new Entry(this.referent++, obj);
				this.referents[obj] = e;
			}

			return e.referent;
		}


		public virtual void enc_ndr_referent(object obj, int type) {
			if (obj == null) {
				enc_ndr_long(0);
				return;
			}
			switch (type) {
			case 1: // unique
			case 3: // ref
				enc_ndr_long(RuntimeHelp.identityHashCode(obj));
				return;
			case 2: // ptr
				enc_ndr_long(getDceReferent(obj));
				return;
			}
		}


		public override string ToString() {
			return "start=" + this.start + ",index=" + this.index + ",length=" + getLength();
		}
	}

}