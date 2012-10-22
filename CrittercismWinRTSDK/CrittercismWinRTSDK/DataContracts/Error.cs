// file:	DataContracts\Error.cs
// summary:	Implements the error class
namespace CrittercismSDK.DataContracts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;

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
        public string app_id { get; set; }

        /// <summary>
        /// Gets or sets the application state.
        /// </summary>
        [DataMember]
        public AppState app_state { get; set; }

        /// <summary>
        /// Gets or sets the error.
        /// </summary>
        /// <value> The error. </value>
        [DataMember]
        public ExceptionObject error { get; set; }

        /// <summary>
        /// Gets or sets the platform
        /// </summary>
        [DataMember]
        public Platform platform { get; set; }

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
            app_state = new AppState();
            error = exception;
            platform = new Platform();
        }
    }
}
