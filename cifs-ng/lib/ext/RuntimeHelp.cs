/*
 *	jcifs-ng for dotnet
 *  ikas-mc@github 2021
 */

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Org.BouncyCastle.Security;
namespace cifs_ng.lib.ext {
	public class RuntimeHelp {

		private static readonly SecureRandom random = new SecureRandom();

		public static double nextDouble() => random.NextDouble();

		public static int identityHashCode(object ob) {
			return RuntimeHelpers.GetHashCode(ob);
		}

		public static int hashCode(object element) {
			return element == null ? 0 : element.GetHashCode();
		}

		public static int hashCode(params object[] obs) {
			if (obs == null)
				return 0;

			int result = 1;
			foreach (var element in obs) {
				result = 31 * result + (element == null ? 0 : element.GetHashCode());
			}
			return result;
		}

		public static string getOsName() {
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
				return "Windows";
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
				return "Linux";
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
				return "Osx";
			}
			return "";
		}
	}
}