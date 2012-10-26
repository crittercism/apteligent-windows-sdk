// file:	DataContracts\Error.cs
// summary:	Implements the error class
namespace CrittercismSDK.DataContracts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;
    using Windows.UI.Xaml;

    /// <summary>
    /// Error.
    /// </summary>
    [DataContract]
    public class Error : MessageReport
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

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Error()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="appId">     Identifier for the application. </param>
        /// <param name="exception"> The exception. </param>
        public Error(string appId, ExceptionObject exception)
        {
            app_id = appId;
            app_state = new Dictionary<string, object>();
            app_state.Add("app_version", Application.Current.GetType().AssemblyQualifiedName.Split('=')[1].Split(',')[0]);
            app_state.Add("battery_level", "50"); // Because in WinRT is impossible to obtain that information
            error = exception;
            platform = new Platform();
        }
    }
}
