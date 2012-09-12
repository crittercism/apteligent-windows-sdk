using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using CrittercismSDK;

namespace CrittercismWP7TestApplication
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                throw new Exception("This is my handled exception");
            }
            catch (Exception ex)
            {
                Crittercism.CreateErrorReport(ex);
                Crittercism.LeaveBreadcrum("Before crash");
                throw new Exception("This is my unhandled exception");
            }
        }
    }
}