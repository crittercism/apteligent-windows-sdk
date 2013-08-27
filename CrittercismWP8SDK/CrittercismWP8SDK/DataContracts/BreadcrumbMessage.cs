// file:	DataContracts\BreadcrumbMessage.cs
// summary:	Implements the breadcrumb message class
namespace CrittercismSDK.DataContracts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
  
    /// <summary>
    /// Breadcrumb Message
    /// </summary>
    [DataContract]
    public class BreadcrumbMessage
    {
        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value> The message. </value>
        [DataMember]
        public string message { get; internal set; }

        /// <summary>
        /// Gets or sets the timestamp.
        /// </summary>
        /// <value> The timestamp. </value>
        [DataMember]
        public string timestamp { get; internal set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public BreadcrumbMessage()
        {
            message = string.Empty;
            timestamp = string.Empty;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="messageString"> The message </param>
        public BreadcrumbMessage(string messageString)
        {
            message = messageString;
            timestamp = MessageReport.DateTimeString(DateTime.Now);
        }
    }
}