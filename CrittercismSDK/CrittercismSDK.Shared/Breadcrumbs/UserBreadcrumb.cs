using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;
using System.Text;

namespace CrittercismSDK {
    [JsonConverter(typeof(UserBreadcrumbConverter))]
    internal class UserBreadcrumb {
        #region Properties
        private string timestamp;
        private string message;
        #endregion

        #region Constructor
        internal UserBreadcrumb(string timestamp,string message) {
            this.timestamp = timestamp;
            this.message = message;
        }
        internal UserBreadcrumb(Breadcrumb breadcrumb) {
            // Convert a Breadcrumb into a UserBreadcrumb
            this.timestamp = breadcrumb.GetTimestamp();
            string message = "";
            switch (breadcrumb.GetBreadcrumbType()) {
                case BreadcrumbType.Launch:
                    message = "session_start";
                    break;
                case BreadcrumbType.Text:
                    {
                        // 1 - user breadcrumb       ; {text:,level:}
                        Dictionary<string,Object> data = (Dictionary<string,Object>)breadcrumb.GetData();
                        message = (string)data["text"];
                    }
                    break;
            }
            this.message = message;
        }
        #endregion

        #region JSON
        internal JObject ToJObject() {
            JObject answer = new JObject();
            answer["message"] = message;
            answer["timestamp"] = timestamp;
            return answer;
        }
        #endregion
    }
}
