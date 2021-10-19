/*
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

using System;

namespace jcifs {

	/// <summary>
	/// Utility class holding several protocol constrants
	/// 
	/// @author mbechler
	/// 
	/// @internal
	/// </summary>
	public static class SmbConstants {

		public static int DEFAULT_PORT = 445;

		public static int DEFAULT_MAX_MPX_COUNT = 10;
		public static int DEFAULT_RESPONSE_TIMEOUT = 30000;
		public static int DEFAULT_SO_TIMEOUT = 35000;
		public static int DEFAULT_RCV_BUF_SIZE = 0xFFFF;
		public static int DEFAULT_SND_BUF_SIZE = 0xFFFF;
		public static int DEFAULT_NOTIFY_BUF_SIZE = 1024;

		public static int DEFAULT_SSN_LIMIT = 250;
		public static int DEFAULT_CONN_TIMEOUT = 35000;

		public static int FLAGS_NONE = 0x00;
		public static int FLAGS_LOCK_AND_READ_WRITE_AND_UNLOCK = 0x01;
		public static int FLAGS_RECEIVE_BUFFER_POSTED = 0x02;
		public static int FLAGS_PATH_NAMES_CASELESS = 0x08;
		public static int FLAGS_PATH_NAMES_CANONICALIZED = 0x10;
		public static int FLAGS_OPLOCK_REQUESTED_OR_GRANTED = 0x20;
		public static int FLAGS_NOTIFY_OF_MODIFY_ACTION = 0x40;
		public static int FLAGS_RESPONSE = 0x80;

		public static int FLAGS2_NONE = 0x0000;
		public static int FLAGS2_LONG_FILENAMES = 0x0001;
		public static int FLAGS2_EXTENDED_ATTRIBUTES = 0x0002;
		public static int FLAGS2_SECURITY_SIGNATURES = 0x0004;
		public static int FLAGS2_SECURITY_REQUIRE_SIGNATURES = 0x0010;
		public static int FLAGS2_EXTENDED_SECURITY_NEGOTIATION = 0x0800;
		public static int FLAGS2_RESOLVE_PATHS_IN_DFS = 0x1000;
		public static int FLAGS2_PERMIT_READ_IF_EXECUTE_PERM = 0x2000;
		public static int FLAGS2_STATUS32 = 0x4000;
		public static int FLAGS2_UNICODE = 0x8000;

		public static int CAP_NONE = 0x0000;
		public static int CAP_RAW_MODE = 0x0001;
		public static int CAP_MPX_MODE = 0x0002;
		public static int CAP_UNICODE = 0x0004;
		public static int CAP_LARGE_FILES = 0x0008;
		public static int CAP_NT_SMBS = 0x0010;
		public static int CAP_RPC_REMOTE_APIS = 0x0020;
		public static int CAP_STATUS32 = 0x0040;
		public static int CAP_LEVEL_II_OPLOCKS = 0x0080;
		public static int CAP_LOCK_AND_READ = 0x0100;
		public static int CAP_NT_FIND = 0x0200;
		public static int CAP_DFS = 0x1000;
		public static int CAP_LARGE_READX = 0x4000;
		public static int CAP_LARGE_WRITEX = 0x8000;
		public static int CAP_EXTENDED_SECURITY = unchecked((int)0x80000000);

		// file attribute encoding
		/// <summary>
		/// File is marked read-only
		/// </summary>
		public static int ATTR_READONLY = 0x01;
		/// <summary>
		/// File is marked hidden
		/// </summary>
		public static int ATTR_HIDDEN = 0x02;
		/// <summary>
		/// File is marked a system file
		/// </summary>
		public static int ATTR_SYSTEM = 0x04;
		/// <summary>
		/// File is marked a volume
		/// </summary>
		public static int ATTR_VOLUME = 0x08;
		/// <summary>
		/// File is a directory
		/// </summary>
		public static int ATTR_DIRECTORY = 0x10;

		/// <summary>
		/// Files is marked to be archived
		/// </summary>
		public static int ATTR_ARCHIVE = 0x20;

		// extended file attribute encoding(others same as above)
		public static int ATTR_COMPRESSED = 0x800;
		public static int ATTR_NORMAL = 0x080;
		public static int ATTR_TEMPORARY = 0x100;

		// access mask encoding
		public static int FILE_READ_DATA = 0x00000001; // 1
		public static int FILE_WRITE_DATA = 0x00000002; // 2
		public static int FILE_APPEND_DATA = 0x00000004; // 3
		public static int FILE_READ_EA = 0x00000008; // 4
		public static int FILE_WRITE_EA = 0x00000010; // 5
		public static int FILE_EXECUTE = 0x00000020; // 6
		public static int FILE_DELETE = 0x00000040; // 7
		public static int FILE_READ_ATTRIBUTES = 0x00000080; // 8
		public static int FILE_WRITE_ATTRIBUTES = 0x00000100; // 9
		public static int DELETE = 0x00010000; // 16
		public static int READ_CONTROL = 0x00020000; // 17
		public static int WRITE_DAC = 0x00040000; // 18
		public static int WRITE_OWNER = 0x00080000; // 19
		public static int SYNCHRONIZE = 0x00100000; // 20
		public static int GENERIC_ALL = 0x10000000; // 28
		public static int GENERIC_EXECUTE = 0x20000000; // 29
		public static int GENERIC_WRITE = 0x40000000; // 30
		public static int GENERIC_READ = unchecked((int)0x80000000); // 31

		// flags for move and copy
		public static int FLAGS_TARGET_MUST_BE_FILE = 0x0001;
		public static int FLAGS_TARGET_MUST_BE_DIRECTORY = 0x0002;
		public static int FLAGS_COPY_TARGET_MODE_ASCII = 0x0004;
		public static int FLAGS_COPY_SOURCE_MODE_ASCII = 0x0008;
		public static int FLAGS_VERIFY_ALL_WRITES = 0x0010;
		public static int FLAGS_TREE_COPY = 0x0020;

		// open function
		public static int OPEN_FUNCTION_FAIL_IF_EXISTS = 0x0000;
		public static int OPEN_FUNCTION_OVERWRITE_IF_EXISTS = 0x0020;

		public static int SECURITY_SHARE = 0x00;
		public static int SECURITY_USER = 0x01;

		public static int CMD_OFFSET = 4;
		public static int ERROR_CODE_OFFSET = 5;
		public static int FLAGS_OFFSET = 9;
		public static int SIGNATURE_OFFSET = 14;
		public static int TID_OFFSET = 24;
		public static int SMB1_HEADER_LENGTH = 32;

		public static long MILLISECONDS_BETWEEN_1970_AND_1601 = 11644473600000L;

		public static string DEFAULT_OEM_ENCODING = "UTF-8"; //TODO 0 !!!

		public static int FOREVER = -1;

		/// <summary>
		/// When specified as the <tt>shareAccess</tt> constructor parameter,
		/// other SMB clients (including other threads making calls into jCIFS)
		/// will not be permitted to access the target file and will receive "The
		/// file is being accessed by another process" message.
		/// </summary>
		public static int FILE_NO_SHARE = 0x00;
		/// <summary>
		/// When specified as the <tt>shareAccess</tt> constructor parameter,
		/// other SMB clients will be permitted to read from the target file while
		/// this file is open. This constant may be logically OR'd with other share
		/// access flags.
		/// </summary>
		public static int FILE_SHARE_READ = 0x01;
		/// <summary>
		/// When specified as the <tt>shareAccess</tt> constructor parameter,
		/// other SMB clients will be permitted to write to the target file while
		/// this file is open. This constant may be logically OR'd with other share
		/// access flags.
		/// </summary>
		public static int FILE_SHARE_WRITE = 0x02;
		/// <summary>
		/// When specified as the <tt>shareAccess</tt> constructor parameter,
		/// other SMB clients will be permitted to delete the target file while
		/// this file is open. This constant may be logically OR'd with other share
		/// access flags.
		/// </summary>
		public static int FILE_SHARE_DELETE = 0x04;
		/// <summary>
		/// Default sharing mode for files
		/// </summary>
		public static int DEFAULT_SHARING = FILE_SHARE_READ | FILE_SHARE_WRITE | FILE_SHARE_DELETE;

		/// <summary>
		/// Returned by <seealso cref="jcifs.SmbResource.getType()"/> if the resource this <tt>SmbFile</tt>
		/// represents is a regular file or directory.
		/// </summary>
		public static int TYPE_FILESYSTEM = 0x01;
		/// <summary>
		/// Returned by <seealso cref="jcifs.SmbResource.getType()"/> if the resource this <tt>SmbFile</tt>
		/// represents is a workgroup.
		/// </summary>
		public static int TYPE_WORKGROUP = 0x02;
		/// <summary>
		/// Returned by <seealso cref="jcifs.SmbResource.getType()"/> if the resource this <tt>SmbFile</tt>
		/// represents is a server.
		/// </summary>
		public static int TYPE_SERVER = 0x04;
		/// <summary>
		/// Returned by <seealso cref="jcifs.SmbResource.getType()"/> if the resource this <tt>SmbFile</tt>
		/// represents is a share.
		/// </summary>
		public static int TYPE_SHARE = 0x08;
		/// <summary>
		/// Returned by <seealso cref="jcifs.SmbResource.getType()"/> if the resource this <tt>SmbFile</tt>
		/// represents is a named pipe.
		/// </summary>
		public static int TYPE_NAMED_PIPE = 0x10;
		/// <summary>
		/// Returned by <seealso cref="jcifs.SmbResource.getType()"/> if the resource this <tt>SmbFile</tt>
		/// represents is a printer.
		/// </summary>
		public static int TYPE_PRINTER = 0x20;
		/// <summary>
		/// Returned by <seealso cref="jcifs.SmbResource.getType()"/> if the resource this <tt>SmbFile</tt>
		/// represents is a communications device.
		/// </summary>
		public static int TYPE_COMM = 0x40;

		/* open flags */

		public static int O_RDONLY = 0x01;
		public static int O_WRONLY = 0x02;
		public static int O_RDWR = 0x03;
		public static int O_APPEND = 0x04;

		// Open Function Encoding
		// create if the file does not exist
		public static int O_CREAT = 0x0010;
		// fail if the file exists
		public static int O_EXCL = 0x0020;
		// truncate if the file exists
		public static int O_TRUNC = 0x0040;

	}

}