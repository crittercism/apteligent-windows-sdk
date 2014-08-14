// file:    DataContracts\AppLoad.cs
// summary:    Implements the application load class
namespace CrittercismSDK.DataContracts
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    
    /// <summary>
    /// Application load.
    /// </summary>
    [DataContract]
    internal class AppLoad : MessageReport
    {
        /// <summary>
        /// Crittercism-issued Application identification string
        /// </summary>
        [DataMember]
        public string app_id { get; internal set; }

        /// <summary>
        /// User-specified state of the application as it's executing
        /// </summary>
        [DataMember]
        public Dictionary<string, object> app_state { get; internal set; }

        /// <summary>
        /// Execution platform on which the app runs
        /// </summary>
        [DataMember]
        public Platform platform { get; internal set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public AppLoad() { }

        public AppLoad(string _appId)
            : this(_appId, System.Windows.Application.Current.GetType().Assembly.GetName().Version.ToString())
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public AppLoad(string _appId, string _appVersion)
        {
            if (!String.IsNullOrEmpty(_appId))
            {
                app_id = _appId;

                app_state = ComputeAppState(_appVersion);

                platform = new Platform();
            }
            else
            {
                throw new Exception("Crittercism requires an application_id to properly initialize itself.");
            }
        }
    }
}
