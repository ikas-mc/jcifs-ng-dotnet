/* jcifs smb client library in Java
 * Copyright (C) 2004  "Michael B. Allen" <jcifs at samba dot org>
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
    public static class DosError
    {
        public static int[,] DOS_ERROR_CODES = new int[,]
        {
            {0x00000000, 0x00000000},
            {0x00010001, unchecked((int) 0xc0000002)},
            {0x00010002, unchecked((int) 0xc0000002)},
            {0x00020001, unchecked((int) 0xc000000f)},
            {0x00020002, unchecked((int) 0xc000006a)},
            {0x00030001, unchecked((int) 0xc000003a)},
            {0x00030002, unchecked((int) 0xc00000cb)},
            {0x00040002, unchecked((int) 0xc00000ca)},
            {0x00050001, unchecked((int) 0xc0000022)},
            {0x00050002, unchecked((int) 0xc000000d)},
            {0x00060001, unchecked((int) 0xc0000008)},
            {0x00060002, unchecked((int) 0xc00000cc)},
            {0x00080001, unchecked((int) 0xc000009a)},
            {0x00130003, unchecked((int) 0xc00000a2)},
            {0x00150003, unchecked((int) 0xc0000013)},
            {0x001f0001, unchecked((int) 0xc0000001)},
            {0x001f0003, unchecked((int) 0xc0000001)},
            {0x00200001, unchecked((int) 0xc0000043)},
            {0x00200003, unchecked((int) 0xc0000043)},
            {0x00210003, unchecked((int) 0xc0000054)},
            {0x00270003, unchecked((int) 0xc000007f)},
            {0x00340001, unchecked((int) 0xC00000bd)},
            {0x00320001, unchecked((int) 0xC00000BB)},
            {0x00430001, unchecked((int) 0xc00000cc)},
            {0x00470001, unchecked((int) 0xC00000d0)},
            {0x00500001, unchecked((int) 0xc0000035)},
            {0x00570001, unchecked((int) 0xC000000d)},
            {0x005a0002, unchecked((int) 0xc00000ce)},
            {0x005b0002, unchecked((int) 0xc000000d)},
            {0x006d0001, unchecked((int) 0xC000014b)},
            {0x007b0001, unchecked((int) 0xc0000033)},
            {0x00910001, unchecked((int) 0xC0000101)},
            {0x00b70001, unchecked((int) 0xc0000035)},
            {0x00e70001, unchecked((int) 0xc00000ab)},
            {0x00e80001, unchecked((int) 0xc00000b1)},
            {0x00e90001, unchecked((int) 0xc00000b0)},
            {0x00ea0001, unchecked((int) 0xc0000016)},
            {0x08bf0002, unchecked((int) 0xC0000193)},
            {0x08c00002, unchecked((int) 0xC0000070)},
            {0x08c10002, unchecked((int) 0xC000006f)},
            {0x08c20002, unchecked((int) 0xC0000071)}
        };

        /*
         * These aren't really used by jCIFS -- the map above is used
         * to immediately map to NTSTATUS codes.
         */
        public static string[] DOS_ERROR_MESSAGES = new string[]
        {
            "The operation completed successfully.", "Incorrect function.", "Incorrect function.", "The system cannot find the file specified.", "Bad password.", "The system cannot find the path specified.", "reserved", "The client does not have the necessary access rights to perform the requested function.", "Access is denied.", "The TID specified was invalid.", "The handle is invalid.", "The network name cannot be found.", "Not enough storage is available to process this command.",
            "The media is write protected.", "The device is not ready.", "A device attached to the system is not functioning.", "A device attached to the system is not functioning.", "The process cannot access the file because it is being used by another process.", "The process cannot access the file because it is being used by another process.", "The process cannot access the file because another process has locked a portion of the file.", "The disk is full.",
            "A duplicate name exists on the network.", "The network name cannot be found.", "ERRnomoreconn.", "The file exists.", "The parameter is incorrect.", "Too many Uids active on this session.", "The Uid is not known as a valid user identifier on this session.", "The pipe has been ended.", "The filename, directory name, or volume label syntax is incorrect.", "The directory is not empty.", "Cannot create a file when that file already exists.", "All pipe instances are busy.",
            "The pipe is being closed.", "No process is on the other end of the pipe.", "More data is available.", "This user account has expired.", "The user is not allowed to log on from this workstation.", "The user is not allowed to log on at this time.", "The password of this user has expired."
        };
    }
}