/*
 *	jcifs-ng for dotnet
 *  ikas-mc@github 2021
 */

namespace cifs_ng.lib.security {
	public static class PrincipalHelp {
		public static bool implies(this Principal principal, Subject subject) {
			if (subject == null)
				return false;
			return subject.getPrincipals()?.Contains(principal) ?? false;
		}
	}


	public interface Principal {
		string getName();
	}
}