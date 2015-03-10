// file:    DataContracts\AppLoad.cs
// summary:    Implements the application load class
namespace CrittercismSDK.DataContracts
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Windows;
   
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
        /// Constructor.
        /// </summary>
        public AppLoad() {
            app_id=Crittercism.AppID;
            app_state=ComputeAppState();
            platform=new Platform();
        }
    }
}
