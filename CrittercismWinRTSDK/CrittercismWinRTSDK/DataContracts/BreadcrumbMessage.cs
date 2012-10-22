// file:	DataContracts\BreadcrumbMessage.cs
// summary:	Implements the breadcrumb message class
namespace CrittercismSDK.DataContracts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using Windows.UI.Xaml;

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
        public string message { get; set; }

        /// <summary>
        /// Gets or sets the timestamp.
        /// </summary>
        /// <value> The timestamp. </value>
        [DataMember]
        public string timestamp { get; set; }

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
            timestamp = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ssK", System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}