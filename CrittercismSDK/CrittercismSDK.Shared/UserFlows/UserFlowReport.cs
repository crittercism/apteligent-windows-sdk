using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
#if WINDOWS_PHONE
using Windows.Devices.Sensors;
using Windows.Graphics.Display;
using Microsoft.Phone.Net.NetworkInformation;
#endif

namespace CrittercismSDK {
    [DataContract]
    internal class UserFlowReport : MessageReport {
        [DataMember]
        public Dictionary<string,object> appState { get; internal set; }
        [DataMember]
        public List<UserFlow> userFlows { get; internal set; }
        [DataMember]
        public List<UserBreadcrumb> breadcrumbs { get; internal set; }
        [DataMember]
        public List<Breadcrumb> systemBreadcrumbs { get; internal set; }
        [DataMember]
        public List<Endpoint> endpoints { get; internal set; }
        private UserFlowReport() {
        }
        public UserFlowReport(
            Dictionary<string,object> appState,
            List<UserFlow> userFlows,
            List<UserBreadcrumb> breadcrumbs,
            List<Breadcrumb> systemBreadcrumbs,
            List<Endpoint> endpoints) {
            this.appState = appState;
            this.userFlows = userFlows;
            this.breadcrumbs = breadcrumbs;
            this.systemBreadcrumbs = systemBreadcrumbs;
            this.endpoints = endpoints;
        }
    }
}
