using CrittercismSDK;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Networking.Connectivity;
using Windows.System.Threading;
using Windows.UI.Xaml.Navigation;
#elif WINDOWS_PHONE
using Microsoft.Phone.Info;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Net.NetworkInformation;
using Windows.Networking.Connectivity;
using Windows.System.Threading;
#else
using Microsoft.Win32;
using System.Net;
using System.Net.NetworkInformation;
using System.Timers;
#endif // NETFX_CORE

namespace CrittercismSDK {
    /// <summary>
    /// Crittercism.
    /// </summary>
    public class Crittercism {
        #region Constants
        // How often we should check for Internet access.
        private const int interval = 5000;  // milliseconds
        private const string errorNotInitialized = "Crittercism not initialized yet.";
        #endregion Constants

        #region Properties
        // The isForegrounded flag is used to prevent Root_UIElement_GotFocus from
        // reporting more than one "App Foreground" per background/foreground activity.
        // "App Load" when app launches prevents "App Foreground" too, so this flag
        // starts out being true.  "App Background" will make it false.
        internal static volatile bool isForegrounded = true;

        // Approximation to process start time if we can't do anything better (in some .NET's)
        internal static long StartTime = DateTime.UtcNow.Ticks;

        // For UnitTest
        internal static IMockNetwork TestNetwork = null;

        internal static string AppVersion { get; private set; }
        internal static string DeviceId { get; private set; }
        internal static string DeviceModel { get; private set; }

#if NETFX_CORE
        internal static string Version = typeof(Crittercism).GetTypeInfo().Assembly.GetName().Version.ToString();
#else
        internal static string Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
#endif

#if WINDOWS_PHONE
        internal static string Carrier = Microsoft.Phone.Net.NetworkInformation.
            DeviceNetworkInformation.CellularMobileOperator;
#else
        internal static string Carrier = "UNKNOWN";
#endif

        // NOTE: Appears platform won't allow "wp" to be replaced by "windows"
        // carte blanche.  Doing so prevents handled exception reports from
        // being accepted by platform.
        //#if WINDOWS_PHONE
        internal static readonly string OSName = "wp";
        //#else
        //        internal static readonly string OSName="windows";
        //#endif

        internal static long SessionId { get; private set; }

        internal static JObject Settings { get; set; }

        /// <summary>
        /// Gets or sets a queue of messages.
        /// </summary>
        /// <value> A SynchronizedQueue of messages. </value>
        internal static SynchronizedQueue<MessageReport> MessageQueue { get; set; }

        internal static object lockObject = new Object();
        internal static volatile bool initialized = false;

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
        internal static string OSVersion = "";

        /// <summary>
        /// Gets or sets the arbitrary user metadata.
        /// </summary>
        /// <value> The user metadata. </value>
        private static Dictionary<string,string> Metadata { get; set; }

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
        internal static Task readerThread = null;
#else
        internal static Thread readerThread = null;
#endif

        /// <summary>
        /// AutoResetEvent for readerThread to observe
        /// </summary>
        internal static AutoResetEvent readerEvent = new AutoResetEvent(false);

        #endregion Properties

        #region Events
        public static event EventHandler UserflowTimeOut;
        #endregion

        #region OptOutStatus

        ////////////////////////////////////////////////////////////////
        // Developer's app only needs to OptOut once in it's life time.
        // To ever undo this, SetOptOutStatus(false) before calling
        // Init again.
        ////////////////////////////////////////////////////////////////

        // OptOut is internal for test cleanup, OW only 2 methods in
        // class Crittercism.cs should be touching member variable OptOut directly.
        internal static volatile bool OptOut = false;

        // Is OptOut known to be equal to what's persisted on disk?
        internal static volatile bool OptOutLoaded = false;
        // Have we hooked WINDOWS_PHONE Application.Current.RootVisual GotFocus and LostFocus events yet?
        internal static volatile bool IsRootUIElementFocusHooked = false;

        private static string OptOutStatusPath = "Crittercism\\OptOutStatus.js";
        private static void SaveOptOutStatus(bool optOutStatus) {
            // Knows how to persist value of OptOut
            StorageHelper.Save(Convert.ToBoolean(optOutStatus),OptOutStatusPath);
        }

        private static bool LoadOptOutStatus() {
            // Knows how to unpersist value of OptOut
            bool answer = false;
            if (StorageHelper.FileExists(OptOutStatusPath)) {
                answer = (bool)StorageHelper.Load(OptOutStatusPath,typeof(Boolean));
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
                        OptOut = LoadOptOutStatus();
                        OptOutLoaded = true;
                    };
                };
            };
#if WINDOWS_PHONE_APP || WINDOWS_PHONE
            // NOTE: If we could figure out a way (we can't), we would put a shorter
            // bit of code to be called by Crittercism.Init into WINDOWS_PHONE apps
            // that would get root.GotFocus and root.LostFocus events hooked early
            // on, much more like what are able to do for NETFX_CORE .  However, it
            // turns out root == null is going to be the normal case for Crittercism.Init
            // during a WINDOWS_PHONE app launch.  So, we must use more creative code.
            if ((!OptOut) && (!IsRootUIElementFocusHooked)) {
                HookRootUIElementFocus();
            };
#endif
            return OptOut;
        }

