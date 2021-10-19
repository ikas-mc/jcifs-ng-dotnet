using System;
using System.Collections.Generic;
using System.Net;
using Org.BouncyCastle.Security;

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
namespace jcifs {



	/// 
	/// 
	/// <summary>
	/// Implementors of this interface should extend <seealso cref="jcifs.config.BaseConfiguration"/> or
	/// <seealso cref="jcifs.config.DelegatingConfiguration"/> to get forward compatibility.
	/// 
	/// @author mbechler
	/// 
	/// </summary>
	public interface Configuration {

		/// 
		/// <returns> random source to use </returns>
		SecureRandom getRandom();


		/// 
		/// 
		/// <summary>
		/// Property <tt>jcifs.smb.client.dfs.ttl</tt> (int, default 300)
		/// </summary>
		/// <returns> title to live, in seconds, for DFS cache entries </returns>
		long getDfsTtl();


		/// 
		/// <summary>
		/// Property <tt>jcifs.smb.client.dfs.strictView</tt> (boolean, default false)
		/// </summary>
		/// <returns> whether a authentication failure during DFS resolving will throw an exception </returns>
		bool isDfsStrictView();


		/// 
		/// <summary>
		/// Property <tt>jcifs.smb.client.dfs.disabled</tt> (boolean, default false)
		/// </summary>
		/// <returns> whether DFS lookup is disabled </returns>
		bool isDfsDisabled();


		/// <summary>
		/// Enable hack to make kerberos auth work with DFS sending short names
		/// 
		/// This works by appending the domain name to the netbios short name and will fail horribly if this mapping is not
		/// correct for your domain.
		/// 
		/// Property <tt>jcifs.smb.client.dfs.convertToFQDN</tt> (boolean, default false)
		/// </summary>
		/// <returns> whether to convert NetBIOS names returned by DFS to FQDNs </returns>
		bool isDfsConvertToFQDN();


		/// <summary>
		/// Minimum protocol version
		/// 
		/// Property <tt>jcifs.smb.client.minVersion</tt> (string, default SMB1)
		/// </summary>
		/// <seealso cref= DialectVersion </seealso>
		/// <returns> minimum protocol version to use/allow
		/// @since 2.1 </returns>
		DialectVersion getMinimumVersion();


		/// <summary>
		/// Maximum protocol version
		/// 
		/// Property <tt>jcifs.smb.client.maxVersion</tt> (string, default SMB210)
		/// </summary>
		/// <seealso cref= DialectVersion </seealso>
		/// <returns> maximum protocol version to use/allow
		/// @since 2.1 </returns>
		DialectVersion getMaximumVersion();


		/// <summary>
		/// Use SMB2 non-backward compatible negotiation style
		/// 
		/// Property <tt>jcifs.smb.client.useSMB2Negotiation</tt> (boolean, default false)
		/// </summary>
		/// <returns> whether to use non-backward compatible protocol negotiation </returns>
		bool isUseSMB2OnlyNegotiation();


		/// <summary>
		/// Enforce secure negotiation
		/// 
		/// Property <tt>jcifs.smb.client.requireSecureNegotiate</tt> (boolean, default true)
		/// 
		/// This does not provide any actual downgrade protection if SMB1 is allowed.
		/// 
		/// It will also break connections with SMB2 servers that do not properly sign error responses.
		/// </summary>
		/// <returns> whether to enforce the use of secure negotiation. </returns>
		bool isRequireSecureNegotiate();


		/// <summary>
		/// Enable port 139 failover
		/// 
		/// Property <tt>jcifs.smb.client.port139.enabled</tt> (boolean, default false)
		/// </summary>
		/// <returns> whether to failover to legacy transport on port 139 </returns>
		bool isPort139FailoverEnabled();


		/// 
		/// <summary>
		/// Property <tt>jcifs.smb.client.useUnicode</tt> (boolean, default true)
		/// </summary>
		/// <returns> whether to announce support for unicode </returns>
		bool isUseUnicode();


		/// 
		/// <summary>
		/// Property <tt>jcifs.smb.client.forceUnicode</tt> (boolean, default false)
		/// </summary>
		/// <returns> whether to use unicode, even if the server does not announce it </returns>
		bool isForceUnicode();


