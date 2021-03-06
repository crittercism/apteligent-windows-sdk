﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace CrittercismSDK {
    class TimeUtils {
        #region Constants
        internal const int MSEC_PER_SEC = 1000;
        internal const int TICKS_PER_MSEC = 10000;
        internal const int TICKS_PER_SEC = MSEC_PER_SEC * TICKS_PER_MSEC;
        #endregion

        #region Methods
        internal static string GMTDateString(DateTime dateTime) {
            return dateTime.ToUniversalTime().ToString("s",CultureInfo.InvariantCulture) + "Z";
        }
        internal static string ISO8601DateString(DateTime dateTime) {
            return dateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ",CultureInfo.InvariantCulture);
        }
        internal static long StringToTicks(string timestamp) {
            return Convert.ToDateTime(timestamp).ToUniversalTime().Ticks;
        }
        #endregion
    }
}
