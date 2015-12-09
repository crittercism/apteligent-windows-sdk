﻿using System;
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
            JObject o = userBreadcrumb.ToJObject();
            o.WriteTo(writer);
        }
        internal static bool IsUserBreadcrumbJson(JObject o) {
            bool answer = (o != null);
            answer = answer && (o.Count == 2);
            {
                JToken timestamp = null;
                answer = answer && o.TryGetValue("timestamp",out timestamp);
                answer = answer && JsonUtils.IsJsonDate(timestamp);
            }
            {
                JToken message = null;
                answer = answer && o.TryGetValue("message",out message);
                answer = answer && (message.Type == JTokenType.String);
            }
            return answer;
        }
        public override object ReadJson(JsonReader reader,Type objectType,object existingValue,JsonSerializer serializer) {
            UserBreadcrumb userBreadcrumb = null;
            // Load JObject from stream .  For better or worse, probably a bit of the latter,
            // Newtonsoft.Json deserializes a persisted timestamp string as a JTokenType.Date .
            JObject o = JObject.Load(reader);
            if (IsUserBreadcrumbJson(o)) {
                // Extract values from "JObject o" .
                string timestamp = JsonUtils.JsonDateToISO8601DateString(o["timestamp"]);
                string message = (string)((JValue)(o["message"])).Value;
                // Call UserBreadcrumb constructor.
                userBreadcrumb = new UserBreadcrumb(
                    timestamp,
                    message
                );
            }
            return userBreadcrumb;
        }
        public override bool CanConvert(Type objectType) {
            return objectType == typeof(UserBreadcrumb);
        }
    }
}
