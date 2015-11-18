using CrittercismSDK;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
#if NETFX_CORE
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Networking.Connectivity;
#elif WINDOWS_PHONE
using Microsoft.Phone.Info;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Net.NetworkInformation;
#else
using Microsoft.Win32;
#endif // NETFX_CORE

namespace CrittercismSDK {
    /// <summary>
    /// Crittercism.
    /// </summary>
    public class Crittercism {
        #region Constants

        private const string errorNotInitialized="Crittercism not initialized yet.";

        #endregion Constants

        #region Properties
        /// <summary>
        /// Enable SendMessage
        /// </summary>
        internal static bool enableSendMessage = true;

        internal static string AppVersion { get; private set; }
        internal static string DeviceId { get; private set; }
        internal static string DeviceModel { get; private set; }

#if NETFX_CORE
        internal static string Version=typeof(Crittercism).GetTypeInfo().Assembly.GetName().Version.ToString();
#else
        internal static string Version=Assembly.GetExecutingAssembly().GetName().Version.ToString();
#endif

#if WINDOWS_PHONE
        internal static string Carrier=Microsoft.Phone.Net.NetworkInformation.
            DeviceNetworkInformation.CellularMobileOperator;
#else
        internal static string Carrier="UNKNOWN";
#endif

        // NOTE: Appears platform won't allow "wp" to be replaced by "windows"
        // carte blanche.  Doing so prevents handled exception reports from
        // being accepted by platform.
//#if WINDOWS_PHONE
        internal static readonly string OSName="wp";
//#else
//        internal static readonly string OSName="windows";
//#endif

        internal static long SessionId { get; private set; }

        /// <summary>
        /// Gets or sets a queue of messages.
        /// </summary>
        /// <value> A SynchronizedQueue of messages. </value>
        internal static SynchronizedQueue<MessageReport> MessageQueue { get; set; }

        /// <summary>
        /// Gets or sets the current breadcrumbs.
        /// </summary>
        /// <value> The breadcrumbs. </value>
        private static Breadcrumbs PrivateBreadcrumbs { get; set; }

        internal static Breadcrumbs CurrentBreadcrumbs() {
            // Copy of current PrivateBreadcrumbs
            return PrivateBreadcrumbs.Copy();
        }

        internal static object lockObject=new Object();
        internal static volatile bool initialized=false;

        /// <summary>
        /// Gets or sets the identifier of the application.
        /// </summary>
        /// <value> The identifier of the application. </value>
        internal static string AppID { get; set; }

        internal static AppLocator appLocator { get; private set; }

        /// <summary>
        /// Gets or sets the operating system platform.
        /// </summary>
        /// <value> The operating system platform. </value>
        internal static string OSVersion="";

        /// <summary>
        /// Gets or sets the arbitrary user metadata.
        /// </summary>
        /// <value> The user metadata. </value>
        private static Dictionary<string, string> Metadata { get; set; }

        private static Dictionary<string,string> CurrentMetadata() {
            // Copy of current Metadata
            return new Dictionary<string,string>(Metadata);
        }

        /// <summary> 
        /// Message Counter
        /// </summary>
        internal static int messageCounter = 0;

        /// <summary> 
        /// The initial date
        /// </summary>
        internal static DateTime initialDate = DateTime.UtcNow;

        /// <summary>
        /// The thread for the reader
        /// </summary>
#if NETFX_CORE
        internal static Task readerThread=null;
#else
        internal static Thread readerThread = null;
#endif

        /// <summary>
        /// AutoResetEvent for readerThread to observe
        /// </summary>
        internal static AutoResetEvent readerEvent = new AutoResetEvent(false);

        #endregion Properties

        #region OptOutStatus

        ////////////////////////////////////////////////////////////////
        // Developer's app only needs to OptOut once in it's life time.
        // To ever undo this, SetOptOutStatus(false) before calling
        // Init again.
        ////////////////////////////////////////////////////////////////

        // OptOut is internal for test cleanup, OW only 2 methods in
        // class Crittercism.cs should be touching member variable OptOut directly.
        internal static volatile bool OptOut=false;

        // Is OptOut known to be equal to what's persisted on disk?
        internal static volatile bool OptOutLoaded=false;

