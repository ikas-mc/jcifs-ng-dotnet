using System;
using System.IO;
using cifs_ng.lib.ext;
using Logger = org.slf4j.Logger;
using LoggerFactory = org.slf4j.LoggerFactory;
using CIFSContext = jcifs.CIFSContext;
using CIFSException = jcifs.CIFSException;
using PropertyConfiguration = jcifs.config.PropertyConfiguration;

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
namespace jcifs.context {





	/// <summary>
	/// Global singleton context
	/// 
	/// @author mbechler
	/// 
	/// </summary>
	public class SingletonContext : BaseContext, CIFSContext {

		private static readonly Logger log = LoggerFactory.getLogger(typeof(SingletonContext));
		private static SingletonContext INSTANCE;


		/// <summary>
		/// Initialize singleton context using custom properties
		/// 
		/// This method can only be called once.
		/// </summary>
		/// <param name="props"> </param>
		/// <exception cref="CIFSException"> </exception>
		/// throws jcifs.CIFSException
		public static void init(Properties props) {
			lock (typeof(SingletonContext)) {
				if (INSTANCE != null) {
					throw new CIFSException("Singleton context is already initialized");
				}
				//TODO jcifs.properties
				// Properties p = new Properties();
				// try {
				// 	string filename = System.getProperty("jcifs.properties");
				// 	if (filename != null && filename.Length > 1) {
    //     
				// 		using (FileStream @in = new FileStream(filename, FileMode.Open, FileAccess.Read)) {
				// 			p.load(@in);
				// 		}
				// 	}
    //     
				// }
				// catch (IOException ioe) {
				// 	log.error("Failed to load config", ioe); //$NON-NLS-1$
				// }
				// p.putAll(System.getProperties());
				// if (props != null) {
				// 	p.putAll(props);
				// }
				INSTANCE = new SingletonContext(props);
			}
		}


		/// <summary>
		/// Get singleton context
		/// 
		/// The singleton context will use system properties for configuration as well as values specified in a file
		/// specified through this <tt>jcifs.properties</tt> system property.
		/// </summary>
		/// <returns> a global context, initialized on first call </returns>
		public static SingletonContext getInstance() {
			lock (typeof(SingletonContext)) {
				if (INSTANCE == null) {
					try {
						log.debug("Initializing singleton context");
						init(null);
					}
					catch (CIFSException e) {
						log.error("Failed to create singleton JCIFS context", e);
					}
				}
				return INSTANCE;
			}
		}


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
		/// 
		/// </summary>
		public static void registerSmbURLHandler()
		{
			//TODO 
			throw new Exception("not support registerSmbURLHandler");

			// SingletonContext.getInstance();
			// string pkgs = System.getProperty("java.protocol.handler.pkgs");
			// if (pkgs == null) {
			// 	System.setProperty("java.protocol.handler.pkgs", "jcifs");
			// }
			// else if (pkgs.IndexOf("jcifs", StringComparison.Ordinal) == -1) {
			// 	pkgs += "|jcifs";
			// 	System.setProperty("java.protocol.handler.pkgs", pkgs);
			// }
		}


		/// 
		/// throws jcifs.CIFSException
		private SingletonContext(Properties p) : base(new PropertyConfiguration(p)) {
		}

	}

}