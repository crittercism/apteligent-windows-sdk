// file:	DataContracts\AppLoad.cs
// summary:	Implements the application load class
namespace CrittercismSDK.DataContracts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;

    /// <summary>
    /// Application load.
    /// </summary>
    [DataContract]
    public class AppLoad : MessageReport
    {
        /// <summary>
        /// Gets or sets the identifier of the application.
        /// </summary>
        /// <value> The identifier of the application. </value>
        [DataMember]
        public string app_id { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the device.
        /// </summary>
        /// <value> The identifier of the device. </value>
        [DataMember]
        public string did { get; set; }

        /// <summary>
        /// Gets or sets the library version.
        /// </summary>
        /// <value> The library version. </value>
        [DataMember]
        public string library_version { get; set; }

        /// <summary>
        /// Gets or sets the platform of the device.
        /// </summary>
        /// <value> The platform of the device. </value>
        [DataMember]
        public string platform { get; set; }

        /// <summary>
        /// Gets or sets the state of the application.
        /// </summary>
        /// <value> The application state. </value>
        [DataMember]
        public AppState app_state { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public AppLoad()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="appId">            Identifier for the application. </param>
        /// <param name="deviceId">         Identifier for the device. </param>
        /// <param name="libraryVersion">   The library version. </param>
        /// <param name="devicePlatform">   The device platform. </param>
        public AppLoad(string appId, string deviceId, string libraryVersion, string devicePlatform)
        {
            app_id = appId;
            did = deviceId;
            library_version = libraryVersion;
            platform = devicePlatform;
            app_state = new AppState();
        }
    }
}