        private static string OptOutStatusPath="Crittercism\\OptOutStatus.js";
        private static void SaveOptOutStatus(bool optOutStatus) {
            // Knows how to persist value of OptOut
            StorageHelper.Save(Convert.ToBoolean(optOutStatus),OptOutStatusPath);
        }

        private static bool LoadOptOutStatus() {
            // Knows how to unpersist value of OptOut
            bool answer=false;
            if (StorageHelper.FileExists(OptOutStatusPath)) {
                answer=(bool)StorageHelper.Load(OptOutStatusPath,typeof(Boolean));
            };
            return answer;
        }

        public static bool GetOptOutStatus() {
            // Returns in memory cached value OptOut, getting it to be correct
            // value from persisted storage first, if necessary.
            if (!OptOutLoaded) {
                // Logic here to make sure OptOut is unpersisted correctly from
                // any possible previous session.  App is born with default OptOut
                // value equal to false (Crittercism enabled).
                lock (lockObject) {
                    // Check flag again inside lock in case our thread loses race.
                    if (!OptOutLoaded) {
                        OptOut=LoadOptOutStatus();
                        OptOutLoaded=true;
                    };
                };
            };
            return OptOut;
        }

        public static void SetOptOutStatus(bool optOut) {
            // Set in memory cached value OptOut, persisting if necessary.
            lock (lockObject) {
                // OptOut is volatile, but this method accesses it twice,
                // so we need the lock
                if (optOut!=GetOptOutStatus()) {
                    OptOut=optOut;
                    SaveOptOutStatus(optOut);
                }
            }
        }

        #endregion OptOutStatus

        #region Life Cycle
        private static string LoadAppVersion() {
            string answer = "UNKNOWN";
            try {
#if NETFX_CORE
                PackageVersion version = Package.Current.Id.Version;
                answer = "" + version.Major + "." + version.Minor + "." + version.Build + "." + version.Revision;
#elif WINDOWS_PHONE
                answer = Application.Current.GetType().Assembly.GetName().Version.ToString();
#else
                // Should probably work in most cases.
                Assembly assembly = Assembly.GetEntryAssembly();
                if (assembly != null) {
                    AssemblyName assemblyName = assembly.GetName();
                    if (assemblyName != null) {
                        Version version = assemblyName.Version;
                        if (version != null) {
                            answer = version.ToString();
                        }
                    }
                }
#endif
            } catch (Exception) {
                // Return "UNKNOWN" if anything throws an Exception .
            };
            Debug.WriteLine("LoadAppVersion == " + answer);
            return answer;
        }

        /// <summary>
        /// Retrieves the device id from storage.
        /// 
        /// If we don't have a device id, we create and store a new one.
        /// </summary>
        /// <returns>String with device_id, null otherwise</returns>
        private static string LoadDeviceId() {
            string deviceId=null;
            string path=Path.Combine(StorageHelper.CrittercismPath(),"DeviceId.js");
            try {
                if (StorageHelper.FileExists(path)) {
                    deviceId=(string)StorageHelper.Load(path,typeof(String));
                }
            } catch (Exception ie) {
                LogInternalException(ie);
            }
            if (deviceId==null) {
                try {
                    deviceId=Guid.NewGuid().ToString();
                    StorageHelper.Save(deviceId,path);
                } catch (Exception ie) {
                    LogInternalException(ie);
                    // if deviceId==null is returned, then Crittercism should say
                    // it wasn't able to initialize
                }
            }
            Debug.WriteLine("LoadDeviceId --> "+deviceId);
            return deviceId;
        }

        private static string LoadDeviceModel() {
            // TODO: We wish this method could be a lot better.
#if NETFX_CORE
#if WINDOWS_PHONE_APP
            return "Windows Phone";
#else
            return "Windows PC";
#endif // WINDOWS_PHONE_APP
#elif WINDOWS_PHONE
            return DeviceStatus.DeviceName;
#else
            return "Windows PC";
#endif // NETFX_CORE
        }

