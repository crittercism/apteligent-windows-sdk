using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;

namespace CrittercismSDK {
    public class CRFilter {
        private string value = null;
        private StringComparison comparisonType;
        private Regex rgx;

        private CRFilter(string value,StringComparison comparisonType,Regex rgx) {
            this.value = value;
            this.comparisonType = comparisonType;
            this.rgx = rgx;
        }

        public static CRFilter FilterWithString(string value) {
            return FilterWithString(value,StringComparison.OrdinalIgnoreCase);
        }

        public static CRFilter FilterWithString(string value,StringComparison comparisonType) {
            return new CRFilter(value,comparisonType,null);
        }

        public static CRFilter FilterWithRegex(Regex rgx) {
            return new CRFilter(null,StringComparison.OrdinalIgnoreCase,rgx);
        }

        internal bool IsMatch(string input) {
            bool answer = false;
            if (value is string) {
                answer = (input.IndexOf(value,comparisonType) >= 0);
            } else if (rgx is Regex) {
                answer = rgx.IsMatch(input);
            };
            return answer;
        }

    }
}
