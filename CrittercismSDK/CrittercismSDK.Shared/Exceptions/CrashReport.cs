using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// <summary>
    /// Crash (Unhandled Exception).
    /// </summary>
    [DataContract]
    internal class CrashReport : MessageReport
    {
        /// <summary>
        /// Gets or sets the identifier of the application.
        /// </summary>
        /// <value> The identifier of the application. </value>
        [DataMember]
        public string app_id { get; internal set; }

        /// <summary>
        /// Gets or sets the state of the application.
        /// </summary>
        /// <value> The application state. </value>
        [DataMember]
        public Dictionary<string, object> app_state { get; internal set; }

        [DataMember]
        public UserBreadcrumbs breadcrumbs { get; internal set; }

        [DataMember]
        public List<Endpoint> endpoints { get; internal set; }

        [DataMember]
        public List<Breadcrumb> systemBreadcrumbs { get; internal set; }

        [DataMember]
        public List<Transaction> transactions { get; internal set; }

        [DataMember]
        public Dictionary<string, string> metadata { get; internal set; }

        /// <summary>
        /// Gets or sets the crash
        /// </summary>
        [DataMember]
        public ExceptionObject crash { get; internal set; }

        /// <summary>
        /// Gets or sets the platform.
        /// </summary>
        /// <value> The platform. </value>
        [DataMember]
        public Platform platform { get; internal set; }
        
        /// <summary>
        /// Default constructor.
        /// </summary>
        public CrashReport()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="appId">Identifier for the application.</param>
        /// <param name="currentBreadcrumbs">The current breadcrumbs.</param>
        /// <param name="exception">The exception.</param>
        public CrashReport(
            string appId,
            Dictionary<string,string> metadata,
            UserBreadcrumbs breadcrumbs,
            List<Endpoint> endpoints,
            List<Breadcrumb> systemBreadcrumbs,
            List<Transaction> transactions,
            ExceptionObject exception)
        {
            app_id = appId;
            app_state = ComputeAppState();
            this.metadata = metadata;
            this.breadcrumbs = breadcrumbs;
            this.endpoints = endpoints;
            this.systemBreadcrumbs = systemBreadcrumbs;
            this.transactions = transactions;
            crash = exception;
            platform = new Platform();
        }
    }
}
