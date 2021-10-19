using System;
using System.Collections.Generic;
using System.Net;
using cifs_ng.lib;
using cifs_ng.lib.ext;
using Org.BouncyCastle.Security;
using Logger = org.slf4j.Logger;
using LoggerFactory = org.slf4j.LoggerFactory;
using CIFSException = jcifs.CIFSException;
using Configuration = jcifs.Configuration;
using DialectVersion = jcifs.DialectVersion;
using ResolverType = jcifs.ResolverType;
using SmbConstants = jcifs.SmbConstants;

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
namespace jcifs.config {





	/// <summary>
	/// @author mbechler
	/// 
	/// </summary>
	public class BaseConfiguration : Configuration {

		private static readonly Logger log = LoggerFactory.getLogger(typeof(BaseConfiguration));
		private static readonly IDictionary<string, int> DEFAULT_BATCH_LIMITS = new Dictionary<string, int>();

		static BaseConfiguration() {
			DEFAULT_BATCH_LIMITS["TreeConnectAndX.QueryInformation"] = 0;
		}

		private readonly IDictionary<string, int> batchLimits = new Dictionary<string, int>();

		protected internal int localPid = -1;
		protected internal TimeZoneInfo localTimeZone;
		protected internal SecureRandom random;
		protected internal bool useBatching = true;
		protected internal bool useUnicode = true;
		protected internal bool forceUnicode = false;
		protected internal bool signingPreferred = false;
		protected internal bool signingEnforced = false;
		protected internal bool ipcSigningEnforced = true;
		protected internal bool encryptionEnabled = false;
		protected internal bool useNtStatus = true;
		protected internal bool useExtendedSecurity = true;
		protected internal bool forceExtendedSecurity = false;
		protected internal bool smb2OnlyNegotiation = false;
		protected internal bool port139FailoverEnabled = false;
		protected internal bool useNTSmbs = true;
		protected internal bool useLargeReadWrite = true;
		protected internal int lanmanCompatibility = 3;
		protected internal bool allowNTLMFallback = true;
		protected internal bool useRawNTLM = false;
		protected internal bool disableSpnegoIntegrity = false;
		protected internal bool enforceSpnegoIntegrity = true;
		protected internal bool disablePlainTextPasswords = true;
		protected internal string oemEncoding = SmbConstants.DEFAULT_OEM_ENCODING;
		protected internal int flags2 = 0;
		protected internal int capabilities = 0;
		protected internal int sessionLimit = SmbConstants.DEFAULT_SSN_LIMIT;
		protected internal bool smbTcpNoDelay = false;
		protected internal int smbResponseTimeout = SmbConstants.DEFAULT_RESPONSE_TIMEOUT;
		protected internal int smbSocketTimeout = SmbConstants.DEFAULT_SO_TIMEOUT;
		protected internal int smbConnectionTimeout = SmbConstants.DEFAULT_CONN_TIMEOUT;
		protected internal int smbSessionTimeout = SmbConstants.DEFAULT_SO_TIMEOUT;
		protected internal bool idleTimeoutDisabled = false;
		protected internal IPAddress smbLocalAddress;
		protected internal int smbLocalPort = 0;
		protected internal int maxMpxCount = SmbConstants.DEFAULT_MAX_MPX_COUNT;
		protected internal int smbSendBufferSize = SmbConstants.DEFAULT_SND_BUF_SIZE;
		protected internal int smbRecvBufferSize = SmbConstants.DEFAULT_RCV_BUF_SIZE;
		protected internal int smbNotifyBufferSize = SmbConstants.DEFAULT_NOTIFY_BUF_SIZE;
		protected internal string nativeOs;
		protected internal string nativeLanMan = "jCIFS";
		protected internal int vcNumber = 1;
		protected internal bool dfsDisabled = false;
		protected internal long dfsTTL = 300;
		protected internal bool dfsStrictView = false;
		protected internal bool dfsConvertToFqdn;
		protected internal string logonShare;
		protected internal string defaultDomain;
		protected internal string defaultUserName;
		protected internal string defaultPassword;
		protected internal string netbiosHostname;
		protected internal int netbiosCachePolicy = 60 * 60 * 10;
		protected internal int netbiosSocketTimeout = 5000;
		protected internal int netbiosSendBufferSize = 576;
		protected internal int netbiosRevcBufferSize = 576;
		protected internal int netbiosRetryCount = 2;
		protected internal int netbiosRetryTimeout = 3000;
		protected internal string netbiosScope;
		protected internal int netbiosLocalPort = 0;
		protected internal IPAddress netbiosLocalAddress;
		protected internal string lmhostsFilename;
		protected internal IPAddress[] winsServer = new IPAddress[0];
		protected internal IPAddress broadcastAddress;
		protected internal IList<ResolverType> resolverOrder;
		protected internal int maximumBufferSize = 0x10000;
		protected internal int transactionBufferSize = 0xFFFF - 512;
		protected internal int bufferCacheSize = 16;
		protected internal int smbListSize = 65535;
		protected internal int smbListCount = 200;
		protected internal long smbAttributeExpiration = 5000L;
		protected internal bool ignoreCopyToException = false;
		protected internal int maxRequestRetries = 2;
		protected internal bool traceResourceUsage;
		protected internal bool strictResourceLifecycle;
		protected internal ISet<string> disallowCompound;
		protected internal DialectVersion minVersion;
		protected internal DialectVersion maxVersion;
		protected internal bool requireSecureNegotiate = true;
		protected internal bool sendNTLMTargetName = true;
		private byte[] machineId;
		protected internal string guestUsername = "GUEST";
		protected internal string guestPassword = "";
		protected internal bool allowGuestFallback = false;


