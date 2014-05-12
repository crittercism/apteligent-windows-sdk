using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace TestPhoneApp
{
    public partial class TestDetail : PhoneApplicationPage
    {
        public TestDetail()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            string testName = this.NavigationContext.QueryString["name"];
            this.Title = testName;
            this.DataContext = MainPage.testMethods.FirstOrDefault(t => t.TestName == testName);
        }
    }
}