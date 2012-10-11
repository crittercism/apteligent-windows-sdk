// file:	DataContracts\Crash.cs
// summary:	Implements the crash class (Unhandled Exception)
namespace CrittercismSDK.DataContracts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;

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
        public string app_id { get; set; }

        /// <summary>
        /// Gets or sets the state of the application.
        /// </summary>
        /// <value> The application state. </value>
        [DataMember]
        public AppState app_state { get; set; }

        /// <summary>
        /// Gets or sets the platform.
        /// </summary>
        /// <value> The platform. </value>
        [DataMember]
        public string platform { get; set; }

        /// <summary>
        /// Gets or sets the breadcrumbs.
        /// </summary>
        /// <value> The breadcrumbs. </value>
        [DataMember]
        public Breadcrumbs breadcrumbs { get; set; }

        /// <summary>
        /// Gets or sets the identifier of device.
        /// </summary>
        /// <value> The identifier of device. </value>
        [DataMember]
        public string did { get; set; }

        /// <summary>
        /// Gets or sets the name of the exception.
        /// </summary>
        /// <value> The name of the exception. </value>
        [DataMember]
        public string exception_name { get; set; }

        /// <summary>
        /// Gets or sets the exception reason.
        /// </summary>
        /// <value> The exception reason. </value>
        [DataMember]
        public string exception_reason { get; set; }

        /// <summary>
        /// Gets or sets the library version.
        /// </summary>
        /// <value> The library version. </value>
        [DataMember]
        public string library_version { get; set; }

        /// <summary>
        /// Gets or sets the unsymbolized stacktrace.
        /// </summary>
        /// <value> The unsymbolized stacktrace. </value>
        [DataMember]
        public string unsymbolized_stacktrace { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Crash()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="appId">                Identifier for the application. </param>
        /// <param name="devicePlatform">       The device platform. </param>
        /// <param name="currentBreadcrumbs">   The current breadcrumbs. </param>
        /// <param name="deviceId">             Identifier for the device. </param>
        /// <param name="exceptionName">        Name of the exception. </param>
        /// <param name="exceptionReason">      The exception reason. </param>
        /// <param name="libraryVersion">       The library version. </param>
        /// <param name="stacktrace">           The stacktrace. </param>
        public Crash(string appId, string devicePlatform, Breadcrumbs currentBreadcrumbs, string deviceId, string exceptionName, string exceptionReason, string libraryVersion, string stacktrace)
        {
            app_id = appId;
            app_state = new AppState();
            platform = devicePlatform;
            breadcrumbs = currentBreadcrumbs;
            did = deviceId;
            exception_name = exceptionName;
            exception_reason = exceptionReason;
            library_version = libraryVersion;
            unsymbolized_stacktrace = stacktrace;
        }
    }
}