        internal static Dictionary<string,string> LoadMetadata() {
            Dictionary<string,string> answer=null;
            try {
                string path=Path.Combine(StorageHelper.CrittercismPath(),"Metadata.js");
                if (StorageHelper.FileExists(path)) {
                    answer=(Dictionary<string,string>)StorageHelper.Load(
                        path,
                        typeof(Dictionary<string,string>));
                }
            } catch (Exception ie) {
                LogInternalException(ie);
            }
            if (answer==null) {
                answer=new Dictionary<string,string>();
            }
            Debug.WriteLine("LoadMetadata: "+JsonConvert.SerializeObject(answer));
            return answer;
        }

        private static bool SaveMetadata() {
            bool answer=false;
            try {
                Debug.WriteLine("SaveMetadata: "+JsonConvert.SerializeObject(Metadata));
                string path=Path.Combine(StorageHelper.CrittercismPath(),"Metadata.js");
                answer=StorageHelper.Save(Metadata,path);
            } catch (Exception ie) {
                Crittercism.LogInternalException(ie);
            };
            return answer;
        }

        private static string LoadOSVersion() {
#if NETFX_CORE
            // TODO: Returning an empty string here makes us sad.
            // "You cannot get the OS or .NET framework version in a Windows Store app ...
            // Marked as answer by Anne Jing Microsoft contingent staff, Moderator"
            // https://social.msdn.microsoft.com/Forums/sqlserver/en-US/66e662a9-9ece-4863-8cf1-a5e259c7b571/c-windows-store-8-os-version-name-and-net-version-name
            string answer="";
#else
            string answer=Environment.OSVersion.Platform.ToString();
#endif
            return answer;
        }

        /// <summary>
        /// Retrieves the session id from storage.
        /// 
        /// If we don't have a session id, we create and store a new one.
        /// </summary>
        /// <returns>The session id is a positive integer.</returns>
        private static long LoadSessionId() {
            // API Protocol doesn't specify this, but 1 is consistent with iOS SDK's choice.
            const long FIRST_SESSION_NUMBER=1;
            long sessionId=FIRST_SESSION_NUMBER-1;
            string path=Path.Combine(StorageHelper.CrittercismPath(),"SessionId.js");
            try {
                if (StorageHelper.FileExists(path)) {
                    sessionId=(long)StorageHelper.Load(path,typeof(long));
                }
            } catch (Exception ie) {
                LogInternalException(ie);
            }
            try {
                if (sessionId<FIRST_SESSION_NUMBER) {
                    sessionId=FIRST_SESSION_NUMBER;
                } else {
                    sessionId++;
                }
                StorageHelper.Save(sessionId,path);
            } catch (Exception ie) {
                LogInternalException(ie);
            }
            Debug.WriteLine("LoadSessionId --> "+sessionId);
            return sessionId;
        }

