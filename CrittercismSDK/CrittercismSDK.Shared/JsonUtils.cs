using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CrittercismSDK
{
    internal class JsonUtils
    {
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
    }
}
