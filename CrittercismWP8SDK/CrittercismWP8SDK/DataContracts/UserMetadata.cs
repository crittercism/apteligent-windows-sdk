namespace CrittercismSDK.DataContracts
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    
    // FIXME jbley note that this class will not be serialized in the standard way.

    /// <summary>
    /// Application load.
    /// </summary>
    [DataContract]
    internal class UserMetadata : MessageReport
    {
        /// <summary>
        /// Crittercism-issued Application identification string
        /// </summary>
        [DataMember]
        public string app_id { get; internal set; }

        [DataMember]
        public Dictionary<string, string> metadata { get; internal set; }

        /// <summary>
        /// Execution platform on which the app runs
        /// </summary>
        [DataMember]
        public Platform platform { get; internal set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public UserMetadata() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public UserMetadata(string _appId, string _appVersion, Dictionary<string,string> userMetadata)
        {
            if (!String.IsNullOrEmpty(_appId))
            {
                app_id = _appId;

                metadata = userMetadata;

                platform = new Platform();
            }
            else
            {
                throw new Exception("Crittercism requires an application_id to properly initialize itself.");
            }
        }
    }
}
