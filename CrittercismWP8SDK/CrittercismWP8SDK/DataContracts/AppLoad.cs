// file:	DataContracts\AppLoad.cs
// summary:	Implements the application load class
namespace CrittercismSDK.DataContracts {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;

    /// <summary>
    /// Application load.
    /// </summary>
    [DataContract]
    public class AppLoad : MessageReport {
        /// <summary>
        /// Crittercism-issued Application identification string
        /// </summary>
        [DataMember]
        public string app_id { get; set; }

        /// <summary>
        /// User-specified state of the application as it's executing
        /// </summary>
        [DataMember]
        public Dictionary<string, object> app_state { get; set; }

        /// <summary>
        /// Execution platform on which the app runs
        /// </summary>
        [DataMember]
        public Platform platform { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public AppLoad() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public AppLoad(string _appId, string _appVersion) {
            if (!String.IsNullOrEmpty(_appId)) {
                app_id = _appId;

                // Initialize app state dictionary with base battery level and app version keys
                app_state = new Dictionary<string, object> { 
                    { "battery_level", Windows.Phone.Devices.Power.Battery.GetDefault().RemainingChargePercent.ToString() },
                    { "app_version", String.IsNullOrEmpty(_appVersion) ? "Unspecified" : _appVersion }
                };

                platform = new Platform();
            } else {
                throw new Exception("Crittercism requires an application_id to properly initialize itself.");
            }
        }
    }
}
