using System.Globalization;
using System.Threading;

namespace Common {
	public class CsharpUtils {
		public static void FixCsharpBadDecimalSeparator() {
			CultureInfo customCulture = (CultureInfo) Thread.CurrentThread.CurrentCulture.Clone();
			customCulture.NumberFormat.NumberDecimalSeparator = ".";
			Thread.CurrentThread.CurrentCulture = customCulture;
		}
	}
}
