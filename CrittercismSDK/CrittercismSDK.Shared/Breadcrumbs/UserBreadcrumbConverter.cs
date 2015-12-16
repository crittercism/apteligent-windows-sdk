using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CrittercismSDK {
    internal class UserBreadcrumbConverter : JsonConverter {
        public override void WriteJson(JsonWriter writer,object value,JsonSerializer serializer) {
            UserBreadcrumb userBreadcrumb = (UserBreadcrumb)value;
            JToken t = userBreadcrumb.ToJToken();
            t.WriteTo(writer);
        }
        internal static bool IsUserBreadcrumbJson(JToken t) {
            ////////////////////////////////////////////////////////////////
            // UserBreadcrumb's are agnostic about deserializing from
            // either form,  { "message":message,"timestamp":timestamp} or [message,timestamp].
            // * Platform's "crash" and "errors" Windows endpoints currently
            // want the legacy "Windows style" dictionary object format.
            // * Platform's "transactions" endpoint only accepts the newer
            // array format.
            ////////////////////////////////////////////////////////////////
            bool answer = (t != null);
            if (t.Type == JTokenType.Object) {
                // * Platform's "crash" and "errors" Windows endpoints currently
                // want the legacy "Windows style" dictionary object format.
                JObject o = (JObject)t;
                answer = answer && (o.Count == 2);
                {
                    JToken message = null;
                    answer = answer && o.TryGetValue("message",out message);
                    answer = answer && (message.Type == JTokenType.String);
                }
                {
                    JToken timestamp = null;
                    answer = answer && o.TryGetValue("timestamp",out timestamp);
                    answer = answer && JsonUtils.IsJsonDate(timestamp);
                }
            } else if (t.Type == JTokenType.Array) {
                // * Platform's "transactions" endpoint only accepts the newer
                // array format.
                JArray a = (JArray)t;
                answer = answer && (a.Count == 2);
                {
                    JToken message = a[0];
                    answer = answer && (message.Type == JTokenType.String);
                }
                {
                    JToken timestamp = a[1];
                    answer = answer && JsonUtils.IsJsonDate(timestamp);
                }
            } else {
                answer = false;
            }
            return answer;
        }
        public override object ReadJson(JsonReader reader,Type objectType,object existingValue,JsonSerializer serializer) {
            UserBreadcrumb userBreadcrumb = null;
            // Load JObject from stream .  For better or worse, probably a bit of the latter,
            // Newtonsoft.Json deserializes a persisted timestamp string as a JTokenType.Date .
            JToken t = JToken.Load(reader);
            string timestamp = "";
            string message = "";
            bool windowsStyle = true;
            if (IsUserBreadcrumbJson(t)) {
                if (t.Type == JTokenType.Object) {
                    // Extract values from "JObject o" .
                    JObject o = (JObject)t;
                    timestamp = JsonUtils.JsonDateToGMTDateString(o["timestamp"]);
                    message = (string)((JValue)(o["message"])).Value;
                    windowsStyle = true;
                } else if (t.Type == JTokenType.Array) {
                    // Extract values from "JArray a" .
                    JArray a = (JArray)t;
                    timestamp = JsonUtils.JsonDateToGMTDateString(a[0]);
                    message = (string)((JValue)(a[1])).Value;
                    windowsStyle = false;
                } else {
                    // Shouldn't get here.
                    Debug.Assert(false,"ReadJson failure");
                }
                // Call UserBreadcrumb constructor.
                userBreadcrumb = new UserBreadcrumb(
                    timestamp,
                    message,
                    windowsStyle
                );
            }
            return userBreadcrumb;
        }
        public override bool CanConvert(Type objectType) {
            return objectType == typeof(UserBreadcrumb);
        }
    }
}
