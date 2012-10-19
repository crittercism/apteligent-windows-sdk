using CrittercismSDK.DataContracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;

namespace CrittercismSDK
{
    class QueueReader
    {
        /// <summary>
        /// Reads the queue.
        /// </summary>
        public void ReadQueue()
        {
            ConnectionProfile connectionProfile = NetworkInformation.GetInternetConnectionProfile();
            while (Crittercism.MessageQueue != null && Crittercism.MessageQueue.Count > 0 && connectionProfile.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess)
            {
                if (NetworkInterface.GetIsNetworkAvailable())
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
                    }
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
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                try
                {
                    MemoryStream messageStream = new MemoryStream();
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(message.GetType());
                    serializer.WriteObject(messageStream, message);
                    messageStream.Flush();

                    // Debug code, not need to copy the stream to the httpwebrequest
                    messageStream.Seek(0, SeekOrigin.Begin);
                    StreamReader reader = new StreamReader(messageStream);
                    string jsonMessage = reader.ReadToEnd();

                    // HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri("https://api.crittercism.com/v0/crashes", UriKind.Absolute));
                    HttpWebRequest request = null;
                    switch (message.GetType().Name)
                    {
                        case "AppLoad":
                            request = (HttpWebRequest)WebRequest.Create(new Uri("http://192.168.15.139/tce/v0/app_loads", UriKind.Absolute));
                            break;
                        case "Error":
                            request = (HttpWebRequest)WebRequest.Create(new Uri("http://192.168.15.139/tce/v0/errors", UriKind.Absolute));
                            break;
                        default:
                            request = (HttpWebRequest)WebRequest.Create(new Uri("http://192.168.15.139/tce/v0/crashes", UriKind.Absolute));
                            break;
                    }

                    request.Method = "POST";
                    request.ContentType = "application/json; charset=utf-8";
                    bool sendCompleted = false;
                    System.Threading.ManualResetEvent resetEvent = new System.Threading.ManualResetEvent(false);
                    request.BeginGetRequestStream(
                        (result) =>
                        {
                            try
                            {
                                Stream requestStream = request.EndGetRequestStream(result);
                                serializer.WriteObject(requestStream, message);
                                requestStream.Flush();
                                request.BeginGetResponse(
                                     (asyncResponse) =>
                                     {
                                         try
                                         {
                                             HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(asyncResponse);
                                             if (response.StatusCode == HttpStatusCode.OK)
                                             {
                                                 if (message.GetType().Name == "AppLoad" && string.IsNullOrEmpty(Crittercism.DeviceId))
                                                 {
                                                     DataContractJsonSerializer serializerBody = new DataContractJsonSerializer(typeof(AppLoadResponse));
                                                     Stream responseStream = response.GetResponseStream();
                                                     AppLoadResponse appLoadResponse = serializerBody.ReadObject(responseStream) as AppLoadResponse;
                                                     if (appLoadResponse != null && !string.IsNullOrEmpty(appLoadResponse.did))
                                                     {
                                                         Crittercism.DeviceId = appLoadResponse.did;
                                                         appLoadResponse.SaveToDisk();
                                                     }
                                                 }

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
                                                     }
                                                     catch
                                                     {
                                                         // if is another error we just ignore for now
                                                     }
                                                 }
                                             }
                                         }
                                         catch
                                         {
                                             // if is another error we just ignore for now
                                         }

                                         resetEvent.Set();
                                     }, null);
                            }
                            catch
                            {
                                // release the lock if something fail.
                                resetEvent.Set();
                            }
                        }, null);
                    resetEvent.WaitOne(10000); // timeout of 10 seconds to send the message
                    return sendCompleted;
                }
                catch
                {
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
    }
}