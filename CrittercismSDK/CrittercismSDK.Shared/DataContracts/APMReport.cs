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
                return new Object[] { appIdentifiersArray,deviceStateArray,EndpointsArray()};
            }
            internal set {
            }
        }

        private Object[] appIdentifiersArray;
        private Object[] deviceStateArray;
        private APMEndpoint[] endpoints;

        private Object[] EndpointsArray() {
            List<Object[]> list=new List<Object[]>();
            foreach (APMEndpoint endpoint in endpoints) {
                list.Add(endpoint.ToArray());
            };
            return list.ToArray();
        }

        internal APMReport(Object[] appIdentifiersArray,Object[] deviceStateArray,APMEndpoint[] endpoints) {
            this.appIdentifiersArray=appIdentifiersArray;
            this.deviceStateArray=deviceStateArray;
            this.endpoints=endpoints;
        }
    }
}
