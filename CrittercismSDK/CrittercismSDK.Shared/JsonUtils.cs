using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CrittercismSDK
{
    internal class JsonUtils {
        internal static double StringToExtendedReal(Object x) {
            double answer = 0.0;
            if (x is String) {
                String s = (String)x;
                if (s.Equals("INFINITY",StringComparison.OrdinalIgnoreCase)) {
                    answer = double.PositiveInfinity;
                } else if (s.Equals("+INFINITY",StringComparison.OrdinalIgnoreCase)) {
                    answer = double.PositiveInfinity;
                } else if (s.Equals("-INFINITY",StringComparison.OrdinalIgnoreCase)) {
                    answer = double.NegativeInfinity;
                } else if (s.Equals("NAN",StringComparison.OrdinalIgnoreCase)) {
                    answer = double.NaN;
                } else {
                    Debug.WriteLine(String.Format("JSON {0} isn't extended real",x));
                }
            } else if (IsNumber(x)) {
                answer = (Double)x;
            } else {
                Debug.WriteLine(String.Format("JSON {0} isn't extended real",x));
            }
            return answer;
        }
        internal static bool IsNumber(Object x) {
            bool answer = ((x is Byte)
                || (x is Decimal)
                || (x is Double)
                || (x is Int16)
                || (x is Int32)
                || (x is Int64)
                || (x is SByte)
                || (x is Single)
                || (x is UInt16)
                || (x is UInt32)
                || (x is UInt64));
            return answer;
        }
        internal static bool IsJsonDate(JToken json) {
            // Covering up the sins of not strictly RFC 7159 compliant
            // Newtonsoft.Json which is schizophrenic about converting strings
            // into either JTokenType.String's or JTokenType.Date's.
            bool answer = ((json.Type == JTokenType.Date) || (json.Type == JTokenType.String));
            return answer;
        }
        internal static long JsonDateToTicks(JToken json) {
            // json that IsJsonDate converted to long ticks .
            long answer = 0;
            switch (json.Type) {
                case JTokenType.Date:
                    answer = ((DateTime)((JValue)json).Value).ToUniversalTime().Ticks;  // ticks
                    break;
                case JTokenType.String:
                    answer = DateUtils.StringToTicks((string)((JValue)json).Value);  // ticks
                    break;
            }
            return answer;
        }
    }
}
