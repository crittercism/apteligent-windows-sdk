using System;
using System.Collections.Generic;
using System.Text;

namespace CrittercismSDK
{
    ////////////////////////////////////////////////////////////////
    // * ABORTED is defunct, dropped by iOS SDK 5.0.6 as part of the
    // SPRT-212 resolution, and has never been visible on platform.
    // * SLOW was an unfocussed concept that was never settled nor
    // implemented
    // * INTERRUPTED means a begun transaction with given name was
    // interrupted by beginning another transaction with the same name
    // "Static API" design cannot support concurrent transactions with
    // the same name.  The price is to invent this "Interrupt" concept.
    // We understand platform converts INTERRUPTED to FAILED .  
    // * Only final state transactions are reported to platform.
    // * Hence, given all the above, only ENDED, FAILED, TIMEOUT,
    // INTERRUPTED, and CRASHED transactions are ever reported to
    // platform as of now.  Platform converts INTERRUPTED to FAILED.
    // SLOW and ABORTED aren't being used.
    ////////////////////////////////////////////////////////////////
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
        INTERRUPTED,
        CANCELLED
    }
}
