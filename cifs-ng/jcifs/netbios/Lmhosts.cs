using System;
using System.Collections.Generic;
using System.IO;
using cifs_ng.lib.ext;
using cifs_ng.lib.io;
using Logger = org.slf4j.Logger;
using LoggerFactory = org.slf4j.LoggerFactory;
using CIFSContext = jcifs.CIFSContext;
using SmbFileInputStream = jcifs.smb.SmbFileInputStream;

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

namespace jcifs.netbios {





	/// 
	/// 
	public class Lmhosts {

		private static readonly Logger log = LoggerFactory.getLogger(typeof(Lmhosts));

		private readonly IDictionary<Name, NbtAddress> table = new Dictionary<Name, NbtAddress>();
		private long lastModified = 1L;
		private int alt;


		/// <summary>
		/// This is really just for <seealso cref="jcifs.netbios.UniAddress"/>. It does
		/// not throw an <seealso cref="java.net.UnknownHostException"/> because this
		/// is queried frequently and exceptions would be rather costly to
		/// throw on a regular basis here.
		/// </summary>
		/// <param name="host"> </param>
		/// <param name="tc"> </param>
		/// <returns> resolved name, null if not found </returns>
		public virtual NbtAddress getByName(string host, CIFSContext tc) {
			lock (this) {
				return getByName(new Name(tc.getConfig(), host, 0x20, null), tc);
			}
		}


		internal virtual NbtAddress getByName(Name name, CIFSContext tc) {
			lock (this) {
				NbtAddress result = null;
        
				try {
					if (tc.getConfig().getLmHostsFileName()!=null) {
						FileInfo f = new FileInfo(tc.getConfig().getLmHostsFileName());
						long lm;
						if ((lm = f.LastWriteTime.ToFileTimeUtc()) > this.lastModified) {
							if (log.isDebugEnabled()) {
								log.debug("Reading " + tc.getConfig().getLmHostsFileName());
							}
							this.lastModified = lm;
							this.table.Clear();
							using (StreamReader r = f.OpenText()) {
								populate(r, tc);
							}
						}
						result = this.table.get(name);
					}
				}
				catch (IOException fnfe) {
					log.error("Could not read lmhosts " + tc.getConfig().getLmHostsFileName(), fnfe); //$NON-NLS-1$
				}
				return result;
			}
		}


		/// throws java.io.IOException
		internal virtual void populate(StreamReader br, CIFSContext tc) {
			string line;

			while ((line = br.ReadLine())!=null) {
				line = line.ToUpper().Trim();
				if (line.Length == 0) {
					continue;
				}
				else if (line[0] == '#') {
					if (line.StartsWith("#INCLUDE ", StringComparison.Ordinal)) {
						line = line.Substring(line.IndexOf('\\'));
						string url = "smb:" + line.Replace('\\', '/');

						//TODO stream
						using (StreamReader rdr = new StreamReader(new InputStreamToStream(new SmbFileInputStream(url, tc)))) {
							if (this.alt > 0) {
								try {
									populate(rdr, tc);
								}
								catch (IOException ioe) {
									log.error("Failed to read include " + url, ioe);
									continue;
								}

								/*
								 * An include was loaded successfully. We can skip
								 * all other includes up to the #END_ALTERNATE tag.
								 */

								while ((line = br.ReadLine())!=null) {
									line = line.ToUpper().Trim();
									if (line.StartsWith("#END_ALTERNATE", StringComparison.Ordinal)) {
										break;
									}
								}
							}
							else {
								populate(rdr, tc);
							}
						}
					}
					else if (line.StartsWith("#BEGIN_ALTERNATE", StringComparison.Ordinal)) {
					}
					else if (line.StartsWith("#END_ALTERNATE", StringComparison.Ordinal) && this.alt > 0) {
						throw new IOException("no lmhosts alternate includes loaded");
					}
				}
				else if (char.IsDigit(line[0])) {
					char[] data = line.ToCharArray();
					int ip, i, j;
					Name name;
					NbtAddress addr;
					char c;

					c = '.';
					ip = i = 0;
					for (; i < data.Length && c == '.'; i++) {
						int b = 0x00;

						for (; i < data.Length && (c = data[i]) >= 48 && c <= (char)57; i++) {
							b = b * 10 + c - '0';
						}
						ip = (ip << 8) + b;
					}
					while (i < data.Length && char.IsWhiteSpace(data[i])) {
						i++;
					}
					j = i;
					while (j < data.Length && char.IsWhiteSpace(data[j]) == false) {
						j++;
					}

					name = new Name(tc.getConfig(), line.Substring(i, j - i), 0x20, null);
					addr = new NbtAddress(name, ip, false, NbtAddress.B_NODE, false, false, true, true, NbtAddress.UNKNOWN_MAC_ADDRESS);
					if (log.isDebugEnabled()) {
						log.debug("Adding " + name + " with addr " + addr);
					}
					this.table[name] = addr;
				}
			}
		}
	}

}