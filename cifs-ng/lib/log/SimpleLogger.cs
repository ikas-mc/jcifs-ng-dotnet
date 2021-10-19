/*
 *	jcifs-ng for dotnet
 *  ikas-mc@github 2021
 */

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
namespace org.slf4j {

	public class SimpleLogger : Logger {
		private string name;
		private Level level;

		public SimpleLogger(string name, Level level) {
			this.name = name;
			this.level = level;
		}

		public bool isTraceEnabled() {
			return level <= Level.TRACE;
		}

		public bool isDebugEnabled() {
			return level <= Level.DEBUG;
		}

		public bool isInfoEnabled() {
			return level <= Level.INFO;
		}

		public bool isWarnEnabled() {
			return level <= Level.WARN;
		}

		public bool isErrorEnabled() {
			return level <= Level.ERROR;
		}

		public void debug(string msg, Exception e = null, [CallerMemberName] string caller = "", [CallerLineNumber] int line = -1) {
			if (isDebugEnabled()) {
				print(msg, e, caller, line);
			}
		}

		public void error(string msg, Exception e = null, [CallerMemberName] string caller = "", [CallerLineNumber] int line = -1) {
			if (isErrorEnabled()) {
				print(msg, e, caller, line);
			}
		}

		public void warn(string msg, Exception e = null, [CallerMemberName] string caller = "", [CallerLineNumber] int line = -1) {
			if (isWarnEnabled()) {
				print(msg, e, caller, line);
			}
		}



		public void trace(string msg, Exception e = null, [CallerMemberName] string caller = "", [CallerLineNumber] int line = -1) {
			if (isTraceEnabled()) {
				print(msg, e, caller, line);
			}
		}


		public void info(string msg, Exception e = null, [CallerMemberName] string caller = "", [CallerLineNumber] int line = -1) {
			if (isInfoEnabled()) {
				print(msg, e, caller, line);
			}
		}

		private void print(string msg, Exception e, string caller, int line, [CallerMemberName] string level = "") {
			Debug.WriteLine($"{level}-{DateTime.Now.ToString()}-{name}-{caller}:({line}) {msg}");
			Exception x = e;
			while (null != x) {
				Debug.WriteLine(x.Message);
				Debug.WriteLine(x.StackTrace);
				x = x.InnerException;
			}
		}
	}
}