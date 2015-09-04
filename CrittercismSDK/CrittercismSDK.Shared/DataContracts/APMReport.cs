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
                return new Object[] { appIdentifiersArray,deviceStateArray,EndpointsToJsonArray() };
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
        private APMEndpoint[] endpoints;

        private Object[] EndpointsToJsonArray() {
            // Serialize enddpoints into JSON .
            List<Object[]> list=new List<Object[]>();
            foreach (APMEndpoint endpoint in endpoints) {
                list.Add(endpoint.ToJsonArray());
            };
            return list.ToArray();
        }

        private APMEndpoint[] JsonArrayToEndpoints(Object[] json) {
            // Deserialize JSON into APMEndpoint[] endpoints.
            // json is an array of jsonArray's each representing an APMEndpoint .
            List<APMEndpoint> list=new List<APMEndpoint>();
            foreach (Object[] jsonArray in json) {
                list.Add(new APMEndpoint(jsonArray));
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
