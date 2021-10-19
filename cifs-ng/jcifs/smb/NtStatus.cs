/* jcifs smb client library in Java
 * Copyright (C) 2004  "Michael B. Allen" <jcifs at samba dot org>
 *
 * This library is free software); you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation); either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY); without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library); if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */

namespace jcifs.smb {

	public static class NtStatus {

		/*
		 * Don't bother to edit this. Everything within the interface
		 * block is automatically generated from the ntstatus package.
		 */

		public const int NT_STATUS_OK = 0x00000000;
		public const int NT_STATUS_PENDING = 0x00000103;
		public const int NT_STATUS_NOTIFY_ENUM_DIR = 0x0000010C;
		public const int NT_STATUS_BUFFER_OVERFLOW = unchecked((int)0x80000005);
		public const int NT_STATUS_UNSUCCESSFUL = unchecked((int)0xC0000001);
		public const int NT_STATUS_NOT_IMPLEMENTED = unchecked((int)0xC0000002);
		public const int NT_STATUS_INVALID_INFO_CLASS = unchecked((int)0xC0000003);
		public const int NT_STATUS_ACCESS_VIOLATION = unchecked((int)0xC0000005);
		public const int NT_STATUS_INVALID_HANDLE = unchecked((int)0xC0000008);
		public const int NT_STATUS_INVALID_PARAMETER = unchecked((int)0xC000000d);
		public const int NT_STATUS_NO_SUCH_DEVICE = unchecked((int)0xC000000e);
		public const int NT_STATUS_NO_SUCH_FILE = unchecked((int)0xC000000f);
		public const int NT_STATUS_END_OF_FILE = unchecked((int)0xC0000011);
		public const int NT_STATUS_MORE_PROCESSING_REQUIRED = unchecked((int)0xC0000016);
		public const int NT_STATUS_ACCESS_DENIED = unchecked((int)0xC0000022);
		public const int NT_STATUS_BUFFER_TOO_SMALL = unchecked((int)0xC0000023);
		public const int NT_STATUS_OBJECT_NAME_INVALID = unchecked((int)0xC0000033);
		public const int NT_STATUS_OBJECT_NAME_NOT_FOUND = unchecked((int)0xC0000034);
		public const int NT_STATUS_OBJECT_NAME_COLLISION = unchecked((int)0xC0000035);
		public const int NT_STATUS_PORT_DISCONNECTED = unchecked((int)0xC0000037);
		public const int NT_STATUS_OBJECT_PATH_INVALID = unchecked((int)0xC0000039);
		public const int NT_STATUS_OBJECT_PATH_NOT_FOUND = unchecked((int)0xC000003a);
		public const int NT_STATUS_OBJECT_PATH_SYNTAX_BAD = unchecked((int)0xC000003b);
		public const int NT_STATUS_SHARING_VIOLATION = unchecked((int)0xC0000043);
		public const int NT_STATUS_DELETE_PENDING = unchecked((int)0xC0000056);
		public const int NT_STATUS_NO_LOGON_SERVERS = unchecked((int)0xC000005e);
		public const int NT_STATUS_USER_EXISTS = unchecked((int)0xC0000063);
		public const int NT_STATUS_NO_SUCH_USER = unchecked((int)0xC0000064);
		public const int NT_STATUS_WRONG_PASSWORD = unchecked((int)0xC000006a);
		public const int NT_STATUS_LOGON_FAILURE = unchecked((int)0xC000006d);
		public const int NT_STATUS_ACCOUNT_RESTRICTION = unchecked((int)0xC000006e);
		public const int NT_STATUS_INVALID_LOGON_HOURS = unchecked((int)0xC000006f);
		public const int NT_STATUS_INVALID_WORKSTATION = unchecked((int)0xC0000070);
		public const int NT_STATUS_PASSWORD_EXPIRED = unchecked((int)0xC0000071);
		public const int NT_STATUS_ACCOUNT_DISABLED = unchecked((int)0xC0000072);
		public const int NT_STATUS_NONE_MAPPED = unchecked((int)0xC0000073);
		public const int NT_STATUS_INVALID_SID = unchecked((int)0xC0000078);
		public const int NT_STATUS_DISK_FULL = unchecked((int)0xC000007f);
		public const int NT_STATUS_INSTANCE_NOT_AVAILABLE = unchecked((int)0xC00000ab);
		public const int NT_STATUS_PIPE_NOT_AVAILABLE = unchecked((int)0xC00000ac);
		public const int NT_STATUS_INVALID_PIPE_STATE = unchecked((int)0xC00000ad);
		public const int NT_STATUS_PIPE_BUSY = unchecked((int)0xC00000ae);
		public const int NT_STATUS_PIPE_DISCONNECTED = unchecked((int)0xC00000b0);
		public const int NT_STATUS_PIPE_CLOSING = unchecked((int)0xC00000b1);
		public const int NT_STATUS_PIPE_LISTENING = unchecked((int)0xC00000b3);
		public const int NT_STATUS_FILE_IS_A_DIRECTORY = unchecked((int)0xC00000ba);
		public const int NT_STATUS_DUPLICATE_NAME = unchecked((int)0xC00000bd);
		public const int NT_STATUS_NETWORK_NAME_DELETED = unchecked((int)0xC00000c9);
		public const int NT_STATUS_NETWORK_ACCESS_DENIED = unchecked((int)0xC00000ca);
		public const int NT_STATUS_BAD_DEVICE_TYPE = unchecked((int)0xC00000cb);
		public const int NT_STATUS_BAD_NETWORK_NAME = unchecked((int)0xC00000cc);
		public const int NT_STATUS_REQUEST_NOT_ACCEPTED = unchecked((int)0xC00000d0);
		public const int NT_STATUS_CANT_ACCESS_DOMAIN_INFO = unchecked((int)0xC00000da);
		public const int NT_STATUS_NO_SUCH_DOMAIN = unchecked((int)0xC00000df);
		public const int NT_STATUS_NOT_A_DIRECTORY = unchecked((int)0xC0000103);
		public const int NT_STATUS_CANNOT_DELETE = unchecked((int)0xC0000121);
		public const int NT_STATUS_INVALID_COMPUTER_NAME = unchecked((int)0xC0000122);
		public const int NT_STATUS_PIPE_BROKEN = unchecked((int)0xC000014b);
		public const int NT_STATUS_NO_SUCH_ALIAS = unchecked((int)0xC0000151);
		public const int NT_STATUS_LOGON_TYPE_NOT_GRANTED = unchecked((int)0xC000015b);
		public const int NT_STATUS_NO_TRUST_SAM_ACCOUNT = unchecked((int)0xC000018b);
		public const int NT_STATUS_TRUSTED_DOMAIN_FAILURE = unchecked((int)0xC000018c);
		public const int NT_STATUS_TRUSTED_RELATIONSHIP_FAILURE = unchecked((int)0xC000018d);
		public const int NT_STATUS_NOLOGON_WORKSTATION_TRUST_ACCOUNT = unchecked((int)0xC0000199);
		public const int NT_STATUS_PASSWORD_MUST_CHANGE = unchecked((int)0xC0000224);
		public const int NT_STATUS_NOT_FOUND = unchecked((int)0xC0000225);
		public const int NT_STATUS_ACCOUNT_LOCKED_OUT = unchecked((int)0xC0000234);
		public const int NT_STATUS_CONNECTION_REFUSED = unchecked((int)0xC0000236);
		public const int NT_STATUS_PATH_NOT_COVERED = unchecked((int)0xC0000257);
		public const int NT_STATUS_IO_REPARSE_TAG_NOT_HANDLED = unchecked((int)0xC0000279);
		public const int NT_STATUS_NO_MORE_FILES = unchecked((int)0x80000006);

