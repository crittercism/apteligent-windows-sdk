using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace CrittercismSDK {
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

        #region Constructors
        internal UserBreadcrumbs(List<UserBreadcrumb> previous_session,List<UserBreadcrumb> current_session) {
            this.previous_session = previous_session;
            this.current_session = current_session;
        }
        internal UserBreadcrumbs() {
            // Newtonsoft.Json requires "class should either have a default constructor,
            // one constructor with arguments or a constructor marked with the JsonConstructor
            // attribute" in order to serialize UserBreadcrumbs .
        }
        #endregion

        #region JSON
        public override string ToString() {
            return JsonConvert.SerializeObject(this);
        }
        #endregion
    }
}
