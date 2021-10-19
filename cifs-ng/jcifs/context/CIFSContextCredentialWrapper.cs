using System;
using CIFSContext = jcifs.CIFSContext;
using Credentials = jcifs.Credentials;
using CredentialsInternal = jcifs.smb.CredentialsInternal;
using NtlmAuthenticator = jcifs.smb.NtlmAuthenticator;
using NtlmPasswordAuthenticator = jcifs.smb.NtlmPasswordAuthenticator;
using SmbAuthException = jcifs.smb.SmbAuthException;
using SmbRenewableCredentials = jcifs.smb.SmbRenewableCredentials;

/*
 * © 2016 AgNO3 Gmbh & Co. KG
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
namespace jcifs.context {



	/// <summary>
	/// Context wrapper supplying alternate credentials
	/// 
	/// @author mbechler
	/// 
	/// </summary>
	public sealed class CIFSContextCredentialWrapper : CIFSContextWrapper, CIFSContext {

		private Credentials creds;


		/// <param name="delegate"> </param>
		/// <param name="creds">
		///            Crendentials to use </param>
		public CIFSContextCredentialWrapper(AbstractCIFSContext @delegate, Credentials creds) : base(@delegate) {
			this.creds = creds;
		}


		/// 
		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.context.CIFSContextWrapper#getCredentials() </seealso>
		public override Credentials getCredentials() {
			return this.creds;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.CIFSContext#renewCredentials(java.lang.String, java.lang.Throwable) </seealso>
		public override bool renewCredentials(string locationHint, Exception error) {
			Credentials cred = getCredentials();
			if (cred is SmbRenewableCredentials) {
				SmbRenewableCredentials renewable = (SmbRenewableCredentials) cred;
				CredentialsInternal renewed = renewable.renew();
				if (renewed != null) {
					this.creds = renewed;
					return true;
				}
			}
			NtlmAuthenticator auth = NtlmAuthenticator.getDefault();
			if (auth != null) {
				NtlmPasswordAuthenticator newAuth = NtlmAuthenticator.requestNtlmPasswordAuthentication(auth, locationHint, (error is SmbAuthException) ? (SmbAuthException) error : null);
				if (newAuth != null) {
					this.creds = newAuth;
					return true;
				}
			}
			return false;
		}
	}

}