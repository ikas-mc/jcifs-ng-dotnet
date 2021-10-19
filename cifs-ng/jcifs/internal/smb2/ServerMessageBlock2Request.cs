using System;
using CIFSContext = jcifs.CIFSContext;
using Configuration = jcifs.Configuration;
using CommonServerMessageBlockRequest = jcifs.@internal.CommonServerMessageBlockRequest;
using CommonServerMessageBlockResponse = jcifs.@internal.CommonServerMessageBlockResponse;
using jcifs.@internal;
using jcifs.util.transport;

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
	/// @author mbechler </summary>
	/// @param <T>
	///            request type
	///  </param>
	public abstract class ServerMessageBlock2Request <T> : ServerMessageBlock2, CommonServerMessageBlockRequest, Request<T> where T : ServerMessageBlock2Response {
		public abstract int size();

		private T response;
		private int? overrideTimeout;


		/// <param name="config"> </param>
		protected  ServerMessageBlock2Request(Configuration config) : base(config) {
		}


		/// <param name="config"> </param>
		/// <param name="command"> </param>
		public ServerMessageBlock2Request(Configuration config, int command) : base(config, command) {
		}


		public  virtual CommonServerMessageBlock ignoreDisconnect() {
			return this;
		}


		//TODO type  @Override public ServerMessageBlock2Request<?> getNext()
		protected  override ServerMessageBlock2 getNext() {
		//TODO type  return (ServerMessageBlock2Request<?>) super.getNext();
			//TODO 1 type
			return (ServerMessageBlock2) base.getNext();
		}
		
		T Request<T>.getResponse()
		{
			return (T) this.getResponse();
		}

		//TODO 
		CommonServerMessageBlockRequest CommonServerMessageBlockRequest.getNext()
		{
			return (CommonServerMessageBlockRequest)getNext();
		}

		Response Request.getResponse()
		{
			return getResponse();
		}
		
		Request Request.getNext()
		{
			return (Request)getNext();
		}

		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.util.transport.Request#isCancel() </seealso>
		public virtual bool isCancel() {
			return false;
		}

	

		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.CommonServerMessageBlockRequest#isResponseAsync() </seealso>
		public virtual bool isResponseAsync() {
			return getAsyncId() != 0;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.CommonServerMessageBlockRequest#allowChain(jcifs.internal.CommonServerMessageBlockRequest) </seealso>
		public virtual bool allowChain(CommonServerMessageBlockRequest next) {
			return getConfig().isAllowCompound(this.GetType().Name) && getConfig().isAllowCompound(next.GetType().Name);
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.CommonServerMessageBlockRequest#createCancel() </seealso>
		public virtual CommonServerMessageBlockRequest createCancel() {
			return new Smb2CancelRequest(getConfig(), getMid(), getAsyncId());
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.CommonServerMessageBlockRequest#split() </seealso>
		public virtual CommonServerMessageBlockRequest split() {
		//TODO type  ServerMessageBlock2Request<?> n = getNext();
			//TODO 1 type
			var n = getNext();
			if (n != null) {
				setNext(null);
				n.clearFlags(SMB2_FLAGS_RELATED_OPERATIONS);
			}
			return (CommonServerMessageBlockRequest)n;
		}


		//TODO 1
		/// 
		/// <param name="next"> </param>
		public virtual void setNext(ServerMessageBlock2Request<T> next) {
			base.setNext(next);
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.util.transport.Request#getCreditCost() </seealso>
		public virtual int getCreditCost() {
			return 1;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.util.transport.Request#setRequestCredits(int) </seealso>
		public virtual void setRequestCredits(int credits) {
			setCredit(credits);
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.CommonServerMessageBlockRequest#getOverrideTimeout() </seealso>
		public virtual int? getOverrideTimeout() {
			return this.overrideTimeout;
		}


		/// <param name="overrideTimeout">
		///            the overrideTimeout to set </param>
		public virtual void setOverrideTimeout(int? overrideTimeout) {
			this.overrideTimeout = overrideTimeout;
		}


		/// 
		/// <returns> create response </returns>
		public virtual T initResponse(CIFSContext tc) {
			T resp = createResponse(tc, this);
			if (resp == null) {
				return default(T);
			}
			resp.setDigest(getDigest());
			setResponse(resp);

			ServerMessageBlock2 n = getNext();
			
		//TODO type  if (n instanceof ServerMessageBlock2Request<?>)
			//TODO 1 check type
			if (n is Request<CommonServerMessageBlockResponse> request) {
		//TODO type  resp.setNext(((ServerMessageBlock2Request<?>) n).initResponse(tc));
				//TODO
				if (!(request.initResponse(tc) is ServerMessageBlock2Response newResponse)) {
					throw new ArgumentException("response config error");
				}
				resp.setNext(newResponse);
			}
			return resp;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.CommonServerMessageBlockRequest#setTid(int) </seealso>
		public virtual void setTid(int t) {
			setTreeId(t);
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb2.ServerMessageBlock2#encode(byte[], int) </seealso>
		public override int encode(byte[] dst, int dstIndex) {
			int len = base.encode(dst, dstIndex);
			int exp = size();
			int actual = getLength();
			if (exp != actual) {
				throw new System.InvalidOperationException(string.Format("Wrong size calculation have {0:D} expect {1:D}", exp, actual));
			}
			return len;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb2.ServerMessageBlock2#getResponse() </seealso>
		public  override CommonServerMessageBlockResponse getResponse() {
			return this.response;
		}


		/// <param name="config2">
		/// @return </param>
		protected  abstract T createResponse(CIFSContext tc, ServerMessageBlock2Request<T> req);


		/// 
		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb2.ServerMessageBlock2#setResponse(jcifs.internal.CommonServerMessageBlockResponse) </seealso>
		public override void setResponse(CommonServerMessageBlockResponse msg) {
			if (msg != null && !(msg is ServerMessageBlock2)) {
				throw new System.ArgumentException("Incompatible response");
			}
			this.response = (T) msg;
		}
	}

}