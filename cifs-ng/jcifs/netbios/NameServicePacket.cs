using System;
using System.Net;
using Configuration = jcifs.Configuration;
using Hexdump = jcifs.util.Hexdump;

/* jcifs smb client library in Java
 * Copyright (C) 2000  "Michael B. Allen" <jcifs at samba dot org>
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

namespace jcifs.netbios {



	internal abstract class NameServicePacket {

		// opcode
		internal const int QUERY = 0;
		internal const int WACK = 7;

		// rcode
		internal const int FMT_ERR = 0x1;
		internal const int SRV_ERR = 0x2;
		internal const int IMP_ERR = 0x4;
		internal const int RFS_ERR = 0x5;
		internal const int ACT_ERR = 0x6;
		internal const int CFT_ERR = 0x7;

		// type/class
		internal const int NB_IN = 0x00200001;
		internal const int NBSTAT_IN = 0x00210001;
		internal const int NB = 0x0020;
		internal const int NBSTAT = 0x0021;
		internal const int IN = 0x0001;
		internal const int A = 0x0001;
		internal const int NS = 0x0002;
		internal const int NULL = 0x000a;

		internal const int HEADER_LENGTH = 12;

		// header field offsets
		internal const int OPCODE_OFFSET = 2;
		internal const int QUESTION_OFFSET = 4;
		internal const int ANSWER_OFFSET = 6;
		internal const int AUTHORITY_OFFSET = 8;
		internal const int ADDITIONAL_OFFSET = 10;


		internal static void writeInt2(int val, byte[] dst, int dstIndex) {
			dst[dstIndex++] = unchecked((byte)((val >> 8) & 0xFF));
			dst[dstIndex] = unchecked((byte)(val & 0xFF));
		}


		internal static void writeInt4(int val, byte[] dst, int dstIndex) {
			dst[dstIndex++] = unchecked((byte)((val >> 24) & 0xFF));
			dst[dstIndex++] = unchecked((byte)((val >> 16) & 0xFF));
			dst[dstIndex++] = unchecked((byte)((val >> 8) & 0xFF));
			dst[dstIndex] = unchecked((byte)(val & 0xFF));
		}


		internal static int readInt2(byte[] src, int srcIndex) {
			return ((src[srcIndex] & 0xFF) << 8) + (src[srcIndex + 1] & 0xFF);
		}


		internal static int readInt4(byte[] src, int srcIndex) {
			return ((src[srcIndex] & 0xFF) << 24) + ((src[srcIndex + 1] & 0xFF) << 16) + ((src[srcIndex + 2] & 0xFF) << 8) + (src[srcIndex + 3] & 0xFF);
		}


		internal static int readNameTrnId(byte[] src, int srcIndex) {
			return readInt2(src, srcIndex);
		}

		internal int addrIndex;
		internal NbtAddress[] addrEntry;

		internal int nameTrnId;

		internal int opCode, resultCode, questionCount, answerCount, authorityCount, additionalCount;
		internal bool received, isResponse, isAuthAnswer, isTruncated, isRecurDesired, isRecurAvailable, isBroadcast;

		internal Name questionName;
		internal Name recordName;

		internal int questionType, questionClass, recordType, recordClass, ttl, rDataLength;

		internal IPAddress addr;
		protected internal Configuration config;


		internal NameServicePacket(Configuration config) {
			this.config = config;
			this.isRecurDesired = true;
			this.isBroadcast = true;
			this.questionCount = 1;
			this.questionClass = IN;
		}


		internal virtual int writeWireFormat(byte[] dst, int dstIndex) {
			int start = dstIndex;
			dstIndex += writeHeaderWireFormat(dst, dstIndex);
			dstIndex += writeBodyWireFormat(dst, dstIndex);
			return dstIndex - start;
		}


		internal virtual int readWireFormat(byte[] src, int srcIndex) {
			int start = srcIndex;
			srcIndex += readHeaderWireFormat(src, srcIndex);
			srcIndex += readBodyWireFormat(src, srcIndex);
			return srcIndex - start;
		}


		internal virtual int writeHeaderWireFormat(byte[] dst, int dstIndex) {
			int start = dstIndex;
			writeInt2(this.nameTrnId, dst, dstIndex);
			dst[dstIndex + OPCODE_OFFSET] = (byte)((this.isResponse ? 0x80 : 0x00) + ((this.opCode << 3) & 0x78) + (this.isAuthAnswer ? 0x04 : 0x00) + (this.isTruncated ? 0x02 : 0x00) + (this.isRecurDesired ? 0x01 : 0x00));
			dst[dstIndex + OPCODE_OFFSET + 1] = (byte)((this.isRecurAvailable ? 0x80 : 0x00) + (this.isBroadcast ? 0x10 : 0x00) + (this.resultCode & 0x0F));
			writeInt2(this.questionCount, dst, start + QUESTION_OFFSET);
			writeInt2(this.answerCount, dst, start + ANSWER_OFFSET);
			writeInt2(this.authorityCount, dst, start + AUTHORITY_OFFSET);
			writeInt2(this.additionalCount, dst, start + ADDITIONAL_OFFSET);
			return HEADER_LENGTH;
		}


		internal virtual int readHeaderWireFormat(byte[] src, int srcIndex) {
			this.nameTrnId = readInt2(src, srcIndex);
			this.isResponse = ((src[srcIndex + OPCODE_OFFSET] & 0x80) == 0) ? false : true;
			this.opCode = (src[srcIndex + OPCODE_OFFSET] & 0x78) >> 3;
			this.isAuthAnswer = ((src[srcIndex + OPCODE_OFFSET] & 0x04) == 0) ? false : true;
			this.isTruncated = ((src[srcIndex + OPCODE_OFFSET] & 0x02) == 0) ? false : true;
			this.isRecurDesired = ((src[srcIndex + OPCODE_OFFSET] & 0x01) == 0) ? false : true;
			this.isRecurAvailable = ((src[srcIndex + OPCODE_OFFSET + 1] & 0x80) == 0) ? false : true;
			this.isBroadcast = ((src[srcIndex + OPCODE_OFFSET + 1] & 0x10) == 0) ? false : true;
			this.resultCode = src[srcIndex + OPCODE_OFFSET + 1] & 0x0F;
			this.questionCount = readInt2(src, srcIndex + QUESTION_OFFSET);
			this.answerCount = readInt2(src, srcIndex + ANSWER_OFFSET);
			this.authorityCount = readInt2(src, srcIndex + AUTHORITY_OFFSET);
			this.additionalCount = readInt2(src, srcIndex + ADDITIONAL_OFFSET);
			return HEADER_LENGTH;
		}


		internal virtual int writeQuestionSectionWireFormat(byte[] dst, int dstIndex) {
			int start = dstIndex;
			dstIndex += this.questionName.writeWireFormat(dst, dstIndex);
			writeInt2(this.questionType, dst, dstIndex);
			dstIndex += 2;
			writeInt2(this.questionClass, dst, dstIndex);
			dstIndex += 2;
			return dstIndex - start;
		}


		internal virtual int readQuestionSectionWireFormat(byte[] src, int srcIndex) {
			int start = srcIndex;
			srcIndex += this.questionName.readWireFormat(src, srcIndex);
			this.questionType = readInt2(src, srcIndex);
			srcIndex += 2;
			this.questionClass = readInt2(src, srcIndex);
			srcIndex += 2;
			return srcIndex - start;
		}


		internal virtual int writeResourceRecordWireFormat(byte[] dst, int dstIndex) {
			int start = dstIndex;
			if (this.recordName == this.questionName) {
				dst[dstIndex++] = unchecked((byte) 0xC0); // label string pointer to
				dst[dstIndex++] = (byte) 0x0C; // questionName (offset 12)
			}
			else {
				dstIndex += this.recordName.writeWireFormat(dst, dstIndex);
			}
			writeInt2(this.recordType, dst, dstIndex);
			dstIndex += 2;
			writeInt2(this.recordClass, dst, dstIndex);
			dstIndex += 2;
			writeInt4(this.ttl, dst, dstIndex);
			dstIndex += 4;
			this.rDataLength = writeRDataWireFormat(dst, dstIndex + 2);
			writeInt2(this.rDataLength, dst, dstIndex);
			dstIndex += 2 + this.rDataLength;
			return dstIndex - start;
		}


		internal virtual int readResourceRecordWireFormat(byte[] src, int srcIndex) {
			int start = srcIndex;
			int end;

			if ((src[srcIndex] & 0xC0) == 0xC0) {
				this.recordName = this.questionName; // label string pointer to questionName
				srcIndex += 2;
			}
			else {
				srcIndex += this.recordName.readWireFormat(src, srcIndex);
			}
			this.recordType = readInt2(src, srcIndex);
			srcIndex += 2;
			this.recordClass = readInt2(src, srcIndex);
			srcIndex += 2;
			this.ttl = readInt4(src, srcIndex);
			srcIndex += 4;
			this.rDataLength = readInt2(src, srcIndex);
			srcIndex += 2;

			this.addrEntry = new NbtAddress[this.rDataLength / 6];
			end = srcIndex + this.rDataLength;
			/*
			 * Apparently readRDataWireFormat can return 0 if resultCode != 0 in
			 * which case this will look indefinitely. Putting this else clause around
			 * the loop might fix that. But I would need to see a capture to confirm.
			 * if (resultCode != 0) {
			 * srcIndex += rDataLength;
			 * } else {
			 */
			for (this.addrIndex = 0; srcIndex < end; this.addrIndex++) {
				srcIndex += readRDataWireFormat(src, srcIndex);
			}

			return srcIndex - start;
		}


		internal abstract int writeBodyWireFormat(byte[] dst, int dstIndex);


		internal abstract int readBodyWireFormat(byte[] src, int srcIndex);


		internal abstract int writeRDataWireFormat(byte[] dst, int dstIndex);


		internal abstract int readRDataWireFormat(byte[] src, int srcIndex);


		public override string ToString() {
			string opCodeString, resultCodeString, questionTypeString, recordTypeString;

			switch (this.opCode) {
			case QUERY:
				opCodeString = "QUERY";
				break;
			case WACK:
				opCodeString = "WACK";
				break;
			default:
				opCodeString = Convert.ToString(this.opCode);
				break;
			}
			switch (this.resultCode) {
			case FMT_ERR:
				resultCodeString = "FMT_ERR";
				break;
			case SRV_ERR:
				resultCodeString = "SRV_ERR";
				break;
			case IMP_ERR:
				resultCodeString = "IMP_ERR";
				break;
			case RFS_ERR:
				resultCodeString = "RFS_ERR";
				break;
			case ACT_ERR:
				resultCodeString = "ACT_ERR";
				break;
			case CFT_ERR:
				resultCodeString = "CFT_ERR";
				break;
			default:
				resultCodeString = "0x" + Hexdump.toHexString(this.resultCode, 1);
				break;
			}
			switch (this.questionType) {
			case NB:
				questionTypeString = "NB";
				break;
			case NBSTAT:
				questionTypeString = "NBSTAT";
				break;
			default:
				questionTypeString = "0x" + Hexdump.toHexString(this.questionType, 4);
				break;
			}
			switch (this.recordType) {
			case A:
				recordTypeString = "A";
				break;
			case NS:
				recordTypeString = "NS";
				break;
			case NULL:
				recordTypeString = "NULL";
				break;
			case NB:
				recordTypeString = "NB";
				break;
			case NBSTAT:
				recordTypeString = "NBSTAT";
				break;
			default:
				recordTypeString = "0x" + Hexdump.toHexString(this.recordType, 4);
				break;
			}

			return "nameTrnId=" + this.nameTrnId + ",isResponse=" + this.isResponse + ",opCode=" + opCodeString + ",isAuthAnswer=" + this.isAuthAnswer + ",isTruncated=" + this.isTruncated + ",isRecurAvailable=" + this.isRecurAvailable + ",isRecurDesired=" + this.isRecurDesired + ",isBroadcast=" + this.isBroadcast + ",resultCode=" + resultCodeString + ",questionCount=" + this.questionCount + ",answerCount=" + this.answerCount + ",authorityCount=" + this.authorityCount + ",additionalCount=" + this.additionalCount + ",questionName=" + this.questionName + ",questionType=" + questionTypeString + ",questionClass=" + (this.questionClass == IN ? "IN" : "0x" + Hexdump.toHexString(this.questionClass, 4)) + ",recordName=" + this.recordName + ",recordType=" + recordTypeString + ",recordClass=" + (this.recordClass == IN ? "IN" : "0x" + Hexdump.toHexString(this.recordClass, 4)) + ",ttl=" + this.ttl + ",rDataLength=" + this.rDataLength;
		}
	}

}