using System;
using System.Collections.Generic;
using System.Text;

namespace CrittercismSDK
{
    internal class StringUtils {
        internal static string TruncatedString(string s) {
            // Truncate string s max allowed string length (255 characters, not including null character)
            const int maxStringLength = 255;
            string answer = s;
            if (s.Length > maxStringLength) {
                DebugUtils.LOG_WARN(String.Format("Truncating long string to 255 characters: \"{0}\"",s));
                answer = s.Substring(0,maxStringLength);
            }
            return answer;
        }
    }
}
