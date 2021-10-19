using System;
using CIFSContext = jcifs.CIFSContext;
using SmbPipeHandle = jcifs.SmbPipeHandle;
using SmbPipeResource = jcifs.SmbPipeResource;
using SmbComNTCreateAndX = jcifs.@internal.smb1.com.SmbComNTCreateAndX;
using SmbComNTCreateAndXResponse = jcifs.@internal.smb1.com.SmbComNTCreateAndXResponse;

/* jcifs smb client library in Java
 * Copyright (C) 2000  "Michael B. Allen" <jcifs at samba dot org>
 *                     "Paul Walker" <jcifs at samba dot org>
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
	/// This class will allow a Java program to read and write data to Named
	/// Pipes and Transact NamedPipes.
	/// 
	/// <para>
	/// There are three Win32 function calls provided by the Windows SDK
	/// that are important in the context of using jCIFS. They are:
	/// 
	/// <ul>
	/// <li><code>CallNamedPipe</code> A message-type pipe call that opens,
	/// writes to, reads from, and closes the pipe in a single operation.
	/// <li><code>TransactNamedPipe</code> A message-type pipe call that
	/// writes to and reads from an existing pipe descriptor in one operation.
	/// <li><code>CreateFile</code>, <code>ReadFile</code>,
	/// <code>WriteFile</code>, and <code>CloseFile</code> A byte-type pipe can
	/// be opened, written to, read from and closed using the standard Win32
	/// file operations.
	/// </ul>
	/// 
	/// </para>
	/// <para>
	/// The jCIFS API maps all of these operations into the standard Java
	/// <code>XxxputStream</code> interface. A special <code>PIPE_TYPE</code>
	/// flags is necessary to distinguish which type of Named Pipe behavior
	/// is desired.
	/// 
	/// </para>
	/// <para>
	/// <table border="1" cellpadding="3" cellspacing="0" width="100%" summary="Usage examples">
	/// <tr bgcolor="#ccccff">
	/// <td colspan="2"><b><code>SmbNamedPipe</code> Constructor Examples</b></td>
	/// <tr>
	/// <td width="20%"><b>Code Sample</b></td>
	/// <td><b>Description</b></td>
	/// </tr>
	/// <tr>
	/// <td width="20%">
	/// 
	/// <pre>
	/// new SmbNamedPipe("smb://server/IPC$/PIPE/foo", SmbNamedPipe.PIPE_TYPE_RDWR | SmbNamedPipe.PIPE_TYPE_CALL, context);
	/// </pre>
	/// 
	/// </td>
	/// <td>
	/// Open the Named Pipe foo for reading and writing. The pipe will behave like the <code>CallNamedPipe</code> interface.
	/// </td>
	/// </tr>
	/// <tr>
	/// <td width="20%">
	/// 
	/// <pre>
	/// new SmbNamedPipe("smb://server/IPC$/foo", SmbNamedPipe.PIPE_TYPE_RDWR | SmbNamedPipe.PIPE_TYPE_TRANSACT, context);
	/// </pre>
	/// 
	/// </td>
	/// <td>
	/// Open the Named Pipe foo for reading and writing. The pipe will behave like the <code>TransactNamedPipe</code>
	/// interface.
	/// </td>
	/// </tr>
	/// <tr>
	/// <td width="20%">
	/// 
	/// <pre>
	/// new SmbNamedPipe("smb://server/IPC$/foo", SmbNamedPipe.PIPE_TYPE_RDWR, context);
	/// </pre>
	/// 
	/// </td>
	/// <td>
	/// Open the Named Pipe foo for reading and writing. The pipe will
	/// behave as though the <code>CreateFile</code>, <code>ReadFile</code>,
	/// <code>WriteFile</code>, and <code>CloseFile</code> interface was
	/// being used.
	/// </td>
	/// </tr>
	/// </table>
	/// 
	/// </para>
	/// <para>
	/// See <a href="../../../pipes.html">Using jCIFS to Connect to Win32
	/// Named Pipes</a> for a detailed description of how to use jCIFS with
	/// Win32 Named Pipe server processes.
	/// 
	/// </para>
	/// </summary>

	public class SmbNamedPipe : SmbFile, SmbPipeResource {

		private readonly int pipeType;


		/// <summary>
		/// Open the Named Pipe resource specified by the url
		/// parameter. The pipeType parameter should be at least one of
		/// the <code>PIPE_TYPE</code> flags combined with the bitwise OR
		/// operator <code>|</code>. See the examples listed above.
		/// </summary>
		/// <param name="url"> </param>
		/// <param name="pipeType"> </param>
		/// <param name="unshared">
		///            whether to use an exclusive connection for this pipe </param>
		/// <param name="tc"> </param>
		/// <exception cref="MalformedURLException"> </exception>

		/// throws java.net.MalformedURLException
		public SmbNamedPipe(string url, int pipeType, bool unshared, CIFSContext tc) : base(url, tc) {
			this.pipeType = pipeType;
			setNonPooled(unshared);
			if (!getLocator().isIPC()) {
				throw new UriFormatException("Named pipes are only valid on IPC$");
			}
			this.fileLocator.updateType(SmbConstants.TYPE_NAMED_PIPE);
		}


		/// <summary>
		/// Open the Named Pipe resource specified by the url
		/// parameter. The pipeType parameter should be at least one of
		/// the <code>PIPE_TYPE</code> flags combined with the bitwise OR
		/// operator <code>|</code>. See the examples listed above.
		/// </summary>
		/// <param name="url"> </param>
		/// <param name="pipeType"> </param>
		/// <param name="tc"> </param>
		/// <exception cref="MalformedURLException"> </exception>
		/// throws java.net.MalformedURLException
		public SmbNamedPipe(string url, int pipeType, CIFSContext tc) : this(url, pipeType, (pipeType & SmbPipeResourceConstants.PIPE_TYPE_UNSHARED) != 0, tc) {
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.smb.SmbFile#customizeCreate(jcifs.internal.smb1.com.SmbComNTCreateAndX,
		///      jcifs.internal.smb1.com.SmbComNTCreateAndXResponse) </seealso>
		protected internal override void customizeCreate(SmbComNTCreateAndX request, SmbComNTCreateAndXResponse response) {
			request.addFlags0(0x16);
			response.setExtended(true);
		}


		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		/// <seealso cref= jcifs.smb.SmbFile#getType() </seealso>
		/// throws SmbException
		public override int getType() {
			return SmbConstants.TYPE_NAMED_PIPE;
		}


		/// <returns> the pipe type </returns>
		public virtual int getPipeType() {
			return this.pipeType;
		}


		/// <returns> a handle for interacting with the pipe </returns>
		public virtual SmbPipeHandle openPipe() {
			return new SmbPipeHandleImpl(this);
		}

	}

}