		public static int[] NT_STATUS_CODES = new int[] {NT_STATUS_OK, NT_STATUS_PENDING, NT_STATUS_NOTIFY_ENUM_DIR, NT_STATUS_BUFFER_OVERFLOW, NT_STATUS_UNSUCCESSFUL, NT_STATUS_NOT_IMPLEMENTED, NT_STATUS_INVALID_INFO_CLASS, NT_STATUS_ACCESS_VIOLATION, NT_STATUS_INVALID_HANDLE, NT_STATUS_INVALID_PARAMETER, NT_STATUS_NO_SUCH_DEVICE, NT_STATUS_NO_SUCH_FILE, NT_STATUS_END_OF_FILE, NT_STATUS_MORE_PROCESSING_REQUIRED, NT_STATUS_ACCESS_DENIED, NT_STATUS_BUFFER_TOO_SMALL, NT_STATUS_OBJECT_NAME_INVALID, NT_STATUS_OBJECT_NAME_NOT_FOUND, NT_STATUS_OBJECT_NAME_COLLISION, NT_STATUS_PORT_DISCONNECTED, NT_STATUS_OBJECT_PATH_INVALID, NT_STATUS_OBJECT_PATH_NOT_FOUND, NT_STATUS_OBJECT_PATH_SYNTAX_BAD, NT_STATUS_SHARING_VIOLATION, NT_STATUS_DELETE_PENDING, NT_STATUS_NO_LOGON_SERVERS, NT_STATUS_USER_EXISTS, NT_STATUS_NO_SUCH_USER, NT_STATUS_WRONG_PASSWORD, NT_STATUS_LOGON_FAILURE, NT_STATUS_ACCOUNT_RESTRICTION, NT_STATUS_INVALID_LOGON_HOURS, NT_STATUS_INVALID_WORKSTATION, NT_STATUS_PASSWORD_EXPIRED, NT_STATUS_ACCOUNT_DISABLED, NT_STATUS_NONE_MAPPED, NT_STATUS_INVALID_SID, NT_STATUS_DISK_FULL, NT_STATUS_INSTANCE_NOT_AVAILABLE, NT_STATUS_PIPE_NOT_AVAILABLE, NT_STATUS_INVALID_PIPE_STATE, NT_STATUS_PIPE_BUSY, NT_STATUS_PIPE_DISCONNECTED, NT_STATUS_PIPE_CLOSING, NT_STATUS_PIPE_LISTENING, NT_STATUS_FILE_IS_A_DIRECTORY, NT_STATUS_DUPLICATE_NAME, NT_STATUS_NETWORK_NAME_DELETED, NT_STATUS_NETWORK_ACCESS_DENIED, NT_STATUS_BAD_DEVICE_TYPE, NT_STATUS_BAD_NETWORK_NAME, NT_STATUS_REQUEST_NOT_ACCEPTED, NT_STATUS_CANT_ACCESS_DOMAIN_INFO, NT_STATUS_NO_SUCH_DOMAIN, NT_STATUS_NOT_A_DIRECTORY, NT_STATUS_CANNOT_DELETE, NT_STATUS_INVALID_COMPUTER_NAME, NT_STATUS_PIPE_BROKEN, NT_STATUS_NO_SUCH_ALIAS, NT_STATUS_LOGON_TYPE_NOT_GRANTED, NT_STATUS_NO_TRUST_SAM_ACCOUNT, NT_STATUS_TRUSTED_DOMAIN_FAILURE, NT_STATUS_TRUSTED_RELATIONSHIP_FAILURE, NT_STATUS_NOLOGON_WORKSTATION_TRUST_ACCOUNT, NT_STATUS_PASSWORD_MUST_CHANGE, NT_STATUS_NOT_FOUND, NT_STATUS_ACCOUNT_LOCKED_OUT, NT_STATUS_CONNECTION_REFUSED, NT_STATUS_PATH_NOT_COVERED, NT_STATUS_IO_REPARSE_TAG_NOT_HANDLED, NT_STATUS_NO_MORE_FILES};

