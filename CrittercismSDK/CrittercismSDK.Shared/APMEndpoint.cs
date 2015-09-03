using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;
using System.Text;

namespace CrittercismSDK
{
    internal class APMEndpoint
    {
        // (CRUnknownNetwork)
        const long ACTIVE_NETWORK=2;
        // System.Net.WebExceptionStatus
        const long ERROR_TABLE_CODE=5;

        private string method;
        private string url;
        private string timestamp;
        private long latency;
        private long activeNetwork;
        private long bytesRead;
        private long bytesSent;
        private long statusCode;
        private long errorTable;
        private long errorCode;
        public APMEndpoint(
            string method,
            Uri uri,
            long latency,      // milliseconds
            long bytesRead,
            long bytesSent,
            HttpStatusCode statusCode,
            WebExceptionStatus exceptionStatus
        ) {
            this.method=method;
            this.url=uri.AbsoluteUri;
            this.timestamp=DateUtils.ISO8601DateString(DateTime.UtcNow);
            this.latency=latency;
            this.activeNetwork=ACTIVE_NETWORK;
            this.bytesRead=bytesRead;
            this.bytesSent=bytesSent;
            this.statusCode=(long)statusCode;
            this.errorTable=ERROR_TABLE_CODE;
            this.errorCode=(long)exceptionStatus;
        }
        public Object[] ToArray() {
            Object[] answer=new Object[] {
                method,
                url,
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
    }
}
