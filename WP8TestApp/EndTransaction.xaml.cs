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
    public partial class EndTransaction : PhoneApplicationPage {
        public EndTransaction() {
            InitializeComponent();
            Crittercism.TransactionTimeOut += TransactionTimeOutHandler;
        }

        private void endTransactionClick(object sender,RoutedEventArgs e)
        {
            Crittercism.EndTransaction(App.transactionName);
            App.transactionName = null;
            GoBack();
        }

        private void failTransactionClick(object sender,RoutedEventArgs e) {
            Crittercism.FailTransaction(App.transactionName);
            App.transactionName = null;
            GoBack();
        }

        private void cancelTransactionClick(object sender,RoutedEventArgs e)
        {
            Crittercism.CancelTransaction(App.transactionName);
            App.transactionName = null;
            GoBack();
        }

        internal void GoBack() {
            Frame frame = Parent as Frame;
            frame.GoBack();
        }

        private void TransactionTimeOutHandler(object sender,EventArgs e) {
            Demo.TransactionTimeOutHandler(this,e);
        }
    }
}
