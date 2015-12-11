using System;
using System.Collections.Generic;
using System.Text;

namespace CrittercismSDK
{
    internal class StringUtils {
        internal static string TruncateString(string s,int maxLength) {
            // Truncate string s max allowed string length (maxLength characters, not including null character)
            string answer = s;
            if (s.Length > maxLength) {
                DebugUtils.LOG_WARN(String.Format("Truncating long string to {0} characters: \"{1}\"",maxLength,s));
                answer = s.Substring(0,maxLength);
            }
            return answer;
        }
    }
}
