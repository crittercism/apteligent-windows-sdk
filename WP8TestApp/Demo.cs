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
        internal static void UserFlowTimeOutHandler(Page page,EventArgs e) {
            // UserFlow timed out.
            page.Dispatcher.BeginInvoke(new Action(() => {
                // This page is being shown?
                Frame frame = page.Parent as Frame;
                bool shown = ((frame != null) && (frame.Content == page));
                if (shown) {
                    // Show userFlow "Timed Out" dialog.
                    UserFlowTimeOutShowMessage(e);
                }
                if (page is CrashSim) {
                    // Change label of userFlowButton back to "Begin UserFlow".
                    CrashSim sectionPage = (CrashSim)page;
                    sectionPage.userFlowButton.Content = "Begin UserFlow";
                } else if (page is EndUserFlow) {
                    if (shown) {
                        // We've found ourselves currently on the "End UserFlow" Page .
                        EndUserFlow sectionPage = (EndUserFlow)page;
                        sectionPage.GoBack();
                    }
                }
            }));
        }
        private static void UserFlowTimeOutShowMessage(EventArgs e) {
            // Show MessageBox routine for caller UserFlowTimeOutHandler
            string name = ((CRUserFlowEventArgs)e).Name;
            string message = String.Format("UserFlow '{0}'\r\nTimed Out",name);
            Debug.WriteLine(message);
            MessageBox.Show(message,"WP8TestApp",MessageBoxButton.OK);
        }
    }
}