#if NETFX_CORE || WINDOWS_PHONE
        private static void HookRootUIElementFocus() {
            try {
                lock (lockObject) {
                    // Check flag again inside lock in case our thread loses race.
                    if (!IsRootUIElementFocusHooked) {
                        UIElement root = null;
#if NETFX_CORE
                        if (Window.Current != null) {
                            // If there is a current Window (don't ask us why we're checking this)
                            root = Window.Current.Content as UIElement;
                        }
#elif WINDOWS_PHONE
                        if (Application.Current != null) {
                            // We are unaware of any Application.Current == null possibilities,
                            // but it doesn't cost us much to be paranoid here.
                            if (Thread.CurrentThread.ManagedThreadId == 1) {
                                // Testing we're on the main thread.  OW, we might get a
                                // System.UnauthorizedAccessException (translation: "you are
                                // not on the main thread").
                                root = Application.Current.RootVisual as UIElement;
                            }
                        }
#endif
                        if (root != null) {
                            // This may be assuming our users are accepting and not modifying the
                            // MS generated "new Frame" just once part of the MS boiler plate code.
                            root.GotFocus += Root_UIElement_GotFocus;
                            root.LostFocus += Root_UIElement_LostFocus;
                            IsRootUIElementFocusHooked = true;
#if NETFX_CORE
                            if (root is Frame) {
                                // We monitor Navigated event to record "View" automatic breadcrumbs.
                                ((Frame)root).Navigated += Root_UIElement_Navigated;
                                // Artificially get the very first automatic "View" breadcrumb.
                                Root_UIElement_Navigated(root, null);
                            }
#endif
                        }
                    };
                };
            } catch (Exception) {
            };
        }
