using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CrittercismSDK {
    internal class UserFlowConverter : JsonConverter {
        public override void WriteJson(JsonWriter writer,object value,JsonSerializer serializer) {
            UserFlow userFlow = (UserFlow)value;
            JArray a = userFlow.ToJArray();
            a.WriteTo(writer);
        }
        internal static bool IsUserFlowJson(JArray a) {
            bool answer = (a != null);
            answer = answer && (a.Count == (int)UserFlowIndex.COUNT);
            answer = answer && (a[(int)UserFlowIndex.Name].Type == JTokenType.String);
            answer = answer && (a[(int)UserFlowIndex.State].Type == JTokenType.Integer);
            answer = answer && ((a[(int)UserFlowIndex.Timeout].Type == JTokenType.Integer)
                                || (a[(int)UserFlowIndex.Timeout].Type == JTokenType.Float));
            answer = answer && (a[(int)UserFlowIndex.Value].Type == JTokenType.Integer);
            answer = answer && (a[(int)UserFlowIndex.Metadata].Type == JTokenType.Object);
            answer = answer && JsonUtils.IsJsonDate(a[(int)UserFlowIndex.BeginTime]);
            answer = answer && JsonUtils.IsJsonDate(a[(int)UserFlowIndex.EndTime]);
            answer = answer && ((a[(int)UserFlowIndex.EyeTime].Type == JTokenType.Integer)
                                || (a[(int)UserFlowIndex.EyeTime].Type == JTokenType.Float));
            return answer;
        }
        public override object ReadJson(JsonReader reader,Type objectType,object existingValue,JsonSerializer serializer) {
            UserFlow userFlow = null;
            // Load JArray from stream .  For better or worse, probably a bit of the latter,
            // Newtonsoft.Json deserializes a persisted timestamp string as a JTokenType.Date .
            JArray a = JArray.Load(reader);
            if (IsUserFlowJson(a)) {
                // Extract values from "JArray a" .  This is all according to the
                // Crittercism "UserFlows Wire Protocol - v1" in Confluence.
                string name = (string)((JValue)(a[(int)UserFlowIndex.Name])).Value;
                UserFlowState state = (UserFlowState)(long)((JValue)(a[(int)UserFlowIndex.State])).Value;
                double timeoutSeconds = Convert.ToDouble(((JValue)(a[(int)UserFlowIndex.Timeout])).Value); // seconds (!!!)
                int timeout = (int)Convert.ToDouble(timeoutSeconds * TimeUtils.MSEC_PER_SEC); // milliseconds
                int value = Convert.ToInt32(((JValue)(a[(int)UserFlowIndex.Value])).Value);
#if DEBUG
                {
                    // NOTE: UserFlow metadata skeleton in the closet.  Tossed around
                    // in early design and even implemented in iOS SDK client side.  It
                    // was never supported on platform nor publicly exposed to users.
                    JObject o = a[(int)UserFlowIndex.Metadata] as JObject;
                    Debug.Assert(o.Count == 0);
                }
#endif
                Dictionary<string,string> metadata = new Dictionary<string,string>();
                long beginTime = JsonUtils.JsonDateToTicks(a[(int)UserFlowIndex.BeginTime]); // ticks
                long endTime = JsonUtils.JsonDateToTicks(a[(int)UserFlowIndex.EndTime]); // ticks
                double eyeTimeSeconds = Convert.ToDouble(((JValue)(a[(int)UserFlowIndex.EyeTime])).Value); // seconds (!!!)
                long eyeTime = (long)(eyeTimeSeconds * TimeUtils.TICKS_PER_SEC); // ticks
                // Call UserFlow constructor.
                userFlow = new UserFlow(
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
            return userFlow;
        }
        public override bool CanConvert(Type objectType) {
            return objectType == typeof(UserFlow);
        }
    }
}
