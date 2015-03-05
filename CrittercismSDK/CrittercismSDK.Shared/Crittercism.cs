// file:	CrittercismSDK\Crittercism.cs
// summary:	Implements the crittercism class
namespace CrittercismSDK {
    using CrittercismSDK.DataContracts;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
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
#endif

    /// <summary>
    /// Crittercism.
    /// </summary>
    public class Crittercism {
        #region Constants
        private const string errorNotInitialized="ERROR: Crittercism not initialized yet.";
        #endregion

        #region Properties
        /// <summary>
        /// The auto run queue reader
        /// </summary>
        internal static bool _autoRunQueueReader = true;

        /// <summary>
        /// The enable communication layer
        /// </summary>
        internal static bool _enableCommunicationLayer = true;

        /// <summary>
        /// The enable raise exception in communication layer
        /// </summary>
        internal static bool _enableRaiseExceptionInCommunicationLayer = false;

        internal static string AppVersion { get; private set; }
        internal static string DeviceId { get; private set; }
        internal static string DeviceModel { get; private set; }

        /// <summary>
        /// Gets or sets a queue of messages.
        /// </summary>
        /// <value> A SynchronizedQueue of messages. </value>
        internal static SynchronizedQueue<MessageReport> MessageQueue { get; set; }

        /// <summary>
        /// Gets or sets the current breadcrumbs.
        /// </summary>
        /// <value> The breadcrumbs. </value>
        internal static Breadcrumbs PrivateBreadcrumbs { get; set; }

        private static Breadcrumbs CurrentBreadcrumbs() {
            Breadcrumbs answer=PrivateBreadcrumbs.Copy();
            return answer;
        }

        internal static object lockObject=new Object();
        internal static volatile bool initialized=false;

        /// <summary>
        /// Gets or sets the identifier of the application.
        /// </summary>
        /// <value> The identifier of the application. </value>
        internal static string AppID { get; set; }

        internal static AppLocator appLocator { get; private set; }

        internal static volatile bool OptOut;

        /// <summary>
        /// Gets or sets the operating system platform.
        /// </summary>
        /// <value> The operating system platform. </value>
        internal static string OSVersion="";

        /// <summary>
        /// Gets or sets the arbitrary user metadata.
        /// </summary>
        /// <value> The user metadata. </value>
        internal static Dictionary<string, string> Metadata { get; set; }

        private static Dictionary<string,string> CurrentMetadata() {
            Dictionary<string,string> answer=null;
            lock (lockObject) {
                answer=new Dictionary<string,string>(Metadata);
            }
            return answer;
        }

        /// <summary> 
        /// Message Counter
        /// </summary>
        internal static int messageCounter = 0;

        /// <summary> 
        /// The initial date
        /// </summary>
        internal static DateTime initialDate = DateTime.Now;

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

        #endregion

        #region Methods

        static Crittercism() {
            // Developer's app only needs to OptOut once in it's life time.
            // To ever undo this, SetOptOutStatus(false) before calling
            // Init again.
            OptOut=LoadOptOutStatus();
        }

        private static string PrivateAppVersion() {
#if NETFX_CORE
            PackageVersion version=Package.Current.Id.Version;
            string answer=""+version.Major+"."+version.Minor+"."+version.Build+"."+version.Revision;
            Debug.WriteLine("PrivateAppVersion == "+answer);
            return answer;
#else
            // Note that GetExecutingAssembly wouldn't work because we (Crittercism) *are* the executing assembly
            return Application.Current.GetType().Assembly.GetName().Version.ToString();
#endif
        }

        /// <summary>
        /// Retrieves the device id from storage.
        /// 
        /// If we don't have a device id, we create and store a new one.
        /// </summary>
        /// <returns>String with device_id, null otherwise</returns>
        private static string PrivateDeviceId() {
            string deviceId=null;
            const string path="DeviceId.js";
            try {
                if (StorageHelper.FileExists(path)) {
                    deviceId=(string)StorageHelper.Load(path,typeof(String));
                }
            } catch (Exception e) {
                Crittercism.LogInternalException(e);
            }
            if (deviceId==null) {
                try {
                    deviceId=Guid.NewGuid().ToString();
                    StorageHelper.Save(deviceId,path);
                } catch (Exception e) {
                    Crittercism.LogInternalException(e);
                    // if deviceId==null is returned, then Crittercism should say
                    // it wasn't able to initialize
                }
            }
            Debug.WriteLine("LoadDeviceId --> "+deviceId);
            return deviceId;
        }

