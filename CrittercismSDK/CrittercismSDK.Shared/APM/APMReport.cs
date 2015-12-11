using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace CrittercismSDK
{
    [DataContract]
    class APMReport : MessageReport
    {
        [DataMember]
        public Object[]  d {
            get {
                return new Object[] { appIdentifiersArray,deviceStateArray,endpoints };
            }
            internal set {
                Object[] dvalue=(Object[])value;
                appIdentifiersArray=(Object[])dvalue[0];
                deviceStateArray=(Object[])dvalue[1];
                endpoints=(List<Endpoint>)dvalue[2];
            }
        }

        private Object[] appIdentifiersArray;
        private Object[] deviceStateArray;
        private List<Endpoint> endpoints;

        internal APMReport(Object[] appIdentifiersArray,Object[] deviceStateArray,List<Endpoint> endpoints) {
            this.appIdentifiersArray=appIdentifiersArray;
            this.deviceStateArray=deviceStateArray;
            this.endpoints=endpoints;
        }
    }
}