        /// <summary>
        /// Initialises Crittercism.
        /// </summary>
        /// <param name="appID">  Identifier for the application. </param>
        public static void Init(string appID) {
            try {
                if (GetOptOutStatus()) {
                    return;
                } else if (initialized) {
                    DebugUtils.LOG_ERROR("Crittercism is already initialized");
                    return;
                };
                lock (lockObject) {
                    appLocator=new AppLocator(appID);
                    if (appLocator.domain==null) {
                        DebugUtils.LOG_ERROR("Illegal Crittercism appID");
                        return;
                    }
                    AppID=appID;
                    APM.Init();
                    MessageReport.Init();
                    TransactionReporter.Init();
                    AppVersion=LoadAppVersion();
                    DeviceId=LoadDeviceId();
                    DeviceModel=LoadDeviceModel();
                    Metadata=LoadMetadata();
                    OSVersion=LoadOSVersion();
                    SessionId=LoadSessionId();
                    QueueReader queueReader=new QueueReader();
#if NETFX_CORE
                    Action threadStart=() => { queueReader.ReadQueue(); };
                    readerThread=new Task(threadStart);
#else
                    ThreadStart threadStart=new ThreadStart(queueReader.ReadQueue);
                    readerThread=new Thread(threadStart);
                    readerThread.Name="Crittercism";
#endif
                    // enableSendMessage for unit test purposes
                    if (enableSendMessage) {
#if NETFX_CORE
                        Application.Current.UnhandledException+=Application_UnhandledException;
                        NetworkInformation.NetworkStatusChanged+=NetworkInformation_NetworkStatusChanged;
#elif WINDOWS_PHONE
                        Application.Current.UnhandledException+=new EventHandler<ApplicationUnhandledExceptionEventArgs>(SilverlightApplication_UnhandledException);
                        DeviceNetworkInformation.NetworkAvailabilityChanged+=DeviceNetworkInformation_NetworkAvailabilityChanged;
                        try {
                            if (PhoneApplicationService.Current!=null) {
                                PhoneApplicationService.Current.Activated+=new EventHandler<ActivatedEventArgs>(PhoneApplicationService_Activated);
                                PhoneApplicationService.Current.Deactivated+=new EventHandler<DeactivatedEventArgs>(PhoneApplicationService_Deactivated);
                            }
                        } catch (Exception ie) {
                            LogInternalException(ie);
                        }
#else
                        AppDomain.CurrentDomain.UnhandledException+=new UnhandledExceptionEventHandler(AppDomain_UnhandledException);
                        System.Windows.Forms.Application.ThreadException+=new ThreadExceptionEventHandler(WindowsFormsApplication_ThreadException);
#endif
                    };
                    PrivateBreadcrumbs=Breadcrumbs.SessionStart();
                    MessageQueue=new SynchronizedQueue<MessageReport>(new Queue<MessageReport>());
                    LoadQueue();
                    // NOTE: Put initialized=true before readerThread.Start() .
                    // Later on, initialized may be reset back to false during shutdown,
                    // and readerThread will see initialized==false as a message to exit.
                    // Spares us from creating an additional "shuttingdown" flag.
                    initialized=true;
                };
                readerThread.Start();
                CreateAppLoadReport();
            } catch (Exception) {
                initialized=false;
            }
            if (initialized) {
                Debug.WriteLine("Crittercism initialized.");
            } else {
                Debug.WriteLine("Crittercism did not initialize.");
            }
        }

        internal static void Save() {
            // Save current Crittercism state
            try {
                lock (lockObject) {
                    Debug.WriteLine("Save: SAVE STATE");
                    PrivateBreadcrumbs.Save();
                    SaveMetadata();
                    foreach (MessageReport message in MessageQueue) {
                        message.Save();
                    }
                }
            } catch (Exception ie) {
                LogInternalException(ie);
            }
        }

        /// <summary>
        /// Shuts down Crittercism.
        /// </summary>
        public static void Shutdown() {
            // Shutdown Crittercism, including readerThread .
            Debug.WriteLine("Shutdown");
            try {
                if (initialized) {
                    lock (lockObject) {
                        if (initialized) {
                            initialized=false;
                            // Stop the producers
                            APM.Shutdown();
                            TransactionReporter.Shutdown();
                            // Get the readerThread to exit.
                            readerEvent.Set();
#if NETFX_CORE
                            readerThread.Wait();
#else
                            readerThread.Join();
#endif
                            // Save state.
                            Save();
                        }
                    }
                }
            } catch (Exception ie) {
                LogInternalException(ie);
            }
        }
        #endregion Shutdown

        #region AppLoads
        /// <summary>
        /// Creates the application load report.
        /// </summary>
        private static void CreateAppLoadReport() {
            if (GetOptOutStatus()) {
                return;
            }
            AppLoad appLoad=new AppLoad();
            AddMessageToQueue(appLoad);
        }
        #endregion AppLoads

        #region Breadcrumbs
        /// <summary>
        /// Leave breadcrumb.
        /// </summary>
        /// <param name="breadcrumb">   The breadcrumb. </param>
        public static void LeaveBreadcrumb(string breadcrumb) {
            if (GetOptOutStatus()) {
            } else if (!initialized) {
                Debug.WriteLine(errorNotInitialized);
            } else {
                try {
                    PrivateBreadcrumbs.LeaveBreadcrumb(breadcrumb);
                } catch (Exception ie) {
                    LogInternalException(ie);
                }
            }
        }
        #endregion Breadcrumbs

        #region Exceptions and Crashes
        internal static void LogInternalException(Exception e) {
            Debug.WriteLine("UNEXPECTED ERROR!!! "+e.Message);
            Debug.WriteLine(e.StackTrace);
            Debug.WriteLine("");
        }

