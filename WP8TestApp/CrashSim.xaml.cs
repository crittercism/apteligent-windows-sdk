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
            Crittercism.SetUsername("MrsCritter");
        }

        private void leaveBreadcrumbClick(object sender,RoutedEventArgs e)
        {
            Crittercism.LeaveBreadcrumb("Leaving Breadcrumb");
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

        private void testCrashClick(object sender,RoutedEventArgs e)
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
