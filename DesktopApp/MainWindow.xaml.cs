using System;
using System.Collections.Generic;
using System.Linq;
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

namespace DesktopApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void setUsernameClick(object sender,RoutedEventArgs e) {
            Crittercism.SetUsername("MrsCritter");
        }

        private void leaveBreadcrumbClick(object sender,RoutedEventArgs e) {
            Crittercism.LeaveBreadcrumb("Leaving Breadcrumb");
        }

        private void handledExceptionClick(object sender,RoutedEventArgs e) {
            try {
                DeepError1(10);
            } catch (Exception ex) {
                Crittercism.LogHandledException(ex);
            }
        }

        private void handledUnthrownExceptionClick(object sender,RoutedEventArgs e) {
            Exception exception=new Exception("description");
            exception.Data.Add("MethodName","methodName");
            Crittercism.LogHandledException(exception);
        }

        private void testCrashClick(object sender,RoutedEventArgs e) {
            DeepError1(10);
        }

        void DeepError1(int n) {
            DeepError2(n-1);
        }

        void DeepError2(int n) {
            DeepError3(n-1);
        }

        void DeepError3(int n) {
            DeepError4(n-1);
        }

        void DeepError4(int n) {
            if (n<=0) {
                int i=0;
                int j=5;
                int k=j/i;
            } else {
                DeepError1(n-1);
            }
        }
        
        private void testMultithreadClick(object sender,RoutedEventArgs e) {
            Thread thread=new Thread(new ThreadStart(Worker.Work));
            thread.Start();
        }

        private void critterClick(object sender,RoutedEventArgs e) {
            string username=Crittercism.Username();
            if (username==null) {
                username="User";
            }
            string response="";
            MessageBoxResult result=MessageBox.Show("Do you love Crittercism?","DesktopApp",MessageBoxButton.YesNo);
            switch (result) {
                case MessageBoxResult.Yes:
                    response=" loves Crittercism.";
                    break;
                case MessageBoxResult.No:
                    response=" doesn't love Crittercism.";
                    break;
            }
            Crittercism.LeaveBreadcrumb(username+" "+response);
        }
    }
}
