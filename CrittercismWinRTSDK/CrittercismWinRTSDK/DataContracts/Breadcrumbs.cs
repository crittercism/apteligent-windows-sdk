// file:	DataContracts\Breadcrumbs.cs
// summary:	Implements the breadcrumbs class
namespace CrittercismSDK.DataContracts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;

    /// <summary>
    /// Breadcrumbs.
    /// </summary>
    [DataContract]
    public class Breadcrumbs
    {
        /// <summary>
        /// Gets or sets the breadcrumbs of the current session.
        /// </summary>
        /// <value> The breadcrumbs of the current session. </value>
        [DataMember]
        public List<BreadcrumbMessage> current_session { get; internal set; }

        /// <summary>
        /// Gets or sets the breadcrumbs of the previous session.
        /// </summary>
        /// <value> The breadcrumbs of the previous session. </value>
        [DataMember]
        public List<BreadcrumbMessage> previous_session { get; internal set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Breadcrumbs()
        {
            current_session = new List<BreadcrumbMessage>();
            previous_session = new List<BreadcrumbMessage>();
        }

        /// <summary>
        /// Saves to disk.
        /// </summary>
        /// <returns>   true if it succeeds, false if it fails. </returns>
        internal bool SaveToDisk()
        {
            try
            {
                return StorageHelper.SaveToDisk(this, Crittercism.dataFolder, this.GetType().Name + ".txt");
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the breadcrumbs.
        /// </summary>
        /// <returns>   The breadcrumbs. </returns>
        internal static Breadcrumbs GetBreadcrumbs()
        {
            Breadcrumbs actualBreadcrumbs = new Breadcrumbs();
            try
            {
                Breadcrumbs breadcrumbs = StorageHelper.LoadFromDisk(typeof(Breadcrumbs), Crittercism.dataFolder, typeof(Breadcrumbs).Name + ".txt") as Breadcrumbs;
                if (breadcrumbs != null)
                {
                    actualBreadcrumbs.previous_session = new List<BreadcrumbMessage>(breadcrumbs.current_session);
                }

                return actualBreadcrumbs;
            }
            catch
            {
                return actualBreadcrumbs;
            }
        }
    }
}
