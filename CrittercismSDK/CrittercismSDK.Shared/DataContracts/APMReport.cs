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
                return new Object[] { appIdentifiersArray,deviceStateArray,EndpointsToArray() };
            }
            internal set {
                Object[] dvalue=(Object[])value;
                appIdentifiersArray=(Object[])dvalue[0];
                deviceStateArray=(Object[])dvalue[1];
                endpoints=JsonArrayToEndpoints((Object[])dvalue[2]);
            }
        }

        private Object[] appIdentifiersArray;
        private Object[] deviceStateArray;
        private Endpoint[] endpoints;

        private Object[] EndpointsToArray() {
            // Serialize enddpoints into JSON .
            List<Object[]> list=new List<Object[]>();
            foreach (Endpoint endpoint in endpoints) {
                list.Add(endpoint.ToArray());
            };
            return list.ToArray();
        }

        private Endpoint[] JsonArrayToEndpoints(Object[] json) {
            // Deserialize JSON into Endpoint[] endpoints.
            // json is an array of jsonArray's each representing an Endpoint .
            List<Endpoint> list=new List<Endpoint>();
            foreach (Object[] jsonArray in json) {
                list.Add(new Endpoint(jsonArray));
            };
            return list.ToArray();
        }

        internal APMReport(Object[] appIdentifiersArray,Object[] deviceStateArray,Endpoint[] endpoints) {
            this.appIdentifiersArray=appIdentifiersArray;
            this.deviceStateArray=deviceStateArray;
            this.endpoints=endpoints;
        }
    }
}
