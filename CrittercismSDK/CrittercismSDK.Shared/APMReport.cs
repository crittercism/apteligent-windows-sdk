using System;
using System.Collections.Generic;
using System.Text;

namespace CrittercismSDK
{
    class APMReport
    {
        private APMEndpoint[] endpoints;
        private Object[] appIdentifiersArray;
        private Object[] deviceStateArray;
        internal APMReport(APMEndpoint[] endpoints,Object[] appIdentifiersArray,Object[] deviceStateArray) {
            this.endpoints=endpoints;
            this.appIdentifiersArray=appIdentifiersArray;
            this.deviceStateArray=deviceStateArray;
        }
    }
}
