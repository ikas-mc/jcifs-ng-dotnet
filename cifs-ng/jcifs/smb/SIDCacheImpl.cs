using jcifs;

using System.Collections.Generic;
using System.IO;
using cifs_ng.lib.ext;
using CIFSContext = jcifs.CIFSContext;
using CIFSException = jcifs.CIFSException;
using SidResolver = jcifs.SidResolver;
using DcerpcHandle = jcifs.dcerpc.DcerpcHandle;
using UnicodeString = jcifs.dcerpc.UnicodeString;
using rpc = jcifs.dcerpc.rpc;
using sid_t = jcifs.dcerpc.rpc.sid_t;
using LsaPolicyHandle = jcifs.dcerpc.msrpc.LsaPolicyHandle;
using MsrpcEnumerateAliasesInDomain = jcifs.dcerpc.msrpc.MsrpcEnumerateAliasesInDomain;
using MsrpcGetMembersInAlias = jcifs.dcerpc.msrpc.MsrpcGetMembersInAlias;
using MsrpcLookupSids = jcifs.dcerpc.msrpc.MsrpcLookupSids;
using MsrpcQueryInformationPolicy = jcifs.dcerpc.msrpc.MsrpcQueryInformationPolicy;
using SamrAliasHandle = jcifs.dcerpc.msrpc.SamrAliasHandle;
using SamrDomainHandle = jcifs.dcerpc.msrpc.SamrDomainHandle;
using SamrPolicyHandle = jcifs.dcerpc.msrpc.SamrPolicyHandle;
using lsarpc = jcifs.dcerpc.msrpc.lsarpc;
using LsarTranslatedName = jcifs.dcerpc.msrpc.lsarpc.LsarTranslatedName;
using samr = jcifs.dcerpc.msrpc.samr;

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
namespace jcifs.smb {




	/// <summary>
	/// Internal use only: SID resolver cache
	/// 
	/// @author mbechler
	/// @internal
	/// </summary>
	public class SIDCacheImpl : SidResolver {

		private IDictionary<jcifs.smb.SID, jcifs.smb.SID> sidCache = new Dictionary<SID, SID>();


		/// <param name="baseContext"> </param>
		public SIDCacheImpl(CIFSContext baseContext) {
		}


		/// throws java.io.IOException
		internal virtual void resolveSids(DcerpcHandle handle, LsaPolicyHandle policyHandle, SID[] sids) {
			MsrpcLookupSids rpc = new MsrpcLookupSids(policyHandle, sids);
			handle.sendrecv(rpc);
			switch (rpc.retval) {
			case 0:
			case NtStatus.NT_STATUS_NONE_MAPPED:
			case 0x00000107: // NT_STATUS_SOME_NOT_MAPPED
				break;
			default:
				throw new SmbException(rpc.retval, false);
			}

			for (int si = 0; si < sids.Length; si++) {
				jcifs.smb.SID @out = sids[si].unwrap<SID>(typeof(SID));
				lsarpc.LsarTranslatedName resp = rpc.names.names[si];
				@out.domainName = null;
				switch (resp.sid_type) {
				case SIDConstants.SID_TYPE_USER:
				case SIDConstants.SID_TYPE_DOM_GRP:
				case SIDConstants.SID_TYPE_DOMAIN:
				case SIDConstants.SID_TYPE_ALIAS:
				case SIDConstants.SID_TYPE_WKN_GRP:
					rpc.unicode_string ustr = rpc.domains.domains[resp.sid_index].name;
					@out.domainName = (new UnicodeString(ustr, false)).ToString();
					break;
				}

				UnicodeString ucstr = new UnicodeString(resp.name, false);
				@out.acctName = ucstr.ToString();
				@out.type = resp.sid_type;
				@out.origin_server = null;
				@out.origin_ctx = null;
			}
		}


		/// throws jcifs.CIFSException
		internal virtual void resolveSids0(string authorityServerName, CIFSContext tc, SID[] sids) {
			lock (this.sidCache) {
				try {
						using (DcerpcHandle handle = DcerpcHandle.getHandle("ncacn_np:" + authorityServerName + "[\\PIPE\\lsarpc]", tc)) {
						string server = authorityServerName;
						int dot = server.IndexOf('.');
						if (dot > 0 && char.IsDigit(server[0]) == false) {
							server = server.Substring(0, dot);
						}
						using (LsaPolicyHandle policyHandle = new LsaPolicyHandle(handle, "\\\\" + server, 0x00000800)) {
							resolveSids(handle, policyHandle, sids);
						}
						}
				}
				catch (IOException e) {
					throw new CIFSException("Failed to resolve SIDs", e);
				}
			}
		}


