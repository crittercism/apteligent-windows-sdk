using System;
using System.Collections.Generic;
using System.Text;

namespace CrittercismSDK
{
    internal interface IMockNetwork
    {
        bool SendRequest(MessageReport messageReport);
    }
}
