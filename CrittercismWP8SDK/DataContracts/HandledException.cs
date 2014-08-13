namespace CrittercismSDK.DataContracts
{
    using Microsoft.Phone.Net.NetworkInformation;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;
    using Windows.Devices.Sensors;
    using Windows.Graphics.Display;

    [DataContract]
    internal class HandledException : MessageReport
    {
        /// <summary>
        /// Gets or sets the identifier of the application.
        /// </summary>
        /// <value> The identifier of the application. </value>
        [DataMember]
        public string app_id { get; internal set; }

        /// <summary>
        /// Gets or sets the application state.
        /// </summary>
        [DataMember]
        public Dictionary<string, object> app_state { get; internal set; }

        /// <summary>
        /// Gets or sets the error.
        /// </summary>
        /// <value> The error. </value>
        [DataMember]
        public ExceptionObject error { get; internal set; }

        /// <summary>
        /// Gets or sets the platform
        /// </summary>
        [DataMember]
        public Platform platform { get; internal set; }

        [DataMember]
        public Breadcrumbs breadcrumbs { get; internal set; }

        [DataMember]
        public Dictionary<string,string> metadata { get; internal set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public HandledException()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="appId">     Identifier for the application. </param>
        /// <param name="exception"> The exception. </param>
        public HandledException(string appId, string appVersion, Dictionary<string,string> currentMetadata, Breadcrumbs currentBreadcrumbs, ExceptionObject exception)
        {
            app_id = appId;
            app_state = ComputeAppState(appVersion);

            error = exception;
            metadata = currentMetadata;
            breadcrumbs = currentBreadcrumbs;
            platform = new Platform();
        }
    }
}
