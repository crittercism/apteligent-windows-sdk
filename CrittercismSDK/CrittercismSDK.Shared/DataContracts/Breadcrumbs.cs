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
    internal class Breadcrumbs
    {
        /// <summary>
        /// Gets or sets the breadcrumbs of the current session.
        /// </summary>
        /// <value> The breadcrumbs of the current session. </value>
        [DataMember]
        public List<BreadcrumbMessage> current_session { get; private set; }

        /// <summary>
        /// Gets or sets the breadcrumbs of the previous session.
        /// </summary>
        /// <value> The breadcrumbs of the previous session. </value>
        [DataMember]
        public List<BreadcrumbMessage> previous_session { get; private set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Breadcrumbs()
        {
            current_session = new List<BreadcrumbMessage>();
            previous_session = new List<BreadcrumbMessage>();
        }

        public Breadcrumbs Copy() {
            Breadcrumbs answer=new Breadcrumbs();
            answer.current_session=new List<BreadcrumbMessage>(current_session);
            answer.previous_session=new List<BreadcrumbMessage>(previous_session);
            return answer;
        }

        public void Clear() {
            lock (this) {
                previous_session=current_session;
                current_session=new List<BreadcrumbMessage>();
            }
        }

        /// <summary>
        /// Saves to disk.
        /// </summary>
        /// <returns>   true if it succeeds, false if it fails. </returns>
        internal bool Save() {
            try {
                return StorageHelper.Save(this);
            } catch (Exception e) {
                Crittercism.LogInternalException(e);
                return false;
            }
        }

        /// <summary>
        /// Gets the breadcrumbs.
        /// </summary>
        /// <returns>   The breadcrumbs. </returns>
        internal static Breadcrumbs GetBreadcrumbs() {
            Breadcrumbs actualBreadcrumbs=new Breadcrumbs();
            try {
                const string path="Breadcrumbs.js";
                Breadcrumbs breadcrumbs=null;
                if (StorageHelper.FileExists(path)) {
                    breadcrumbs=StorageHelper.Load(path,typeof(Breadcrumbs)) as Breadcrumbs;
                }
                if (breadcrumbs!=null) {
                    actualBreadcrumbs.previous_session=new List<BreadcrumbMessage>(breadcrumbs.current_session);
                }
            } catch (Exception e) {
                Crittercism.LogInternalException(e);
            };
            return actualBreadcrumbs;
        }
    }
}
