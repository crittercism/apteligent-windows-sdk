using Newtonsoft.Json;
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
        internal Endpoint(Object[] jsonArray) {
            this.method=(string)jsonArray[0];
            this.uriString=(string)jsonArray[1];
            this.timestamp=(string)jsonArray[2];
            this.latency=(long)jsonArray[3];
            this.activeNetwork=(long)jsonArray[4];
            this.bytesRead=(long)jsonArray[5];
            this.bytesSent=(long)jsonArray[6];
            this.statusCode=(long)jsonArray[7];
            this.errorTable=(long)jsonArray[8];
            this.errorCode=(long)jsonArray[9];
        }
        #endregion

        #region JSON
        internal Object[] ToArray() {
            Object[] answer = new Object[] {
                method,
                uriString,
                timestamp,
                latency,
                activeNetwork,
                bytesRead,
                bytesSent,
                statusCode,
                errorTable,
                errorCode
            };
            return answer;
        }
        public override string ToString() {
            return JsonConvert.SerializeObject(this);
        }
        #endregion
    }
}
