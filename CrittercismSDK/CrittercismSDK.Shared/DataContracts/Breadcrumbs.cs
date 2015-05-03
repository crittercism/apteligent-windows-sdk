// file:	DataContracts\Breadcrumbs.cs
// summary:	Implements the breadcrumbs class
namespace CrittercismSDK
{
    using System;
    using System.Collections.Generic;
    using System.IO;
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
        /// Gets or sets the breadcrumbs of the crashed session.
        /// </summary>
        /// <value> The breadcrumbs of the crashed session. </value>
        [DataMember]
        public List<BreadcrumbMessage> previous_session { get; private set; }

        /// <summary>
        /// Gets or sets the breadcrumbs of the current session.
        /// </summary>
        /// <value> The breadcrumbs of the current session. </value>
        [DataMember]
        public List<BreadcrumbMessage> current_session { get; private set; }

        private bool Saved { get; set; }

        // 1 "session_start" breadcrumb + 100 user breadcrumbs cap
        private const int MaxBreadcrumbCount=101;

        /// <summary>
        /// Default constructor.
        /// </summary>
        private Breadcrumbs() {
            previous_session=new List<BreadcrumbMessage>();
            current_session=new List<BreadcrumbMessage>();
            Saved=false;
        }

        internal Breadcrumbs Copy() {
            Breadcrumbs answer=new Breadcrumbs();
            lock (this) {
                answer.current_session=new List<BreadcrumbMessage>(current_session);
                answer.previous_session=new List<BreadcrumbMessage>(previous_session);
            }
            return answer;
        }

        internal void LeaveBreadcrumb(string breadcrumb) {
            try {
                lock (this) {
                    current_session.Add(new BreadcrumbMessage(breadcrumb));
                    if (current_session.Count>MaxBreadcrumbCount) {
                        // Remove the oldest breadcrumb after the "session_start".
                        current_session.RemoveAt(1);
                    }
                    Saved=false;
                };
            } catch (Exception e) {
                Crittercism.LogInternalException(e);
                // explicit nop
            }
        }

        /// <summary>
        /// Saves to disk.
        /// </summary>
        /// <returns>   true if it succeeds, false if it fails. </returns>
        internal bool Save() {
            bool answer=false;
            try {
                lock (this) {
                    if (!Saved) {
                        answer=StorageHelper.Save(this);
                        Saved=true;
                    }
                }
            } catch (Exception e) {
                Crittercism.LogInternalException(e);
            }
            return answer;
        }

        /// <summary>
        /// Gets new session breadcrumbs.
        /// </summary>
        /// <returns>   The breadcrumbs. </returns>
        internal static Breadcrumbs SessionStart() {
            Breadcrumbs answer=null;
            try {
                // Breadcrumbs answer has previous_session == the previous current_session
                // and new current_session == empty but for new session_start breadcrumb .
                string path=Path.Combine(StorageHelper.CrittercismPath(),"Breadcrumbs.js");
                if (StorageHelper.FileExists(path)) {
                    answer=StorageHelper.Load(path,typeof(Breadcrumbs)) as Breadcrumbs;
                }
                if (answer==null) {
                    answer=new Breadcrumbs();
                }
                answer.previous_session=answer.current_session;
                answer.current_session=new List<BreadcrumbMessage>();
                answer.current_session.Add(new BreadcrumbMessage("session_start"));
            } catch (Exception e) {
                Crittercism.LogInternalException(e);
            };
            return answer;
        }
    }
}
