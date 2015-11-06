using System;
using System.Collections.Generic;
using System.Text;

namespace CrittercismSDK
{
    internal enum TransactionState
    {
        CREATED = 0,
        BEGUN,
        ENDED,
        SLOW,
        FAILED,
        TIMEOUT,
        CRASHED,
        ABORTED,
        INTERRUPTED
    }
}
