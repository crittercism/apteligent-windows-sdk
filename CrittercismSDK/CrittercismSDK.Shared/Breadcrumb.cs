using System;
using System.Collections.Generic;
using System.Text;

namespace CrittercismSDK
{
    internal class Breadcrumb
    {
        #region Properties
        private string timestamp;
        private BreadcrumbType breadcrumbType;
        private Object data;
        internal string GetTimestamp() {
            return timestamp;
        }
        internal BreadcrumbType GetBreadcrumbType() {
            return breadcrumbType;
        }
        internal Object GetData() {
            return data;
        }
        #endregion

        #region Constructor
        internal Breadcrumb(BreadcrumbType breadcrumbType,Object data) {
            this.timestamp = DateUtils.GMTDateString(DateTime.UtcNow);
            this.breadcrumbType = breadcrumbType;
            this.data = data;
        }
        #endregion
    }
}
