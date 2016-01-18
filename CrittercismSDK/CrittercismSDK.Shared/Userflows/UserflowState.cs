using System;
using System.Collections.Generic;
using System.Text;

namespace CrittercismSDK {
    ////////////////////////////////////////////////////////////////
    // * CREATED = Userflow created but not begun yet.
    // * BEBUN = Userflow begun via Begin .
    // * SLOW = 3 was an unfocussed concept that was never settled nor
    // implemented
    // * CANCELLED = 7 is an internal state (not sent to platform)
    // causing a not yet final state userflow to disappear as
    // if it never existed.
    // * ABORTED = 7 is defunct, dropped by iOS SDK 5.0.6 as part
    // of the SPRT-212 resolution, and has never been visible on platform.
    // * INTERRUPTED = 8 is being dropped by iOS SDK >5.4.5 since
    // being informed by D.S.: "I think it makes sense to eliminate
    // interrupted userflows.  The platform throws them away ..."
    // These were begun userflows with a given name interrupted
    // by starting new userflows with the same name.  The new
    // approach is to make the earlier userflows be CANCELLED .
    // * Only final state userflows are reported to platform.
    // Hence, given all the above, only ENDED, FAILED, TIMEOUT,
    // and CRASHED userflows are ever reported to platform.
    // SLOW isn't being used.
    ////////////////////////////////////////////////////////////////
    internal enum UserflowState {
        CREATED = 0,
        BEGUN,
        ENDED,
        SLOW,
        FAILED,
        TIMEOUT,
        CRASHED,
        CANCELLED
    }
}
