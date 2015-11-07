using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp {
    public partial class EndTransactionDialog : Form {
        // Dialog return value
        private string answer;
        public string Answer {
            get { return answer; }
        }
        public EndTransactionDialog() {
            InitializeComponent();
        }

        private void successButton_Click(object sender,EventArgs e) {
            // Set dialog return value
            answer = "End Transaction";
            this.DialogResult = DialogResult.Yes;
        }

        private void failButton_Click(object sender,EventArgs e) {
            // Set dialog return value
            answer = "Fail Transaction";
            this.DialogResult = DialogResult.Yes;
        }

        private void cancelButton_Click(object sender,EventArgs e) {
            // Set dialog return value
            answer = "Cancel";
            this.DialogResult = DialogResult.No;
        }
    }
}
