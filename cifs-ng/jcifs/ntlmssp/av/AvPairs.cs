using System;
using System.Collections.Generic;
using System.Linq;
using cifs_ng.lib.ext;
using CIFSException = jcifs.CIFSException;
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
	public sealed class AvPairs {

		private AvPairs() {
		}


		/// <summary>
		/// Decode a list of AvPairs
		/// </summary>
		/// <param name="data"> </param>
		/// <returns> individual pairs </returns>
		/// <exception cref="CIFSException"> </exception>
		/// throws jcifs.CIFSException
		public static IList<AvPair> decode(byte[] data) {
			List<AvPair> pairs = new List<AvPair>();
			int pos = 0;
			bool foundEnd = false;
			while (pos + 4 <= data.Length) {
				int avId = SMBUtil.readInt2(data, pos);
				int avLen = SMBUtil.readInt2(data, pos + 2);
				pos += 4;

				if (avId == AvPair.MsvAvEOL) {
					if (avLen != 0) {
						throw new CIFSException("Invalid avLen for AvEOL");
					}
					foundEnd = true;
					break;
				}

				byte[] raw = new byte[avLen];
				Array.Copy(data, pos, raw, 0, avLen);
				pairs.Add(parseAvPair(avId, raw));

				pos += avLen;
			}
			if (!foundEnd) {
				throw new CIFSException("Missing AvEOL");
			}
			return pairs;
		}


		/// 
		/// <param name="pairs"> </param>
		/// <param name="type"> </param>
		/// <returns> whether the list contains a pair of that type </returns>
		public static bool contains(IList<AvPair> pairs, int type) {
			if (pairs == null) {
				return false;
			}
			foreach (AvPair p in pairs) {
				if (p.getType() == type) {
					return true;
				}
			}
			return false;
		}


		/// 
		/// <param name="pairs"> </param>
		/// <param name="type"> </param>
		/// <returns> first occurance of the given type </returns>
		public static AvPair get(IList<AvPair> pairs, int type) {
			IEnumerator<AvPair> it = pairs.GetEnumerator();
			while (it.MoveNext()) {
				AvPair p = it.Current;
				if (p.getType() == type) {
					return p;
				}
			}
			return null;
		}


		/// <summary>
		/// Remove all occurances of the given type
		/// </summary>
		/// <param name="pairs"> </param>
		/// <param name="type"> </param>
		public static void remove(IList<AvPair> pairs, int type)
		{
			//TODO 
			pairs.RemoveAll(x => x.getType() == type);
		}


		/// <summary>
		/// Replace all occurances of the given type
		/// </summary>
		/// <param name="pairs"> </param>
		/// <param name="rep"> </param>
		public static void replace(IList<AvPair> pairs, AvPair rep) {
			remove(pairs, rep.getType());
			pairs.Add(rep);
		}


		/// 
		/// <param name="pairs"> </param>
		/// <returns> encoded avpairs </returns>
		public static byte[] encode(IList<AvPair> pairs) {
			int size = 0;
			foreach (AvPair p in pairs) {
				size += 4 + p.getRaw().Length;
			}
			size += 4;

			byte[] enc = new byte[size];
			int pos = 0;
			foreach (AvPair p in pairs) {
				byte[] raw = p.getRaw();
				SMBUtil.writeInt2(p.getType(), enc, pos);
				SMBUtil.writeInt2(raw.Length, enc, pos + 2);
				Array.Copy(raw, 0, enc, pos + 4, raw.Length);
				pos += 4 + raw.Length;
			}

			// MsvAvEOL
			SMBUtil.writeInt2(AvPair.MsvAvEOL, enc, pos);
			SMBUtil.writeInt2(0, enc, pos + 2);
			pos += 4;
			return enc;
		}


		private static AvPair parseAvPair(int avId, byte[] raw) {
			switch (avId) {
			case AvPair.MsvAvFlags:
				return new AvFlags(raw);
			case AvPair.MsvAvTimestamp:
				return new AvTimestamp(raw);
			case AvPair.MsvAvTargetName:
				return new AvTargetName(raw);
			case AvPair.MsvAvSingleHost:
				return new AvSingleHost(raw);
			case AvPair.MsvAvChannelBindings:
				return new AvChannelBindings(raw);
			default:
				return new AvPair(avId, raw);
			}
		}
	}

}