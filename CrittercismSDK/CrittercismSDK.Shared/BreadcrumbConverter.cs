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
        public override object ReadJson(JsonReader reader,Type objectType,object existingValue,JsonSerializer serializer) {
            Breadcrumb breadcrumb = null;
            // Load JArray from stream .  For better or worse, probably a bit of the latter,
            // Newtonsoft.Json deserializes a persisted timestamp string as a JTokenType.Date .
            JArray a = JArray.Load(reader);
            if ((a!=null)
                && (a.Count <= (int)BreadcrumbIndex.COUNT)
                && (a[(int)BreadcrumbIndex.Timestamp].Type == JTokenType.Date)
                && (a[(int)BreadcrumbIndex.Type].Type == JTokenType.Integer)) {
                // Extract values from "JArray a" .
                string timestamp = DateUtils.ISO8601DateString((DateTime)((JValue)(a[(int)BreadcrumbIndex.Timestamp])).Value);
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
