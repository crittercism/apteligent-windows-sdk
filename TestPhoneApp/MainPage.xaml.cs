using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using TestPhoneApp.Resources;
using Microsoft.Phone.Shell;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CrittercismSDK.DataContracts;
using CrittercismSDK;
using System.Threading;
using System.Reflection;
using System.ComponentModel;

namespace TestPhoneApp
{
    public partial class MainPage : PhoneApplicationPage
    {
        internal static List<TestInfo> testMethods = new List<TestInfo>();
        internal static bool testExecuted = false;
        BackgroundWorker worker = null;

        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

        private void PhoneApplicationPage_Loaded_1(object sender, RoutedEventArgs e)
        {
            if (!testExecuted)
            {
                Type[] listOfTypes = this.GetType().Assembly.GetTypes();
                var listOfTestClass = listOfTypes.Where(t => t.GetCustomAttributes(typeof(TestClassAttribute), false).Count() > 0).ToList();
                foreach (Type testClass in listOfTestClass)
                {
                    var methods = testClass.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                    MethodInfo cleanUpMethod = methods.FirstOrDefault(m => m.GetCustomAttributes(typeof(TestCleanupAttribute), false).Count() > 0);
                    foreach (MethodInfo testMethod in methods.Where(m => m.GetCustomAttributes(typeof(TestMethodAttribute), false).Count() > 0).ToList())
                    {
                        testMethods.Add(new TestInfo(testMethod, cleanUpMethod));
                    }
                }

                this.UnitTestList.ItemsSource = testMethods;
                this.worker = new BackgroundWorker();
                this.worker.ProgressChanged += worker_ProgressChanged;
                this.worker.DoWork += worker_DoWork;
                this.worker.RunWorkerCompleted += worker_RunWorkerCompleted;
                this.worker.WorkerReportsProgress = true;
                this.worker.RunWorkerAsync();
                testExecuted = true;
            }

            this.TestTotalLabel.Text = testMethods.Count.ToString();
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            foreach (var item in testMethods)
            {
                this.Dispatcher.BeginInvoke(() => item.Status = TestStatus.Running);
                bool result = item.Run();
                if (result)
                {
                    this.worker.ReportProgress(1, item);
                }
                else
                {
                    this.worker.ReportProgress(0, item);
                }
            }
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            TestInfo test = e.UserState as TestInfo;
            if (e.ProgressPercentage == 0)
            {
                test.Status = TestStatus.Fail;
            }
            else
            {
                test.Status = TestStatus.Success;
            }
            this.TestPassLabel.Text = testMethods.Where(t => t.Status == TestStatus.Success).Count().ToString();
            this.TestFailLabel.Text = testMethods.Where(t => t.Status == TestStatus.Fail).Count().ToString();
        }

        private void UnitTestList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.UnitTestList.SelectedIndex != -1)
            {
                TestInfo test = e.AddedItems[0] as TestInfo;
                NavigationService.Navigate(new Uri(string.Format("/TestDetail.xaml?name={0}", test.TestName), UriKind.Relative));
            }
        }


    }
}