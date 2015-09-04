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
                return new Object[] {EndpointsArray(),appIdentifiersArray,deviceStateArray};
            }
            internal set {
            }
        }

        private APMEndpoint[] endpoints;
        private Object[] appIdentifiersArray;
        private Object[] deviceStateArray;

        private Object[] EndpointsArray() {
            List<Object[]> list=new List<Object[]>();
            foreach (APMEndpoint endpoint in endpoints) {
                list.Add(endpoint.ToArray());
            };
            return list.ToArray();
        }

        internal APMReport(APMEndpoint[] endpoints,Object[] appIdentifiersArray,Object[] deviceStateArray) {
            this.endpoints=endpoints;
            this.appIdentifiersArray=appIdentifiersArray;
            this.deviceStateArray=deviceStateArray;
        }
    }
}