		/// 
		/// <summary>
		/// Property <tt>jcifs.smb.client.useBatching</tt> (boolean, default true)
		/// </summary>
		/// <returns> whether to enable support for SMB1 AndX command batching </returns>
		bool isUseBatching();


		/// 
		/// <summary>
		/// Property <tt>jcifs.smb.client.nativeOs</tt> (string, default <tt>os.name</tt>)
		/// </summary>
		/// <returns> OS string to report </returns>
		string getNativeOs();


		/// 
		/// <summary>
		/// Property <tt>jcifs.smb.client.nativeLanMan</tt> (string, default <tt>jCIFS</tt>)
		/// </summary>
		/// <returns> Lanman string to report </returns>
		string getNativeLanman();


		/// 
		/// <summary>
		/// Property <tt>jcifs.smb.client.rcv_buf_size</tt> (int, default 65535)
		/// </summary>
		/// <returns> receive buffer size, in bytes </returns>
		/// @deprecated use getReceiveBufferSize instead 
		[Obsolete("use getReceiveBufferSize instead")]
		int getRecieveBufferSize();


		/// 
		/// <summary>
		/// Property <tt>jcifs.smb.client.rcv_buf_size</tt> (int, default 65535)
		/// </summary>
		/// <returns> receive buffer size, in bytes </returns>
		int getReceiveBufferSize();


		/// 
		/// <summary>
		/// Property <tt>jcifs.smb.client.snd_buf_size</tt> (int, default 65535)
		/// </summary>
		/// <returns> send buffer size, in bytes </returns>
		int getSendBufferSize();


		/// 
		/// <summary>
		/// Property <tt>jcifs.smb.client.soTimeout</tt> (int, default 35000)
		/// </summary>
		/// <returns> socket timeout, in milliseconds </returns>
		int getSoTimeout();


		/// 
		/// <summary>
		/// Property <tt>jcifs.smb.client.connTimeout</tt> (int, default 35000)
		/// </summary>
		/// <returns> timeout for establishing a socket connection, in milliseconds </returns>
		int getConnTimeout();


		/// <summary>
		/// Property <tt>jcifs.smb.client.sessionTimeout</tt> (int, default 35000)
		/// 
		/// </summary>
		/// <returns> timeout for SMB sessions, in milliseconds </returns>
		int getSessionTimeout();


		/// 
		/// <summary>
		/// Property <tt>jcifs.smb.client.responseTimeout</tt> (int, default 30000)
		/// </summary>
		/// <returns> timeout for SMB responses, in milliseconds </returns>
		int getResponseTimeout();


		/// 
		/// <summary>
		/// Property <tt>jcifs.smb.client.lport</tt> (int)
		/// </summary>
		/// <returns> local port to use for outgoing connections </returns>
		int getLocalPort();


		/// 
		/// <summary>
		/// Property <tt>jcifs.smb.client.laddr</tt> (string)
		/// </summary>
		/// <returns> local address to use for outgoing connections </returns>
		IPAddress getLocalAddr();


		/// 
		/// <summary>
		/// Property <tt>jcifs.netbios.hostname</tt> (string)
		/// </summary>
		/// <returns> local NETBIOS/short name to announce </returns>
		string getNetbiosHostname();


		/// 
		/// <summary>
		/// Property <tt>jcifs.smb.client.logonShare</tt>
		/// </summary>
		/// <returns> share to connect to during authentication, if unset connect to IPC$ </returns>
		string getLogonShare();


		/// 
		/// 
		/// <summary>
		/// Property <tt>jcifs.smb.client.domain</tt>
		/// </summary>
		/// <returns> default credentials, domain name </returns>
		string getDefaultDomain();


		/// 
		/// <summary>
		/// Property <tt>jcifs.smb.client.username</tt>
		/// </summary>
		/// <returns> default credentials, user name </returns>
		string getDefaultUsername();


		/// 
		/// <summary>
		/// Property <tt>jcifs.smb.client.password</tt>
		/// </summary>
		/// <returns> default credentials, password </returns>
		string getDefaultPassword();


