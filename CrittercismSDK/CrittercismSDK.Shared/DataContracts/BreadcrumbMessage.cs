using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
  
namespace CrittercismSDK
{
    /// <summary>
    /// Breadcrumb Message
    /// </summary>
    [DataContract]
    internal class BreadcrumbMessage
    {
        const int MaxBreadcrumbLength = 140;
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
        public BreadcrumbMessage(string messageString) {
            message=messageString;
            if (message.Length>MaxBreadcrumbLength) {
                message=message.Substring(0,MaxBreadcrumbLength);
            }
            timestamp=DateUtils.GMTDateString(DateTime.UtcNow);
        }
    }
}
