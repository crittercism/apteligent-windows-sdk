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
    internal class ExceptionObject
    {
        /// <summary>
        /// Gets or sets the name of the exception.
        /// </summary>
        /// <value> The name of the exception. </value>
        [DataMember]
        public string name { get; internal set; }

        /// <summary>
        /// Gets or sets the exception reason.
        /// </summary>
        /// <value> The exception reason. </value>
        [DataMember]
        public string reason { get; internal set; }

        /// <summary>
        /// Gets or sets the unsymbolized stacktrace.
        /// </summary>
        /// <value> The unsymbolized stacktrace. </value>
        [DataMember]
        public List<string> stack_trace { get; internal set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ExceptionObject()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="exceptionName">    Name of the exception. </param>
        /// <param name="exceptionReason">  The exception reason. </param>
        /// <param name="stacktrace">       The stacktrace. </param>
        public ExceptionObject(string exceptionName, string exceptionReason, string stacktrace)
        {
            name = exceptionName;
            reason = exceptionReason;
            stack_trace = stacktrace.Split(new string[] {"\r\n"},  StringSplitOptions.RemoveEmptyEntries).ToList();
        }
    }
}