		/// <summary>
		/// Lanman compatibility level
		/// 
		/// {@href https://technet.microsoft.com/en-us/library/cc960646.aspx}
		/// 
		/// 
		/// <table>
		/// <tr>
		/// <td>0 or 1</td>
		/// <td>LM and NTLM</td>
		/// </tr>
		/// <tr>
		/// <td>2</td>
		/// <td>NTLM only</td>
		/// </tr>
		/// <tr>
		/// <td>3-5</td>
		/// <td>NTLMv2 only</td>
		/// </tr>
		/// </table>
		/// 
		/// 
		/// Property <tt>jcifs.smb.lmCompatibility</tt> (int, default 3)
		/// </summary>
		/// <returns> lanman compatibility level, defaults to 3 i.e. NTLMv2 only </returns>
		int getLanManCompatibility();


		/// 
		/// <summary>
		/// Property <tt>jcifs.smb.allowNTLMFallback</tt> (boolean, default true)
		/// </summary>
		/// <returns> whether to allow fallback from kerberos to NTLM </returns>
		bool isAllowNTLMFallback();


		/// <summary>
		/// Property <tt>jcifs.smb.useRawNTLM</tt> (boolean, default false)
		/// </summary>
		/// <returns> whether to use raw NTLMSSP tokens instead of SPNEGO wrapped ones
		/// @since 2.1 </returns>
		bool isUseRawNTLM();


		/// 
		/// <summary>
		/// Property <tt>jcifs.smb.client.disablePlainTextPasswords</tt> (boolean, default true)
		/// </summary>
		/// <returns> whether the usage of plaintext passwords is prohibited, defaults to false </returns>
		bool isDisablePlainTextPasswords();


		/// 
		/// 
		/// <summary>
		/// Property <tt>jcifs.resolveOrder</tt> (string, default <tt>LMHOSTS,DNS,WINS,BCAST</tt>)
		/// </summary>
		/// <returns> order and selection of resolver modules, see <seealso cref="ResolverType"/> </returns>
		IList<ResolverType> getResolveOrder();


		/// 
		/// <summary>
		/// Property <tt>jcifs.netbios.baddr</tt> (string, default <tt>255.255.255.255</tt>)
		/// </summary>
		/// <returns> broadcast address to use </returns>
		IPAddress getBroadcastAddress();


		/// 
		/// 
		/// <summary>
		/// Property <tt>jcifs.netbios.wins</tt> (string, comma separated)
		/// </summary>
		/// <returns> WINS server to use </returns>
		IPAddress[] getWinsServers();


		/// 
		/// <summary>
		/// Property <tt>jcifs.netbios.lport</tt> (int)
		/// </summary>
		/// <returns> local bind port for nebios connections </returns>
		int getNetbiosLocalPort();


		/// 
		/// <summary>
		/// Property <tt>jcifs.netbios.laddr</tt> (string)
		/// </summary>
		/// <returns> local bind address for netbios connections </returns>
		IPAddress getNetbiosLocalAddress();


		/// 
		/// 
		/// <summary>
		/// Property <tt>jcifs.netbios.soTimeout</tt> (int, default 5000)
		/// </summary>
		/// <returns> socket timeout for netbios connections, in milliseconds </returns>
		int getNetbiosSoTimeout();


		/// 
		/// 
		/// <returns> virtual circuit number to use </returns>
		int getVcNumber();


		/// 
		/// <summary>
		/// Property <tt>jcifs.smb.client.capabilities</tt> (int)
		/// </summary>
		/// <returns> custom capabilities </returns>
		int getCapabilities();


		/// 
		/// 
		/// <summary>
		/// Property <tt>jcifs.smb.client.flags2</tt> (int)
		/// </summary>
		/// <returns> custom flags2 </returns>
		int getFlags2();


		/// 
		/// <summary>
		/// Property <tt>jcifs.smb.client.ssnLimit</tt> (int, 250)
		/// </summary>
		/// <returns> maximum number of sessions on a single connection </returns>
		int getSessionLimit();


		/// 
		/// <summary>
		/// Property <tt>jcifs.encoding</tt> (string, default <tt>Cp850</tt>)
		/// </summary>
		/// <returns> OEM encoding to use </returns>
		string getOemEncoding();


		/// <returns> local timezone </returns>
		TimeZoneInfo getLocalTimezone();


		/// <returns> Process id to send, randomized if unset </returns>
		int getPid();


