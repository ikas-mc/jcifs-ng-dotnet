using System;
using cifs_ng.lib.ext;
using cifs_ng.lib.security;
using jcifs.lib;
using Logger = org.slf4j.Logger;
using LoggerFactory = org.slf4j.LoggerFactory;
using Configuration = jcifs.Configuration;
using SmbConstants = jcifs.SmbConstants;
using CommonServerMessageBlock = jcifs.@internal.CommonServerMessageBlock;
using SMBSigningDigest = jcifs.@internal.SMBSigningDigest;
using SmbComReadAndXResponse = jcifs.@internal.smb1.com.SmbComReadAndXResponse;
using SmbComNtCancel = jcifs.@internal.smb1.trans.nt.SmbComNtCancel;
using SMBUtil = jcifs.@internal.util.SMBUtil;
using NtlmPasswordAuthenticator = jcifs.smb.NtlmPasswordAuthenticator;
using SmbException = jcifs.smb.SmbException;
using SmbTransportInternal = jcifs.smb.SmbTransportInternal;
using Crypto = jcifs.util.Crypto;
using Hexdump = jcifs.util.Hexdump;

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
namespace jcifs.@internal.smb1 {





	/// 
	/// <summary>
	/// @internal
	/// </summary>
	public class SMB1SigningDigest : SMBSigningDigest {

		private static readonly Logger log = LoggerFactory.getLogger(typeof(SMB1SigningDigest));

		private MessageDigest digestField;
		private byte[] macSigningKey;
		private bool bypass = false;
		private int updates;
		private int signSequence;


		/// 
		/// <param name="macSigningKey"> </param>
		/// <param name="bypass"> </param>
		public SMB1SigningDigest(byte[] macSigningKey, bool bypass) : this(macSigningKey, bypass, 0) {
		}


		/// 
		/// <param name="macSigningKey"> </param>
		/// <param name="bypass"> </param>
		/// <param name="initialSequence"> </param>
		public SMB1SigningDigest(byte[] macSigningKey, bool bypass, int initialSequence) {
			this.digestField = Crypto.getMD5();
			this.macSigningKey = macSigningKey;
			this.signSequence = initialSequence;
			this.bypass = bypass;

			if (log.isTraceEnabled()) {
				log.trace("macSigningKey:");
				log.trace(Hexdump.toHexString(macSigningKey, 0, macSigningKey.Length));
			}
		}


		/// <summary>
		/// This constructor used to instance a SigningDigest object for
		/// signing/verifying SMB using kerberos session key.
		/// The MAC Key = concat(Session Key, Digest of Challenge);
		/// Because of Kerberos Authentication don't have challenge,
		/// The MAC Key = Session Key
		/// </summary>
		/// <param name="macSigningKey">
		///            The MAC key used to sign or verify SMB. </param>
		public SMB1SigningDigest(byte[] macSigningKey) {
			this.digestField = Crypto.getMD5();
			this.macSigningKey = macSigningKey;
		}


		/// <summary>
		/// Construct a digest with a non-zero starting sequence number
		/// </summary>
		/// <param name="macSigningKey"> </param>
		/// <param name="initialSequence"> </param>
		public SMB1SigningDigest(byte[] macSigningKey, int initialSequence) {
			this.digestField = Crypto.getMD5();
			this.macSigningKey = macSigningKey;
			this.signSequence = initialSequence;
		}


		/// 
		/// <param name="transport"> </param>
		/// <param name="auth"> </param>
		/// <exception cref="SmbException"> </exception>
		/// throws jcifs.smb.SmbException
		public SMB1SigningDigest(SmbTransportInternal transport, NtlmPasswordAuthenticator auth) {
			this.digestField = Crypto.getMD5();
			try {
				byte[] serverEncryptionKey = transport.getServerEncryptionKey();
				switch (transport.getContext().getConfig().getLanManCompatibility()) {
				case 0:
				case 1:
				case 2:
					this.macSigningKey = new byte[40];
					auth.getUserSessionKey(transport.getContext(), serverEncryptionKey, this.macSigningKey, 0);
					Array.Copy(auth.getUnicodeHash(transport.getContext(), serverEncryptionKey), 0, this.macSigningKey, 16, 24);
					break;
				case 3:
				case 4:
				case 5:
					this.macSigningKey = new byte[16];
					auth.getUserSessionKey(transport.getContext(), serverEncryptionKey, this.macSigningKey, 0);
					break;
				default:
					this.macSigningKey = new byte[40];
					auth.getUserSessionKey(transport.getContext(), serverEncryptionKey, this.macSigningKey, 0);
					Array.Copy(auth.getUnicodeHash(transport.getContext(), serverEncryptionKey), 0, this.macSigningKey, 16, 24);
					break;
				}
			}
			catch (Exception ex) {
				throw new SmbException("", ex);
			}
			if (log.isTraceEnabled()) {
				log.trace("LM_COMPATIBILITY=" + transport.getContext().getConfig().getLanManCompatibility());
				log.trace(Hexdump.toHexString(this.macSigningKey, 0, this.macSigningKey.Length));
			}
		}


		/// <summary>
		/// Update digest with data
		/// </summary>
		/// <param name="input"> </param>
		/// <param name="offset"> </param>
		/// <param name="len"> </param>
		public virtual void update(byte[] input, int offset, int len) {
			if (log.isTraceEnabled()) {
				log.trace("update: " + this.updates + " " + offset + ":" + len);
				log.trace(Hexdump.toHexString(input, offset, Math.Min(len, 256)));
			}
			if (len == 0) {
				return; // CRITICAL
			}
			this.digestField.update(input, offset, len);
			this.updates++;
		}