        private static string PrivateDeviceModel() {
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
            const string path="Metadata.js";
            if (StorageHelper.FileExists(path)) {
                answer=(Dictionary<string,string>)StorageHelper.Load(
                     path,
                     typeof(Dictionary<string,string>)
                 );
            }
            if (answer==null) {
                answer=new Dictionary<string,string>();
            }
            return answer;
        }

        /// <summary>
        /// Initialises this object.
        /// </summary>
        /// <param name="appID">  Identifier for the application. </param>
        public static void Init(string appID) {
            try {
                if (OptOut) {
                    // SaveOptOutStatus must have been called in this session
                    // and OptOut = true is already persisted.
                    return;
                } else if (initialized) {
                    Debug.WriteLine("ERROR: Crittercism is already initialized");
                    return;
                };
                lock (lockObject) {
                    AppVersion=PrivateAppVersion();
                    DeviceId=PrivateDeviceId();
                    DeviceModel=PrivateDeviceModel();
                    Metadata=LoadMetadata();
                    appLocator=new AppLocator(appID);
                    QueueReader queueReader=new QueueReader(appLocator);
#if NETFX_CORE
                    Action threadStart=() => { queueReader.ReadQueue(); };
                    readerThread=new Task(threadStart);
#else
                    ThreadStart threadStart=new ThreadStart(queueReader.ReadQueue);
                    readerThread=new Thread(threadStart);
                    readerThread.Name="Crittercism Sender";
#endif
                    readerThread.Start();
                    StartApplication(appID);
                    // _autoRunQueueReader for unit test purposes
                    if (_autoRunQueueReader&&_enableCommunicationLayer&&!(_enableRaiseExceptionInCommunicationLayer)) {
#if NETFX_CORE
                        Application.Current.UnhandledException+=Current_UnhandledException;
                        NetworkInformation.NetworkStatusChanged+=NetworkInformation_NetworkStatusChanged;
#elif WINDOWS_PHONE
                        Application.Current.UnhandledException+=new EventHandler<ApplicationUnhandledExceptionEventArgs>(Current_UnhandledException);
                        DeviceNetworkInformation.NetworkAvailabilityChanged+=DeviceNetworkInformation_NetworkAvailabilityChanged;
                        try {
                            if (PhoneApplicationService.Current!=null) {
                                PhoneApplicationService.Current.Activated+=new EventHandler<ActivatedEventArgs>(Current_Activated);
                                PhoneApplicationService.Current.Deactivated+=new EventHandler<DeactivatedEventArgs>(Current_Deactivated);
                            }
                        } catch (Exception e) {
                            Crittercism.LogInternalException(e);
                        }
#else
                        AppDomain currentDomain=AppDomain.CurrentDomain;
                        currentDomain.UnhandledException+=new UnhandledExceptionEventHandler(Current_UnhandledException);
#endif
                    }
                    initialized=true;
                    Debug.WriteLine("Crittercism initialized.");
                }
            } catch (Exception) {
            }
            if (!initialized) {
                Debug.WriteLine("Crittercism did not initialize.");
            }
        }

        /// <summary>
        /// Sets "username" metadata value.
        /// </summary>
        /// <param name="username"> The username. </param>
        public static void SetUsername(string username) {
            // SetValue will check OptOut and initialized .
            SetValue("username", username);
        }

        /// <summary>
        /// Gets "username" metadata value.
        /// </summary>
        public static string Username() {
            // ValueFor will check OptOut and initialized .
            return ValueFor("username");
        }

        /// <summary>
        /// Sets a user metadata value.
        /// </summary>
        /// <param name="key">      The key. </param>
        /// <param name="value">    The value. </param>
        public static void SetValue(string key,string value) {
            if (OptOut) {
                return;
            } else if (!initialized) {
                Debug.WriteLine(errorNotInitialized);
                return;
            }
            try {
                lock (lockObject) {
                    if (!Metadata.ContainsKey(key)||!Metadata[key].Equals(value)) {
                        Metadata[key]=value;
                        UserMetadata metadata=new UserMetadata(
                            AppID,new Dictionary<string,string>(Metadata));
                        AddMessageToQueue(metadata);
                    }
                }
            } catch (Exception e) {
                Crittercism.LogInternalException(e);
                // explicit nop
            }
        }

        /// <summary>
        /// Returns a user metadata value.
        /// </summary>
        /// <param name="key">      The key. </param>
        public static string ValueFor(string key) {
            if (OptOut) {
                return null;
            } else if (!initialized) {
                Debug.WriteLine(errorNotInitialized);
                return null;
            };
            string answer=null;
            lock (lockObject) {
                if (Metadata.ContainsKey(key)) {
                    answer=Metadata[key];
                }
            }
            return answer;
        }

