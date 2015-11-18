using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;
using Newtonsoft.Json;
#if NETFX_CORE
//using System.Threading.Tasks;
//using Windows.UI.Xaml;
#elif WINDOWS_PHONE
#else
using System.Web;
#endif

namespace CrittercismSDK
{
    /// <summary>
    /// Application load.
    /// </summary>
    [DataContract]
    internal class MetadataReport : MessageReport
    {
        /// <summary>
        /// Crittercism-issued Application identification string
        /// </summary>
        [DataMember]
        public string app_id { get; internal set; }

        [DataMember]
        public Dictionary<string, string> metadata { get; internal set; }

        /// <summary>
        /// Execution platform on which the app runs
        /// </summary>
        [DataMember]
        public Platform platform { get; internal set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MetadataReport() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public MetadataReport(string _appId,Dictionary<string,string> userMetadata) {
            if (!String.IsNullOrEmpty(_appId)) {
                app_id=_appId;
                metadata=userMetadata;
                platform=new Platform();
            } else {
                throw new Exception("Crittercism requires an application_id to properly initialize itself.");
            }
        }
        internal override string ContentType() {
            // MetadataReport ContentType is an exceptional override.
            return "application/x-www-form-urlencoded";
        }
        internal override string PostBody() {
            // MetadataReport PostBody is an exceptional override.
            string answer = "";
            answer += "did=" + platform.device_id + "&";
            answer += "app_id=" + app_id + "&";
            string metadataJson = JsonConvert.SerializeObject(metadata);
#if NETFX_CORE
            answer+="metadata="+WebUtility.UrlEncode(metadataJson)+"&";
            answer+="device_name="+WebUtility.UrlEncode(platform.device_model);
#else
            // Only .NETFramework 4.5 has WebUtility.UrlEncode, earlier version
            // .NETFramework 4.0 has HttpUtility.UrlEncode
            answer += "metadata=" + HttpUtility.UrlEncode(metadataJson) + "&";
            answer += "device_name=" + HttpUtility.UrlEncode(platform.device_model);
#endif
            return answer;
        }
    }
}
