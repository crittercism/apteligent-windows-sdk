using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace CrittercismWP8TestApplication {
    public partial class CrashSim : PhoneApplicationPage {
        public CrashSim() {
            InitializeComponent();
        }

        private void crashSimulateClick(object sender, RoutedEventArgs e) {
            int x = 0;
            int y = 1 / x;
        }

        private void backButtonClicked(object sender, RoutedEventArgs e) {
            NavigationService.Navigate(new Uri("/Crashes.xaml", UriKind.Relative));
        }

        private void nextButtonClicked(object sender, RoutedEventArgs e) {
            NavigationService.Navigate(new Uri("/Loads.xaml", UriKind.Relative));
        }
    }
}