		/// throws jcifs.CIFSException
		public virtual void resolveSids(CIFSContext tc, string authorityServerName, jcifs.SID[] sids, int offset, int length) {
			List<SID> list = new List<SID>(sids.Length);
			int si;

			lock (this.sidCache) {
				for (si = 0; si < length; si++) {
					SID s = sids[offset + si].unwrap<SID>(typeof(SID));
					SID sid = this.sidCache.get(s);
					if (sid != null) {
						s.type = sid.type;
						s.domainName = sid.domainName;
						s.acctName = sid.acctName;
					}
					else {
						list.Add(s);
					}
				}

				if (list.Count > 0) {
					SID[] resolved = list.ToArray();
					resolveSids0(authorityServerName, tc, resolved);
					for (si = 0; si < resolved.Length; si++) {
						this.sidCache[resolved[si]] = resolved[si];
					}
				}
			}
		}


		/// <summary>
		/// Resolve an array of SIDs using a cache and at most one MSRPC request.
		/// <para>
		/// This method will attempt
		/// to resolve SIDs using a cache and cache the results of any SIDs that
		/// required resolving with the authority. SID cache entries are currently not
		/// expired because under normal circumstances SID information never changes.
		/// 
		/// </para>
		/// </summary>
		/// <param name="authorityServerName">
		///            The hostname of the server that should be queried. For maximum efficiency this should be the hostname
		///            of a domain controller however a member server will work as well and a domain controller may not
		///            return names for SIDs corresponding to local accounts for which the domain controller is not an
		///            authority. </param>
		/// <param name="tc">
		///            The context that should be used to communicate with the named server. </param>
		/// <param name="sids">
		///            The SIDs that should be resolved. After this function is called, the names associated with the SIDs
		///            may be queried with the <tt>toDisplayString</tt>, <tt>getDomainName</tt>, and <tt>getAccountName</tt>
		///            methods. </param>
		/// throws jcifs.CIFSException
		public virtual void resolveSids(CIFSContext tc, string authorityServerName, jcifs.SID[] sids) {
			List<SID> list = new List<SID>(sids.Length);
			int si;

			lock (this.sidCache) {
				for (si = 0; si < sids.Length; si++) {
					SID s = sids[si].unwrap<SID>(typeof(SID));
					SID sid = this.sidCache.get(s);
					if (sid != null) {
						s.type = sid.type;
						s.domainName = sid.domainName;
						s.acctName = sid.acctName;
					}
					else {
						list.Add(s);
					}
				}

				if (list.Count > 0) {
					SID[] resolved = list.ToArray();
					resolveSids0(authorityServerName, tc, resolved);
					for (si = 0; si < resolved.Length; si++) {
						this.sidCache[resolved[si]] = resolved[si];
					}
				}
			}
		}


		/// throws jcifs.CIFSException
		public virtual jcifs.SID getServerSid(CIFSContext tc, string server) {
			lsarpc.LsarDomainInfo info = new lsarpc.LsarDomainInfo();
			MsrpcQueryInformationPolicy rpc;

			lock (this.sidCache) {
				try {
						using (DcerpcHandle handle = DcerpcHandle.getHandle("ncacn_np:" + server + "[\\PIPE\\lsarpc]", tc)) {
						// NetApp doesn't like the 'generic' access mask values
						using (LsaPolicyHandle policyHandle = new LsaPolicyHandle(handle, null, 0x00000001)) {
							rpc = new MsrpcQueryInformationPolicy(policyHandle, (short) lsarpc.POLICY_INFO_ACCOUNT_DOMAIN, info);
							handle.sendrecv(rpc);
							if (rpc.retval != 0) {
								throw new SmbException(rpc.retval, false);
							}
						}
        
						return new SID(info.sid, SIDConstants.SID_TYPE_DOMAIN, (new UnicodeString(info.name, false)).ToString(), null, false);
						}
				}
				catch (IOException e) {
					throw new CIFSException("Failed to get SID from server", e);
				}
			}
		}


