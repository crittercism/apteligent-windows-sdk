using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        private void leaveBreadcrumbClick(object sender,RoutedEventArgs e) {
            Crittercism.LeaveBreadcrumb("Leaving Breadcrumb");
        }

        private void handledExceptionClick(object sender,RoutedEventArgs e) {
            int i=0;
            int j=5;
            try {
                int k=j/i;
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
            int x=0;
            int y=1/x;
        }

        private void testMultithreadClick(object sender,RoutedEventArgs e) {
            //Thread thread=new Thread(new ThreadStart(Worker.Work));
            //thread.Start();
        }

        private void backButtonClicked(object sender,RoutedEventArgs e) {
            //NavigationService.Navigate(new Uri("/Crashes.xaml",UriKind.Relative));
        }

        private void nextButtonClicked(object sender,RoutedEventArgs e) {
            //NavigationService.Navigate(new Uri("/Loads.xaml",UriKind.Relative));
        }
    }
}
