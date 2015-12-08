using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace CrittercismSDK
{
    /// <summary>
    /// Breadcrumbs.
    /// </summary>
    [DataContract]
    internal class Breadcrumbs
    {
        #region Constants
        #endregion

        #region Properties
        private static Object lockObject = new Object();
        private static Breadcrumbs userBreadcrumbs = null;
        private static Breadcrumbs networkBreadcrumbs = null;
        private static Breadcrumbs systemBreadcrumbs = null;
        private string name;
        private int maxCount;
        // The breadcrumbs of the crashed session.
        [DataMember]
        private List<Breadcrumb> previous_session;
        // The breadcrumbs of the current session. </value>
        [DataMember]
        private List<Breadcrumb> current_session;
        private bool saved { get; set; }
        #endregion

        #region Static Methods
        internal static Breadcrumbs UserBreadcrumbs() {
            lock (lockObject) {
                if (userBreadcrumbs==null) {
                    const int maxUserBreadcrumbsCount = 100;
                    userBreadcrumbs = SessionStart("UserBreadcrumbs",maxUserBreadcrumbsCount);
                }
            }
            return userBreadcrumbs;
        }
        internal static Breadcrumbs NetworkBreadcrumbs() {
            lock (lockObject) {
                if (networkBreadcrumbs == null) {
                    const int maxEndpointsCount = 10;
                    networkBreadcrumbs = SessionStart("NetworkBreadcrumbs",maxEndpointsCount);
                }
            }
            return networkBreadcrumbs;
        }
        internal static Breadcrumbs SystemBreadcrumbs() {
            lock (lockObject) {
                if (systemBreadcrumbs == null) {
                    const int maxSystemBreadcrumbsCount = 100;
                    systemBreadcrumbs = SessionStart("SystemBreadcrumbs",maxSystemBreadcrumbsCount);
                }
            }
            return systemBreadcrumbs;
        }
        /// <summary>
        /// Gets new session breadcrumbs.
        /// </summary>
        /// <returns>   The breadcrumbs. </returns>
        private static Breadcrumbs SessionStart(string name,int maxCount) {
            Breadcrumbs answer = null;
            try {
                // Breadcrumbs answer has previous_session == the previous current_session
                // and new current_session == empty but for new session_start breadcrumb .
                string path = Path.Combine(StorageHelper.CrittercismPath(),name + ".js");
                if (StorageHelper.FileExists(path)) {
                    answer = StorageHelper.Load(path,typeof(Breadcrumbs)) as Breadcrumbs;
                }
                if (answer == null) {
                    answer = new Breadcrumbs(name,maxCount);
                }
                answer.previous_session = answer.current_session;
                answer.current_session = new List<Breadcrumb>();
                if (name == "UserBreadcrumbs") {
                    Dictionary<string,Object> data = new Dictionary<string,Object>();
                    data["text"] = "session_start";
                    data["level"] = (int)BreadcrumbTextType.Normal;
                    answer.current_session.Add(new Breadcrumb(BreadcrumbType.Text,data));
                }
            } catch (Exception ie) {
                Crittercism.LogInternalException(ie);
            };
            return answer;
        }
        #endregion

        #region Life Cycle
        private Breadcrumbs(string name,int maxCount) {
            this.name = name;
            this.maxCount = maxCount;
            previous_session=new List<Breadcrumb>();
            current_session=new List<Breadcrumb>();
            saved=false;
        }
        #endregion

        #region Leaving Breadcrumbs
        internal void LeaveBreadcrumb(string breadcrumb) {
            LeaveUserBreadcrumb(breadcrumb,BreadcrumbTextType.Normal);
        }
        private void AddBreadcrumb(BreadcrumbType breadcrumbType,Object data) {
            try {
                lock (this) {
                    current_session.Add(new Breadcrumb(breadcrumbType,data));
                    if (current_session.Count > maxCount) {
                        // Remove the oldest breadcrumb after the "session_start".
                        // Only userBreadcrumbs will contain a "session_start".
                        current_session.RemoveAt((this == userBreadcrumbs) ? 1 : 0);
                    }
                    saved = false;
                };
            } catch (Exception ie) {
                Crittercism.LogInternalException(ie);
                // explicit nop
            }
        }
        internal static void LeaveUserBreadcrumb(string text,BreadcrumbTextType priority) {
            Dictionary<string,Object> data = new Dictionary<string,Object>();
            data["text"] = text;
            data["level"] = (int)priority;
            UserBreadcrumbs().AddBreadcrumb(BreadcrumbType.Text,data);
        }
        internal static void LeaveNetworkBreadcrumb(Endpoint endpoint) {
            // 2 - network breadcrumb ; [method,url,timestamp,latency,on_wifi,bytes_in,bytes_out,response_code,error_table,error_string, location_array]
            ////////////////////////////////////////////////////////////////
            // 2 - Network Breadcrumb
            // Array of breadcrumb data
            // Same as existing format:
            // ["POST", "http://10.0.1.99:6013/android_v2/update_user_metadata",
            // "2014-01-04T21:11:41.806+0000", 116, 0, 217, 502, 200, 3, "0"]
            ////////////////////////////////////////////////////////////////
            NetworkBreadcrumbs().AddBreadcrumb(BreadcrumbType.Network,endpoint);
        }
        #endregion

        #region Extracting Breadcrumbs
        private Breadcrumb CurrentSessionStartBreadcrumb() {
            Breadcrumb answer = null;
            if (current_session.Count>0) {
                answer = current_session[0];
            }
            return answer;
        }
        private static List<UserBreadcrumb> ConvertToUserBreadcrumbs(List<Breadcrumb> breadcrumbs) {
            List<UserBreadcrumb> answer = new List<UserBreadcrumb>();
            foreach (Breadcrumb breadcrumb in breadcrumbs) {
                UserBreadcrumb userBreadcrumb = new UserBreadcrumb(breadcrumb);
                answer.Add(userBreadcrumb);
            }
            return answer;
        }
        internal static List<UserBreadcrumb> ExtractUserBreadcrumbsFrom(long beginTime,long endTime) {
            List<Breadcrumb> list = userBreadcrumbs.RecentBreadcrumbs(beginTime,endTime);
            Breadcrumb sessionStartBreadcrumb = userBreadcrumbs.CurrentSessionStartBreadcrumb();
            if ((sessionStartBreadcrumb!=null)
                &&(list.IndexOf(sessionStartBreadcrumb)<0)) {
                // Add session start breadcrumb at the begining, this if statement should always be true
                // In case we didn't log session start breadcrumb, we don't want to send an empty breadcrumb
                // and bread the server
                list.Insert(0,sessionStartBreadcrumb);
            }
            // TODO: [CRBreadcrumbsArchive convertArrayToUserBreadcrumbFormat:breadcrumbs]  Ouch...
            List<UserBreadcrumb> answer = ConvertToUserBreadcrumbs(list);
            return answer;
        }
        private List<Breadcrumb> RecentBreadcrumbs() {
            // Copy of current state of current_session .
            return new List<Breadcrumb>(current_session);
        }
        internal List<Breadcrumb> RecentBreadcrumbs(long beginTime,long endTime) {
            // Recent breadcrumbs filtered by time .
            List<Breadcrumb> answer = new List<Breadcrumb>();
            foreach (Breadcrumb breadcrumb in current_session) {
                long breadcrumbTime = DateUtils.StringToTicks(breadcrumb.GetTimestamp());
                bool afterBeginTime = (beginTime <= breadcrumbTime);
                bool beforeEndTime = (breadcrumbTime <= endTime);
                if (afterBeginTime && beforeEndTime) {
                    answer.Add(breadcrumb);
                }
            }
            return answer;
        }
        private static List<Endpoint> ExtractEndpointsFromBreadcrumbs(List<Breadcrumb> breadcrumbs) {
            // Input = list of network breadcrumbs
            List<Endpoint> answer = new List<Endpoint>();
            foreach (Breadcrumb breadcrumb in breadcrumbs) {
                Endpoint endpoint = breadcrumb.GetData() as Endpoint;
                if (endpoint != null) {
                    // Should be an unnecessary check.
                    answer.Add(endpoint);
                }
            }
            return answer;
        }
        internal static List<Endpoint> ExtractAllEndpoints() {
            List<Breadcrumb> breadcrumbs = NetworkBreadcrumbs().RecentBreadcrumbs();
            return ExtractEndpointsFromBreadcrumbs(breadcrumbs);
        }
        internal static List<Endpoint> ExtractEndpoints(long beginTime,long endTime) {
            List<Breadcrumb> breadcrumbs = NetworkBreadcrumbs().RecentBreadcrumbs(beginTime,endTime);
            return ExtractEndpointsFromBreadcrumbs(breadcrumbs);
        }
        #endregion

        #region Instance Methods
        internal Breadcrumbs Copy() {
            Breadcrumbs answer=new Breadcrumbs(name,maxCount);
            lock (this) {
                answer.current_session=new List<Breadcrumb>(current_session);
                answer.previous_session=new List<Breadcrumb>(previous_session);
            }
            return answer;
        }

        /// <summary>
        /// Saves to disk.
        /// </summary>
        /// <returns>   true if it succeeds, false if it fails. </returns>
        private bool Save() {
            bool answer=false;
            try {
                lock (this) {
                    if (!saved) {
                        answer=StorageHelper.Save(this);
                        saved=true;
                    }
                }
            } catch (Exception ie) {
                Crittercism.LogInternalException(ie);
            }
            return answer;
        }
        internal static bool SaveAll() {
            bool answer = true;
            answer = answer && UserBreadcrumbs().Save();
            answer = answer && NetworkBreadcrumbs().Save();
            answer = answer && SystemBreadcrumbs().Save();
            return answer;
        }
        #endregion
    }
}
