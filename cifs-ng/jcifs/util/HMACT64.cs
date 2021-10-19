using System;
using cifs_ng.lib.security;
using jcifs.lib;

/* HMACT64 keyed hashing algorithm
 * Copyright (C) 2003 "Eric Glass" <jcifs at samba dot org>
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

namespace jcifs.util
{
    /// <summary>
    /// This is an implementation of the HMACT64 keyed hashing algorithm.
    /// HMACT64 is defined by Luke Leighton as a modified HMAC-MD5 (RFC 2104)
    /// in which the key is truncated at 64 bytes (rather than being hashed
    /// via MD5).
    /// </summary>
    internal class HMACT64 : MessageDigest, ICloneable
    {
        private const int BlockLength = 64;

        private const byte Ipad = unchecked(unchecked(0x36));

        private const byte Opad = unchecked(unchecked(0x5c));

        private MessageDigest _md5;

        private byte[] _ipad = new byte[BlockLength];

        private byte[] _opad = new byte[BlockLength];

        /// <summary>Creates an HMACT64 instance which uses the given secret key material.</summary>
        /// <remarks>Creates an HMACT64 instance which uses the given secret key material.</remarks>
        /// <param name="key">The key material to use in hashing.</param>
        public HMACT64(byte[] key)
        {
            int length = Math.Min(key.Length, BlockLength);
            for (int i = 0; i < length; i++)
            {
                _ipad[i] = unchecked((byte) (key[i] ^ Ipad));
                _opad[i] = unchecked((byte) (key[i] ^ Opad));
            }

            for (int i1 = length; i1 < BlockLength; i1++)
            {
                _ipad[i1] = Ipad;
                _opad[i1] = Opad;
            }

            _md5 = getInstance("MD5");
            _md5.update(_ipad);
        }


        private HMACT64(HMACT64 hmac)
        {
            this._ipad = hmac._ipad;
            this._opad = hmac._opad;
            this._md5 = (MessageDigest) getInstance("MD5");
            _md5.update(_ipad);
        }

        private byte[] EngineDigest()
        {
            byte[] digest = _md5.digest();
            _md5.update(_opad);
            return _md5.digest(digest);
        }

        private int EngineDigest(byte[] buf, int offset, int len)
        {
            byte[] digest = _md5.digest();
            _md5.update(_opad);
            _md5.update(digest);
            _md5.digest(buf, offset, len);
            return len;
        }

        private int EngineGetDigestLength()
        {
            return _md5.getDigestLength();
        }

        private void EngineReset()
        {
            _md5.reset();
            _md5.update(_ipad);
        }

        private void EngineUpdate(byte b)
        {
            _md5.update(b);
        }

        private void EngineUpdate(byte[] input, int offset, int len)
        {
            _md5.update(input, offset, len);
        }

        public override byte[] digest()
        {
            return EngineDigest();
        }

        public override int getDigestLength()
        {
            return EngineGetDigestLength();
        }

        public override void reset()
        {
            EngineReset();
        }

        public override void update(byte[] b)
        {
            EngineUpdate(b, 0, b.Length);
        }

        public override void update(byte b)
        {
            EngineUpdate(b);
        }

        public override void update(byte[] b, int offset, int len)
        {
            EngineUpdate(b, offset, len);
        }

        public object Clone()
        {
            return new HMACT64(this);
        }
    }
}