		/// 
		/// <summary>
		/// Property <tt>jcifs.smb.client.maxMpxCount</tt> (int, default 10)
		/// </summary>
		/// <returns> maximum count of concurrent commands to announce </returns>
		int getMaxMpxCount();


		/// 
		/// <summary>
		/// Property <tt>jcifs.smb.client.signingPreferred</tt> (boolean, default false)
		/// </summary>
		/// <returns> whether to enable SMB signing (for everything), if available </returns>
		bool isSigningEnabled();


		/// 
		/// <summary>
		/// Property <tt>jcifs.smb.client.ipcSigningEnforced</tt> (boolean, default true)
		/// </summary>
		/// <returns> whether to enforce SMB signing for IPC connections </returns>
		bool isIpcSigningEnforced();


		/// 
		/// <summary>
		/// Property <tt>jcifs.smb.client.signingEnforced</tt> (boolean, default false)
		/// </summary>
		/// <returns> whether to enforce SMB signing (for everything) </returns>
		bool isSigningEnforced();


		/// <summary>
		/// Property <tt>jcifs.smb.client.encryptionEnabled</tt> (boolean, default false)
		/// 
		/// This is an experimental option allowing to indicate support during protocol
		/// negotiation, SMB encryption is not implemented yet.
		/// </summary>
		/// <returns> whether SMB encryption is enabled
		/// @since 2.1 </returns>
		bool isEncryptionEnabled();


		/// 
		/// <summary>
		/// Property <tt>jcifs.smb.client.forceExtendedSecurity</tt> (boolean, default false)
		/// </summary>
		/// <returns> whether to force extended security usage </returns>
		bool isForceExtendedSecurity();


		/// 
		/// 
		/// <summary>
		/// Property <tt>jcifs.netbios.lmhosts</tt> (string)
		/// </summary>
		/// <returns> lmhosts file to use </returns>
		string getLmHostsFileName();


		/// 
		/// <summary>
		/// Property <tt>jcifs.netbios.scope</tt> (string)
		/// </summary>
		/// <returns> default netbios scope to set in requests </returns>
		string getNetbiosScope();


		/// 
		/// <summary>
		/// Property <tt>jcifs.netbios.snd_buf_size</tt> (int, default 576)
		/// </summary>
		/// <returns> netbios send buffer size </returns>
		int getNetbiosSndBufSize();


		/// 
		/// <summary>
		/// Property <tt>jcifs.netbios.rcv_buf_size</tt> (int, default 576)
		/// </summary>
		/// <returns> netbios recieve buffer size </returns>
		int getNetbiosRcvBufSize();


		/// 
		/// <summary>
		/// Property <tt>jcifs.netbios.retryTimeout</tt> (int, default 3000)
		/// </summary>
		/// <returns> timeout of retry requests, in milliseconds </returns>
		int getNetbiosRetryTimeout();


		/// 
		/// <summary>
		/// Property <tt>jcifs.netbios.retryCount</tt> (int, default 2)
		/// </summary>
		/// <returns> maximum number of retries for netbios requests </returns>
		int getNetbiosRetryCount();


		/// 
		/// 
		/// <summary>
		/// Property <tt>jcifs.netbios.cachePolicy</tt> in minutes (int, default 600)
		/// </summary>
		/// <returns> netbios cache timeout, in seconds, 0 - disable caching, -1 - cache forever </returns>
		int getNetbiosCachePolicy();


		/// 
		/// <returns> the maximum size of IO buffers, limits the maximum message size </returns>
		int getMaximumBufferSize();


		/// 
		/// <summary>
		/// Property <tt>jcifs.smb.client.transaction_buf_size</tt> (int, default 65535)
		/// </summary>
		/// <returns> maximum data size for SMB transactions </returns>
		int getTransactionBufferSize();


		/// 
		/// <summary>
		/// Property <tt>jcifs.smb.maxBuffers</tt> (int, default 16)
		/// </summary>
		/// <returns> number of buffers to keep in cache </returns>
		int getBufferCacheSize();


		/// 
		/// <summary>
		/// Property <tt>jcifs.smb.client.listCount</tt> (int, default 200)
		/// </summary>
		/// <returns> maxmimum number of elements to request in a list request </returns>
		int getListCount();


