/*
 *	jcifs-ng for dotnet
 *  ikas-mc@github 2021
 */

using System;
namespace org.slf4j {
	public class LoggerFactory {
		public static Logger getLogger(Type type) {
			if (null == factory) {
				lock (typeof(LoggerFactory)) {
					if (null == factory) {
						factory = DefaultFactory;
					}
				}
			}
			return factory(type.FullName);
		}

		private static volatile Func<string, Logger> factory;
		private static readonly Func<string, Logger> DefaultFactory = (name) => new SimpleLogger(name, Level.TRACE);
		public static void configLoggerFactory(Func<string, Logger> loggerFactory) {
			factory = loggerFactory ?? DefaultFactory;
		}
	}
}