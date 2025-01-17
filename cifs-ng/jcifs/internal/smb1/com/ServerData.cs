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
namespace jcifs.@internal.smb1.com {

	public class ServerData {

		public byte sflags;
		public int sflags2;
		public int smaxMpxCount;
		public int maxBufferSize;
		public int sessKey;
		public int scapabilities;
		public string oemDomainName;
		public int securityMode;
		public int security;
		public bool encryptedPasswords;
		public bool signaturesEnabled;
		public bool signaturesRequired;
		public int maxNumberVcs;
		public int maxRawSize;
		public long serverTime;
		public int serverTimeZone;
		public int encryptionKeyLength;
		public byte[] encryptionKey;
		public byte[] guid;
	}
}