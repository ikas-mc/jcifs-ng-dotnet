using System;
using CIFSContext = jcifs.CIFSContext;
using CIFSException = jcifs.CIFSException;
using SmbTree = jcifs.SmbTree;
using CommonServerMessageBlockResponse = jcifs.@internal.CommonServerMessageBlockResponse;
using jcifs.@internal;

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
namespace jcifs.smb {



	/// <summary>
	/// @author mbechler
	/// @internal
	/// </summary>
	public interface SmbTreeInternal : SmbTree {

		/// <param name="tf"> </param>
		/// <exception cref="SmbException"> </exception>
		/// throws SmbException;
		[Obsolete]
		void connectLogon(CIFSContext tf);


		/// <param name="request"> </param>
		/// <param name="params"> </param>
		/// <returns> response message </returns>
		/// <exception cref="CIFSException"> </exception>
		/// throws jcifs.CIFSException;
		T send<T>(Request<T> request, params RequestParam[] @params) where T : CommonServerMessageBlockResponse ;
	}

}