		/// <returns> calculated digest </returns>
		public virtual byte[] digest() {
			byte[] b;

			b = this.digestField.digest();

			if (log.isTraceEnabled()) {
				log.trace("digest: ");
				log.trace(Hexdump.toHexString(b, 0, b.Length));
			}
			this.updates = 0;

			return b;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.SMBSigningDigest#sign(byte[], int, int, jcifs.internal.CommonServerMessageBlock,
		///      jcifs.internal.CommonServerMessageBlock) </seealso>
		public virtual void sign(byte[] data, int offset, int length, CommonServerMessageBlock request, CommonServerMessageBlock response) {
			if (log.isTraceEnabled()) {
				log.trace("Signing with seq " + this.signSequence);
			}

			((ServerMessageBlock) request).setSignSeq(this.signSequence);
			if (response != null) {
				((ServerMessageBlock) response).setSignSeq(this.signSequence + 1);
			}

			try {
				update(this.macSigningKey, 0, this.macSigningKey.Length);
				int index = offset + SmbConstants.SIGNATURE_OFFSET;
				for (int i = 0; i < 8; i++) {
					data[index + i] = 0;
				}
				SMBUtil.writeInt4(this.signSequence, data, index);
				update(data, offset, length);
				Array.Copy(digest(), 0, data, index, 8);
				if (this.bypass) {
					this.bypass = false;
					Array.Copy("BSRSPYL ".getBytes(), 0, data, index, 8);
				}
			}
			catch (Exception ex) {
				log.error("Signature failed", ex);
			}
			finally {
				if (request is SmbComNtCancel) {
					this.signSequence++;
				}
				else {
					this.signSequence += 2;
				}
			}
		}


		/// 
		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.internal.SMBSigningDigest#verify(byte[], int, int, int, jcifs.internal.CommonServerMessageBlock) </seealso>
		public virtual bool verify(byte[] data, int offset, int l, int extraPad, CommonServerMessageBlock m) {

			ServerMessageBlock msg = (ServerMessageBlock) m;

			if ((msg.getFlags2() & SmbConstants.FLAGS2_SECURITY_SIGNATURES) == 0) {
				// signature requirements need to be checked somewhere else
				log.warn("Expected signed response, but is not signed");
				return false;
			}

			update(this.macSigningKey, 0, this.macSigningKey.Length);
			int index = offset;
			update(data, index, SmbConstants.SIGNATURE_OFFSET);
			index += SmbConstants.SIGNATURE_OFFSET;
			byte[] sequence = new byte[8];
			SMBUtil.writeInt4(msg.getSignSeq(), sequence, 0);
			update(sequence, 0, sequence.Length);
			index += 8;
			if (msg.getCommand() == ServerMessageBlock.SMB_COM_READ_ANDX) {
				/*
				 * SmbComReadAndXResponse reads directly from the stream into separate byte[] b.
				 */
				SmbComReadAndXResponse raxr = (SmbComReadAndXResponse) msg;
				int length = msg.getLength() - raxr.getDataLength();
				update(data, index, length - SmbConstants.SIGNATURE_OFFSET - 8);
				update(raxr.getData(), raxr.getOffset(), raxr.getDataLength());
			}
			else {
				update(data, index, msg.getLength() - SmbConstants.SIGNATURE_OFFSET - 8);
			}
			byte[] signature = digest();
			for (int i = 0; i < 8; i++) {
				if (signature[i] != data[offset + SmbConstants.SIGNATURE_OFFSET + i]) {
					if (log.isDebugEnabled()) {
						log.debug("signature verification failure"); //$NON-NLS-1$
						log.debug("Expect: " + Hexdump.toHexString(signature, 0, 8));
						log.debug("Have: " + Hexdump.toHexString(data, offset + SmbConstants.SIGNATURE_OFFSET, 8));
					}
					return true;
				}
			}

			return false;
		}


		public override string ToString() {
			return "MacSigningKey=" + Hexdump.toHexString(this.macSigningKey, 0, this.macSigningKey.Length);
		}


		/// 
		/// <param name="cfg"> </param>
		/// <param name="t"> </param>
		/// <param name="dst"> </param>
		/// <param name="dstIndex"> </param>
		public static void writeUTime(Configuration cfg, long t, byte[] dst, int dstIndex) {
			if (t == 0L || t == unchecked((long)0xFFFFFFFFFFFFFFFFL)) {
				SMBUtil.writeInt4(0xFFFFFFFF, dst, dstIndex);
				return;
			}

			//TODO time
			if (cfg.getLocalTimezone().IsDaylightSavingTime(DateTime.Now)) {
				// in DST
				if (cfg.getLocalTimezone().IsDaylightSavingTime(DateTimeOffset.FromUnixTimeMilliseconds(t))) {
					// t also in DST so no correction
				}
				else {
					// t not in DST so subtract 1 hour
					t -= 3600000;
				}
			}
			else {
				// not in DST
				if (cfg.getLocalTimezone().IsDaylightSavingTime(DateTimeOffset.FromUnixTimeMilliseconds(t))) {
					// t is in DST so add 1 hour
					t += 3600000;
				}
				else {
					// t isn't in DST either
				}
			}
			SMBUtil.writeInt4((int)(t / 1000L), dst, dstIndex);
		}

	}

}