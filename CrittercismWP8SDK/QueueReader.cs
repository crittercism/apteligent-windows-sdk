using CrittercismSDK.DataContracts;
using CrittercismSDK.DataContracts.Legacy;
using CrittercismSDK.DataContracts.Unified;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.NetworkInformation;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Net;

namespace CrittercismSDK
{
    public class QueueReader
    {
        internal static string HostToUse;

        static QueueReader()
        {
            const string CRITTERCISM_API_HOST = "https://api.crittercism.com";

            // Use this as follows in your test app's AssemblyInfo.cs:
            //[assembly: AssemblyMetadata("Crittercism.CustomApiUrl", "http://127.0.0.1:8080")]
            const string CRITTERCISM_API_HOST_OVERRIDE_KEY = "Crittercism.CustomApiUrl";

            HostToUse = CRITTERCISM_API_HOST;
            try
            {
                System.Reflection.Assembly asm = System.Windows.Application.Current.GetType().Assembly;
                foreach (System.Reflection.CustomAttributeData att in asm.CustomAttributes)
                {
                    if (att.AttributeType.Equals(Type.GetType("System.Reflection.AssemblyMetadataAttribute")))
                    {
                        if (att.ConstructorArguments != null &&
                            att.ConstructorArguments.Count == 2 &&
                            att.ConstructorArguments[0].Value.Equals(CRITTERCISM_API_HOST_OVERRIDE_KEY))
                        {
                            HostToUse = att.ConstructorArguments[1].Value.ToString();
                        }
                    }
                }
            }
            catch
            {
               HostToUse = CRITTERCISM_API_HOST;
            }
        }

        /// <summary>
        /// Reads the queue.
        /// </summary>
        internal void ReadQueue()
        {
            int retry = 0;
            while (Crittercism.MessageQueue != null && Crittercism.MessageQueue.Count > 0 && NetworkInterface.GetIsNetworkAvailable() && retry < 3)
            {
                MessageReport message = Crittercism.MessageQueue.Peek();
                if (!message.IsLoaded)
                {
                    message.LoadFromDisk();
                }

                if (SendMessage(message))
                {
                    Crittercism.MessageQueue.Dequeue();
                    message.DeleteFromDisk();
                    retry = 0;
                }
                else
                {
                    retry++;
                }
            }
        }

        /// <summary>
        /// Send message to the endpoint.
        /// </summary>
        /// <param name="message">  The message. </param>
        /// <returns>   true if it succeeds, false if it fails. </returns>
        private bool SendMessage(MessageReport message)
        {
            // check if the communication layer is enable and if not return true.. this is used for unit testing.
            if (!Crittercism._enableCommunicationLayer)
            {
                return true;
            }

            if (NetworkInterface.GetIsNetworkAvailable())
            {
                try
                {
                    // FIXME jbley many many things special-cased for UserMetadata - really need /v1 here
                    string postBody = null;
                    HttpWebRequest request = null;
                    switch (message.GetType().Name)
                    {
                        case "AppLoad":
                            request = (HttpWebRequest)WebRequest.Create(new Uri(HostToUse + "/v0/appload", UriKind.Absolute));
                            request.ContentType = "application/json; charset=utf-8";
                            MessageReport[] messages = { message };
                            postBody = Newtonsoft.Json.JsonConvert.SerializeObject(messages);
                            break;
                        case "HandledException":
                            // FIXME jbley fix up the URI here
                            request = (HttpWebRequest)WebRequest.Create(new Uri(HostToUse + "/v1/errors", UriKind.Absolute));
                            request.ContentType = "application/json; charset=utf-8";
                            postBody = Newtonsoft.Json.JsonConvert.SerializeObject(message);
                            break;
                        case "Crash":
                            request = (HttpWebRequest)WebRequest.Create(new Uri(HostToUse + "/v1/crashes", UriKind.Absolute));
                            request.ContentType = "application/json; charset=utf-8";
                            postBody = Newtonsoft.Json.JsonConvert.SerializeObject(message);
                            break;
                        case "UserMetadata":
                            request = (HttpWebRequest)WebRequest.Create(new Uri(HostToUse + "/feedback/update_user_metadata", UriKind.Absolute));
                            request.ContentType = "application/x-www-form-urlencoded";
                            UserMetadata um = message as UserMetadata;
                            postBody = ComputeFormPostBody(um);
                            break;
                        default:
                            // FIXME jbley maybe some logging here?
                            return true; // consider this message "consumed"
                    }

                    request.Method = "POST";

                    bool sendCompleted = false;
                    Exception lastException = null;
                    System.Threading.ManualResetEvent resetEvent = new System.Threading.ManualResetEvent(false);
                    request.BeginGetRequestStream(
                        (result) =>
                        {
                            try
                            {
                                Stream requestStream = request.EndGetRequestStream(result);
                                StreamWriter writer = new StreamWriter(requestStream);
                                writer.Write(postBody);
                                writer.Flush();
                                writer.Close();
                                request.BeginGetResponse(
                                     (asyncResponse) =>
                                     {
                                         try
                                         {
                                             HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(asyncResponse);
                                             if (response.StatusCode == HttpStatusCode.OK)
                                             {
                                                 sendCompleted = true;
                                             }
                                         }
                                         catch (WebException webEx)
                                         {
                                             if (webEx.Response != null)
                                             {
                                                 HttpWebResponse response = (HttpWebResponse)webEx.Response;
                                                 if (response.StatusCode == HttpStatusCode.BadRequest)
                                                 {
                                                     try
                                                     {
                                                         StreamReader errorReader = (new StreamReader(webEx.Response.GetResponseStream()));
                                                         string errorMessage = errorReader.ReadToEnd();
                                                         System.Diagnostics.Debug.WriteLine(errorMessage);
                                                         lastException = new Exception(errorMessage, webEx);
                                                     }
                                                     catch (Exception ex)
                                                     {
                                                         lastException = ex;
                                                     }
                                                 }
                                             }
                                         }
                                         catch (Exception ex)
                                         {
                                             lastException = ex;
                                         }

                                         resetEvent.Set();
                                     }, null);
                            }
                            catch (Exception ex)
                            {
                                lastException = ex;

                                // release the lock if something fail.
                                resetEvent.Set();
                            }
                        }, null);
                    resetEvent.WaitOne(30000); // timeout of 30 seconds to send the message
                    if (Crittercism._enableRaiseExceptionInCommunicationLayer && lastException != null)
                    {
                        throw lastException;
                    }

                    return sendCompleted;
                }
                catch
                {
                    if (Crittercism._enableRaiseExceptionInCommunicationLayer)
                    {
                        throw;
                    }

                    // This is in case of have a exception in the middle of the send process
                    return false;
                }
            }
            else
            {
                // This is in case of internet is not available
                return false;
            }
        }

        public static string ComputeFormPostBody(UserMetadata um)
        {
            string postBody = "";
            postBody += "did=" + um.platform.device_id + "&";
            postBody += "app_id=" + um.app_id + "&";
            string metadataJson = Newtonsoft.Json.JsonConvert.SerializeObject(um.metadata);
            postBody += "metadata=" + HttpUtility.UrlEncode(metadataJson)+"&";
            postBody += "device_name=" + HttpUtility.UrlEncode(um.platform.device_model);
            return postBody;
        }

    }
}