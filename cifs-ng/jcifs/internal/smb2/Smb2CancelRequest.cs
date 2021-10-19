using jcifs.util.transport;
using Configuration = jcifs.Configuration;
using CommonServerMessageBlockRequest = jcifs.@internal.CommonServerMessageBlockRequest;
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
namespace jcifs.@internal.smb2 {



	/// <summary>
	/// @author mbechler
	/// 
	/// </summary>
	public class Smb2CancelRequest : ServerMessageBlock2, CommonServerMessageBlockRequest {

		/// <param name="config"> </param>
		/// <param name="mid"> </param>
		/// <param name="asyncId"> </param>
		public Smb2CancelRequest(Configuration config, long mid, long asyncId) : base(config, SMB2_CANCEL) {
			setMid(mid);
			setAsyncId(asyncId);
			if (asyncId != 0) {
				addFlags(SMB2_FLAGS_ASYNC_COMMAND);
			}
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
		/// <seealso cref= jcifs.internal.CommonServerMessageBlockRequest#isResponseAsync() </seealso>
		public virtual bool isResponseAsync() {
			return false;
		}


		//TODO type  @Override public ServerMessageBlock2Request<?> getNext()
		//TODO 1 type
		protected  override ServerMessageBlock2 getNext() {
			return null;
		}

		//TODO 
		Response Request.getResponse()
		{
			return getResponse();
		}
		
		Request Request.getNext()
		{
			return (Request)getNext();
		}
		
		CommonServerMessageBlockRequest CommonServerMessageBlockRequest.getNext()
		{
			return (CommonServerMessageBlockRequest)getNext();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.CommonServerMessageBlockRequest#getOverrideTimeout() </seealso>
		public virtual int? getOverrideTimeout() {
			return null;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.CommonServerMessageBlockRequest#allowChain(jcifs.internal.CommonServerMessageBlockRequest) </seealso>
		public virtual bool allowChain(CommonServerMessageBlockRequest next) {
			return false;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.CommonServerMessageBlockRequest#split() </seealso>
		public virtual CommonServerMessageBlockRequest split() {
			return null;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.CommonServerMessageBlockRequest#createCancel() </seealso>
		public virtual CommonServerMessageBlockRequest createCancel() {
			return null;
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
		/// <seealso cref= jcifs.internal.CommonServerMessageBlockRequest#setTid(int) </seealso>
		public virtual void setTid(int t) {
			setTreeId(t);
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.util.transport.Request#isCancel() </seealso>
		public virtual bool isCancel() {
			return true;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.CommonServerMessageBlockRequest#size() </seealso>
		public virtual int size() {
			return size8(Smb2Constants.SMB2_HEADER_LENGTH + 4);
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb2.ServerMessageBlock2#writeBytesWireFormat(byte[], int) </seealso>
		protected  override int writeBytesWireFormat(byte[] dst, int dstIndex) {
			int start = dstIndex;
			SMBUtil.writeInt2(4, dst, dstIndex);
			dstIndex += 4;
			return dstIndex - start;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.smb2.ServerMessageBlock2#readBytesWireFormat(byte[], int) </seealso>
		protected  override int readBytesWireFormat(byte[] buffer, int bufferIndex) {
			return 0;
		}

	}

}