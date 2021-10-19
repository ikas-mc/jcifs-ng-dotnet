/*
 *	jcifs-ng for dotnet
 *  ikas-mc@github 2021
 */

using System;
using System.Runtime.CompilerServices;
namespace org.slf4j {
	public enum Level {
		ERROR = 40,
		WARN = 30,
		INFO = 20,
		DEBUG = 10,
		TRACE = 0
	}

	public interface Logger {
		bool isTraceEnabled();
		bool isDebugEnabled();
		bool isInfoEnabled();
		bool isWarnEnabled();
		bool isErrorEnabled();
		void debug(string msg, Exception e = null, [CallerMemberName] string caller = "", [CallerLineNumber] int line = -1);
		void error(string msg, Exception e = null, [CallerMemberName] string caller = "", [CallerLineNumber] int line = -1);
		void warn(string msg, Exception e = null, [CallerMemberName] string caller = "", [CallerLineNumber] int line = -1);
		void trace(string msg, Exception e = null, [CallerMemberName] string caller = "", [CallerLineNumber] int line = -1);
		void info(string msg, Exception e = null, [CallerMemberName] string caller = "", [CallerLineNumber] int line = -1);
	}


}