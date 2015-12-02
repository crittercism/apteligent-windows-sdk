using System;
using System.Collections.Generic;
using System.Text;

namespace CrittercismSDK {
    internal enum EndpointIndex {
        Method = 0,
        UriString,
        Timestamp,
        Latency,
        ActiveNetwork,
        BytesRead,
        BytesSent,
        StatusCode,
        ErrorTable,
        ErrorCode,
        COUNT
    }
}
