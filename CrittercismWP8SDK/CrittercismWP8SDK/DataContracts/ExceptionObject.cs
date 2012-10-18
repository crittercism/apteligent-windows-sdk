// file:	DataContracts\ExceptionObject.cs
// summary:	Implements the exception object class
namespace CrittercismSDK.DataContracts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;

    /// <summary>
    /// Exception object.
    /// </summary>
    [DataContract]
    public class ExceptionObject
    {
        /// <summary>
        /// Gets or sets the library version.
        /// </summary>
        /// <value> The library version. </value>
        [DataMember]
        public string library_version { get; set; }

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
        /// Gets or sets the state.
        /// </summary>
        /// <value> The state. </value>
        [DataMember]
        public AppState state { get; set; }

        /// <summary>
        /// Gets or sets the unsymbolized stacktrace.
        /// </summary>
        /// <value> The unsymbolized stacktrace. </value>
        [DataMember]
        public string unsymbolized_stacktrace { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ExceptionObject()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="libraryVersion">   The library version. </param>
        /// <param name="exceptionName">    Name of the exception. </param>
        /// <param name="exceptionReason">  The exception reason. </param>
        /// <param name="currentState">     State of the current. </param>
        /// <param name="stacktrace">       The stacktrace. </param>
        public ExceptionObject(string libraryVersion, string exceptionName, string exceptionReason, AppState currentState, string stacktrace)
        {
            library_version = libraryVersion;
            exception_name = exceptionName;
            exception_reason = exceptionReason;
            state = currentState;
            unsymbolized_stacktrace = stacktrace;
        }
    }
}
