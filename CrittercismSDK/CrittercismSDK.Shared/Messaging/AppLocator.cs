//#define MOCK
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
#if NETFX_CORE
using Windows.UI.Xaml;
#elif WINDOWS_PHONE
#else
using System.Web;
#endif

namespace CrittercismSDK {
    class AppLocator {
        // Chosen POP domain
        internal readonly string domain;

        // URL's needed by Crittercism SDK
        internal readonly string apiURL;
        internal readonly string apmURL;
        internal readonly string appLoadURL;
        internal readonly string txnURL;

        // AppLocator Sequences
        private const int AppLocatorLength = 8;
        private const string US_WEST_1_PROD_DESIGNATOR = "00555300";
        private const string US_WEST_2_CI_DESIGNATOR = "00555304";
        private const string US_WEST_2_STAGING_DESIGNATOR = "00555305";
        private const string EU_CENTRAL_1_PROD_DESIGNATOR = "00444503";

        // Base URL prefixes
        private const string CritterBaseURLPrefix = "https://api.";
        private const string NetDataBaseURLPrefix = "https://apm.";
        private const string AppLoadBaseURLPrefix = "https://appload.ingest.";
        private const string TxnBaseURLPrefix = "https://txn.ingest.";

        // Domains
        private const string ProductionDomain = "crittercism.com";
        private const string EuropeProductionDomain = "eu.crittercism.com";
        private const string ContinuousIntegrationDomain = "crit-ci.com";
        private const string StagingDomain = "crit-staging.com";

        #region Constructor
        private string GetDomainFromAppId(string appId) {
            ////////////////////////////////////////////////////////////////
            // Check if the server is specified in the appId.
            // It is important we make sure to validate this fact because
            // we do not want to possibly send data to the wrong server.
            ////////////////////////////////////////////////////////////////'
            string pattern = "^[0-9a-fA-F]+$";
            Regex hexRegex = new Regex(pattern,RegexOptions.IgnoreCase);
            if (hexRegex.Matches(appId).Count == 1) {
                if (appId.Length == 24) {
                    return ProductionDomain;
                } else if (appId.Length == 40) {
                    string appLocatorSequence = appId.Substring(appId.Length - AppLocatorLength);
                    if (appLocatorSequence.Equals(US_WEST_1_PROD_DESIGNATOR)) {
                        return ProductionDomain;
                    } else if (appLocatorSequence.Equals(EU_CENTRAL_1_PROD_DESIGNATOR)) {
                        return EuropeProductionDomain;
                    } else if (appLocatorSequence.Equals(US_WEST_2_CI_DESIGNATOR)) {
                        return ContinuousIntegrationDomain;
                    } else if (appLocatorSequence.Equals(US_WEST_2_STAGING_DESIGNATOR)) {
                        return StagingDomain;
                    }
                }
            }
            return null;
        }
        internal AppLocator(string appID) {
#if MOCK
            domain = "mock.crittercism.com";
            apiURL = "http://"+domain;
            apmURL = apiURL;
            appLoadURL = apiURL;
            txnURL = apiURL;
#else
            domain = GetDomainFromAppId(appID);
            if (domain != null) {
                apiURL = CritterBaseURLPrefix + domain;
                apmURL = NetDataBaseURLPrefix + domain;
                appLoadURL = AppLoadBaseURLPrefix + domain;
                txnURL = TxnBaseURLPrefix + domain;
            }
#endif
        }
        #endregion

        #region GetUri
        internal HttpWebRequest GetWebRequest(Type t) {
            HttpWebRequest answer = null;
            switch (t.Name) {
                case "AppLoad":
                    answer = (HttpWebRequest)WebRequest.Create(new Uri(appLoadURL + "/v0/appload",UriKind.Absolute));
                    break;
                case "APMReport":
                    answer = (HttpWebRequest)WebRequest.Create(new Uri(apmURL + "/api/apm/network",UriKind.Absolute));
                    break;
                case "HandledException":
                    answer = (HttpWebRequest)WebRequest.Create(new Uri(apiURL + "/v1/errors",UriKind.Absolute));
                    break;
                case "CrashReport":
                    answer = (HttpWebRequest)WebRequest.Create(new Uri(apiURL + "/v1/crashes",UriKind.Absolute));
                    break;
                case "MetadataReport":
                    answer = (HttpWebRequest)WebRequest.Create(new Uri(apiURL + "/feedback/update_user_metadata",UriKind.Absolute));
                    break;
                case "UserflowReport":
                    answer = (HttpWebRequest)WebRequest.Create(new Uri(txnURL + "/api/v1/userflows",UriKind.Absolute));
                    break;
            }
            return answer;
        }
        #endregion
    }
}
