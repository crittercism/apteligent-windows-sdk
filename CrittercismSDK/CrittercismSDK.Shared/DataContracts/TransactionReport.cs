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

namespace CrittercismSDK
{
    [DataContract]
    internal class TransactionReport : MessageReport
    {
        [DataMember]
        public Dictionary<string, object> appState { get; internal set; }
        [DataMember]
        public Object[] transactions { get; internal set; }
        [DataMember]
        public Breadcrumbs breadcrumbs { get; internal set; }
        [DataMember]
        public Object[] systemBreadcrumbs { get; internal set; }
        [DataMember]
        public Object[] endpoints { get; internal set; }
        private TransactionReport()
        {
        }
        public TransactionReport(
            Dictionary<string,object> appState,
            Object[] transactions,
            Breadcrumbs breadcrumbs,
            Object[] systemBreadcrumbs,
            Object[] endpoints)
        {
            this.appState = appState;
            this.transactions = transactions;
            this.breadcrumbs = breadcrumbs;
            this.systemBreadcrumbs = systemBreadcrumbs;
            this.endpoints = endpoints;
        }
    }
}