		/// 
		/// <summary>
		/// Property <tt>jcifs.smb.client.listSize</tt> (int, default 65535)
		/// </summary>
		/// <returns> maximum data size for list requests </returns>
		int getListSize();


		/// 
		/// 
		/// <summary>
		/// Property <tt>jcifs.smb.client.attrExpirationPeriod</tt> (int, 5000)
		/// </summary>
		/// <returns> timeout of file attribute cache </returns>
		long getAttributeCacheTimeout();


		/// 
		/// 
		/// <summary>
		/// Property <tt>jcifs.smb.client.ignoreCopyToException</tt> (boolean, false)
		/// </summary>
		/// <returns> whether to ignore exceptions that occur during file copy </returns>
		bool isIgnoreCopyToException();


		/// <param name="cmd"> </param>
		/// <returns> the batch limit for the given command </returns>
		int getBatchLimit(string cmd);


		/// 
		/// <summary>
		/// Property <tt>jcifs.smb.client.notify_buf_size</tt> (int, default 1024)
		/// </summary>
		/// <returns> the size of the requested server notify buffer </returns>
		int getNotifyBufferSize();


		/// 
		/// 
		/// <summary>
		/// Property <tt>jcifs.smb.client.maxRequestRetries</tt> (int, default 2)
		/// </summary>
		/// <returns> retry SMB requests on failure up to n times </returns>
		int getMaxRequestRetries();


		/// <summary>
		/// Property <tt>jcifs.smb.client.strictResourceLifecycle</tt> (bool, default false)
		/// 
		/// If enabled, SmbFile instances starting with their first use will hold a reference to their tree.
		/// This means that trees/sessions/connections won't be idle-disconnected even if there are no other active
		/// references (currently executing code, file descriptors).
		/// 
		/// Depending on the usage scenario, this may have some benefit as there won't be any delays for restablishing these
		/// resources, however comes at the cost of having to properly release all SmbFile instances you no longer need.
		/// </summary>
		/// <returns> whether to use strict resource lifecycle </returns>
		bool isStrictResourceLifecycle();


		/// <summary>
		/// This is solely intended for debugging
		/// </summary>
		/// <returns> whether to track the locations from which resources were created </returns>
		bool isTraceResourceUsage();


		/// <param name="command"> </param>
		/// <returns> whether to allow creating compound requests with that command </returns>
		bool isAllowCompound(string command);


		/// <summary>
		/// Machine identifier
		/// 
		/// ClientGuid, ... are derived from this value.
		/// 
		/// Normally this should be randomly assigned for each client instance/configuration.
		/// </summary>
		/// <returns> machine identifier (32 byte) </returns>
		byte[] getMachineId();


		/// 
		/// 
		/// <summary>
		/// Property <tt>jcifs.smb.client.disableSpnegoIntegrity</tt> (boolean, false)
		/// </summary>
		/// <returns> whether to disable sending/verifying SPNEGO mechanismListMIC </returns>
		bool isDisableSpnegoIntegrity();


		/// 
		/// <summary>
		/// Property <tt>jcifs.smb.client.enforceSpnegoIntegrity</tt> (boolean, false)
		/// </summary>
		/// <returns> whether to enforce verifying SPNEGO mechanismListMIC </returns>
		bool isEnforceSpnegoIntegrity();


		/// <summary>
		/// Property <tt>jcifs.smb.client.SendNTLMTargetName</tt> (boolean, true)
		/// </summary>
		/// <returns> whether to send an AvTargetName with the NTLM exchange </returns>
		bool isSendNTLMTargetName();


		/// <summary>
		/// Property <tt>jcifs.smb.client.guestPassword</tt>, defaults to empty string
		/// </summary>
		/// <returns> password used when guest authentication is requested </returns>
		string getGuestPassword();


		/// <summary>
		/// Property <tt>jcifs.smb.client.guestUsername</tt>, defaults to GUEST
		/// </summary>
		/// <returns> username used when guest authentication is requested </returns>
		string getGuestUsername();


		/// <summary>
		/// Property <tt>jcifs.smb.client.allowGuestFallback</tt>, defaults to false
		/// </summary>
		/// <returns> whether to permit guest logins when user authentication is requested </returns>
		bool isAllowGuestFallback();
	}

}