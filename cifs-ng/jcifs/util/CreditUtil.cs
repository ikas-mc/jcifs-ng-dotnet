using System;
namespace jcifs.util {
	public class CreditUtil {
		public const int SINGLE_CREDIT_SIZE = 65536;

		public static int calcCreditCost(int length) {
			return Math.Abs((length - 1) / SINGLE_CREDIT_SIZE) + 1;
		}
	}
}