		/// throws jcifs.CIFSException
		public virtual jcifs.SID[] getGroupMemberSids(CIFSContext tc, string authorityServerName, jcifs.SID domsid, int rid, int flags) {
			lsarpc.LsarSidArray sidarray = new lsarpc.LsarSidArray();
			MsrpcGetMembersInAlias rpc = null;

			lock (this.sidCache) {
				try {
						using (DcerpcHandle handle = DcerpcHandle.getHandle("ncacn_np:" + authorityServerName + "[\\PIPE\\samr]", tc)) {
						SamrPolicyHandle policyHandle = new SamrPolicyHandle(handle, authorityServerName, 0x00000030);
						SamrDomainHandle domainHandle = new SamrDomainHandle(handle, policyHandle, 0x00000200, domsid.unwrap<rpc.sid_t>(typeof(rpc.sid_t)));
						using (SamrAliasHandle aliasHandle = new SamrAliasHandle(handle, domainHandle, 0x0002000c, rid)) {
							rpc = new MsrpcGetMembersInAlias(aliasHandle, sidarray);
							handle.sendrecv(rpc);
							if (rpc.retval != 0) {
								throw new SmbException(rpc.retval, false);
							}
							SID[] sids = new SID[rpc.sids.num_sids];
        
							string origin_server = handle.getServer();
							CIFSContext origin_ctx = handle.getTransportContext();
        
							for (int i = 0; i < sids.Length; i++) {
								sids[i] = new SID(rpc.sids.sids[i].sid, 0, null, null, false);
								sids[i].origin_server = origin_server;
								sids[i].origin_ctx = origin_ctx;
							}
							if (sids.Length > 0 && (flags & SID.SID_FLAG_RESOLVE_SIDS) != 0) {
								resolveSids(origin_ctx, origin_server, sids);
							}
							return sids;
						}
						}
				}
				catch (IOException e) {
					throw new CIFSException("Failed to get group member SIDs", e);
				}
			}

		}


		/// 
		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.SidResolver#getLocalGroupsMap(jcifs.CIFSContext, java.lang.String, int) </seealso>
		/// throws jcifs.CIFSException
		public virtual IDictionary<jcifs.SID, IList<jcifs.SID>> getLocalGroupsMap(CIFSContext tc, string authorityServerName, int flags) {
			SID domSid =(SID) getServerSid(tc, authorityServerName);
			lock (this.sidCache) {
				try {
						using (DcerpcHandle handle = DcerpcHandle.getHandle("ncacn_np:" + authorityServerName + "[\\PIPE\\samr]", tc)) {
						samr.SamrSamArray sam = new samr.SamrSamArray();
						using (SamrPolicyHandle policyHandle = new SamrPolicyHandle(handle, authorityServerName, 0x02000000))
						using (SamrDomainHandle domainHandle = new SamrDomainHandle(handle, policyHandle, 0x02000000, domSid)) {
							MsrpcEnumerateAliasesInDomain rpc = new MsrpcEnumerateAliasesInDomain(domainHandle, 0xFFFF, sam);
							handle.sendrecv(rpc);
							if (rpc.retval != 0) {
								throw new SmbException(rpc.retval, false);
							}
        
							IDictionary<jcifs.SID, IList<jcifs.SID>> map = new Dictionary<jcifs.SID, IList<jcifs.SID>>();
        
							for (int ei = 0; ei < rpc.sam.count; ei++) {
								samr.SamrSamEntry entry = rpc.sam.entries[ei];
        
								jcifs.SID[] mems = getGroupMemberSids(tc, authorityServerName, domSid, entry.idx, flags);
								SID groupSid = new SID(domSid, entry.idx);
								groupSid.type = SIDConstants.SID_TYPE_ALIAS;
								groupSid.domainName = domSid.getDomainName();
								groupSid.acctName = (new UnicodeString(entry.name, false)).ToString();
        
								for (int mi = 0; mi < mems.Length; mi++) {
									IList<jcifs.SID> groups = map.get(mems[mi]);
									if (groups == null) {
										groups = new List<jcifs.SID>();
										map[mems[mi]] = groups;
									}
									if (!groups.Contains(groupSid)) {
										groups.Add(groupSid);
									}
								}
							}
        
							return map;
						}
						}
				}
				catch (IOException e) {
					throw new CIFSException("Failed to resolve groups", e);
				}
			}
		}
	}

}