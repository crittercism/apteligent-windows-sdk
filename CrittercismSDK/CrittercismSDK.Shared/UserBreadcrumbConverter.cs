using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CrittercismSDK
{
    internal class UserBreadcrumbConverter : JsonConverter {
        public override void WriteJson(JsonWriter writer,object value,JsonSerializer serializer) {
            UserBreadcrumb userBreadcrumb = (UserBreadcrumb)value;
            JArray a = userBreadcrumb.ToJArray();
            a.WriteTo(writer);
        }
        public override object ReadJson(JsonReader reader,Type objectType,object existingValue,JsonSerializer serializer) {
            UserBreadcrumb userBreadcrumb = null;
            // Load JArray from stream .  For better or worse, probably a bit of the latter,
            // Newtonsoft.Json deserializes a persisted timestamp string as a JTokenType.Date .
            JArray a = JArray.Load(reader);
            if ((a != null)
                && (a.Count == (int)UserBreadcrumbIndex.COUNT)
                && (a[(int)UserBreadcrumbIndex.Message].Type == JTokenType.String)
                && (a[(int)UserBreadcrumbIndex.Timestamp].Type == JTokenType.Date)) {
                // Extract values from "JArray a" .
                string message = (string)((JValue)(a[(int)UserBreadcrumbIndex.Message])).Value;
                string timestamp = DateUtils.ISO8601DateString((DateTime)((JValue)(a[(int)UserBreadcrumbIndex.Timestamp])).Value);
                // Call Endpoint constructor.
                userBreadcrumb = new UserBreadcrumb(
                    message,
                    timestamp
                );
            }
            return userBreadcrumb;
        }
        public override bool CanConvert(Type objectType) {
            return objectType == typeof(Endpoint);
        }
    }
}