#endif // WINDOWS_PHONE_APP || WINDOWS_PHONE

        public static void SetOptOutStatus(bool optOut) {
            // Set in memory cached value OptOut, persisting if necessary.
            lock (lockObject) {
                // OptOut is volatile, but this method accesses it twice,
                // so we need the lock
                if (optOut != GetOptOutStatus()) {
                    OptOut = optOut;
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
            string deviceId = null;
            string path = Path.Combine(StorageHelper.CrittercismPath(),"DeviceId.js");
            try {
                if (StorageHelper.FileExists(path)) {
                    deviceId = (string)StorageHelper.Load(path,typeof(String));
                }
            } catch (Exception ie) {
                LogInternalException(ie);
            }
            if (deviceId == null) {
                try {
                    deviceId = Guid.NewGuid().ToString();
                    StorageHelper.Save(deviceId,path);
                } catch (Exception ie) {
                    LogInternalException(ie);
                    // if deviceId==null is returned, then Crittercism should say
                    // it wasn't able to initialize
                }
            }
            Debug.WriteLine("LoadDeviceId --> " + deviceId);
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
            Dictionary<string,string> answer = null;
            try {
                string path = Path.Combine(StorageHelper.CrittercismPath(),"Metadata.js");
                if (StorageHelper.FileExists(path)) {
                    answer = (Dictionary<string,string>)StorageHelper.Load(
                        path,
                        typeof(Dictionary<string,string>));
                }
            } catch (Exception ie) {
                LogInternalException(ie);
            }
            if (answer == null) {
                answer = new Dictionary<string,string>();
            }
            Debug.WriteLine("LoadMetadata: " + JsonConvert.SerializeObject(answer));
            return answer;
        }

        private static bool SaveMetadata() {
            bool answer = false;
            try {
                Debug.WriteLine("SaveMetadata: " + JsonConvert.SerializeObject(Metadata));
                string path = Path.Combine(StorageHelper.CrittercismPath(),"Metadata.js");
                answer = StorageHelper.Save(Metadata,path);
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
            string answer = "";
#else
            string answer = Environment.OSVersion.Platform.ToString();
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
            const long FIRST_SESSION_NUMBER = 1;
            long sessionId = FIRST_SESSION_NUMBER - 1;
            string path = Path.Combine(StorageHelper.CrittercismPath(),"SessionId.js");
            try {
                if (StorageHelper.FileExists(path)) {
                    sessionId = (long)StorageHelper.Load(path,typeof(long));
                }
            } catch (Exception ie) {
                LogInternalException(ie);
            }
            try {
                if (sessionId < FIRST_SESSION_NUMBER) {
                    sessionId = FIRST_SESSION_NUMBER;
                } else {
                    sessionId++;
                }
                StorageHelper.Save(sessionId,path);
            } catch (Exception ie) {
                LogInternalException(ie);
            }
            Debug.WriteLine("LoadSessionId --> " + sessionId);
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
                    appLocator = new AppLocator(appID);
                    if (appLocator.domain == null) {
                        DebugUtils.LOG_ERROR("Illegal Crittercism appID");
                        return;
                    }
                    AppID = appID;
                    // Put default initialized Settings before APM.Init() and UserflowReporter.Init() .
                    Settings = LoadSettings();
                    APM.Init();
                    MessageReport.Init();
                    UserflowReporter.Init();
                    AppVersion = LoadAppVersion();
                    DeviceId = LoadDeviceId();
                    DeviceModel = LoadDeviceModel();
                    Metadata = LoadMetadata();
                    OSVersion = LoadOSVersion();
                    SessionId = LoadSessionId();
                    QueueReader queueReader = new QueueReader();
#if NETFX_CORE
                    Action threadStart = () => { queueReader.ReadQueue(); };
                    readerThread = new Task(threadStart);
#else
                    ThreadStart threadStart = new ThreadStart(queueReader.ReadQueue);
                    readerThread = new Thread(threadStart);
                    readerThread.Name = "Crittercism";
#endif
                    // Testing for unit test purposes
                    if (Crittercism.TestNetwork == null) {
                        Breadcrumbs.HandleReachabilityUpDown(ReachabilityStatusString());
#if NETFX_CORE
#if WINDOWS_PHONE_APP
                        HookRootUIElementFocus();
#endif
                        Window.Current.VisibilityChanged += Window_VisibilityChanged;
                        Application.Current.UnhandledException += Application_UnhandledException;
                        NetworkInformation.NetworkStatusChanged += NetworkInformation_NetworkStatusChanged;
#elif WINDOWS_PHONE
                        HookRootUIElementFocus();
                        Application.Current.UnhandledException += new EventHandler<ApplicationUnhandledExceptionEventArgs>(SilverlightApplication_UnhandledException);
                        DeviceNetworkInformation.NetworkAvailabilityChanged += DeviceNetworkInformation_NetworkAvailabilityChanged;
                        try {
                            if (PhoneApplicationService.Current != null) {
                                PhoneApplicationService.Current.Activated += new EventHandler<ActivatedEventArgs>(PhoneApplicationService_Activated);
                                PhoneApplicationService.Current.Deactivated += new EventHandler<DeactivatedEventArgs>(PhoneApplicationService_Deactivated);
                            }
                        } catch (Exception ie) {
                            LogInternalException(ie);
                        }
#else
                        AppDomain.CurrentDomain.UnhandledException+=new UnhandledExceptionEventHandler(AppDomain_UnhandledException);
                        System.Windows.Forms.Application.ThreadException+=new ThreadExceptionEventHandler(WindowsFormsApplication_ThreadException);
                        NetworkChange.NetworkAvailabilityChanged += new NetworkAvailabilityChangedEventHandler(NetworkChange_NetworkAvailabilityChanged);
#endif
                    };
                    Breadcrumbs.UserBreadcrumbs();
                    MessageQueue = new SynchronizedQueue<MessageReport>(new Queue<MessageReport>());
                    LoadQueue();
                    // InstallTimer installs a timer that does occasional chores every
                    // once in a while, such as check if the app has Internet access .
                    InstallTimer();
                    // NOTE: Put initialized=true before readerThread.Start() .
                    // Later on, initialized may be reset back to false during shutdown,
                    // and readerThread will see initialized==false as a message to exit.
                    // Spares us from creating an additional "shuttingdown" flag.
                    initialized = true;
                };
                readerThread.Start();
                // It seems sensible to put CreateAppLoadReport here after all the
                // necessary Crittercism infrastructure to do so (including MessageQueue,
                // UserflowReporter, readerThread, etc.) have been initialized above.
                CreateAppLoadReport();
            } catch (Exception) {
                initialized = false;
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
                    Breadcrumbs.SaveAll();
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
                            initialized = false;
                            // Stop the producers
                            APM.Shutdown();
                            UserflowReporter.Shutdown();
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

        #region Timing
        ////////////////////////////////////////////////////////////////
        // OnTimerElapsed checks network connectivity every 5 seconds
        // when app is in foreground.  We find empirically that we don't
        // get MS events we expect would correspond to Crittercism's
        // "Connection DOWN" and "Connectivity LOST: ...".  We can get
        // these missing network automatic breadcrumbs by using our
        // OnTimerElapsed mechanism.
        ////////////////////////////////////////////////////////////////

        // Different .NET frameworks get different timer's
#if NETFX_CORE || WINDOWS_PHONE
        private static ThreadPoolTimer timer = null;
        private static void OnTimerElapsed(ThreadPoolTimer timer) {
            lock (lockObject) {
#if NETFX_CORE
                // This method acts like a NOP if there isn't an actual network change.
                // We're polling periodically because Microsoft's events aren't reliable.
                NetworkInformation_NetworkStatusChanged(null);
#elif WINDOWS_PHONE
                // This method acts like a NOP if there isn't an actual network change.
                // We're polling periodically because Microsoft's events aren't reliable.
                DeviceNetworkInformation_NetworkAvailabilityChanged(null,null);
#endif
            }
        }
#else
        private static System.Timers.Timer timer=null;
        private static void OnTimerElapsed(Object source, ElapsedEventArgs e) {
            lock (lockObject) {
                // This method acts like a NOP if there isn't an actual network change.
                // We're polling periodically because Microsoft's events aren't reliable.
                NetworkChange_NetworkAvailabilityChanged(null,EventArgs.Empty);
            }
        }
#endif // NETFX_CORE
        private static void InstallTimer() {
            Debug.WriteLine("InstallTimer");
            lock (lockObject) {
#if NETFX_CORE || WINDOWS_PHONE
                if (timer == null) {
                    // Creates a single-use timer.
                    // https://msdn.microsoft.com/en-US/library/windows/apps/windows.system.threading.threadpooltimer.aspx
                    Debug.WriteLine("InstallTimer ThreadPoolTimer.CreatePeriodicTimer");
                    timer = ThreadPoolTimer.CreatePeriodicTimer(
                        OnTimerElapsed,
                        TimeSpan.FromMilliseconds(interval));
                }
#else
                if (timer==null) {
                    // Generates an event after a set interval
                    // https://msdn.microsoft.com/en-us/library/system.timers.timer(v=vs.110).aspx
                    Debug.WriteLine("InstallTimer new Timer");
                    timer = new System.Timers.Timer(interval);
                    timer.Elapsed += OnTimerElapsed;
                    // the Timer keep raising the Elapsed event (true)
                    timer.AutoReset = true;         // keep firing
                    timer.Enabled = true;           // Start the timer
                }
#endif // NETFX_CORE
            }
        }
        private static void RemoveTimer() {
            // Call if we don't need the timer anymore.
            lock (lockObject) {
#if NETFX_CORE || WINDOWS_PHONE
                if (timer != null) {
                    timer.Cancel();
                    timer = null;
                }
#else
                if (timer!=null) {
                    timer.Stop();
                    timer = null;
                }
#endif // NETFX_CORE
            }
        }
        #endregion

        #region AppLoads
        /// <summary>
        /// Creates the application load report.
        /// </summary>
        private static void CreateAppLoadReport() {
            // Creates the application load report.
            if (GetOptOutStatus()) {
                return;
            }
            AppLoad appLoad = new AppLoad();
            AddMessageToQueue(appLoad);
            CreateAppLoadUserflow();
        }
        private static void CreateAppLoadUserflow() {
            // Automatic "App Load" Userflow
            long now = DateTime.UtcNow.Ticks;
#if NETFX_CORE || WINDOWS_PHONE
            long beginTime = StartTime;
#else
            long beginTime = Process.GetCurrentProcess().StartTime.ToUniversalTime().Ticks;
            if (now < beginTime) {
                // In case the beginTime from System.Diagnostics.Process is insane
                // for any reason.
                beginTime = StartTime;
            };
#endif
            long endTime = now;
            Debug.WriteLine("App Load time == " + (1.0E-7) * (endTime - beginTime) + " seconds");
            new Userflow("App Load",beginTime,endTime);
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
                    Breadcrumbs.LeaveUserBreadcrumb(breadcrumb,BreadcrumbTextType.Normal);
                } catch (Exception ie) {
                    LogInternalException(ie);
                }
            }
        }
        #endregion Breadcrumbs

        #region Exceptions and Crashes
        internal static void LogInternalException(Exception e) {
            Debug.WriteLine("UNEXPECTED ERROR!!! " + e.Message);
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
            string answer = e.StackTrace;
            // Using seen for cycle detection to break cycling.
            List<Exception> seen = new List<Exception>();
            seen.Add(e);
            if (answer != null) {
                // There has to be some way of telling where InnerException ie stacktrace
                // ends and main Exception e stacktrace begins.  This is it.
                answer = ((e.GetType().FullName + " : " + e.Message + "\r\n")
                    + answer);
                Exception ie = e.InnerException;
                while ((ie != null) && (seen.IndexOf(ie) < 0)) {
                    seen.Add(ie);
                    answer = ((ie.GetType().FullName + " : " + ie.Message + "\r\n")
                        + (ie.StackTrace + "\r\n")
                        + answer);
                    ie = ie.InnerException;
                }
            } else {
                answer = "";
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
                        Dictionary<string,string> metadata = CurrentMetadata();
                        UserBreadcrumbs breadcrumbs = Breadcrumbs.GetAllSessionsBreadcrumbs();
                        List<Endpoint> endpoints = Breadcrumbs.ExtractAllEndpoints();
                        List<Breadcrumb> systemBreadcrumbs = Breadcrumbs.SystemBreadcrumbs().RecentBreadcrumbs();
                        string stacktrace = StackTrace(e);
                        string exceptionName = e.GetType().FullName;
                        string exceptionReason = e.Message;
                        ExceptionObject exception = new ExceptionObject(exceptionName,exceptionReason,stacktrace);
                        HandledException he = new HandledException(AppID,metadata,breadcrumbs,endpoints,systemBreadcrumbs,exception);
                        AddMessageToQueue(he);
                        Breadcrumbs.LeaveErrorBreadcrumb(exceptionName,exceptionReason);
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
                Dictionary<string,string> metadata = CurrentMetadata();
                UserBreadcrumbs breadcrumbs = Breadcrumbs.GetAllSessionsBreadcrumbs();
                List<Endpoint> endpoints = Breadcrumbs.ExtractAllEndpoints();
                List<Breadcrumb> systemBreadcrumbs = Breadcrumbs.SystemBreadcrumbs().RecentBreadcrumbs();
                List<Userflow> userflows = UserflowReporter.CrashUserflows();
                string stacktrace = StackTrace(e);
                ExceptionObject exception = new ExceptionObject(e.GetType().FullName,e.Message,stacktrace);
                CrashReport crashReport = new CrashReport(AppID,metadata,breadcrumbs,endpoints,systemBreadcrumbs,userflows,exception);
                // Add crash to message queue and save state .
                Shutdown();
                AddMessageToQueue(crashReport);
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
                        if (!Metadata.ContainsKey(key) || !Metadata[key].Equals(value)) {
                            if (value == null) {
                                Metadata.Remove(key);
                            } else {
                                Metadata[key] = value;
                            }
                            MetadataReport metadataReport = new MetadataReport(
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
            string answer = null;
            if (GetOptOutStatus()) {
            } else if (!initialized) {
                DebugUtils.LOG_ERROR(errorNotInitialized);
            } else {
                try {
                    lock (lockObject) {
                        if (Metadata.ContainsKey(key)) {
                            answer = Metadata[key];
                        }
                    }
                } catch (Exception ie) {
                    LogInternalException(ie);
                }
            }
            return answer;
        }
        #endregion Metadata

        #region Settings
        internal static void SetSettings(string json) {
            // Called from AppLoad.DidReceiveResponse
            try {
                Debug.WriteLine("AppLoad response == " + json);
                // Checking for a sane "response"
                JObject response = null;
                try {
                    response = JToken.Parse(json) as JObject;
                } catch {
                };
                if (CheckSettings(response)) {
                    // This is an AppLoad response JSON we can apply to current session.
                    // TODO: Some "lock" goes here.  TBD.
                    Settings = response;
                    SaveSettings(json);
                    APM.SettingsChange();
                    UserflowReporter.SettingsChange();
                }
            } catch (Exception ie) {
                Crittercism.LogInternalException(ie);
            }
        }
        internal static bool CheckSettings(JObject settings) {
            // Minimum sanity test for "settings" received from platform.
            // Use the same sanity test that iOS SDK uses.  Look for pattern
            // {"apm":{"net":...},...}
            bool answer = false;
            if (settings != null) {
                // We get this far if received "settings" was a legal JSON object.
                JObject apm = settings["apm"] as JObject;
                if (apm != null) {
                    JObject net = apm["net"] as JObject;
                    if (net != null) {
                        // Good enough!
                        answer = true;
                    };
                };
            };
            return answer;
        }
        private static JObject LoadSettings() {
            // Load AppLoad response JSON from prior session.
            JObject answer = null;
            try {
                string path = Path.Combine(StorageHelper.CrittercismPath(),"Settings.js");
                if (StorageHelper.FileExists(path)) {
                    string json = StorageHelper.LoadString(path);
                    answer = JObject.Parse(json);
                }
            } catch (Exception ie) {
                Crittercism.LogInternalException(ie);
            }
            Debug.WriteLine("LoadSettings: " + JsonConvert.SerializeObject(answer));
            return answer;
        }
        internal static void SaveSettings(string json) {
            // Persist AppLoad response JSON
            try {
                Debug.WriteLine("SaveSettings: " + json);
                string path = Path.Combine(StorageHelper.CrittercismPath(),"Settings.js");
                StorageHelper.SaveString(path,json);
            } catch (Exception ie) {
                Crittercism.LogInternalException(ie);
            }
        }
        #endregion

        #region Userflows
        internal static void OnUserflowTimeOut(EventArgs e) {
            EventHandler handler = UserflowTimeOut;
            if (handler != null) {
                handler(null,e);
            }
        }

        public static void BeginUserflow(string name) {
            // Init and begin a userflow with a default value.
            try {
                AbortUserflow(name);
                // Do not begin a new userflow if the userflow count is at or has exceeded the max.
                if (UserflowReporter.UserflowCount() >= UserflowReporter.MAX_USERFLOW_COUNT) {
                    DebugUtils.LOG_ERROR(String.Format(("Crittercism only supports a maximum of {0} concurrent userflows."
                                                       + "\r\nIgnoring Crittercism.BeginUserflow() call for {1}."),
                                                       UserflowReporter.MAX_USERFLOW_COUNT,name));
                    return;
                }
                (new Userflow(name)).Begin();
            } catch (Exception ie) {
                Crittercism.LogInternalException(ie);
            }
        }
        public static void BeginUserflow(string name,int value) {
            // Init and begin a userflow with an input value.
            try {
                AbortUserflow(name);
                // Do not begin a new userflow if the userflow count is at or has exceeded the max.
                if (UserflowReporter.UserflowCount() >= UserflowReporter.MAX_USERFLOW_COUNT) {
                    DebugUtils.LOG_ERROR(String.Format(("Crittercism only supports a maximum of {0} concurrent userflows."
                                                       + "\r\nIgnoring Crittercism.BeginUserflow() call for {1}."),
                                                       UserflowReporter.MAX_USERFLOW_COUNT,name));
                    return;
                }
                (new Userflow(name,value)).Begin();
            } catch (Exception ie) {
                Crittercism.LogInternalException(ie);
            }
        }
        private static void AbortUserflow(string name) {
            // Cancel a userflow with this name if one exists, otherwise be quiet.
            try {
                Userflow userflow = Userflow.UserflowForName(name);
                if (userflow != null) {
                    DebugUtils.LOG_WARN(String.Format("Cancelling unfinished identically named userflow {0}.",name));
                    userflow.Cancel();
                }
            } catch (Exception ie) {
                Crittercism.LogInternalException(ie);
            }
        }
        public static void CancelUserflow(string name) {
            // Cancel a userflow as if it never was.
            try {
                Userflow userflow = Userflow.UserflowForName(name);
                if (userflow != null) {
                    userflow.Cancel();
                } else {
                    CantFindUserflow(name);
                }
            } catch (Exception ie) {
                Crittercism.LogInternalException(ie);
            }
        }
        public static void EndUserflow(string name) {
            // End an already begun userflow successfully.
            try {
                Userflow userflow = Userflow.UserflowForName(name);
                if (userflow != null) {
                    userflow.End();
                } else {
                    CantFindUserflow(name);
                }
            } catch (Exception ie) {
                Crittercism.LogInternalException(ie);
            }
        }
        public static void FailUserflow(string name) {
            // End an already begun userflow as a failure.
            try {
                Userflow userflow = Userflow.UserflowForName(name);
                if (userflow != null) {
                    userflow.Fail();
                } else {
                    CantFindUserflow(name);
                }
            } catch (Exception ie) {
                Crittercism.LogInternalException(ie);
            }
        }
        public static int GetUserflowValue(string name) {
            // Get the currency cents value of a userflow.
            int answer = 0;
            try {
                Userflow userflow = Userflow.UserflowForName(name);
                if (userflow != null) {
                    answer = userflow.Value();
                } else {
                    CantFindUserflow(name);
                }
            } catch (Exception ie) {
                Crittercism.LogInternalException(ie);
            }
            return answer;
        }
        public static void SetUserflowValue(string name,int value) {
            // Set the currency cents value of a userflow.
            try {
                Userflow userflow = Userflow.UserflowForName(name);
                if (userflow != null) {
                    userflow.SetValue(value);
                } else {
                    CantFindUserflow(name);
                }
            } catch (Exception ie) {
                Crittercism.LogInternalException(ie);
            }
        }

        internal static void CantFindUserflow(string name) {
#if NETFX_CORE || WINDOWS_PHONE
#else
            Trace.WriteLine(String.Format("Can't find userflow named \"{0}\"",name));
#endif
        }

        #endregion

        #region Network Requests
        private static string RemoveQueryString(string uriString) {
            // String obtained by removing query string portion of uriString .
            string answer = uriString;
            int p = uriString.IndexOf('?');
            if (p >= 0) {
                answer = uriString.Substring(0,p);
            };
            return answer;
        }
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
                        Debug.WriteLine("APM FILTERED: " + uriString);
                    } else {
                        string timestamp = TimeUtils.ISO8601DateString(DateTime.UtcNow);
                        Endpoint endpoint = new Endpoint(method,
                            RemoveQueryString(uriString),
                            timestamp,
                            latency,
                            bytesRead,
                            bytesSent,
                            statusCode,
                            exceptionStatus);
                        APM.Enqueue(endpoint);
                        Breadcrumbs.LeaveNetworkBreadcrumb(endpoint);
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
            List<MessageReport> messageReports = MessageReport.LoadMessages();
            foreach (MessageReport messageReport in messageReports) {
                // I'm wondering if we needed to restrict to 50 messageReport of something similar?
                MessageQueue.Enqueue(messageReport);
            }
        }

        private const int MaxMessageQueueCount = 100;

        /// <summary>
        /// Adds message to queue
        /// </summary>
        internal static void AddMessageToQueue(MessageReport messageReport) {
            while (MessageQueue.Count >= MaxMessageQueueCount) {
                // Sacrifice an oldMessage
                MessageReport oldMessage = MessageQueue.Dequeue();
                oldMessage.Delete();
            }
            MessageQueue.Enqueue(messageReport);
            readerEvent.Set();
        }
        #endregion // MessageQueue

        #region Event Handlers
#if NETFX_CORE || WINDOWS_PHONE
        ////////////////////////////////////////////////////////////////
        // NOTE: The value of monitoring (root) Root_UIElement_GotFocus,
        // (root) Root_UIElement_LostFocus, and Window_VisibilityChanged
        // is proven by the following "App Background" + "App Foreground"
        // VS2015 Output log of the HubApp.WindowsPhone test app:
        //    * App Background
        //    Root_UIElement_LostFocus: 635884114165261968
        //    Window_VisibilityChanged: 635884114165541917
        //    * App Foreground
        //    Window_VisibilityChanged: 635884114201681836
        //    Root_UIElement_GotFocus: 635884114203470952
        // Here, UIElement root = Window.Current.Content as UIElement;
        ////////////////////////////////////////////////////////////////
        private static void Root_UIElement_GotFocus(object sender,RoutedEventArgs e) {
            long root_UIElement_GotFocus_Time = DateTime.UtcNow.Ticks;  // ticks
            Debug.WriteLine("Root_UIElement_GotFocus: " + root_UIElement_GotFocus_Time);
            if (GetOptOutStatus()) {
                return;
            }
            try {
                // Automatic "App Foreground" Userflow
                if (!isForegrounded) {
                    // NOTE: Being a UIElement event handler, Root_UIElement_GotFocus should
                    // only get called via the main UI thread.  So, no thread-safety worries here.
                    new Userflow("App Foreground",window_VisibilityChanged_Time,root_UIElement_GotFocus_Time);
                    isForegrounded = true;
                };
            } catch (Exception ie) {
                LogInternalException(ie);
            }
        }
        private static long root_UIElement_LostFocus_Time = 0;  // ticks
        private static void Root_UIElement_LostFocus(object sender,RoutedEventArgs e) {
            root_UIElement_LostFocus_Time = DateTime.UtcNow.Ticks;
            Debug.WriteLine("Root_UIElement_LostFocus: " + root_UIElement_LostFocus_Time);
            // That's all we do in this method:  Record the time we treat
            // as the beginTime of an "App Background" UserFlow .
        }
#if NETFX_CORE
        private static string lastViewName = null;
        private static void Root_UIElement_Navigated(object sender,NavigationEventArgs e) {
            try {
                if (sender is Frame) {
                    // It should be a Frame since we checked for Frame type when we subscribed
                    // to Navigated event.  The Frame's Content is only known to be "object"
                    // at this point, since Frame inherits from ContentControl and that is how
                    // Content is declared.
                    Object content = ((Frame)sender).Content as Object;
                    if (content != null) {
                        // This should normally be the case.  We're only going to leave breadcrumbs
                        // if we can say something better than "Unknown", which isn't too informative.
                        string viewName = content.GetType().Name;
                        // Go for more.
                        FrameworkElement frameworkElement = content as FrameworkElement;
                        if (frameworkElement != null) {
                            string frameworkElementName = frameworkElement.Name;
                            if ((frameworkElementName != null) && (frameworkElementName != "")) {
                                viewName = viewName + " " + frameworkElementName;
                            };
                        };
                        // It may be kind of bozo we're sending Deactivated and Activated in pairs
                        // like this, but it is the Wire+Protocol spec which is the clown, so we do it.
                        if (lastViewName != null) {
                            Breadcrumbs.LeaveViewBreadcrumb(BreadcrumbViewType.Deactivated, lastViewName);
                        };
                        Breadcrumbs.LeaveViewBreadcrumb(BreadcrumbViewType.Activated, viewName);
                        lastViewName = viewName;
                        Debug.WriteLine("Root_UIElement_Navigated: " + viewName);
                    };
                };
            } catch (Exception ie) {
                Crittercism.LogInternalException(ie);
            };
        }
#endif
        private static long window_VisibilityChanged_Time = 0;  // ticks
#endif

#if NETFX_CORE
#pragma warning disable 1998
        private static void Window_VisibilityChanged(object sender,VisibilityChangedEventArgs e) {
            window_VisibilityChanged_Time = DateTime.UtcNow.Ticks;
            Debug.WriteLine("Window_VisibilityChanged: " + window_VisibilityChanged_Time);
            if (GetOptOutStatus()) {
                return;
            }
            try {
                if (e.Visible) {
                    // If we could absolutely rely on Root_UIElement_GotFocus getting installed,
                    // we might shift this "Foreground()" to that method.  However, we're not absolutely
                    // sure that will always happen, and it might not happen.
                    Foreground();
                } else {
                    Background();
                }
            } catch (Exception ie) {
                LogInternalException(ie);
            }
        }
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
                string newReachabilityStatusString = ReachabilityStatusString();
                Breadcrumbs.HandleReachabilityChange(newReachabilityStatusString);
                // If we are connected to Internet, prod the readerThread .
                if (newReachabilityStatusString.IndexOf("InternetAccess") == 0) {
                    if (MessageQueue != null && MessageQueue.Count > 0) {
                        readerEvent.Set();
                    }
                }
            } catch (Exception ie) {
                LogInternalException(ie);
            }
        }

        private static string ReachabilityStatusString() {
            ConnectionProfile connectedProfile = NetworkInformation.GetInternetConnectionProfile();
            // Compute reachabilityStatusString (e.g. "InternetAccess+WiFi")
            NetworkConnectivityLevel networkConnectivityLevel = NetworkConnectivityLevel.None;
            string reachabilityStatusString = networkConnectivityLevel.ToString();
            if (connectedProfile != null) {
                networkConnectivityLevel = connectedProfile.GetNetworkConnectivityLevel();
                reachabilityStatusString = networkConnectivityLevel.ToString();
                // A non-null connectedSsid means we've got WiFi connectivity.
                string connectedSsid = null;
                if (connectedProfile.IsWlanConnectionProfile &&
                    connectedProfile.WlanConnectionProfileDetails != null) {
                    connectedSsid = connectedProfile.WlanConnectionProfileDetails.GetConnectedSsid();
                    if (connectedSsid != null) {
                        reachabilityStatusString = reachabilityStatusString + "+WiFi";
                    };
                };
            };
            Debug.WriteLine("ReachabilityStatusString == " + reachabilityStatusString);
            return reachabilityStatusString;
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

        static void PhoneApplicationService_Activated(object sender,ActivatedEventArgs e) {
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
            window_VisibilityChanged_Time = DateTime.UtcNow.Ticks;
            Debug.WriteLine("Window_VisibilityChanged: " + window_VisibilityChanged_Time);
            if (GetOptOutStatus()) {
                return;
            }
            try {
                if (!e.IsApplicationInstancePreserved) {
                    // App was tombstoned.  State lost.
                    if (PhoneApplicationService.Current.State.ContainsKey("Crittercism.AppID")) {
                        // Take this to mean that Crittercism was Init'd in this app prior to
                        // a Deactivate.  Restart Crittercism asynchronously.
                        BackgroundWorker backgroundWorker = new BackgroundWorker();
                        backgroundWorker.DoWork += new DoWorkEventHandler(BackgroundWorker_DoWork);
                        backgroundWorker.RunWorkerAsync();
                    }
                    // Above will (generally?) get Crittercism.Init called generating a
                    // new automatic "App Load" userflow.
                } else {
                    Foreground();
                }
            } catch (Exception ie) {
                LogInternalException(ie);
            }
        }

        static void BackgroundWorker_DoWork(object sender,DoWorkEventArgs e) {
            Init((string)PhoneApplicationService.Current.State["Crittercism.AppID"]);
        }

        static void PhoneApplicationService_Deactivated(object sender,DeactivatedEventArgs e) {
            window_VisibilityChanged_Time = DateTime.UtcNow.Ticks;
            Debug.WriteLine("Window_VisibilityChanged: " + window_VisibilityChanged_Time);
            if (GetOptOutStatus()) {
                return;
            }
            try {
                PhoneApplicationService.Current.State["Crittercism.AppID"] = AppID;
                Background();
            } catch (Exception ie) {
                LogInternalException(ie);
            }
        }

        static void DeviceNetworkInformation_NetworkAvailabilityChanged(object sender,NetworkNotificationEventArgs e) {
            if (GetOptOutStatus()) {
                return;
            }
            try {
                if (e != null) {
                    // Our own OnTimerElapsed is allowedd to send e == null .
                    switch (e.NotificationType) {
                        case NetworkNotificationType.InterfaceConnected:
                            if (NetworkInterface.GetIsNetworkAvailable()) {
                                if (MessageQueue != null && MessageQueue.Count > 0) {
                                    readerEvent.Set();
                                }
                            }
                            break;
                    };
                };
                string newReachabilityStatusString = ReachabilityStatusString();
                Breadcrumbs.HandleReachabilityChange(newReachabilityStatusString);
            } catch (Exception ie) {
                LogInternalException(ie);
            }
        }

        private static string ReachabilityStatusString() {
            ConnectionProfile connectedProfile = NetworkInformation.GetInternetConnectionProfile();
            // Compute reachabilityStatusString (e.g. "InternetAccess+WiFi")
            NetworkConnectivityLevel networkConnectivityLevel = NetworkConnectivityLevel.None;
            string reachabilityStatusString = networkConnectivityLevel.ToString();
            if (connectedProfile != null) {
                networkConnectivityLevel = connectedProfile.GetNetworkConnectivityLevel();
                reachabilityStatusString = networkConnectivityLevel.ToString();
                // Compute networkInterfaceType
                NetworkAdapter networkAdapter = connectedProfile.NetworkAdapter;
                if (networkAdapter.IanaInterfaceType == 71) {
                    // 71 == An IEEE 802.11 wireless network interface.
                    // https://msdn.microsoft.com/en-us/library/windows/apps/windows.networking.connectivity.networkadapter.ianainterfacetype.aspx
                    reachabilityStatusString = reachabilityStatusString + "+WiFi";
                };
            };
            Debug.WriteLine("ReachabilityStatusString == " + reachabilityStatusString);
            return reachabilityStatusString;
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

        private static void NetworkChange_NetworkAvailabilityChanged(object sender, EventArgs e)
        {
            // Microsoft documents event NetworkAddressChanged
            // https://msdn.microsoft.com/en-us/library/system.net.networkinformation.networkchange(v=vs.110).aspx
            // and this is our handler.  However, [cough] we've never yet seen
            // this event get triggered.  So, we are also applying the strategy of
            // calling our own handler via a timer every 5 seconds or so.  If there
            // isn't really a change in ReachabilityStatusString() then the
            // Breadcrumbs.HandleReachabilityChange acts like a NOP, so this strategy
            // works OK.  In discussions on the WWW, others have noticed MS event not
            // firing, and debates ensue about how or whether it is possible to define
            // the Interent being UP/DOWN ensue.  Maybe MS just quietly gave up?
            Debug.WriteLine("NetworkChange_NetworkAvailabilityChanged");
            Breadcrumbs.HandleReachabilityChange(ReachabilityStatusString());
        }

        private static string ReachabilityStatusString() {
            string reachabilityStatusString = "None";
            if (NetworkInterface.GetIsNetworkAvailable()) {
                // however, this will include all adapters
                NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface adapter in adapters) {
                    // Some filtering of the adapters adapted from
                    // https://social.msdn.microsoft.com/Forums/vstudio/en-US/a6b3541b-b7de-49e2-a7a6-ba0687761af5/networkavailabilitychanged-event-does-not-fire?forum=csharpgeneral
                    const int minimumSpeed = 10000000;
                    if ((adapter.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                        && (adapter.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
                        && (adapter.Description.IndexOf("virtual", StringComparison.OrdinalIgnoreCase) < 0)
                        && (adapter.Name.IndexOf("virtual", StringComparison.OrdinalIgnoreCase) < 0)
                        && (!adapter.Description.Equals("Microsoft Loopback Adapter", StringComparison.OrdinalIgnoreCase))
                        && (adapter.Speed >= minimumSpeed)) {
                        // Declare "adapter" to be an Internet adapter.
                        if (adapter.OperationalStatus == OperationalStatus.Up) {
                            reachabilityStatusString = "InternetAccess";
                            if (adapter.NetworkInterfaceType == NetworkInterfaceType.Wireless80211) {
                                // There are a lot of possibilities for NetworkInterfaceType, but
                                // in presence of multiple adapters, it's hard to pick out the one
                                // we should report.  We'll just look for "+WiFi" .
                                reachabilityStatusString += "+WiFi";
                                break;
                            };
                        };
                    };
                };
            };
            return reachabilityStatusString;
        }
#endif

#if NETFX_CORE || WINDOWS_PHONE
        private static void Foreground() {
            Breadcrumbs.LeaveEventBreadcrumb("foregrounded");
            APM.Foreground();
            UserflowReporter.Foreground();
            InstallTimer();
        }

        private static void Background() {
            try {
                if (root_UIElement_LostFocus_Time != 0) {
                    // The minimum sanity check we can do on the slighly risky Root_UIElement_LostFocus
                    // event handler installation is to check it did record a root_UIElement_LostFocus_Time .
                    // Automatic "App Background" Userflow
                    new Userflow("App Background",root_UIElement_LostFocus_Time,window_VisibilityChanged_Time);
                    isForegrounded = false;
                }
            } catch (Exception ie) {
                LogInternalException(ie);
            }
            Breadcrumbs.LeaveEventBreadcrumb("backgrounded");
            APM.Background();
            UserflowReporter.Background();
            RemoveTimer();
        }
#endif

        #endregion // Event Handlers
    }
}
