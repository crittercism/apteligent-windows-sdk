using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CrittercismSDK
{
    [JsonConverter(typeof(BreadcrumbConverter))]
    internal class Breadcrumb
    {
        ////////////////////////////////////////////////////////////////////////
        // "Unified Breadcrumbs Format" SPEC
        // https://crittercism.atlassian.net/wiki/display/DEV/Unified+Breadcrumbs+Format
        ////////////////////////////////////////////////////////////////////////

        #region Properties
        private string timestamp; // GMTDateString
        private BreadcrumbType breadcrumbType;
        private Object data;
        internal string GetTimestamp() {
            return timestamp;
        }
        internal BreadcrumbType GetBreadcrumbType() {
            return breadcrumbType;
        }
        internal Object GetData() {
            return data;
        }
        #endregion

        #region Constructor
        internal Breadcrumb(BreadcrumbType breadcrumbType,Object data)
            : this(DateUtils.GMTDateString(DateTime.UtcNow),breadcrumbType,data) {
        }
        internal Breadcrumb(string timestamp,BreadcrumbType breadcrumbType,Object data) {
            this.timestamp = timestamp;
            this.breadcrumbType = breadcrumbType;
            this.data = data;
        }
        #endregion

        #region JSON
        internal JArray ToJArray() {
            // Per "Unified Breadcrumbs Format" SPEC
            List<JToken> list = new List<JToken>();
            list.Add(timestamp);
            list.Add((int)breadcrumbType);
            if (breadcrumbType!=(int)BreadcrumbType.Launch) {
                // SPEC: "[Session launched] is special in that it will only have a timestamp
                // and breadcrumb type field".  There is implied assumption in the next line
                // of code that if "data" isn't a primitive type, then data knows how to serialize
                // itself into a JSON JToken of some sort via some other code we've written based
                // on the Type of the data (e.g. Endpoint).
                JToken dataObject = JToken.FromObject(data);
                list.Add(dataObject);
            }
            JArray answer = new JArray(list);
            return answer;
        }
        public override string ToString() {
            return JsonConvert.SerializeObject(this);
        }
        #endregion
    }
}
