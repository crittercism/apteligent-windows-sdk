using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;
using System.Text;

namespace CrittercismSDK
{
    class Endpoint {
        // (CRUnknownNetwork)
        const long ACTIVE_NETWORK=2;
        // System.Net.WebExceptionStatus
        const long ERROR_TABLE_CODE=5;

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

        internal Endpoint(
            string method,
            string uriString,
            long latency,      // milliseconds
            long bytesRead,
            long bytesSent,
            HttpStatusCode statusCode,
            WebExceptionStatus exceptionStatus
        ) {
            this.method=method;
            this.uriString=uriString;
            this.timestamp=DateUtils.ISO8601DateString(DateTime.UtcNow);
            this.latency=latency;
            this.activeNetwork=ACTIVE_NETWORK;
            this.bytesRead=bytesRead;
            this.bytesSent=bytesSent;
            this.statusCode=(long)statusCode;
            this.errorTable=ERROR_TABLE_CODE;
            this.errorCode=(long)exceptionStatus;
        }

        internal Object[] ToJsonArray() {
            Object[] answer=new Object[] {
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
    }
}
