using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CrittercismSDK {
    internal class TransactionConverter : JsonConverter {
        public override void WriteJson(JsonWriter writer,object value,JsonSerializer serializer) {
            Transaction transaction = (Transaction)value;
            JArray a = transaction.ToJArray();
            a.WriteTo(writer);
        }
        public override object ReadJson(JsonReader reader,Type objectType,object existingValue,JsonSerializer serializer) {
            Transaction transaction = null;
            // Load JArray from stream .  For better or worse, probably a bit of the latter,
            // Newtonsoft.Json deserializes a persisted timestamp string as a JTokenType.Date .
            JArray a = JArray.Load(reader);
            if ((a!=null)
                && (a.Count == (int)TransactionIndex.COUNT)
                && (a[(int)TransactionIndex.Name].Type == JTokenType.String)
                && (a[(int)TransactionIndex.State].Type == JTokenType.Integer)
                && ((a[(int)TransactionIndex.Timeout].Type == JTokenType.Integer)
                    || (a[(int)TransactionIndex.Timeout].Type == JTokenType.Float))
                && (a[(int)TransactionIndex.Value].Type == JTokenType.Integer)
                && (a[(int)TransactionIndex.Metadata].Type == JTokenType.Object)
                && (a[(int)TransactionIndex.BeginTime].Type == JTokenType.Date)
                && (a[(int)TransactionIndex.EndTime].Type == JTokenType.Date)
                && ((a[(int)TransactionIndex.EyeTime].Type == JTokenType.Integer)
                    ||(a[(int)TransactionIndex.EyeTime].Type == JTokenType.Float))) {
                // Extract values from "JArray a" .  This is all according to the
                // Crittercism "Transactions Wire Protocol - v1" in Confluence.
                string name = (string)((JValue)(a[(int)TransactionIndex.Name])).Value;
                TransactionState state = (TransactionState)(long)((JValue)(a[(int)TransactionIndex.State])).Value;
                double timeoutSeconds = Convert.ToDouble(((JValue)(a[(int)TransactionIndex.Timeout])).Value);  // seconds (!!!)
                int timeout = (int)Convert.ToDouble(timeoutSeconds*Transaction.MSEC_PER_SEC);  // milliseconds
                int value = Convert.ToInt32(((JValue)(a[(int)TransactionIndex.Value])).Value);
#if DEBUG
                {
                    // NOTE: Transaction metadata skeleton in the closet.  Tossed around
                    // in early design and even implemented in iOS SDK client side.  It
                    // was never supported on platform nor publicly exposed to users.
                    JObject o = a[(int)TransactionIndex.Metadata] as JObject;
                    Debug.Assert(o.Count == 0);
                }
#endif
                Dictionary<string,string> metadata = new Dictionary<string,string>();
                long beginTime = ((DateTime)((JValue)(a[(int)TransactionIndex.BeginTime])).Value).ToUniversalTime().Ticks;  // ticks
                long endTime = ((DateTime)((JValue)(a[(int)TransactionIndex.EndTime])).Value).ToUniversalTime().Ticks;  // ticks
                double eyeTimeSeconds = Convert.ToDouble(((JValue)(a[(int)TransactionIndex.EyeTime])).Value);  // seconds (!!!)
                long eyeTime = (long)(eyeTimeSeconds*Transaction.TICKS_PER_SEC);  // ticks
                // Call Transaction constructor.
                transaction = new Transaction(
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
            return transaction;
        }
        public override bool CanConvert(Type objectType) {
            return objectType == typeof(Transaction);
        }
    }
}
