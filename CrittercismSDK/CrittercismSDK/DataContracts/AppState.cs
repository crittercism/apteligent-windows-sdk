// file:	DataContracts\AppState.cs
// summary:	Implements the application state class
namespace CrittercismSDK.DataContracts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    
    /// <summary> 
    /// Application state.
    /// </summary>
    [DataContract]
    public class AppState
    {
        /// <summary>
        /// Gets or sets the battery level.
        /// </summary>
        /// <value> The battery level. </value>
        [DataMember]
        public string battery_level { get; set; }

        /// <summary>
        /// Gets or sets the application version.
        /// </summary>
        /// <value> The application version. </value>
        [DataMember]
        public string app_version { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public AppState()
        {
            ////battery_level = ();
            ////app_version = ();
        }
    }
}
