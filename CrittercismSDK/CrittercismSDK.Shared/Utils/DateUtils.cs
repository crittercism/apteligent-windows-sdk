using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace CrittercismSDK {
    class DateUtils {
        internal static string GMTDateString(DateTime dateTime) {
            return dateTime.ToUniversalTime().ToString("s",CultureInfo.InvariantCulture) + "Z";
        }
        internal static string ISO8601DateString(DateTime dateTime) {
            return dateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ",CultureInfo.InvariantCulture);
        }
        internal static long StringToTicks(string timestamp) {
            return Convert.ToDateTime(timestamp).Ticks;
        }
    }
}
