using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace CrittercismSDK {
    /// <summary>
    /// Breadcrumbs.
    /// </summary>
    [DataContract]
    internal class Breadcrumbs {
        #region Constants
        internal const int MAX_TEXT_LENGTH = 140;
        #endregion

        #region Properties
        private static Object lockObject = new Object();
        private static Breadcrumbs userBreadcrumbs = null;
        private static Breadcrumbs networkBreadcrumbs = null;
        private static Breadcrumbs systemBreadcrumbs = null;
        private string name;
        private int maxCount;
        // The breadcrumbs of the previous session.
        [DataMember]
        private List<Breadcrumb> previous_session;
        // The breadcrumbs of the current session
        [DataMember]
        private List<Breadcrumb> current_session;
        private bool saved { get; set; }
        #endregion

        #region Static Methods
        internal static Breadcrumbs UserBreadcrumbs() {
            lock (lockObject) {
                if (userBreadcrumbs == null) {
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
            previous_session = new List<Breadcrumb>();
            current_session = new List<Breadcrumb>();
            saved = false;
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
            data["text"] = StringUtils.TruncateString(text,MAX_TEXT_LENGTH);
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
        internal static void LeaveEventBreadcrumb(string eventType) {
            // 3 - app event (i.e. foreground/background)   ; {event:}
            ////////////////////////////////////////////////////////////////
            // 3 - Application Event
            // Properties:
            // event - string, event type, i.e. "foregrounded", "backgrounded"
            ////////////////////////////////////////////////////////////////
            Dictionary<string,Object> data = new Dictionary<string,Object>();
            data["event"] = eventType;
            SystemBreadcrumbs().AddBreadcrumb(BreadcrumbType.Event,data);
        }
        internal static void LeaveReachabilityBreadcrumb(BreadcrumbReachabilityType change) {
            // 4 - network change ; {change:}
            ////////////////////////////////////////////////////////////////
            // 4 - Network change
            // Used to convey various changes to the network connectivity of the
            // device.
            // Properties:
            // change - number, type of network change that occurred
            // 0 - internet connection up                      ;
            // 1 - internet connection down                    ;
            // EXAMPLE:
            // network event, internet connectivity gained
            // ["1992-04-26T13:12:36Z", 4, {change: 0}],
            ////////////////////////////////////////////////////////////////
            Debug.Assert(((change == BreadcrumbReachabilityType.Up) || (change == BreadcrumbReachabilityType.Down)),
              "Illegal change arg for LeaveReachabilityBreadcrumb");
            Dictionary<string,Object> data = new Dictionary<string,Object>();
            data["change"] = change;
            SystemBreadcrumbs().AddBreadcrumb(BreadcrumbType.Reachability,data);
        }
        internal static void LeaveReachabilityBreadcrumb(BreadcrumbReachabilityType change,string reachabilityType) {
            // 4 - network change ; {change:,type:}
            ////////////////////////////////////////////////////////////////
            // 4 - Network change
            // Used to convey various changes to the network connectivity of the
            // device.
            // Properties:
            // change - number, type of network change that occurred
            // 2 - connectivity type gained                    ; type
            // 3 - connectivity type lost                      ; type
            // reachabilityType - string, connection type
            // only applies to change types 2 and 3
            // oldType - string, previous connection type
            // only applies to change type 4
            // newType - string, new connection type
            // only applies to change type 4
            // EXAMPLE:
            // network event, connectivity type gained, "wifi"
            // ["1992-04-26T13:12:38Z", 4, {change: 2, type: "wifi"}],
            ////////////////////////////////////////////////////////////////
            Debug.Assert(((change == BreadcrumbReachabilityType.Gained) || (change == BreadcrumbReachabilityType.Lost)),
              "Illegal change arg for LeaveReachabilityBreadcrumb");
            Dictionary<string,Object> data = new Dictionary<string,Object>();
            data["change"] = change;
            data["type"] = reachabilityType;
            SystemBreadcrumbs().AddBreadcrumb(BreadcrumbType.Reachability,data);
        }
        internal static void LeaveReachabilityBreadcrumb(BreadcrumbReachabilityType change,string oldType,string newType) {
            // 4 - network change ; {change:,oldType:,newType:}
            ////////////////////////////////////////////////////////////////
            // 4 - Network change
            // Used to convey various changes to the network connectivity of the
            // device.
            // Properties:
            // change - number, type of network change that occurred
            // 4 - switch from one connection type to another  ; oldType,newType
            // oldType - string, previous connection type
            // only applies to change type 4
            // newType - string, new connection type
            // only applies to change type 4
            ////////////////////////////////////////////////////////////////
            Debug.Assert((change == BreadcrumbReachabilityType.Switch),
              "Illegal change arg for LeaveReachabilityBreadcrumb");
            Dictionary<string,Object> data = new Dictionary<string,Object>();
            data["change"] = change;
            data["oldType"] = oldType;
            data["newType"] = newType;
            SystemBreadcrumbs().AddBreadcrumb(BreadcrumbType.Reachability,data);
        }
        internal static void LeaveViewBreadcrumb(BreadcrumbViewType eventType,string viewName) {
            // 5 - uiview/"activity" load ; {event:,viewName:}
            ////////////////////////////////////////////////////////////////
            // 5 - UI View/Activity Events
            // Used to denote when user interface views (activities on android)
            // are loaded and unloaded.
            // Properties:
            // event - number, type of ui event
            // 0 - view loaded/activated
            // 1 - view unloaded/deactivated
            // viewName - string, name of view or activity
            ////////////////////////////////////////////////////////////////
            Dictionary<string,Object> data = new Dictionary<string,Object>();
            data["event"] = eventType;
            data["viewName"] = viewName;
            SystemBreadcrumbs().AddBreadcrumb(BreadcrumbType.View,data);
        }
        internal static void LeaveErrorBreadcrumb(string name,string reason) {
            // 6 - handled exception ; {name:,reason:}
            ////////////////////////////////////////////////////////////////
            // 6 - Handled Exception Occurred
            // Properties
            // name, string - exception name
            // reason, string - exception reason
            ////////////////////////////////////////////////////////////////
            Dictionary<string,Object> data = new Dictionary<string,Object>();
            data["name"] = name;
            data["reason"] = reason;
            SystemBreadcrumbs().AddBreadcrumb(BreadcrumbType.Error,data);
        }
        #endregion

        #region Extracting Breadcrumbs
        private static Breadcrumb SessionStartBreadcrumb(List<Breadcrumb> session) {
            // First Breadcrumb in session (Launch Breadcrumb equivalent to "session_start").
            Breadcrumb answer = null;
            if (session.Count > 0) {
                answer = session[0];
            }
            return answer;
        }
        internal static List<UserBreadcrumb> ConvertToUserBreadcrumbs(List<Breadcrumb> session,bool windowsStyle) {
            // Convert List<Breadcrumb> to List<UserBreadcrumb> .
            List<UserBreadcrumb> answer = new List<UserBreadcrumb>();
            foreach (Breadcrumb breadcrumb in session) {
                UserBreadcrumb userBreadcrumb = new UserBreadcrumb(breadcrumb,windowsStyle);
                answer.Add(userBreadcrumb);
            }
            return answer;
        }
        internal static List<UserBreadcrumb> ExtractUserBreadcrumbs(long beginTime,long endTime) {
            // Extract UserBreadcrumb's from converted userBreadcrumb's filtered by time. (TransactionReport "breadcrumbs")
            List<Breadcrumb> list = userBreadcrumbs.RecentBreadcrumbs(beginTime,endTime);
            {
                Breadcrumb sessionStartBreadcrumb = SessionStartBreadcrumb(userBreadcrumbs.current_session);
                if ((sessionStartBreadcrumb != null)
                    && (list.IndexOf(sessionStartBreadcrumb) < 0)) {
                    // Add session start breadcrumb at the begining, this if statement should always be true
                    // In case we didn't log session start breadcrumb, we don't want to send an empty breadcrumb
                    // and bread the server
                    list.Insert(0,sessionStartBreadcrumb);
                }
            }
            List<UserBreadcrumb> answer = ConvertToUserBreadcrumbs(list,false);
            return answer;
        }
        internal static UserBreadcrumbs GetAllSessionsBreadcrumbs() {
            // Extract legacy UserBreadcrumbs object from userBreadcrumbs. (CrashReport "breadcrumbs".)
            List<UserBreadcrumb> previous = ConvertToUserBreadcrumbs(userBreadcrumbs.previous_session,true);
            List<UserBreadcrumb> current = ConvertToUserBreadcrumbs(userBreadcrumbs.current_session,true);
            UserBreadcrumbs answer = new CrittercismSDK.UserBreadcrumbs(previous,current);
            return answer;
        }
        private static List<Endpoint> ExtractEndpointsFromBreadcrumbs(List<Breadcrumb> breadcrumbs) {
            // Called by ExtractAllEndpoints and ExtractEndpoints .
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
            // Extract Endpoint's from all Network Breadcrumb's. (CrashReport "endpoints".)
            List<Breadcrumb> breadcrumbs = NetworkBreadcrumbs().RecentBreadcrumbs();
            return ExtractEndpointsFromBreadcrumbs(breadcrumbs);
        }
        internal static List<Endpoint> ExtractEndpoints(long beginTime,long endTime) {
            // Extract Endpoint's from all Network Breadcrumb's filtered by time. (TransactionReport "endpoints".)
            List<Breadcrumb> breadcrumbs = NetworkBreadcrumbs().RecentBreadcrumbs(beginTime,endTime);
            return ExtractEndpointsFromBreadcrumbs(breadcrumbs);
        }
        internal List<Breadcrumb> RecentBreadcrumbs() {
            // Copy of current_session Breadcrumb's. (CrashReport "systemBreadcrumbs".)
            return new List<Breadcrumb>(current_session);
        }
        internal List<Breadcrumb> RecentBreadcrumbs(long beginTime,long endTime) {
            // Recent Breadcrumb's filtered by time . (TransactionReport "systemBreadcrumbs".)
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
        #endregion

        #region Instance Methods
        internal Breadcrumbs Copy() {
            Breadcrumbs answer = new Breadcrumbs(name,maxCount);
            lock (this) {
                answer.current_session = new List<Breadcrumb>(current_session);
                answer.previous_session = new List<Breadcrumb>(previous_session);
            }
            return answer;
        }

        /// <summary>
        /// Saves to disk.
        /// </summary>
        /// <returns>   true if it succeeds, false if it fails. </returns>
        private bool Save() {
            bool answer = false;
            try {
                lock (this) {
                    if (!saved) {
                        answer = StorageHelper.Save(this);
                        saved = true;
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
