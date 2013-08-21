﻿// file:	DataContracts\Crash.cs
// summary:	Implements the crash class (Unhandled Exception)
namespace CrittercismSDK.DataContracts
{
    using Microsoft.Phone.Net.NetworkInformation;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO.IsolatedStorage;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;
    using Windows.Devices.Sensors;
    using Windows.Graphics.Display;

    /// <summary>
    /// Crash (Unhandled Exception).
    /// </summary>
    [DataContract]
    public class Crash : MessageReport
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

        /// <summary>
        /// Gets or sets the breadcrumbs.
        /// </summary>
        /// <value> The breadcrumbs. </value>
        [DataMember]
        public Breadcrumbs breadcrumbs { get; internal set; }

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
        public Crash()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="appId">Identifier for the application.</param>
        /// <param name="appVersion">The app version.</param>
        /// <param name="currentBreadcrumbs">The current breadcrumbs.</param>
        /// <param name="exception">The exception.</param>
        public Crash(string appId, string appVersion, Dictionary<string,string> currentMetadata, Breadcrumbs currentBreadcrumbs, ExceptionObject exception)
        {
            app_id = appId;
            app_state = ComputeAppState(appVersion);
            metadata = currentMetadata;
            breadcrumbs = currentBreadcrumbs;
            crash = exception;
            platform = new Platform();
        }
    }
}