        private static string StackTrace(Exception e) {
            // Allowing for the fact that the "name" and "reason" of the outermost
            // exception e are already shown in the Crittercism portal, we don't
            // need to repeat that bit of info.  However, for InnerException's, we
            // will include this information in the StackTrace .  The horizontal
            // lines (hyphens) separate InnerException's from each other and the
            // outermost Exception e .
            string answer=e.StackTrace;
            // Using seen for cycle detection to break cycling.
            List<Exception> seen=new List<Exception>();
            seen.Add(e);
            if (answer!=null) {
                // There has to be some way of telling where InnerException ie stacktrace
                // ends and main Exception e stacktrace begins.  This is it.
                answer=((e.GetType().FullName+" : "+e.Message+"\r\n")
                    +answer);
                Exception ie=e.InnerException;
                while ((ie!=null)&&(seen.IndexOf(ie)<0)) {
                    seen.Add(ie);
                    answer=((ie.GetType().FullName+" : "+ie.Message+"\r\n")
                        +(ie.StackTrace+"\r\n")
                        +answer);
                    ie=ie.InnerException;
                }
            } else {
                answer="";
            }
            return answer;
        }

        /// <summary>
        /// Creates handled exception report.
        /// </summary>
        public static void LogHandledException(Exception e) {
            if (GetOptOutStatus()) {
            } else if (!initialized) {
                DebugUtils.LOG_ERROR(errorNotInitialized);
            } else {
                try {
                    lock (lockObject) {
                        Dictionary<string,string> metadata=CurrentMetadata();
                        Breadcrumbs breadcrumbs=CurrentBreadcrumbs();
                        string stacktrace=StackTrace(e);
                        ExceptionObject exception=new ExceptionObject(e.GetType().FullName,e.Message,stacktrace);
                        HandledException he=new HandledException(AppID,metadata,breadcrumbs,exception);
                        AddMessageToQueue(he);
                    }
                } catch (Exception ie) {
                    LogInternalException(ie);
                }
            }
        }
        
        /// <summary>
        /// Creates a crash report.
        /// </summary>
        /// <param name="currentException"> The current exception. </param>
        public static void LogUnhandledException(Exception e) {
            if (initialized) {
                // Seems Windows Forms apps can generate unhandled exceptions
                // without really crashing.  For Windows Forms apps, we're only
                // reporting the first one in a given session.  This is why
                // we're checking "initialized".
                Dictionary<string,string> metadata=CurrentMetadata();
                Breadcrumbs breadcrumbs=CurrentBreadcrumbs();
                string stacktrace=StackTrace(e);
                ExceptionObject exception=new ExceptionObject(e.GetType().FullName,e.Message,stacktrace);
                Crash crash=new Crash(AppID,metadata,breadcrumbs,exception);
                // Add crash to message queue and save state .
                Shutdown();
                AddMessageToQueue(crash);
                Save();
                // App is probably going to crash now, because we choose not
                // to handle the unhandled exception ourselves and typically
                // most apps will choose to log the exception (e.g. with Crittercism)
                // but let the crash go ahead.
            }
        }
        #endregion Exceptions and Crashes

        #region Metadata
        /// <summary>
        /// Sets "username" metadata value.
        /// </summary>
        /// <param name="username"> The username. </param>
        public static void SetUsername(string username) {
            // SetValue will check GetOptOutStatus() and initialized .
            SetValue("username",username);
        }

        /// <summary>
        /// Gets "username" metadata value.
        /// </summary>
        public static string Username() {
            // ValueFor will check GetOptOutStatus() and initialized .
            return ValueFor("username");
        }

        /// <summary>
        /// Sets a user metadata value.
        /// </summary>
        /// <param name="key">      The key. </param>
        /// <param name="value">    The value. </param>
        public static void SetValue(string key,string value) {
            if (GetOptOutStatus()) {
            } else if (!initialized) {
                DebugUtils.LOG_ERROR(errorNotInitialized);
            } else {
                try {
                    lock (lockObject) {
                        if (!Metadata.ContainsKey(key)||!Metadata[key].Equals(value)) {
                            if (value==null) {
                                Metadata.Remove(key);
                            } else {
                                Metadata[key]=value;
                            }
                            MetadataReport metadataReport=new MetadataReport(
                                AppID,new Dictionary<string,string>(Metadata));
                            AddMessageToQueue(metadataReport);
                        }
                    }
                } catch (Exception ie) {
                    LogInternalException(ie);
                }
            }
        }

