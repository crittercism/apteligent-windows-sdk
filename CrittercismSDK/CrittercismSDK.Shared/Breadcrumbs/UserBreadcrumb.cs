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
        ////////////////////////////////////////////////////////////////
        // NOTE: RE LEGACY "Windows style" BREADCRUMBS (windowsStyle = true)
        // * Platform's "crash" and "errors" Windows endpoints currently
        // want the legacy "Windows style" dictionary object format.
        // * Platform's "transactions" endpoint only accepts the newer
        // array format.
        ////////////////////////////////////////////////////////////////

        #region Properties
        private string timestamp; // GMTDateString
        private string message;
        internal bool windowsStyle = true;
        #endregion

        #region Constructor
        internal UserBreadcrumb(string timestamp,string message,bool windowsStyle) {
            this.timestamp = timestamp;
            this.message = message;
            this.windowsStyle = windowsStyle;
        }
        internal UserBreadcrumb(Breadcrumb breadcrumb,bool windowsStyle) {
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
            this.windowsStyle = windowsStyle;
        }
        #endregion

        #region JSON
        private JObject ToJObject() {
            JObject answer = new JObject();
            answer["message"] = message;
            answer["timestamp"] = timestamp;
            return answer;
        }
        private JArray ToJArray() {
            List<JToken> list = new List<JToken>();
            list.Add(message);
            list.Add(timestamp);
            JArray answer = new JArray(list);
            return answer;
        }
        internal JToken ToJToken() {
            JToken answer = null;
            if (windowsStyle) {
                answer = ToJObject();
            } else {
                answer = ToJArray();
            }
            return answer;
        }
        #endregion
    }
}