		/// <exception cref="CIFSException">
		///  </exception>
		/// throws jcifs.CIFSException
		protected internal BaseConfiguration() : this(false) {
		}


		/// 
		/// <param name="initDefaults">
		///            whether to initialize defaults based on other settings </param>
		/// <exception cref="CIFSException"> </exception>
		/// throws jcifs.CIFSException
		public BaseConfiguration(bool initDefaults) {
			if (initDefaults) {
				this.initDefaults();
			}
		}


		public virtual SecureRandom getRandom() {
			return this.random;
		}


		public virtual string getNetbiosHostname() {
			return this.netbiosHostname;
		}


		public virtual IPAddress getLocalAddr() {
			return this.smbLocalAddress;
		}


		public virtual int getLocalPort() {
			return this.smbLocalPort;
		}


		public virtual int getConnTimeout() {
			return this.smbConnectionTimeout;
		}


		public virtual int getResponseTimeout() {
			return this.smbResponseTimeout;
		}


		public virtual int getSoTimeout() {
			return this.smbSocketTimeout;
		}


		public virtual int getSessionTimeout() {
			return this.smbSessionTimeout;
		}


		public virtual int getSendBufferSize() {
			return this.smbSendBufferSize;
		}


		[Obsolete]
		public virtual int getRecieveBufferSize() {
			return this.smbRecvBufferSize;
		}


		public virtual int getReceiveBufferSize() {
			return this.smbRecvBufferSize;
		}


		public virtual int getNotifyBufferSize() {
			return this.smbNotifyBufferSize;
		}


		public virtual int getMaxMpxCount() {
			return this.maxMpxCount;
		}


		public virtual string getNativeLanman() {
			return this.nativeLanMan;
		}


		public virtual string getNativeOs() {
			return this.nativeOs;
		}


		public virtual int getVcNumber() {
			return this.vcNumber;
		}


		public virtual int getCapabilities() {
			return this.capabilities;
		}


		public virtual DialectVersion getMinimumVersion() {
			return this.minVersion;
		}


		public virtual DialectVersion getMaximumVersion() {
			return this.maxVersion;
		}


		public virtual bool isUseSMB2OnlyNegotiation() {
			return this.smb2OnlyNegotiation;
		}


		public virtual bool isRequireSecureNegotiate() {
			return this.requireSecureNegotiate;
		}


