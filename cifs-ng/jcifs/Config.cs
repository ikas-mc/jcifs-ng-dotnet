using System;
using System.Net;
using cifs_ng.lib.ext;
using Logger = org.slf4j.Logger;
using LoggerFactory = org.slf4j.LoggerFactory;
using SingletonContext = jcifs.context.SingletonContext;

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

namespace jcifs {





	/// <summary>
	/// This class now contains only utilities for config parsing.
	/// 
	/// We strongly suggest that you create an explicit <seealso cref="jcifs.context.CIFSContextWrapper"/>
	/// with your desired config. It's base implementation <seealso cref="jcifs.context.BaseContext"/>
	/// should be sufficient for most needs.
	/// 
	/// If you want to retain the classic singleton behavior you can use
	/// <seealso cref="jcifs.context.SingletonContext.getInstance()"/>
	/// witch is initialized using system properties.
	/// 
	/// </summary>
	public class Config {

		private static readonly Logger log = LoggerFactory.getLogger(typeof(Config));


		/// <summary>
		/// This static method registers the SMB URL protocol handler which is
		/// required to use SMB URLs with the <tt>java.net.URL</tt> class. If this
		/// method is not called before attempting to create an SMB URL with the
		/// URL class the following exception will occur:
		/// <blockquote>
		/// 
		/// <pre>
		/// Exception MalformedURLException: unknown protocol: smb
		///     at java.net.URL.&lt;init&gt;(URL.java:480)
		///     at java.net.URL.&lt;init&gt;(URL.java:376)
		///     at java.net.URL.&lt;init&gt;(URL.java:330)
		///     at jcifs.smb.SmbFile.&lt;init&gt;(SmbFile.java:355)
		///     ...
		/// </pre>
		/// 
		/// <blockquote>
		/// </summary>
		public static void registerSmbURLHandler() {
			SingletonContext.registerSmbURLHandler();
		}


		/// <summary>
		/// Retrieve an <code>int</code>. If the key does not exist or
		/// cannot be converted to an <code>int</code>, the provided default
		/// argument will be returned.
		/// </summary>
		public static int getInt(Properties props, string key, int def) {
			string s = props.getProperty(key);
			if (s != null) {
				try {
					def = int.Parse(s);
				}
				catch (System.FormatException nfe) {
					log.error("Not a number", nfe);
				}
			}
			return def;
		}


		/// <summary>
		/// Retrieve an <code>int</code>. If the property is not found, <code>-1</code> is returned.
		/// </summary>
		public static int getInt(Properties props, string key) {
			string s = props.getProperty(key);
			int result = -1;
			if (s != null) {
				try {
					result = int.Parse(s);
				}
				catch (System.FormatException nfe) {
					log.error("Not a number", nfe);
				}
			}
			return result;
		}


		/// <summary>
		/// Retrieve a <code>long</code>. If the key does not exist or
		/// cannot be converted to a <code>long</code>, the provided default
		/// argument will be returned.
		/// </summary>
		public static long getLong(Properties props, string key, long def) {
			string s = props.getProperty(key);
			if (s != null) {
				try {
					def = long.Parse(s);
				}
				catch (System.FormatException nfe) {
					log.error("Not a number", nfe);
				}
			}
			return def;
		}


		/// <summary>
		/// Retrieve an <code>IPAddress</code>. If the address is not
		/// an IP address and cannot be resolved <code>null</code> will
		/// be returned.
		/// </summary>
		public static IPAddress getInetAddress(Properties props, string key, IPAddress def) {
			string addr = props.getProperty(key);
			if (addr != null) {
				try {
					def = IPAddress.Parse(addr);
				}
				catch (FormatException uhe) {
					log.error("Unknown host " + addr, uhe);
				}
			}
			return def;
		}


		public static IPAddress getLocalHost(Properties props) {
			string addr = props.getProperty("jcifs.smb.client.laddr");

			if (addr != null) {
				try {
					return IPAddress.Parse(addr);
				}
				catch (FormatException uhe) {
					log.error("Ignoring jcifs.smb.client.laddr address: " + addr, uhe);
				}
			}

			return null;
		}


		/// <summary>
		/// Retrieve a boolean value. If the property is not found, the value of <code>def</code> is returned.
		/// </summary>
		public static bool getBoolean(Properties props, string key, bool def) {
			string b = props.getProperty(key);
			if (b != null) {
				def = b.ToLower().Equals("true");
			}
			return def;
		}


		/// <summary>
		/// Retrieve an array of <tt>IPAddress</tt> created from a property
		/// value containing a <tt>delim</tt> separated list of host names and/or
		/// ip addresses.
		/// </summary>
		public static IPAddress[] getInetAddressArray(Properties props, string key, string delim, IPAddress[] def) {
			string p = props.getProperty(key);
			if (p != null) {
				StringTokenizer tok = new StringTokenizer(p, delim);
				int len = tok.countTokens();
				IPAddress[] arr = new IPAddress[len];
				for (int i = 0; i < len; i++) {
					string addr = tok.nextToken();
					try {
						arr[i] = IPAddress.Parse(addr);
					}
					catch (FormatException uhe) {
						log.error("Unknown host " + addr, uhe);
						return def;
					}
				}
				return arr;
			}
			return def;
		}

	}

}