using System;
using System.Collections.Generic;
using cifs_ng.lib;
using cifs_ng.lib.ext;
using jcifs.util;

/* jcifs smb client library in Java
 * Copyright (C) 2000  "Michael B. Allen" <jcifs at samba dot org>
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
    /// There are hundreds of error codes that may be returned by a CIFS
    /// server. Rather than represent each with it's own <code>Exception</code>
    /// class, this class represents all of them. For many of the popular
    /// error codes, constants and text messages like "The device is not ready"
    /// are provided.
    /// <para>
    /// The jCIFS client maps DOS error codes to NTSTATUS codes. This means that
    /// the user may receive a different error from a legacy server than that of
    /// a newer variant such as Windows NT and above. If you should encounter
    /// such a case, please report it to jcifs at samba dot org and we will
    /// change the mapping.
    /// </para>
    /// </summary>
    public class SmbException : CIFSException
    {
        // to replace a bunch of one-off binary searches
        private static readonly IDictionary<int, string> errorCodeMessages;
        private static readonly IDictionary<int, string> winErrorCodeMessages;
        private static readonly IDictionary<int, int> dosErrorCodeStatuses;

        static SmbException()
        {
            IDictionary<int, string> errorCodeMessagesTmp = new Dictionary<int, string>();
            for (int i = 0; i < NtStatus.NT_STATUS_CODES.Length; i++)
            {
                errorCodeMessagesTmp[NtStatus.NT_STATUS_CODES[i]] = NtStatus.NT_STATUS_MESSAGES[i];
            }

            IDictionary<int, int> dosErrorCodeStatusesTmp = new Dictionary<int, int>();
            for (int i = 0; i < DosError.DOS_ERROR_CODES.GetLength(0); i++)
            {
                dosErrorCodeStatusesTmp[DosError.DOS_ERROR_CODES[i, 0]] = DosError.DOS_ERROR_CODES[i, 1];
                int mappedNtCode = DosError.DOS_ERROR_CODES[i, 1];
                string mappedNtMessage = errorCodeMessagesTmp.get(mappedNtCode);
                if (mappedNtMessage != null)
                {
                    errorCodeMessagesTmp[DosError.DOS_ERROR_CODES[i, 0]] = mappedNtMessage;
                }
            }

            // for backward compatibility since this is was different message in the NtStatus.NT_STATUS_CODES than returned
            // by getMessageByCode
            errorCodeMessagesTmp[0] = "NT_STATUS_SUCCESS";

            //TODO 1 unmodifiableMap
            errorCodeMessages = Collections.unmodifiableMap(errorCodeMessagesTmp);
            dosErrorCodeStatuses = Collections.unmodifiableMap(dosErrorCodeStatusesTmp);

            IDictionary<int, string> winErrorCodeMessagesTmp = new Dictionary<int, string>();
            for (int i = 0; i < WinError.WINERR_CODES.Length; i++)
            {
                winErrorCodeMessagesTmp[WinError.WINERR_CODES[i]] = WinError.WINERR_MESSAGES[i];
            }

            //TODO 1 unmodifiableMap
            winErrorCodeMessages = Collections.unmodifiableMap(winErrorCodeMessagesTmp);
        }


        /// 
        /// <param name="errcode"> </param>
        /// <returns> message for NT STATUS code
        /// @internal </returns>
        public static string getMessageByCode(int errcode)
        {
            string message = errorCodeMessages.get(errcode);
            if (message == null)
            {
                message = "0x" + Hexdump.toHexString(errcode, 8);
            }

            return message;
        }


        internal static int getStatusByCode(int errcode)
        {
            int statusCode;
            if ((errcode & 0xC0000000) != 0)
            {
                statusCode = errcode;
            }
            else if (dosErrorCodeStatuses.ContainsKey(errcode))
            {
                statusCode = dosErrorCodeStatuses.get(errcode);
            }
            else
            {
                statusCode = NtStatus.NT_STATUS_UNSUCCESSFUL;
            }

            return statusCode;
        }


        internal static string getMessageByWinerrCode(int errcode)
        {
            string message = winErrorCodeMessages.get(errcode);
            if (message == null)
            {
                message = "W" + Hexdump.toHexString(errcode, 8);
            }

            return message;
        }

        private int status;


        /// 
        public SmbException()
        {
        }


        /// 
        /// <param name="errcode"> </param>
        /// <param name="rootCause"> </param>
        public SmbException(int errcode, Exception rootCause) : base(getMessageByCode(errcode), rootCause)
        {
            this.status = getStatusByCode(errcode);
        }


        /// 
        /// <param name="msg"> </param>
        public SmbException(string msg) : base(msg)
        {
            this.status = NtStatus.NT_STATUS_UNSUCCESSFUL;
        }


        /// 
        /// <param name="msg"> </param>
        /// <param name="rootCause"> </param>
        public SmbException(string msg, Exception rootCause) : base(msg, rootCause)
        {
            this.status = NtStatus.NT_STATUS_UNSUCCESSFUL;
        }


        /// 
        /// <param name="errcode"> </param>
        /// <param name="winerr"> </param>
        public SmbException(int errcode, bool winerr) : base(winerr ? getMessageByWinerrCode(errcode) : getMessageByCode(errcode))
        {
            this.status = winerr ? errcode : getStatusByCode(errcode);
        }


        /// 
        /// <returns> status code </returns>
        public virtual int getNtStatus()
        {
            return this.status;
        }


        /// 
        /// <returns> cause </returns>
        [Obsolete]
        public virtual Exception getRootCause()
        {
            return this.InnerException;
        }


        /// <param name="e"> </param>
        /// <returns> a CIFS exception wrapped in an SmbException </returns>
        internal static SmbException wrap(CIFSException e)
        {
            if (e is SmbException)
            {
                return (SmbException) e;
            }

            return new SmbException(e.Message, e);
        }
    }
}