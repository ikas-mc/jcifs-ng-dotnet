using System;

/*
 * © 2017 AgNO3 Gmbh & Co. KG
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

	public static class SIDConstants
	{
		/// 
		public const int SID_TYPE_USE_NONE = 0;

		/// 
		public const int SID_TYPE_USER = 1;

		/// 
		public const int SID_TYPE_DOM_GRP = 2;

		/// 
		public const int SID_TYPE_DOMAIN = 3;

		/// 
		public const int SID_TYPE_ALIAS = 4;

		/// 
		public const int SID_TYPE_WKN_GRP = 5;

		/// 
		public const int SID_TYPE_DELETED = 6;

		/// 
		public static int SID_TYPE_INVALID = 7;

		/// 
		public static int SID_TYPE_UNKNOWN = 8;
	}

	/// <summary>
	/// A Windows SID is a numeric identifier used to represent Windows
	/// accounts. SIDs are commonly represented using a textual format such as
	/// <tt>S-1-5-21-1496946806-2192648263-3843101252-1029</tt> but they may
	/// also be resolved to yield the name of the associated Windows account
	/// such as <tt>Administrators</tt> or <tt>MYDOM\alice</tt>.
	/// <para>
	/// Consider the following output of <tt>examples/SidLookup.java</tt>:
	/// 
	/// <pre>
	///        toString: S-1-5-21-4133388617-793952518-2001621813-512
	/// toDisplayString: WNET\Domain Admins
	///         getType: 2
	///     getTypeText: Domain group
	///   getDomainName: WNET
	///  getAccountName: Domain Admins
	/// </pre>
	/// </para>
	/// </summary>
	public interface SID {

		/// 
		/// <returns> domain SID </returns>
		SID getDomainSid();


		/// <summary>
		/// Get the RID
		/// 
		/// This is the last subauthority identifier
		/// </summary>
		/// <returns> the RID </returns>
		int getRid();


		/// <summary>
		/// Return a String representing this SID ideal for display to
		/// users. This method should return the same text that the ACL
		/// editor in Windows would display.
		/// <para>
		/// Specifically, if the SID has
		/// been resolved and it is not a domain SID or builtin account,
		/// the full DOMAIN\name form of the account will be
		/// returned (e.g. MYDOM\alice or MYDOM\Domain Users).
		/// If the SID has been resolved but it is is a domain SID,
		/// only the domain name will be returned (e.g. MYDOM).
		/// If the SID has been resolved but it is a builtin account,
		/// only the name component will be returned (e.g. SYSTEM).
		/// If the sid cannot be resolved the numeric representation from
		/// toString() is returned.
		/// 
		/// </para>
		/// </summary>
		/// <returns> display format, potentially with resolved names </returns>
		string toDisplayString();


		/// <summary>
		/// Return the sAMAccountName of this SID unless it could not
		/// be resolved in which case the numeric RID is returned. If this
		/// SID is a domain SID, this method will return an empty String.
		/// </summary>
		/// <returns> the account name </returns>
		string getAccountName();


		/// <summary>
		/// Return the domain name of this SID unless it could not be
		/// resolved in which case the numeric representation is returned.
		/// </summary>
		/// <returns> the domain name </returns>
		string getDomainName();


		/// <summary>
		/// Return text representing the SID type suitable for display to
		/// users. Text includes 'User', 'Domain group', 'Local group', etc.
		/// </summary>
		/// <returns> textual representation of type </returns>
		string getTypeText();


		/// <summary>
		/// Returns the type of this SID indicating the state or type of account.
		/// <para>
		/// SID types are described in the following table.
		/// <table summary="Type codes">
		/// <tr>
		/// <th>Type</th>
		/// <th>Name</th>
		/// </tr>
		/// <tr>
		/// <td>SID_TYPE_USE_NONE</td>
		/// <td>0</td>
		/// </tr>
		/// <tr>
		/// <td>SID_TYPE_USER</td>
		/// <td>User</td>
		/// </tr>
		/// <tr>
		/// <td>SID_TYPE_DOM_GRP</td>
		/// <td>Domain group</td>
		/// </tr>
		/// <tr>
		/// <td>SID_TYPE_DOMAIN</td>
		/// <td>Domain</td>
		/// </tr>
		/// <tr>
		/// <td>SID_TYPE_ALIAS</td>
		/// <td>Local group</td>
		/// </tr>
		/// <tr>
		/// <td>SID_TYPE_WKN_GRP</td>
		/// <td>Builtin group</td>
		/// </tr>
		/// <tr>
		/// <td>SID_TYPE_DELETED</td>
		/// <td>Deleted</td>
		/// </tr>
		/// <tr>
		/// <td>SID_TYPE_INVALID</td>
		/// <td>Invalid</td>
		/// </tr>
		/// <tr>
		/// <td>SID_TYPE_UNKNOWN</td>
		/// <td>Unknown</td>
		/// </tr>
		/// </table>
		/// 
		/// </para>
		/// </summary>
		/// <returns> type code </returns>
		int getType();


		/// 
		/// <param name="type"> </param>
		/// <returns> unwrapped instance </returns>
		T unwrap<T>(Type type);

	}

}