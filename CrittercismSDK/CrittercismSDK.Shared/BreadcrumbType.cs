using System;
using System.Collections.Generic;
using System.Text;

namespace CrittercismSDK
{
    internal enum BreadcrumbType
    {
        Launch = 0,      // 0 - session launched      ; --
        Text,            // 1 - user breadcrumb       ; {text:,level:}
        Network,         // 2 - network breadcrumb    ; [verb,url,...,statusCode,errorCode]
        Event,           // 3 - app event             ; {event:}
        Reachability,    // 4 - network change        ; {change:,type:,oldType:,newType:}
        View,            // 5 - uiview change / load  ; {event:,viewName:}
        Error,           // 6 - handled exception     ; {name:,reason:}
        Crash,           // 7 - crash                 ; {name:,reason:}
        COUNT
    }
}
