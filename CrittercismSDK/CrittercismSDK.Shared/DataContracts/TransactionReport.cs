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
        public List<Transaction> transactions { get; internal set; }
        [DataMember]
        public List<Breadcrumb> breadcrumbs { get; internal set; }
        [DataMember]
        public List<Breadcrumb> systemBreadcrumbs { get; internal set; }
        [DataMember]
        public List<Endpoint> endpoints { get; internal set; }
        private TransactionReport()
        {
        }
        public TransactionReport(
            Dictionary<string,object> appState,
            List<Transaction> transactions,
            List<Breadcrumb> breadcrumbs,
            List<Breadcrumb> systemBreadcrumbs,
            List<Endpoint> endpoints)
        {
            this.appState = appState;
            this.transactions = transactions;
            this.breadcrumbs = breadcrumbs;
            this.systemBreadcrumbs = systemBreadcrumbs;
            this.endpoints = endpoints;
        }
    }
}