		public virtual bool isPort139FailoverEnabled() {
			return this.port139FailoverEnabled;
		}


		public virtual bool isUseBatching() {
			return this.useBatching;
		}


		public virtual bool isUseUnicode() {
			return this.useUnicode;
		}


		public virtual bool isForceUnicode() {
			return this.forceUnicode;
		}


		public virtual bool isDfsDisabled() {
			return this.dfsDisabled;
		}


		public virtual bool isDfsStrictView() {
			return this.dfsStrictView;
		}


		public virtual long getDfsTtl() {
			return this.dfsTTL;
		}


		public virtual bool isDfsConvertToFQDN() {
			return this.dfsConvertToFqdn;
		}


		public virtual string getLogonShare() {
			return this.logonShare;
		}


		public virtual string getDefaultDomain() {
			return this.defaultDomain;
		}


		public virtual string getDefaultUsername() {
			return this.defaultUserName;
		}


		public virtual string getDefaultPassword() {
			return this.defaultPassword;
		}


		public virtual bool isDisablePlainTextPasswords() {
			return this.disablePlainTextPasswords;
		}


		public virtual int getLanManCompatibility() {
			return this.lanmanCompatibility;
		}


		public virtual bool isAllowNTLMFallback() {
			return this.allowNTLMFallback;
		}


		public virtual bool isUseRawNTLM() {
			return this.useRawNTLM;
		}


		public virtual bool isDisableSpnegoIntegrity() {
			return this.disableSpnegoIntegrity;
		}


		public virtual bool isEnforceSpnegoIntegrity() {
			return this.enforceSpnegoIntegrity;
		}


		public virtual IPAddress getBroadcastAddress() {
			return this.broadcastAddress;
		}


		public virtual IList<ResolverType> getResolveOrder() {
			return this.resolverOrder;
		}


		public virtual IPAddress[] getWinsServers() {
			return this.winsServer;
		}


		public virtual int getNetbiosLocalPort() {
			return this.netbiosLocalPort;
		}


		public virtual IPAddress getNetbiosLocalAddress() {
			return this.netbiosLocalAddress;
		}


		public virtual int getNetbiosSoTimeout() {
			return this.netbiosSocketTimeout;
		}


		public virtual string getNetbiosScope() {
			return this.netbiosScope;
		}


		public virtual int getNetbiosCachePolicy() {
			return this.netbiosCachePolicy;
		}


		public virtual int getNetbiosRcvBufSize() {
			return this.netbiosRevcBufferSize;
		}


		public virtual int getNetbiosRetryCount() {
			return this.netbiosRetryCount;
		}


		public virtual int getNetbiosRetryTimeout() {
			return this.netbiosRetryTimeout;
		}


		public virtual int getNetbiosSndBufSize() {
			return this.netbiosSendBufferSize;
		}


		public virtual string getLmHostsFileName() {
			return this.lmhostsFilename;
		}


		public virtual int getFlags2() {
			return this.flags2;
		}


		public virtual int getSessionLimit() {
			return this.sessionLimit;
		}


		public virtual string getOemEncoding() {
			return this.oemEncoding;
		}


		public virtual TimeZoneInfo getLocalTimezone() {
			return this.localTimeZone;
		}


		public virtual int getPid() {
			return this.localPid;
		}


		public virtual bool isSigningEnabled() {
			return this.signingPreferred;
		}


		public virtual bool isSigningEnforced() {
			return this.signingEnforced;
		}


		public virtual bool isIpcSigningEnforced() {
			return this.ipcSigningEnforced;
		}


		public virtual bool isEncryptionEnabled() {
			return this.encryptionEnabled;
		}


		public virtual bool isForceExtendedSecurity() {
			return this.forceExtendedSecurity;
		}


		public virtual int getTransactionBufferSize() {
			return this.transactionBufferSize;
		}


		public virtual int getMaximumBufferSize() {
			return this.maximumBufferSize;
		}


		public virtual int getBufferCacheSize() {
			return this.bufferCacheSize;
		}


		public virtual int getListCount() {
			return this.smbListCount;
		}


