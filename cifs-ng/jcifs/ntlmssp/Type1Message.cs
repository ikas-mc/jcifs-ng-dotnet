using jcifs.util;

using System;
using System.IO;
using cifs_ng.lib.ext;
using CIFSContext = jcifs.CIFSContext;

/* jcifs smb client library in Java
 * Copyright (C) 2002  "Michael B. Allen" <jcifs at samba dot org>
 *                   "Eric Glass" <jcifs at samba dot org>
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

namespace jcifs.ntlmssp {



	/// <summary>
	/// Represents an NTLMSSP Type-1 message.
	/// </summary>
	public class Type1Message : NtlmMessage {

		private string suppliedDomain;
		private string suppliedWorkstation;


		/// <summary>
		/// Creates a Type-1 message using default values from the current
		/// environment.
		/// </summary>
		/// <param name="tc">
		///            context to use </param>
		public Type1Message(CIFSContext tc) : this(tc, getDefaultFlags(tc), tc.getConfig().getDefaultDomain(), tc.getNameServiceClient().getLocalHost().getHostName()) {
		}


		/// <summary>
		/// Creates a Type-1 message with the specified parameters.
		/// </summary>
		/// <param name="tc">
		///            context to use </param>
		/// <param name="flags">
		///            The flags to apply to this message. </param>
		/// <param name="suppliedDomain">
		///            The supplied authentication domain. </param>
		/// <param name="suppliedWorkstation">
		///            The supplied workstation name. </param>
		public Type1Message(CIFSContext tc, int flags, string suppliedDomain, string suppliedWorkstation) {
			setFlags(getDefaultFlags(tc) | flags);
			setSuppliedDomain(suppliedDomain);
			setSuppliedWorkstation(suppliedWorkstation);
		}


		/// <summary>
		/// Creates a Type-1 message using the given raw Type-1 material.
		/// </summary>
		/// <param name="material">
		///            The raw Type-1 material used to construct this message. </param>
		/// <exception cref="IOException">
		///             If an error occurs while parsing the material. </exception>
		/// throws java.io.IOException
		public Type1Message(byte[] material) {
			parse(material);
		}


		/// <summary>
		/// Returns the default flags for a generic Type-1 message in the
		/// current environment.
		/// </summary>
		/// <param name="tc">
		///            context to use </param>
		/// <returns> An <code>int</code> containing the default flags. </returns>
		public static int getDefaultFlags(CIFSContext tc) {
			return NtlmFlags.NTLMSSP_NEGOTIATE_NTLM | NtlmFlags.NTLMSSP_NEGOTIATE_VERSION | (tc.getConfig().isUseUnicode() ? NtlmFlags.NTLMSSP_NEGOTIATE_UNICODE : NtlmFlags.NTLMSSP_NEGOTIATE_OEM);
		}


		/// <summary>
		/// Returns the supplied authentication domain.
		/// </summary>
		/// <returns> A <code>String</code> containing the supplied domain. </returns>
		public virtual string getSuppliedDomain() {
			return this.suppliedDomain;
		}


		/// <summary>
		/// Sets the supplied authentication domain for this message.
		/// </summary>
		/// <param name="suppliedDomain">
		///            The supplied domain for this message. </param>
		public virtual void setSuppliedDomain(string suppliedDomain) {
			this.suppliedDomain = suppliedDomain;
		}


		/// <summary>
		/// Returns the supplied workstation name.
		/// </summary>
		/// <returns> A <code>String</code> containing the supplied workstation name. </returns>
		public virtual string getSuppliedWorkstation() {
			return this.suppliedWorkstation;
		}


		/// <summary>
		/// Sets the supplied workstation name for this message.
		/// </summary>
		/// <param name="suppliedWorkstation">
		///            The supplied workstation for this message. </param>
		public virtual void setSuppliedWorkstation(string suppliedWorkstation) {
			this.suppliedWorkstation = suppliedWorkstation;
		}


		public override byte[] toByteArray() {
			try {
				int flags = getFlags();
				int size = 8 * 4 + ((flags & NtlmFlags.NTLMSSP_NEGOTIATE_VERSION) != 0 ? 8 : 0);

				byte[] domain = new byte[0];
				string suppliedDomainString = getSuppliedDomain();
				if ((flags & NtlmFlags.NTLMSSP_NEGOTIATE_VERSION) == 0 && suppliedDomainString != null && suppliedDomainString.Length != 0) {
					flags |= NtlmFlags.NTLMSSP_NEGOTIATE_OEM_DOMAIN_SUPPLIED;
					domain = suppliedDomainString.ToUpper().getBytes(getOEMEncoding());
					size += domain.Length;
				}
				else {
					var x = unchecked((int) 0xffffffff);
					flags &= (NtlmFlags.NTLMSSP_NEGOTIATE_OEM_DOMAIN_SUPPLIED ^ x);
				}

				byte[] workstation = new byte[0];
				string suppliedWorkstationString = getSuppliedWorkstation();
				if ((flags & NtlmFlags.NTLMSSP_NEGOTIATE_VERSION) == 0 && suppliedWorkstationString != null && suppliedWorkstationString.Length != 0) {
					flags |= NtlmFlags.NTLMSSP_NEGOTIATE_OEM_WORKSTATION_SUPPLIED;
					workstation = suppliedWorkstationString.ToUpper().getBytes(getOEMEncoding());
					size += workstation.Length;
				}
				else {
					var x = unchecked((int) 0xffffffff);
					flags &= (NtlmFlags.NTLMSSP_NEGOTIATE_OEM_WORKSTATION_SUPPLIED ^ x);
				}

				byte[] type1 = new byte[size];
				int pos = 0;

				Array.Copy(NTLMSSP_SIGNATURE, 0, type1, 0, NTLMSSP_SIGNATURE.Length);
				pos += NTLMSSP_SIGNATURE.Length;

				writeULong(type1, pos, NTLMSSP_TYPE1);
				pos += 4;

				writeULong(type1, pos, flags);
				pos += 4;

				int domOffOff = writeSecurityBuffer(type1, pos, domain);
				pos += 8;

				int wsOffOff = writeSecurityBuffer(type1, pos, workstation);
				pos += 8;

				if ((flags & NtlmFlags.NTLMSSP_NEGOTIATE_VERSION) != 0) {
					Array.Copy(NTLMSSP_VERSION, 0, type1, pos, NTLMSSP_VERSION.Length);
					pos += NTLMSSP_VERSION.Length;
				}

				pos += writeSecurityBufferContent(type1, pos, domOffOff, domain);
				pos += writeSecurityBufferContent(type1, pos, wsOffOff, workstation);
				return type1;
			}
			catch (IOException ex) {
				throw new System.InvalidOperationException(ex.Message);
			}
		}


		public override string ToString() {
			string suppliedDomainString = getSuppliedDomain();
			string suppliedWorkstationString = getSuppliedWorkstation();
			return "Type1Message[suppliedDomain=" + (suppliedDomainString == null ? "null" : suppliedDomainString) + ",suppliedWorkstation=" + (suppliedWorkstationString == null ? "null" : suppliedWorkstationString) + ",flags=0x" + Hexdump.toHexString(getFlags(), 8) + "]";
		}


		/// throws java.io.IOException
		private void parse(byte[] material) {
			int pos = 0;
			for (int i = 0; i < 8; i++) {
				if (material[i] != NTLMSSP_SIGNATURE[i]) {
					throw new IOException("Not an NTLMSSP message.");
				}
			}
			pos += 8;

			if (readULong(material, pos) != NTLMSSP_TYPE1) {
				throw new IOException("Not a Type 1 message.");
			}
			pos += 4;

			int flags = readULong(material, pos);
			setFlags(flags);
			pos += 4;

			if ((flags & NtlmFlags.NTLMSSP_NEGOTIATE_OEM_DOMAIN_SUPPLIED) != 0) {
				byte[] domain = readSecurityBuffer(material, pos);
				setSuppliedDomain(domain.toString(getOEMEncoding()));
			}
			pos += 8;

			if ((flags & NtlmFlags.NTLMSSP_NEGOTIATE_OEM_WORKSTATION_SUPPLIED) != 0) {
				byte[] workstation = readSecurityBuffer(material, pos);
				setSuppliedWorkstation(workstation.toString(getOEMEncoding()));
			}
			pos += 8;
		}

	}

}