        public static bool GetOptOutStatus() {
            return OptOut;
        }

        public static void SetOptOutStatus(bool optOut) {
            // OptOut is volatile, but this method accesses it twice,
            // so we need the lock
            lock (lockObject) {
                if (optOut!=OptOut) {
                    OptOut=optOut;
                    SaveOptOutStatus(optOut);
                }
            }
        }

        internal static void Save() {
            // Save current Crittercism state
            try {
                lock (lockObject) {
                    Debug.WriteLine("Save: SAVE STATE");
                    foreach (MessageReport message in MessageQueue) {
                        message.Save();
                    }
                    PrivateBreadcrumbs.Save();
                }
            } catch (Exception e) {
                LogInternalException(e);
            }
        }

        private static string optOutStatusPath="OptOutStatus.js";
        private static void SaveOptOutStatus(bool optOut) {
            StorageHelper.Save(Convert.ToBoolean(optOut),optOutStatusPath);
        }

        internal static bool LoadOptOutStatus() {
            bool answer=false;
            if (StorageHelper.FileExists(optOutStatusPath)) {
                answer=(bool)StorageHelper.Load(optOutStatusPath,typeof(Boolean));
            };
            return answer;
        }

        /// <summary>
        /// Leave breadcrum.
        /// </summary>
        /// <param name="breadcrumb">   The breadcrumb. </param>
        public static void LeaveBreadcrumb(string breadcrumb) {
            if (OptOut) {
                return;
            } else if (!initialized) {
                Debug.WriteLine(errorNotInitialized);
                return;
            };
            PrivateBreadcrumbs.LeaveBreadcrumb(breadcrumb);
        }

        /// <summary>
        /// Creates crash report.
        /// </summary>
        public static void LogCrash(Exception exception) {
            // TODO: Are we going to document this?  What's the reason?
            if (OptOut) {
                return;
            } else if (!initialized) {
                Debug.WriteLine(errorNotInitialized);
                return;
            };
            try {
                CreateCrashReport(exception);
            } catch (Exception e) {
                Crittercism.LogInternalException(e);
                // explicit nop
            }
        }

        internal static void LogInternalException(Exception e) {
            Debug.WriteLine("UNEXPECTED ERROR!!! "+e.Message);
            Debug.WriteLine(e.StackTrace);
            Debug.WriteLine("");
        }

        /// <summary>
        /// Creates handled exception report.
        /// </summary>
        public static void LogHandledException(Exception e) {
            if (OptOut) {
                return;
            } else if (!initialized) {
                Debug.WriteLine(errorNotInitialized);
                return;
            };
            Dictionary<string,string> metadata=CurrentMetadata();
            Breadcrumbs breadcrumbs=CurrentBreadcrumbs();
            string stacktrace=e.StackTrace;
            if (stacktrace==null) {
                // Assuming the Exception e being passed in hasn't been thrown.  In this case,
                // supply our own current "stacktrace".  The mscorlib System.Diagnostics.Stacktrace
                // isn't available for Windows Store library.  Instead, generate our own stacktrace
                // string by throwing and catching our own Exception.
                try {
                    throw new Exception();
                } catch (Exception e2) {
                    stacktrace=e2.StackTrace;
                }
            }
            ExceptionObject exception=new ExceptionObject(e.GetType().FullName,e.Message,stacktrace);
            HandledException he=new HandledException(AppID,metadata,breadcrumbs,exception);
            AddMessageToQueue(he);
        }
        
        /// <summary>
        /// Creates a crash report.
        /// </summary>
        /// <param name="currentException"> The current exception. </param>
        internal static void CreateCrashReport(Exception currentException) {
            Dictionary<string,string> metadata=CurrentMetadata();
            Breadcrumbs breadcrumbs=PrivateBreadcrumbs.Crash();
            ExceptionObject exception=new ExceptionObject(currentException.GetType().FullName,currentException.Message,currentException.StackTrace);
            Crash crash=new Crash(AppID,metadata,breadcrumbs,exception);
            // It seems reasonable to assume crashes occur so seldomly, but
            // are so important, that we'll make very sure these get Save'd
            // immediately, and save state .
            AddMessageToQueue(crash);
            crash.Save();
            PrivateBreadcrumbs.Save();
            Save();
            // App is probably going to crash now, because we choose not
            // to handle the unhandled exception ourselves and typically
            // most apps will choose to log the exception (e.g. with Crittercism)
            // but let the crash go ahead.
        }
        
