// file:	DataContracts\Platform.cs
// summary:	Implements the application state class

namespace CrittercismSDK.DataContracts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using Windows.UI.Xaml;

    /// <summary>
    /// Platform
    /// </summary>
    [DataContract]
    public class Platform
    {
        /// <summary>
        /// Gets or sets the client version.
        /// </summary>
        /// <value> The client version. </value>
        [DataMember]
        public string client { get; internal set; }

        /// <summary>
        /// Gets or sets the device id.
        /// </summary>
        /// <value> The device id. </value>
        [DataMember]
        public string device_id { get; internal set; }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value> The model. </value>
        [DataMember]
        public string device_model { get; internal set; }

        /// <summary>
        /// Gets or sets the OS name.
        /// </summary>
        /// <value> The OS name. </value>
        [DataMember]
        public string os_name { get; internal set; }

        /// <summary>
        /// Gets or sets the OS version.
        /// </summary>
        /// <value> The OS version. </value>
        [DataMember]
        public string os_version { get; internal set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Platform()
        {
            client = "winRTv1.0";
            device_id = Crittercism.DeviceId;
            device_model = "Nokia Lumia 800";
            os_name = "Windows Phone";
            os_version = "8.0";
        }
    }
}