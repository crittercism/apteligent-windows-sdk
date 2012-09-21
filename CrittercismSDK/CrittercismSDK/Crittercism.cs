// file:	CrittercismSDK\Crittercism.cs
// summary:	Implements the crittercism class
namespace CrittercismSDK
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO.IsolatedStorage;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using CrittercismSDK.DataContracts;
#if WINDOWS_PHONE
    using System.Windows;
    using Microsoft.Phone.Shell;
#endif

    /// <summary>
    /// Crittercism.
    /// </summary>
    public class Crittercism
    {
        #region Properties

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

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value> The key. </value>
        internal static string Key { get; set; }

        /// <summary>
        /// Gets or sets the secret.
        /// </summary>
        /// <value> The secret. </value>
        internal static string Secret { get; set; }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        /// <value> The username. </value>
        internal static string Username { get; set; }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        /// <value> The email. </value>
        internal static string Email { get; set; }

        /// <summary>
        /// Gets or sets the gender.
        /// </summary>
        /// <value> The gender. </value>
        internal static string Gender { get; set; }

        /// <summary>
        /// Gets or sets the age.
        /// </summary>
        /// <value> The age. </value>
        internal static int Age { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the device.
        /// </summary>
        /// <value> The identifier of the device. </value>
        internal static string DeviceId { get; set; }

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

        /// <summary>
        /// Initialises this object.
        /// </summary>
        /// <param name="appID">  Identifier for the application. </param>
        /// <param name="key">    The key. </param>
        /// <param name="secret"> The secret. </param>
        public static void Init(string appID, string key, string secret)
        {
            QueueReader queueReader = new QueueReader();
            ThreadStart threadStart = new ThreadStart(queueReader.ReadQueue);
            readerThread = new Thread(threadStart);
            readerThread.Name = "Crittercism Sender";
            StartApplication(appID, key, secret);

#if WINDOWS_PHONE
            Application.Current.UnhandledException += new EventHandler<ApplicationUnhandledExceptionEventArgs>(Current_UnhandledException);
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
            Username = username;
        }

        /// <summary>
        /// Sets an email.
        /// </summary>
        /// <param name="email">    The email. </param>
        public static void SetEmail(string email)
        {
            Email = email;
        }

        /// <summary>
        /// Sets a gender.
        /// </summary>
        /// <param name="gender">   The gender. </param>
        public static void SetGender(string gender)
        {
            Gender = gender;
        }

        /// <summary>
        /// Sets an age.
        /// </summary>
        /// <param name="age">  The age. </param>
        public static void SetAge(int age)
        {
            Age = age;
        }

        /// <summary>
        /// Sets an arbitrary user metadata value.
        /// </summary>
        /// <param name="value">    The value. </param>
        /// <param name="key">      The key. </param>
        public static void SetValue(string value, string key)
        {
            lock (ArbitraryUserMetadata)
            {
                ArbitraryUserMetadata.Add(key, value);
            }
        }

        /// <summary>
        /// Leave breadcrum.
        /// </summary>
        /// <param name="breadcrumb">   The breadcrumb. </param>
        public static void LeaveBreadcrum(string breadcrumb)
        {
            lock (CurrentBreadcrumbs)
            {
                CurrentBreadcrumbs.current_session.Add(new string[] { breadcrumb, DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ssK", System.Globalization.CultureInfo.InvariantCulture) });
                CurrentBreadcrumbs.SaveToDisk();
            }
        }

        /// <summary>
        /// Creates the error report.
        /// </summary>
        public static void CreateErrorReport(Exception e)
        {
            AppState appState = new AppState();
            List<ExceptionObject> exceptions = new List<ExceptionObject>() { new ExceptionObject("1.0", e.GetType().Name, e.Message, appState, e.StackTrace) };
            Error error = new Error(AppID, OSPlatform, DeviceId, System.Reflection.Assembly.GetExecutingAssembly().FullName.Split('=')[1].Split(',')[0].ToString(), exceptions);
            error.SaveToDisk();
            AddMessageToQueue(error);
        }
        
        /// <summary>
        /// Creates a crash report.
        /// </summary>
        /// <param name="currentException"> The current exception. </param>
        private static void CreateCrashReport(Exception currentException)
        {
            Breadcrumbs breadcrumbs = new Breadcrumbs();
            breadcrumbs.current_session = new List<string[]>(CurrentBreadcrumbs.current_session);
            breadcrumbs.previous_session = new List<string[]>(CurrentBreadcrumbs.previous_session);
            Crash crash = new Crash(AppID, OSPlatform, breadcrumbs, DeviceId, currentException.GetType().Name, currentException.Message, System.Reflection.Assembly.GetExecutingAssembly().FullName.Split('=')[1].Split(',')[0].ToString(), currentException.StackTrace);
            crash.SaveToDisk();
            AddMessageToQueue(crash);
            CurrentBreadcrumbs.previous_session = new List<string[]>(CurrentBreadcrumbs.current_session);
            CurrentBreadcrumbs.current_session.Clear();
        }

        /// <summary>
        /// Creates the application load report.
        /// </summary>
        private static void CreateAppLoadReport()
        {
            AppLoad appLoad = new AppLoad(AppID, DeviceId, System.Reflection.Assembly.GetExecutingAssembly().FullName.Split('=')[1].Split(',')[0].ToString(), OSPlatform);
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
                foreach(MessageReport message in messages)
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
            else
            {
                message.DeleteFromDisk();
            }
        }

        /// <summary>
        /// This method is invoked when the application starts or resume
        /// </summary>
        /// <param name="appID">    Identifier for the application. </param>
        /// <param name="key">      The key. </param>
        /// <param name="secret">   The secret. </param>
        private static void StartApplication(string appID, string key, string secret)
        {
            AppID = appID;
            Key = key;
            Secret = secret;
            CurrentBreadcrumbs = Breadcrumbs.GetBreadcrumbs();
            DeviceId = AppLoadResponse.GetDeviceId();
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
            StartApplication(AppID, Key, Secret);
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
            StartApplication((string)PhoneApplicationService.Current.State["Crittercism.AppID"], (string)PhoneApplicationService.Current.State["Crittercism.Key"], (string)PhoneApplicationService.Current.State["Crittercism.Secret"]);
        }

        static void Current_Deactivated(object sender, DeactivatedEventArgs e)
        {
            PhoneApplicationService.Current.State.Add("Crittercism.AppID", AppID);
            PhoneApplicationService.Current.State.Add("Crittercism.Key", Key);
            PhoneApplicationService.Current.State.Add("Crittercism.Secret", Secret);
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