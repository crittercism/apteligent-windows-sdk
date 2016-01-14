using System;
using System.Collections.Generic;
using System.Text;

namespace CrittercismSDK {
    ////////////////////////////////////////////////////////////////
    // * CREATED = UserFlow created but not begun yet.
    // * BEBUN = UserFlow begun via Begin .
    // * SLOW = 3 was an unfocussed concept that was never settled nor
    // implemented
    // * CANCELLED = 7 is an internal state (not sent to platform)
    // causing a not yet final state userFlow to disappear as
    // if it never existed.
    // * ABORTED = 7 is defunct, dropped by iOS SDK 5.0.6 as part
    // of the SPRT-212 resolution, and has never been visible on platform.
    // * INTERRUPTED = 8 is being dropped by iOS SDK >5.4.5 since
    // being informed by D.S.: "I think it makes sense to eliminate
    // interrupted userFlows.  The platform throws them away ..."
    // These were begun userFlows with a given name interrupted
    // by starting new userFlows with the same name.  The new
    // approach is to make the earlier userFlows be CANCELLED .
    // * Only final state userFlows are reported to platform.
    // Hence, given all the above, only ENDED, FAILED, TIMEOUT,
    // and CRASHED userFlows are ever reported to platform.
    // SLOW isn't being used.
    ////////////////////////////////////////////////////////////////
    internal enum UserFlowState {
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
