using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CrittercismSDK {
    internal class BreadcrumbConverter : JsonConverter {
        public override void WriteJson(JsonWriter writer,object value,JsonSerializer serializer) {
            Breadcrumb breadcrumb = (Breadcrumb)value;
            JArray a = new JArray(breadcrumb.ToJArray());
            a.WriteTo(writer);
        }
        internal static bool IsBreadcrumbJson(JArray a) {
            bool answer = (a != null);
            answer = answer && (a.Count <= (int)BreadcrumbIndex.COUNT);
            answer = answer && JsonUtils.IsJsonDate(a[(int)BreadcrumbIndex.Timestamp]);
            answer = answer && (a[(int)BreadcrumbIndex.Type].Type == JTokenType.Integer);
            return answer;
        }
        public override object ReadJson(JsonReader reader,Type objectType,object existingValue,JsonSerializer serializer) {
            Breadcrumb breadcrumb = null;
            // Load JArray from stream .  For better or worse, probably a bit of the latter,
            // Newtonsoft.Json deserializes a persisted timestamp string as a JTokenType.Date .
            JArray a = JArray.Load(reader);
            if (IsBreadcrumbJson(a)) {
                // Extract values from "JArray a" .
                string timestamp = JsonUtils.JsonDateToGMTDateString(a[(int)BreadcrumbIndex.Timestamp]);
                BreadcrumbType breadcrumbType = (BreadcrumbType)(long)((JValue)(a[(int)BreadcrumbIndex.Type])).Value;
                Object data = null;
                // Launch = 0,      // 0 - session launched      ; --
                // Text,            // 1 - user breadcrumb       ; {text:,level:}
                // Network,         // 2 - network breadcrumb    ; [verb,url,...,statusCode,errorCode]
                // Event,           // 3 - app event             ; {event:}
                // Reachability,    // 4 - network change        ; {change:,type:,oldType:,newType:}
                // View,            // 5 - uiview change / load  ; {event:,viewName:}
                // Error,           // 6 - handled exception     ; {name:,reason:}
                // Crash,           // 7 - crash                 ; {name:,reason:}
                if (breadcrumbType==BreadcrumbType.Launch) {
                    // SPEC: "[Session launched] is special in that it will only have a timestamp
                    // and breadcrumb type field".
                } else {
                    JToken dataToken = a[(int)BreadcrumbIndex.Data];
                    switch (breadcrumbType) {
                        case BreadcrumbType.Network:
                            data = dataToken.ToObject(typeof(Endpoint));
                            break;
                        default:
                            // TODO: This might be good enough, but should double check again later.
                            data = dataToken.ToObject(typeof(Dictionary<string,Object>));
                            break;
                    }
                }
                // Call Breadcrumb constructor.
                breadcrumb = new Breadcrumb(
                    timestamp,
                    breadcrumbType,
                    data
                );
            }
            return breadcrumb;
        }
        public override bool CanConvert(Type objectType) {
            return objectType == typeof(Breadcrumb);
        }
    }
}
