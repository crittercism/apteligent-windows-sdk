using CrittercismSDK.DataContracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Net.NetworkInformation;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows;
#if NETFX_CORE
using Windows.UI.Xaml;
#if WINDOWS_PHONE_APP
using System.Threading.Tasks;
#endif
#endif

namespace CrittercismSDK
{
    internal class QueueReader
    {
        private const string CRITTERCISM_HOST="https://api.crittercism.com";

        /// <summary>
        /// Reads the queue.
        /// </summary>
        internal void ReadQueue() {
            Debug.WriteLine("ReadQueue: ENTER");
            try {
                while (true) {
                    Crittercism.readerEvent.WaitOne();
                    Debug.WriteLine("ReadQueue: WAKE");
                    ReadStep();
                    Debug.WriteLine("ReadQueue: SLEEP");
                };
            } catch (Exception e) {
                Crittercism.LogInternalException(e);
            }
            Debug.WriteLine("ReadQueue: EXIT");
        }

        private void ReadStep() {
            Debug.WriteLine("ReadStep: ENTER");
            try {
                int retry=0;
                while (Crittercism.MessageQueue!=null&&Crittercism.MessageQueue.Count>0&&NetworkInterface.GetIsNetworkAvailable()&&retry<100) {
                    MessageReport message=Crittercism.MessageQueue.Peek();
                    if (SendMessage(message)) {
                        Debug.WriteLine("ReadStep: SendMessage "+message.Name);
                        Crittercism.MessageQueue.Dequeue();
                        message.Delete();
                        retry=0;
                    } else {
                        retry++;
                        Debug.WriteLine("ReadStep: retry == {0}",retry);
                    }
                };
            } catch (Exception e) {
                Crittercism.LogInternalException(e);
            }
            Debug.WriteLine("ReadStep: EXIT");
        }

