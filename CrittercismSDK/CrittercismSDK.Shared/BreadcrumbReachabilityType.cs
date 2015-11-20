using System;
using System.Collections.Generic;
using System.Text;

namespace CrittercismSDK
{
    internal enum BreadcrumbReachabilityType
    {
        Up = 0,   // 0 - internet connection up
        Down = 1, // 1 - internet connection down
        Gained = 2,   // 2 - connectivity type gained
        Lost = 3,     // 3 - connectivity type lost
        Switch = 4,   // 4 - switch from one connection type to another
        COUNT
    }
}
