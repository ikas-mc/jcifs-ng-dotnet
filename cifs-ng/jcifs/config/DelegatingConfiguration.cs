using System;
using System.Collections.Generic;
using System.Net;
using Org.BouncyCastle.Security;
using Configuration = jcifs.Configuration;
using DialectVersion = jcifs.DialectVersion;
using ResolverType = jcifs.ResolverType;

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
	public class DelegatingConfiguration : Configuration {

		private readonly Configuration @delegate;


		/// <param name="delegate">
		///            delegate to pass all non-overridden method calls to
		///  </param>
		public DelegatingConfiguration(Configuration @delegate) {
			this.@delegate = @delegate;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#getRandom() </seealso>
		public virtual SecureRandom getRandom() {
			return this.@delegate.getRandom();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#getMinimumVersion() </seealso>
		public virtual DialectVersion getMinimumVersion() {
			return this.@delegate.getMinimumVersion();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#getMaximumVersion() </seealso>
		public virtual DialectVersion getMaximumVersion() {
			return this.@delegate.getMaximumVersion();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#isUseSMB2OnlyNegotiation() </seealso>
		public virtual bool isUseSMB2OnlyNegotiation() {
			return this.@delegate.isUseSMB2OnlyNegotiation();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#isRequireSecureNegotiate() </seealso>
		public virtual bool isRequireSecureNegotiate() {
			return this.@delegate.isRequireSecureNegotiate();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#isSendNTLMTargetName() </seealso>
		public virtual bool isSendNTLMTargetName() {
			return this.@delegate.isSendNTLMTargetName();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#isPort139FailoverEnabled() </seealso>
		public virtual bool isPort139FailoverEnabled() {
			return this.@delegate.isPort139FailoverEnabled();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#getDfsTtl() </seealso>
		public virtual long getDfsTtl() {
			return this.@delegate.getDfsTtl();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#isDfsStrictView() </seealso>
		public virtual bool isDfsStrictView() {
			return this.@delegate.isDfsStrictView();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#isDfsDisabled() </seealso>
		public virtual bool isDfsDisabled() {
			return this.@delegate.isDfsDisabled();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#isDfsConvertToFQDN() </seealso>
		public virtual bool isDfsConvertToFQDN() {
			return this.@delegate.isDfsConvertToFQDN();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#isForceUnicode() </seealso>
		public virtual bool isForceUnicode() {
			return this.@delegate.isForceUnicode();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#isUseUnicode() </seealso>
		public virtual bool isUseUnicode() {
			return this.@delegate.isUseUnicode();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#isUseBatching() </seealso>
		public virtual bool isUseBatching() {
			return this.@delegate.isUseBatching();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#getNativeOs() </seealso>
		public virtual string getNativeOs() {
			return this.@delegate.getNativeOs();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#getNativeLanman() </seealso>
		public virtual string getNativeLanman() {
			return this.@delegate.getNativeLanman();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#getMaximumBufferSize() </seealso>
		public virtual int getMaximumBufferSize() {
			return this.@delegate.getMaximumBufferSize();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// @deprecated use getReceiveBufferSize instead 
		[Obsolete("use getReceiveBufferSize instead")]
		public virtual int getRecieveBufferSize() {
			return this.@delegate.getReceiveBufferSize();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#getReceiveBufferSize() </seealso>
		public virtual int getReceiveBufferSize() {
			return this.@delegate.getReceiveBufferSize();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#getSendBufferSize() </seealso>
		public virtual int getSendBufferSize() {
			return this.@delegate.getSendBufferSize();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#getNotifyBufferSize() </seealso>
		public virtual int getNotifyBufferSize() {
			return this.@delegate.getNotifyBufferSize();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#getSoTimeout() </seealso>
		public virtual int getSoTimeout() {
			return this.@delegate.getSoTimeout();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#getResponseTimeout() </seealso>
		public virtual int getResponseTimeout() {
			return this.@delegate.getResponseTimeout();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#getConnTimeout() </seealso>
		public virtual int getConnTimeout() {
			return this.@delegate.getConnTimeout();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#getSessionTimeout() </seealso>
		public virtual int getSessionTimeout() {
			return this.@delegate.getSessionTimeout();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#getLocalPort() </seealso>
		public virtual int getLocalPort() {
			return this.@delegate.getLocalPort();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#getLocalAddr() </seealso>
		public virtual IPAddress getLocalAddr() {
			return this.@delegate.getLocalAddr();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#getNetbiosHostname() </seealso>
		public virtual string getNetbiosHostname() {
			return this.@delegate.getNetbiosHostname();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#getLogonShare() </seealso>
		public virtual string getLogonShare() {
			return this.@delegate.getLogonShare();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#getDefaultDomain() </seealso>
		public virtual string getDefaultDomain() {
			return this.@delegate.getDefaultDomain();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#getDefaultUsername() </seealso>
		public virtual string getDefaultUsername() {
			return this.@delegate.getDefaultUsername();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#getDefaultPassword() </seealso>
		public virtual string getDefaultPassword() {
			return this.@delegate.getDefaultPassword();
		}


		/// 
		/// <seealso cref= jcifs.Configuration#isDisablePlainTextPasswords() </seealso>
		public virtual bool isDisablePlainTextPasswords() {
			return this.@delegate.isDisablePlainTextPasswords();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#isForceExtendedSecurity() </seealso>
		public virtual bool isForceExtendedSecurity() {
			return this.@delegate.isForceExtendedSecurity();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#getLanManCompatibility() </seealso>
		public virtual int getLanManCompatibility() {
			return this.@delegate.getLanManCompatibility();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#isAllowNTLMFallback() </seealso>
		public virtual bool isAllowNTLMFallback() {
			return this.@delegate.isAllowNTLMFallback();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#isUseRawNTLM() </seealso>
		public virtual bool isUseRawNTLM() {
			return this.@delegate.isUseRawNTLM();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#isDisableSpnegoIntegrity() </seealso>
		public virtual bool isDisableSpnegoIntegrity() {
			return this.@delegate.isDisableSpnegoIntegrity();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#isEnforceSpnegoIntegrity() </seealso>
		public virtual bool isEnforceSpnegoIntegrity() {
			return this.@delegate.isEnforceSpnegoIntegrity();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#getResolveOrder() </seealso>
		public virtual IList<ResolverType> getResolveOrder() {
			return this.@delegate.getResolveOrder();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#getBroadcastAddress() </seealso>
		public virtual IPAddress getBroadcastAddress() {
			return this.@delegate.getBroadcastAddress();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#getWinsServers() </seealso>
		public virtual IPAddress[] getWinsServers() {
			return this.@delegate.getWinsServers();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#getNetbiosLocalPort() </seealso>
		public virtual int getNetbiosLocalPort() {
			return this.@delegate.getNetbiosLocalPort();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#getNetbiosLocalAddress() </seealso>
		public virtual IPAddress getNetbiosLocalAddress() {
			return this.@delegate.getNetbiosLocalAddress();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#getVcNumber() </seealso>
		public virtual int getVcNumber() {
			return this.@delegate.getVcNumber();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#getCapabilities() </seealso>
		public virtual int getCapabilities() {
			return this.@delegate.getCapabilities();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#getFlags2() </seealso>
		public virtual int getFlags2() {
			return this.@delegate.getFlags2();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#getSessionLimit() </seealso>
		public virtual int getSessionLimit() {
			return this.@delegate.getSessionLimit();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#getOemEncoding() </seealso>
		public virtual string getOemEncoding() {
			return this.@delegate.getOemEncoding();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#getLocalTimezone() </seealso>
		public virtual TimeZoneInfo getLocalTimezone() {
			return this.@delegate.getLocalTimezone();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#getPid() </seealso>
		public virtual int getPid() {
			return this.@delegate.getPid();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#getMaxMpxCount() </seealso>
		public virtual int getMaxMpxCount() {
			return this.@delegate.getMaxMpxCount();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#isSigningEnabled() </seealso>
		public virtual bool isSigningEnabled() {
			return this.@delegate.isSigningEnabled();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#isSigningEnforced() </seealso>
		public virtual bool isSigningEnforced() {
			return this.@delegate.isSigningEnforced();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#isIpcSigningEnforced() </seealso>
		public virtual bool isIpcSigningEnforced() {
			return this.@delegate.isIpcSigningEnforced();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#isEncryptionEnabled() </seealso>
		public virtual bool isEncryptionEnabled() {
			return this.@delegate.isEncryptionEnabled();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#getLmHostsFileName() </seealso>
		public virtual string getLmHostsFileName() {
			return this.@delegate.getLmHostsFileName();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#getNetbiosScope() </seealso>
		public virtual string getNetbiosScope() {
			return this.@delegate.getNetbiosScope();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#getNetbiosSoTimeout() </seealso>
		public virtual int getNetbiosSoTimeout() {
			return this.@delegate.getNetbiosSoTimeout();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#getNetbiosSndBufSize() </seealso>
		public virtual int getNetbiosSndBufSize() {
			return this.@delegate.getNetbiosSndBufSize();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#getNetbiosRetryTimeout() </seealso>
		public virtual int getNetbiosRetryTimeout() {
			return this.@delegate.getNetbiosRetryTimeout();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#getNetbiosRetryCount() </seealso>
		public virtual int getNetbiosRetryCount() {
			return this.@delegate.getNetbiosRetryCount();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#getNetbiosRcvBufSize() </seealso>
		public virtual int getNetbiosRcvBufSize() {
			return this.@delegate.getNetbiosRcvBufSize();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#getNetbiosCachePolicy() </seealso>
		public virtual int getNetbiosCachePolicy() {
			return this.@delegate.getNetbiosCachePolicy();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#getTransactionBufferSize() </seealso>
		public virtual int getTransactionBufferSize() {
			return this.@delegate.getTransactionBufferSize();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#getBufferCacheSize() </seealso>
		public virtual int getBufferCacheSize() {
			return this.@delegate.getBufferCacheSize();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#getListCount() </seealso>
		public virtual int getListCount() {
			return this.@delegate.getListCount();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#getListSize() </seealso>
		public virtual int getListSize() {
			return this.@delegate.getListSize();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#getAttributeCacheTimeout() </seealso>
		public virtual long getAttributeCacheTimeout() {
			return this.@delegate.getAttributeCacheTimeout();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#isIgnoreCopyToException() </seealso>
		public virtual bool isIgnoreCopyToException() {
			return this.@delegate.isIgnoreCopyToException();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#getBatchLimit(java.lang.String) </seealso>
		public virtual int getBatchLimit(string cmd) {
			return this.@delegate.getBatchLimit(cmd);
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#isAllowCompound(java.lang.String) </seealso>
		public virtual bool isAllowCompound(string command) {
			return this.@delegate.isAllowCompound(command);
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#isTraceResourceUsage() </seealso>
		public virtual bool isTraceResourceUsage() {
			return this.@delegate.isTraceResourceUsage();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#isStrictResourceLifecycle() </seealso>
		public virtual bool isStrictResourceLifecycle() {
			return this.@delegate.isStrictResourceLifecycle();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#getMaxRequestRetries() </seealso>
		public virtual int getMaxRequestRetries() {
			return this.@delegate.getMaxRequestRetries();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#getMachineId() </seealso>
		public virtual byte[] getMachineId() {
			return this.@delegate.getMachineId();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#getGuestUsername() </seealso>
		public virtual string getGuestUsername() {
			return this.@delegate.getGuestUsername();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#getGuestPassword() </seealso>
		public virtual string getGuestPassword() {
			return this.@delegate.getGuestPassword();
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.Configuration#isAllowGuestFallback() </seealso>
		public virtual bool isAllowGuestFallback() {
			return this.@delegate.isAllowGuestFallback();
		}
	}

}