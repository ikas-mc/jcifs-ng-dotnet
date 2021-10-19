using jcifs.@internal.smb2;

using System.Collections.Generic;
using cifs_ng.lib;
using Smb2Constants = jcifs.@internal.smb2.Smb2Constants;

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




	/// <summary>
	/// @author mbechler
	/// 
	/// </summary>
	public sealed class DialectVersion:IEnum {

		/// <summary>
		/// Legacy SMB1/CIFS
		/// </summary>
		public static readonly DialectVersion SMB1 = new DialectVersion("SMB1", InnerEnum.SMB1);

		/// <summary>
		/// SMB 2.02 - Windows Vista+
		/// </summary>
		public static readonly DialectVersion SMB202 = new DialectVersion("SMB202", InnerEnum.SMB202, Smb2Constants.SMB2_DIALECT_0202);

		/// <summary>
		/// SMB 2.1 - Windows 7/Server 2008R2
		/// </summary>
		public static readonly DialectVersion SMB210 = new DialectVersion("SMB210", InnerEnum.SMB210, Smb2Constants.SMB2_DIALECT_0210);

		/// <summary>
		/// SMB 3.0 - Windows 8/Server 2012
		/// </summary>
		public static readonly DialectVersion SMB300 = new DialectVersion("SMB300", InnerEnum.SMB300, Smb2Constants.SMB2_DIALECT_0300);

		/// <summary>
		/// SMB 3.0.2 - Windows 8.1/Server 2012R2
		/// </summary>
		public static readonly DialectVersion SMB302 = new DialectVersion("SMB302", InnerEnum.SMB302, Smb2Constants.SMB2_DIALECT_0302);

		/// <summary>
		/// SMB 3.1.1 - Windows 10/Server 2016
		/// </summary>
		public static readonly DialectVersion SMB311 = new DialectVersion("SMB311", InnerEnum.SMB311, Smb2Constants.SMB2_DIALECT_0311);

		private static readonly List<DialectVersion> valueList = new List<DialectVersion>();

		static DialectVersion() {
			valueList.Add(SMB1);
			valueList.Add(SMB202);
			valueList.Add(SMB210);
			valueList.Add(SMB300);
			valueList.Add(SMB302);
			valueList.Add(SMB311);
		}

		public enum InnerEnum {
			SMB1,
			SMB202,
			SMB210,
			SMB300,
			SMB302,
			SMB311
		}

		public readonly InnerEnum innerEnumValue;
		private readonly string nameValue;
		private readonly int ordinalValue;
		private static int nextOrdinal = 0;

		private readonly bool smb2;
		private readonly int dialect;


		/// 
		private DialectVersion(string name, InnerEnum innerEnum) {
			this.smb2 = false;
			this.dialect = -1;

			nameValue = name;
			ordinalValue = nextOrdinal++;
			innerEnumValue = innerEnum;
		}


		private DialectVersion(string name, InnerEnum innerEnum, int dialectId) {
			this.smb2 = true;
			this.dialect = dialectId;

			nameValue = name;
			ordinalValue = nextOrdinal++;
			innerEnumValue = innerEnum;
		}


		/// <returns> the smb2 </returns>
		public bool isSMB2() {
			return this.smb2;
		}


		/// <returns> the dialect </returns>
		public int getDialect() {
			if (!this.smb2) {
				throw new System.NotSupportedException();
			}
			return this.dialect;
		}


		/// 
		/// <param name="v"> </param>
		/// <returns> whether this version is a least the given one </returns>
		public bool atLeast(DialectVersion v) {
			return ordinal() >= v.ordinal();
		}


		/// 
		/// <param name="v"> </param>
		/// <returns> whether this version is a most the given one </returns>
		public bool atMost(DialectVersion v) {
			return ordinal() <= v.ordinal();
		}


		/// 
		/// <param name="a"> </param>
		/// <param name="b"> </param>
		/// <returns> smaller of the two versions </returns>
		public static DialectVersion min(DialectVersion a, DialectVersion b) {
			if (a.atMost(b)) {
				return a;
			}
			return b;
		}


		/// 
		/// <param name="a"> </param>
		/// <param name="b"> </param>
		/// <returns> larger of the two versions </returns>
		public static DialectVersion max(DialectVersion a, DialectVersion b) {
			if (a.atLeast(b)) {
				return a;
			}
			return b;
		}


		/// <param name="min">
		///            may be null for open end </param>
		/// <param name="max">
		///            may be null for open end </param>
		/// <returns> range of versions </returns>
		public static ISet<DialectVersion> range(DialectVersion min, DialectVersion max) {
			EnumSet<DialectVersion> vers = EnumSet<DialectVersion>.noneOf(6);
			foreach (DialectVersion ver in values()) {

				if (min != null && !ver.atLeast(min)) {
					continue;
				}

				if (max != null && !ver.atMost(max)) {
					continue;
				}

				vers.Add(ver);
			}
			return vers;
		}


		public static DialectVersion[] values() {
			return valueList.ToArray();
		}

		public int ordinal() {
			return ordinalValue;
		}

		public override string ToString() {
			return nameValue;
		}

		public static DialectVersion valueOf(string name) {
			foreach (DialectVersion enumInstance in DialectVersion.valueList) {
				if (enumInstance.nameValue == name) {
					return enumInstance;
				}
			}
			throw new System.ArgumentException(name);
		}
	}

}