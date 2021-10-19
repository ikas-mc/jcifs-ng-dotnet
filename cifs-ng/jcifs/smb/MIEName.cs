using System;
using cifs_ng.lib.ext;
using Org.BouncyCastle.Asn1;

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
    /// This class is used to parse the name of context initiator and
    /// context acceptor which are retrieved from GSSContext.
    /// 
    /// @author Shun
    /// 
    /// </summary>
    internal class MIEName
    {
        private static byte[] TOK_ID = new byte[] {0x4, 0x1};
        private static int TOK_ID_SIZE = 2;
        private static int MECH_OID_LEN_SIZE = 2;
        private static int NAME_LEN_SIZE = 4;

        private DerObjectIdentifier oid;
        private string name;


        /// <summary>
        /// Instance a <code>MIEName</code> object.
        /// </summary>
        /// <param name="buf">
        ///            the name of context initiator or acceptor </param>
        internal MIEName(byte[] buf)
        {
            int i;
            int len;
            if (buf.Length < TOK_ID_SIZE + MECH_OID_LEN_SIZE)
            {
                throw new ArgumentException();
            }

            // TOK_ID
            for (i = 0; i < TOK_ID.Length; i++)
            {
                if (TOK_ID[i] != buf[i])
                {
                    throw new ArgumentException();
                }
            }

            // MECH_OID_LEN
            len = 0xff00 & (buf[i++] << 8);
            len |= 0xff & buf[i++];

            // MECH_OID
            if (buf.Length < i + len)
            {
                throw new ArgumentException();
            }

            byte[] bo = new byte[len];
            Array.Copy(buf, i, bo, 0, len);
            i += len;
            this.oid = DerObjectIdentifier.GetInstance(bo);

            // NAME_LEN
            if (buf.Length < i + NAME_LEN_SIZE)
            {
                throw new ArgumentException();
            }

            len = unchecked((int) 0xff000000) & (buf[i++] << 24);
            len |= 0x00ff0000 & (buf[i++] << 16);
            len |= 0x0000ff00 & (buf[i++] << 8);
            len |= 0x000000ff & buf[i++];

            // NAME
            if (buf.Length < i + len)
            {
                throw new ArgumentException();
            }

            this.name = buf.toString( i, len);
        }


        internal MIEName(DerObjectIdentifier oid, string name)
        {
            this.oid = oid;
            this.name = name;
        }


        /*
         * (non-Javadoc)
         * 
         * @see java.lang.Object#equals(java.lang.Object)
         */
        public override bool Equals(object other)
        {
            if (other is MIEName)
            {
                MIEName terg = (MIEName) other;
                if (Equals(this.oid, terg.oid) && ((this.name==null && terg.name==null) || (this.name!=null && this.name.Equals(terg.name, StringComparison.OrdinalIgnoreCase))))
                {
                    return true;
                }
            }

            return false;
        }


        /*
         * (non-Javadoc)
         * 
         * @see java.lang.Object#hashCode()
         */
        public override int GetHashCode()
        {
            return this.oid.GetHashCode();
        }


        /*
         * (non-Javadoc)
         * 
         * @see java.lang.Object#toString()
         */
        public override string ToString()
        {
            return this.name;
        }
    }
}