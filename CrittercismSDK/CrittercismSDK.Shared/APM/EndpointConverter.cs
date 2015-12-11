using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CrittercismSDK {
    internal class EndpointConverter : JsonConverter {
        public override void WriteJson(JsonWriter writer,object value,JsonSerializer serializer) {
            Endpoint endpoint = (Endpoint)value;
            JArray a = endpoint.ToJArray();
            a.WriteTo(writer);
        }
        internal static bool IsEndpointJson(JArray a) {
            bool answer = (a != null);
            answer = answer && (a.Count <= (int)EndpointIndex.COUNT);
            answer = answer && (a[(int)EndpointIndex.Method].Type == JTokenType.String);
            answer = answer && (a[(int)EndpointIndex.UriString].Type == JTokenType.String);
            answer = answer && JsonUtils.IsJsonDate(a[(int)EndpointIndex.Timestamp]);
            answer = answer && (a[(int)EndpointIndex.Latency].Type == JTokenType.Integer);
            answer = answer && (a[(int)EndpointIndex.ActiveNetwork].Type == JTokenType.Integer);
            answer = answer && (a[(int)EndpointIndex.BytesRead].Type == JTokenType.Integer);
            answer = answer && (a[(int)EndpointIndex.BytesSent].Type == JTokenType.Integer);
            answer = answer && (a[(int)EndpointIndex.StatusCode].Type == JTokenType.Integer);
            answer = answer && (a[(int)EndpointIndex.ErrorTable].Type == JTokenType.Integer);
            answer = answer && (a[(int)EndpointIndex.ErrorCode].Type == JTokenType.Integer);
            return answer;
        }
        public override object ReadJson(JsonReader reader,Type objectType,object existingValue,JsonSerializer serializer) {
            Endpoint endpoint = null;
            // Load JArray from stream .  For better or worse, probably a bit of the latter,
            // Newtonsoft.Json deserializes a persisted timestamp string as a JTokenType.Date .
            JArray a = JArray.Load(reader);
            if (IsEndpointJson(a)) {
                // Extract values from "JArray a" .
                string method = (string)((JValue)(a[(int)EndpointIndex.Method])).Value;
                string uriString = (string)((JValue)(a[(int)EndpointIndex.UriString])).Value;
                string timestamp = JsonUtils.JsonDateToISO8601DateString(a[(int)EndpointIndex.Timestamp]);
                long latency = (long)((JValue)(a[(int)EndpointIndex.Latency])).Value;
#if DEBUG
                {
                    long activeNetwork = (long)((JValue)(a[(int)EndpointIndex.ActiveNetwork])).Value;
                    Debug.Assert(activeNetwork == Endpoint.ACTIVE_NETWORK);
                }
#endif
                long bytesRead = (long)((JValue)(a[(int)EndpointIndex.BytesRead])).Value;
                long bytesSent = (long)((JValue)(a[(int)EndpointIndex.BytesSent])).Value;
                long statusCode = (long)((JValue)(a[(int)EndpointIndex.StatusCode])).Value;
#if DEBUG
                {
                    long errorTable = (long)((JValue)(a[(int)EndpointIndex.ErrorTable])).Value;
                    Debug.Assert(errorTable == Endpoint.ERROR_TABLE_CODE);
                }
#endif
                long errorCode = (long)((JValue)(a[(int)EndpointIndex.ErrorCode])).Value;
                // Call Endpoint constructor.
                endpoint = new Endpoint(
                    method,
                    uriString,
                    timestamp,
                    latency,
                    bytesRead,
                    bytesSent,
                    (HttpStatusCode)statusCode,
                    (WebExceptionStatus)errorCode
                );
            }
            return endpoint;
        }
        public override bool CanConvert(Type objectType) {
            return objectType == typeof(Endpoint);
        }
    }
}
