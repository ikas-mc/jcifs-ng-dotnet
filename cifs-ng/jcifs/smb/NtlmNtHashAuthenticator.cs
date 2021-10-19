using System;
using Org.BouncyCastle.Utilities.Encoders;

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
namespace jcifs.smb
{
    /// <summary>
    /// Authenticator directly specifing the user's NT hash
    /// 
    /// @author mbechler
    /// 
    /// </summary>
    [Serializable]
    public class NtlmNtHashAuthenticator : NtlmPasswordAuthenticator
    {
        private const long serialVersionUID = 4328214169536360351L;
        private readonly byte[] ntHash;


        /// <summary>
        /// Create username/password credentials with specified domain
        /// </summary>
        /// <param name="domain"> </param>
        /// <param name="username"> </param>
        /// <param name="passwordHash">
        ///            NT password hash </param>
        public NtlmNtHashAuthenticator(string domain, string username, byte[] passwordHash) : base(domain, username, null, AuthenticationType.USER)
        {
            if (passwordHash == null || passwordHash.Length != 16)
            {
                throw new ArgumentException("Password hash must be provided, expected length 16 byte");
            }

            this.ntHash = passwordHash;
        }


        /// <summary>
        /// Create username/password credentials with specified domain
        /// </summary>
        /// <param name="domain"> </param>
        /// <param name="username"> </param>
        /// <param name="passwordHashHex">
        ///            NT password hash, hex encoded </param>
        public NtlmNtHashAuthenticator(string domain, string username, string passwordHashHex) : this(domain, username, Hex.Decode(passwordHashHex))
        {
        }


        private NtlmNtHashAuthenticator(byte[] passwordHash) : base()
        {
            this.ntHash = passwordHash;
        }


        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        /// <seealso cref= jcifs.smb.NtlmPasswordAuthenticator#getNTHash() </seealso>
        protected internal override byte[] getNTHash()
        {
            return this.ntHash;
        }


        public override object Clone()
        {
            NtlmNtHashAuthenticator cloned = new NtlmNtHashAuthenticator((byte[]) this.ntHash.Clone());
            cloneInternal(cloned, this);
            return cloned;
        }
    }
}