        /// <summary>
        /// Creates the application load report.
        /// </summary>
        private static void CreateAppLoadReport() {
            if (OptOut) {
                return;
            }
            AppLoad appLoad=new AppLoad();
            AddMessageToQueue(appLoad);
        }

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
        private static void AddMessageToQueue(MessageReport message) {
            while (MessageQueue.Count>=MaxMessageQueueCount) {
                // Sacrifice an oldMessage
                MessageReport oldMessage=MessageQueue.Dequeue();
                oldMessage.Delete();
            }
            MessageQueue.Enqueue(message);
            readerEvent.Set();
        }

        private static string PrivateOSVersion() {
#if NETFX_CORE
            // TODO: We don't want to return hardcoded string here.
            string answer="8.1";
#else
            string answer=Environment.OSVersion.Platform.ToString();
#endif
            return answer;
        }

        /// <summary>
        /// This method is invoked when the application starts or resume
        /// </summary>
        /// <param name="appID">    Identifier for the application. </param>
        private static void StartApplication(string appID)
        {
            // TODO: Why do we pass appID arg to this method?
            AppID=appID;
            OSVersion=PrivateOSVersion();
            PrivateBreadcrumbs = Breadcrumbs.LoadBreadcrumbs();
            MessageQueue = new SynchronizedQueue<MessageReport>(new Queue<MessageReport>());
            LoadQueue();
            CreateAppLoadReport();
        }

#if NETFX_CORE
#pragma warning disable 1998
        private static async void Current_UnhandledException(object sender,UnhandledExceptionEventArgs args) {
            Debug.WriteLine("Current_UnhandledException ENTER");
            Exception e=args.Exception;
            Debug.WriteLine("Current_UnhandledException e.Message == "+e.Message);
            Debug.WriteLine("Current_UnhandledException e.StackTrace == "+e.StackTrace);
            Crittercism.LogCrash(e);
            //args.Handled=true;
            Debug.WriteLine("Current_UnhandledException EXIT");
        }

        static void NetworkInformation_NetworkStatusChanged(object sender) {
            Debug.WriteLine("NetworkStatusChanged");
            ConnectionProfile profile=NetworkInformation.GetInternetConnectionProfile();
            bool isConnected=(profile!=null
                &&(profile.GetNetworkConnectivityLevel()==NetworkConnectivityLevel.InternetAccess));
            if (isConnected) {
                if (MessageQueue!=null&&MessageQueue.Count>0) {
                    readerEvent.Set();
                }
            }
        }
#elif WINDOWS_PHONE
        /// <summary>
        /// Event handler. Called by Current for unhandled exception events.
        /// </summary>
        /// <param name="sender">   Source of the event. </param>
        /// <param name="e">        Application unhandled exception event information. </param>
        static void Current_UnhandledException(object sender,ApplicationUnhandledExceptionEventArgs args) {
            try {
                CreateCrashReport((Exception)args.ExceptionObject);
            } catch (Exception e) {
                Crittercism.LogInternalException(e);
                // explicit nop
            }
        }
        static void Current_Activated(object sender, ActivatedEventArgs e)
        {
            BackgroundWorker backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += new DoWorkEventHandler(backgroundWorker_DoWork);
            backgroundWorker.RunWorkerAsync();
        }

        static void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            StartApplication((string)PhoneApplicationService.Current.State["Crittercism.AppID"]);
        }

        static void Current_Deactivated(object sender, DeactivatedEventArgs e)
        {
            PhoneApplicationService.Current.State["Crittercism.AppID"] = AppID;
        }

        static void DeviceNetworkInformation_NetworkAvailabilityChanged(object sender,NetworkNotificationEventArgs e) {
            // This flag is for unit test
            if (_autoRunQueueReader) {
                switch (e.NotificationType) {
                    case NetworkNotificationType.InterfaceConnected:
                        if (NetworkInterface.GetIsNetworkAvailable()) {
                            if (MessageQueue!=null&&MessageQueue.Count>0) {
                                readerEvent.Set();
                            }
                        }
                        break;
                }
            }
        }
#else
        static void Current_UnhandledException(object sender,UnhandledExceptionEventArgs args) {
            try {
                CreateCrashReport((Exception)args.ExceptionObject);
            } catch (Exception e) {
                Crittercism.LogInternalException(e);
                // explicit nop
            }
        }
#endif
        #endregion
    }
}