        /// <summary>
        /// Returns a user metadata value.
        /// </summary>
        /// <param name="key">      The key. </param>
        public static string ValueFor(string key) {
            string answer=null;
            if (GetOptOutStatus()) {
            } else if (!initialized) {
                DebugUtils.LOG_ERROR(errorNotInitialized);
            } else {
                try {
                    lock (lockObject) {
                        if (Metadata.ContainsKey(key)) {
                            answer=Metadata[key];
                        }
                    }
                } catch (Exception ie) {
                    LogInternalException(ie);
                }
            }
            return answer;
        }
        #endregion Metadata

        #region Transactions
        public static void BeginTransaction(string name) {
            // Init and begin a transaction with a default value.
            try {
                CancelTransaction(name);
                // Do not begin a new transaction if the transaction count is at or has exceeded the max.
                if (TransactionReporter.TransactionCount() >= TransactionReporter.MAX_TRANSACTION_COUNT) {
                    DebugUtils.LOG_ERROR(String.Format(("Crittercism only supports a maximum of {0} concurrent transactions."
                                                       + "\r\nIgnoring Crittercism.BeginTransaction() call for {1}."),
                                                       TransactionReporter.MAX_TRANSACTION_COUNT,name));
                    return;
                }
                (new Transaction(name)).Begin();
            } catch (Exception ie) {
                Crittercism.LogInternalException(ie);
            }
        }
        public static void BeginTransaction(string name,int value) {
            // Init and begin a transaction with an input value.
            try {
                CancelTransaction(name);
                // Do not begin a new transaction if the transaction count is at or has exceeded the max.
                if (TransactionReporter.TransactionCount() >= TransactionReporter.MAX_TRANSACTION_COUNT) {
                    DebugUtils.LOG_ERROR(String.Format(("Crittercism only supports a maximum of {0} concurrent transactions."
                                                       + "\r\nIgnoring Crittercism.BeginTransaction() call for {1}."),
                                                       TransactionReporter.MAX_TRANSACTION_COUNT,name));
                    return;
                }
                (new Transaction(name,value)).Begin();
            } catch (Exception ie) {
                Crittercism.LogInternalException(ie);
            }
        }
        public static void CancelTransaction(string name) {
            // Cancel a transaction as if it never was.
            try {
                Transaction transaction = Transaction.TransactionForName(name);
                if (transaction != null) {
                    transaction.Cancel();
                } else {
                    CantFindTransaction(name);
                }
            } catch (Exception ie) {
                Crittercism.LogInternalException(ie);
            }
        }
        public static void EndTransaction(string name) {
            // End an already begun transaction successfully.
            try {
                Transaction transaction = Transaction.TransactionForName(name);
                if (transaction!=null) {
                    transaction.End();
                } else {
                    CantFindTransaction(name);
                }
            } catch (Exception ie) {
                Crittercism.LogInternalException(ie);
            }
        }
        public static void FailTransaction(string name) {
            // End an already begun transaction as a failure.
            try {
                Transaction transaction = Transaction.TransactionForName(name);
                if (transaction != null) {
                    transaction.Fail();
                } else {
                    CantFindTransaction(name);
                }
            } catch (Exception ie) {
                Crittercism.LogInternalException(ie);
            }
        }
        public static int GetTransactionValue(string name) {
            // Get the currency cents value of a transaction.
            int answer = 0;
            try {
                Transaction transaction = Transaction.TransactionForName(name);
                if (transaction != null) {
                    answer = transaction.Value();
                } else {
                    CantFindTransaction(name);
                }
            } catch (Exception ie) {
                Crittercism.LogInternalException(ie);
            }
            return answer;
        }
        public static void SetTransactionValue(string name,int value) {
            // Set the currency cents value of a transaction.
            try {
                Transaction transaction = Transaction.TransactionForName(name);
                if (transaction != null) {
                    transaction.SetValue(value);
                } else {
                    CantFindTransaction(name);
                }
            } catch (Exception ie) {
                Crittercism.LogInternalException(ie);
            }
        }