		public virtual int getListSize() {
			return this.smbListSize;
		}


		public virtual long getAttributeCacheTimeout() {
			return this.smbAttributeExpiration;
		}


		public virtual bool isIgnoreCopyToException() {
			return this.ignoreCopyToException;
		}


		public virtual int getMaxRequestRetries() {
			return this.maxRequestRetries;
		}


		public virtual bool isTraceResourceUsage() {
			return this.traceResourceUsage;
		}


		public virtual bool isStrictResourceLifecycle() {
			return this.strictResourceLifecycle;
		}


		public virtual bool isSendNTLMTargetName() {
			return this.sendNTLMTargetName;
		}


		public virtual string getGuestUsername() {
			return this.guestUsername;
		}


		public virtual string getGuestPassword() {
			return this.guestPassword;
		}


		public virtual bool isAllowGuestFallback() {
			return this.allowGuestFallback;
		}


		public virtual byte[] getMachineId() {
			return this.machineId;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#getBatchLimit(java.lang.String) </seealso>
		public virtual int getBatchLimit(string cmd) {
			int? set = this.batchLimits.get(cmd);
			if (set != null) {
				return set.Value;
			}

			set = doGetBatchLimit(cmd);
			if (set != null) {
				this.batchLimits[cmd] = set.Value;
				return set.Value;
			}

			set = DEFAULT_BATCH_LIMITS.get(cmd);
			if (set != null) {
				return set.Value;
			}
			return 1;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#isAllowCompound(java.lang.String) </seealso>
		public virtual bool isAllowCompound(string command) {
			if (this.disallowCompound == null) {
				return true;
			}
			return !this.disallowCompound.Contains(command);
		}


		/// <param name="cmd">
		/// @return </param>
		protected internal virtual int? doGetBatchLimit(string cmd) {
			return null;
		}


		protected internal virtual void initResolverOrder(string ro) {
			this.resolverOrder = new List<ResolverType>();
			if (ro == null || ro.Length == 0) {
				/*
				 * No resolveOrder has been specified, use the
				 * default which is LMHOSTS,DNS,WINS,BCAST or just
				 * LMHOSTS,DNS,BCAST if jcifs.netbios.wins has not
				 * been specified.
				 */
				if (this.winsServer.Length == 0) {
					this.resolverOrder.Add(ResolverType.RESOLVER_LMHOSTS);
					this.resolverOrder.Add(ResolverType.RESOLVER_DNS);
					this.resolverOrder.Add(ResolverType.RESOLVER_BCAST);
				}
				else {
					this.resolverOrder.Add(ResolverType.RESOLVER_LMHOSTS);
					this.resolverOrder.Add(ResolverType.RESOLVER_DNS);
					this.resolverOrder.Add(ResolverType.RESOLVER_WINS);
					this.resolverOrder.Add(ResolverType.RESOLVER_BCAST);
				}
			}
			else {
				StringTokenizer st = new StringTokenizer(ro, ",");
				while (st.hasMoreTokens()) {
					string s = st.nextToken().Trim();
					if (s.Equals("LMHOSTS", StringComparison.OrdinalIgnoreCase)) {
						this.resolverOrder.Add(ResolverType.RESOLVER_LMHOSTS);
					}
					else if (s.Equals("WINS", StringComparison.OrdinalIgnoreCase)) {
						if (this.winsServer.Length == 0) {
							log.error("UniAddress resolveOrder specifies WINS however " + " WINS server has not been configured");
							continue;
						}
						this.resolverOrder.Add(ResolverType.RESOLVER_WINS);
					}
					else if (s.Equals("BCAST", StringComparison.OrdinalIgnoreCase)) {
						this.resolverOrder.Add(ResolverType.RESOLVER_BCAST);
					}
					else if (s.Equals("DNS", StringComparison.OrdinalIgnoreCase)) {
						this.resolverOrder.Add(ResolverType.RESOLVER_DNS);
					}
					else {
						log.error("unknown resolver method: " + s);
					}
				}
			}
		}


		protected internal virtual void initProtocolVersions(string minStr, string maxStr) {
			DialectVersion min = (minStr != null && minStr.Length > 0) ? DialectVersion.valueOf(minStr) : null;
			DialectVersion max = (maxStr != null && maxStr.Length > 0) ? DialectVersion.valueOf(maxStr) : null;
			initProtocolVersions(min, max);
		}


		protected internal virtual void initProtocolVersions(DialectVersion min, DialectVersion max) {
			this.minVersion = min != null ? min : DialectVersion.SMB1;
			this.maxVersion = max != null ? max : DialectVersion.SMB210;

			if (this.minVersion.atLeast(this.maxVersion)) {
				this.maxVersion = this.minVersion;
			}
		}


		protected internal virtual void initDisallowCompound(string prop) {
			if (prop == null) {
				return;
			}
			ISet<string> disallow = new HashSet<string>();
			StringTokenizer st = new StringTokenizer(prop, ",");
			while (st.hasMoreTokens()) {
				disallow.Add(st.nextToken().Trim());
			}
			this.disallowCompound = disallow;
		}


		/// throws jcifs.CIFSException
		protected internal virtual void initDefaults() {

			try {
				"".getBytes(SmbConstants.DEFAULT_OEM_ENCODING);
			}
			catch (Exception) {
				throw new CIFSException("The default OEM encoding " + SmbConstants.DEFAULT_OEM_ENCODING + " does not appear to be supported by this JRE.");
			}

			this.localPid = (int)(RuntimeHelp.nextDouble() * 65536d);
			this.localTimeZone = System.TimeZoneInfo.Local; //TimeZone.getDefault(); 
			this.random = new SecureRandom();

			if (this.machineId == null) {
				byte[] mid = new byte[32];
				this.random.NextBytes(mid);
				this.machineId = mid;
			}

			if (this.nativeOs==null) {
				this.nativeOs = RuntimeHelp.getOsName();
			}

			if (this.flags2 == 0) {
				this.flags2 = SmbConstants.FLAGS2_LONG_FILENAMES | SmbConstants.FLAGS2_EXTENDED_ATTRIBUTES | (this.useExtendedSecurity ? SmbConstants.FLAGS2_EXTENDED_SECURITY_NEGOTIATION : 0) | (this.signingPreferred ? SmbConstants.FLAGS2_SECURITY_SIGNATURES : 0) | (this.useNtStatus ? SmbConstants.FLAGS2_STATUS32 : 0) | (this.useUnicode || this.forceUnicode ? SmbConstants.FLAGS2_UNICODE : 0);
			}

			if (this.capabilities == 0) {
				this.capabilities = (this.useNTSmbs ? SmbConstants.CAP_NT_SMBS : 0) | (this.useNtStatus ? SmbConstants.CAP_STATUS32 : 0) | (this.useExtendedSecurity ? SmbConstants.CAP_EXTENDED_SECURITY : 0) | (this.useLargeReadWrite ? SmbConstants.CAP_LARGE_READX : 0) | (this.useLargeReadWrite ? SmbConstants.CAP_LARGE_WRITEX : 0) | (this.useUnicode ? SmbConstants.CAP_UNICODE : 0);
			}

			if (this.broadcastAddress == null) {
				try {
					this.broadcastAddress = IPAddress.Parse("255.255.255.255");
				}
				catch (FormatException uhe) {
					log.debug("Failed to get broadcast address", uhe);
				}
			}

			if (this.resolverOrder == null) {
				initResolverOrder(null);
			}

			if (this.minVersion == null || this.maxVersion == null) {
				initProtocolVersions((DialectVersion) null, null);
			}

			if (this.disallowCompound == null) {
				// Samba woes on these
				// Smb2SessionSetupRequest + X -> INTERNAL_ERROR
				// Smb2TreeConnectRequest + IoCtl -> NETWORK_NAME_DELETED
				this.disallowCompound = new HashSet<string>();
				this.disallowCompound.Add("Smb2SessionSetupRequest");
				this.disallowCompound.Add(" Smb2TreeConnectRequest");
			}
		}

	}

}