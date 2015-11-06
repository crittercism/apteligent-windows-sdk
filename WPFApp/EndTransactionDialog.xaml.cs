using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

namespace WPFApp
{
    /// <summary>
    /// Interaction logic for EndTransactionDialog.xaml
    /// </summary>
    public partial class EndTransactionDialog : Window
    {
        // Dialog return value
        private string answer;
        public string Answer {
            get { return answer; }
        }
        public EndTransactionDialog()
        {
            InitializeComponent();
        }
        private void endTransactionClick(object sender,RoutedEventArgs e) {
            // Set dialog return value
            answer = "End Transaction";
            this.DialogResult = true;
        }
        private void failTransactionClick(object sender,RoutedEventArgs e) {
            // Set dialog return value
            answer = "Fail Transaction";
            this.DialogResult = true;
        }
        private void cancelTransactionClick(object sender,RoutedEventArgs e) {
            // Set dialog return value
            answer = "Cancel";
            this.DialogResult = false;
        }
        private void Window_Closed(object sender,EventArgs e) {
        }
    }
}