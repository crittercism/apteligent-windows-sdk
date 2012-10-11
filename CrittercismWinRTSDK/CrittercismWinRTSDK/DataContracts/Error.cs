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
        /// Gets or sets the platform of the device.
        /// </summary>
        /// <value> The platform of the device. </value>
        [DataMember]
        public string platform { get; set; }

        /// <summary>
        /// Gets or sets the hashed device identifier.
        /// </summary>
        /// <value> The hashed device identifier. </value>
        [DataMember]
        public string hashed_device_id { get; set; }

        /// <summary>
        /// Gets or sets the library version.
        /// </summary>
        /// <value> The library version. </value>
        [DataMember]
        public string library_version { get; set; }

        /// <summary>
        /// Gets or sets the exceptions.
        /// </summary>
        /// <value> The exceptions. </value>
        [DataMember]
        public List<ExceptionObject> exceptions { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Error()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="appId">            Identifier for the application. </param>
        /// <param name="devicePlatform">   The device platform. </param>
        /// <param name="hashedDeviceId">   Identifier for the hashed device. </param>
        /// <param name="libraryVersion">   The library version. </param>
        /// <param name="exceptionList">    List of exceptions. </param>
        public Error(string appId, string devicePlatform, string hashedDeviceId, string libraryVersion, List<ExceptionObject> exceptionList)
        {
            app_id = appId;
            platform = devicePlatform;
            hashed_device_id = hashedDeviceId;
            library_version = libraryVersion;
            exceptions = exceptionList;
        }
    }
}