        internal static void CantFindTransaction(string name) {
#if NETFX_CORE || WINDOWS_PHONE
#else
            Trace.WriteLine(String.Format("Can't find transaction named \"{0}\"",name));
#endif
        }

        #endregion

        #region Network Requests
        public static void LogNetworkRequest(
            string method,
            string uriString,
            long latency,      // milliseconds
            long bytesRead,
            long bytesSent,
            HttpStatusCode statusCode,
            WebExceptionStatus exceptionStatus
        ) {
            if (GetOptOutStatus()) {
            } else if (!initialized) {
                DebugUtils.LOG_ERROR(errorNotInitialized);
            } else {
                try {
                    Debug.WriteLine(
                        "LogNetworkRequest({0},{1} ,{2},{3},{4},{5},{6})",
                        method,
                        uriString,
                        latency,
                        bytesRead,
                        bytesSent,
                        statusCode,
                        exceptionStatus);
                    if (APM.IsFiltered(uriString)) {
                        Debug.WriteLine("APM FILTERED: "+uriString);
                    } else {
                        APMEndpoint endpoint=new APMEndpoint(method,
                            uriString,
                            latency,
                            bytesRead,
                            bytesSent,
                            statusCode,
                            exceptionStatus);
                        APM.Enqueue(endpoint);
                    }
                } catch (Exception ie) {
                    LogInternalException(ie);
                }
            }
        }
#endregion LogNetworkRequest

        #region Configuring Service Monitoring
        public static void AddFilter(CRFilter filter) {
            if (GetOptOutStatus()) {
            } else if (!initialized) {
                DebugUtils.LOG_ERROR(errorNotInitialized);
            } else {
                try {
                    APM.AddFilter(filter);
                } catch (Exception ie) {
                    LogInternalException(ie);
                }
            }
        }

        public static void RemoveFilter(CRFilter filter) {
            if (GetOptOutStatus()) {
            } else if (!initialized) {
                DebugUtils.LOG_ERROR(errorNotInitialized);
            } else {
                try {
                    APM.RemoveFilter(filter);
                } catch (Exception ie) {
                    LogInternalException(ie);
                }
            }
        }
        #endregion

        #region MessageQueue
        /// <summary>
        /// Loads the messages from disk into the queue.
        /// </summary>
        private static void LoadQueue() {
            List<MessageReport> messages=MessageReport.LoadMessages();
            foreach (MessageReport message in messages) {
                // I'm wondering if we needed to restrict to 50 message of something similar?
                MessageQueue.Enqueue(message);
            }
        }

        private const int MaxMessageQueueCount=100;

        /// <summary>
        /// Adds message to queue
        /// </summary>
        internal static void AddMessageToQueue(MessageReport message) {
            while (MessageQueue.Count>=MaxMessageQueueCount) {
                // Sacrifice an oldMessage
                MessageReport oldMessage=MessageQueue.Dequeue();
                oldMessage.Delete();
            }
            MessageQueue.Enqueue(message);
            readerEvent.Set();
        }
        #endregion // MessageQueue

        #region Event Handlers

#if NETFX_CORE
#pragma warning disable 1998
        private static async void Application_UnhandledException(object sender,UnhandledExceptionEventArgs args) {
            if (GetOptOutStatus()) {
                return;
            }
            try {
                LogUnhandledException(args.Exception);
            } catch (Exception ie) {
                LogInternalException(ie);
            }
        }

        static void NetworkInformation_NetworkStatusChanged(object sender) {
            if (GetOptOutStatus()) {
                return;
            }
            try {
                Debug.WriteLine("NetworkStatusChanged");
                ConnectionProfile profile=NetworkInformation.GetInternetConnectionProfile();
                bool isConnected=(profile!=null
                    &&(profile.GetNetworkConnectivityLevel()==NetworkConnectivityLevel.InternetAccess));
                if (isConnected) {
                    if (MessageQueue!=null&&MessageQueue.Count>0) {
                        readerEvent.Set();
                    }
                }
            } catch (Exception ie) {
                LogInternalException(ie);
            }
        }
#elif WINDOWS_PHONE
        /// <summary>
        /// Event handler. Called by Current for unhandled exception events.
        /// </summary>
        /// <param name="sender">   Source of the event. </param>
        /// <param name="e">        Application unhandled exception event information. </param>
        static void SilverlightApplication_UnhandledException(object sender,ApplicationUnhandledExceptionEventArgs args) {
            if (GetOptOutStatus()) {
                return;
            }
            try {
                LogUnhandledException((Exception)args.ExceptionObject);
            } catch (Exception ie) {
                LogInternalException(ie);
            }
        }

