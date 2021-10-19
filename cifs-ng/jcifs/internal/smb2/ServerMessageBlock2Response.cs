using System;
using System.Threading;
using jcifs.util.transport;
using Configuration = jcifs.Configuration;
using CommonServerMessageBlockRequest = jcifs.@internal.CommonServerMessageBlockRequest;
using CommonServerMessageBlockResponse = jcifs.@internal.CommonServerMessageBlockResponse;
using SMBProtocolDecodingException = jcifs.@internal.SMBProtocolDecodingException;
using NtStatus = jcifs.smb.NtStatus;

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
namespace jcifs.@internal.smb2 {



	/// <summary>
	/// @author mbechler
	/// 
	/// </summary>
	public abstract class ServerMessageBlock2Response : ServerMessageBlock2, CommonServerMessageBlockResponse {

		private bool receivedField;
		private bool errorField;
		private long? expiration;

		private bool verifyFailed;
		private Exception exceptionField;
		private bool asyncHandled;


		/// <param name="config"> </param>
		/// <param name="command"> </param>
		public ServerMessageBlock2Response(Configuration config, int command) : base(config, command) {
		}


		/// <param name="config"> </param>
		public ServerMessageBlock2Response(Configuration config) : base(config) {
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.CommonServerMessageBlockResponse#getNextResponse() </seealso>
		public virtual CommonServerMessageBlockResponse getNextResponse() {
			return (CommonServerMessageBlockResponse) getNext();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.CommonServerMessageBlockResponse#prepare(jcifs.internal.CommonServerMessageBlockRequest) </seealso>
		public virtual void prepare(CommonServerMessageBlockRequest next) {
			CommonServerMessageBlockResponse n = getNextResponse();
			if (n != null) {
				n.prepare(next);
			}
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb2.ServerMessageBlock2#reset() </seealso>
		public override void reset() {
			base.reset();
			this.receivedField = false;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.util.transport.Response#received() </seealso>
		public virtual void received() {
			if (isAsync() && getStatus() == NtStatus.NT_STATUS_PENDING) {
				lock (this) {
					Monitor.PulseAll(this);
				}
				return;
			}
			this.receivedField = true;
			lock (this) {
				Monitor.PulseAll(this);
			}
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.util.transport.Response#exception(java.lang.Exception) </seealso>
		public virtual void exception(Exception e) {
			this.errorField = true;
			this.exceptionField = e;
			this.receivedField = true;
			lock (this) {
				Monitor.PulseAll(this);
			}
		}

		Response Response.getNextResponse()
		{
			return getNextResponse();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.util.transport.Response#error() </seealso>
		public virtual void error() {
			this.errorField = true;
			lock (this) {
				Monitor.PulseAll(this);
			}
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.util.transport.Response#clearReceived() </seealso>
		public virtual void clearReceived() {
			this.receivedField = false;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.util.transport.Response#isReceived() </seealso>
		public virtual bool isReceived() {
			return this.receivedField;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.util.transport.Response#isError() </seealso>
		public virtual bool isError() {
			return this.errorField;
		}


		/// <returns> whether the packet has been signed. </returns>
		public virtual bool isSigned() {
			return (getFlags() & SMB2_FLAGS_SIGNED) != 0;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.util.transport.Response#getExpiration() </seealso>
		public virtual long? getExpiration() {
			return this.expiration;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.util.transport.Response#setExpiration(java.lang.Long) </seealso>
		public virtual void setExpiration(long? exp) {
			this.expiration = exp;
		}


		/// <returns> whether the interim response has been handled </returns>
		public virtual bool isAsyncHandled() {
			return this.asyncHandled;
		}


		/// <param name="asyncHandled">
		///            the asyncHandled to set </param>
		public virtual void setAsyncHandled(bool asyncHandled) {
			this.asyncHandled = asyncHandled;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.util.transport.Response#getException() </seealso>
		public virtual Exception getException() {
			return this.exceptionField;
		}


		/// 
		/// <returns> error status code </returns>
		public virtual int getErrorCode() {
			return getStatus();
		}


		/// 
		/// <returns> whether signature verification failed </returns>
		public virtual bool isVerifyFailed() {
			return this.verifyFailed;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.util.transport.Response#getGrantedCredits() </seealso>
		public virtual int getGrantedCredits() {
			return getCredit();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <exception cref="SMBProtocolDecodingException">
		/// </exception>
		/// <seealso cref= jcifs.internal.smb2.ServerMessageBlock2#haveResponse(byte[], int, int) </seealso>
		/// throws jcifs.internal.SMBProtocolDecodingException
		protected  override void haveResponse(byte[] buffer, int start, int len) {
			if (isRetainPayload()) {
				byte[] payload = new byte[len];
				Array.Copy(buffer, start, payload, 0, len);
				setRawPayload(payload);
			}

			if (!verifySignature(buffer, start, len)) {
				throw new SMBProtocolDecodingException("Signature verification failed for " + this.GetType().FullName);
			}

			setAsyncHandled(false);
			received();
		}


		/// 
		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.util.transport.Response#verifySignature(byte[], int, int) </seealso>
		public virtual bool verifySignature(byte[] buffer, int i, int size) {
			// observed too that signatures on error responses are sometimes wrong??
			// Looks like the failure case also is just reflecting back the signature we sent

			// with SMB3's negotiation validation it's no longer possible to ignore this (on the validation response)
			// make sure that validation is performed in any case
			Smb2SigningDigest dgst = (Smb2SigningDigest)getDigest();
			if (dgst != null && !isAsync() && (getConfig().isRequireSecureNegotiate() || getErrorCode() == NtStatus.NT_STATUS_OK)) {
				// TODO: SMB2 - do we need to check the MIDs?
				// We only read what we were waiting for, so first guess would be no.
				bool verify = dgst.verify(buffer, i, size, 0, this);
				this.verifyFailed = verify;
				return !verify;
			}
			return true;
		}

	}

}