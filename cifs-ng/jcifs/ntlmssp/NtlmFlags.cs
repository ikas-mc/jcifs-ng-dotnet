/* jcifs smb client library in Java
 * Copyright (C) 2002  "Michael B. Allen" <jcifs at samba dot org>
 *                    "Eric Glass" <jcifs at samba dot org>
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

namespace jcifs.ntlmssp
{
    /// <summary>
    /// Flags used during negotiation of NTLMSSP authentication.
    /// </summary>
    public static class NtlmFlags
    {
        /// <summary>
        /// Indicates whether Unicode strings are supported or used.
        /// </summary>
        public static int NTLMSSP_NEGOTIATE_UNICODE = 0x00000001;

        /// <summary>
        /// Indicates whether OEM strings are supported or used.
        /// </summary>
        public static int NTLMSSP_NEGOTIATE_OEM = 0x00000002;

        /// <summary>
        /// Indicates whether the authentication target is requested from
        /// the server.
        /// </summary>
        public static int NTLMSSP_REQUEST_TARGET = 0x00000004;

        /// <summary>
        /// Specifies that communication across the authenticated channel
        /// should carry a digital signature (message integrity).
        /// </summary>
        public static int NTLMSSP_NEGOTIATE_SIGN = 0x00000010;

        /// <summary>
        /// Specifies that communication across the authenticated channel
        /// should be encrypted (message confidentiality).
        /// </summary>
        public static int NTLMSSP_NEGOTIATE_SEAL = 0x00000020;

        /// <summary>
        /// Indicates datagram authentication.
        /// </summary>
        public static int NTLMSSP_NEGOTIATE_DATAGRAM_STYLE = 0x00000040;

        /// <summary>
        /// Indicates that the LAN Manager session key should be used for
        /// signing and sealing authenticated communication.
        /// </summary>
        public static int NTLMSSP_NEGOTIATE_LM_KEY = 0x00000080;

        /// <summary>
        /// ??? According to spec this is a reserved bit and must be set to zero
        /// </summary>
        public static int NTLMSSP_NEGOTIATE_NETWARE = 0x00000100;

        /// <summary>
        /// Indicates support for NTLM authentication.
        /// </summary>
        public static int NTLMSSP_NEGOTIATE_NTLM = 0x00000200;

        /// <summary>
        /// Indicates that this is an anonymous connection
        /// </summary>
        public static int NTLMSSP_NEGOTIATE_ANONYMOUS = 0x00000800;

        /// <summary>
        /// Indicates whether the OEM-formatted domain name in which the
        /// client workstation has membership is supplied in the Type-1 message.
        /// This is used in the negotation of local authentication.
        /// </summary>
        public static int NTLMSSP_NEGOTIATE_OEM_DOMAIN_SUPPLIED = 0x00001000;

        /// <summary>
        /// Indicates whether the OEM-formatted workstation name is supplied
        /// in the Type-1 message. This is used in the negotiation of local
        /// authentication.
        /// </summary>
        public static int NTLMSSP_NEGOTIATE_OEM_WORKSTATION_SUPPLIED = 0x00002000;

        /// <summary>
        /// Sent by the server to indicate that the server and client are
        /// on the same machine. This implies that the server will include
        /// a local security context handle in the Type 2 message, for
        /// use in local authentication.
        /// </summary>
        public static int NTLMSSP_NEGOTIATE_LOCAL_CALL = 0x00004000;

        /// <summary>
        /// Indicates that authenticated communication between the client
        /// and server should carry a "dummy" digital signature.
        /// </summary>
        public static int NTLMSSP_NEGOTIATE_ALWAYS_SIGN = 0x00008000;

        /// <summary>
        /// Sent by the server in the Type 2 message to indicate that the
        /// target authentication realm is a domain.
        /// </summary>
        public static int NTLMSSP_TARGET_TYPE_DOMAIN = 0x00010000;

        /// <summary>
        /// Sent by the server in the Type 2 message to indicate that the
        /// target authentication realm is a server.
        /// </summary>
        public static int NTLMSSP_TARGET_TYPE_SERVER = 0x00020000;

        /// <summary>
        /// Sent by the server in the Type 2 message to indicate that the
        /// target authentication realm is a share (presumably for share-level
        /// authentication).
        /// </summary>
        public static int NTLMSSP_TARGET_TYPE_SHARE = 0x00040000;

        /// <summary>
        /// Indicates that the NTLM2 signing and sealing scheme should be used
        /// for protecting authenticated communications. This refers to a
        /// particular session security scheme, and is not related to the use
        /// of NTLMv2 authentication.
        /// </summary>
        public static int NTLMSSP_NEGOTIATE_EXTENDED_SESSIONSECURITY = 0x00080000;

        /// <summary>
        /// ?? According to spec this is a reserved bit and must be set to zero
        /// </summary>
        public static int NTLMSSP_REQUEST_INIT_RESPONSE = 0x00100000;

        /// <summary>
        /// ?? According to spec this is NTLMSSP_NEGOTIATE_IDENTIFY
        /// 
        /// If set, requests an identify level token
        /// </summary>
        public static int NTLMSSP_REQUEST_ACCEPT_RESPONSE = 0x00200000;

        /// <summary>
        /// Requests the usage of the LMOWF
        /// </summary>
        public static int NTLMSSP_REQUEST_NON_NT_SESSION_KEY = 0x00400000;

        /// <summary>
        /// Sent by the server in the Type 2 message to indicate that it is
        /// including a Target Information block in the message. The Target
        /// Information block is used in the calculation of the NTLMv2 response.
        /// </summary>
        public static int NTLMSSP_NEGOTIATE_TARGET_INFO = 0x00800000;

        /// 
        public static int NTLMSSP_NEGOTIATE_VERSION = 0x2000000;

        /// <summary>
        /// Indicates that 128-bit encryption is supported.
        /// </summary>
        public static int NTLMSSP_NEGOTIATE_128 = 0x20000000;

        /// <summary>
        /// Request explicit key exchange
        /// </summary>
        public static int NTLMSSP_NEGOTIATE_KEY_EXCH = 0x40000000;

        /// <summary>
        /// Indicates that 56-bit encryption is supported.
        /// </summary>
        public static int NTLMSSP_NEGOTIATE_56 = unchecked((int) 0x80000000);
    }
}