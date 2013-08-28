// file:	CrittercismSDK\Crittercism.cs
// summary:	Implements the crittercism class
namespace CrittercismSDK
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using CrittercismSDK.DataContracts;
#if WINDOWS_PHONE
    using System.Windows;
    using Microsoft.Phone.Shell;
    using Microsoft.Phone.Net.NetworkInformation;
#endif

    /// <summary>
    /// Crittercism.
    /// </summary>
    public class Crittercism
    {
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

        /// <summary>
        /// Gets or sets a queue of messages.
        /// </summary>
        /// <value> A Queue of messages. </value>
        internal static Queue<MessageReport> MessageQueue { get; set; }

        /// <summary>
        /// Gets or sets the current breadcrumbs.
        /// </summary>
        /// <value> The breadcrumbs. </value>
        internal static Breadcrumbs CurrentBreadcrumbs { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the application.
        /// </summary>
        /// <value> The identifier of the application. </value>
        internal static string AppID { get; set; }


        internal static bool OptOut {get; set; }

        /// <summary>
        /// Gets or sets the identifier of the device.
        /// </summary>
        /// <value> The identifier of the device. </value>
        ////internal static string DeviceId
        ////{
        ////    get
        ////    {
        ////        try
        ////        {
        ////            // get application settings for isolate storage
        ////            var appSettings = System.IO.IsolatedStorage.IsolatedStorageSettings.ApplicationSettings;
        ////            Dictionary<string, string> crittercismSettings = null;

        ////            // if there not exist a dictionary for crittercism settings then create one with the device id as unique guid
        ////            if (!appSettings.Contains("CrittercismSettings"))
        ////            {
        ////                crittercismSettings = new Dictionary<string, string>();
        ////                crittercismSettings.Add("DeviceId", System.Guid.NewGuid().ToString());
        ////                appSettings.Add("CrittercismSettings", crittercismSettings);
        ////            }
        ////            else
        ////            {
        ////                // if the settings already exist just get them.
        ////                crittercismSettings = appSettings["CrittercismSettings"] as Dictionary<string, string>;
        ////            }

        ////            // if I have settings and there is a value for the device id return it, it should be because when the settings are created it is automatically set
        ////            // else add a new device id and return it. The end user can modify this settings because it is store on the application settings that is accessible for him
        ////            if (crittercismSettings != null)
        ////            {
        ////                if (!crittercismSettings.ContainsKey("DeviceId"))
        ////                {
        ////                    crittercismSettings.Add("DeviceId", System.Guid.NewGuid().ToString());
        ////                }

        ////                return crittercismSettings["DeviceId"] as string;
        ////            }
        ////        }
        ////        catch
        ////        {
        ////            // eat any possible crash, to avoid the dll to stack overflow
        ////        }

        ////        // return a empty string in case of error.
        ////        return string.Empty;
        ////    }
        ////}

        /// <summary>
        /// Gets or sets the operating system platform.
        /// </summary>
        /// <value> The operating system platform. </value>
        internal static string OSPlatform { get; set; }

        /// <summary>
        /// Gets or sets the arbitrary user metadata.
        /// </summary>
        /// <value> The user metadata. </value>
        internal static Dictionary<string, string> ArbitraryUserMetadata { get; set; }

        /// <summary>
        /// Folder name for the messages files
        /// </summary>
        internal static string FolderName = "CrittercismMessages";

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
        internal static Thread readerThread = null;

        #endregion

        #region Methods

        static Crittercism()
        {
            ArbitraryUserMetadata = new Dictionary<string, string>();
        }

        /// <summary>
        /// Initialises this object.
        /// </summary>
        /// <param name="appID">  Identifier for the application. </param>
        public static void Init(string appID) {
            OptOut = CheckOptOutFromDisk();
            QueueReader queueReader = new QueueReader();
            ThreadStart threadStart = new ThreadStart(queueReader.ReadQueue);
            readerThread = new Thread(threadStart);
            readerThread.Name = "Crittercism Sender";
            StartApplication(appID);

#if WINDOWS_PHONE
            if (_autoRunQueueReader && _enableCommunicationLayer && !(_enableRaiseExceptionInCommunicationLayer))  // for unit test purposes
            {
                Application.Current.UnhandledException += new EventHandler<ApplicationUnhandledExceptionEventArgs>(Current_UnhandledException);
                DeviceNetworkInformation.NetworkAvailabilityChanged += DeviceNetworkInformation_NetworkAvailabilityChanged;
                try
                {
                    if (PhoneApplicationService.Current != null)
                    {
                        PhoneApplicationService.Current.Activated += new EventHandler<ActivatedEventArgs>(Current_Activated);
                        PhoneApplicationService.Current.Deactivated += new EventHandler<DeactivatedEventArgs>(Current_Deactivated);
                    }
                }
                catch
                {
                }
            }
#else
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            System.Windows.Application.Current.Activated += new EventHandler(Current_Activated);
#endif
        }

        /// <summary>
        /// Sets a username.
        /// </summary>
        /// <param name="username"> The username. </param>
        public static void SetUsername(string username)
        {
            SetValue("username", username);
        }

        /// <summary>
        /// Sets an arbitrary user metadata value.
        /// </summary>
        /// <param name="key">      The key. </param>
        /// <param name="value">    The value. </param>
        public static void SetValue(string key, string value)
        {
            lock (ArbitraryUserMetadata)
            {
                ArbitraryUserMetadata[key] = value;
            }
        }

        public static bool GetOptOutStatus()
        {
            return OptOut;
        }

        public static void SetOptOutStatus(bool optOut)
        {
            if (optOut == OptOut)
            {
                return; // mission accomplished
            }
            OptOut = optOut;
            SetOptOutOnDisk(optOut);
        }

        private static readonly string CrittercismConfigFolder = "CrittercismConfig";
        private static readonly string CrittercismOptOutFile = CrittercismConfigFolder + "\\" + "OptOut.txt";
        private static void SetOptOutOnDisk(bool optOut)
        {
            IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication();
            if (optOut)
            {
                if (!storage.DirectoryExists(CrittercismConfigFolder))
                {
                    storage.CreateDirectory(CrittercismConfigFolder);
                }
                if (!storage.FileExists(CrittercismOptOutFile))
                {
                    using (IsolatedStorageFileStream optOutFile = new IsolatedStorageFileStream(CrittercismOptOutFile, FileMode.Create, FileAccess.Write, storage))
                    {
                        optOutFile.Close();
                    }
                }
            }
            else
            {
                if (storage.FileExists(CrittercismOptOutFile))
                {
                    storage.DeleteFile(CrittercismOptOutFile);
                }
            }

        }

        internal static bool CheckOptOutFromDisk()
        {
            try
            {
                IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication();
                return storage.FileExists(CrittercismOptOutFile);
            }
            catch
            {
                // swallow, best effort
                return false;
            }
        }

        /// <summary>
        /// Leave breadcrum.
        /// </summary>
        /// <param name="breadcrumb">   The breadcrumb. </param>
        public static void LeaveBreadcrumb(string breadcrumb)
        {
            lock (CurrentBreadcrumbs)
            {
                CurrentBreadcrumbs.current_session.Add(new BreadcrumbMessage(breadcrumb));
                CurrentBreadcrumbs.SaveToDisk();
            }
        }

        /// <summary>
        /// Creates the error report.
        /// </summary>
        public static void LogHandledException(Exception e)
        {
            if (OptOut)
            {
                return;
            }
            var appVersion = System.Windows.Application.Current.GetType().Assembly.GetName().Version.ToString();
            Breadcrumbs breadcrumbs = new Breadcrumbs();
            breadcrumbs.current_session = new List<BreadcrumbMessage>(CurrentBreadcrumbs.current_session);
            breadcrumbs.previous_session = new List<BreadcrumbMessage>(CurrentBreadcrumbs.previous_session);
            ExceptionObject exception = new ExceptionObject(e.GetType().FullName, e.Message, e.StackTrace);
            Error error = new Error(AppID, appVersion, new Dictionary<string,string>(ArbitraryUserMetadata), breadcrumbs, exception);
            error.SaveToDisk();
            AddMessageToQueue(error);
        }
        
        /// <summary>
        /// Creates a crash report.
        /// </summary>
        /// <param name="currentException"> The current exception. </param>
        internal static void CreateCrashReport(Exception currentException)
        {
            if (OptOut)
            {
                return;
            }
            var appVersion = System.Windows.Application.Current.GetType().Assembly.GetName().Version.ToString();
            Breadcrumbs breadcrumbs = new Breadcrumbs();
            breadcrumbs.current_session = new List<BreadcrumbMessage>(CurrentBreadcrumbs.current_session);
            breadcrumbs.previous_session = new List<BreadcrumbMessage>(CurrentBreadcrumbs.previous_session);
            ExceptionObject exception = new ExceptionObject(currentException.GetType().FullName, currentException.Message, currentException.StackTrace);
            Crash crash = new Crash(AppID, appVersion, new Dictionary<string,string>(ArbitraryUserMetadata), breadcrumbs, exception);
            crash.SaveToDisk();
            AddMessageToQueue(crash);
            CurrentBreadcrumbs.previous_session = new List<BreadcrumbMessage>(CurrentBreadcrumbs.current_session);
            CurrentBreadcrumbs.current_session.Clear();
        }

        /// <summary>
        /// Creates the application load report.
        /// </summary>
        private static void CreateAppLoadReport()
        {
            if (OptOut)
            {
                return;
            }
            var appVersion = System.Windows.Application.Current.GetType().Assembly.GetName().Version.ToString();
            // the following code doesn't work because the executing assembly is the same crittercimswp8sdk in WP8 ... 
            // var appVersion = System.Reflection.Assembly.GetExecutingAssembly().FullName.Split('=')[1].Split(',')[0].ToString();

            var appLoad = new AppLoad(AppID, appVersion);

            appLoad.SaveToDisk();
            AddMessageToQueue(appLoad);
        }

        /// <summary>
        /// Loads the messages from disk into the queue.
        /// </summary>
        private static void LoadQueueFromDisk()
        {
            IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication();
            if (storage.DirectoryExists(FolderName))
            {
                string[] fileNames = storage.GetFileNames(FolderName + "\\*");
                List<MessageReport> messages = new List<MessageReport>();
                foreach (string file in fileNames)
                {
                    string[] fileSplit = file.Split('_');
                    MessageReport message = null;
                    switch (fileSplit[0])
                    {
                        case "AppLoad":
                            message = new AppLoad();
                            break;

                        case "Error":
                            message = new Error();
                            break;

                        default:
                            message = new Crash();
                            break;
                    }

                    message.Name = file;
                    message.CreationDate = storage.GetCreationTime(FolderName + "\\" + file);
                    message.IsLoaded = false;
                    messages.Add(message);
                }

                messages.Sort((m1, m2) => m1.CreationDate.CompareTo(m2.CreationDate));
                foreach (MessageReport message in messages)
                {
                    // I'm wondering if we needed to restrict to 50 message of something similar?
                    MessageQueue.Enqueue(message);
                }
            }
        }

        /// <summary>
        /// Adds  message to queue
        /// </summary>
        private static void AddMessageToQueue(MessageReport message)
        {
            if (DateTime.Now.Subtract(initialDate) <= new TimeSpan(0, 0, 0, 1, 0))
            {
                messageCounter++;
            }
            else
            {
                messageCounter = 0;
                initialDate = DateTime.Now;
            }

            if (messageCounter < 50)
            {
                MessageQueue.Enqueue(message);
                if (_autoRunQueueReader)  // This flag is for unit test
                {
                    if (readerThread.ThreadState == ThreadState.Unstarted)
                    {
                        readerThread.Start();
                    }
                    else if (readerThread.ThreadState == ThreadState.Stopped || readerThread.ThreadState == ThreadState.Aborted)
                    {
                        QueueReader queueReader = new QueueReader();
                        ThreadStart threadStart = new ThreadStart(queueReader.ReadQueue);
                        readerThread = new Thread(threadStart);
                        readerThread.Name = "Crittercism Sender";
                        readerThread.Start();
                    }
                }
            }
            else
            {
                message.DeleteFromDisk();
            }
        }

        /// <summary>
        /// This method is invoked when the application starts or resume
        /// </summary>
        /// <param name="appID">    Identifier for the application. </param>
        private static void StartApplication(string appID)
        {
            AppID = appID;
            CurrentBreadcrumbs = Breadcrumbs.GetBreadcrumbs();
            OSPlatform = Environment.OSVersion.Platform.ToString();
            MessageQueue = new Queue<MessageReport>();
            LoadQueueFromDisk();
            CreateAppLoadReport();
        }

        /// <summary>
        /// This method is invoked when the application resume
        /// </summary>
        private static void StartApplication()
        {
            StartApplication(AppID);
        }

#if WINDOWS_PHONE
        /// <summary>
        /// Event handler. Called by Current for unhandled exception events.
        /// </summary>
        /// <param name="sender">   Source of the event. </param>
        /// <param name="e">        Application unhandled exception event information. </param>
        static void Current_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            CreateCrashReport((Exception)e.ExceptionObject);
            e.Handled = true;
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
            PhoneApplicationService.Current.State.Add("Crittercism.AppID", AppID);
        }

        static void DeviceNetworkInformation_NetworkAvailabilityChanged(object sender, NetworkNotificationEventArgs e)
        {
            if (_autoRunQueueReader)  // This flag is for unit test
            {
                switch (e.NotificationType)
                {
                    case NetworkNotificationType.InterfaceConnected:
                        if (NetworkInterface.GetIsNetworkAvailable())
                        {
                            if (MessageQueue != null && MessageQueue.Count > 0)
                            {
                                if (readerThread.ThreadState == ThreadState.Unstarted)
                                {
                                    readerThread.Start();
                                }
                                else if (readerThread.ThreadState == ThreadState.Stopped || readerThread.ThreadState == ThreadState.Aborted)
                                {
                                    QueueReader queueReader = new QueueReader();
                                    ThreadStart threadStart = new ThreadStart(queueReader.ReadQueue);
                                    readerThread = new Thread(threadStart);
                                    readerThread.Name = "Crittercism Sender";
                                    readerThread.Start();
                                }
                            }
                        }

                        break;
                }
            }
        }
#else
        /// <summary>
        /// Event handler. Called by CurrentDomain for unhandled exception events.
        /// </summary>
        /// <param name="sender"> Source of the event. </param>
        /// <param name="e">      Unhandled exception event information. </param>
        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            CreateCrashReport((Exception)e.ExceptionObject);
        }

        /// <summary>
        /// Event handler. Called by Current for activated events.
        /// </summary>
        /// <param name="sender">   Source of the event. </param>
        /// <param name="e">        Event information. </param>
        static void Current_Activated(object sender, EventArgs e)
        {
            BackgroundWorker backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += new DoWorkEventHandler(backgroundWorker_DoWork);
            backgroundWorker.RunWorkerAsync();
        }

        static void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            StartApplication();
        }
#endif

        #endregion
    }
}