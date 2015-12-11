using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;
using System.Text;

namespace CrittercismSDK
{
    [JsonConverter(typeof(EndpointConverter))]
    internal class Endpoint {
        // (CRUnknownNetwork)
        internal const long ACTIVE_NETWORK=2;
        // System.Net.WebExceptionStatus
        internal const long ERROR_TABLE_CODE=5;

        private string method;
        private string uriString;
        private string timestamp;
        private long latency;
        private long activeNetwork;
        private long bytesRead;
        private long bytesSent;
        private long statusCode;
        private long errorTable;
        private long errorCode;

        #region Constructors
        internal Endpoint(
            string method,
            string uriString,
            string timestamp,
            long latency,      // milliseconds
            long bytesRead,
            long bytesSent,
            HttpStatusCode statusCode,
            WebExceptionStatus exceptionStatus
        ) {
            this.method=method;
            this.uriString=uriString;
            this.timestamp=timestamp;
            this.latency=latency;
            this.activeNetwork=ACTIVE_NETWORK;
            this.bytesRead=bytesRead;
            this.bytesSent=bytesSent;
            this.statusCode=(long)statusCode;
            this.errorTable=ERROR_TABLE_CODE;
            this.errorCode=(long)exceptionStatus;
        }
        #endregion

        #region JSON
        internal JArray ToJArray() {
            List<JToken> list = new List<JToken>();
            list.Add(method);
            list.Add(uriString);
            list.Add(timestamp);
            list.Add(latency);
            list.Add(activeNetwork);
            list.Add(bytesRead);
            list.Add(bytesSent);
            list.Add(statusCode);
            list.Add(errorTable);
            list.Add(errorCode);
            JArray answer = new JArray(list);
            return answer;
        }
        public override string ToString() {
            return JsonConvert.SerializeObject(this);
        }
#endregion
    }
}
