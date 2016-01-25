using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CrittercismSDK;

namespace WPFApp {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private static Random random = new Random();

        public MainWindow() {
            InitializeComponent();
            App.UserflowEvent += UserflowEventHandler;
            UserflowEventHandler(this,EventArgs.Empty);
        }

        private void setUsernameClick(object sender,RoutedEventArgs e) {
            string[] names = { "Blue Jay","Chinchilla","Chipmunk","Gerbil","Hamster","Parrot","Robin","Squirrel","Turtle" };
            string name = names[random.Next(0,names.Length)];
            Crittercism.SetUsername("Critter " + name);
        }

        private void leaveBreadcrumbClick(object sender,RoutedEventArgs e) {
            string[] names = { "Breadcrumb","Strawberry","Seed","Grape","Lettuce" };
            string name = names[random.Next(0,names.Length)];
            Crittercism.LeaveBreadcrumb(name);
        }

        private static string[] urls = new string[] {
            "http://www.hearst.com",
            "http://www.urbanoutfitters.com",
            "http://www.pinterest.com",
            "http://www.docusign.com",
            "http://www.netflix.com",
            "http://www.paypal.com",
            "http://www.groupon.com",
            "http://www.ebay.com",
            "http://www.yahoo.com",
            "http://www.linkedin.com",
            "http://www.bloomberg.com",
            "http://www.hoteltonight.com",
            "http://www.npr.org",
            "http://www.samsclub.com",
            "http://www.postmates.com",
            "http://www.teslamotors.com",
            "http://www.bhphotovideo.com",
            "http://www.getkeepsafe.com",
            "http://www.boltcreative.com",
            "http://www.crittercism.com/customers/"
        };
        private void logNetworkRequestClick(object sender,RoutedEventArgs e) {
            string[] methods = new string[] { "GET","POST","HEAD","PUT" };
            string method = methods[random.Next(0,methods.Length)];
            string url = urls[random.Next(0,urls.Length)];
            if (random.Next(0,2) == 1) {
                url = url + "?doYouLoveCrittercism=YES";
            }
            // latency in milliseconds
            long latency = (long)Math.Floor(4000.0 * random.NextDouble());
            long bytesRead = random.Next(0,10000);
            long bytesSent = random.Next(0,10000);
            long responseCode = 200;
            if (random.Next(0,5) == 0) {
                // Some common response other than 200 == OK .
                long[] responseCodes = new long[] { 301,308,400,401,402,403,404,405,408,500,502,503 };
                responseCode = responseCodes[random.Next(0,responseCodes.Length)];
            }
            Crittercism.LogNetworkRequest(
                method,
                url,
                latency,
                bytesRead,
                bytesSent,
                (HttpStatusCode)responseCode,
                WebExceptionStatus.Success);
        }

        private const string beginUserflowLabel = "Begin Userflow";
        private const string endUserflowLabel = "End Userflow";
        private string[] userflowNames = new string[] { "Buy Critter Feed","Sing Critter Song","Write Critter Poem" };
        private void userflowClick(object sender,RoutedEventArgs e) {
            Button button = sender as Button;
            if (button != null) {
                Debug.Assert(button == userflowButton);
                String label = button.Content.ToString();
                if (label == beginUserflowLabel) {
                    App.userflowName = userflowNames[random.Next(0,userflowNames.Length)];
                    Crittercism.BeginUserflow(App.userflowName);
                    // Broadcast UserflowEvent to all open windows. 
                    App.OnUserflowEvent(EventArgs.Empty);
                } else if (label == endUserflowLabel) {
                    EndUserflowDialog dialog = new EndUserflowDialog();
                    dialog.Owner = Window.GetWindow(this);
                    dialog.ShowDialog();
                    Nullable<bool> dialogResult = dialog.DialogResult;
                    if (dialogResult == true) {
                        switch (dialog.Answer) {
                            case "End Userflow":
                                Crittercism.EndUserflow(App.userflowName);
                                break;
                            case "Fail Userflow":
                                Crittercism.FailUserflow(App.userflowName);
                                break;
                            case "Cancel Userflow":
                                Crittercism.CancelUserflow(App.userflowName);
                                break;
                        };
                        App.userflowName = null;
                        // Broadcast UserflowEvent to all open windows. 
                        App.OnUserflowEvent(EventArgs.Empty);
                    }
                }
            }
        }
        private void UserflowEventHandler(object sender,EventArgs e) {
            // Update userflowButton ("Begin Userflow"/"End Userflow") in reaction to UserflowEvent .
            // Execute this Action on the main UI thread.
            userflowButton.Dispatcher.Invoke(new Action(() => {
                if (App.userflowName == null) {
                    userflowButton.Content = beginUserflowLabel;
                } else {
                    userflowButton.Content = endUserflowLabel;
                }
            }));
        }
        private void handledExceptionClick(object sender,RoutedEventArgs e) {
            try {
                ThrowException();
            } catch (Exception ex) {
                Crittercism.LogHandledException(ex);
            }
        }

        private void handledUnthrownExceptionClick(object sender,RoutedEventArgs e) {
            Exception exception = new Exception("description");
            exception.Data.Add("MethodName","methodName");
            Crittercism.LogHandledException(exception);
        }

        private void testCrashClick(object sender,RoutedEventArgs e) {
            ThrowException();
        }

        private void DeepError(int n) {
            if (n == 0) {
                throw new Exception("Exception " + random.NextDouble());
            } else {
                DeepError(n - 1);
            }
        }

        private void ThrowException() {
            DeepError(random.Next(0,4));
        }

        private void OuterException() {
            try {
                DeepError(4);
            } catch (Exception ie) {
                throw new Exception("Outer Exception",ie);
            }
        }

        private void testMultithreadClick(object sender,RoutedEventArgs e) {
            Thread thread = new Thread(new ThreadStart(Worker.Work));
            thread.Start();
        }

        private void testNewWindowClick(object sender,RoutedEventArgs e) {
            (new MainWindow()).Show();
        }

        private void critterClick(object sender,RoutedEventArgs e) {
            string username = Crittercism.Username();
            if (username == null) {
                username = "User";
            }
            string response = "";
            MessageBoxResult result = MessageBox.Show(this,"Do you love Crittercism?","WPFApp",MessageBoxButton.YesNo);
            switch (result) {
                case MessageBoxResult.Yes:
                    response = "loves Crittercism.";
                    break;
                case MessageBoxResult.No:
                    response = "doesn't love Crittercism.";
                    break;
            }
            Crittercism.LeaveBreadcrumb(username + " " + response);
        }

        private void Window_Closed(object sender,EventArgs e) {
            Crittercism.LeaveBreadcrumb("Closed");
            App.UserflowEvent -= UserflowEventHandler;
            if (Application.Current.Windows.Count == 0) {
                // Last window is closing.
                Crittercism.Shutdown();
                Application.Current.Shutdown();
            }
        }
    }
}
