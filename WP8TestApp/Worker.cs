using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CrittercismSDK;

namespace WP8TestApp
{
    class Worker
    {
        // Work will be called when a worker thread is started via
        // Thread thread = new Thread(new ThreadStart(Worker.Work));
        // thread.Start();
        public static void Work()
        {
            Console.WriteLine("Worker.Work running in its own thread.");
            Random random = new Random();
            while (true) {
                // Wait around 2 second.
                Thread.Sleep(random.Next(4000));
                if (random.Next(10)==0) {
                    try {
                        string[] names= { "Breadcrumb","Strawberry","Seed","Grape","Lettuce" };
                        string name=names[random.Next(0,names.Length)];
                        Crittercism.LeaveBreadcrumb(name);
                    } catch (Exception e) {
                        Console.WriteLine("UNEXPECTED ERROR!!! "+e.Message);
                    };
                } else {
                    int i=0;
                    int j=5;
                    try {
                        int k=j/i;
                    } catch (Exception ex) {
                        Crittercism.LogHandledException(ex);
                    }
                }
            }
        }
    }
}
