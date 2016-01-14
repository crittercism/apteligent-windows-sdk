using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CrittercismSDK {
    internal class UserflowConverter : JsonConverter {
        public override void WriteJson(JsonWriter writer,object value,JsonSerializer serializer) {
            Userflow userflow = (Userflow)value;
            JArray a = userflow.ToJArray();
            a.WriteTo(writer);
        }
        internal static bool IsUserflowJson(JArray a) {
            bool answer = (a != null);
            answer = answer && (a.Count == (int)UserflowIndex.COUNT);
            answer = answer && (a[(int)UserflowIndex.Name].Type == JTokenType.String);
            answer = answer && (a[(int)UserflowIndex.State].Type == JTokenType.Integer);
            answer = answer && ((a[(int)UserflowIndex.Timeout].Type == JTokenType.Integer)
                                || (a[(int)UserflowIndex.Timeout].Type == JTokenType.Float));
            answer = answer && (a[(int)UserflowIndex.Value].Type == JTokenType.Integer);
            answer = answer && (a[(int)UserflowIndex.Metadata].Type == JTokenType.Object);
            answer = answer && JsonUtils.IsJsonDate(a[(int)UserflowIndex.BeginTime]);
            answer = answer && JsonUtils.IsJsonDate(a[(int)UserflowIndex.EndTime]);
            answer = answer && ((a[(int)UserflowIndex.EyeTime].Type == JTokenType.Integer)
                                || (a[(int)UserflowIndex.EyeTime].Type == JTokenType.Float));
            return answer;
        }
        public override object ReadJson(JsonReader reader,Type objectType,object existingValue,JsonSerializer serializer) {
            Userflow userflow = null;
            // Load JArray from stream .  For better or worse, probably a bit of the latter,
            // Newtonsoft.Json deserializes a persisted timestamp string as a JTokenType.Date .
            JArray a = JArray.Load(reader);
            if (IsUserflowJson(a)) {
                // Extract values from "JArray a" .  This is all according to the
                // Crittercism "Userflows Wire Protocol - v1" in Confluence.
                string name = (string)((JValue)(a[(int)UserflowIndex.Name])).Value;
                UserflowState state = (UserflowState)(long)((JValue)(a[(int)UserflowIndex.State])).Value;
                double timeoutSeconds = Convert.ToDouble(((JValue)(a[(int)UserflowIndex.Timeout])).Value); // seconds (!!!)
                int timeout = (int)Convert.ToDouble(timeoutSeconds * TimeUtils.MSEC_PER_SEC); // milliseconds
                int value = Convert.ToInt32(((JValue)(a[(int)UserflowIndex.Value])).Value);
#if DEBUG
                {
                    // NOTE: Userflow metadata skeleton in the closet.  Tossed around
                    // in early design and even implemented in iOS SDK client side.  It
                    // was never supported on platform nor publicly exposed to users.
                    JObject o = a[(int)UserflowIndex.Metadata] as JObject;
                    Debug.Assert(o.Count == 0);
                }
#endif
                Dictionary<string,string> metadata = new Dictionary<string,string>();
                long beginTime = JsonUtils.JsonDateToTicks(a[(int)UserflowIndex.BeginTime]); // ticks
                long endTime = JsonUtils.JsonDateToTicks(a[(int)UserflowIndex.EndTime]); // ticks
                double eyeTimeSeconds = Convert.ToDouble(((JValue)(a[(int)UserflowIndex.EyeTime])).Value); // seconds (!!!)
                long eyeTime = (long)(eyeTimeSeconds * TimeUtils.TICKS_PER_SEC); // ticks
                // Call Userflow constructor.
                userflow = new Userflow(
                    name,
                    state,
                    timeout,
                    value,
                    metadata,
                    beginTime,
                    endTime,
                    eyeTime
                );
            }
            return userflow;
        }
        public override bool CanConvert(Type objectType) {
            return objectType == typeof(Userflow);
        }
    }
}
