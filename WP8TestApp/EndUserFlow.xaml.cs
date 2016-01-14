using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;
using CrittercismSDK;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Threading;

namespace WP8TestApp {
    public partial class EndUserFlow : PhoneApplicationPage {
        public EndUserFlow() {
            InitializeComponent();
            Crittercism.UserFlowTimeOut += UserFlowTimeOutHandler;
        }

        private void endUserFlowClick(object sender,RoutedEventArgs e)
        {
            Crittercism.EndUserFlow(App.userFlowName);
            App.userFlowName = null;
            GoBack();
        }

        private void failUserFlowClick(object sender,RoutedEventArgs e) {
            Crittercism.FailUserFlow(App.userFlowName);
            App.userFlowName = null;
            GoBack();
        }

        private void cancelUserFlowClick(object sender,RoutedEventArgs e)
        {
            Crittercism.CancelUserFlow(App.userFlowName);
            App.userFlowName = null;
            GoBack();
        }

        internal void GoBack() {
            Frame frame = Parent as Frame;
            frame.GoBack();
        }

        private void UserFlowTimeOutHandler(object sender,EventArgs e) {
            Demo.UserFlowTimeOutHandler(this,e);
        }
    }
}
