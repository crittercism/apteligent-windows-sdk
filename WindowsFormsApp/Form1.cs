using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CrittercismSDK;

namespace WindowsFormsApp {
    public partial class Form1 : Form {
        private static int ApplicationOpenFormsCount = 0;

        public Form1() {
            InitializeComponent();
            ApplicationOpenFormsCount++;
        }

        protected override void OnLoad(EventArgs e) {
            Crittercism.LeaveBreadcrumb("OnLoad");
        }

        private void setUsername_Click(object sender,EventArgs e) {
            Random random=new Random();
            string[] names= { "Blue Jay","Chinchilla","Chipmunk","Gerbil","Hamster","Parrot","Robin","Squirrel","Turtle" };
            string name=names[random.Next(0,names.Length)];
            Crittercism.SetUsername("Critter "+name);
        }

        private void leaveBreadcrumb_Click(object sender,EventArgs e) {
            Random random=new Random();
            string[] names= { "Breadcrumb","Strawberry","Seed","Grape","Lettuce" };
            string name=names[random.Next(0,names.Length)];
            Crittercism.LeaveBreadcrumb(name);
        }

        string[] urls=new string[] {
            "http://www.hearst.com",
            "http://www.urbanoutfitters.com",
            "http://www.pinterest.com",
            "http://www.docusign.com",
            "http://www.netflix.com",
            "http://www.paypal.com",
            "http://www.groupon.com",
            "http://www.ebay.com",
            "http://www.yahoo.com",
            "http://www.linkedin.com",
            "http://www.bloomberg.com",
            "http://www.hoteltonight.com",
            "http://www.npr.org",
            "http://www.samsclub.com",
            "http://www.postmates.com",
            "http://www.teslamotors.com",
            "http://www.bhphotovideo.com",
            "http://www.getkeepsafe.com",
            "http://www.boltcreative.com",
            "http://www.crittercism.com/customers/"
        };
        private void logNetworkRequest_Click(object sender,EventArgs e) {
            Random random=new Random();
            string[] methods=new string[] { "GET","POST","HEAD","PUT" };
            string method=methods[random.Next(0,methods.Length)];
            string url=urls[random.Next(0,urls.Length)];
            if (random.Next(0,2)==1) {
                url=url+"?doYouLoveCrittercism=YES";
            }
            Uri uri=new Uri(url);
            // latency in milliseconds
            long latency=(long)Math.Floor(4000.0*random.NextDouble());
            long bytesRead=random.Next(0,10000);
            long bytesSent=random.Next(0,10000);
            long responseCode=200;
            if (random.Next(0,5)==0) {
                // Some common response other than 200 == OK .
                long[] responseCodes=new long[] { 301,308,400,401,402,403,404,405,408,500,502,503 };
                responseCode=responseCodes[random.Next(0,responseCodes.Length)];
            }
            Crittercism.LogNetworkRequest(
                method,
                new Uri(url),
                latency,
                bytesRead,
                bytesSent,
                (HttpStatusCode)responseCode,
                WebExceptionStatus.Success);
        }

        private void handledException_Click(object sender,EventArgs e) {
            try {
                ThrowException();
            } catch (Exception ex) {
                Crittercism.LogHandledException(ex);
            }
        }

        private void crash_Click(object sender,EventArgs e) {
            ThrowException();
        }

        private void DeepError(int n) {
            if (n==0) {
                throw new Exception("Deep Inner Exception");
            } else {
                DeepError(n-1);
            }
        }

        private void ThrowException() {
            try {
                DeepError(4);
            } catch (Exception ie) {
                throw new Exception("Outer Exception",ie);
            }
        }

        private void testMultithreadClick(object sender,EventArgs e) {
            Thread thread=new Thread(new ThreadStart(Worker.Work));
            thread.Start();
        }

        private void pictureBox1_Click(object sender,EventArgs e) {
            
            string username=Crittercism.Username();
            if (username==null) {
                username="User";
            }
            string response="";
            DialogResult result=MessageBox.Show("Do you love Crittercism?","WindowsFormsApp",MessageBoxButtons.YesNo);
            switch (result) {
                case DialogResult.Yes:
                    response="loves Crittercism.";
                    break;
                case DialogResult.No:
                    response="doesn't love Crittercism.";
                    break;
            }
            Crittercism.LeaveBreadcrumb(username+" "+response);
        }

        private void Form1_FormClosed(object sender,FormClosedEventArgs e) {
            Crittercism.LeaveBreadcrumb("FormClosed");
            ApplicationOpenFormsCount--;
            if (ApplicationOpenFormsCount==0) {
                // Last window is closing.
                Crittercism.Shutdown();
                Application.Exit();
            }
        }

        private void newWindow_Click(object sender,EventArgs e) {
            (new Form1()).Show();
        }
    }
}
