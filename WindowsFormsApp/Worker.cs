using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CrittercismSDK;

namespace WindowsFormsApp
{
    class Worker
    {
        // Work will be called when a worker thread is started via
        // Thread thread = new Thread(new ThreadStart(Worker.Work));
        // thread.Start();
        public static void Work()
        {
            Console.WriteLine("Worker.Work running in its own thread.");
            Random rnd = new Random();
            while (true) {
                // Wait around 2 second.
                Thread.Sleep(rnd.Next(4000));
                if (rnd.Next(10)==0) {
                    Random random=new Random();
                    string[] names= { "Breadcrumb","Strawberry","Seed","Grape","Lettuce" };
                    string name=names[random.Next(0,names.Length)];
                    Crittercism.LeaveBreadcrumb(name);
                } else {
                    int i=0;
                    int j=5;
                    try {
                        int k=j/i;
                    } catch (Exception e) {
                        Crittercism.LogHandledException(e);
                    }
                }
            }
        }
    }
}
