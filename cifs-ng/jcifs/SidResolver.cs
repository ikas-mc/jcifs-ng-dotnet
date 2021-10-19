using System.Collections.Generic;

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



	/// <summary>
	/// This is an internal API for resolving SIDs to names and/or retrieving member SIDs
	/// 
	/// @author mbechler
	/// @internal
	/// </summary>
	public interface SidResolver {

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
		/// <param name="tc">
		///            context to use </param>
		/// <param name="authorityServerName">
		///            The hostname of the server that should be queried. For maximum efficiency this should be the hostname
		///            of a domain controller however a member server will work as well and a domain controller may not
		///            return names for SIDs corresponding to local accounts for which the domain controller is not an
		///            authority. </param>
		/// <param name="sids">
		///            The SIDs that should be resolved. After this function is called, the names associated with the SIDs
		///            may be queried with the <tt>toDisplayString</tt>, <tt>getDomainName</tt>, and <tt>getAccountName</tt>
		///            methods. </param>
		/// <exception cref="CIFSException"> </exception>
		/// throws CIFSException;
		void resolveSids(CIFSContext tc, string authorityServerName, SID[] sids);


		/// <summary>
		/// Resolve part of an array of SIDs using a cache and at most one MSRPC request.
		/// </summary>
		/// <param name="tc"> </param>
		/// <param name="authorityServerName"> </param>
		/// <param name="sids"> </param>
		/// <param name="off"> </param>
		/// <param name="len"> </param>
		/// <exception cref="CIFSException"> </exception>
		/// throws CIFSException;
		void resolveSids(CIFSContext tc, string authorityServerName, SID[] sids, int off, int len);


		/// <param name="tc"> </param>
		/// <param name="authorityServerName"> </param>
		/// <param name="domsid"> </param>
		/// <param name="rid"> </param>
		/// <param name="flags"> </param>
		/// <returns> the SIDs of the group members </returns>
		/// <exception cref="CIFSException"> </exception>
		/// throws CIFSException;
		SID[] getGroupMemberSids(CIFSContext tc, string authorityServerName, SID domsid, int rid, int flags);


		/// <param name="authorityServerName"> </param>
		/// <param name="tc"> </param>
		/// <returns> the server's SID </returns>
		/// <exception cref="CIFSException"> </exception>
		/// throws CIFSException;
		SID getServerSid(CIFSContext tc, string authorityServerName);


		/// <summary>
		/// This specialized method returns a Map of users and local groups for the
		/// target server where keys are SIDs representing an account and each value
		/// is an ArrayList of SIDs represents the local groups that the account is
		/// a member of.
		/// <p/>
		/// This method is designed to assist with computing access control for a
		/// given user when the target object's ACL has local groups. Local groups
		/// are not listed in a user's group membership (e.g. as represented by the
		/// tokenGroups constructed attribute retrieved via LDAP).
		/// <p/>
		/// Domain groups nested inside a local group are currently not expanded. In
		/// this case the key (SID) type will be SID_TYPE_DOM_GRP rather than
		/// SID_TYPE_USER.
		/// </summary>
		/// <param name="tc">
		///            The context to use </param>
		/// <param name="authorityServerName">
		///            The server from which the local groups will be queried. </param>
		/// <param name="flags">
		///            Flags that control the behavior of the operation. When all
		///            name associated with SIDs will be required, the SID_FLAG_RESOLVE_SIDS
		///            flag should be used which causes all group member SIDs to be resolved
		///            together in a single more efficient operation. </param>
		/// <returns> a map of group SID to member SIDs </returns>
		/// <exception cref="CIFSException"> </exception>
		/// throws CIFSException;
		IDictionary<SID, IList<SID>> getLocalGroupsMap(CIFSContext tc, string authorityServerName, int flags);

	}

}