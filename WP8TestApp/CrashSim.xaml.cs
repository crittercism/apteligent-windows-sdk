using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using CrittercismSDK;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Threading;

namespace WP8TestApp {
    public partial class CrashSim : PhoneApplicationPage {
        public CrashSim() {
            InitializeComponent();
        }

        private void setUsernameClick(object sender,RoutedEventArgs e) {
            Random random=new Random();
            string[] names= { "Blue Jay","Chinchilla","Chipmunk","Gerbil","Hamster","Parrot","Robin","Squirrel","Turtle" };
            string name=names[random.Next(0,names.Length)];
            Crittercism.SetUsername("Critter "+name);
        }

        private void leaveBreadcrumbClick(object sender,RoutedEventArgs e)
        {
            Random random=new Random();
            string[] names= { "Breadcrumb","Strawberry","Seed","Grape","Lettuce" };
            string name=names[random.Next(0,names.Length)];
            Crittercism.LeaveBreadcrumb(name);
        }

        private static string[] urls=new string[] {
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
            Random random=new Random();
            string[] methods=new string[] { "GET","POST","HEAD","PUT" };
            string method=methods[random.Next(0,methods.Length)];
            string url=urls[random.Next(0,urls.Length)];
            if (random.Next(0,2)==1) {
                url=url+"?doYouLoveCrittercism=YES";
            }
            // latency in milliseconds
            long latency=(long)Math.Floor(4000.0*random.NextDouble());
            long bytesRead=random.Next(0,10000);
            long bytesSent=random.Next(0,10000);
            long responseCode=200;
            if (random.Next(0,5)==0) {
                // Some common response other than 200 == OK .
                long[] responseCodes=new long[] { 301,308,400,401,402,403,404,405,408,500,502,503 };
                responseCode=responseCodes[random.Next(0,responseCodes.Length)];
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

        private void handledExceptionClick(object sender,RoutedEventArgs e) {
            try {
                ThrowException(); ;
            } catch (Exception ex) {
                Crittercism.LogHandledException(ex);
            }
        }

        private void handledUnthrownExceptionClick(object sender, RoutedEventArgs e)
        {
            Exception exception = new Exception("description");
            exception.Data.Add("MethodName", "methodName");
            Crittercism.LogHandledException(exception);
        }

        private void crashClick(object sender,RoutedEventArgs e)
        {
            ThrowException();
        }

        private void DeepError(int n) {
            if (n==0) {
                throw new Exception("Deep Inner Exception");
            } else {
                DeepError(n-1);
            }
        }

        private void ThrowException() {
            try {
                DeepError(4);
            } catch (Exception ie) {
                throw new Exception("Outer Exception",ie);
            }
        }
        
        private void testMultithreadClick(object sender,RoutedEventArgs e)
        {
            Thread thread = new Thread(new ThreadStart(Worker.Work));
            thread.Start();
        }

        private void backButtonClicked(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Crashes.xaml", UriKind.Relative));
        }

        private void nextButtonClicked(object sender, RoutedEventArgs e) {
            NavigationService.Navigate(new Uri("/Loads.xaml", UriKind.Relative));
        }
    }
}
