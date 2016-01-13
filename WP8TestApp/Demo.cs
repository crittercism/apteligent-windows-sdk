using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Windows.UI.Core;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using CrittercismSDK;

namespace WP8TestApp {
    class Demo {
        internal static void TransactionTimeOutHandler(Page page,EventArgs e) {
            // Transaction timed out.
            page.Dispatcher.BeginInvoke(new Action(() => {
                // This page is being shown?
                Frame frame = page.Parent as Frame;
                bool shown = ((frame != null) && (frame.Content == page));
                if (shown) {
                    // Show transaction "Timed Out" dialog.
                    TransactionTimeOutShowMessage(e);
                }
                if (page is CrashSim) {
                    // Change label of transactionButton back to "Begin Transaction".
                    CrashSim sectionPage = (CrashSim)page;
                    sectionPage.transactionButton.Content = "Begin Transaction";
                } else if (page is EndTransaction) {
                    if (shown) {
                        // We've found ourselves currently on the "End Transaction" Page .
                        EndTransaction sectionPage = (EndTransaction)page;
                        sectionPage.GoBack();
                    }
                }
            }));
        }
        private static void TransactionTimeOutShowMessage(EventArgs e) {
            // Show MessageBox routine for caller TransactionTimeOutHandler
            string name = ((CRTransactionEventArgs)e).Name;
            string message = String.Format("Transaction '{0}'\r\nTimed Out",name);
            Debug.WriteLine(message);
            MessageBox.Show(message,"WP8TestApp",MessageBoxButton.OK);
        }
    }
}