        static void PhoneApplicationService_Activated(object sender, ActivatedEventArgs e) {
            ////////////////////////////////////////////////////////////////
            // The Windows Phone execution model allows only one app to run in the
            // foreground at a time. When the user navigates away from an app, the
            // app is typically put in a dormant state. In a dormant state, the
            // app's code no longer executes, but the app remains in memory. When
            // the user presses the Back button to return to a dormant app, it resumes
            // running and its state is automatically restored. It is possible,
            // however, for an app to be tombstoned after the user navigates away.
            // If the user navigates back to a tombstoned app, the app must restore
            // its own state because it is no longer in memory.
            // https://msdn.microsoft.com/en-us/library/windows/apps/ff967547(v=vs.105).aspx
            ////////////////////////////////////////////////////////////////
            if (GetOptOutStatus()) {
                return;
            }
            try {
                if (!e.IsApplicationInstancePreserved) {
                    // App was tombstoned.  State lost.
                    if (PhoneApplicationService.Current.State.ContainsKey("Crittercism.AppID")) {
                        // Take this to mean that Crittercism was Init'd in this app prior to
                        // a Deactivate.  Restart Crittercism asynchronously.
                        BackgroundWorker backgroundWorker=new BackgroundWorker();
                        backgroundWorker.DoWork+=new DoWorkEventHandler(BackgroundWorker_DoWork);
                        backgroundWorker.RunWorkerAsync();
                    }
                }
            } catch (Exception ie) {
                LogInternalException(ie);
            }
        }

        static void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Init((string)PhoneApplicationService.Current.State["Crittercism.AppID"]);
        }

        static void PhoneApplicationService_Deactivated(object sender, DeactivatedEventArgs e)
        {
            if (GetOptOutStatus()) {
                return;
            }
            try {
                PhoneApplicationService.Current.State["Crittercism.AppID"] = AppID;
            } catch (Exception ie) {
                LogInternalException(ie);
            }
        }

        static void DeviceNetworkInformation_NetworkAvailabilityChanged(object sender,NetworkNotificationEventArgs e) {
            if (GetOptOutStatus()) {
                return;
            }
            try {
                switch (e.NotificationType) {
                    case NetworkNotificationType.InterfaceConnected:
                        if (NetworkInterface.GetIsNetworkAvailable()) {
                            if (MessageQueue!=null&&MessageQueue.Count>0) {
                                readerEvent.Set();
                            }
                        }
                        break;
                }
            } catch (Exception ie) {
                LogInternalException(ie);
            }
        }
#else
        static void AppDomain_UnhandledException(object sender,UnhandledExceptionEventArgs args) {
            if (GetOptOutStatus()) {
                return;
            }
            try {
                LogUnhandledException((Exception)args.ExceptionObject);
            } catch (Exception ie) {
                LogInternalException(ie);
            }
        }

        private static void WindowsFormsApplication_ThreadException(object sender,ThreadExceptionEventArgs t) {
            ////////////////////////////////////////////////////////////////
            // Crittercism unhandled exception handler for Windows Forms apps.
            // Crittercism users must add
            //     Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            // to their Program.cs Main() .
            // MSDN: "Application.SetUnhandledExceptionMode Method (UnhandledExceptionMode)
            // Call SetUnhandledExceptionMode before you instantiate the main form
            // of your application using the Run method.
            // To catch exceptions that occur in threads not created and owned by
            // Windows Forms, use the UnhandledException event handler."
            // https://msdn.microsoft.com/en-us/library/ms157905(v=vs.110).aspx
            ////////////////////////////////////////////////////////////////
            if (GetOptOutStatus()) {
                return;
            }
            try {
                LogUnhandledException(t.Exception);
            } catch (Exception e) {
                LogInternalException(e);
            }
        }
#endif

        #endregion // Event Handlers
    }
}
