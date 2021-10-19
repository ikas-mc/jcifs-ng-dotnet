/*
 * © 2016 AgNO3 Gmbh & Co. KG
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

using System;
using System.Text;
using cifs_ng.lib.ext;

namespace jcifs.util
{
    using Logger = org.slf4j.Logger;
    using LoggerFactory = org.slf4j.LoggerFactory;
    using Configuration = jcifs.Configuration;
    using RuntimeCIFSException = jcifs.RuntimeCIFSException;


    /// <summary>
    /// @author mbechler
    /// 
    /// </summary>
    public sealed class Strings
    {
        private static readonly Logger log = LoggerFactory.getLogger(typeof(Strings));

        //TODO 
        public static readonly Encoding UNI_ENCODING = Encoding.Unicode; //Charset.forName("UTF-16LE");
        public static readonly Encoding ASCII_ENCODING = Encoding.ASCII; //("US-ASCII");
        public static readonly Encoding UTF_16LE_ENCODING = Encoding.Unicode; //Charset.forName("UTF-16LE");

        /// 
        private Strings()
        {
        }


        /// 
        /// <param name="str"> </param>
        /// <param name="encoding"> </param>
        /// <returns> encoded </returns>
        public static byte[] getBytes(string str, Encoding encoding)
        {
            if (str == null)
            {
                return new byte[0];
            }

            return str.getBytes(encoding);
        }


        /// 
        /// <param name="str"> </param>
        /// <returns> the string as bytes (UTF16-LE) </returns>
        public static byte[] getUNIBytes(string str)
        {
            return getBytes(str, UNI_ENCODING);
        }


        /// 
        /// <param name="str"> </param>
        /// <returns> the string as bytes (ASCII) </returns>
        public static byte[] getASCIIBytes(string str)
        {
            return getBytes(str, ASCII_ENCODING);
        }


        /// <param name="str"> </param>
        /// <param name="config"> </param>
        /// <returns> the string as bytes </returns>
        public static byte[] getOEMBytes(string str, Configuration config)
        {
            if (str == null)
            {
                return new byte[0];
            }

            return str.getBytes(config.getOemEncoding());
        }


        /// <param name="src"> </param>
        /// <param name="srcIndex"> </param>
        /// <param name="len"> </param>
        /// <returns> decoded string </returns>
        public static string fromUNIBytes(byte[] src, int srcIndex, int len) {
            return UNI_ENCODING.GetString(src, srcIndex, len);
        }


        /// <param name="buffer"> </param>
        /// <param name="bufferIndex"> </param>
        /// <param name="maxLen"> </param>
        /// <returns> position of terminating null bytes </returns>
        public static int findUNITermination(byte[] buffer, int bufferIndex, int maxLen)
        {
            int len = 0;
            while (buffer[bufferIndex + len] != (byte) 0x00 || buffer[bufferIndex + len + 1] != (byte) 0x00)
            {
                len += 2;
                if (len > maxLen)
                {
                    if (log.isDebugEnabled())
                    {
                        log.warn("Failed to find string termination with max length " + maxLen);
                        log.debug(Hexdump.toHexString(buffer, bufferIndex, len));
                    }

                    throw new RuntimeCIFSException("zero termination not found");
                }
            }

            return len;
        }


        /// <param name="src"> </param>
        /// <param name="srcIndex"> </param>
        /// <param name="len"> </param>
        /// <param name="config"> </param>
        /// <returns> decoded string </returns>
        public static string fromOEMBytes(byte[] src, int srcIndex, int len, Configuration config)
        {
            return src.toString(srcIndex, len, config.getOemEncoding());
        }


        /// <param name="buffer"> </param>
        /// <param name="bufferIndex"> </param>
        /// <param name="maxLen"> </param>
        /// <returns> position of terminating null byte </returns>
        public static int findTermination(byte[] buffer, int bufferIndex, int maxLen)
        {
            int len = 0;
            while (buffer[bufferIndex + len] != (byte) 0x00)
            {
                len++;
                if (len > maxLen)
                {
                    throw new RuntimeCIFSException("zero termination not found");
                }
            }

            return len;
        }
        
    }
}