		public static string[] NT_STATUS_MESSAGES = new string[] {"The operation completed successfully.", "Request is pending", "A notify change request is being completed.", "The data was too large to fit into the specified buffer.", "A device attached to the system is not functioning.", "Incorrect function.", "The parameter is incorrect.", "Invalid access to memory location.", "The handle is invalid.", "The parameter is incorrect.", "The system cannot find the file specified.", "The system cannot find the file specified.", "End of file", "More data is available.", "Access is denied.", "The data area passed to a system call is too small.", "The filename, directory name, or volume label syntax is incorrect.", "The system cannot find the file specified.", "Cannot create a file when that file already exists.", "The handle is invalid.", "The specified path is invalid.", "The system cannot find the path specified.", "The specified path is invalid.", "The process cannot access the file because it is being used by another process.", "Access is denied.", "There are currently no logon servers available to service the logon request.", "The specified user already exists.", "The specified user does not exist.", "The specified network password is not correct.", "Logon failure: unknown user name or bad password.", "Logon failure: user account restriction.", "Logon failure: account logon time restriction violation.", "Logon failure: user not allowed to log on to this computer.", "Logon failure: the specified account password has expired.", "Logon failure: account currently disabled.", "No mapping between account names and security IDs was done.", "The security ID structure is invalid.", "The file system is full.", "All pipe instances are busy.", "All pipe instances are busy.", "The pipe state is invalid.", "All pipe instances are busy.", "No process is on the other end of the pipe.", "The pipe is being closed.", "Waiting for a process to open the other end of the pipe.", "File is a directory.", "A duplicate name exists on the network.", "The specified network name is no longer available.", "Network access is denied.", "Bad device type", "The network name cannot be found.", "No more connections can be made to this remote computer at this time because there are already as many connections as the computer can accept.", "Indicates a Windows NT Server could not be contacted or that objects within the domain are protected such that necessary information could not be retrieved.", "The specified domain did not exist.", "The directory name is invalid.", "Access is denied.", "The format of the specified computer name is invalid.", "The pipe has been ended.", "The specified local group does not exist.", "Logon failure: the user has not been granted the requested logon type at this computer.", "The SAM database on the Windows NT Server does not have a computer account for this workstation trust relationship.", "The logon request failed because the trust relationship between the primary domain and the trusted domain failed.", "The logon request failed because the trust relationship between this workstation and the primary domain failed.", "The account used is a Computer Account. Use your global user account or local user account to access this server.", "The user must change his password before he logs on the first time.", "The object was not found.", "The referenced account is currently locked out and may not be logged on to.", "Connection refused", "The remote system is not reachable by the transport.", "The layered file system driver for this I/O tag did not handle it when needed.", "No more files were found that match the file specification."};
	}

}