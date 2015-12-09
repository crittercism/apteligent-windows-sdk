using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace CrittercismSDK
{
    [DataContract]
    internal class UserBreadcrumbs {
        #region Properties
        // The breadcrumbs of the previous session.
        [DataMember]
        private List<UserBreadcrumb> previous_session;
        // The breadcrumbs of the current session
        [DataMember]
        private List<UserBreadcrumb> current_session;
        #endregion

        #region Constructor
        internal UserBreadcrumbs(List<UserBreadcrumb> previous_session,List<UserBreadcrumb> current_session) {
            this.previous_session = previous_session;
            this.current_session = current_session;
        }
        #endregion
    }
}
