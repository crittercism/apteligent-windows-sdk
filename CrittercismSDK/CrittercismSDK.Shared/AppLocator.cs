#define MOCK
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CrittercismSDK
{
    class AppLocator
    {
        // URL's needed by Crittercism SDK
        internal readonly string apiURL;
        internal readonly string apmURL;
        internal readonly string appLoadURL;
        internal readonly string txnURL;

        // AppLocator Sequences
        private const int AppLocatorLength=8;
        private const string US_WEST_1_PROD_DESIGNATOR="00555300";
        private const string US_WEST_2_CI_DESIGNATOR="00555304";
        private const string US_WEST_2_STAGING_DESIGNATOR="00555305";
        private const string EU_CENTRAL_1_PROD_DESIGNATOR="00444503";

        // Base URL prefixes
        private const string CritterBaseURLPrefix="https://api.";
        private const string NetDataBaseURLPrefix="https://apm.";
        private const string AppLoadBaseURLPrefix="https://appload.ingest.";
        private const string TxnBaseURLPrefix="https://txn.ingest.";

        // Domains
        private const string ProductionDomain="crittercism.com";
        private const string EuropeProductionDomain="eu.crittercism.com";
        private const string ContinuousIntegrationDomain="crit-ci.com";
        private const string StagingDomain="crit-staging.com";

        private static string GetDomainFromAppId(string appId) {
            ////////////////////////////////////////////////////////////////
            // Check if the server is specified in the appId.
            // It is important we make sure to validate this fact because
            // we do not want to possibly send data to the wrong server.
            ////////////////////////////////////////////////////////////////'
            string pattern="^[0-9a-fA-F]+$";
            Regex hexRegex=new Regex(pattern,RegexOptions.IgnoreCase);
            if (hexRegex.Matches(appId).Count!=1) {
                return null;
            }
            if (appId.Length==24) {
                return ProductionDomain;
            } else if (appId.Length==40) {
                string appLocatorSequence=appId.Substring(appId.Length-AppLocatorLength);
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
            return null;
        }

        internal AppLocator(string appID) {
#if MOCK
            string mockURL="http://mock.crittercism.com";
            apiURL=mockURL;
            apmURL=mockURL;
            appLoadURL=mockURL;
            txnURL=mockURL;
#else
            string domain=GetDomainFromAppId(appID);
            apiURL=CritterBaseURLPrefix+domain;
            apmURL=NetDataBaseURLPrefix+domain;
            appLoadURL=AppLoadBaseURLPrefix+domain;
            txnURL=TxnBaseURLPrefix+domain;
#endif
        }
    }
}
