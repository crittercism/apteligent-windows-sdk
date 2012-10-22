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
        /// Gets or sets the breadcrumbs.
        /// </summary>
        /// <value> The breadcrumbs. </value>
        [DataMember]
        public Breadcrumbs breadcrumbs { get; set; }

        /// <summary>
        /// Gets or sets the crash
        /// </summary>
        [DataMember]
        public ExceptionObject crash { get; set; }

        /// <summary>
        /// Gets or sets the platform.
        /// </summary>
        /// <value> The platform. </value>
        [DataMember]
        public Platform platform { get; set; }
        
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
        public Crash(string appId, Breadcrumbs currentBreadcrumbs, ExceptionObject exception)
        {
            app_id = appId;
            app_state = new AppState();
            breadcrumbs = currentBreadcrumbs;
            crash = exception;
            platform = new Platform();
        }
    }
}