        /// <summary>
        /// Send message to the endpoint.
        /// </summary>
        /// <param name="message">  The message. </param>
        /// <returns>   true if it succeeds, false if it fails. </returns>
        private bool SendMessage(MessageReport message) {
            Debug.WriteLine("SendMessage: ENTER");
            bool sendCompleted=false;
            try {
                if (!Crittercism._enableCommunicationLayer) {
                    // check if the communication layer is enable and if not return true.. this is used for unit testing.
                    sendCompleted=true;
                } else if (NetworkInterface.GetIsNetworkAvailable()) {
                    try {
                        // FIXME jbley many many things special-cased for UserMetadata - really need /v1 here
                        string postBody=null;
                        HttpWebRequest request=null;
                        switch (message.GetType().Name) {
                            case "AppLoad":
                                request=(HttpWebRequest)WebRequest.Create(new Uri(CRITTERCISM_HOST+"/v1/loads",UriKind.Absolute));
                                request.ContentType="application/json; charset=utf-8";
                                postBody=JsonConvert.SerializeObject(message);
                                break;
                            case "HandledException":
                                // FIXME jbley fix up the URI here
                                request=(HttpWebRequest)WebRequest.Create(new Uri(CRITTERCISM_HOST+"/v1/errors",UriKind.Absolute));
                                request.ContentType="application/json; charset=utf-8";
                                postBody=JsonConvert.SerializeObject(message);
                                break;
                            case "Crash":
                                request=(HttpWebRequest)WebRequest.Create(new Uri(CRITTERCISM_HOST+"/v1/crashes",UriKind.Absolute));
                                request.ContentType="application/json; charset=utf-8";
                                postBody=JsonConvert.SerializeObject(message);
                                break;
                            case "UserMetadata":
                                request=(HttpWebRequest)WebRequest.Create(new Uri(CRITTERCISM_HOST+"/feedback/update_user_metadata",UriKind.Absolute));
                                request.ContentType="application/x-www-form-urlencoded";
                                UserMetadata um=message as UserMetadata;
                                postBody=ComputeFormPostBody(um);
                                break;
                            default:
                                // FIXME jbley maybe some logging here?
                                // consider this message "consumed"
                                sendCompleted=true;
                                break;
                        }
                        if (!sendCompleted) {
                            request.Method="POST";
                            sendCompleted=SendRequest(request,postBody);
                        }
                    } catch {
                        //Debug.WriteLine("SendMessage: catch");
                        if (Crittercism._enableRaiseExceptionInCommunicationLayer) {
                            throw;
                        }
                    }
                }
            } catch (Exception e) {
                Crittercism.LogInternalException(e);
            }
            Debug.WriteLine("SendMessage: WAKE UP READER");
            Crittercism.readerEvent.Set();
            Debug.WriteLine("SendMessage: EXIT ---> "+sendCompleted);
            return sendCompleted;
        }

#if WINDOWS_PHONE_APP
        private bool SendRequest(HttpWebRequest request,string postBody) {
            //Debug.WriteLine("SendMessage: request.RequestUri == {0}", request.RequestUri);
            bool sendCompleted=false;
            Debug.WriteLine("SendRequest: ENTER");
            try {
                Exception lastException=null;
                Task<Stream> writerTask=request.GetRequestStreamAsync();
                using (Stream writer=writerTask.Result) {
                    // NOTE: SendMessage caller's request.ContentType=="application/json; charset=utf-8"
                    // or request.ContentType=="application/x-www-form-urlencoded"
                    Debug.WriteLine("SendMessage: POST BODY:");
                    Debug.WriteLine(postBody);
                    byte[] postBytes=Encoding.UTF8.GetBytes(postBody);
                    writer.Write(postBytes,0,postBytes.Length);
                    writer.Flush();
                }
                Task<WebResponse> responseTask=request.GetResponseAsync();
                using (HttpWebResponse response=(HttpWebResponse)responseTask.Result) {
                    try {
                        Debug.WriteLine("SendMessage: response.StatusCode == {0}",(int)response.StatusCode);
                        if (response.StatusCode==HttpStatusCode.OK) {
                            sendCompleted=true;
                        }
                    } catch (WebException webEx) {
                        Debug.WriteLine("SendMessage: webEx == {0}",webEx);
                        if (webEx.Response!=null) {
                            //Debug.WriteLine("SendMessage: response.StatusCode == {0}",(int)response.StatusCode);
                            if (response.StatusCode==HttpStatusCode.BadRequest) {
                                try {
                                    using (StreamReader errorReader=(new StreamReader(webEx.Response.GetResponseStream()))) {
                                        string errorMessage=errorReader.ReadToEnd();
                                        Debug.WriteLine("SendMessage: "+errorMessage);
                                        lastException=new Exception(errorMessage,webEx);
                                    }
                                } catch (Exception ex) {
                                    lastException=ex;
                                }
                            }
                        }
                    } catch (Exception ex) {
                        Debug.WriteLine("SendMessage: ex == {0}",ex.Message);
                        lastException=ex;
                    }
                }
                if (Crittercism._enableRaiseExceptionInCommunicationLayer&&lastException!=null) {
                    throw lastException;
                }
            } catch (Exception e) {
                Crittercism.LogInternalException(e);
            }
            Debug.WriteLine("SendRequest: EXIT ---> "+sendCompleted);
            return sendCompleted;
        }
#else
        private bool SendRequest(HttpWebRequest request,string postBody) {
            //Debug.WriteLine("SendMessage: request.RequestUri == {0}", request.RequestUri);
            bool sendCompleted=false;
            Debug.WriteLine("SendRequest: ENTER");
            try {
                Exception lastException=null;
                ManualResetEvent resetEvent=new ManualResetEvent(false);
                request.BeginGetRequestStream(
                    (result) => {
                        //Debug.WriteLine("SendMessage: BeginGetRequestStream");
                        try {
                            using (Stream requestStream=request.EndGetRequestStream(result)) {
                                using (StreamWriter writer=new StreamWriter(requestStream)) {
                                    writer.Write(postBody);
                                    //Debug.WriteLine("SendMessage: postBody == {0}",postBody);
                                    writer.Flush();
#if NETFX_CORE
#else
                                    writer.Close();
#endif
                                }
                            }
                            request.BeginGetResponse(
                                 (asyncResponse) => {
                                     //Debug.WriteLine("SendMessage: BeginGetResponse");
                                     try {
                                         using (HttpWebResponse response=(HttpWebResponse)request.EndGetResponse(asyncResponse)) {
                                             Debug.WriteLine("SendMessage: response.StatusCode == {0}",(int)response.StatusCode);
                                             if (response.StatusCode==HttpStatusCode.OK) {
                                                 sendCompleted=true;
                                             }
                                         }
                                     } catch (WebException webEx) {
                                         Debug.WriteLine("SendMessage: webEx == {0}",webEx);
                                         if (webEx.Response!=null) {
                                             using (HttpWebResponse response=(HttpWebResponse)webEx.Response) {
                                                 //Debug.WriteLine("SendMessage: response.StatusCode == {0}",(int)response.StatusCode);
                                                 if (response.StatusCode==HttpStatusCode.BadRequest) {
                                                     try {
                                                         using (StreamReader errorReader=(new StreamReader(webEx.Response.GetResponseStream()))) {
                                                             string errorMessage=errorReader.ReadToEnd();
                                                             Debug.WriteLine(errorMessage);
                                                             lastException=new Exception(errorMessage,webEx);
                                                         }
                                                     } catch (Exception ex) {
                                                         lastException=ex;
                                                     }
                                                 }
                                             }
                                         }
                                     } catch (Exception ex) {
                                         //Debug.WriteLine("SendMessageKBR: ex == {0}",ex);
                                         lastException=ex;
                                     }
                                     resetEvent.Set();
                                 },null);
                        } catch (Exception ex) {
                            //Debug.WriteLine("SendMessage: ex#2 == {0}",ex);
                            lastException=ex;
                            // release the lock if something fail.
                            resetEvent.Set();
                        }
                    },null);
                {
                    // 300000 milliseconds == 5 minute timeout.
                    const int SENDREQUEST_MILLISECONDS_TIMEOUT=300000;
                    resetEvent.WaitOne(SENDREQUEST_MILLISECONDS_TIMEOUT);
                }
                if (Crittercism._enableRaiseExceptionInCommunicationLayer&&lastException!=null) {
                    throw lastException;
                }
            } catch (Exception e) {
                Crittercism.LogInternalException(e);
            }
            Debug.WriteLine("SendRequest: EXIT ---> "+sendCompleted);
            return sendCompleted;
        }
#endif // WINDOWS_PHONE_APP

        public static string ComputeFormPostBody(UserMetadata um) {
            string postBody="";
            postBody+="did="+um.platform.device_id+"&";
            postBody+="app_id="+um.app_id+"&";
            string metadataJson=JsonConvert.SerializeObject(um.metadata);
            postBody+="metadata="+WebUtility.UrlEncode(metadataJson)+"&";
            postBody+="device_name="+WebUtility.UrlEncode(um.platform.device_model);
            return postBody;
        }

    }
}
