using CIFSException = jcifs.CIFSException;

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
	/// 
	/// </summary>
	public class SmbPipeOutputStream : SmbFileOutputStream {

		private SmbPipeHandleImpl handle;


		/// <param name="handle"> </param>
		/// <exception cref="SmbException"> </exception>
		/// throws jcifs.CIFSException
		internal SmbPipeOutputStream(SmbPipeHandleImpl handle, SmbTreeHandleImpl th) : base((SmbFile)handle.getPipe(), th, null, 0, 0, 0) {
			this.handle = handle;
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.smb.SmbFileOutputStream#isOpen() </seealso>
		public override bool isOpen() {
			return this.handle.isOpen();
		}


		/// throws jcifs.CIFSException
		 internal override SmbTreeHandleImpl ensureTreeConnected() {
			lock (this) {
				return (SmbTreeHandleImpl)this.handle.ensureTreeConnected();
			}
		}


		/// throws jcifs.CIFSException
		 internal override SmbFileHandleImpl ensureOpen() {
			lock (this) {
				return (SmbFileHandleImpl)this.handle.ensureOpen();
			}
		}


		/// <returns> the handle </returns>
		 internal virtual SmbPipeHandleImpl getHandle() {
			return this.handle;
		}

	}

}