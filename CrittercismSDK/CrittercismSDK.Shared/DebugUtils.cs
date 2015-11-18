using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CrittercismSDK
{
    internal class DebugUtils {
        internal static void LOG_ERROR(string message) {
            // "Release" error message visible to developers.
            message = "[Crittercism] error: " + message;
#if NETFX_CORE || WINDOWS_PHONE
            // TODO: Not right, but "Trace" doesn't exist in these .NET frameworks.
            Debug.WriteLine(message);
#else
            Trace.WriteLine(message);
#endif
        }
        internal static void LOG_WARN(string message) {
            // "Release" warn message visible to developers.
            message = "[Crittercism] warning: " + message;
#if NETFX_CORE || WINDOWS_PHONE
            // TODO: Not right, but "Trace" doesn't exist in these .NET frameworks.
            Debug.WriteLine(message);
#else
            Trace.WriteLine(message);
#endif
        }
    }
}
