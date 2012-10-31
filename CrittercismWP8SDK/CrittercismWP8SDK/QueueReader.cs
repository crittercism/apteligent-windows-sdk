using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.NetworkInformation;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Net;
using CrittercismSDK.DataContracts;

namespace CrittercismSDK
{
    class QueueReader
    {
        /// <summary>
        /// Reads the queue.
        /// </summary>
        public void ReadQueue()
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
                    string jsonMessage = Newtonsoft.Json.JsonConvert.SerializeObject(message);
                    HttpWebRequest request = null;
                    switch (message.GetType().Name)
                    {
                        case "AppLoad":
                            request = (HttpWebRequest)WebRequest.Create(new Uri("https://api.crittercism.com/v1/loads", UriKind.Absolute));
                            break;
                        case "Error":
                            request = (HttpWebRequest)WebRequest.Create(new Uri("https://api.crittercism.com/v1/errors", UriKind.Absolute));
                            break;
                        default:
                            request = (HttpWebRequest)WebRequest.Create(new Uri("https://api.crittercism.com/v1/crashes", UriKind.Absolute));
                            break;
                    }

                    request.Method = "POST";
                    request.ContentType = "application/json; charset=utf-8";
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
                                writer.Write(jsonMessage);
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
    }
}