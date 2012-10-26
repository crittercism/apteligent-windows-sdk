// file:	DataContracts\Crash.cs
// summary:	Implements the crash class (Unhandled Exception)
namespace CrittercismSDK.DataContracts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;
    using Windows.UI.Xaml;

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
            app_state = new Dictionary<string, object>();
            app_state.Add("app_version", Application.Current.GetType().AssemblyQualifiedName.Split('=')[1].Split(',')[0]);
            app_state.Add("battery_level", "50"); // Because in WinRT is impossible to obtain that information
            breadcrumbs = currentBreadcrumbs;
            crash = exception;
            platform = new Platform();
        }
    }
}
