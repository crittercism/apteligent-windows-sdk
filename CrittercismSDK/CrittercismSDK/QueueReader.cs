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
        public void ReadQueue()
        {
            while (true)
            {
                if (Crittercism.MessageQueue != null && Crittercism.MessageQueue.Count > 0)
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
                else
                {
                    System.Threading.Thread.Sleep(500);
                }
            }
        }

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

                    //// System.Diagnostics.Debug.WriteLine(jsonMessage);

                    // send message
                    // HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri("https://api.crittercism.com/v0/crashes", UriKind.Absolute));

                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri("http://localhost:15438/v0/crashes", UriKind.Absolute));
                    request.Method = "POST";
                    request.ContentType = "application/json; charset=utf-8";
                    
                    //// request.ContentLength = messageStream.Length;

                    //request.Credentials
                    //Stream requestStream = request.GetRequestStream(); // this is only for Windows 8
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
                                requestStream.Close();
                                
                                request.BeginGetResponse(
                                     (asyncResponse) =>
                                     {
                                         HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(asyncResponse);
                                         if (response.StatusCode == HttpStatusCode.OK)
                                         {
                                             sendCompleted = true;
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
                    resetEvent.WaitOne(30000); // time out of 30 seconds to send the message
                    return sendCompleted;

                    //// return true;
                }
                catch (Exception ex)
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